using FactGame.Web.DataAccess.Models;
using System;
using System.Collections.Generic;

namespace FactGame.Web.ViewModels
{
    public class AdminRegisteringViewModel
    {
        public string Name { get; set; }

        public string GameID { get; set; }

        public string AdminToken { get; set; }

        public AdminPlayerListViewModel PlayerList { get; set; }

        public AdminRegisteringViewModel()
        {
            PlayerList = new AdminPlayerListViewModel();
        }
    }
}
