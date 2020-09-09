using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ddac7.Models;
using ddac7.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace ddac7.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly UserManager<ClinicAppUser> _userManager;
        private readonly AuthDbContext _context;

        public HomeController(UserManager<ClinicAppUser> userManager, AuthDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.IsInRole("Clinic")) 
            {
                return LocalRedirect("~/Clinic/Index");
            }

            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }
        public IActionResult AnotherTestView()
        {
            return View();
        }
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ViewClinicList(string SearchString)
        {
            var result = (from a in _context.Clinic
                          where a.Status.Equals("Open")
                          select new Clinic()
                          {
                              Id = a.Id,
                              ClinicName = a.ClinicName,
                              ClinicDesc = a.ClinicDesc,
                              ContactNum = a.ContactNum,
                              ContactEmail = a.ContactEmail,
                              Status = a.Status,
                              UserID = a.UserID
                          }).ToList();

            if (!String.IsNullOrEmpty(SearchString))
            {
                result = result.Where(s => 
                    CultureInfo.CurrentCulture.CompareInfo.IndexOf
                    (s.ClinicName, SearchString, CompareOptions.IgnoreCase)>=0).ToList();
            }

            return View(result);
        }

        // View Clinic Details Page
        public async Task<IActionResult> ClinicDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clinic = await _context.Clinic
                .FirstOrDefaultAsync(m => m.Id == id);

            if (clinic == null)
            {
                return NotFound();
            }

            return View(clinic);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
