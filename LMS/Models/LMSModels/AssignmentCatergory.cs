using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class AssignmentCatergory
    {
        public AssignmentCatergory()
        {
            Assignments = new HashSet<Assignment>();
        }

        public int CatId { get; set; }
        public string Name { get; set; } = null!;
        public int ClassId { get; set; }
        public uint GradeWeight { get; set; }

        public virtual Class Class { get; set; } = null!;
        public virtual ICollection<Assignment> Assignments { get; set; }
    }
}
