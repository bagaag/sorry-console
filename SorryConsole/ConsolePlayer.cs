using Sorry;
using System;
using System.Collections.Generic;

namespace SorryConsole
{
    class ConsolePlayer : Player
    {
        internal ConsolePlayer(Board.Color color) : base(color){ }
        internal Game game;
        internal Player player;
        internal int card;
        public ConsoleBoard Board;

        public override void Play(Game g)
        {
            game = g;
            player = g.CurrentPlayer;
            card = game.CurrentCard;
            Console.Write(Board.ToString());
            TurnPrompt();
            Console.WriteLine();
            Console.Write("> ");
            string cmd = Console.ReadLine();
            while (true)
            {
                if (cmd == "board")
                {
                    Console.Write(Board.ToString());
                    TurnPrompt();
                }
                else if (cmd == "pawns")
                {
                    Pawns();
                }
                else if (cmd.StartsWith("move "))
                {
                    cmd = cmd.Substring(5);
                    string[] parts = cmd.Split(' ');
                    int i, pawn, distance;
                    if ((parts.Length == 1 && !int.TryParse(parts[0], out i))
                        || (parts.Length == 2 && (!int.TryParse(parts[0], out i) || !int.TryParse(parts[1], out i)))
                        || parts.Length <= 0 || parts.Length > 2)
                    {
                        Console.WriteLine("Move command requires one or two numeric parameters. Use 'help move' for details.");
                    }
                    else {
                        pawn = int.Parse(parts[0]);
                        if (parts.Length==1) 
                        {
                            distance = game.CurrentCard;
                            if (distance == 4) distance = -4;
                        }
                        else
                        {
                            distance = int.Parse(parts[1]);
                        } 
                        if (Move(pawn, distance))
                        {
                            break;
                        }
                    }
                }
                else if (cmd == "")
                {
                    try
                    {
                        ComputerPlayer computerPlayer = new ComputerPlayer(player.Color);
                        computerPlayer.Play(g);
                        break;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else if (cmd == "exit")
                {
                    Exit();
                }
                else if (cmd.StartsWith("help"))
                {
                    string[] split = cmd.Split(' ');
                    if (split.Length>1) 
                    {
                        Help(split[1]);
                    }
                    else 
                    {
                        Help();
                    }
                }
                else if (cmd == "pass")
                {
                    if (game.AllowPass())
                    {
                        Pass();
                        break;
                    }
                    else 
                    {
                        Console.WriteLine("Cannot pass on playable card.");
                    }
                }
                else if (cmd == "candidates")
                {
                    Candidates();
                }
                else if (cmd == "distance")
                {
                    Distance();
                }
                else if (cmd == "info")
                {
                    Info();
                }
                else if (cmd.StartsWith("replace "))
                {
                    string[] split = cmd.Split(' ');
                    try
                    {
                        Replace(int.Parse(split[1]), int.Parse(split[2]));
                        break;
                    } catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else if (cmd.StartsWith("cheat "))
                {
                    string[] split = cmd.Split(' ');
                    if (split.Length>1) 
                    {
                        Cheat(int.Parse(split[1]));
                        TurnPrompt();
                    }
                }
                else
                {
                    Console.WriteLine("Invalid command. Enter 'help' for assistance.");
                }
                Console.WriteLine();
                Console.Write("> ");
                cmd = Console.ReadLine();
            }
        }

        void TurnPrompt() 
        {
            if (game.CurrentCard == 7 && game.RemainingDistance > 0)
            {
                Console.WriteLine(player.Color + " has " + game.RemainingDistance + " remaining on a 7.");
            }
            else 
            {
                Console.WriteLine(player.Color + " draws a " + (game.CurrentCard == 0 ? "Sorry" : "" + game.CurrentCard) + ".");
            }
        }

        internal void Pass()
        {
            Console.WriteLine(game.CurrentPlayer.Color + " passes.");            
        }
  
        void Help()
        {
            Console.WriteLine("Commands:");
            Console.WriteLine("  (blank)         Let the computer handle the current turn");
            Console.WriteLine("  board           Display the game board");
            Console.WriteLine("  candidates      List opponent pawns in play on the board with their positions");
            Console.WriteLine("  exit            Quit the game");
            Console.WriteLine("  help [command]  Print this help or get help for specified command");
            Console.WriteLine("  info            Display game and current player information");
            Console.WriteLine("  move # [#]      Move specified current player's pawn specied number of spaces");
            Console.WriteLine("  pass            End current turn");
            Console.WriteLine("  replace # #     Replace opponent pawn with current player pawn");
        }

        void Help(string cmd) 
        {
            if (cmd.Equals("(blank)") || cmd.Equals("blank")) 
            {
                Console.WriteLine("Not yet implemented");
            }
            else if (cmd.Equals("board"))
            {
                Console.WriteLine("Displays the game board.");
            }
            else if (cmd.Equals("candidates")) 
            {
                Console.WriteLine("Lists opponent pawns in play on the board with their positions. This is useful");
                Console.WriteLine("for finding a pawn to land on or replace with a Sorry or 11 card. Pawns in");
                Console.WriteLine("Home, Home Stretch and Start positions are not listed. Each pawn listed is");
                Console.WriteLine("numbered, and that number can be used in the replace command.");
            }
            else if (cmd.Equals("distance")) 
            {
                Console.WriteLine("Prints the distance to Home for each of the current player's pawns.");
            }
            else if (cmd.Equals("exit")) 
            {
                Console.WriteLine("Quits the game.");
            }
            else if (cmd.Equals("help"))
            {
                Console.WriteLine("Use 'help' by itself to see a list of commands. Use 'help <command>' to see more");
                Console.WriteLine("detailed help for the specified command");
            }
            else if (cmd.Equals("info")) 
            {
                Console.WriteLine("Displays game and current player information.");
            }
            else if (cmd.Equals("move"))
            {
                Console.WriteLine("Moves the pawn indicated by the first number the number of spaces indicated by");
                Console.WriteLine("the second number. If the second number is omitted, the distance defaults to");
                Console.WriteLine("the value of the current card. Use the 'pawns' command for each pawn's number");
                Console.WriteLine("and location.");
                Console.WriteLine("Examples:");
                Console.WriteLine("  move 1       moves pawn #1 the default distance based on current card");
                Console.WriteLine("  move 1 5     moves pawn #1 forward five spaces");
                Console.WriteLine("  move 2 -4    moves pawn #2 back four spaces");
            }
            else if (cmd.Equals("pass"))
            {
                Console.WriteLine("The current player can use 'pass' when they cannot act on the card drawn. For");
                Console.WriteLine("example, if all pawns are in Start and a 5 card is drawn, the player must pass.");
            }
            else if (cmd.Equals("pawns"))
            {
                Console.WriteLine("Displays the current player's pawns and their positions on the board. Use the");
                Console.WriteLine("'info' command to see important positions for the current player.");
            }
            else if (cmd.Equals("replace"))
            {
                Console.WriteLine("Replaces the opponent pawn indicated by the first number with the current");
                Console.WriteLine("player's pawn indicated by the second number. Use the 'candidates' command for");
                Console.WriteLine("a list of opponent pawns and their numbers. Use the 'pawns' command for a list");
                Console.WriteLine("of current player's pawns and their numbers.");
                Console.WriteLine("Example: ");
                Console.WriteLine("  replace 1 2     replaces opponent pawn #1 with current player's pawn #2");
            }
            else 
            {
                Console.WriteLine("Unknown command or extended help unavailable.");
            }
        }

        void Info()
        {
            Board.Color color = game.CurrentPlayer.Color;
            string currentPlayer = color.ToString();
            int entryPosition = game.EntryPosition(color);
            int exitPosition = game.ExitPosition(color);
            Console.WriteLine("Current player: " + currentPlayer);
            Console.WriteLine("  Location on the board Start enters to: " + entryPosition);
            Console.WriteLine("  Location on the board where Home Stretch exits: " + exitPosition);
            Console.WriteLine("Pawn positions:");
            Pawns();
            Console.WriteLine("Pawn distances from home:");
            Distance();
        }

        void Candidates()
        {
            List<Pawn> pawns = game.GetOpponentsOnBoard();
            for (int i=0; i<pawns.Count; i++) 
            {
                Pawn pawn = pawns[i];
                Console.WriteLine((i+1) + ": " + pawn.Color + " at " + Program.TranslatePosition(pawn.Position));
            }
            if (pawns.Count==0)
            {
                Console.WriteLine("No opponent pawns on board.");
            }
        }

        internal void Replace(int them, int me)
        {
            List<Pawn> pawns = game.GetOpponentsOnBoard();
            Pawn target = pawns[them - 1];
            if (game.CurrentCard == 11)
            {
                game.PlayElevenCard(game.CurrentPlayer.Pawn(me - 1), target);
            }
            else if (game.CurrentCard == 0)
            {
                game.PlaySorryCard(game.CurrentPlayer.Pawn(me - 1), target);
            }
        }

        void Exit()
        {
            Console.WriteLine("Goodbye.");
            Environment.Exit(0);
        }

        void Pawns()
        {
            for (int i=0; i<4; i++)
            {
                Pawn p = player.Pawn(i);
                Console.WriteLine("  " + (p.ID+1) + " is at " + Program.TranslatePosition(p.Position) + ".");
            }
        }

        internal bool Move(int pawn, int distance)
        {
            try
            {
                game.Move(player.Pawn(pawn - 1), distance, true);
                return true;
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        void Distance()
        {
            for (int i=0; i<4; i++)
            {
                Console.WriteLine("  " + (i + 1) + ": " + game.DistanceToHome(player.Pawn(i)));
            }
        }

        void Cheat(int card) 
        {
            game.Cheat(card);            
        }

    }
}
