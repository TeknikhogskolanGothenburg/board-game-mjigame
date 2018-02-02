using GameEngine;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
namespace WebApp.Controllers
{
    public class TicTacToeController : Controller
    {
        private static List<TicTacToe> Games;
        private static TicTacToe ticTacToeGame;

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult PreGame(string userName, string email)
        {
            Log.Information("User with username: " + userName + " and email: " + email);
            
            if (ticTacToeGame == null)
            {
                ticTacToeGame = new TicTacToe();
                Games.Add(ticTacToeGame);
            }

            try
            {
                ticTacToeGame.JoinGame(userName, GetSessionID(), email);
            }

            catch
            {
                Log.Error("Spelaren försökte joina ett game som var fullt");
                ViewBag.GameIsFullMessage = "Game is full, please try later";
                return View( "Login" );
            }

            return RedirectToAction("Game", "TicTacToe");
        }

        public ActionResult Game(string fieldID)
        {
            try
            {
                ticTacToeGame.SetDisplayName(Session.SessionID);
            }

            catch(Exception e)
            {
                Log.Error(e.ToString());
                return View("Login");
            }
         

            if (fieldID == null || ticTacToeGame.Players.Count < 2 || ticTacToeGame.ActivePlayer.ID != Session.SessionID)
            {
                return View(ticTacToeGame);
            }

            if (ticTacToeGame == null)
            {
                return View("Login");
            }

            else
            {
                try
                {
                    ticTacToeGame.MakeMove(fieldID);
                }
                catch
                {
                    return View(ticTacToeGame);
                }

                if (ticTacToeGame.CheckIfGameIsOver(ticTacToeGame.ActivePlayer))
                {
                    ticTacToeGame.ResetGameBoard();
                    ticTacToeGame.TogglePlayer();
                    return View("GameOver", ticTacToeGame);
                }

                ticTacToeGame.TogglePlayer();
                return View(ticTacToeGame);
            }

         

        }

        public ActionResult GameOver()
        {
            return View();
        }

        private string GetSessionID()
        {
            Session["what"] = "ever";
            string sessionID = Session.SessionID;
            return sessionID;
        }
    }
}