﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactGame.Web.ViewModels
{
    public class PlayerClosedViewModel
    {
        public string GameName { get; set; }

        public decimal Score { get; set; }

        public string Fact { get; set; }

        public List<string> FactGuessedByPlayerNames { get; set; }

        public List<PlayerClosedVoteViewModel> Votes { get; set; }

        public PlayerListViewModel PlayerList { get; set; }

        public PlayerClosedViewModel()
        {
            FactGuessedByPlayerNames = new List<string>();
            Votes = new List<PlayerClosedVoteViewModel>();
            PlayerList = new PlayerListViewModel();
        }
    }

    public class PlayerClosedVoteViewModel
    {
        public string Fact { get; set; }

        public string FactID { get; set; }

        public string GuessPlayerName { get; set; }

        public string ActualPlayerName { get; set; }

        public bool Correct { get; set; }
    }
}
