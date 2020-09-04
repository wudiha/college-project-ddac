using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ddac7.Areas.Identity.Data;
using ddac7.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ddac7.Controllers
{
    //[Authorize(Roles ="SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ClinicAppUser> _userManager;
        private readonly AuthDbContext _context;

        public AdminController(RoleManager<IdentityRole> roleManager, AuthDbContext context, UserManager<ClinicAppUser> userManager) {
            this.roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        //Admin Main Page
        public IActionResult Index()
        {
            return View();
        }

        //Admin Clinic List
        public IActionResult ClinicList()
        {

            var clinic = (from user in _context.Users
                          join userRole in _context.UserRoles on user.Id equals userRole.UserId
                          join role in _context.Roles on userRole.RoleId equals role.Id
                          where role.Name == "Clinic"
                          select new User()
                          {
                              Id = user.Id,
                              Email = user.Email,
                              PhoneNumber = user.PhoneNumber,
                          }).ToList();

            return View(clinic);

        }

        //Get clinic details for edit
        [HttpGet]
        public async Task<IActionResult> EditClinic(string id)
        {
            var clinic = await _userManager.FindByIdAsync(id);

            if (id == null)
            {
                return NotFound();
            }

            var model = new User
            {
                Id = clinic.Id,
                Email = clinic.Email,
                PhoneNumber = clinic.PhoneNumber,
            };

            if (clinic == null)
            {
                return NotFound();
            }
            return View(model);
        }

        //Edit clinic
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User model)
        {
            var clinic = await _userManager.FindByIdAsync(model.Id);

            if (model.Id == null)
            {
                return NotFound();
            }
            else
            {
                clinic.PhoneNumber = model.PhoneNumber;
                var result = await _userManager.UpdateAsync(clinic);

                if (result.Succeeded)
                {
                    TempData["message"] = "Clinic " + clinic.Email + " account has been updated.";
                    return RedirectToAction("ClinicList", "Admin");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteClinic(string id)
        {
            var clinic = await _userManager.FindByIdAsync(id);

            if (id == null)
            {
                return NotFound();
            }

            var model = new User
            {
                Id = clinic.Id,
                Email = clinic.Email,
                PhoneNumber = clinic.PhoneNumber
            };

            if (clinic == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var clinic = await _context.Users.FindAsync(id);
            _context.Users.Remove(clinic);
            await _context.SaveChangesAsync();
            TempData["message"] = "Clinic " + clinic.Email + " account has been deleted.";
            return RedirectToAction("ClinicList", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(Models.RoleModel roleModel)
        {
            if (ModelState.IsValid) { 
            IdentityRole identityRole = new IdentityRole {
                Name = roleModel.RoleName
            };
                IdentityResult identityResult = await roleManager.CreateAsync(identityRole); //sync method

                if (identityResult.Succeeded)
                {
                    return RedirectToAction("index", "home");
                }

                foreach(IdentityError error in identityResult.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            //use role manager service to create role
            
            return View(roleModel);
        }

    }
}