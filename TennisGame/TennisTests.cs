using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TennisGameDojo;
using Xunit;
using Xunit.Abstractions;

namespace FactoryDojo
{
    public class TennisTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public TennisTests( ITestOutputHelper outputHelper ) {
            _outputHelper = outputHelper;
        }


        [Fact]
        public void A_New_Game_Should_Have_Two_Players_With_0_Points() {
            var playerLists = new List<TennisPlayer>();
            playerLists.Add( new TennisPlayer() );
            playerLists.Add( new TennisPlayer() );

            var game = new Tennis( playerLists );

            Assert.True( game.Players.Count == 2 );
            Assert.True( game.Players.Select( x => x.GamePoints.PointValue == 0 ).Count() > 0 );
        }

        [Fact]
        public void When_Game_On_Find_Point_For_Point_Winner() {
            var playerLists = new List<TennisPlayer>();
            playerLists.Add( new TennisPlayer() );
            playerLists.Add( new TennisPlayer() );

            var game = new Tennis( playerLists );

            game.Play();

            Assert.True( game.Players.Where( p => p.WonGame == true ).Count() == 1 );
            Assert.True( game.Players.Where( p => p.WonGame == false ).Count() == 1 );
        }


        [Fact]
        public void Get_Game_Point_Winner_Points() {
            var playerLists = new List<TennisPlayer>();
            playerLists.Add( new TennisPlayer() );
            playerLists.Add( new TennisPlayer() );

            var game = new Tennis( playerLists );

            game.Play();

            var winner = game.Players.Where( p => p.WonGame == true ).FirstOrDefault();

            _outputHelper.WriteLine( winner.GamePoints.PointValue.ToString() );
            Assert.True( winner.GamePoints.PointValue > 0 );
        }

        [Fact]
        public void Players_Play_And_Points_Are_Scored() {
            var playerLists = new List<TennisPlayer>();
            playerLists.Add( new TennisPlayer() { Name = "Daniel" } );
            playerLists.Add( new TennisPlayer() { Name = "Thomas" } );

            var game = new Tennis( playerLists );

            game.Play();
            game.Play();
            game.Play();
            game.Play();
            game.Play();

            foreach (var player in game.Players) {
                _outputHelper.WriteLine( player.GamePoints.PointValue.ToString() );
            }

            Assert.True( game.PointHistoryList.Count == 5 );
            Assert.True( game.PointHistoryList.Count == 5 );
        }


        [Fact]
        public void If_One_Player_Has_40_Points_And_The_Other_10_Set_A_SetPoint() {
            var playerLists = new List<TennisPlayer>();
            playerLists.Add( new TennisPlayer() );
            playerLists.Add( new TennisPlayer() );

            var game = new Tennis( playerLists );

            game.Players[0].GamePoints.PointValue = 40;
            game.Players[1].GamePoints.PointValue = 10;

            game.Players[0].WonGame = true;
            game.Players[1].WonGame = false;

            game.SetGamePointForGameWinner();
            
            foreach (var player in game.Players) {
                _outputHelper.WriteLine( player.GamePoints.PointValue.ToString() );
                _outputHelper.WriteLine( player.SetPoints.ToString() );
            }

            Assert.True( game.Players.Where( p => p.SetPoints > 0 ).FirstOrDefault() != null );
        }

        [Fact]
        public void If_Playsers_Has_40_Game_Is_Deuce() {
            var playerLists = new List<TennisPlayer>();
            playerLists.Add( new TennisPlayer() );
            playerLists.Add( new TennisPlayer() );

            var game = new Tennis( playerLists );

            game.Players[0].GamePoints.PointValue = 40;
            game.Players[1].GamePoints.PointValue = 40;

            game.Players[0].WonGame = true;
            game.Players[1].WonGame = false;

            game.SetGamePointForGameWinner();

            foreach (var player in game.Players) {
                _outputHelper.WriteLine( player.GamePoints.PointValue.ToString() );
                _outputHelper.WriteLine( player.SetPoints.ToString() );
            }

            Assert.True( game.Deuce );
        }


        [Fact]
        public void If_Game_Is_Deuce_Winning_Player_Has_Advantage() {
            var playerLists = new List<TennisPlayer>();
            playerLists.Add( new TennisPlayer() );
            playerLists.Add( new TennisPlayer() );

            var game = new Tennis( playerLists );

            game.Players[0].GamePoints.PointValue = 40;
            game.Players[1].GamePoints.PointValue = 40;

            game.Players[0].WonGame = true;
            game.Players[1].WonGame = false;

            game.SetGamePointForGameWinner();
            //deuce now

            Assert.True( game.Players[0].Advantage == true );
        }


