using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactGame.Web.DataAccess.Models
{
    public class Player
    {
        [BsonId]
        public string ID { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public string Color { get; set; }

        public string Fact { get; set; }

        public string FactID { get; set; }

        public decimal Score { get; set; }

        public List<Vote> Votes { get; set; }

        public Player()
        {
            Votes = new List<Vote>();
        }
    }
}
