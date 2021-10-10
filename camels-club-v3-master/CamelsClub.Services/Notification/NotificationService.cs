using CamelsClub.Data.Extentions;
using CamelsClub.Data.UnitOfWork;
using CamelsClub.Localization.Shared;
using CamelsClub.Repositories;
using CamelsClub.Services.Helpers;
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
    public class NotificationService : INotificationService
    {

        private readonly IUnitOfWork _unit;
        private readonly INotificationRepository _repo;
        private readonly INotificationTypeRepository _notificationTypeRepository;
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly ICompetitionRepository _competitionRepository;
        private readonly ITokenRepository _tokenrepo;
        private readonly IUserRepository _userrepo;
        //   private readonly INotificationService _notificationService;

        public NotificationService(IUnitOfWork unitOfWork,
                                    INotificationRepository notificationRepository,
                                    IMessageRepository messageRepository,
                                    ICompetitionRepository competitionRepository,
                                    IFriendRequestRepository friendRequestRepository,
                                    IUserRepository UserRepository,
                                    INotificationTypeRepository notificationTypeRepository,
                                    ITokenRepository tokenRepository)
                                    //, INotificationService notificationService)
        {

            _unit = unitOfWork;
            _notificationTypeRepository = notificationTypeRepository;
            _repo = notificationRepository;
            _messageRepository = messageRepository;
            _competitionRepository = competitionRepository;
            _friendRequestRepository = friendRequestRepository;
            _tokenrepo = tokenRepository;
            _userrepo = UserRepository;

        }

        

        public int GetUnseenNotificationCount(int UserID)
        {
            var query = _repo.GetAll().Where(n => n.DestinationID == UserID && !n.IsDeleted && n.SeenDateTime == null);
            return query.Count();
        }
        
        public UnSeenNotificationViewModel GetUnSeenNotificationsAndMessagesCount(int userID)
        {
            var notificationsCount = 
                _repo.GetAll()
                    .Where(x => x.DestinationID == userID)
                    .Where(x => x.SeenDateTime == null)
                    .Count();

            var messagesCount =
           _messageRepository.GetAll()
               .Where(x => !x.IsDeleted)
               .Where(x => x.ToUserID == userID)
               .Where(x => !x.IsSeen)
               .Count();

            return new UnSeenNotificationViewModel
            {
                MessagesCount = messagesCount,
                NotificationsCount = notificationsCount
            };

        }
        public NotificationPagingViewModel<NotificationViewModel> GetMyNotifications(int userID, string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var query = _repo.GetAll().Where(comp => !comp.IsDeleted)
                                   //   .Where(x => x.CompetitionID != null)
                                      .Where(x => x.DestinationID == userID);

            List<NotificationViewModel> result = new List<NotificationViewModel>();


            int records = query.Count();
            if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
            int pages = (int)Math.Ceiling((double)records / pageSize);
            int excludedRows = (pageIndex - 1) * pageSize;

            var notifications = query.Select(n => new NotificationViewModel
            {
                ID = n.ID,
                SourceUserImage = n.Source.UserProfile.MainImage != null ? protocol + "://" + hostName + "/uploads/User-Document/" + n.Source.UserProfile.MainImage : "",
                Content = language == Languages.Arabic ? n.ContentArabic : n.ContentEnglish,
                IsSeen = n.SeenDateTime == null ? false : true,
                NotificationType = language == Languages.Arabic ? n.NotificationType.NameArabic : n.NotificationType.NameEnglish,
                NotificationTypeID = n.NotificationTypeID,
                HasRelation = n.Source.FromFriendRequests.Where(x=> x.ToUserID == userID).Any() ||
                        n.Source.ToFriendRequests.Where(x => x.FromUserID == userID).Any(),
                IsLoggedIDComesFirst = n.SourceID > userID,
                

                IsBossChecker = n.Competition.CompetitionCheckers
                        .Where(x => x.UserID == userID && x.IsBoss)
                        .Where(x => !x.IsDeleted).Any(),
                IsBossReferee = n.Competition.CompetitionReferees
                        .Where(x => x.UserID == userID && x.IsBoss)
                        .Where(x => !x.IsDeleted).Any(),
                IsChecker = n.Competition.CompetitionCheckers
                        .Where(x => x.UserID == userID && !x.IsBoss)
                        .Where(x => !x.IsDeleted).Any(),
                IsReferee = n.Competition.CompetitionReferees
                        .Where(x => x.UserID == userID && !x.IsBoss)
                        .Where(x => !x.IsDeleted).Any(),
                IsInvitedUser = n.Competition.CompetitionInvites
                        .Where(x => x.UserID == userID)
                        .Where(x => !x.IsDeleted)
                        .Any(),
                HasJoinedCompetition = 
                        n.Competition.CompetitionCheckers
                        .Where(x=>x.UserID==userID)
                        .Where(x=>!x.IsDeleted)
                        .Where(x=>x.JoinDateTime.HasValue)
                        .Any() ||
                        n.Competition.CompetitionInvites
                        .Where(x => x.UserID == userID)
                        .Where(x => !x.IsDeleted)
                        .Where(x => x.SubmitDateTime.HasValue)
                        .Any() ||
                        n.Competition.CompetitionReferees
                        .Where(x => x.UserID == userID)
                        .Where(x => !x.IsDeleted)
                        .Where(x => x.JoinDateTime.HasValue)
                        .Any(),
                HasRejectedCompetition =
                        n.Competition.CompetitionCheckers
                        .Where(x => x.UserID == userID && x.RejectDateTime != null)
                        .Where(x => !x.IsDeleted)
                        .Any() ||
                        n.Competition.CompetitionInvites
                        .Where(x => x.UserID == userID && x.RejectDateTime != null)
                        .Where(x => !x.IsDeleted)
                        .Any() ||
                        n.Competition.CompetitionReferees
                        .Where(x => x.UserID == userID && x.RejectDateTime != null)
                        .Where(x => !x.IsDeleted)
                        .Any(),
                SourceID = n.SourceID,
                SourceName = n.Source.UserName,
                SourceDisplayName = n.Source.DisplayName,
                DestinationID = n.DestinationID,
                DestinationName = n.Destination.UserName,
                DestinationDisplayName = n.Destination.DisplayName,
                CreatedDate = n.CreatedDate,
                PostID = n.PostID,
                IsPostExist = n.PostID != null && ( n.Post.ParentPostID == null || n.Post.ParentPostID != null && ! n.Post.ParentPost.IsDeleted ) ,
                CommentID = n.CommentID,
                CompetitionID = n.CompetitionID,
                CompetitorsEndDateJoin = n.CompetitionID != null ? n.Competition.CompetitorsEndJoinDate : (DateTime?)null
            });
            
            result = notifications
                                        .OrderByDescending(n => n.ID)
                                        .Skip(excludedRows)
                                        .Take(pageSize)
                                        .ToList();
            result = result.Where(x => x.IsPostExist || x.PostID == null).ToList();
            result.ForEach(x =>
            {
                if (x.HasRelation)
                {
                    if(x.IsLoggedIDComesFirst)
                    {
                        var sourceUserID = x.SourceID;
                        var request =  _friendRequestRepository.GetAll().Where(r => r.FromUserID == userID && r.ToUserID == sourceUserID).FirstOrDefault();
                        x.Status = request.Status;
                        x.ActionByLoggedUser = userID == request.ActionBy;
                    }
                    else
                    {
                        var sourceUserID = x.SourceID;
                        var request = _friendRequestRepository.GetAll().Where(r => r.FromUserID == sourceUserID && r.ToUserID == userID).FirstOrDefault();
                        x.Status = request.Status;
                        x.ActionByLoggedUser = userID == request.ActionBy;

                    }
                }
            });
            
            result.ForEach(x =>
            {
                if(x.CompetitionID != null)
                {

                   var cometition =  _competitionRepository.GetById(x.CompetitionID.Value);
                    x.CompetitionNameArabic = cometition.NameArabic;
                    x.CompetitionNameEnglish = cometition.NamEnglish;
                    x.CompetitionImage = protocol + "://" + hostName + "/uploads/Competition-Document/" + cometition.Image;
                    x.IsCompleted = cometition.Completed.HasValue;
                    x.IsPublished = cometition.Published.HasValue;
                }
                if(x.CompetitionID != null && x.HasJoinedCompetition == false &&x.IsInvitedUser && x.CompetitorsEndDateJoin.HasValue && x.CompetitorsEndDateJoin > DateTime.Now.Date)
                {
                    x.ExpiredCompetition = true;

                }
            });
            result = result.Where(x => x.ExpiredCompetition != true || x.ExpiredCompetition == null).ToList();
            var unSeenMessagesCount = result.Where(x => !x.IsSeen).Count();
            return new NotificationPagingViewModel<NotificationViewModel>() { PageIndex = pageIndex, PageSize = pageSize, Result = result , UnSeenMessagesCount = unSeenMessagesCount, Records = records, Pages = pages };
        }


        public void UpdateNotificationStatus(int Id)
        {
            _repo.MakeNotificationIsSeen(Id);
            _unit.Save();
        }

        
        public List<NotificationTypeViewModel> GetTypes()
        {
            return
            _notificationTypeRepository.GetAll().Select(x => new NotificationTypeViewModel
            {
                ID = x.ID,
                NameArabic = x.NameArabic,
                NameEnglish = x.NameEnglish
            }).ToList();
        }

        public bool IsExist(int Id)
        {
           return  _repo.GetAll().Where(not => not.ID == Id).Any();
            
        }


        public void SendNotifictionForInvites(NotificationCreateViewModel notifucation, List<CompetitionInviteCreateViewModel> Invites)
        {

            List<int> IDs = Invites.Select(inv => inv.UserID).ToList();
            var Users = _userrepo.GetUserList(IDs);
            SendForAll(notifucation, IDs);
            if (Users.Count > 0)
            {
                foreach (var user in Users)
                {
                    notifucation.DestinationID = user.ID;
                    notifucation.DestinationName = user.Name;
                    var Insertednotification = _repo.Add(notifucation.ToModel());
                    try
                    {
                        SendForOne(notifucation);
                        _unit.Save();
                    }
                    catch
                    {
                        throw new Exception(Resource.SendingFailure);
                    }

                }
            }



        }

        public void SendNotifictionForCheckers(NotificationCreateViewModel notifucation, List<CompetitionCheckerCreateViewModel> Checkers)
        {


            List<int> IDs = Checkers.Select(inv => inv.UserID).ToList();
            var Users = _userrepo.GetUserList(IDs);
            if (Users.Count > 0)
            {
                foreach (var user in Users)
                {
                    notifucation.DestinationID = user.ID;
                    notifucation.DestinationName = user.Name;
                    var Insertednotification = _repo.Add(notifucation.ToModel());
                    try
                    {
                        SendForOne(notifucation);
                        _unit.Save();
                    }
                    catch
                    {
                        throw new Exception(Resource.SendingFailure);
                    }

                }
            }




        }

        public void SendNotifictionForReferees(NotificationCreateViewModel notifucation, List<CompetitionRefereeCreateViewModel> Referees)
        {

            List<int> IDs = Referees.Select(inv => inv.UserID).ToList();
            var Users = _userrepo.GetUserList(IDs);
            if (Users.Count > 0)
            {
                foreach (var user in Users)
                {
                    notifucation.DestinationID = user.ID;
                    notifucation.DestinationName = user.Name;
                    var Insertednotification = _repo.Add(notifucation.ToModel());
                    try
                    {
                        SendForOne(notifucation);
                        _unit.Save();
                    }
                    catch
                    {
                        throw new Exception(Resource.SendingFailure);
                    }

                }
            }



        }


        public void SendNotifictionForFriends(NotificationCreateViewModel notifucation, List<int> UsersIDs)
        {

            if (UsersIDs.Count > 0)
            {
                foreach (var Id in UsersIDs)
                {
                    notifucation.DestinationID = Id;
                    var Insertednotification = _repo.Add(notifucation.ToModel());
                    try
                    {
                        SendForOne(notifucation);
                        _unit.Save();
                    }
                    catch
                    {
                        //throw new Exception(Resource.SendingFailure);
                    }


                }
            }

            _unit.Save();

        }

        public void SendNotifictionForUserWithoutCallingDB(NotificationCreateViewModel notifucation)
        {

            try
            {
                SendForOne(notifucation);
            }
            catch (Exception ex)
            {
                //throw new Exception(Resource.SendingFailure);
            }



        }


        public void SendNotifictionForUser(NotificationCreateViewModel notifucation)
        {

            var Insertednotification = _repo.Add(notifucation.ToModel());
            try
            {
                SendForOne(notifucation);
                _unit.Save();
            }
            catch(Exception ex)
            {
                //throw new Exception(Resource.SendingFailure);
            }



        }
        public void AddNotification(NotificationCreateViewModel notification) 
        {
            var Insertednotification = _repo.Add(notification.ToModel());

        }

        public void AddNotifications(List<NotificationCreateViewModel> notifications)
        {
            var Insertednotification = _repo.AddRange(notifications.Select(x=> x.ToModel()).ToList());

        }

        private void SendForOne(NotificationCreateViewModel notifucation)
        {
            var TokensViewModel = _tokenrepo.GetDevicesIDsByUserID(notifucation.DestinationID);
            if (TokensViewModel != null && TokensViewModel.DevicesIDs.Count() > 0)
            {
                foreach (var item in TokensViewModel.DevicesIDs)
                {
                        var res = NotificationHelper
                         .NewSendNotification(item,
                                        null,
                                      notifucation);
                  
                }

            }
        }

        private void SendForAll(NotificationCreateViewModel notifucation, List<int> UsersIDs)
        {

            // var userIDs = _userrepo.GetUsersIDs();
            var TokensViewModel = _tokenrepo.GetDevicesIDsByListOfUserIDs(UsersIDs);
            if (TokensViewModel != null && TokensViewModel.DevicesIDs.Count() > 0)
            {
                decimal count = TokensViewModel.DevicesIDs.Count() / 1000;
                int repeated = 1;
                do
                {
                    var execludedrows = (repeated - 1) * 1000;
                    TokensViewModel.DevicesIDs.Skip(execludedrows).Take(1000).ToList();
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        try
                        {
                            NotificationHelper
                                .SendNotification("",
                                            TokensViewModel
                                                .DevicesIDs.Skip(execludedrows)
                                                .Take(1000).ToList(),
                                            notifucation.UserImagePath,
                                            notifucation.CompetitionImagePath,
                                            notifucation.CompetitionID,
                                            notifucation.ContentArabic,
                                            notifucation.ContentEnglish,
                                            notifucation.Type);
                        }
                        catch
                        {
                            //throw new Exception(Resource.SendingFailure);
                        }
                    });
                    repeated++;
                } while (Decimal.ToInt32(Math.Ceiling(count)) > repeated);
            }
        }



    }

    public class NotificationResultViewModel
    {
        public int UnReadMessages { get; set; }
        public List<NotificationViewModel> Notifications { get; set; }
        
    }
}
