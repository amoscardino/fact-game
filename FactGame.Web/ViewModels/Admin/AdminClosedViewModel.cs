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

        public AdminVoteGridViewModel VoteGrid { get; set; }

        public PlayerListViewModel PlayerList { get; set; }

        public AdminClosedViewModel()
        {
            PlayerList = new PlayerListViewModel();
            VoteGrid = new AdminVoteGridViewModel();
        }
    }
}
