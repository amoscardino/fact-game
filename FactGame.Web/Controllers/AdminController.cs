using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using FactGame.Web.DataAccess.Models;
using FactGame.Web.Models;
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
                Players = game.Players
            };

            return View("AdminRegistering", vm);
        }

        private IActionResult AdminVoting(Game game)
        {
            var vm = new AdminVotingViewModel
            {
                Name = game.Name,
                GameID = game.ID,
                AdminToken = game.AdminToken
            };

            vm.Facts = game.Players
                .OrderBy(p => p.FactID)
                .Select(p => new AdminVotingFactViewModel
                {
                    Fact = p.Fact,
                    FactID = p.FactID
                })
                .ToList();

            vm.Players = game.Players
                .OrderBy(p => p.Name)
                .Select(p => new AdminVotingPlayerViewModel
                {
                    PlayerID = p.ID,
                    PlayerName = p.Name,
                    Symbol = p.Symbol,
                    ColorCode = p.Color
                })
                .ToList();

            foreach (var vmFact in vm.Facts)
            {
                vmFact.Players = game.Players
                    .OrderBy(p => p.Name)
                    .Select(p => new AdminVotingPlayerViewModel
                    {
                        PlayerID = p.ID,
                        PlayerName = p.Name,
                        Symbol = p.Symbol,
                        ColorCode = p.Color,
                        Votes = game.Players
                            .Where(q => q.Votes.Any(x => x.GuessPlayerID == p.ID && x.FactID == vmFact.FactID))
                            .Select(q => new AdminVotingVoteViewModel
                            {
                                PlayerName = q.Name,
                                Symbol = q.Symbol,
                                ColorCode = q.Color
                            })
                            .ToList()
                    })
                    .ToList();
            }

            return View("AdminVoting", vm);
        }

        private IActionResult AdminClosed(Game game)
        {
            var vm = new AdminClosedViewModel
            {
                Name = game.Name,
                GameID = game.ID,
                AdminToken = game.AdminToken
            };

            vm.Facts = game.Players
                .OrderBy(p => p.FactID)
                .Select(p => new AdminClosedFactViewModel
                {
                    Fact = p.Fact,
                    FactID = p.FactID,
                    PlayerID = p.ID
                })
                .ToList();

            vm.Players = game.Players
                .OrderBy(p => p.Name)
                .Select(p => new AdminClosedPlayerViewModel
                {
                    PlayerID = p.ID,
                    PlayerName = p.Name,
                    Symbol = p.Symbol,
                    ColorCode = p.Color,
                    Score = p.Score
                })
                .ToList();

            foreach (var vmFact in vm.Facts)
            {
                vmFact.Players = game.Players
                    .OrderBy(p => p.Name)
                    .Select(p => new AdminClosedPlayerViewModel
                    {
                        PlayerID = p.ID,
                        PlayerName = p.Name,
                        Symbol = p.Symbol,
                        ColorCode = p.Color,
                        Votes = game.Players
                            .Where(q => q.Votes.Any(x => x.GuessPlayerID == p.ID && x.FactID == vmFact.FactID))
                            .Select(q => new AdminClosedVoteViewModel
                            {
                                PlayerName = q.Name,
                                Symbol = q.Symbol,
                                ColorCode = q.Color
                            })
                            .ToList()
                    })
                    .ToList();
            }

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
            var maxScore = game.Players.Count();

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
    }
}
