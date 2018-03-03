using System;
using System.Collections.Generic;
using System.Text;

namespace Sorry
{
    internal class Space
    {
        // (0-59) space; (-1 - -5): safety, -6: home, -7: start
        public int Position { get; internal set; }
        public Board.Color Color { get; internal set; }
        public Board.SlideType SlideType { get; internal set; } = Board.SlideType.None;
        public bool Entry { get; internal set; } = false;
        public bool Exit { get; internal set; } = false;
        public Pawn Resident { get; internal set; } = null;
    }
}
