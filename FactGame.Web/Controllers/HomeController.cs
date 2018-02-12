using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using FactGame.Web.DataAccess.Models;
using FactGame.Web.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using FactGame.Web.DataAccess;

namespace FactGame.Web.Controllers
{
    public class HomeController : Controller
    {
        #region Private Members
        private IFactGameRepository _repo;
        #endregion

        #region Constructor
        public HomeController(IFactGameRepository repo)
        {
            _repo = repo;
        }
        #endregion

        #region Public Actions
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateGame(CreateGameViewModel model)
        {
            var game = new Game
            {
                ID = _repo.GetNewId(),
                Name = model.Name,
                AdminToken = _repo.GetNewId()
            };

            await _repo.UpdateGame(game);

            return RedirectToAction("Index", "Admin", new { game.ID, game.AdminToken });
        }
        #endregion
    }
}
