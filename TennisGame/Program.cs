using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace TennisGameDojo
{
    class Program
    {
        static void Main( string[] args ) {
        }
    }


    public class Tennis
    {
        private const int MAX_SET_POINT_MARGIN = 2;
        private const int MAX_SET_WON_MARGIN = 2;
        private const int MAX_PLAYERS = 2;
        private const int MAX_SET_POINTS = 6;


        public List<TennisPlayer> Players { get; internal set; }
        public List<PointHistory> PointHistoryList { get; set; }
        public bool Deuce { get; private set; }
        public int GameDelay { get; set; }
        public bool IsInAdvantage { get; set; }
        public Guid PlayerInAdvantage { get; set; }
        public bool IsMatchOver { get; set; }


        /// <summary>
        /// initialize a new game in the construtor
        /// </summary>
        /// <param name="playerLists"></param>
        public Tennis( List<TennisPlayer> playerLists ) {
            if (playerLists == null)
                throw new ArgumentNullException();

            if (playerLists.Count != MAX_PLAYERS) {
                throw new ArgumentException( "must be played by two players" );
            }

            foreach (var player in playerLists) {
                player.GamePoints = GamePointEnum.ZERO;
                player.Identifier = Guid.NewGuid();
            }

            PointHistoryList = new List<PointHistory>();
            Players = playerLists;
        }


        /// <summary>
        /// This is where the actual game is going on.
        /// </summary>
        public void Play() {
            //a game could, if you wanted, take some time to find a point
            SetDelay();

            //find a winner amongst the players
            PickGameWinner();

            //we must update the (game/set/match) winner points
            SetGamePointForGameWinner();
        }


        public void SetGamePointForGameWinner() {
            var gameWinner = GetGameWinner();

            switch (gameWinner.GamePoints) {
                case GamePointEnum.ZERO: {
                    SetGamePoint( GamePointEnum.FIFTEEN );
                    break;
                }
                case GamePointEnum.FIFTEEN: {
                    SetGamePoint( GamePointEnum.THIRTY );
                    break;
                }
                case GamePointEnum.THIRTY: {
                    SetGamePoint( GamePointEnum.FORTY );
                    break;
                }
                case GamePointEnum.FORTY: {
                    SetGamePoint( GamePointEnum.FORTY );
                    UpdateScore();
                    break;
                }
            }
        }

        private void SetGamePoint(GamePointEnum points) {
            var gameWinner = GetGameWinner();

            SetGamePointHistory();
            gameWinner.GamePoints = points;
        }

        private void SetGamePointHistory() {
            var gameWinner = GetGameWinner();
            var gameLoser = GetGameLoser();

            PointHistoryList.Add( new PointHistory { WinnerPoint = gameWinner.GamePoints, LoserPoint = gameLoser.GamePoints, WinnerSetPoint = gameWinner.SetPoints, LoserSetPoint = gameLoser.SetPoints } );
        }

        private void SetGameSetHistory( TennisPlayer winner, TennisPlayer loser ) {
            PointHistoryList.Add( new PointHistory { WinnerPoint = winner.GamePoints, LoserPoint = loser.GamePoints, WinnerSetPoint = winner.SetPoints + 1, LoserSetPoint = loser.SetPoints } );
        }

        private void ResetMatch() {
            IsMatchOver = false;
            PointHistoryList = new List<PointHistory>();
        }

        private void ResetGamePlayers() {
            foreach (var player in Players) {
                player.WonGame = false;
                player.GamePoints = GamePointEnum.ZERO;
                player.Advantage = false;
            }
        }

        private void ResetGame() {
            Deuce = false;
            IsInAdvantage = false;
            PlayerInAdvantage = Guid.Empty;
        }

        private bool IsDeuce() {
            Deuce = false;

            var player1 = Players[0];
            var player2 = Players[1];

            if (player1.GamePoints == GamePointEnum.FORTY) {
                if (player2.GamePoints == GamePointEnum.FORTY) {
                    Deuce = true;
                }
            }

            if (player2.GamePoints == GamePointEnum.FORTY) {
                if (player1.GamePoints == GamePointEnum.FORTY) {
                    Deuce = true;
                }
            }

            if (Deuce) {
                return true;
            }

            return false;
        }

        private void UpdateScore() {
            var winningPlayer = GetGameWinner();

            if (IsDeuce() && IsInAdvantage == false) {
                SetAdvantage();
                return;
            }

            if (IsInAdvantage) {
                if (!IsAdvantagePlayerWinningGame()) {
                    SetAdvantage();
                    return;
                }
            }

            SetPointsForWinner();
            SetSetsWonForWinner();
            MatchOver();

            ResetGamePlayers();
            ResetGame();
        }

        private void SetPointsForWinner() {
            var winningPlayer = GetGameWinner();
            var losingPlayer = GetGameLoser();

            winningPlayer.SetPoints++;
            SetGameSetHistory( winningPlayer, losingPlayer );
        }

        private void SetSetsWonForWinner() {
            var winningPlayer = GetGameWinner();
            var losingPlayer = GetGameLoser();

            var setPointMargin = ( winningPlayer.SetPoints - losingPlayer.SetPoints );

            if (setPointMargin >= MAX_SET_POINT_MARGIN) {
                if (winningPlayer.SetPoints >= MAX_SET_POINTS) {
                    winningPlayer.SetsWon++;

                    //reset setpoints
                    winningPlayer.SetPoints = 0;
                    losingPlayer.SetPoints = 0;
                }
            }
        }

        private void MatchOver() {
            var winningPlayer = GetGameWinner();
            var losingPlayer = GetGameLoser();

            var setsWonMargin = ( winningPlayer.SetsWon - losingPlayer.SetsWon );

            if (setsWonMargin >= MAX_SET_WON_MARGIN) {
                IsMatchOver = true;
            } else {
                IsMatchOver = false;
            }
        }

        private bool IsAdvantagePlayerWinningGame() {
            var winningPlayer = GetGameWinner();

            //player is in advantage and therefor wins game
            if (winningPlayer.Identifier == PlayerInAdvantage) {
                return true;
            }

            //reset advantage since winning player is not in advantage
            foreach (var player in Players) {
                player.Advantage = false;
            }

            return false;
        }

        /// <summary>
        /// advantage can be given to a player if all players has 40 gamepoints
        /// </summary>
        private void SetAdvantage() {
            var winningPlayer = GetGameWinner();
            winningPlayer.Advantage = true;

            IsInAdvantage = true;
            PlayerInAdvantage = winningPlayer.Identifier;
        }

        private void PickGameWinner() {
            //reset all players WonGame prop' so we're certain we pick just one game winner
            foreach (var player in Players) {
                player.WonGame = false;
            }

            //this is a computer game so we pick a random player 
            var gamerWinnerIndex = RandomNumber( 0, MAX_PLAYERS );

            //set the random player as game winner
            Players[gamerWinnerIndex].WonGame = true;
        }

        private TennisPlayer GetGameWinner() {
            var gameWinner = Players.Where( p => p.WonGame == true ).FirstOrDefault();

            if (gameWinner == null) {
                throw new ArgumentException( "get game winner" );
            }

            return gameWinner;
        }

        private TennisPlayer GetGameLoser() {
            var gameLoser = Players.Where( p => p.WonGame == false ).FirstOrDefault();

            if (gameLoser == null) {
                throw new ArgumentException( "get game loser" );
            }

            return gameLoser;
        }

        private void SetDelay() {
            Thread.Sleep( RandomNumber( 0, GameDelay ) );
        }

        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();
        private static int RandomNumber( int min, int max ) {
            lock (syncLock) { // synchronize
                return random.Next( min, max );
            }
        }
    }

    public enum GamePointEnum { ZERO = 0, FIFTEEN = 15, THIRTY = 30, FORTY = 40 };


    public class TennisPlayer
    {
        public Guid Identifier { get; set; }
        public string Name { get; set; }
        public GamePointEnum GamePoints { get; set; }
        public int SetPoints { get; set; }
        public bool WonGame { get; internal set; }
        public bool Advantage { get; internal set; }
        public int SetsWon { get; set; }
    }

    public class PointHistory
    {
        public GamePointEnum WinnerPoint { get; set; }
        public GamePointEnum LoserPoint { get; set; }
        public int WinnerSetPoint { get; set; }
        public int LoserSetPoint { get; set; }
    }

}
