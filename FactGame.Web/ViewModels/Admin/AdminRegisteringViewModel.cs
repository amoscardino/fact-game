using System;
using System.Collections.Generic;

namespace FactGame.Web.Models
{
    public class AdminRegisteringViewModel
    {
        public string Name { get; set; }

        public string GameID { get; set; }

        public string AdminToken { get; set; }

        public IEnumerable<DataModels.Player> Players { get; set; }
    }
}
