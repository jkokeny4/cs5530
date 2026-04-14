using System;
using System.Collections.Generic;

namespace LMS.Models.LMSModels
{
    public partial class Submission
    {
        public int SubId { get; set; }
        public DateTime Date { get; set; }
        public uint? Score { get; set; }
        public string? Contents { get; set; }
        public string SId { get; set; } = null!;
        public int AssId { get; set; }

        public virtual Assignment Ass { get; set; } = null!;
        public virtual Student SIdNavigation { get; set; } = null!;
    }
}
