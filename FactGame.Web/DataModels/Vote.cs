﻿using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FactGame.Web.DataModels
{
    public class Vote
    {
        public string FactID { get; set; }

        public string GuessPlayerID { get; set; }
    }
}
