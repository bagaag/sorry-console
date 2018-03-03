using System;
using System.Collections.Generic;
using System.Text;

namespace Sorry
{
    internal class Deck
    {
        private int[] Cards = new int[]{
                0,0,0,0, // 4 Sorry cards
                1,1,1,1,1, // 5 ones
                2,2,2,2, // 4 of everything else, no sixes or nines
                3,3,3,3,
                4,4,4,4,
                5,5,5,5,
                7,7,7,7, 
                8,8,8,8,
                10,10,10,10, 
                11,11,11,11,
                12,12,12,12
            };
        private Stack<int> Pile;
        private Random R = new Random();

        internal Deck()
        {
            Shuffle();
        }

        internal void Shuffle()
        {
            for (int i=0; i<5; i++)
            {
                R.Shuffle(Cards);
            }
            Pile = new Stack<int>(Cards);
        }

        internal int Draw()
        {
            if (Pile.Count == 0)
            {
                Shuffle();
            }
            int i = Pile.Pop();
            return i;
        }
    }
}
