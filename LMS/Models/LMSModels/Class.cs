using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Class
    {
        public Class()
        {
            AssignmentCatergories = new HashSet<AssignmentCatergory>();
            Enrolleds = new HashSet<Enrolled>();
        }

        public int ClassId { get; set; }
        public int CourseId { get; set; }
        public string ProfessorId { get; set; } = null!;
        public uint Year { get; set; }
        public string Season { get; set; } = null!;
        public string Location { get; set; } = null!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public virtual Course Course { get; set; } = null!;
        public virtual Professor Professor { get; set; } = null!;
        public virtual ICollection<AssignmentCatergory> AssignmentCatergories { get; set; }
        public virtual ICollection<Enrolled> Enrolleds { get; set; }
    }
}
