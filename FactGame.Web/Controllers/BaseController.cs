using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Threading.Tasks;
using Dapper;
using System.Data;

namespace FactGame.Web.Controllers
{
    public class BaseController : Controller
    {
        private const string SCHEMA_SQL = @"CREATE TABLE IF NOT EXISTS `Vote` (
                                                `VoterPlayerID`	TEXT NOT NULL,
                                                `FactID`	TEXT NOT NULL,
                                                `GuessPlayerID`	TEXT NOT NULL,
                                                FOREIGN KEY(`VoterPlayerID`) REFERENCES `Player`(`ID`),
                                                FOREIGN KEY(`FactID`) REFERENCES `Player`(`FactID`),
                                                FOREIGN KEY(`GuessPlayerID`) REFERENCES `Player`(`ID`),
                                                PRIMARY KEY(`VoterPlayerID`,`FactID`)
                                            );
                                            CREATE TABLE IF NOT EXISTS `Player` (
                                                `ID`	TEXT NOT NULL UNIQUE,
                                                `GameID`	TEXT NOT NULL,
                                                `Name`	TEXT NOT NULL,
                                                `Symbol`	TEXT NOT NULL,
                                                `Color`	TEXT NOT NULL,
                                                `Fact`	TEXT NOT NULL,
                                                `FactID`	TEXT NOT NULL UNIQUE,
                                                `Score`	NUMERIC DEFAULT 0,
                                                FOREIGN KEY(`GameID`) REFERENCES `Game`(`ID`),
                                                PRIMARY KEY(`ID`)
                                            );
                                            CREATE TABLE IF NOT EXISTS `Game` (
                                                `ID`	TEXT NOT NULL UNIQUE,
                                                `AdminToken`	TEXT NOT NULL,
                                                `Name`	TEXT NOT NULL,
                                                `Status`	INTEGER NOT NULL,
                                                PRIMARY KEY(`ID`)
                                            );";

        private IHostingEnvironment _HostingEnvironment;

        public BaseController(IHostingEnvironment hostingEnvironment)
        {
            _HostingEnvironment = hostingEnvironment;
        }

        protected async Task<IDbConnection> GetDatabaseConnection()
        {
            var databasePath = Path.Combine(_HostingEnvironment.ContentRootPath, "App_Data", "FactGame.sqlite");
            var connectionString = "Data Source=" + databasePath;

            var connection = new SqliteConnection(connectionString);
            
            await connection.ExecuteAsync(SCHEMA_SQL);

            return connection;
        }

        protected string GetNewID()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}
