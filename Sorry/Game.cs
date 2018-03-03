using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sorry
{

    public delegate void GameOverEvent(Player winner);
    public delegate void TurnEvent(Player player);
    public delegate void MovedEvent(Pawn pawn, int fromPosition, int toPosition);
    public delegate void GameErrorEvent(Player player, Exception error);

    public struct MoveResult
    {
        public int Distance;
        public Kick[] Kicked;
        public int Position;
    }

    public struct Kick
    {
        public Board.Color Color;
        public int Position;
    }

    public class Game
    {
        internal Board Board;

        public Player[] Players { get; internal set; }

        public Player CurrentPlayer { get; internal set; }
        public int CurrentCard { get; internal set; }
        public int RemainingDistance { get; internal set; } // used for 7s
        public bool TurnFulfilled { get; internal set; }

        public event GameOverEvent GameOver;
        public event TurnEvent Turn;
        public event MovedEvent Moved;
        public event GameErrorEvent GameError;

        public Game(Player[] players)
        {
            this.Players = players;
            Board = new Board();

            // validate player count
            if (players.Length > 4 || players.Length < 2)
            {
                throw new ApplicationException("Player count must be greater than 1 and less than 5");
            }

            HashSet<Board.Color> colors = new HashSet<Board.Color>();

            foreach (Player player in Players)
            {
                // validates unique player colors
                if (!colors.Add(player.Color))
                {
                    throw new ApplicationException("Each player must have a separate color");
                }
            }
            // set initial player
            CurrentPlayer = Players[0];
        }

        public int Draw()
        {
            if (CurrentCard == 2)
            {
                CurrentCard = Board.Deck.Draw();
            }
            else
            {
                throw new ApplicationException("Player cannot draw when current card is not a 2");
            }
            return CurrentCard;
        }

        public MoveResult Move(Pawn pawn, int distance, bool apply = false)
        {
            int fromPosition = pawn.Position;
            // return value
            MoveResult ret = new MoveResult();
            // list of pawns kicked back to their start by this move
            List<Kick> kicks = new List<Kick>();

            // can't leave start on certain cards
            if (pawn.Position == Board.POSITION_START)
            {
                if (CurrentCard != 2 && CurrentCard != 1)
                {
                    throw new ApplicationException("Cannot leave Start position on current card");
                }
                else if (CurrentCard == 2 && distance != 1)
                {
                    throw new ApplicationException("Invalid distance");
                }
            }
            // can't leave home with any card
            else if (pawn.Position == Board.POSITION_HOME)
            {
                throw new ApplicationException("Cannot move pawn from Home position");
            }
            // 7 cards can be split between two moves
            else if (CurrentCard == 7)
            {
                // first half of split
                if (RemainingDistance == 0 && (distance < 0 || distance > 7))
                {
                    throw new ApplicationException("Invalid distance");
                }
                // if this is the 1st half, validate that 2nd half is possible
                else if (RemainingDistance == 0 && distance < 7) 
                {
                    // check that maxHomeDistance is >= 7-distance for at least one of the other 3 pawns
                    int minNeeded = 7 - distance;
                    bool minMet = false;
                    for (int i = 0; i < 4; i++)
                    {
                        Pawn p = CurrentPlayer.Pawn(i);
                        if (p.ID != pawn.ID && p.Position != Board.POSITION_START) 
                        {
                            int d = Board.DistanceToHome(p);
                            if (d >= minNeeded) 
                            {
                                minMet = true;
                                break;
                            }
                        }
                    }
                    if (!minMet) 
                    {
                        throw new ApplicationException("Invalid distance");
                    }
                }
                // second half of split
                else if (RemainingDistance > 0 && RemainingDistance != distance)
                {
                    throw new ApplicationException("Invalid distance");
                }
            }
            // 4 cards must be backwards
            else if (CurrentCard == 4)
            {
                if (distance != -4)
                {
                    throw new ApplicationException("Invalid distance");
                }
            }
            // 10 cards can be 10 forward or 1 backward
            else if (CurrentCard == 10)
            {
                if (distance != 10 && distance != -1)
                {
                    throw new ApplicationException("Invalid distance");
                }
            }
            // all other cards must be forward their number
            else if (distance != CurrentCard)
            {
                throw new ApplicationException("Invalid distance");
            }

            // set increment based on whether we're going forward or backward
            int increment = distance > 0 ? 1 : -1;
            int position = pawn.Position;

            // iterate through spaces for distance
            for (int i = 0; i < Math.Abs(distance); i++)
            {
                // if we get home and still have distance to go, it's an invalid move
                if (position == Board.POSITION_HOME)
                {
                    throw new ApplicationException("Pawn reached home before completing distance");
                }
                // backwards board loop condition
                if (position == 0 && increment == -1)
                {
                    position = 59;
                }
                // forwards board loop condition
                else if (position == 59 && increment == 1)
                {
                    position = 0;
                }
                // exit safety condition
                else if (position == -1 && increment == -1)
                {
                    position = Board.ExitPosition(pawn.Color);
                }
                // enter safety condition
                else if (position == Board.ExitPosition(pawn.Color) && increment == 1)
                {
                    position = -1;
                }
                // exit Start condition
                else if (position == Board.POSITION_START)
                {
                    position = Board.EntryPosition(pawn.Color);
                }
                // enter Home condition
                else if (position == Board.POSITION_HOME + 1)
                {
                    position = Board.POSITION_HOME;
                }
                // move within safety
                else if (position < 0)
                {
                    position += (increment * -1);
                }
                // move within standard spaces
                else
                {
                    position += increment;
                }

                ret.Distance++;
            }

            // check for slides and kicks, unless we've reached home
            if (position != Board.POSITION_HOME)
            {
                Space space = GetSpace(position, pawn.Color);
                // landing space is occupied, kick the resident back to start
                if (space.Resident != null)
                {
                    Kick kick = new Kick
                    {
                        Color = space.Resident.Color,
                        Position = space.Resident.Position
                    };
                    kicks.Add(kick);
                }
                // slide if we landed on a slide start of another color
                if (space.SlideType == Board.SlideType.Start && space.Color != CurrentPlayer.Color)
                {
                    while (true)
                    {
                        position++;
                        ret.Distance++;
                        space = Board.Spaces[position];
                        // kick any pawn encountered on the slide
                        if (space.Resident != null)
                        {
                            Kick kick = new Kick
                            {
                                Color = space.Resident.Color,
                                Position = space.Resident.Position
                            };
                            kicks.Add(kick);
                        }
                        if (space.SlideType == Board.SlideType.Stop)
                        {
                            break;
                        }
                    }
                }
            }
            ret.Position = position;

            // apply move if requested
            if (apply)
            {
                // process kicks
                foreach (Kick kick in kicks)
                {
                    Space space = GetSpace(kick.Position, kick.Color);
                    Pawn kickedPawn = space.Resident;
                    kickedPawn.Position = Board.POSITION_START;
                    space.Resident = null;
                    Moved(kickedPawn, space.Position, Board.POSITION_START);
                }
                // remove as resident from current space
                Space orig = GetSpace(pawn.Position, pawn.Color);
                if (orig != null) orig.Resident = null;

                // set as resident in landing space
                Space landing = GetSpace(position, pawn.Color);
                if (landing != null) landing.Resident = pawn;
                pawn.Position = position;

                // retain remainder of a 7 card
                if (CurrentCard == 7 && RemainingDistance == 0)
                {
                    RemainingDistance = 7 - distance;
                    TurnFulfilled = RemainingDistance == 0;
                }
                else
                {
                    RemainingDistance = 0;
                    TurnFulfilled = true;
                }

                // raise moved event
                Moved(pawn, fromPosition, position);
            }

            ret.Kicked = kicks.ToArray();
            return ret;
        }

        /** Returns list of candidate pawns to be replaced by a Sorry card */
        public List<Pawn> GetOpponentsOnBoard()
        {
            List<Pawn> candidates = new List<Pawn>();
            foreach (Player player in Players)
            {
                if (player.Color != CurrentPlayer.Color)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Pawn pawn = player.Pawn(i);
                        if (pawn.Position >= 0) candidates.Add(pawn);
                    }
                }
            }
            return candidates;
        }

        public void PlayElevenCard(Pawn playerPawn, Pawn target)
        {
            // apply several validation rules
            if (playerPawn.Color != CurrentPlayer.Color)
            {
                throw new ApplicationException("Player pawn is not owned by current player");
            }
            if (CurrentCard != 11)
            {
                throw new ApplicationException("Current card is not an 11");
            }
            if (playerPawn.Position < 0)
            {
                throw new ApplicationException("Player pawn must be on the board");
            }
            if (target.Color == CurrentPlayer.Color)
            {
                throw new ApplicationException("Cannot apply 11 card to current player");
            }
            if (target.Position < 0)
            {
                throw new ApplicationException("Target pawn must be on the board");
            }
            int fromPosition = playerPawn.Position;
            int toPosition = target.Position;
            target.Position = fromPosition;
            playerPawn.Position = toPosition;
            Moved(target, toPosition, Board.POSITION_START);
            Moved(playerPawn, fromPosition, toPosition);
            Moved(target, Board.POSITION_START, fromPosition);
            TurnFulfilled = true;
        }

        public void PlaySorryCard(Pawn playerPawn, Pawn target)
        {
            // apply several validation rules
            if (playerPawn.Color != CurrentPlayer.Color)
            {
                throw new ApplicationException("Player pawn is not owned by current player");
            }
            if (playerPawn.Position != Board.POSITION_START)
            {
                throw new ApplicationException("Player pawn must be in Start position");
            }
            if (CurrentCard != 0)
            {
                throw new ApplicationException("Current card is not a Sorry card");
            }
            if (target.Color == CurrentPlayer.Color)
            {
                throw new ApplicationException("Cannot apply Sorry card to current player");
            }
            if (target.Position < 0)
            {
                throw new ApplicationException("Target pawn must be on the board");
            }

            // send Target back to start and put player pawn in its place
            Board.Spaces[target.Position].Resident = playerPawn;
            playerPawn.Position = target.Position;
            target.Position = Board.POSITION_START;
            TurnFulfilled = true;
            Moved(target, playerPawn.Position, Board.POSITION_START);
            Moved(playerPawn, Board.POSITION_START, playerPawn.Position);
        }

        // gets a space for a given position from the main board or player's safety zone
        // returns null for Home or Start
        internal Space GetSpace(int position, Board.Color playerColor)
        {
            Space ret = null;
            if (position >= 0)
            {
                ret = Board.Spaces[position];
            }
            else if (position < 0 && position > Board.POSITION_HOME)
            {
                ret = Board.Safety[(int)playerColor, Math.Abs(position) - 1];
            }
            return ret;
        }

        public bool PlayerWon()
        {
            int count = 0;
            for (int i = 0; i < 4; i++)
            {
                if (CurrentPlayer.Pawn(i).Position == Board.POSITION_HOME)
                {
                    count++;
                }
            }
            return count == 4;
        }

        /// <summary>
        /// Returns the first pawn of the current player found in Start or null if there are none.
        /// </summary>
        /// <returns></returns>
        public Pawn GetStartPawn()
        {
            return GetStartPawn(CurrentPlayer);
        }

        /// <summary>
        /// Returns the first pawn found in Start or null if there are none.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public Pawn GetStartPawn(Player player)
        {
            for (int i = -0; i < 4; i++)
            {
                if (player.Pawn(i).Position == Sorry.Board.POSITION_START) return player.Pawn(i);
            }
            return null;
        }

        /// <summary>
        /// Returns true if the player can't act on the current card
        /// </summary>
        /// <returns></returns>
        public bool AllowPass()
        {
            bool hasPawnInStart = GetStartPawn(CurrentPlayer) != null;
            int maxHomeDistance = 0;
            for (int i = 0; i < 4; i++)
            {
                Pawn p = CurrentPlayer.Pawn(i);
                if (p.Position != Board.POSITION_START)
                {
                    int d = Board.DistanceToHome(p);
                    if (d > maxHomeDistance) maxHomeDistance = d;
                }
            }
            if (CurrentCard == 0)
            {
                return !hasPawnInStart || GetOpponentsOnBoard().Count == 0;
            }
            else if (CurrentCard == 1 || CurrentCard == 2)
            {
                return !hasPawnInStart && (maxHomeDistance == 0 || maxHomeDistance < CurrentCard);
            }
            else if (CurrentCard == 4)
            {
                return maxHomeDistance == 0;
            }
            else if (CurrentCard == 3 || CurrentCard == 5 || CurrentCard == 8 || CurrentCard == 12)
            {
                return maxHomeDistance == 0 || maxHomeDistance < CurrentCard;
            }
            else if (CurrentCard == 7)
            {
                if (maxHomeDistance == 0) return true;
                else if (RemainingDistance > 0) return maxHomeDistance >= RemainingDistance;
                else return maxHomeDistance < CurrentCard;
            }
            else if (CurrentCard == 10)
            {
                return maxHomeDistance < 10 || maxHomeDistance > 0;
            }
            else if (CurrentCard == 11)
            {
                return maxHomeDistance < 11 || maxHomeDistance == 0;
            }
            else return false;
        }

        public void Cheat(int card)
        {
            this.CurrentCard = card;
        }

        public void Start()
        {
            CurrentCard = Board.Deck.Draw();
            while (true)
            {
                // pass control to player
                bool passAllowed = AllowPass();
                TurnFulfilled = false;
                Turn?.Invoke(CurrentPlayer);
                while (true) 
                {
                    CurrentPlayer.Play(this);
                    // make sure the turn was completed
                    if (!TurnFulfilled && !passAllowed && CurrentCard != 7)
                    {
                        GameError?.Invoke(CurrentPlayer, new IncompleteTurnException("Player returned without completing turn"));
                    } 
                    else break;
                }
                if (TurnFulfilled && PlayerWon())
                {
                    GameOver?.Invoke(CurrentPlayer);
                    break;
                }
                else if (TurnFulfilled || passAllowed)
                {
                    // end turn
                    if (CurrentCard!=2) {
                        NextPlayer();
                    }
                    // draw card
                    CurrentCard = Board.Deck.Draw();
                }
            }
        }

        Player GetPlayer(Board.Color color)
        {
            foreach (Player player in Players)
            {
                if (player.Color == color) return player;
            }
            return null;
        }

        void NextPlayer()
        {
            Player next = null;
            Board.Color color = CurrentPlayer.Color;
            int c = (int)color;
            while (next == null)
            {
                c++;
                if (c > 3) c = 0;
                next = GetPlayer((Board.Color)c);
            }
            CurrentPlayer = next;
        }

        public int DistanceToHome(Pawn pawn)
        {
            return Board.DistanceToHome(pawn);
        }

        public int EntryPosition(Board.Color color)
        {
            return Board.EntryPosition(color);
        }

        public int ExitPosition(Board.Color color)
        {
            return Board.ExitPosition(color);
        }

        public Pawn GetPawnAt(int position) 
        {
            return Board.Spaces[position].Resident;
        }
    }
}
