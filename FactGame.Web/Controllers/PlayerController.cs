using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FactGame.Web.DataModels;
using FactGame.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;

namespace FactGame.Web.Controllers
{
    public class PlayerController : BaseController
    {
        #region Constructor
        public PlayerController(IConfiguration config) : base(config) { }
        #endregion

        #region Action: Index
        [Route("/game/{id}")]
        public async Task<IActionResult> Index(string id)
        {
            var game = await GetGameAsync(id);

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

                var player = game.Players.SingleOrDefault(p => p.ID.ToString() == playerId);

                if (player != null)
                {
                    vm.PlayerID = player.ID.ToString();
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
            var player = game.Players.SingleOrDefault(p => p.ID.ToString() == playerId);

            if (player == null)
                return View("PlayerVotingNotRegistered", vm);

            vm.PlayerID = playerId;

            vm.Players = game.Players
                    .OrderBy(p => p.Name)
                    .Select(p => new SelectListItem { Value = p.ID.ToString(), Text = p.Name })
                    .ToList();

            vm.Votes = game.Players
                    .OrderBy(p => p.FactID)
                    .Select(p => new PlayerVoteViewModel
                    {
                        Fact = p.Fact,
                        FactID = p.FactID.ToString(),
                        GuessPlayerID = player.Votes
                            .Where(x => x.FactID == p.FactID)
                            .Select(x => x.GuessPlayerID.ToString())
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
            var player = game.Players.SingleOrDefault(p => p.ID.ToString() == playerId);

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
                            GuessPlayerName = gp.Name,
                            ActualPlayerName = ap.Name,
                            Correct = ap.ID == gp.ID
                        }).ToList();

            return View("PlayerClosed", vm);
        }
        #endregion

        #region Action: Register
        public async Task<IActionResult> Register(PlayerRegisteringViewModel model)
        {
            if (!ModelState.IsValid)
                return View("PlayerRegistering", model);

            var game = await GetGameAsync(model.GameID);

            var player = game.Players.SingleOrDefault(p => p.ID.ToString() == model.PlayerID);

            if (player == null)
            {
                player = new Player { ID = ObjectId.GenerateNewId() };
                game.Players.Add(player);
            }

            player.Name = model.Name;
            player.Symbol = model.Symbol;
            player.Color = model.ColorCode;
            player.Fact = model.Fact;
            player.FactID = ObjectId.GenerateNewId();

            await UpdateGameAsync(game);

            var hasCookie = Request.Cookies.ContainsKey("FactGameGame" + model.GameID);

            if (!hasCookie)
            {
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTime.Now.AddYears(1),
                    Secure = Request.IsHttps
                };

                Response.Cookies.Append("FactGameGame" + model.GameID, player.ID.ToString(), cookieOptions);
            }

            return RedirectToAction("Index", "Player", new { id = model.GameID });
        }
        #endregion

        #region Action: Vote
        public async Task<IActionResult> Vote(PlayerVotingViewModel model)
        {
            var game = await GetGameAsync(model.GameID);
            var player = game.Players.SingleOrDefault(x => x.ID.ToString() == model.PlayerID);

            if (player == null)
                throw new InvalidOperationException("Invalid Player ID");

            player.Votes = model.Votes
                .Where(x => !string.IsNullOrWhiteSpace(x.GuessPlayerID))
                .Select(x => new Vote
                {
                    FactID = ObjectId.Parse(x.FactID),
                    GuessPlayerID = ObjectId.Parse(x.GuessPlayerID)
                })
                .ToList();

            await UpdateGameAsync(game);

            return RedirectToAction("Index", "Player", new { id = model.GameID });
        }
        #endregion
    }
}
