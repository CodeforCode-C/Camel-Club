using CamelsClub.API.Filters;
using CamelsClub.API.Helpers;
using CamelsClub.Data.UnitOfWork;
using CamelsClub.Localization.Shared;
using CamelsClub.Services;
using CamelsClub.Services.Helpers;
using CamelsClub.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace CamelsClub.API.Controllers
{
    public class CamelCompetitionController : BaseController
    {
        private readonly IUnitOfWork _unit;
        private readonly ICamelCompetitionService _camelCompetitionService;

        public CamelCompetitionController(IUnitOfWork unit, ICamelCompetitionService camelCompetitionService)
        {
            _unit = unit;
            _camelCompetitionService = camelCompetitionService;

        }

      
        [HttpPost]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/Involve")]
        [ValidateViewModel]
        public ResultViewModel<bool> Involve(JoinCompetitionCreateViewModel viewModel)
        {
            ResultViewModel<bool> resultViewModel = new ResultViewModel<bool>();
            try
            {
                if (_camelCompetitionService.IsGroupExistInCompetition(viewModel.CompetitionID, viewModel.GroupID))
                {
                    return new ResultViewModel<bool>("this group is already added");
                }
                viewModel.UserID = int.Parse(UserID);
                resultViewModel.Data = _camelCompetitionService.AddGroup(viewModel);
                resultViewModel.Message = Resource.AddedSuccessfully;
                resultViewModel.Success = true;
                return resultViewModel;

            }
            catch (Exception ex)
            {
                return new ResultViewModel<bool>(ex.Message);
            }

        }

        [HttpPost]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competitor/ReplaceCamel")]
        [ValidateViewModel]
        public ResultViewModel<bool> ReplaceCamel(ReplaceCamelCompetitionCreateViewModel viewModel)
        {
            ResultViewModel<bool> resultViewModel = new ResultViewModel<bool>();
            try
            {
                viewModel.UserID = int.Parse(UserID);
                resultViewModel.Data = _camelCompetitionService.ReplaceCamel(viewModel);
                resultViewModel.Message = Resource.UpdatedSuccessfully;
                resultViewModel.Success = true;
                return resultViewModel;

            }
            catch (Exception ex)
            {
                return new ResultViewModel<bool>(ex.Message);
            }


        }

        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competitor/GetCamels")]
        public ResultViewModel<List<UserCamelCompetitionViewModel>> GetCompetitorCamels(int competitionID)
        {
            try
            {
                var resultViewModel = new ResultViewModel<List<UserCamelCompetitionViewModel>>();
                var loggedUserID = int.Parse(UserID);
                resultViewModel.Data = _camelCompetitionService.GetCompetitorCamels(loggedUserID, competitionID);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;

            }
            catch (Exception ex)
            {
                return new ResultViewModel<List<UserCamelCompetitionViewModel>>(ex.Message);
            }

        }

        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competitor/GetAvailableCamels")]
        public ResultViewModel<List<CamelViewModel>> GetAvailableCamels()
        {
            try
            {
                var resultViewModel = new ResultViewModel<List<CamelViewModel>>();
                var loggedUserID = int.Parse(UserID);
                resultViewModel.Data = _camelCompetitionService.GetAvailableCamels(loggedUserID);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;

            }
            catch (Exception ex)
            {
                return new ResultViewModel<List<CamelViewModel>>(ex.Message);
            }

        }


        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competitor/HasCamelsToReplace")]
        public ResultViewModel<bool> HasCamelsToReplace(int competitionID)
        {
            try
            {
                var resultViewModel = new ResultViewModel<bool>();
                var loggedUserID = int.Parse(UserID);
                resultViewModel.Data = _camelCompetitionService.HasCamelsToReplace(loggedUserID, competitionID);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;

            }
            catch (Exception ex)
            {
                return new ResultViewModel<bool>(ex.Message);
            }

        }

    }
}
