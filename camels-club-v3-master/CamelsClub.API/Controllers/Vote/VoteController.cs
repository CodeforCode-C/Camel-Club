using CamelsClub.API.Filters;
using CamelsClub.Data.UnitOfWork;
using CamelsClub.Localization.Shared;
using CamelsClub.Services;
using CamelsClub.Services.Helpers;
using System;
using CamelsClub.ViewModels;

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CamelsClub.API.Controllers
{
    public class VoteController : BaseController
    {

        private readonly IUnitOfWork _unit;
        private readonly ICamelService _camelService;
        private readonly IVoteService _voteService ;
        public VoteController(IUnitOfWork unit, ICamelService camelService , IVoteService voteService)
        {
            _unit = unit;
            _camelService = camelService;
            _voteService = voteService;

        }


        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/vote/GetCompetitons")]
        public ResultViewModel <List<CompetitionViewModel>> GetCompetitons()  {
            var resultViewModel = new ResultViewModel<List<CompetitionViewModel>>();
            resultViewModel.Data  = _voteService.GetCompetitons();

            resultViewModel.Success = true;
            resultViewModel.Message = Resource.DataLoaded;

            return resultViewModel;
        }

        

        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/vote/GetCompetitonsGroup")]
        public ResultViewModel<List<GroupViewModel>> GetCompetitionGroups(int competitionID)
        {
            var resultViewModel = new ResultViewModel<List<GroupViewModel>>();
            resultViewModel.Data = _voteService.GetCompetitionGroups(competitionID);

            resultViewModel.Success = true;
            resultViewModel.Message = Resource.DataLoaded;

            return resultViewModel;
        }



        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/vote/GetCompetitionCompetitors")]
        public ResultViewModel<List<CompetitionInviteViewModel>> GetCompetitionCompetitors(int competitionID)
        {
            var resultViewModel = new ResultViewModel<List<CompetitionInviteViewModel>>();
            resultViewModel.Data = _voteService.GetCompetitionCompetitors(competitionID);

            resultViewModel.Success = true;
            resultViewModel.Message = Resource.DataLoaded;

            return resultViewModel;
        }

        [HttpPost]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/vote/Add")]
        public ResultViewModel<bool> AddVote(UserCamelReviewCreateViewModel viewModel)
        {
            var resultViewModel = new ResultViewModel<bool>();
            viewModel.UserID = int.Parse(UserID);
            resultViewModel.Data = _voteService.EvaluateCamel(viewModel);
            resultViewModel.Success = true;
            resultViewModel.Message = Resource.DataLoaded;
            return resultViewModel;
        }

        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/vote/GetCompetitorGroups")]
        public ResultViewModel<List<GroupViewModel>> GetCompetitorGroups(int competitionID, int competitorID)
        {
            var resultViewModel = new ResultViewModel<List<GroupViewModel>>();
            resultViewModel.Data = _voteService.GetCompetitorGroups(competitionID , competitorID);

            resultViewModel.Success = true;
            resultViewModel.Message = Resource.DataLoaded;

            return resultViewModel;
        }

        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/vote/GetGroupCamels")]
        public ResultViewModel<List<VoteCamelCompetitionViewModel>> GetGroupCamels(int competitionID, int groupId)
        {
            var resultViewModel = new ResultViewModel<List<VoteCamelCompetitionViewModel>>();
            resultViewModel.Data = _voteService.GetGroupCamels(competitionID, groupId, int.Parse(UserID));

            resultViewModel.Success = true;
            resultViewModel.Message = Resource.DataLoaded;

            return resultViewModel;
        }

    }
}
