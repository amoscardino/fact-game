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

        /// <summary>
        /// From: https://madskristensen.net/blog/A-shorter-and-URL-friendly-GUID
        /// </summary>
        /// <returns></returns>
        protected string GetNewId()
        {
            var enc = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            enc = enc.Replace("/", "_").Replace("+", "-");

            return enc.Substring(0, 22);
        }

        protected async Task<Game> GetGameAsync(string id)
        {
            var collection = GetCollection();

            return await collection.Find(x => x.ID == id).FirstOrDefaultAsync();
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
