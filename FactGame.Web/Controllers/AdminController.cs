using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using FactGame.Web.DataAccess.Models;
using FactGame.Web.ViewModels;
using System.Data;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using FactGame.Web.DataAccess;

namespace FactGame.Web.Controllers
{
    public class AdminController : Controller
    {
        #region Private Members
        private IFactGameRepository _repo;
        #endregion

        #region Constructor
        public AdminController(IFactGameRepository repo)
        {
            _repo = repo;
        }
        #endregion

        #region Action: Index
        [HttpGet, Route("/game/{id}/admin/{adminToken}")]
        public async Task<IActionResult> Index(string id, string adminToken)
        {
            var game = await _repo.GetGame(id);

            if (game == null || game.AdminToken != adminToken)
                return NotFound("Game not found.");

            switch (game.Status)
            {
                case 0:
                    return AdminRegistering(game);
                case 1:
                    return AdminVoting(game);
                case 2:
                    return AdminClosed(game);
                default:
                    throw new ArgumentOutOfRangeException("Unknown game status");
            }
        }

        private IActionResult AdminRegistering(Game game)
        {
            var vm = new AdminRegisteringViewModel
            {
                Name = game.Name,
                GameID = game.ID,
                AdminToken = game.AdminToken,
                PlayerList = new PlayerListViewModel(game, true, false)
            };

            return View("AdminRegistering", vm);
        }

        private IActionResult AdminVoting(Game game)
        {
            var vm = new AdminVotingViewModel
            {
                Name = game.Name,
                GameID = game.ID,
                AdminToken = game.AdminToken,
                VoteGrid = new AdminVoteGridViewModel(game, false)
            };
            
            return View("AdminVoting", vm);
        }

        private IActionResult AdminClosed(Game game)
        {
            var vm = new AdminClosedViewModel
            {
                Name = game.Name,
                GameID = game.ID,
                AdminToken = game.AdminToken,
                PlayerList = new PlayerListViewModel(game, false, true),
                VoteGrid = new AdminVoteGridViewModel(game, true)
            };

            return View("AdminClosed", vm);
        }
        #endregion

        #region Action: Change Status
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(string id, string adminToken, int newStatus)
        {
            var game = await _repo.GetGame(id);

            // Score the game if we are closing it
            if (newStatus == 2)
                ScoreGame(game);

            game.Status = newStatus;

            await _repo.UpdateGame(game);

            return RedirectToAction("Index", "Admin", new { id, adminToken });
        }

        private void ScoreGame(Game game)
        {
            var maxScore = (decimal)game.Players.Count();

            // Reset any old scores first
            foreach (var player in game.Players)
                player.Score = 0;

            foreach (var player in game.Players)
            {
                var correctPlayers = game.Players
                        .Where(x => x.Votes.Any(y => player.ID == y.GuessPlayerID && player.FactID == y.FactID))
                        .ToList();

                if (correctPlayers.Any())
                {
                    foreach (var correctPlayer in correctPlayers)
                        correctPlayer.Score += maxScore / correctPlayers.Count;
                }
                else
                    player.Score += maxScore;
            }
        }
        #endregion

        #region Action: Remove Player
        [HttpPost]
        public async Task<IActionResult> RemovePlayer(string id, string adminToken, string playerId)
        {
            var game = await _repo.GetGame(id);

            var player = game.Players.SingleOrDefault(p => p.ID == playerId);

            if (player != null)
            {
                game.Players.Remove(player);

                await _repo.UpdateGame(game);
            }

            return RedirectToAction("Index", "Admin", new { id, adminToken });
        }
        #endregion
    }
}
