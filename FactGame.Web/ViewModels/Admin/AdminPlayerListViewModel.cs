﻿using FactGame.Web.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactGame.Web.ViewModels
{
    public class AdminPlayerListViewModel
    {
        public string GameID { get; set; }

        public string AdminToken { get; set; }

        public bool ShowScore { get; set; }

        public bool AllowRemoving { get; set; }

        public List<AdminPlayerListPlayerViewModel> Players { get; private set; }

        public AdminPlayerListViewModel()
        {
            Players = new List<AdminPlayerListPlayerViewModel>();
        }

        public AdminPlayerListViewModel(Game game, bool allowRemoving, bool showScore)
        {
            AllowRemoving = allowRemoving;
            ShowScore = showScore;
            GameID = game.ID;
            AdminToken = game.AdminToken;
            Players = game.Players
                .OrderByDescending(p => p.Score)
                .Select(p => new AdminPlayerListPlayerViewModel
                {
                    ID = p.ID,
                    Name = p.Name,
                    Symbol = p.Symbol,
                    Color = p.Color,
                    Score = p.Score
                })
                .ToList();
        }
    }

    public class AdminPlayerListPlayerViewModel
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public string Color { get; set; }

        public decimal Score { get; set; }
    }
}
