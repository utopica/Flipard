﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flipard.MVC.Controllers
{
    [Authorize]
    public class MainController : Controller
    {
        
        public IActionResult Index()
        {
            return View();
        }
    }
}
