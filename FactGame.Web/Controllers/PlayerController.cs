using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using FactGame.Web.DataModels;
using FactGame.Web.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using System.Data;

namespace FactGame.Web.Controllers
{
    public class PlayerController : BaseController
    {
        #region Constructor
        public PlayerController(IHostingEnvironment hostingEnvironment)
            : base(hostingEnvironment) { }
        #endregion

        #region Action: Index
        [Route("/game/{id}")]
        public async Task<IActionResult> Index(string id)
        {
            var gameSql = @"select * from Game where ID = @ID";

            using (var conn = await GetDatabaseConnection())
            {
                conn.Open();

                var game = await conn.QuerySingleOrDefaultAsync<Game>(gameSql, new { ID = id });

                if (game == null)
                    return NotFound("Game not found.");

                switch (game.Status)
                {
                    case 0:
                        return await PlayerRegistering(conn, game);
                    case 1:
                        return await PlayerVoting(conn, game);
                    case 2:
                        return await PlayerClosed(conn, game);
                    default:
                        throw new ArgumentOutOfRangeException("Unknown game status");
                }
            }
        }

        private async Task<IActionResult> PlayerRegistering(IDbConnection conn, Game game)
        {
            var vm = new PlayerRegisteringViewModel
            {
                GameID = game.ID,
                GameName = game.Name
            };

            var hasCookie = Request.Cookies.ContainsKey("FactGameGame" + game.ID);

            if (hasCookie)
            {
                var playerId = Request.Cookies["FactGameGame" + game.ID];

                var playerSql = @"select * from Player where ID = @ID and GameID = @GameID";
                var player = await conn.QuerySingleOrDefaultAsync<Player>(playerSql, new { ID = playerId, GameID = game.ID });

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

        private async Task<IActionResult> PlayerVoting(IDbConnection conn, Game game)
        {
            var vm = new PlayerVotingViewModel
            {
                GameID = game.ID,
                GameName = game.Name
            };

            var hasCookie = Request.Cookies.ContainsKey("FactGameGame" + game.ID);

            if (!hasCookie)
                return View("PlayerVotingNotRegistered", vm);

            var playersSql = @"select * from Player where GameID = @GameID";

            var playerId = Request.Cookies["FactGameGame" + game.ID];
            var players = await conn.QueryAsync<Player>(playersSql, new { GameID = game.ID });
            var player = players.SingleOrDefault(p => p.ID == playerId);

            if (player == null)
                return View("PlayerVotingNotRegistered", vm);

            vm.PlayerID = playerId;
            vm.Players = players
                    .OrderBy(p => p.Name)
                    .Select(p => new SelectListItem { Value = p.ID, Text = p.Name })
                    .ToList();
            vm.Votes = players
                    .OrderBy(p => p.FactID)
                    .Select(p => new PlayerVoteViewModel { Fact = p.Fact, FactID = p.FactID })
                    .ToList();

            var voteSql = @"select * from Vote where VoterPlayerID = @PlayerID";
            var existingVotes = (await conn.QueryAsync<Vote>(voteSql, new { PlayerID = playerId })).ToList();

            foreach (var existingVote in existingVotes)
            {
                var vmVote = vm.Votes.SingleOrDefault(v => v.FactID == existingVote.FactID);

                if (vmVote != null)
                    vmVote.GuessPlayerID = existingVote.GuessPlayerID;
            }

            return View("PlayerVoting", vm);
        }

        private async Task<IActionResult> PlayerClosed(IDbConnection conn, Game game)
        {
            var vm = new PlayerClosedViewModel
            {
                GameName = game.Name
            };
            var hasCookie = Request.Cookies.ContainsKey("FactGameGame" + game.ID);

            if (!hasCookie)
                return View("PlayerClosedNotRegistered", vm);
            
            var playerId = Request.Cookies["FactGameGame" + game.ID];

            var playersSql = @"select * from Player where GameID = @GameID";
            var votesSql = @"select * from Vote where VoterPlayerID = @PlayerID";
            var otherVotesSql = @"select * from Vote where GuessPlayerID = @PlayerID and FactID = @FactID";

            var players = await conn.QueryAsync<Player>(playersSql, new { GameID = game.ID });
            var player = players.Single(p => p.ID == playerId);
            var votes = await conn.QueryAsync<Vote>(votesSql, new { PlayerID = player.ID });
            var otherVotes = await conn.QueryAsync<Vote>(otherVotesSql, new { PlayerID = player.ID, FactID = player.FactID });

            vm.Score = player.Score;
            vm.Fact = player.Fact;
            vm.FactGuessedByPlayerNames = (from v in otherVotes
                                           join p in players on v.VoterPlayerID equals p.ID
                                           select p.Name).ToList();

            vm.Votes = (from v in votes
                        join gp in players on v.GuessPlayerID equals gp.ID
                        join ap in players on v.FactID equals ap.FactID
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

            var insertSql = @"insert into Player (ID, GameID, Name, Symbol, Color, Fact, FactID) values (@ID, @GameID, @Name, @Symbol, @Color, @Fact, @FactID)";
            var updateSql = @"update Player set Name = @Name, Symbol = @Symbol, Color = @Color, Fact = @Fact, FactID = @FactID where ID = @ID";

            var isNewPlayer = string.IsNullOrWhiteSpace(model.PlayerID);

            var player = new Player
            {
                ID = isNewPlayer ? GetNewID() : model.PlayerID,
                GameID = model.GameID,
                Name = model.Name,
                Symbol = model.Symbol,
                Color = model.ColorCode,
                Fact = model.Fact,
                FactID = GetNewID()
            };

            using (var conn = await GetDatabaseConnection())
            {
                conn.Open();

                await conn.ExecuteAsync(isNewPlayer ? insertSql : updateSql, player);
            }

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

            return RedirectToAction("Index", "Player", new { id = model.GameID });
        }
        #endregion

        #region Action: Vote
        public async Task<IActionResult> Vote(PlayerVotingViewModel model)
        {
            var deleteSql = @"delete from Vote where VoterPlayerID = @PlayerID";
            var insertSql = @"insert into Vote (VoterPlayerID, FactID, GuessPlayerID) values (@VoterPlayerID, @FactID, @GuessPlayerID)";

            using (var conn = await GetDatabaseConnection())
            {
                await conn.ExecuteAsync(deleteSql, new { model.PlayerID });

                foreach (var vote in model.Votes)
                {
                    if (string.IsNullOrWhiteSpace(vote.GuessPlayerID))
                        continue;

                    var newVote = new Vote
                    {
                        VoterPlayerID = model.PlayerID,
                        FactID = vote.FactID,
                        GuessPlayerID = vote.GuessPlayerID
                    };

                    await conn.ExecuteAsync(insertSql, newVote);
                }
            }

            return RedirectToAction("Index", "Player", new { id = model.GameID });
        }
        #endregion
    }
}
