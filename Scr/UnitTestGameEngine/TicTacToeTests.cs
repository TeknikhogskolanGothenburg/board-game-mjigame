﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GameEngine;

namespace UnitTestGameEngine
{
    [TestClass]
    public class TicTacToeTests
    {
        [TestMethod]
        public void CheckWinner_2Xs_NoWinner()
        {
            // Arrage
            GameBoard bg = new GameBoard();
            bg.Fields[0] = "X.png";
            bg.Fields[1] = "X.png";
            TicTacToe ttt = new TicTacToe();
            ttt.GameBoard = bg;
            ttt.Players.Add(new Player()
            {
                Name = "player1"
            });

            // Act
            //var result = ttt.CheckWinner();

            // Assert
            //Assert.AreEqual("waiting for winner", result, true);
        }

        [TestMethod]
        public void CheckWinner_3Xs_XWins()
        {
            // Arrage
            GameBoard bg = new GameBoard();
            bg.Fields[0] = "X.png";
            bg.Fields[1] = "X.png";
            bg.Fields[2] = "X.png";
            TicTacToe ttt = new TicTacToe();
            ttt.GameBoard = bg;
            ttt.Players.Add(new Player()
            {
                Name = "player1"
            });

        }


      [TestMethod]
        public void CheckWinner_3Xs_OWins()
        {
            // Arrage
            GameBoard bg = new GameBoard();
            bg.Fields[0] = "O.png";
            bg.Fields[1] = "O.png";
            bg.Fields[2] = "O.png";
            TicTacToe ttt = new TicTacToe();
            ttt.GameBoard = bg;
            ttt.Players.Add(new Player()
            {
                Name = "player1"
            });
            // Act
            //var result = ttt.CheckWinner();

            // Assert
            //Assert.AreEqual("The winner is player1", result, true);
        }
    }
}
