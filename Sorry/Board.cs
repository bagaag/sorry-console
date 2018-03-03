using System;
using System.Collections.Generic;

namespace Sorry
{
    public class Board
    {
        public enum SlideType { None, Start, Track, Stop };
        public enum Color { Yellow, Green, Red, Blue };

        internal Space[,] Safety = new Space[4,5];

        internal Space[] Spaces = new Space[60];

        internal int[] Exits = { 2, 17, 32, 47 };
        internal int[] Entries = { 4, 19, 34, 49 };

        public const int POSITION_HOME = -6;
        public const int POSITION_START = -7;

        internal Deck Deck;

        internal Board()
        {
            // prepare the deck
            Deck = new Deck();

            // iterate 4 colors to prepare special spaces
            for (int c=0; c<4; c++)
            {
                // safety lanes have 5 slots each
                for (int b=0; b<5; b++)
                {
                    Space s = new Space();
                    s.Position = (b + 1) * -1;
                    Safety[c, b] = s;
                }
            }

            int[] sides = { 0, 15, 30, 45 };

            // initialize standard space attributes (15 spaces per side)
            for (int i=0; i<15; i++)
            {
                // set attributes for each of the 4 sides of the board
                foreach (int s in sides)
                {
                    Space space = new Space();
                    // set colors
                    space.Color = (Board.Color)(s/15);
                    // set location
                    space.Position = i + s;
                    // set slide types
                    if (i == 1 || i == 9)
                    {
                        space.SlideType = SlideType.Start;
                    }
                    if (i == 2 || i == 3 || i == 10 || i == 11 | i == 12)
                    {
                        space.SlideType = SlideType.Track;
                    }
                    if (i == 4 || i == 13)
                    {
                        space.SlideType = SlideType.Stop;
                    }
                    // set entry points
                    if (i == 4)
                    {
                        space.Entry = true;
                    }
                    // set exit points
                    if (i == 2)
                    {
                        space.Exit = true;
                    }
                    Spaces[i + s] = space;
                }
            }
        }

        public int EntryPosition(Board.Color color)
        {
            return Entries[(int)color];
        }

        public int ExitPosition(Board.Color color)
        {
            return Exits[(int)color];
        }

        /// <summary>
        /// Returns distance to home for the given pawn
        /// </summary>
        /// <param name="pawn"></param>
        /// <returns></returns>
        public int DistanceToHome(Pawn pawn)
        {
            return DistanceToHome(pawn.Color, pawn.Position);
        }

        /// <summary>
        /// Returns distance to home for the given player color and board position
        /// </summary>
        /// <param name="color"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public int DistanceToHome(Board.Color color, int position)
        {
            if (position == Board.POSITION_HOME) return 0;
            if (position == Board.POSITION_START) return 65;
            int exit = ExitPosition(color);
            if (position < 0) return 6 + position;
            else if (exit >= position) return exit - position + 6;
            else return 60 - (position - exit) + 6;
        }

    }
}
