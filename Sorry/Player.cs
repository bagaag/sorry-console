using System;
using System.Collections.Generic;
using System.Text;

namespace Sorry
{
    public abstract class Player {

        private Pawn[] _pawns = new Pawn[4];
        public Board.Color Color { get; private set; }

        public Player(Board.Color color)
        {
            this.Color = color;
            for (int i=0; i<4; i++)
            {
                _pawns[i] = new Pawn(color, i);
            }
        }

        public Pawn Pawn(int id)
        {
            return _pawns[id];
        }

        public abstract void Play(Game game);
    }
}
