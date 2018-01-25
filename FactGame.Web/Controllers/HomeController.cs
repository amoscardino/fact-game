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

namespace FactGame.Web.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IHostingEnvironment hostingEnvironment)
            : base(hostingEnvironment) { }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateGame(CreateGameViewModel model)
        {
            var sql = @"insert into Game (ID, AdminToken, Name, Status) values (@ID, @AdminToken, @Name, @Status)";

            using (var conn = await GetDatabaseConnection())
            {
                conn.Open();

                var game = new Game
                {
                    ID = GetNewID(),
                    AdminToken = GetNewID(),
                    Name = model.Name,
                    Status = 0
                };

                await conn.ExecuteAsync(sql, game);

                return RedirectToAction("Index", "Admin", new { game.ID, game.AdminToken });
            }
        }
    }
}
