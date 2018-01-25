using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactGame.Web.DataModels
{
    public class Player
    {
        public string ID { get; set; }

        public string GameID { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public string Color { get; set; }

        public string Fact { get; set; }

        public string FactID { get; set; }

        public decimal Score { get; set; }
    }
}
