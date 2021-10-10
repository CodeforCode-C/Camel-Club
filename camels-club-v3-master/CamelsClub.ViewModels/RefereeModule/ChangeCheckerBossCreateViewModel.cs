using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamelsClub.ViewModels
{
    public class ChangeCheckerBossCreateViewModel
    {
        public int CompetitionID { get; set; }
        public int NewCheckerID { get; set; }
        public int OldCheckerID { get; set; }
    }

    public class NewCheckerBossCreateViewModel
    {
        public int CompetitionID { get; set; }
        public int UserID { get; set; }
    }

    public class NewRefereeBossCreateViewModel
    {
        public int CompetitionID { get; set; }
        public int UserID { get; set; }
    }

}
