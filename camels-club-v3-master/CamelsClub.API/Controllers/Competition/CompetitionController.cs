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
    public class CompetitionController : BaseController
    {
        private readonly IUnitOfWork _unit;
        private readonly ICompetitionService _competitionService;

        public CompetitionController(IUnitOfWork unit, ICompetitionService competitionService)
        {
            _unit = unit;
            _competitionService = competitionService;

        }

        [HttpGet]
        [AuthorizeUserFilter(Role = "User") ]
        [Route("api/Competition/GetList")]
        public ResultViewModel<PagingViewModel<CompetitionViewModel>> GetList(string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                ResultViewModel<PagingViewModel<CompetitionViewModel>> resultViewModel = new ResultViewModel<PagingViewModel<CompetitionViewModel>>();
                var userID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.Search(userID, orderBy, isAscending, pageIndex, pageSize, Language);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;

            }
            catch (Exception ex)
            {
                return new ResultViewModel<PagingViewModel<CompetitionViewModel>>(ex.Message);
            }
        }

        //[HttpGet]
        //[AuthorizeUserFilter(Role = "User")]
        //[Route("api/Competition/GetSponsors")]
        //public ResultViewModel<CompetitionViewModel> GetCompetitionSponsors(int competitionID)
        //{
        //    try
        //    {
        //        ResultViewModel<PagingViewModel<CompetitionViewModel>> resultViewModel = new ResultViewModel<PagingViewModel<CompetitionViewModel>>();
        //        var userID = int.Parse(UserID);
        //        resultViewModel.Data = _competitionService.GetSponsor()
        //        resultViewModel.Success = true;
        //        resultViewModel.Message = Resource.DataLoaded;
        //        return resultViewModel;

        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResultViewModel<PagingViewModel<CompetitionViewModel>>(ex.Message);
        //    }
        //}

        [HttpGet]
        [AuthorizeUserFilter(Role = "Admin")]
        [Route("api/Competition/GetAllCompetitions")]
        public ResultViewModel<PagingViewModel<CompetitionViewModel>> GetAllCompetitions(string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                ResultViewModel<PagingViewModel<CompetitionViewModel>> resultViewModel = new ResultViewModel<PagingViewModel<CompetitionViewModel>>();
                resultViewModel.Data = _competitionService.Search( 0, orderBy, isAscending, pageIndex, pageSize, Language);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;

            }
            catch (Exception ex)
            {
                return new ResultViewModel<PagingViewModel<CompetitionViewModel>>(ex.Message);
            }
           
        }

        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/GetCurrentInvolvedCompetitions")]
        public ResultViewModel<PagingViewModel<V2CompetitionViewModel>> GetCurrentInvolvedCompetitions(string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20)
        {
            try
            {
                ResultViewModel<PagingViewModel<V2CompetitionViewModel>> resultViewModel = new ResultViewModel<PagingViewModel<V2CompetitionViewModel>>();
                var userID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.GetCurrentInvolvedCompetitions(userID, orderBy, isAscending, pageIndex, pageSize, Language);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;

            }
            catch (Exception ex)
            {
                return new ResultViewModel<PagingViewModel<V2CompetitionViewModel>>(ex.Message);
            }
          
        }

        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/GetPublishedCompetitions")]
        public ResultViewModel<List<PublishedCompetitionViewModel>> GetPublishedCompetitions()
        {
            try
            {
                var resultViewModel = new ResultViewModel<List<PublishedCompetitionViewModel>>();
                var userID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.GetPublishedCompetitions(userID);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;

            }
            catch (Exception ex)
            {
                return new ResultViewModel<List<PublishedCompetitionViewModel>>(ex.Message);
            }

        }

        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/GetMyCompetitions")]
        public ResultViewModel<PagingViewModel<CompetitionViewModel>> GetMyCompetitions(string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20)
        {
            ResultViewModel<PagingViewModel<CompetitionViewModel>> resultViewModel = new ResultViewModel<PagingViewModel<CompetitionViewModel>>();
                var userID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.GetMyCompetitons(userID, orderBy, isAscending, pageIndex, pageSize, Language);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;
          }

        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/Publish")]
        public ResultViewModel<bool> Publish(int competitionID)
        {
            try
            {
                ResultViewModel<bool> resultViewModel = new ResultViewModel<bool>();
                var userID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.PublishCompetition(userID, competitionID);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;

            }
            catch (Exception ex)
            {
                return new ResultViewModel<bool>(ex.Message);
            }
            
        }

        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/Start")]
        public ResultViewModel<bool> Start(int competitionID)
        {
            try
            {
                ResultViewModel<bool> resultViewModel = new ResultViewModel<bool>();
                var userID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.StartCompetition(userID, competitionID);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;
            }
            catch (Exception ex)
            {
                return new ResultViewModel<bool>(ex.Message);
            }
        }

        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/StartRefereeing")]
        public ResultViewModel<bool> StartRefereeing(int competitionID)
        {
                ResultViewModel<bool> resultViewModel = new ResultViewModel<bool>();
                var userID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.StartRefereeing(userID, competitionID);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.UpdatedSuccessfully;
                return resultViewModel;
            
        }

        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/InviteCompetitors")]
        public ResultViewModel<bool> InviteCompetitors(int competitionID)
        {
            try
            {
                ResultViewModel<bool> resultViewModel = new ResultViewModel<bool>();
                var userID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.InviteCompetitors(userID, competitionID);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;

            }
            catch (Exception ex)
            {
                return new ResultViewModel<bool>(ex.Message);
            }
        }


        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/GetHelpingConditions")]
        public ResultViewModel<List<CompetitionGeneralConditionViewModel>> GetHelpingConditions()
        {
            try
            {
                ResultViewModel<List<CompetitionGeneralConditionViewModel>> resultViewModel = new ResultViewModel<List<CompetitionGeneralConditionViewModel>>();
                var userID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.GetHelpingConditions();
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;

            }
            catch (Exception ex)
            {
                return new ResultViewModel<List<CompetitionGeneralConditionViewModel>>(ex.Message);
            }
        }


        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/GetCompetitionConditions")]
        public ResultViewModel<List<CompetitionGeneralConditionViewModel>> GetCompetitionConditions(int competitionID)
        {
            try
            {
                ResultViewModel<List<CompetitionGeneralConditionViewModel>> resultViewModel = new ResultViewModel<List<CompetitionGeneralConditionViewModel>>();
                var userID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.GetCompetitionConditions(competitionID);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;

            }
            catch (Exception ex)
            {
                return new ResultViewModel<List<CompetitionGeneralConditionViewModel>>(ex.Message);
            }
        }

        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/User/GetSuspendCompetitionsCount")]
        public ResultViewModel<int> GetSuspendCompetitionsCount()
        {
            try
            {
                ResultViewModel<int> resultViewModel = new ResultViewModel<int>();
                var userID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.GetSuspendCompetitionsCount(userID);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;

            }
            catch (Exception ex)
            {
                return new ResultViewModel<int>(ex.Message);
            }
        }

        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/GetReferees")]
        public ResultViewModel<List<CompetitionRefereeViewModel>> GetReferees(int competitionID)
        {
            try
            {
                var resultViewModel = new ResultViewModel<List<CompetitionRefereeViewModel>>();
                var userID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.GetReferees(competitionID);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;

            }
            catch (Exception ex)
            {
                return new ResultViewModel<List<CompetitionRefereeViewModel>>(ex.Message);
            }
        }
        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/GetCheckers")]
        public ResultViewModel<List<CompetitionCheckerViewModel>> GetCheckers(int competitionID)
        {
            try
            {
                var resultViewModel = new ResultViewModel<List<CompetitionCheckerViewModel>>();
                var userID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.GetCheckers(competitionID);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;

            }
            catch (Exception ex)
            {
                return new ResultViewModel<List<CompetitionCheckerViewModel>>(ex.Message);
            }
        }

        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/GetCompetitors")]
        public ResultViewModel<List<CompetitionInviteViewModel>> GetInvites(int competitionID)
        {
            try
            {
                var resultViewModel = new ResultViewModel<List<CompetitionInviteViewModel>>();
                var userID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.GetCompetitors(competitionID);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;

            }
            catch (Exception ex)
            {
                return new ResultViewModel<List<CompetitionInviteViewModel>>(ex.Message);
            }
        }

        [HttpPost]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/ChangeRefereeBoss")]
        public ResultViewModel<bool> ChangeRefereeBoss(ChangeRefereeBossCreateViewModel viewModel)
        {
            try
            { 
                ResultViewModel<bool> resultViewModel = new ResultViewModel<bool>();
                var userID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.ChangeRefereeBoss(viewModel);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;
            }
            catch (Exception ex)
            {
                return new ResultViewModel<bool>(ex.Message);
            }

        }

        [HttpPost]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/AddNewCheckerBoss")]
        public ResultViewModel<bool> AddNewCheckerBoss(NewCheckerBossCreateViewModel viewModel)
        {
            try
            {
                ResultViewModel<bool> resultViewModel = new ResultViewModel<bool>();
                var userID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.AddNewCheckerBoss(viewModel);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;
            }
            catch (Exception ex)
            {
                return new ResultViewModel<bool>(ex.Message);
            }

        }

        [HttpPost]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/AddNewRefereeBoss")]
        public ResultViewModel<bool> AddNewRefereeBoss(NewRefereeBossCreateViewModel viewModel)
        {
            try
            {
                ResultViewModel<bool> resultViewModel = new ResultViewModel<bool>();
                var userID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.AddNewRefereeBoss(viewModel);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;
            }
            catch (Exception ex)
            {
                return new ResultViewModel<bool>(ex.Message);
            }

        }

        [HttpPost]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/ChangeRefereeBoss")]
        public ResultViewModel<bool> ChangeCheckerBoss(ChangeCheckerBossCreateViewModel viewModel)
        {
            try 
            { 

                ResultViewModel<bool> resultViewModel = new ResultViewModel<bool>();
                var userID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.ChangeCheckerBoss(viewModel);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;
            }
            catch (Exception ex)
            {
                return new ResultViewModel<bool>(ex.Message);
            }

        }
        [HttpPost]
        [AuthorizeUserFilter(Role = "User") ]
        [Route("api/Competition/Add")]
     //   [ValidateViewModel]
        public ResultViewModel<bool> Add(CompetitionCreateViewModel viewModel)
        {
            ResultViewModel<bool> resultViewModel = new ResultViewModel<bool>();
            
                if (!ModelState.IsValid)
                {
                    var res = new ResultViewModel<bool>
                    {
                        Errors = APIHelper.ValidationMessages(ModelState),
                        Success = false,
                        Message = "Invalid Request"
                    };
                
                    return res;
                }
                viewModel.UserID = int.Parse(UserID);
                resultViewModel.Data = _competitionService.Add(viewModel);
                _unit.Save();
                if (viewModel.Image != null)
                {
                    FileHelper.MoveFileFromTempPathToAnotherFolder(viewModel.Image, "Competition-Document");

                }
                if(viewModel.Rewards != null)
                {
                    var sponsorLogos = viewModel.Rewards.Select(x => x.Logo).Where(x=> !string.IsNullOrWhiteSpace(x)).ToList();
                    foreach (var item in sponsorLogos)
                    {
                        FileHelper.MoveFileFromTempPathToAnotherFolder(item , "Competition-Document");

                    }
                }
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.AddedSuccessfully;

                return resultViewModel;

              
        }

      


        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/GetByID")]
        public ResultViewModel<V2CompetitionViewModel> GetByID(int Id)
        {
            try
            {
            ResultViewModel<V2CompetitionViewModel> resultViewModel = new ResultViewModel<V2CompetitionViewModel>();

                if(_competitionService.IsExists(Id))
                {
                    var userID = int.Parse(UserID);
                    var res = _competitionService.GetByID(userID , Id);
                    resultViewModel.Success = true;
                    resultViewModel.Data = res;
                    resultViewModel.Message = Resource.DataLoaded;
                }
                else
                {
                    resultViewModel.Success = false;
                    resultViewModel.Message = Resource.NotFound;
                }
              

                return resultViewModel;
            }
            catch (Exception ex)
            {
                return new ResultViewModel<V2CompetitionViewModel>(ex.Message);
            }

        }

        [HttpGet]
        [AuthorizeUserFilter(Role = "Admin")]
        [Route("api/Competition/GetCompetitionByID")]
        public ResultViewModel<V2CompetitionViewModel> GetCompetitionByID(int Id)
        {
            try
            {
                ResultViewModel<V2CompetitionViewModel> resultViewModel = new ResultViewModel<V2CompetitionViewModel>();

                if (_competitionService.IsExists(Id))
                {
                    var userID = int.Parse(UserID);
                    var res = _competitionService.GetByID(userID, Id);
                    resultViewModel.Success = true;
                    resultViewModel.Data = res;
                    resultViewModel.Message = Resource.DataLoaded;
                }
                else
                {
                    resultViewModel.Success = false;
                    resultViewModel.Message = Resource.NotFound;
                }


                return resultViewModel;
            }
            catch (Exception ex)
            {
                return new ResultViewModel<V2CompetitionViewModel>(ex.Message);
            }

        }

        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Competition/GetEditableByID")]
        public ResultViewModel<CompetitionEditViewModel> GetEditableByID(int Id)
        {
            try 
            { 
                ResultViewModel<CompetitionEditViewModel> resultViewModel = new ResultViewModel<CompetitionEditViewModel>();
                if (_competitionService.IsExists(Id))
                {
                    var res = _competitionService.GetEditableByID(Id);
                    resultViewModel.Success = true;
                    resultViewModel.Data = res;
                    resultViewModel.Message = Resource.DataLoaded;
                }
                else
                {
                    resultViewModel.Success = false;
                    resultViewModel.Message = Resource.NotFound;
                }


                return resultViewModel;
            }
            catch (Exception ex)
            {
                return new ResultViewModel<CompetitionEditViewModel>(ex.Message);
            }

        }


        //  [HttpPut]
        //  [AuthorizeUserFilter(Role = "User")]
        //  [Route("api/Competition/Edit")]
        //  [ValidateViewModel]
        //  public ResultViewModel Edit(CompetitionEditViewModel viewModel)
        //  {
        //      try
        //      {
        //          ResultViewModel resultViewModel = new ResultViewModel();
        //          viewModel.UserID = int.Parse(UserID);
        //          if(_competitionService.IsExists(viewModel.ID) && _competitionService.IsAllowedToEdit(viewModel.ID))
        //          {

        //              _competitionService.Edit(viewModel);
        //              _unit.Save();
        //              resultViewModel.Data = viewModel;
        //              resultViewModel.Message=Resource.UpdatedSuccessfully;
        //              resultViewModel.Success = true;


        //          }
        //          else
        //          {
        //              resultViewModel.Success = false;
        //              resultViewModel.Message = Resource.InvitedUsersJoined;
        //          }

        //          return resultViewModel;

        //      }
        //      catch (Exception ex)
        //      {
        //          return new ResultViewModel(string.Empty, ex.Message, false);
        //      }
        //         }

        //  [HttpDelete]
        //  [AuthorizeUserFilter(Role = "User")]
        //  [Route("api/Competition/Delete")]
        //  public ResultViewModel DeleteCompition(int Id)
        //{
        //      ResultViewModel resultViewModel = new ResultViewModel();
        //      try
        //      {    
        //          if(_competitionService.IsExists(Id))
        //          {
        //              _competitionService.Delete(Id);
        //              _unit.Save();
        //              resultViewModel.Success = true;
        //              resultViewModel.Message = Resource.DataDeleted;
        //          }
        //          else
        //          {
        //              resultViewModel.Success = false;
        //              resultViewModel.Message = Resource.NotFound;
        //          }

        //          return resultViewModel;

        //      }
        //      catch (Exception ex)
        //      {
        //          return new ResultViewModel(null, ex.Message, false);
        //      }

        //  }
    }
}
