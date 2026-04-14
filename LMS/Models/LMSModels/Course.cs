using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Course
    {
        public Course()
        {
            Classes = new HashSet<Class>();
        }

        public int CourseId { get; set; }
        public string Name { get; set; } = null!;
        public int Number { get; set; }
        public int DId { get; set; }

        public virtual Department DIdNavigation { get; set; } = null!;
        public virtual ICollection<Class> Classes { get; set; }
    }
}