        [Fact]
        public void If_Player_Has_Advantage_And_Wins_Game_He_Gets_Set_Point() {
            var playerLists = new List<TennisPlayer>();
            playerLists.Add( new TennisPlayer() );
            playerLists.Add( new TennisPlayer() );

            var game = new Tennis( playerLists );

            game.Players[0].GamePoints.PointValue = 40;
            game.Players[1].GamePoints.PointValue = 40;

            game.Players[0].WonGame = true;
            game.Players[1].WonGame = false;

            //deuce
            game.SetGamePointForGameWinner();

            //advantage
            game.SetGamePointForGameWinner();

            Assert.True( game.Players[0].SetPoints == 1 );
            Assert.True( game.Deuce == false );
        }

        [Fact]
        public void If_Player_Has_6_SetPoints_And_Other_Player_Has_4_SetPoints_Player_With_6_SetPoints_Wins_Set() {
            var playerLists = new List<TennisPlayer>();
            playerLists.Add( new TennisPlayer() );
            playerLists.Add( new TennisPlayer() );

            var game = new Tennis( playerLists );

            game.Players[0].SetPoints = 5;
            game.Players[1].SetPoints = 4;

            game.Players[0].GamePoints.PointValue = 40;
            game.Players[1].GamePoints.PointValue = 15;

            game.Players[0].WonGame = true;
            game.Players[1].WonGame = false;

            game.SetGamePointForGameWinner();

            Assert.True( game.Players[0].SetPoints == 0 );
            Assert.True( game.Players[0].SetsWon == 1 );
        }


        [Fact]
        public void If_Player_Has_6_SetPoints_And_Other_Player_Has_5_And_Player_With_6_Points_Wins_He_Wins_Set() {
            var playerLists = new List<TennisPlayer>();
            playerLists.Add( new TennisPlayer() );
            playerLists.Add( new TennisPlayer() );

            var game = new Tennis( playerLists );

            game.Players[0].SetPoints = 6;
            game.Players[1].SetPoints = 5;

            game.Players[0].GamePoints.PointValue = 40;
            game.Players[1].GamePoints.PointValue = 15;

            game.Players[0].WonGame = true;
            game.Players[1].WonGame = false;

            game.SetGamePointForGameWinner();

            Assert.True( game.Players[0].SetPoints == 0 );
            Assert.True( game.Players[0].SetsWon == 1 );
        }

        [Fact]
        public void If_Player_Has_4_SetPoints_And_Other_Player_Has_3_No_One_Wins_Set() {
            var playerLists = new List<TennisPlayer>();
            playerLists.Add( new TennisPlayer() );
            playerLists.Add( new TennisPlayer() );

            var game = new Tennis( playerLists );

            game.Players[0].SetPoints = 4;
            game.Players[1].SetPoints = 3;

            game.Players[0].GamePoints.PointValue = 40;
            game.Players[1].GamePoints.PointValue = 15;

            game.Players[0].WonGame = true;
            game.Players[1].WonGame = false;

            game.SetGamePointForGameWinner();

            Assert.True( game.Players[0].SetPoints == 5 );
            Assert.True( game.Players[0].SetsWon == 0 );
        }

        [Fact]
        public void If_PLayer_Has_A_Margin_Of_2_Sets_Won_Player_Wins_Match() {
            var playerLists = new List<TennisPlayer>();
            playerLists.Add( new TennisPlayer() );
            playerLists.Add( new TennisPlayer() );

            var game = new Tennis( playerLists );

            game.Players[0].SetPoints = 5;
            game.Players[1].SetPoints = 3;

            game.Players[0].SetsWon = 2;
            game.Players[1].SetsWon = 1;

            game.Players[0].GamePoints.PointValue = 40;
            game.Players[1].GamePoints.PointValue = 15;

            game.Players[0].WonGame = true;
            game.Players[1].WonGame = false;

            game.SetGamePointForGameWinner();

            Assert.True( game.Players[0].SetsWon == 3 );
            Assert.True( game.MatchOver );
        }

        [Fact]
        public void Play_Until_A_Player_Wins_Match() {
            var playerLists = new List<TennisPlayer>();
            playerLists.Add( new TennisPlayer() );
            playerLists.Add( new TennisPlayer() );

            var game = new Tennis( playerLists );

            while (!game.MatchOver) {
                game.Play();
            }

            foreach (var item in game.PointHistoryList) {
                _outputHelper.WriteLine(item.WinnerPoint + " - " + item.LoserPoint );
            }
        }
    }
}
