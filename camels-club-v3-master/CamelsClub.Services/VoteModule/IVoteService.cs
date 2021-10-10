using CamelsClub.ViewModels;
using CamelsClub.ViewModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamelsClub.Services
{
    public interface IVoteService
    {
        // GetCompetitorGroups by competitionID & competitorID

        List<GroupViewModel> GetCompetitorGroups(int competitionID, int competitorID);
        
        /// Get Competition that can be voted, must be started and not finished

        List<CompetitionViewModel> GetCompetitons();
        // get groups in compitions

        List<GroupViewModel> GetCompetitionGroups(int competitionID);
        
        //  Get Competitors in Competition that voted
        List<CompetitionInviteViewModel> GetCompetitionCompetitors(int competitionID);

        bool EvaluateCamel(UserCamelReviewCreateViewModel viewModel);

        List<VoteCamelCompetitionViewModel> GetGroupCamels(int competitionID, int groupID, int loggedUserId);


    }
}
