using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FactGame.Web.DataAccess.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace FactGame.Web.DataAccess
{
    public class MongoDbRepository : IFactGameRepository
    {
        #region Private Members
        private IMongoCollection<Game> _collection;
        #endregion

        #region Constructor
        public MongoDbRepository(IConfiguration config)
        {
            var client = new MongoClient(config.GetConnectionString("FactGameData"));
            var database = client.GetDatabase("factgame");

            _collection = database.GetCollection<Game>("games");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets a new ID string (converted GUID)
        /// </summary>
        /// <returns></returns>
        public string GetNewId()
        {
            var enc = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

            enc = enc.Replace("/", "_").Replace("+", "-");

            return enc.Substring(0, 22);
        }

        /// <summary>
        /// Gets a single game from the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Game> GetGame(string id)
        {
            return await _collection.Find(x => x.ID == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Upserts a single game to the database
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public async Task UpdateGame(Game game)
        {
            await _collection.ReplaceOneAsync(x => x.ID == game.ID, game, new UpdateOptions { IsUpsert = true });
        }
        #endregion
    }
}
