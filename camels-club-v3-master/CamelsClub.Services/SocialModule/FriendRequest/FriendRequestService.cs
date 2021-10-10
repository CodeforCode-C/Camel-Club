using CamelsClub.Data.Extentions;
using CamelsClub.Data.UnitOfWork;
using CamelsClub.Models;
using CamelsClub.Repositories;
using CamelsClub.ViewModels;
using CamelsClub.ViewModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CamelsClub.Services
{
    public class FriendRequestService : IFriendRequestService
    {
        private readonly IUnitOfWork _unit;
        private readonly IFriendRequestRepository _repo;
        private readonly IFriendRepository _friendRepository;
        private readonly INotificationService _notificationService;

        public FriendRequestService(IUnitOfWork unit,
                                    IFriendRequestRepository repo ,
                                    IFriendRepository friendRepository,
                                    INotificationService notificationService)
        {
            _unit = unit;
            _repo = repo;
            _friendRepository = friendRepository;
                  _notificationService = notificationService;
        }
        // get my receivedfriendRequests
        public PagingViewModel<FriendRequestViewModel> Search(int userID , string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var query = _repo.GetAll()
                .Where(x => !x.IsDeleted)
                .Where(x=> x.ToUserID == userID)
                .Where(x => x.Status == (int)FriendRequestStatus.Pending);




            int records = query.Count();
            if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
            int pages = (int)Math.Ceiling((double)records / pageSize);
            int excludedRows = (pageIndex - 1) * pageSize;

            List<FriendRequestViewModel> result = new List<FriendRequestViewModel>();

            var requests = query.Select(obj => new FriendRequestViewModel
            {
                ID = obj.ID,
                ToUserMainImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + obj.ToUser.UserProfile.MainImage,
                ToUserName = obj.ToUser.UserName,
                Notes = obj.Notes ,
                Status = obj.Status
            }).OrderByPropertyName(orderBy, isAscending);

            result = requests.Skip(excludedRows).Take(pageSize).ToList();
            return new PagingViewModel<FriendRequestViewModel>() { PageIndex = pageIndex, PageSize = pageSize, Result = result, Records = records, Pages = pages };
        }

        public PagingViewModel<SentFriendRequestViewModel> GetSentFriendRequests(int userID = 0, string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var query = _repo.GetAll()
                .Where(x => x.ActionBy == userID)
                .Where(x=> x.Status == (int)FriendRequestStatus.Pending);
            

            int records = query.Count();
            if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
            int pages = (int)Math.Ceiling((double)records / pageSize);
            int excludedRows = (pageIndex - 1) * pageSize;

            List<SentFriendRequestViewModel> result = new List<SentFriendRequestViewModel>();

            var requests = query.Select(obj => new WholeFriendRequestViewModel
            {
                ID = obj.ID,
                ToUserMainImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + obj.ToUser.UserProfile.MainImage,
                ToUserName = obj.ToUser.UserName,
                ToDisplayName = obj.ToUser.DisplayName,
                ToUserID = obj.ToUserID,
                FromUserMainImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + obj.FromUser.UserProfile.MainImage,
                FromUserName = obj.FromUser.UserName,
                FromDisplayName = obj.FromUser.DisplayName,
                FromUserID = obj.FromUserID,
                Notes = obj.Notes
            }).OrderByPropertyName(orderBy, isAscending)
             .Skip(excludedRows).Take(pageSize).ToList();

            foreach (var item in requests)
            {
                if(item.ToUserID == userID)
                {
                    result.Add(new SentFriendRequestViewModel
                    {
                        ID = item.ID,
                        ToUserMainImagePath = item.FromUserMainImagePath,
                        ToUserName = item.FromUserName,
                        ToDisplayName = item.FromDisplayName,
                        ToUserID = item.FromUserID
                    });

                }
                else
                {
                    result.Add(new SentFriendRequestViewModel
                    {
                        ID = item.ID,
                        ToUserMainImagePath = item.ToUserMainImagePath,
                        ToUserName = item.ToUserName,
                        ToDisplayName = item.ToDisplayName,
                        ToUserID = item.ToUserID
                    });

                }
            }
            return new PagingViewModel<SentFriendRequestViewModel>() { PageIndex = pageIndex, PageSize = pageSize, Result = result, Records = records, Pages = pages };
        }

        public PagingViewModel<ReceivedFriendRequestViewModel> GetReceivedFriendRequests(int userID = 0, string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var query = _repo.GetAll()
                .Where(x => x.FromUserID == userID || x.ToUserID == userID)
                .Where(x => x.ActionBy != userID)
                .Where(x=> x.Status == (int)FriendRequestStatus.Pending);



            int records = query.Count();
            if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
            int pages = (int)Math.Ceiling((double)records / pageSize);
            int excludedRows = (pageIndex - 1) * pageSize;

            List<ReceivedFriendRequestViewModel> result = new List<ReceivedFriendRequestViewModel>();

            var requests = query.Select(obj => new WholeFriendRequestViewModel
            {
                ID = obj.ID,
                FromUserMainImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + obj.FromUser.UserProfile.MainImage,
                FromUserName = obj.FromUser.UserName,
                FromDisplayName = obj.FromUser.DisplayName,
                FromUserID = obj.FromUserID,
                ToUserMainImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + obj.ToUser.UserProfile.MainImage,
                ToUserName = obj.ToUser.UserName,
                ToDisplayName = obj.ToUser.DisplayName,
                ToUserID = obj.ToUserID,
            }).OrderByPropertyName(orderBy, isAscending)
                .Skip(excludedRows).Take(pageSize).ToList();

            foreach (var item in requests)
            {
                if(item.FromUserID == userID)
                {
                    result.Add(new ReceivedFriendRequestViewModel
                    {
                        ID = item.ID,
                        FromUserID = item.ToUserID,
                        FromDisplayName = item.ToDisplayName,
                        FromUserMainImagePath = item.ToUserMainImagePath,
                        FromUserName = item.ToUserName
                    });
                }
                else
                {
                    result.Add(new ReceivedFriendRequestViewModel
                    {
                        ID = item.ID,
                        FromUserID = item.FromUserID,
                        FromDisplayName = item.FromDisplayName,
                        FromUserMainImagePath = item.FromUserMainImagePath,
                        FromUserName = item.FromUserName
                    });
                }
            }
            return new PagingViewModel<ReceivedFriendRequestViewModel>() { PageIndex = pageIndex, PageSize = pageSize, Result = result, Records = records, Pages = pages };
        }

        public bool ApproveFriendRequest(FriendCreateViewModel viewModel)
        {
            var loggedUserID = viewModel.UserID;
            var WhoSentRequestID = viewModel.FriendUserID;
            if (viewModel.UserID > viewModel.FriendUserID)
            {
                var temp = viewModel.UserID;
                viewModel.UserID = viewModel.FriendUserID;
                viewModel.FriendUserID = temp;
            }

            //mark the request as Approved

            var request =
            _repo.GetAll().
                Where(x => x.FromUserID == viewModel.UserID &&
                          x.ToUserID == viewModel.FriendUserID &&
                          x.ActionBy == WhoSentRequestID)
                .FirstOrDefault();

            request.Status = (int)FriendRequestStatus.Approved;
            request.ActionBy = loggedUserID;
        
            _unit.Save();

            var notifcation = new NotificationCreateViewModel
            {
                ContentArabic = "تم الموافقة على طلب الصداقة",
                ContentEnglish = "The friend request was approved",
                NotificationTypeID = 5,
                SourceID = loggedUserID,
                DestinationID = WhoSentRequestID,
                FriendRequestID = request.ID

            };

             _notificationService.SendNotifictionForUser(notifcation);

            return true;
        }

        public bool IgnoreFriendRequest(FriendCreateViewModel viewModel)
        {
            var loggedUserID = viewModel.UserID;
            var WhoSentRequestID = viewModel.FriendUserID;
            if (viewModel.UserID > viewModel.FriendUserID)
            {
                var temp = viewModel.UserID;
                viewModel.UserID = viewModel.FriendUserID;
                viewModel.FriendUserID = temp;
            }

            //mark the request as Ignored

            var request =
               _repo.GetAll().
                   Where(x => x.FromUserID == viewModel.UserID &&
                             x.ToUserID == viewModel.FriendUserID &&
                             x.ActionBy == WhoSentRequestID)
                   .FirstOrDefault();

            request.Status = (int)FriendRequestStatus.Declined;
            request.ActionBy = loggedUserID;

            _unit.Save();

            //var notifcation = new NotificationCreateViewModel
            //{
            //    ContentArabic = "تم رفض طلب الصداقة ",
            //    ContentEnglish = "a friend request was rejected",
            //    NotificationTypeID = 13,
            //    SourceID = viewModel.UserID,
            //    DestinationID = viewModel.FriendUserID,
            //    FriendRequestID = request.ID

            //};

            //_notificationService.SendNotifictionForUser(notifcation);

            return true;
        }

        public bool UnSeenFriend(FriendCreateViewModel viewModel)
        {
            var loggedUserID = viewModel.UserID;
          //  var WhoSentRequestID = viewModel.FriendUserID;
            if (viewModel.UserID > viewModel.FriendUserID)
            {
                var temp = viewModel.UserID;
                viewModel.UserID = viewModel.FriendUserID;
                viewModel.FriendUserID = temp;
            }

            //mark the request as Ignored

            var request =
               _repo.GetAll().
                   Where(x => x.FromUserID == viewModel.UserID &&
                             x.ToUserID == viewModel.FriendUserID &&
                             x.Status == (int)FriendRequestStatus.Approved)
                   .FirstOrDefault();

           // request.Status = (int)FriendRequestStatus.UnSeen;
            request.Status = (int)FriendRequestStatus.NoFriendRequest;
            request.ActionBy = loggedUserID;

            _unit.Save();

            return true;
        }
        public void Add(FriendRequestCreateViewModel viewModel)
        {
            var loggedUserID = viewModel.FromUserID;
            var toUserID = viewModel.ToUserID;
            if(viewModel.FromUserID == viewModel.ToUserID)
            {
                throw new Exception("How Come !!!!!!!!!!!!!");
            }
            //put action by before swapping 
            var actionBy = viewModel.FromUserID;
            // make always small number in from user id to make both of them unique
            if(viewModel.FromUserID > viewModel.ToUserID)
            {
                var temp = viewModel.FromUserID;
                viewModel.FromUserID = viewModel.ToUserID;
                viewModel.ToUserID = temp;
            }
            //TODO: opened for all existed request no validate on status 
            
            var request =
                _repo
                .GetAll()
                .Where(x => x.FromUserID == viewModel.FromUserID && x.ToUserID == viewModel.ToUserID)
                .FirstOrDefault();
            if(request != null && request.Status == (int)FriendRequestStatus.Pending)
            {
                return;
            }
            if(request != null)
            {
                request.Status = (int)FriendRequestStatus.Pending;
                //logged user
                request.ActionBy = actionBy;
                _unit.Save();
            }
            else
            {
                request = _repo.Add(viewModel.ToModel());
                //logged user
                request.ActionBy = actionBy;

                _unit.Save();

            }
            var notifcation = new NotificationCreateViewModel
            {
                ContentArabic = "تم إستلام طلب صداقة ",
                ContentEnglish = "a friend request was received",
                NotificationTypeID = 4,
                SourceID = loggedUserID,
                DestinationID = toUserID,
                FriendRequestID = request.ID

            };
            _notificationService.SendNotifictionForUser(notifcation);


        }

        public void Edit(FriendRequestCreateViewModel viewModel)
        {
            _repo.SaveIncluded(viewModel.ToModel(), "Status");           
        }
        public FriendRequestViewModel GetByID(int id)
        {

            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var postUserAction = _repo.GetAll().Where(postAction => postAction.ID == id)
                .Select(obj => new FriendRequestViewModel
            {
                    ID = obj.ID,
                    ToUserMainImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + obj.ToUser.UserProfile.MainImage,
                    ToUserName = obj.ToUser.UserName,
                    Notes = obj.Notes,
                    Status = obj.Status
                }).FirstOrDefault();
            
            return postUserAction;

        }

        public bool IsExists(int id)
        {
            return _repo.GetAll().Where(x => x.ID == id && !x.IsDeleted).Any();
        }
        public void Delete(int id)
        {
             _repo.Remove(id);
        }
    }

    public class WholeFriendRequestViewModel
    {
        public int ID { get; set; }
        public string ToUserMainImagePath { get; set; }
        public string ToUserName { get; set; }
        public int ToUserID { get; set; }
        public string FromUserMainImagePath { get; set; }
        public string FromUserName { get; set; }
        public int FromUserID { get; set; }
        public string Notes { get; set; }
        public string FromDisplayName { get; internal set; }
        public string ToDisplayName { get; internal set; }
    }

    public class SentFriendRequestViewModel
    {
        public int ID { get; set; }
        public string ToUserMainImagePath { get; set; }
        public string ToUserName { get; set; }
        public int ToUserID { get; set; }
        public string Notes { get; set; }
        public int Status { get; set; }
        public string ToDisplayName { get; internal set; }
    }
}

