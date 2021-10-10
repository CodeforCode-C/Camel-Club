using CamelsClub.ViewModels;
using System.Collections.Generic;

namespace CamelsClub.Services
{
    public class RefereesReportViewModel
    {
        public int TotalNumberOfAllCamels { get; set; }
        public int TotalNumberOfEvaluatedCamels { get; set; }
        public int CheckingCompletitionRatio { get; set; }
        public List<CompetitionRefereeViewModel> Referees { get; set; }
        public int MaximumRefereesCount { get; internal set; }
        public int MinimumRefereesCount { get; internal set; }
        public bool IsTeamPicked { get; internal set; }
        public bool IsAnyMemberRejected { get; internal set; }
        public bool AllRefereesAccepted { get; internal set; }
        public bool IsReplaceAllowed { get; internal set; }
    }
}