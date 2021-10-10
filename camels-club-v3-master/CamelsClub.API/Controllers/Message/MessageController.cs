using CamelsClub.API.Filters;
using CamelsClub.Data.UnitOfWork;
using CamelsClub.Localization.Shared;
using CamelsClub.Services;
using CamelsClub.Services.Helpers;
using CamelsClub.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace CamelsClub.API.Controllers.Message
{
    public class MessageController : BaseController
    {
        private readonly IUnitOfWork _unit;
        private readonly IMessageService _messageService;

        public MessageController(IUnitOfWork unit, IMessageService messageService)
        {
            _unit = unit;
            _messageService = messageService;

        }
        //[HttpGet]
        //[Route("api/GetCurrentDate")]
        //public DateTime GetCurrentDate()
        //{
        //    return DateTime.Now.ToLocalTime();
        //}

        [HttpGet]
        [AuthorizeUserFilter(Role ="User")]
        [Route("api/User/GetReceivedMessage")]
        public ResultViewModel<PagingViewModel<MessageViewModel>> GetMessages(int FromUserID , string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20)
        {
            ResultViewModel<PagingViewModel<MessageViewModel>> resultViewModel = new ResultViewModel<PagingViewModel<MessageViewModel>>();
                var loggedUserID = int.Parse(UserID);
                resultViewModel.Data = _messageService.GetReceivedMessage(FromUserID,loggedUserID, orderBy, isAscending, pageIndex, pageSize, Language);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;
          }

        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Chat/GetUsers")]
        public ResultViewModel<List<UserChatViewModel>> GetUsersHasChatWithLoggedUser()
        {
            ResultViewModel<List<UserChatViewModel>> resultViewModel = new ResultViewModel<List<UserChatViewModel>>();
                var loggedUserID = int.Parse(UserID);
                resultViewModel.Data = _messageService.GetUsersHasChatWithLoggedUser(loggedUserID);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;
           
        }

        [HttpGet]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/Chat/GetUnSeenMessagesCount")]
        public ResultViewModel<int> GetUnSeenMessagesCount()
        {
            ResultViewModel<int> resultViewModel = new ResultViewModel<int>();
                var loggedUserID = int.Parse(UserID);
                resultViewModel.Data = _messageService.GetUnSeenMessagesCount(loggedUserID);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel; 
        }


        [HttpPost]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/User/SendMessage")]
        public ResultViewModel<bool> SendMessage(MessageCreateViewModel viewModel)
        {
            ResultViewModel<bool> resultViewModel = new ResultViewModel<bool>();
                viewModel.FromUserID = int.Parse(UserID);
                resultViewModel.Data = _messageService.Send(viewModel);
                _unit.Save();
               
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.UpdatedSuccessfully;
                return resultViewModel;
         }

        [HttpPost]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/User/SendImage")]
        public ResultViewModel<bool> SendImage(ImageMessageCreateViewModel viewModel)
        {
            ResultViewModel<bool> resultViewModel = new ResultViewModel<bool>();
            viewModel.FromUserID = int.Parse(UserID);
            resultViewModel.Data = _messageService.SendImage(viewModel);
            _unit.Save();
         
            FileHelper.MoveFileFromTempPathToAnotherFolder(viewModel.ImageName, "User-Document");
        
            resultViewModel.Success = true;
            resultViewModel.Message = Resource.AddedSuccessfully;
            return resultViewModel;
        }

        [HttpPost]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/User/SendImages")]
        public ResultViewModel<bool> SendImages(ImagesMessageCreateViewModel viewModel)
        {
            ResultViewModel<bool> resultViewModel = new ResultViewModel<bool>();
            viewModel.FromUserID = int.Parse(UserID);
            resultViewModel.Data = _messageService.SendImages(viewModel);
            _unit.Save();
            foreach (var item in viewModel.ImagesName)
            {
                FileHelper.MoveFileFromTempPathToAnotherFolder(item, "User-Document");

            }

            resultViewModel.Success = true;
            resultViewModel.Message = Resource.AddedSuccessfully;
            return resultViewModel;
        }

        [HttpPost]
        [AuthorizeUserFilter(Role = "User")]
        [Route("api/User/SendVideo")]
        public ResultViewModel<bool> SendVideo(VideoMessageCreateViewModel viewModel)
        {
            ResultViewModel<bool> resultViewModel = new ResultViewModel<bool>();
            viewModel.FromUserID = int.Parse(UserID);
            resultViewModel.Data = _messageService.SendVideo(viewModel);
            _unit.Save();

            FileHelper.MoveFileFromTempPathToAnotherFolder(viewModel.VideoName, "User-Document");

            resultViewModel.Success = true;
            resultViewModel.Message = Resource.AddedSuccessfully;
            return resultViewModel;
        }

    }


}
