using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactGame.Web.Models
{
    public class PlayerVotingViewModel
    {
        public string GameID { get; set; }

        public string GameName { get; set; }

        public string PlayerID { get; set; }

        public List<SelectListItem> Players { get; set; }

        public List<PlayerVoteViewModel> Votes { get; set; }

        public PlayerVotingViewModel()
        {
            Players = new List<SelectListItem>();
            Votes = new List<PlayerVoteViewModel>();
        }
    }

    public class PlayerVoteViewModel
    {
        public string FactID { get; set; }

        public string Fact { get; set; }

        public string GuessPlayerID { get; set; }
    }
}
