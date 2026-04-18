using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo( "LMSControllerTests" )]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/

        // helper method to get classID
        private int? GetClassId(string subject, int num, string season, int year)
        {
            return (from c in db.Classes
                    join co in db.Courses on c.CourseId equals co.CourseId
                    join d in db.Departments on co.DId equals d.DeptId
                    where d.SubjectAbbrev == subject
                          && co.Number == num
                          && c.Season == season
                          && c.Year == year
                    select (int?)c.ClassId).FirstOrDefault();
        }


        // helper method to get assignmentID
        private int? GetAssignmentId(string subject, int num, string season, int year, string category, string asgname)
        {
            return (from a in db.Assignments
                    join cat in db.AssignmentCatergories on a.CatId equals cat.CatId
                    join c in db.Classes on cat.ClassId equals c.ClassId
                    join co in db.Courses on c.CourseId equals co.CourseId
                    join d in db.Departments on co.DId equals d.DeptId
                    where d.SubjectAbbrev == subject
                          && co.Number == num
                          && c.Season == season
                          && c.Year == year
                          && cat.Name == category
                          && a.Name == asgname
                    select (int?)a.AssId).FirstOrDefault();
        }

        private string CalculateGrade(string uid, int classId)
        {
            var categories = db.AssignmentCatergories
                .Where(c => c.ClassId == classId)
                .ToList();

            if (!categories.Any())
                return "--";

            double weightedTotal = 0;
            double weightSum = 0;

            foreach (var cat in categories)
            {
                var assignments = db.Assignments
                    .Where(a => a.CatId == cat.CatId)
                    .ToList();

                if (!assignments.Any())
                    continue;

                double earned = 0;
                double possible = 0;

                foreach (var a in assignments)
                {
                    possible += a.Points;

                    var sub = db.Submissions
                        .FirstOrDefault(s => s.AssId == a.AssId && s.SId == uid);

                    if (sub != null)
                        // earned += sub.Score;
                        earned += (double)(sub.Score ?? 0);
                }

                if (possible == 0)
                    continue;

                double percent = earned / possible;   // 0.0 - 1.0
                weightedTotal += percent * cat.GradeWeight;
                weightSum += cat.GradeWeight;
            }

            if (weightSum == 0)
                return "--";

            double scaled = (weightedTotal / weightSum) * 100.0;

            if (scaled >= 93) return "A";
            if (scaled >= 90) return "A-";
            if (scaled >= 87) return "B+";
            if (scaled >= 83) return "B";
            if (scaled >= 80) return "B-";
            if (scaled >= 77) return "C+";
            if (scaled >= 73) return "C";
            if (scaled >= 70) return "C-";
            if (scaled >= 67) return "D+";
            if (scaled >= 63) return "D";
            if (scaled >= 60) return "D-";

            return "E";
        }

        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var classID = GetClassId(subject, num, season, year);
            if (classID == null) return Json(null);

            var query = from e in db.Enrolleds
                        join s in db.Students on e.UId equals s.UId
                        where e.ClassId == classID
                        select new
                        {
                            fname = s.FirstName,
                            lname = s.LastName,
                            uid = s.UId,
                            dob = s.Dob,
                            grade = e.Grade
                        };

            return Json(query.ToList());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            var classID = GetClassId(subject, num, season, year);
            if (classID == null) return Json(null);

            var query = from assignment in db.Assignments
                        join cat in db.AssignmentCatergories on assignment.CatId equals cat.CatId
                        where cat.ClassId == classID
                              && (category == null || cat.Name == category)
                        select new
                        {
                            aname = assignment.Name,
                            cname = cat.Name,
                            due = assignment.Due,
                            submissions = db.Submissions.Count(s => s.AssId == assignment.AssId)
                        };

            return Json(query.ToList());
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var classID = GetClassId(subject, num, season, year);
            if (classID == null) return Json(null);

            var query = db.AssignmentCatergories
                .Where(c => c.ClassId == classID)
                .Select(c => new
                {
                    name = c.Name,
                    weight = c.GradeWeight
                });


            return Json(query.ToList());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {

            var cID = (from c in db.Classes
                       join h in db.Courses on c.CourseId equals h.CourseId
                       join d in db.Departments on h.DId equals d.DeptId
                       where d.SubjectAbbrev == subject
                             && h.Number == num
                             && c.Season == season
                             && c.Year == year
                       select (int?)c.ClassId).FirstOrDefault();

            if (cID == null)
            {
                return Json(new { success = false });
            }

            var exists = db.AssignmentCatergories.Any(a =>
                a.ClassId == cID.Value && a.Name == category);

            if (exists)
            {
                return Json(new { success = false });
            }

            var assCat = new AssignmentCatergory
            {
                ClassId = cID.Value,
                Name = category,
                GradeWeight = (uint)catweight
            };

            db.AssignmentCatergories.Add(assCat);
            db.SaveChanges();

            return Json(new { success = true });

        }
            

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var cID = GetClassId(subject, num, season, year);
            if (cID == null) return Json(new { success = false });

            var cat = db.AssignmentCatergories
                .FirstOrDefault(c => c.ClassId == cID && c.Name == category);

            if (cat == null) return Json(new { success = false });

            var exists = db.Assignments.Any(a =>
                a.CatId == cat.CatId && a.Name == asgname);

            if (exists) return Json(new { success = false });

            var aNew = new Assignment
            {
                CatId = cat.CatId,
                Name = asgname,
                Points = (uint)asgpoints,
                Due = asgdue,
                Contents = asgcontents
            };

            db.Assignments.Add(aNew);
            db.SaveChanges();

            // update grades for all students
            var students = db.Enrolleds.Where(e => e.ClassId == cID).ToList();
            foreach (var s in students)
            {
                var grade = CalculateGrade(s.UId, cID.Value);
                var enr = db.Enrolleds.First(e => e.UId == s.UId && e.ClassId == cID);
                enr.Grade = grade;
            }

            db.SaveChanges();

            return Json(new { success = true });
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var aID = GetAssignmentId(subject, num, season, year, category, asgname);
            if (aID == null) return Json(null);

            var query = from s in db.Submissions
                        join st in db.Students on s.SId equals st.UId
                        where s.AssId == aID
                        select new
                        {
                            fname = st.FirstName,
                            lname = st.LastName,
                            uid = st.UId,
                            time = s.Date,
                            score = s.Score
                        };

            return Json(query.ToList());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var aID = GetAssignmentId(subject, num, season, year, category, asgname);
            var cID = GetClassId(subject, num, season, year);

            if (aID == null || cID == null)
                return Json(new { success = false });

            var sub = db.Submissions
                .FirstOrDefault(s => s.AssId == aID && s.SId == uid);

            if (sub == null)
                return Json(new { success = false });

            sub.Score = (uint)score;
            db.SaveChanges();

            // update grade
            var grade = CalculateGrade(uid, cID.Value);
            var enr = db.Enrolleds.First(e => e.UId == uid && e.ClassId == cID);
            enr.Grade = grade;

            db.SaveChanges();

            return Json(new { success = true });
        }


        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from c in db.Classes
                        join co in db.Courses on c.CourseId equals co.CourseId
                        join d in db.Departments on co.DId equals d.DeptId
                        where c.ProfessorId == uid
                        select new
                        {
                            subject = d.SubjectAbbrev,
                            number = co.Number,
                            name = co.Name,
                            season = c.Season,
                            year = c.Year
                        };

            return Json(query.ToList());
        }



        /*******End code to modify********/
    }
}

