// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using LMS.Models;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LMS.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        //private readonly IUserEmailStore<IdentityUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly LMSContext db;
        //private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            LMSContext _db
            /*IEmailSender emailSender*/)
        {
            _userManager = userManager;
            _userStore = userStore;
            //_emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            db = _db;
            //_emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {

            [Required]
            [Display(Name = "Role")]
            public string Role { get; set; }

            public List<SelectListItem> Roles { get; } = new List<SelectListItem>
            {
                new SelectListItem { Value = "Student", Text = "Student" },
                new SelectListItem { Value = "Professor", Text = "Professor" },
                new SelectListItem { Value = "Administrator", Text = "Administrator"  }
            };

            public string Department { get; set; }

            public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>
            {
                new SelectListItem{Value = "None", Text = "NONE"}
                
            };

            [Required]
            [Display( Name = "First Name" )]
            public string FirstName { get; set; }

            [Required]
            [Display( Name = "Last Name" )]
            public string LastName { get; set; }

            [Required]
            [Display( Name = "Date of Birth" )]
            [BindProperty, DisplayFormat( DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true )]
            [DataType( DataType.Date )]
            public System.DateTime DOB { get; set; } = DateTime.Now;

            [Required]
            //[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType( DataType.Password )]
            [Display( Name = "Password" )]
            public string Password { get; set; }

            [DataType( DataType.Password )]
            [Display( Name = "Confirm password" )]
            [Compare( "Password", ErrorMessage = "The password and confirmation password do not match." )]
            public string ConfirmPassword { get; set; }

        }


        public async Task OnGetAsync( string returnUrl = null )
        {
            ReturnUrl = returnUrl;
            ExternalLogins = ( await _signInManager.GetExternalAuthenticationSchemesAsync() ).ToList();
          
                var query = from d in db.Departments
                            select d.SubjectAbbrev;

                foreach( var v in query)
                {
                    
                    Input.Departments.Add(new SelectListItem { Value = v, Text = v });
                }
            



        }

        public async Task<IActionResult> OnPostAsync( string returnUrl = null )
        {
            returnUrl ??= Url.Content( "~/" );
            ExternalLogins = ( await _signInManager.GetExternalAuthenticationSchemesAsync() ).ToList();
            var query = from d in db.Departments
                        select d.SubjectAbbrev;

            foreach (var v in query)
            {

                Input.Departments.Add(new SelectListItem { Value = v, Text = v });
            }

            if ( ModelState.IsValid )
            {
               
         
       
                var uid = CreateNewUser(Input.FirstName, Input.LastName, Input.DOB, Input.Department, Input.Role);
                var user = new ApplicationUser { UserName = uid };

                await _userStore.SetUserNameAsync( user, uid, CancellationToken.None );
                var result = await _userManager.CreateAsync(user, Input.Password);

                if ( result.Succeeded )
                {
                    _logger.LogInformation( "User created a new account with password." );
                    await _userManager.AddToRoleAsync( user, Input.Role );

                    var userId = await _userManager.GetUserIdAsync(user);

                    await _signInManager.SignInAsync( user, isPersistent: false );
                    return LocalRedirect( returnUrl );

                }
                foreach ( var error in result.Errors )
                {
                    ModelState.AddModelError( string.Empty, error.Description );
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
        
        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException( $"Can't create an instance of '{nameof( ApplicationUser )}'. " +
                    $"Ensure that '{nameof( IdentityUser )}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml" );
            }
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a new user of the LMS with the specified information and add it to the database.
        /// Assigns the user a unique uID consisting of a 'u' followed by 7 digits.
        /// </summary>
        /// <param name="firstName">The user's first name</param>
        /// <param name="lastName">The user's last name</param>
        /// <param name="DOB">The user's date of birth</param>
        /// <param name="departmentAbbrev">The department abbreviation that the user belongs to (ignore for Admins) </param>
        /// <param name="role">The user's role: one of "Administrator", "Professor", "Student"</param>
        /// <returns>The uID of the new user</returns>
        string CreateNewUser(string firstName, string lastName, DateTime DOB, string departmentAbbrev, string role)
        {

            var newID = "J'DINKLANGE MORGOONE";
           
            
                // find max to get uID
                var maxUID = "u3000000";
                var adminQuery =
                    from a in db.Administrators
                    select a.UId;
                var profQuery =
                    from p in db.Professors
                    select p.UId;
                var studQuery =
                    from s in db.Students
                    select s.UId;
                var deptID = db.Departments
                    .Where(d => d.SubjectAbbrev == departmentAbbrev)
                    .Select(d => d.DeptId)
                    .SingleOrDefault();

                foreach (var v in adminQuery)
                {
                    if (int.Parse(v.Substring(1)) > int.Parse(maxUID.Substring(1)))
                        maxUID = v;

                }
                foreach (var v in profQuery)
                {
                    if (int.Parse(v.Substring(1)) > int.Parse(maxUID.Substring(1)))
                        maxUID = v;

                }
                foreach (var v in studQuery)
                {
                    if (int.Parse(v.Substring(1)) > int.Parse(maxUID.Substring(1)))
                        maxUID = v;

                }
                Console.WriteLine(departmentAbbrev);
               
         
                

                // if (deptID == -1) {CODE TO HANDLE STUDENTS OR PROFS WITHOUT DEPARTMENTS}

                int num = int.Parse(maxUID.Substring(1));
                num++;
                newID = "u" + num;

                var newDOB = DateOnly.FromDateTime(DOB);

                switch (role)
                {
                    case "Administrator":

                        Administrator newAdmin = new Administrator();
                        newAdmin.FirstName = firstName;
                        newAdmin.LastName = lastName;
                        newAdmin.Dob = newDOB;
                        newAdmin.UId = newID;

                        db.Administrators.Add(newAdmin);

                        break;
                    //case 2 Professor
                    case "Professor":
                        Professor newProf = new Professor();
                        newProf.Dob = newDOB;
                        newProf.FirstName = firstName;
                        newProf.LastName = lastName;
                        newProf.UId = newID;
                        newProf.DId = deptID;

                        db.Professors.Add(newProf);

                        break;

                    //case 3 Student
                    case "Student":
                        Student newStud = new Student();
                        newStud.Dob = newDOB;
                        newStud.FirstName = firstName;
                        newStud.LastName = lastName;
                        newStud.UId = newID;
                        newStud.DId = deptID; 

                        db.Students.Add(newStud);
                        break;


                
               
            }
            db.SaveChanges();


            return newID;
        }


        /*******End code to modify********/
    }
}
