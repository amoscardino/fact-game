using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using FactGame.Web.Models;

namespace FactGame.Web.Controllers
{
    public class ErrorController : BaseController
    {
        public ErrorController(IHostingEnvironment hostingEnvironment)
            : base(hostingEnvironment) { }

        [Route("/error")]
        public IActionResult Index()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
