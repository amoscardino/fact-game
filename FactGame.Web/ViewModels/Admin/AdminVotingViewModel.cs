using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactGame.Web.ViewModels
{
    public class AdminVotingViewModel
    {
        public string Name { get; set; }

        public string GameID { get; set; }

        public string AdminToken { get; set; }

        public List<AdminVotingFactViewModel> Facts { get; set; }

        public List<AdminVotingPlayerViewModel> Players { get; set; }

        public AdminVotingViewModel()
        {
            Facts = new List<AdminVotingFactViewModel>();
            Players = new List<AdminVotingPlayerViewModel>();
        }
    }

    public class AdminVotingFactViewModel
    {
        public string FactID { get; set; }

        public string Fact { get; set; }

        public List<AdminVotingPlayerViewModel> Players { get; set; }

        public AdminVotingFactViewModel()
        {
            Players = new List<AdminVotingPlayerViewModel>();
        }
    }

    public class AdminVotingPlayerViewModel
    {
        public string PlayerID { get; set; }

        public string PlayerName { get; set; }

        public string Symbol { get; set; }

        public string ColorCode { get; set; }

        public List<AdminVotingVoteViewModel> Votes { get; set; }

        public AdminVotingPlayerViewModel()
        {
            Votes = new List<AdminVotingVoteViewModel>();
        }
    }

    public class AdminVotingVoteViewModel
    {
        public string PlayerName { get; set; }

        public string Symbol { get; set; }

        public string ColorCode { get; set; }
    }
}
