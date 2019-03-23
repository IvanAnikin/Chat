using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Server.Controllers
{
    public class Chat_dataController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}