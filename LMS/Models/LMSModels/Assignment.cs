using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Assignment
    {
        public Assignment()
        {
            Submissions = new HashSet<Submission>();
        }

        public int AssId { get; set; }
        public int CatId { get; set; }
        public string Name { get; set; } = null!;
        public uint Points { get; set; }
        public string Contents { get; set; } = null!;
        public DateTime Due { get; set; }

        public virtual AssignmentCatergory Cat { get; set; } = null!;
        public virtual ICollection<Submission> Submissions { get; set; }
    }
}
