using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS.Controllers
{
    public class AdministratorController : Controller
    {
        private readonly LMSContext db;

        public AdministratorController(LMSContext _db)
        {
            db = _db;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Department(string subject)
        {
            ViewData["subject"] = subject;
            return View();
        }

        public IActionResult Course(string subject, string num)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }

        /*******Begin code to modify********/

        /// <summary>
        /// Create a department which is uniquely identified by it's subject code
        /// </summary>
        /// <param name="subject">the subject code</param>
        /// <param name="name">the full name of the department</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the department already exists, true otherwise.</returns>
        public IActionResult CreateDepartment(string subject, string name)
        {


            var exists = db.Departments.Any(db => db.SubjectAbbrev == subject);
            if (!exists)
            {

                var newDepart = new Department();
                newDepart.Name = name;
                newDepart.SubjectAbbrev = subject;
                db.Departments.Add(newDepart);
                db.SaveChanges();
                return Json(new { success = true });
            }
            db.SaveChanges();
            return Json(new { success = false });


        }


        /// <summary>
        /// Returns a JSON array of all the courses in the given department.
        /// Each object in the array should have the following fields:
        /// "number" - The course number (as in 5530)
        /// "name" - The course name (as in "Database Systems")
        /// </summary>
        /// <param name="subjCode">The department subject abbreviation (as in "CS")</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetCourses(string subject)
        {
            var query = from c in db.Courses
                        join d in db.Departments
                        on c.DId equals d.DeptId
                        where d.SubjectAbbrev == subject
                        select new
                        {
                            number = c.Number,
                            name = c.Name,
                        };

            return Json(query.ToList());
        }

        /// <summary>
        /// Returns a JSON array of all the professors working in a given department.
        /// Each object in the array should have the following fields:
        /// "lname" - The professor's last name
        /// "fname" - The professor's first name
        /// "uid" - The professor's uid
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <returns>The JSON result</returns>
        public IActionResult GetProfessors(string subject)
        {
            var query = from p in db.Professors
                        join d in db.Departments
                        on p.DId equals d.DeptId
                        where d.SubjectAbbrev == subject
                        select new
                        {
                            fname = p.FirstName,
                            lname = p.LastName,
                            uid = p.UId,
                            dept = d.Name,
                            subject = d.SubjectAbbrev
                        };


            return Json(query.ToList());
        }



        /// <summary>
        /// Creates a course.
        /// A course is uniquely identified by its number + the subject to which it belongs
        /// </summary>
        /// <param name="subject">The subject abbreviation for the department in which the course will be added</param>
        /// <param name="number">The course number</param>
        /// <param name="name">The course name</param>
        /// <returns>A JSON object containing {success = true/false}.
        /// false if the course already exists, true otherwise.</returns>
        public IActionResult CreateCourse(string subject, int number, string name)
        {
            // Find the department
            var dept = db.Departments
                .FirstOrDefault(d => d.SubjectAbbrev == subject);

            if (dept == null)
                return Json(new { success = false });

            // Check if course already exists in that department
            var exists = db.Courses.Any(c =>
                c.Number == number && c.DId == dept.DeptId);

            if (exists)
                return Json(new { success = false });

            // Create course
            var newCourse = new Course
            {
                Number = number,
                Name = name,
                DId = dept.DeptId
            };

            db.Courses.Add(newCourse);
            db.SaveChanges();

            return Json(new { success = true });
        }



        /// <summary>
        /// Creates a class offering of a given course.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="number">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="start">The start time</param>
        /// <param name="end">The end time</param>
        /// <param name="location">The location</param>
        /// <param name="instructor">The uid of the professor</param>
        /// <returns>A JSON object containing {success = true/false}. 
        /// false if another class occupies the same location during any time 
        /// within the start-end range in the same semester, or if there is already
        /// a Class offering of the same Course in the same Semester,
        /// true otherwise.</returns>
        public IActionResult CreateClass(string subject, int number, string season, int year,
    DateTime start, DateTime end, string location, string instructor)
        {
            // Find the course
            var courseId = (from c in db.Courses
                            join d in db.Departments on c.DId equals d.DeptId
                            where d.SubjectAbbrev == subject && c.Number == number
                            select c.CourseId).FirstOrDefault();

            if (courseId == 0)
                return Json(new { success = false });

            // Check if class already exists in same semester
            var duplicate = db.Classes.Any(c =>
                c.CourseId == courseId &&
                c.Season == season &&
                c.Year == year);

            if (duplicate)
                return Json(new { success = false });

            // Convert times
            var startTime = TimeOnly.FromDateTime(start);
            var endTime = TimeOnly.FromDateTime(end);

            // Check for location/time conflict
            var conflict = db.Classes.Any(c =>
                c.Location == location &&
                c.Season == season &&
                c.Year == year &&
                !(endTime <= c.StartTime || startTime >= c.EndTime)
            );

            if (conflict)
                return Json(new { success = false });

            // Create class
            var newClass = new Class
            {
                CourseId = courseId,
                Season = season,
                Year = (uint)year,
                Location = location,
                StartTime = startTime,
                EndTime = endTime,
                ProfessorId = instructor
            };

            db.Classes.Add(newClass);
            db.SaveChanges();

            return Json(new { success = true });
        }


        /*******End code to modify********/

    }
}

