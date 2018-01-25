using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Threading.Tasks;
using Dapper;

namespace FactGame.Web.Controllers
{
    public class BaseController : Controller
    {
        private IHostingEnvironment _HostingEnvironment;

        public BaseController(IHostingEnvironment hostingEnvironment)
        {
            _HostingEnvironment = hostingEnvironment;
        }

        protected async Task<SqliteConnection> GetDatabaseConnection()
        {
            var databasePath = Path.Combine(_HostingEnvironment.ContentRootPath, "App_Data", "FactGame.sqlite");
            var connectionString = "Data Source=" + databasePath;

            var connection = new SqliteConnection(connectionString);
            
            var schemaSqlFilePath = Path.Combine(_HostingEnvironment.ContentRootPath, "App_Data", "FactGame.schema.sql");
            var schemaSql = await System.IO.File.ReadAllTextAsync(schemaSqlFilePath);

            await connection.ExecuteAsync(schemaSql);

            return connection;
        }

        protected string GetNewID()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }
    }
}
