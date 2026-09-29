using RobotCombat.Domain;
using RobotCombat.Domain.Game;
using System.Net;

namespace RobotCombat.Views
{
    public class ConsoleView(Config config) : IGameView
    {
        public int AskHostPortInformation()
        {
            int port = 0;
            do
            {
                Console.Write("Veuillez entrer le port pour héberger la partie : ");
                string input = Console.ReadLine() ?? "";
                if (int.TryParse(input, out port) && port >= 1024 && port <= 5000)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Port invalide. Veuillez entrer un nombre entre 1024 et 5000.");
                }
            } while (true);
            return port;
        }
        public GameAction AskPlayerAction()
        {
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }

            Console.WriteLine("Choisissez votre action :");
            Console.WriteLine("1 - Attaque         2 - Attaque puissante");
            Console.WriteLine("3 - Défense         4 - Recharge");

            while (true)
            {
                Console.Write("Votre choix : ");
                switch (Console.ReadLine())
                {
                    case "1": return GameAction.ATTACK;
                    case "2": return GameAction.ATTACK_PUISSANCE;
                    case "3": return GameAction.DEFENSE;
                    case "4": return GameAction.RECHARGE;
                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }
            }
        }

        public RobotConfig AskPlayerConfig()
        {
            int remainingPoints = config.PointsToGive;
            var robotConfig = new RobotConfig();
            bool isConfigConfirmed = false;

           
            do
            {
                Console.Clear();
                DisplayRobotConfig(robotConfig);

                if (remainingPoints > 0)
                {
                    DisplayRepartPoints(remainingPoints);
                    AskMenuChoice(ref robotConfig, ref remainingPoints);
                }
                else
                {
                    var messages = new string[]
                    {
                       "Vous avez utilisé tous vos points.",
                       "Voulez-vous confirmer votre config ?"
                    };
                    isConfigConfirmed =  AskYesOrNo(messages);

                    if (!isConfigConfirmed)
                    {
                        robotConfig = new RobotConfig();
                        remainingPoints = config.PointsToGive;
                    }
                }
            } while (!isConfigConfirmed);

            Console.Clear();
            Console.WriteLine("Configuration confirmée !");
            DisplayRobotConfig(robotConfig);
            return robotConfig;
        }

        private void AskMenuChoice(ref RobotConfig robotConfig, ref int remainingPoints)
        {
            Console.Write("Votre choix: ");
            string input = Console.ReadLine() ?? "";

            switch (input)
            {
                case "1":
                    robotConfig.HpPoints += AskPoints("aux HP", ref remainingPoints);
                    break;
                case "2":
                    robotConfig.ArmorPoints += AskPoints("à l'armure", ref remainingPoints);
                    break;
                case "3":
                    robotConfig.DamagePoints += AskPoints("aux dégâts", ref remainingPoints);
                    break;
                case "4":
                    robotConfig = new RobotConfig();
                    remainingPoints = config.PointsToGive;
                    break;
                default:
                    Console.WriteLine("Choix invalide.");
                    break;
            }
        }

       

        private int AskPoints(string target,ref int remainingPoints)
        {
            Console.Write($"Combien de points voulez-vous attribuer {target} ? ");
            if (int.TryParse(Console.ReadLine(), out int points) && points >= 0 && points <= remainingPoints)
            {
                remainingPoints -= points;
                return points;
            }

            Console.WriteLine("Nombre de points invalide.");
            return 0;
        }

       

        public string[] AskPlayerHostInformations()
        {
            string ipAddress;
            string port;
            string name;
            bool isValid;

            do
            {
                Console.WriteLine("Veuillez entrer les informations de l'hôte de la partie :");

                Console.Write("Adresse IP du serveur : ");
                ipAddress = Console.ReadLine() ?? "";

                Console.Write("Port du serveur : ");
                port = Console.ReadLine() ?? "";

                Console.Write("Nom d'utilisateur : ");
                name = Console.ReadLine() ?? "";

                bool ipIsValid = IPAddress.TryParse(ipAddress, out _);
                bool portIsValid = int.TryParse(port, out int portNumber)
                                    && portNumber > 0
                                    && portNumber <= 65535;

                isValid = ipIsValid && portIsValid;

                if (!isValid)
                {
                    Console.WriteLine("Adresse IP ou port invalide. Veuillez réessayer.");
                }

            } while (!isValid);

            return [ipAddress, port,name];
        }

        public bool AskPlayerReplay()
        {            
            var messages = new string[]
            {
                "Voulez-vous rejouer ?"
            };

            return AskYesOrNo(messages);
        }

        public string AskPlayerType()
        {

            String playerType = string.Empty;
            var message = new string[]
            {
                "Voulez-vous créer une partie ?"
            };
            var isHost = AskYesOrNo(message);
            return isHost ? "HOST" : "PLAYER";
        }

        public void ShowMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void ShowWinner(Robot robot)
        {
            if(robot == null)
            {
                return; // todo supprimer return
            }
            Console.WriteLine();
            Console.WriteLine(robot.IsHost ? "L'hôte remporte la partie !": "Le joueur remporte la partie !");
        }

        private void DisplayRobotConfig(RobotConfig robotConfig)
        {
            Console.WriteLine("Configuration de votre robot :");
            Console.WriteLine($"HP {config.BaseHp + robotConfig.HpPoints * config.HpPerPoint}");
            Console.WriteLine($"DEF {config.BaseArmor + robotConfig.ArmorPoints * config.ArmorPerPoint}");
            Console.WriteLine($"ATT {config.BaseDamage + robotConfig.DamagePoints * config.DamagePerPoint}");
        }


        public void DisplayFight(Robot localRobot, Robot remoteRobot)
        {
            Console.Clear();

            Console.WriteLine($"        Vous                Adversaire");
            Console.WriteLine();
            Console.WriteLine("      [ O_O ]                  [ O_O ]");
            Console.WriteLine("     /|#####|\\                /|#####|\\");
            Console.WriteLine("      |#####|                  |#####|");
            Console.WriteLine("     /|     |\\                /|     |\\");
            Console.WriteLine("    /_|_____|_\\              /_|_____|_\\");
            Console.WriteLine();

            Console.WriteLine($"HP      {localRobot.GetStats(StatsType.HP),3}           HP      {remoteRobot.GetStats(StatsType.HP),3} ");
            Console.WriteLine($"{"Énergie  "+ localRobot.GetStats(StatsType.ENERGY) + "/" + config.MaxEnergy}       Énergie  {remoteRobot.GetStats(StatsType.ENERGY)}/{config.MaxEnergy}");
            Console.WriteLine();
        }

        private void DisplayRepartPoints(int pointsRemaining)
        {
            Console.WriteLine();
            Console.WriteLine("Choisissez une configuration pour votre robot");
            Console.WriteLine($"Vous avez {pointsRemaining} point{(pointsRemaining > 1 ? "s" : "")} à répartir :");
            Console.WriteLine($"1 - Points de vie (HP) : +{config.HpPerPoint} par point dépensé");
            Console.WriteLine($"2 - Points de défense (DEF) : +{config.ArmorPerPoint} par point dépensé");
            Console.WriteLine($"3 - Points d'attaque (ATT) : +{config.DamagePerPoint} par point dépensé");
        }


        private static bool AskYesOrNo(string[] messages)
        {
            while (true)
            {
                foreach (var message in messages)
                {
                    Console.WriteLine(message);
                }
                Console.Write("Votre choix (o/n): ");

                switch (Console.ReadLine().ToLowerInvariant())
                {
                    case "o":
                    case "oui":
                        return true;
                    case "n":
                    case "non":
                        return false;
                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }
            }
        }
    }
}