using CamelsClub.ViewModels;
using System;
using System.Collections.Generic;

namespace CamelsClub.Services
{
    public class V2CompetitionViewModel
    {
        public int ID { get; set; }
        public string NameArabic { get; set; }
        public string Address { get; set; }
        public string NameEnglish { get; set; }
        public string CategoryNameArabic { get; set; }
        public string CategoryNameEnglish { get; set; }
        public DateTime? Published { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool ShowReferees { get; set; }
        public bool ShowCheckers { get; set; }
        public bool ShowCompetitors { get; set; }
        public bool IsCheckerPickupTeam { get; set; }
        public bool IsRefereePickupTeam { get; set; }
        public string UserName { get; set; }
        public string ImagePath { get; set; }
        public int CompetitionType { get; set; }
        public bool IsCheckersAllocated { get; set; }
        public bool IsRefereesAllocated { get; set; }
        public int MinimumCheckersCount { get; set; }
        public int MaximumCheckersCount { get; set; }
        public int MaximumRefereesCount { get; set; }
        public int MinimumRefereesCount { get; set; }
        public int MinimumCompetitorsCount { get; set; }
        public int MaximumCompetitorsCount { get; set; }
        public DateTime? Completed { get; set; }
        public bool IsCompetitorsInvited { get; set; }
        public DateTime? StartedDate { get; set; }
        public int UserID { get; set; }
        public string ConditionText { get; set; }
        public bool IsChecker { get; set; }
        public bool IsCheckerBoss { get; set; }
        public bool IsReferee { get; set; }
        public bool IsBossReferee { get; set; }
        public bool IsCompetitor { get; set; }
        public bool HasPicked { get; set; }
        public bool HasJoined { get; set; }
        public bool HasRejected { get; set; }
        public IEnumerable<CompetitionRewardViewModel> Rewards { get; set; }
        public bool IsCheckBossRejected { get; set; }
        public bool IsRefereeBossRejected { get; set; }
        public int CheckingCompletionPercentage { get; set; }
        public int RefereeingCompletionPercentage { get; set; }
        public ModuleComplateViewModel ModuleCompletion { get; set; }
        public List<CompetitionWinnerViewModel> Winners { get; set; }
        public bool Created { get;set; }
        public DateTime? CheckerPickupTeamDateTime { get; set; }
        public DateTime? RefereePickupTeamDateTime { get; set; }
        public DateTime To { get; set; }
        public DateTime From { get; set; }
        public int CamelsCount { get; set; }
        public string RefereeBossName { get; set; }
        public string CheckerBossName { get; set; }
        public List<CompetitionTeamRewardViewModel> TeamRewards { get; internal set; }
        public int VoteType { get; set; }
        public bool CheckingIsFinished { get; set; }
    }
}