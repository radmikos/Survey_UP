using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

using SurveyUP.Models;
using SurveyUP.Models.Tables;

namespace SurveyUP.Pages.Users
{
    public class CreateModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ILogger<CreateModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly N3mikosContext _context;

        public CreateModel(
             UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<CreateModel> logger,
            IEmailSender emailSender,
            N3mikosContext context)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _roleManager = roleManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel       : ApplicationUser
        {
            [Required(ErrorMessage = "Pole Hasło jest wymagane")]
            [StringLength(100, ErrorMessage = " {0} musi mieć przynajmniej {2}, a maksymalnie {1} znaków", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Hasło")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Potwierdź hasło")]
            [Compare("Password", ErrorMessage = "Hasła nie są zgodne")]
            public string ConfirmPassword { get; set; }

            [Display(Name = "Wybierz rolę")]
            public string Role { get; set; }
        }

        public record RoleToAdd {
            public string RoleName { get; set; }
        }
        public string ReturnUrl { get; set; }

        public IActionResult OnGet()
        {
            var rolesToAdd = new List<RoleToAdd>();

            foreach(var v in Enum.GetValues(typeof(Data.Enums.Roles))) {
                rolesToAdd.Add(new RoleToAdd { RoleName = v.ToString() });
            }
            ViewData["Roles"] = new SelectList(rolesToAdd, "RoleName", "RoleName");
            
            return Page();
        }

        [BindProperty]
        public ApplicationUser AspNetUser { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            var returnUrl = Url.Content("~/");

            ModelState["Name"].RawValue = ModelState["Input.Name"].RawValue;
            ModelState["Name"].ValidationState = ModelState["Input.Name"].ValidationState;

            ModelState["Email"].RawValue = ModelState["Input.Email"].RawValue;
            ModelState["Email"].ValidationState = ModelState["Input.Email"].ValidationState;

            ModelState["FirstName"].RawValue = ModelState["Input.FirstName"].RawValue;
            ModelState["FirstName"].ValidationState = ModelState["Input.FirstName"].ValidationState;

            ModelState["PhoneNumber"].RawValue = ModelState["Input.PhoneNumber"].RawValue;
            ModelState["PhoneNumber"].ValidationState = ModelState["Input.PhoneNumber"].ValidationState;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = new ApplicationUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                Name = Input.Name,
                FirstName = Input.FirstName,
                MiddleName = Input.MiddleName,
                AlbumNumber = Input.AlbumNumber,
                Department = Input.Department,
                UniversityDirection = Input.UniversityDirection,
                Degree = Input.Degree,
                LearningForm = Input.LearningForm,
                PermanentAddress = Input.PermanentAddress,
                PermanentCity = Input.PermanentCity,
                PermanentPostalCode = Input.PermanentPostalCode,
                CorrespondAddress = Input.CorrespondAddress,
                CorrespondCity = Input.CorrespondCity,
                CorrespondPostalCode = Input.CorrespondPostalCode,
                NameMother = Input.NameMother,
                NameFather = Input.NameFather,
                BirthDate = Input.BirthDate,
                Pesel = Input.Pesel,
                BirthPlace = Input.BirthPlace,
                PhoneNumber = Input.PhoneNumber

            };
            var result = await _userManager.CreateAsync(user, Input.Password);


            if (result.Succeeded)
            {

                if (!await _roleManager.RoleExistsAsync(Data.Enums.Roles.Administrator.ToString()))
                {
                    await _roleManager.CreateAsync(new ApplicationRole(Data.Enums.Roles.Administrator.ToString()));
                }

                if (!await _roleManager.RoleExistsAsync(Data.Enums.Roles.Sekretariat.ToString()))
                {
                    await _roleManager.CreateAsync(new ApplicationRole(Data.Enums.Roles.Sekretariat.ToString()));
                }
                if (!await _roleManager.RoleExistsAsync(Data.Enums.Roles.Twórca.ToString()))
                {
                    await _roleManager.CreateAsync(new ApplicationRole(Data.Enums.Roles.Twórca.ToString()));
                }
                if (!await _roleManager.RoleExistsAsync(Data.Enums.Roles.Student.ToString()))
                {
                    await _roleManager.CreateAsync(new ApplicationRole(Data.Enums.Roles.Student.ToString()));
                }

                await _userManager.AddToRoleAsync(user, Input.Role);

                _logger.LogInformation("User created a new account with password.");

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                return RedirectToPage("./Index");
                
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            // If we got this far, something failed, redisplay form
            return Page();

        }
    }
}
