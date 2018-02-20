using FactGame.Web.DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactGame.Web.ViewModels
{
    public class AdminVoteGridViewModel
    {
        public bool ShowAnswers { get; set; }

        public List<AdminVoteGridPlayerViewModel> Players { get; set; }

        public List<AdminVoteGridFactViewModel> Facts { get; set; }

        public AdminVoteGridViewModel()
        {
            Players = new List<AdminVoteGridPlayerViewModel>();
            Facts = new List<AdminVoteGridFactViewModel>();
        }

        public AdminVoteGridViewModel(Game game, bool showAnswers)
        {
            ShowAnswers = showAnswers;

            Facts = game.Players
                .OrderBy(p => p.FactID)
                .Select(p => new AdminVoteGridFactViewModel
                {
                    Fact = p.Fact,
                    FactID = p.FactID,
                    PlayerID = p.ID
                })
                .ToList();

            Players = game.Players
                .OrderBy(p => p.Name)
                .Select(p => new AdminVoteGridPlayerViewModel
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
                    .Select(p => new AdminVoteGridPlayerViewModel
                    {
                        PlayerID = p.ID,
                        PlayerName = p.Name,
                        Symbol = p.Symbol,
                        ColorCode = p.Color,
                        Votes = game.Players
                            .OrderBy(q => q.Name)
                            .Where(q => q.Votes.Any(x => x.GuessPlayerID == p.ID && x.FactID == fact.FactID))
                            .Select(q => new AdminVoteGridVoteViewModel
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

    public class AdminVoteGridFactViewModel
    {
        public string PlayerID { get; set; }

        public string FactID { get; set; }

        public string Fact { get; set; }

        public List<AdminVoteGridPlayerViewModel> Players { get; set; }

        public AdminVoteGridFactViewModel()
        {
            Players = new List<AdminVoteGridPlayerViewModel>();
        }
    }

    public class AdminVoteGridPlayerViewModel
    {
        public string PlayerID { get; set; }

        public string PlayerName { get; set; }

        public string Symbol { get; set; }

        public string ColorCode { get; set; }

        public decimal Score { get; set; }

        public List<AdminVoteGridVoteViewModel> Votes { get; set; }

        public AdminVoteGridPlayerViewModel()
        {
            Votes = new List<AdminVoteGridVoteViewModel>();
        }
    }

    public class AdminVoteGridVoteViewModel
    {
        public string PlayerName { get; set; }

        public string Symbol { get; set; }

        public string ColorCode { get; set; }
    }
}
