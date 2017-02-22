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
        public List<TennisPlayer> Players { get; internal set; }
        public List<PointHistory> PointHistoryList { get; set; }
        public bool Deuce { get; private set; }
        public int GameDelay { get; set; }
        public bool IsInAdvantage { get; private set; }
        public Guid PlayerInAdvantage { get; set; }
        public bool MatchOver { get; set; }

        public Tennis( List<TennisPlayer> playerLists ) {
            if (playerLists == null)
                throw new ArgumentNullException();

            if (playerLists.Count != 2) {
                throw new ArgumentException( "must be played by two players" );
            }

            foreach (var player in playerLists) {
                player.GamePoints = new Point();
                player.Identifier = Guid.NewGuid();
            }

            PointHistoryList = new List<PointHistory>();
            Players = playerLists;
        }


        /// <summary>
        /// This is where the actual game is going on.
        /// </summary>
        public void Play() {
            //a game could take some time to find a point
            SetDelay();

            //find a winner amongst the players
            PickGameWinner();

            //we must update the winner points
            SetGamePointForGameWinner();
        }

        public enum GamePointEnum { ZERO = 0, FIFTEEN = 15, THIRTY = 30, FORTY = 40 };

        public void SetGamePointForGameWinner() {
            var gameWinner = GetGameWinner();
            var gameLoser = GetGameLoser();

            switch (gameWinner.GamePoints.PointValue) {
                case 0: {
                    gameWinner.GamePoints.PointValue = (int)GamePointEnum.FIFTEEN;
                    SetGamePointHistory( gameWinner, gameLoser, GamePointEnum.FIFTEEN );
                    break;
                }
                case 15: {
                    gameWinner.GamePoints.PointValue = (int)GamePointEnum.THIRTY;
                    SetGamePointHistory( gameWinner, gameLoser, GamePointEnum.THIRTY );
                    break;
                }
                case 30: {
                    gameWinner.GamePoints.PointValue = (int)GamePointEnum.FORTY;
                    SetGamePointHistory( gameWinner, gameLoser, GamePointEnum.FORTY );
                    break;
                }
                case 40: {
                    SetGamePointHistory( gameWinner, gameLoser, GamePointEnum.FORTY );
                    UpdateScore();
                    break;
                }
            }
        }

        public void ResetMatch() {
            MatchOver = false;
            PointHistoryList = new List<PointHistory>();
        }


        private void SetGamePointHistory( TennisPlayer winner, TennisPlayer loser, GamePointEnum point ) {
            this.PointHistoryList.Add( new PointHistory { WinnerPoint = (int)point, LoserPoint = loser.GamePoints.PointValue } );
        }

        private void ResetGamePlayers() {
            foreach (var player in Players) {
                player.WonGame = false;
                player.GamePoints.PointValue = 0;
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

            if (player1.GamePoints.PointValue == 40) {
                if (player2.GamePoints.PointValue == 40) {
                    this.Deuce = true;
                }
            }

            if (player2.GamePoints.PointValue == 40) {
                if (player1.GamePoints.PointValue == 40) {
                    this.Deuce = true;
                }
            }

            if (this.Deuce) {
                return true;
            }

            return false;
        }

        private void UpdateScore() {
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
            IsMatchOver();

            ResetGamePlayers();
            ResetGame();
        }

        public void SetPointsForWinner() {
            var winningPlayer = GetGameWinner();

            winningPlayer.SetPoints++;
        }

        private void SetSetsWonForWinner() {
            var winningPlayer = GetGameWinner();
            var losingPlayer = GetGameLoser();

            var setPointMargin = ( winningPlayer.SetPoints - losingPlayer.SetPoints );

            if (setPointMargin >= 2) {
                if (winningPlayer.SetPoints >= 6) {
                    winningPlayer.SetsWon++;

                    //reset setpoints
                    winningPlayer.SetPoints = 0;
                    losingPlayer.SetPoints = 0;
                }
            }
        }

        private void IsMatchOver() {
            var winningPlayer = GetGameWinner();
            var losingPlayer = GetGameLoser();

            var setsWonMargin = ( winningPlayer.SetsWon - losingPlayer.SetsWon );

            if (setsWonMargin >= 2) {
                MatchOver = true;
            } else {
                MatchOver = false;
            }
        }

        private bool IsAdvantagePlayerWinningGame() {
            var winningPlayer = GetGameWinner();

            if (winningPlayer.Identifier == PlayerInAdvantage) {
                return true;
            }

            foreach (var player in Players) {
                player.Advantage = false;
            }

            return false;
        }

        private void SetAdvantage() {
            var winningPlayer = GetGameWinner();

            winningPlayer.Advantage = true;
            IsInAdvantage = true;
            PlayerInAdvantage = winningPlayer.Identifier;
        }

        private void PickGameWinner() {
            foreach (var player in Players) {
                player.WonGame = false;
            }

            var gamerWinnerIndex = Tennis.RandomNumber( 0, 2 );

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
            System.Threading.Thread.Sleep( Tennis.RandomNumber( 0, GameDelay ) );
        }

        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();
        private static int RandomNumber( int min, int max ) {
            lock (syncLock) { // synchronize
                return random.Next( min, max );
            }
        }
    }

    public class TennisPlayer
    {
        public Guid Identifier { get; set; }
        public string Name { get; set; }
        public Point GamePoints { get; set; }
        public int SetPoints { get; set; }
        public bool WonGame { get; internal set; }
        public bool Advantage { get; internal set; }
        public int SetsWon { get; set; }
    }

    public class PointHistory
    {
        public int WinnerPoint { get; set; }
        public int LoserPoint { get; set; }
    }


    public class Point
    {
        public int PointValue { get; set; }
    }
}
