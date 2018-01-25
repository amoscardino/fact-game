using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactGame.Web.DataModels
{
    public class Game
    {
        public string ID { get; set; }

        public string AdminToken { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// 0 = Registering
        /// 1 = Voting
        /// 2 = CLosed
        /// </summary>
        public int Status { get; set; }
    }
}
