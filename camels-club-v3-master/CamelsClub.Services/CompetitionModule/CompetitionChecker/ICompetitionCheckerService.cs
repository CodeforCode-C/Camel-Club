using CamelsClub.ViewModels;
using CamelsClub.ViewModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamelsClub.Services
{
    public interface ICompetitionCheckerService
    {
        bool ApproveGroups(List<int> groupIDs, int competitionID);
        PagingViewModel<CompetitionCheckerViewModel> Search(int userID=0,string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic);
        List<GroupViewModel> GetGroupsForFinalFilter(int competitionID, int loggedUserID);
        bool AddNewMembers(List<CompetitionCheckerPickupViewModel> viewModels, int loggedUserID);
        bool AddNewCheckersToTeam(List<CompetitionCheckerPickupViewModel> viewModels, int loggedUserID);

        void Add(CompetitionCheckerCreateViewModel view);
        bool HasFilter(int competitionID);
        void Edit(CompetitionCheckerCreateViewModel viewModel);
        void Delete(int id);
        CompetitionCheckerViewModel GetByID(int id);
        bool IsExists(int id);
        bool JoinCompetition(CheckerJoinCompetitionCreateViewModel viewModel);
        bool RejectCompetition(CheckerJoinCompetitionCreateViewModel viewModel);
        bool HasJoinedCompetition(CheckerJoinCompetitionCreateViewModel viewModel);
        CheckersReportViewModel GetTeam(CheckerJoinCompetitionCreateViewModel viewModel);
        bool PickupTeam(List<CompetitionCheckerPickupViewModel> viewModels, int loggedUserID);
        bool AutoAllocate(CheckerJoinCompetitionCreateViewModel viewModel);
        bool ManualAllocate(List<ManualAllocateCreateViewModel> viewModels);
        List<GroupViewModel> GetGroups(int competitionID, int loggedUserID);
        List<CamelCompetitionViewModel> GetCamels(int competitionID, int groupID, int loggedUserID);
        bool EvaluateCamel(CheckerApproveCreateViewModel viewModel);
        bool ChangeChecker(ChangeCheckerCreateViewModel viewModel);
        ApproveGroupResultViewModel ApproveGroup(ApproveGroupCreateViewModel viewModel);
        bool RejectGroup(ApproveGroupCreateViewModel viewModel);
        bool ReviewApprove(ReviewApproveRequestCreateViewModel viewModel, int loggedUserID);
        List<ReviewApproveViewModel> GetReviewRequests(int competitionID, int loggedUserID);
        bool ReplaceChecker(CheckerReplaceCreateViewModel viewModel);
        bool ReplaceCamel(ReplaceCamelRequestCreateViewModel viewModel);
        bool AddApproveReview(ReviewApproveCreateViewModel viewModel, int loggedUserID);
        CheckersReportViewModel GetPickedCheckers(int competitionID, int loggedUserID);
        bool ApproveReplacedCamel(int loggedUserID, int camelCompetitionID);
        List<CompetitionCheckerViewModel> GetAvailableCheckers(int loggedUserID, int competitionID);
    }
}
