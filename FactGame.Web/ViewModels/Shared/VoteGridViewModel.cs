﻿using FactGame.Web.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactGame.Web.ViewModels
{
    public class VoteGridViewModel
    {
        public bool ShowAnswers { get; set; }

        public List<VoteGridPlayerViewModel> Players { get; set; }

        public List<VoteGridFactViewModel> Facts { get; set; }

        public VoteGridViewModel()
        {
            Players = new List<VoteGridPlayerViewModel>();
            Facts = new List<VoteGridFactViewModel>();
        }

        public VoteGridViewModel(Game game, bool showAnswers)
        {
            ShowAnswers = showAnswers;

            Facts = game.Players
                .OrderBy(p => p.FactID)
                .Select(p => new VoteGridFactViewModel
                {
                    Fact = p.Fact,
                    FactID = p.FactID,
                    PlayerID = p.ID
                })
                .ToList();

            Players = game.Players
                .OrderBy(p => p.Name)
                .Select(p => new VoteGridPlayerViewModel
                {
                    PlayerID = p.ID,
                    PlayerName = p.Name,
                    Symbol = p.Symbol,
                    ColorCode = p.Color,
                    Score = p.Score
                })
                .ToList();

            foreach (var fact in Facts)
            {
                fact.Players = game.Players
                    .OrderBy(p => p.Name)
                    .Select(p => new VoteGridPlayerViewModel
                    {
                        PlayerID = p.ID,
                        PlayerName = p.Name,
                        Symbol = p.Symbol,
                        ColorCode = p.Color,
                        Votes = game.Players
                            .OrderBy(q => q.Name)
                            .Where(q => q.Votes.Any(x => x.GuessPlayerID == p.ID && x.FactID == fact.FactID))
                            .Select(q => new VoteGridVoteViewModel
                            {
                                PlayerName = q.Name,
                                Symbol = q.Symbol,
                                ColorCode = q.Color
                            })
                            .ToList()
                    })
                    .ToList();
            }
        }
    }

    public class VoteGridFactViewModel
    {
        public string PlayerID { get; set; }

        public string FactID { get; set; }

        public string Fact { get; set; }

        public List<VoteGridPlayerViewModel> Players { get; set; }

        public VoteGridFactViewModel()
        {
            Players = new List<VoteGridPlayerViewModel>();
        }
    }

    public class VoteGridPlayerViewModel
    {
        public string PlayerID { get; set; }

        public string PlayerName { get; set; }

        public string Symbol { get; set; }

        public string ColorCode { get; set; }

        public decimal Score { get; set; }

        public List<VoteGridVoteViewModel> Votes { get; set; }

        public VoteGridPlayerViewModel()
        {
            Votes = new List<VoteGridVoteViewModel>();
        }
    }

    public class VoteGridVoteViewModel
    {
        public string PlayerName { get; set; }

        public string Symbol { get; set; }

        public string ColorCode { get; set; }
    }
}
