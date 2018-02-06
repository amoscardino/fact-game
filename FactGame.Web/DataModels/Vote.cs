using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactGame.Web.DataModels
{
    public class Vote
    {
        public ObjectId FactID { get; set; }

        public ObjectId GuessPlayerID { get; set; }
    }
}
