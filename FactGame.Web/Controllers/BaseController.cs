using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using FactGame.Web.DataModels;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Linq;

namespace FactGame.Web.Controllers
{
    public class BaseController : Controller
    {
        private IConfiguration _config;

        public BaseController(IConfiguration config)
        {
            _config = config;
        }

        protected async Task<Game> GetGameAsync(string id)
        {
            var collection = GetCollection();
            var gameId = ObjectId.Parse(id);

            return await collection.Find(x => x.ID == gameId).FirstOrDefaultAsync();
        }

        protected async Task UpdateGameAsync(Game game)
        {
            var collection = GetCollection();

            await collection.ReplaceOneAsync(x => x.ID == game.ID, game, new UpdateOptions { IsUpsert = true });
        }

        private IMongoCollection<Game> GetCollection()
        {
            var client = new MongoClient(_config.GetConnectionString("FactGameData"));
            var database = client.GetDatabase("factgame");
            var collection = database.GetCollection<Game>("games");

            return collection;
        }
    }
}
