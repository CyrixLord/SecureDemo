using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureDemo.Models;

namespace SecureDemo.Controllers
{
    // after going to project->properties->debug and activating https you can force https only with
    //[RequireHttps] but we can apply to entire project as a filter in starutp.cs
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // INSERT METHOD TO LOGIN user from the basic index.cshtml page
        [HttpPost]
        // you could put this here, but we will do something in startup.cs [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return RedirectToAction(nameof(Index));
            }

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, name), // and now to make the user an admin (assign a role) 
                new Claim(ClaimTypes.Role, "admin"), // 24:24 in video: you would normally get this from your database roles "admin",etc instead of hardcoding them here
            }, CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);
            //
            // ok we are now ready to log in the user (principal) who has the claims based on their name
            //

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme, // login the user (principal) with cookies scheme
                principal);
            // after login, redirect to user index page. the login user in index will say authenticated
            return RedirectToAction(nameof(Index));
        }

        //
        // MANAGERS VIEW
        //
        //[Authorize] // require authentication to log in (authorization check: is user logged in or not?)
        // implement an 'authorization policy'
        [Authorize(Policy = "MustBeAdmin")] // made up policy must be defined in startup class in configureservices method
        public IActionResult Manage() => View(); // we want logged in people to use this so we use [authorize]

        
        //
        // LOG USER OUT ACTION
        //

        [HttpPost]
        public async Task<IActionResult> Logout()
        { // sign out of the 'cookie based scheme'
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Index));
        }

        //
        // ERRORS VIEW
        //

        public IActionResult ErrorNotLoggedIn() => View(); // you just makde cshtml files for these
        public IActionResult ErrorForbidden() => View();   // but controller needs to be aware of them too for routing

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
