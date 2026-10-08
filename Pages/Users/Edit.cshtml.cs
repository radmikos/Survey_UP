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
    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public EditModel(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        [BindProperty]
        public UsetInputModelNoPass InputModel { get; set; }


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

            var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Id == id);

            InputModel = new UsetInputModelNoPass
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
                PhoneNumber = user.PhoneNumber,
            };

          

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


            if (!ModelState.IsValid)
            {
                return Page();
            }


            try
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.Id == TempData["id"].ToString());


                user.Email = InputModel.Email;
                user.Name = InputModel.Name;
                user.FirstName = InputModel.FirstName;
                user.MiddleName = InputModel.MiddleName;
                user.AlbumNumber = InputModel.AlbumNumber;
                user.Department = InputModel.Department;
                user.UniversityDirection = InputModel.UniversityDirection;
                user.Degree = InputModel.Degree;
                user.LearningForm = InputModel.LearningForm;
                user.PermanentAddress = InputModel.PermanentAddress;
                user.PermanentCity = InputModel.PermanentCity;
                user.PermanentPostalCode = InputModel.PermanentPostalCode;
                user.CorrespondAddress = InputModel.CorrespondAddress;
                user.CorrespondCity = InputModel.CorrespondCity;
                user.CorrespondPostalCode = InputModel.CorrespondPostalCode;
                user.NameMother = InputModel.NameMother;
                user.NameFather = InputModel.NameFather;
                user.BirthDate = InputModel.BirthDate;
                user.Pesel = InputModel.Pesel;
                user.BirthPlace = InputModel.BirthPlace;
                user.PhoneNumber = InputModel.PhoneNumber;
                user.UserName = InputModel.Email;

                



                await _userManager.UpdateAsync(user);

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
