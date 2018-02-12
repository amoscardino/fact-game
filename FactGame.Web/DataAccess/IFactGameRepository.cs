using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FactGame.Web.DataAccess.Models;

namespace FactGame.Web.DataAccess
{
    public interface IFactGameRepository
    {
        string GetNewId();

        Task<Game> GetGame(string id);

        Task UpdateGame(Game game);
    }
}
