using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ddac7.Controllers
{
    //[Authorize(Roles ="SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;

        public AdminController(RoleManager<IdentityRole> roleManager) {
            this.roleManager = roleManager;
        }
        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
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