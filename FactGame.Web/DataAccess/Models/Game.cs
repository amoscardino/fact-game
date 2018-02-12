using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactGame.Web.DataAccess.Models
{
    public class Game
    {
        [BsonId]
        public string ID { get; set; }

        public string AdminToken { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// 0 = Registering
        /// 1 = Voting
        /// 2 = Closed
        /// </summary>
        public int Status { get; set; }

        public List<Player> Players { get; set; }

        public Game()
        {
            Players = new List<Player>();
        }
    }
}
