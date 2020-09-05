using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ddac7.Areas.Identity.Data;
using ddac7.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ddac7.Controllers
{
    public class ClinicController : Controller
    {
        private readonly UserManager<ClinicAppUser> _userManager;
        private readonly AuthDbContext _context;

        public ClinicController(AuthDbContext context, UserManager<ClinicAppUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> CreateClinic()
        {
            var userCheck = await _userManager.GetUserAsync(User);
            var userId = userCheck.Id;

            var allclinic = _context.Clinic.ToList();
            var check = allclinic.Where(x => x.UserID.Equals(userId)).ToList();

            if (check.Count == 0)
            {
                TempData["message"] = "To manage your clinic, please add your clinic information.";
                return View();
            }
            else
            {
                return RedirectToAction("ManageClinic", new { uID = userId });
            }

        }

        public IActionResult ManageClinic(string uID)
        {
            var result = (from a in _context.Clinic
                          where a.UserID.Equals(uID)
                          select new Clinic()
                          {
                              Id = a.Id,
                              ClinicName = a.ClinicName,
                              ClinicDesc = a.ClinicDesc,
                              ContactNum = a.ContactNum,
                              ContactEmail = a.ContactEmail,
                              Status = a.Status,
                              UserID = a.UserID
                          }).First();

            return View(result);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClinicName,ClinicDesc,ContactNum,ContactEmail,Status,UserID")] Clinic clinic)
        {
            if (ModelState.IsValid)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser.Id;

                var createClinic = new Clinic
                {
                    ClinicName = clinic.ClinicName,
                    ClinicDesc = clinic.ClinicDesc,
                    ContactNum = clinic.ContactNum,
                    ContactEmail = clinic.ContactEmail,
                    Status = clinic.Status,
                    UserID = userId
                };

                _context.Add(createClinic);
                await _context.SaveChangesAsync();
                TempData["message"] = "Clinic " + clinic.ClinicName + " has been created.";

                return RedirectToAction("ManageClinic", new { uID = userId });
            }

            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClinicName,ClinicDesc,ContactNum,ContactEmail,Status,UserID")] Clinic clinic)
        {
            if (ModelState.IsValid)
            {
                    _context.Update(clinic);
                    TempData["message"] = "Your clinic details has been updated.";
                    await _context.SaveChangesAsync();

                return RedirectToAction("ManageClinic", new { uID = clinic.UserID });
            }
            return View(clinic);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, [Bind("Id,ClinicName,ClinicDesc,ContactNum,ContactEmail,Status,UserID")] Clinic clinic)
        {
            if (ModelState.IsValid)
            {
                _context.Update(clinic);
                TempData["message"] = "Your clinic status has been changed to " + clinic.Status + ".";
                await _context.SaveChangesAsync();

                return RedirectToAction("ManageClinic", new { uID = clinic.UserID });
            }
            return View(clinic);

        }
    }
}