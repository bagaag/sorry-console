using Sorry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SorryConsole
{

    class Program
    {
        static Game game;
        static ConsoleBoard board;
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Sorry");

            // get players from args or user input
            Player[] players = null;
            if (args.Length == 1) {
                players = ParsePlayers(args[0]);
                if (players == null) {
                    Console.WriteLine("Usage:");
                    Console.WriteLine("  Specify number of players: Sorry 2");
                    Console.WriteLine("  Or specify player colors:  Sorry Yellow,Green,Red,Blue");
                    Environment.Exit(0);
                }
            } 
            else 
            {
                while (true) 
                {
                    Console.WriteLine("Enter 2, 3 or 4 to specify number of players.");
                    Console.WriteLine("You can also specify color names as Yellow,Green,Red,Blue.");
                    Console.Write("> ");
                    string playerString = Console.ReadLine();
                    players = ParsePlayers(playerString);
                    if (players != null) break;
                }
            }

            Console.WriteLine();
            Console.WriteLine("Enter 'help' for a list of valid commands.");
            Console.WriteLine();

            // create game and board
            game = new Game(players);
            board = new ConsoleBoard(game);
            foreach (ConsolePlayer player in players)
            {
                player.Board = board;
            }

            // subscribe to game events
            game.Moved += Game_Moved;
            game.GameOver += Game_GameOver;
            game.Turn += Game_Turn;
            game.GameError += Game_GameError;
            game.Start();
        }

        private static Player[] ParsePlayers(string playerString) 
        {
            List<Player> players = new List<Player>();
            // if # of players specified in args as a number
            if (playerString.All(Char.IsDigit)) 
            {
                int count = int.Parse(playerString);
                if (count < 2 || count > 4) 
                {
                    Console.WriteLine("Player count must be between 2 and 4.");
                }
                else 
                {
                    foreach (Board.Color color in Enum.GetValues(typeof(Board.Color))) 
                    {
                        if (players.Count < count) 
                        {
                            players.Add(new ConsolePlayer(color));
                        }
                        else break;
                    }
                }
            }
            // if players are expressed in args as "Blue,Red,Green"
            else 
            {
                string[] colorNames = playerString.Split(",");
                if (colorNames.Length != colorNames.Distinct().Count())
                {
                    Console.WriteLine("Color names must be unique.");
                }
                else 
                {
                    bool validNames = true;
                    foreach (string name in colorNames) 
                    {
                        if (!Enum.IsDefined(typeof(Board.Color), name)) 
                        {
                            Console.WriteLine(name + " is not a valid color name.");
                            validNames = false;
                        }
                    }
                    if (validNames) 
                    {
                        foreach (string name in colorNames) 
                        {                            
                            Board.Color color = (Board.Color) Enum.Parse(typeof(Board.Color), name);
                            players.Add(new ConsolePlayer(color));
                        }
                    }
                }
            }
            if (players.Count==0) 
            {
                return null;
            } else {
                return players.ToArray();
            }
        }

        private static void Game_Turn(Player player)
        {
            
            board.SetCurrentPlayer(player.Color, game.CurrentCard);
        }

        private static void Game_GameOver(Player winner)
        {
            Console.WriteLine(winner.Color + " won the game!");
        }

        private static void Game_GameError(Player winner, Exception e)
        {
            Console.WriteLine("Game error: " + e.Message);
        }


        private static void Game_Moved(Pawn pawn, int fromPosition, int toPosition)
        {
            board.MovePawn(pawn, fromPosition, toPosition);
            Console.WriteLine(pawn.Color + " moved #" + (pawn.ID+1) + " from " + TranslatePosition(fromPosition) + " to " + TranslatePosition(toPosition));
        }

        public static string TranslatePosition(int position)
        {
            if (position == Board.POSITION_HOME) return "Home";
            else if (position == Board.POSITION_START) return "Start";
            else if (position < 0) return "Safety (" + Math.Abs(position) + " of 5)";
            else return ""+position;
        }
    }
}
