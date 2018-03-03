using Sorry;
using System;
using System.Collections.Generic;
using System.Text;

namespace SorryConsole
{
    public class Point
    {
        public int X; // 0-34
        public int Y; // 0-17
        internal Point(int x, int y) { X = x; Y = y; }
    }

    public class ConsoleBoard
    {
        string sTemplate = "" +
            "Y---------------------------------G`" +
            "| . > - - o . . . . > - - - o . . |`" +
            "| .   |  + +                    v |`" +
            "| o   |  + +      + + - - - - - | |`" +
            "| |   |           + +           | |`" +
            "| |   |                     + + o |`" +
            "| |   |                     + + . |`" +
            "| ^  + +    Player: .           . |`" +
            "| .  + +      Card: .           . |`" +
            "| .                        + +  . |`" +
            "| .                        + +  v |`" +
            "| . + +                     |   | |`" +
            "| o + +                     |   | |`" +
            "| |           + +           |   | |`" +
            "| | - - - - - + +      + +  |   o |`" +
            "| ^                    + +  |   . |`" +
            "| . . o - - - < . . . . o - - < . |`" +
            "B---------------------------------R";
        List<Point[]> starts = new List<Point[]>();
        List<Point[]> homes = new List<Point[]>();
        List<Point[]> safeties = new List<Point[]>();
        Point[] spaces = new Point[60];
        List<char[]> board = new List<char[]>();
        List<char[]> template = new List<char[]>();
        char[] pawnChar = new char[4] { 'Y', 'G', 'R', 'B' };

        public ConsoleBoard(Game game)
        {
            // create board
            string[] lines = sTemplate.Split("`");
            foreach (String line in lines)
            {
                board.Add(line.ToCharArray()); // gets updated with game action
                template.Add(line.ToCharArray()); // kept clean for returning board to default state
            }
            // create start and home positions (yellow, green, red, blue)
            starts.Add(new Point[4] { new Point(9, 2), new Point(11, 2), new Point(9, 3), new Point(11, 3) });
            starts.Add(new Point[4] { new Point(28, 5), new Point(30, 5), new Point(28, 6), new Point(30, 6) });
            starts.Add(new Point[4] { new Point(23, 14), new Point(25, 14), new Point(23, 15), new Point(25, 15) });
            starts.Add(new Point[4] { new Point(4, 11), new Point(6, 11), new Point(4, 12), new Point(6, 12) });
            homes.Add(new Point[4] { new Point(5, 7), new Point(7, 7), new Point(5, 8), new Point(7, 8) });
            homes.Add(new Point[4] { new Point(19, 3), new Point(21, 3), new Point(19, 4), new Point(21, 4) });
            homes.Add(new Point[4] { new Point(27, 9), new Point(29, 9), new Point(27, 10), new Point(29, 10) });
            homes.Add(new Point[4] { new Point(14, 13), new Point(16, 13), new Point(14, 14), new Point(16, 14) });
            
            // create safety zones (yellow, green, red, blue)
            safeties.Add(new Point[5] { new Point(6, 2), new Point(6, 3), new Point(6, 4), new Point(6, 5), new Point(6, 6) });
            safeties.Add(new Point[5] { new Point(30, 3), new Point(28, 3), new Point(26, 3), new Point(24, 3), new Point(22, 3) });
            safeties.Add(new Point[5] { new Point(28, 15), new Point(28, 14), new Point(28, 13), new Point(28, 12), new Point(28, 11) });
            safeties.Add(new Point[5] { new Point(4, 14), new Point(6, 14), new Point(8, 14), new Point(10, 15), new Point(12, 14) });
            
            // populate array of points that correspond with the 60 board positions
            Point[] corners = new Point[4] { new Point(2, 1), new Point(32, 1), new Point(32, 16), new Point(2, 16) };
            Point[] increments = new Point[4] { new Point(2, 0), new Point(0, 1), new Point(-2, 0), new Point(0, -1) };
            int ix = 0;
            Point tmp, incr;
            for (int side = 0; side < 4; side++)
            {
                tmp = corners[side];
                incr = increments[side];
                for (int i = 0; i < 15; i++)
                {
                    spaces[ix] = new Point(tmp.X, tmp.Y);
                    tmp.X += incr.X;
                    tmp.Y += incr.Y;
                    ix++;
                }
            }
            // start pawns
            foreach (Player player in game.Players) {
                for (int i = 0; i < 4; i++)
                {
                    SetPosition(Board.POSITION_START, player.Pawn(i));
                }
            }
            // set current player
            SetCurrentPlayer(game.CurrentPlayer.Color, game.CurrentCard);
        }

        public void MovePawn(Pawn pawn, int fromPosition, int toPosition)
        {
            ResetPosition(fromPosition, pawn);
            SetPosition(toPosition, pawn);
        }

        /// <summary>
        /// Update the board with the given pawn color
        /// </summary>
        /// <param name="p"></param>
        /// <param name="color"></param>
        public void SetPosition(int position, Pawn pawn)
        {
            Point point = GetPoint(position, pawn);
            char c = pawnChar[(int)pawn.Color];
            board[point.Y][point.X] = c;
        }

        public Point GetPoint(int position, Pawn pawn)
        {
            Point point;
            int color = (int)pawn.Color;
            if (position < Board.POSITION_START || position > 59)
            {
                throw new ApplicationException("Invalid poisition: " + position);
            }
            else if (position == Board.POSITION_HOME)
            {
                point = homes[color][pawn.ID];
            }
            else if (position == Board.POSITION_START)
            {
                point = starts[color][pawn.ID];
            }
            else if (position < 0)
            {
                point = safeties[color][Math.Abs(position) - 1];
            }
            else
            {
                point = spaces[position];
            }
            return point;
        }

        /// <summary>
        /// Resets the given point from the board template
        /// </summary>
        /// <param name="p"></param>
        public void ResetPosition(int position, Pawn pawn)
        {
            Point p = GetPoint(position, pawn);
            board[p.Y][p.X] = template[p.Y][p.X];
        }

        /// <summary>
        /// Displays current player color and current card in center of board
        /// </summary>
        /// <param name="color"></param>
        public void SetCurrentPlayer(Board.Color color, int card)
        {
            board[7][20] = pawnChar[(int)color];
            char[] cardchars = ("" + card).ToCharArray();
            board[8][20] = cardchars[0];
            board[8][21] = cardchars.Length==2 ? cardchars[1] : ' ';
        }

        /// <summary>
        /// TODO: Add color! https://stackoverflow.com/questions/2743260/is-it-possible-to-write-to-the-console-in-colour-in-net
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int y=0; y<board.Count; y++)
            {
                sb.Append(" ");
                sb.Append(board[y]);
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
    }
}
