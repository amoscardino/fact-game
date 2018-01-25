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
using Microsoft.Data.Sqlite;

namespace FactGame.Web.Controllers
{
    public class AdminController : BaseController
    {
        #region Constructor
        public AdminController(IHostingEnvironment hostingEnvironment)
            : base(hostingEnvironment) { }
        #endregion

        #region Action: Index
        [HttpGet, Route("/game/{id}/admin/{adminToken}")]
        public async Task<IActionResult> Index(string id, string adminToken)
        {
            var gameSql = @"select * from Game where ID = @ID and AdminToken = @AdminToken";

            using (var conn = await GetDatabaseConnection())
            {
                conn.Open();

                var game = await conn.QuerySingleOrDefaultAsync<Game>(gameSql, new { ID = id, AdminToken = adminToken });

                if (game == null)
                    return NotFound("Game not found.");

                switch (game.Status)
                {
                    case 0:
                        return await AdminRegistering(conn, game);
                    case 1:
                        return await AdminVoting(conn, game);
                    case 2:
                        return await AdminClosed(conn, game);
                    default:
                        throw new ArgumentOutOfRangeException("Unknown game status");
                }
            }
        }

        private async Task<IActionResult> AdminRegistering(SqliteConnection conn, Game game)
        {
            var sql = @"select * from Player where GameID = @GameID order by ID";

            var vm = new AdminRegisteringViewModel
            {
                Name = game.Name,
                GameID = game.ID,
                AdminToken = game.AdminToken
            };

            vm.Players = await conn.QueryAsync<Player>(sql, new { vm.GameID });

            return View("AdminRegistering", vm);
        }

        private async Task<IActionResult> AdminVoting(SqliteConnection conn, Game game)
        {
            var playersSql = @"select * from Player where GameID = @GameID order by ID";
            var votesSql = @"select p.Name as PlayerName, p.Symbol as Symbol, p.Color as ColorCode
                             from Vote v
                             join Player p on v.VoterPlayerID = p.ID
                             where v.GuessPlayerID = @PlayerID and v.FactID = @FactID";

            var vm = new AdminVotingViewModel
            {
                Name = game.Name,
                GameID = game.ID,
                AdminToken = game.AdminToken
            };

            var players = await conn.QueryAsync<Player>(playersSql, new { vm.GameID });

            vm.Facts = players
                .OrderBy(p => p.FactID)
                .Select(p => new AdminVotingFactViewModel
                {
                    Fact = p.Fact,
                    FactID = p.FactID
                })
                .ToList();

            vm.Players = players
                .OrderBy(p => p.Name)
                .Select(p => new AdminVotingPlayerViewModel
                {
                    PlayerName = p.Name,
                    Symbol = p.Symbol,
                    ColorCode = p.Color
                })
                .ToList();

            foreach (var vmFact in vm.Facts)
            {
                vmFact.Players = players
                    .OrderBy(p => p.Name)
                    .Select(p => new AdminVotingPlayerViewModel
                    {
                        PlayerID = p.ID,
                        PlayerName = p.Name,
                        Symbol = p.Symbol,
                        ColorCode = p.Color
                    })
                    .ToList();

                foreach (var vmPlayer in vmFact.Players)
                {
                    var parameters = new
                    {
                        vmPlayer.PlayerID,
                        vmFact.FactID
                    };

                    vmPlayer.Votes = (await conn.QueryAsync<AdminVotingVoteViewModel>(votesSql, parameters)).ToList();
                }
            }

            return View("AdminVoting", vm);
        }

        private async Task<IActionResult> AdminClosed(SqliteConnection conn, Game game)
        {
            var playersSql = @"select * from Player where GameID = @ID order by ID";
            var votesSql = @"select p.Name as PlayerName, p.Symbol as Symbol, p.Color as ColorCode
                             from Vote v
                             join Player p on v.VoterPlayerID = p.ID
                             where v.GuessPlayerID = @PlayerID and v.FactID = @FactID";

            var vm = new AdminClosedViewModel
            {
                Name = game.Name,
                GameID = game.ID,
                AdminToken = game.AdminToken
            };

            var players = await conn.QueryAsync<Player>(playersSql, new { game.ID });

            vm.Facts = players
                .OrderBy(p => p.FactID)
                .Select(p => new AdminClosedFactViewModel
                {
                    Fact = p.Fact,
                    FactID = p.FactID,
                    PlayerID = p.ID
                })
                .ToList();

            vm.Players = players
                .OrderBy(p => p.Name)
                .Select(p => new AdminClosedPlayerViewModel
                {
                    PlayerName = p.Name,
                    Symbol = p.Symbol,
                    ColorCode = p.Color,
                    Score = p.Score
                })
                .ToList();

            foreach (var vmFact in vm.Facts)
            {
                vmFact.Players = players
                    .OrderBy(p => p.Name)
                    .Select(p => new AdminClosedPlayerViewModel
                    {
                        PlayerID = p.ID,
                        PlayerName = p.Name,
                        Symbol = p.Symbol,
                        ColorCode = p.Color
                    })
                    .ToList();

                foreach (var vmPlayer in vmFact.Players)
                {
                    var parameters = new
                    {
                        vmPlayer.PlayerID,
                        vmFact.FactID
                    };

                    vmPlayer.Votes = (await conn.QueryAsync<AdminClosedVoteViewModel>(votesSql, parameters)).ToList();
                }
            }

            return View("AdminClosed", vm);
        }
        #endregion

        #region Action: Change Status
        [HttpPost]
        public async Task<IActionResult> ChangeStatus(string id, string adminToken, int newStatus)
        {
            var sql = @"update Game set Status = @Status where ID = @ID and AdminToken = @AdminToken";

            using (var conn = await GetDatabaseConnection())
            {
                conn.Open();

                // Score the game if we are closing it
                if (newStatus == 2)
                    await ScoreGame(conn, id);

                await conn.ExecuteAsync(sql, new
                {
                    Status = newStatus,
                    ID = id,
                    AdminToken = adminToken
                });

                return RedirectToAction("Index", "Admin", new { id, adminToken });
            }
        }

        private async Task ScoreGame(SqliteConnection conn, string id)
        {
            var playersSql = @"select * from Player where GameID = @id";
            var votesSql = @"select * from Vote where VoterPlayerID in @PlayerIDs";
            var playerUpdateSql = @"update Player set Score = @Score where ID = @ID";

            var players = await conn.QueryAsync<Player>(playersSql, new { id });
            var votes = await conn.QueryAsync<Vote>(votesSql, new { PlayerIDs = players.Select(p => p.ID) });

            var maxScore = players.Count();

            // Reset any old scores
            foreach (var player in players)
                player.Score = 0;

            // Calculate scores
            foreach (var player in players)
            {
                var correctPlayers = (from vote in votes
                                      join play in players on vote.VoterPlayerID equals play.ID
                                      where vote.GuessPlayerID == player.ID
                                      where vote.FactID == player.FactID
                                      select play).ToList();

                if (correctPlayers.Any())
                {
                    foreach (var correctPlayer in correctPlayers)
                        correctPlayer.Score += maxScore / correctPlayers.Count;
                }
                else
                    player.Score += maxScore;
            }

            // Save scores to DB
            foreach (var player in players)
                await conn.ExecuteAsync(playerUpdateSql, new { player.Score, player.ID });
        }
        #endregion
    }
}
