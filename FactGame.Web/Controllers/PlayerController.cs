using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FactGame.Web.DataAccess.Models;
using FactGame.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using FactGame.Web.DataAccess;

namespace FactGame.Web.Controllers
{
    public class PlayerController : Controller
    {
        #region Private Members
        private IFactGameRepository _repo;
        #endregion

        #region Constructor
        public PlayerController(IFactGameRepository repo)
        {
            _repo = repo;
        }
        #endregion

        #region Action: Index
        [Route("/game/{id}")]
        public async Task<IActionResult> Index(string id)
        {
            var game = await _repo.GetGame(id);

            if (game == null)
                return NotFound("Game not found.");

            switch (game.Status)
            {
                case 0:
                    return PlayerRegistering(game);
                case 1:
                    return PlayerVoting(game);
                case 2:
                    return PlayerClosed(game);
                default:
                    throw new ArgumentOutOfRangeException("Unknown game status");
            }
        }

        private IActionResult PlayerRegistering(Game game)
        {
            var vm = new PlayerRegisteringViewModel
            {
                GameID = game.ID.ToString(),
                GameName = game.Name
            };

            var hasCookie = Request.Cookies.ContainsKey("FactGameGame" + game.ID);

            if (hasCookie)
            {
                var playerId = Request.Cookies["FactGameGame" + game.ID];

                var player = game.Players.SingleOrDefault(p => p.ID == playerId);

                if (player != null)
                {
                    vm.PlayerID = player.ID;
                    vm.Name = player.Name;
                    vm.Symbol = player.Symbol;
                    vm.ColorCode = player.Color;
                    vm.Fact = player.Fact;
                }
            }

            return View("PlayerRegistering", vm);
        }

        private IActionResult PlayerVoting(Game game)
        {
            var vm = new PlayerVotingViewModel
            {
                GameID = game.ID.ToString(),
                GameName = game.Name
            };

            var hasCookie = Request.Cookies.ContainsKey("FactGameGame" + game.ID);

            if (!hasCookie)
                return View("PlayerVotingNotRegistered", vm);

            var playerId = Request.Cookies["FactGameGame" + game.ID];
            var player = game.Players.SingleOrDefault(p => p.ID == playerId);

            if (player == null)
                return View("PlayerVotingNotRegistered", vm);

            vm.PlayerID = playerId;

            vm.Players = game.Players
                    .OrderBy(p => p.Name)
                    .Select(p => new SelectListItem { Value = p.ID, Text = p.Name })
                    .ToList();

            vm.Votes = game.Players
                    .OrderBy(p => p.FactID)
                    .Select(p => new PlayerVoteViewModel
                    {
                        Fact = p.Fact,
                        FactID = p.FactID,
                        GuessPlayerID = player.Votes
                            .Where(x => x.FactID == p.FactID)
                            .Select(x => x.GuessPlayerID)
                            .SingleOrDefault()
                    })
                    .ToList();

            return View("PlayerVoting", vm);
        }

        private IActionResult PlayerClosed(Game game)
        {
            var vm = new PlayerClosedViewModel
            {
                GameName = game.Name
            };

            var hasCookie = Request.Cookies.ContainsKey("FactGameGame" + game.ID);

            if (!hasCookie)
                return View("PlayerClosedNotRegistered", vm);

            var playerId = Request.Cookies["FactGameGame" + game.ID];
            var player = game.Players.SingleOrDefault(p => p.ID == playerId);

            vm.Score = player.Score;
            vm.Fact = player.Fact;
            vm.FactGuessedByPlayerNames = game.Players
                    .Where(x => x.Votes.Any(y => player.FactID == y.FactID && player.ID == y.GuessPlayerID))
                    .Select(x => x.Name)
                    .ToList();

            vm.Votes = (from v in player.Votes
                        join gp in game.Players on v.GuessPlayerID equals gp.ID
                        join ap in game.Players on v.FactID equals ap.FactID
                        orderby ap.FactID
                        select new PlayerClosedVoteViewModel
                        {
                            Fact = ap.Fact,
                            FactID = ap.FactID,
                            GuessPlayerName = gp.Name,
                            ActualPlayerName = ap.Name,
                            Correct = ap.ID == gp.ID
                        }).ToList();

            // Add back any missing votes
            vm.Votes.AddRange(from p in game.Players
                              where !vm.Votes.Select(x => x.FactID).Contains(p.FactID)
                              select new PlayerClosedVoteViewModel
                              {
                                  Fact = p.Fact,
                                  FactID = p.FactID,
                                  ActualPlayerName = p.Name,
                                  GuessPlayerName = "(no guess)",
                                  Correct = false
                              });

            vm.PlayerList = new PlayerListViewModel(game, false, true, playerId);

            return View("PlayerClosed", vm);
        }
        #endregion

        #region Action: Register
        public async Task<IActionResult> Register(PlayerRegisteringViewModel model)
        {
            if (!ModelState.IsValid)
                return View("PlayerRegistering", model);

            var game = await _repo.GetGame(model.GameID);

            var player = game.Players.SingleOrDefault(p => p.ID == model.PlayerID);

            if (player == null)
            {
                player = new Player { ID = _repo.GetNewId() };
                game.Players.Add(player);
            }

            player.Name = model.Name;
            player.Symbol = model.Symbol;
            player.Color = model.ColorCode;
            player.Fact = model.Fact;
            player.FactID = _repo.GetNewId();

            await _repo.UpdateGame(game);

            var hasCookie = Request.Cookies.ContainsKey("FactGameGame" + model.GameID);

            if (!hasCookie)
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.Now.AddYears(1),
                    Secure = Request.IsHttps
                };

                Response.Cookies.Append("FactGameGame" + model.GameID, player.ID, cookieOptions);
            }

            TempData["fg-player-registered"] = true;

            return RedirectToAction("Index", "Player", new { id = model.GameID });
        }
        #endregion

        #region Action: Vote
        public async Task<IActionResult> Vote(PlayerVotingViewModel model)
        {
            var game = await _repo.GetGame(model.GameID);
            var player = game.Players.SingleOrDefault(x => x.ID == model.PlayerID);

            if (player == null)
                throw new InvalidOperationException("Invalid Player ID");

            player.Votes = model.Votes
                .Where(x => !string.IsNullOrWhiteSpace(x.GuessPlayerID))
                .Select(x => new Vote
                {
                    FactID = x.FactID,
                    GuessPlayerID = x.GuessPlayerID
                })
                .ToList();

            await _repo.UpdateGame(game);

            TempData["fg-player-voted"] = true;

            return RedirectToAction("Index", "Player", new { id = model.GameID });
        }
        #endregion
    }
}
