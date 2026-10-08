using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using SurveyUP.Data;
using SurveyUP.Models;
using SurveyUP.Models.Tables;

namespace SurveyUP.Pages.Users
{
    public class ChangeRole : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public ChangeRole(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        [BindProperty]
        public UserInputModel InputModel { get; set; }


        [Display(Name = "Wybierz rolę")]
        [BindProperty]
        public string Role { get; set; }

        public record RoleToAdd
        {
            public string RoleName { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            TempData["id"] = id;

            var rolesToAdd = new List<RoleToAdd>();
            foreach (var v in Enum.GetValues(typeof(Data.Enums.Roles)))
            {
                rolesToAdd.Add(new RoleToAdd { RoleName = v.ToString() });
            }
            ViewData["Roles"] = new SelectList(rolesToAdd, "RoleName", "RoleName");


            var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Id == id);

            InputModel = new UserInputModel
            {
                Email = user.Email,
                Name = user.Name,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                AlbumNumber = user.AlbumNumber,
                Department = user.Department,
                UniversityDirection = user.UniversityDirection,
                Degree = user.Degree,
                LearningForm = user.LearningForm,
                PermanentAddress = user.PermanentAddress,
                PermanentCity = user.PermanentCity,
                PermanentPostalCode = user.PermanentPostalCode,
                CorrespondAddress = user.CorrespondAddress,
                CorrespondCity = user.CorrespondCity,
                CorrespondPostalCode = user.CorrespondPostalCode,
                NameMother = user.NameMother,
                NameFather = user.NameFather,
                BirthDate = user.BirthDate,
                Pesel = user.Pesel,
                BirthPlace = user.BirthPlace,
                PhoneNumber = user.PhoneNumber
            };

            var roles = await _userManager.GetRolesAsync(user);
       
            foreach (var r in roles)
            {
                Role = r;
            }
            TempData["OldRole"] = Role;

            if (InputModel == null)
            {
                return NotFound();
            }
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
          

            try
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Id == TempData["id"].ToString());

                await _userManager.RemoveFromRoleAsync(user, TempData["OldRole"].ToString());
                await _userManager.AddToRoleAsync(user, Role);

            }
            catch (Exception)
            {
                if (!AspNetUsersExists(TempData["id"].ToString()))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool AspNetUsersExists(string id)
        {
            return _userManager.Users.Any(u => u.Id == id);
        }
    }
}
