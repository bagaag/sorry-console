using System;
using System.Collections.Generic;
using Sorry;

namespace SorryConsole
{
    class ComputerPlayer : ConsolePlayer
    {
        internal ComputerPlayer(Board.Color color) : base(color){ }

        public override void Play(Game g)
        {
            game = g;
            player = g.CurrentPlayer;
            card = game.CurrentCard;
            bool turnComplete = false;
            Pawn startPawn = game.GetStartPawn();

            if ((card == 1 || card == 2) && startPawn != null)
            {
                turnComplete = StartCard(startPawn);
            }
            else if (card == 3 || card == 5 || card == 8 || card == 12)
            {
                turnComplete = StandardCard();
            }
            else if (card == 4) 
            {
                turnComplete = FourCard();
            }
            else if (card == 7) 
            {
                turnComplete = SevenCard();
            }
            else if (card == 10) 
            {
                turnComplete = TenCard();
            }
            else if (card == 11) 
            {
                turnComplete = ElevenCard();
            }
            else if (card == 0)
            {
                turnComplete = SorryCard(startPawn);
            }
            if (!turnComplete) 
            {
                Pass();
            }
        }

        bool StartCard(Pawn startPawn) 
        {
            // Check for occupation of entry space
            Pawn entryPawn = game.GetPawnAt(game.EntryPosition(player.Color));
            if (entryPawn != null
                && entryPawn.Color == player.Color) 
            {
                //TODO: reuse move optimization for standard cards
                Move(entryPawn.ID+1, card);
                return true;
            } 
            else 
            {
                //TODO: handle failure
                Move(startPawn.ID+1, 1);
                return true;
            }

        }

        bool StandardCard()
        {
            //TODO: optimize for landing replacement
            //TODO: optimize for player closest to winning
            //TODO: optimize for closest to home (test theory)
            for (int i=0; i<4; i++) 
            {
                Pawn pawn = game.CurrentPlayer.Pawn(i);
                if (pawn.Position != Sorry.Board.POSITION_START 
                    && game.DistanceToHome(pawn) >= card)
                {
                    Move(i+1, card);
                    return true;
                }
            }
            return false;
        }

        bool FourCard() 
        {
            for (int i=0; i<4; i++)
            {
                //TODO: optimize
                Pawn pawn = game.CurrentPlayer.Pawn(i);
                if (pawn.Position > Sorry.Board.POSITION_HOME) 
                {
                    Move(pawn.ID+1, -4);
                    return true;
                }
            }
            return false;
        }

        bool SorryCard(Pawn startPawn) 
        {
            //TODO: optimize candidates
            List<Pawn> candidates = game.GetOpponentsOnBoard();
            if (candidates.Count > 0 && startPawn != null)
            {
                Console.WriteLine(":) Sorry!");
                Replace(candidates[0].ID+1, startPawn.ID+1);
                return true;
            } 
            else 
            {
                Console.WriteLine(":( Nothing to be Sorry about.");
            }
            return false;
        }

        bool SevenCard()
        {
            // TODO: Optimize
            return StandardCard();
        }

        bool TenCard()
        {
            // TODO: Optimize
            return StandardCard();
        }

        bool ElevenCard()
        {
            // TODO: Optimize
            return StandardCard();
        }

    }
}