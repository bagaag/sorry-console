using System;
using System.Collections.Generic;
using System.Text;

namespace Sorry
{
    public class Pawn
    {
        public Board.Color Color { get; private set; }

        public int ID { get; private set; }

        // (0-59) space; (-1 - -5): safety, -6: home, -7: start
        public int Position { get; internal set; } = Board.POSITION_START;

        internal Pawn(Board.Color color, int id)
        {
            this.Color = color;
            this.ID = id;
        }

    }
}
