using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using FactGame.Web.DataModels;
using FactGame.Web.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;

namespace FactGame.Web.Controllers
{
    public class HomeController : BaseController
    {
        public HomeController(IConfiguration config) : base(config) { }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateGame(CreateGameViewModel model)
        {
            var game = new Game
            {
                ID = ObjectId.GenerateNewId(),
                Name = model.Name,
                AdminToken = ObjectId.GenerateNewId()
            };

            await UpdateGameAsync(game);

            return RedirectToAction("Index", "Admin", new { game.ID, game.AdminToken });
        }
    }
}
