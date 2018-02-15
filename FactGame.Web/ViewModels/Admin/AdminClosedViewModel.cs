using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactGame.Web.ViewModels
{
    public class AdminClosedViewModel
    {
        public string Name { get; set; }

        public string GameID { get; set; }

        public string AdminToken { get; set; }

        public List<AdminClosedFactViewModel> Facts { get; set; }

        public List<AdminClosedPlayerViewModel> Players { get; set; }

        public AdminPlayerListViewModel PlayerList { get; set; }

        public AdminClosedViewModel()
        {
            Facts = new List<AdminClosedFactViewModel>();
            Players = new List<AdminClosedPlayerViewModel>();

            PlayerList = new AdminPlayerListViewModel();
        }
    }

    public class AdminClosedFactViewModel
    {
        public string PlayerID { get; set; }

        public string FactID { get; set; }

        public string Fact { get; set; }

        public List<AdminClosedPlayerViewModel> Players { get; set; }

        public AdminClosedFactViewModel()
        {
            Players = new List<AdminClosedPlayerViewModel>();
        }
    }

    public class AdminClosedPlayerViewModel
    {
        public string PlayerID { get; set; }

        public string PlayerName { get; set; }

        public string Symbol { get; set; }

        public string ColorCode { get; set; }

        public decimal Score { get; set; }

        public List<AdminClosedVoteViewModel> Votes { get; set; }

        public AdminClosedPlayerViewModel()
        {
            Votes = new List<AdminClosedVoteViewModel>();
        }
    }

    public class AdminClosedVoteViewModel
    {
        public string PlayerName { get; set; }

        public string Symbol { get; set; }

        public string ColorCode { get; set; }
    }
}
