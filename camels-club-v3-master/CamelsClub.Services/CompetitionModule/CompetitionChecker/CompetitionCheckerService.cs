using CamelsClub.Data.Extentions;
using CamelsClub.Data.UnitOfWork;
using CamelsClub.Localization.Shared;
using CamelsClub.Models;
using CamelsClub.Repositories;
using CamelsClub.ViewModels;
using CamelsClub.ViewModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CamelsClub.Services
{
    public class CompetitionCheckerService : ICompetitionCheckerService
    {
        private readonly IUnitOfWork _unit;
        private readonly ICompetitionCheckerRepository _repo;
        private readonly ICompetitionInviteRepository _competitionInviteRepository;
        private readonly ICamelCompetitionRepository _camelCompetitionRepository;
        private readonly ICompetitionRepository _competitionRepository;
        private readonly ICheckerApproveRepository _checkerApproveRepository;
        private readonly ICompetitionAllocateRepository _competitionAllocateRepository;
        private readonly IReviewApproveRepository _reviewApproveRepository;
        private readonly IApprovedGroupRepository _approvedGroupRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly INotificationService _notificationService;


        public CompetitionCheckerService(IUnitOfWork unit, 
                                         ICompetitionCheckerRepository repo,
                                         IGroupRepository groupRepository,
                                         IReviewApproveRepository reviewApproveRepository,
                                         ICompetitionRepository competitionRepository,
                                         IApprovedGroupRepository approvedGroupRepository,
                                         ICamelCompetitionRepository camelCompetitionRepository,
                                         ICheckerApproveRepository checkerApproveRepository,
                                         ICompetitionAllocateRepository competitionAllocateRepository,
                                         INotificationService notificationService,
                                         ICompetitionInviteRepository competitionInviteRepository)
                                         
        {
            _unit = unit;
            _repo = repo;
            _reviewApproveRepository = reviewApproveRepository;
            _competitionInviteRepository = competitionInviteRepository;
            _approvedGroupRepository = approvedGroupRepository;
            _camelCompetitionRepository = camelCompetitionRepository;
            _competitionAllocateRepository = competitionAllocateRepository;
            _checkerApproveRepository = checkerApproveRepository;
            _competitionRepository = competitionRepository;
            _notificationService = notificationService;
            _groupRepository = groupRepository;

        }
        //get associated competitionCheckers with that user
        public PagingViewModel<CompetitionCheckerViewModel> Search(int userID = 0, string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var query = _repo.GetAll().Where(comp => !comp.IsDeleted)
                .Where(x => x.Competition.UserID == userID);



            int records = query.Count();
            if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
            int pages = (int)Math.Ceiling((double)records / pageSize);
            int excludedRows = (pageIndex - 1) * pageSize;

            List<CompetitionCheckerViewModel> result = new List<CompetitionCheckerViewModel>();

            var CompetitionCheckers = query.Select(obj => new CompetitionCheckerViewModel
            {
                ID = obj.ID,
            //    CompetitionID = obj.CompetitionID,
                UserName = obj.User.UserName

            }).OrderByPropertyName(orderBy, isAscending);

            result = CompetitionCheckers.Skip(excludedRows).Take(pageSize).ToList();
            return new PagingViewModel<CompetitionCheckerViewModel>() { PageIndex = pageIndex, PageSize = pageSize, Result = result, Records = records, Pages = pages };
        }


        public void Add(CompetitionCheckerCreateViewModel viewModel)
        {
           
            var insertedCompetitionChecker = _repo.Add(viewModel.ToModel());
            _unit.Save();

            var checker = _repo.GetAll().Where(ch => ch.ID == insertedCompetitionChecker.ID).FirstOrDefault();
            var notifcation = new NotificationCreateViewModel
            {
                ContentArabic = $"{checker.Competition.NameArabic} تم دعوتك للاشتراك بالمسابقة ",
                ContentEnglish = $"You have been joined to competition {checker.Competition.NameArabic}",
                NotificationTypeID = 6,
                SourceID = checker.Competition.UserID,
                DestinationID = viewModel.UserID,
                CompetitionID = viewModel.CompetitionID

            };

           _notificationService.SendNotifictionForUser(notifcation);

        }

        public void Edit(CompetitionCheckerCreateViewModel viewModel)
        {
          
            _repo.Edit(viewModel.ToModel());

        }

        public CompetitionCheckerViewModel GetByID(int id)
        {

            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var CompetitionChecker = _repo.GetAll().Where(comp => comp.ID == id)
                .Select(obj => new CompetitionCheckerViewModel
                {
                    ID = obj.ID,
              //      CompetitionID = obj.CompetitionID,
                    UserName = obj.User.UserName
                }).FirstOrDefault();

            return CompetitionChecker;
        }
        public bool RejectCompetition(CheckerJoinCompetitionCreateViewModel viewModel)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var data =
               _repo.GetAll()
               .Where(x => x.CompetitionID == viewModel.CompetitionID && !x.IsDeleted)
               .Where(x => x.UserID == viewModel.UserID)
               .Select(x => new
               {
                   Checker = x,
                   Competition = x.Competition
               }).FirstOrDefault();

            if (!data.Checker.IsBoss && data.Checker.PickupDateTime == null)
            {
                throw new Exception("You must picked up by boss first");
            }

            if (data.Checker == null)
            {
                throw new Exception("this user is not a checker, your you don't have permission to do that");
            }
            if (data.Checker.JoinDateTime.HasValue)
            {
                throw new Exception("You joined , you can not reject ");
            }

            data.Checker.RejectDateTime = DateTime.UtcNow;
            if(data.Checker.IsBoss == true)
            {
                data.Checker.IsDeleted = true;
            }
            _unit.Save();

            if (data.Checker.IsBoss)
            {
                //send push notification to owner to notify him
                var notification = new NotificationCreateViewModel
                {
                    ContentArabic = $"{NotificationArabicKeys.RejectCompetitionAsChecker} {data.Competition.NameArabic}",
                    ContentEnglish = $"{NotificationEnglishKeys.RejectCompetitionAsChecker} {data.Competition.NamEnglish}",
                    NotificationTypeID = data.Checker.IsBoss ? (int)TypeOfNotification.RejectCompetitionByCheckerBoss : (int)TypeOfNotification.RejectCompetitionByChecker,
                    ArbNotificationType = $"{NotificationArabicKeys.RejectCompetitionAsChecker} {data.Competition.NameArabic}",
                    EngNotificationType = $"{NotificationEnglishKeys.RejectCompetitionAsChecker} {data.Competition.NamEnglish}",
                    SourceID = data.Checker.UserID,
                    DestinationID = data.Competition.UserID,
                    //   SourceName = insertedCompetition.User.Name,
                    CompetitionID = data.Competition.ID,
                    CompetitionImagePath = protocol + "://" + hostName + "/uploads/Competition-Document/" + data.Competition.Image,

                };
                // Task.Run(() =>
                // {
                _notificationService.SendNotifictionForUser(notification);
                // });

            }
            return true;
        }
        public bool HasJoinedCompetition(CheckerJoinCompetitionCreateViewModel viewModel)
        {
            return _repo.GetAll().Where(x => x.CompetitionID == viewModel.CompetitionID && x.UserID == viewModel.UserID && x.JoinDateTime.HasValue)
                     .Any();
        }
        public bool ReplaceChecker(CheckerReplaceCreateViewModel viewModel)
        {
            var data =
               _repo.GetAll()
               .Where(x => x.CompetitionID == viewModel.CompetitionID && !x.IsDeleted)
               .Where(x => x.UserID == viewModel.UserID)
               .Select(x => new
               {
                   Checker = x,
                   CheckersJoined = x.Competition.CompetitionCheckers.ToList(),
                   Competition = x.Competition
               }).FirstOrDefault();

            if(data.Competition.StartedDate != null)
            {
                throw new Exception($"لقد بدات المسابقة ولا يمكنك الاستبدال");
            }
            if (data.Checker == null)
            {
                throw new Exception("this user is not a checker, your you don't have permission to do that");
            }

            if (!data.Checker.IsBoss)
            {
                throw new Exception("You must login by boss");
            }
            if(!data.CheckersJoined.Any(c=> c.ID == viewModel.CurrentCheckerID))
            {
                throw new Exception($"المميز الحالي لاينتمي لهذه المسابقة");
            }

            if (!data.CheckersJoined.Any(c => c.ID == viewModel.NewCheckerID))
            {
                throw new Exception($"المميز الجديد لاينتمي لهذه المسابقة");
            }
            var currentChecker = data.CheckersJoined.FirstOrDefault(x => x.ID == viewModel.CurrentCheckerID);
            currentChecker.JoinDateTime = null;
            currentChecker.PickupDateTime = null;

            var newChecker = data.CheckersJoined.FirstOrDefault(x => x.ID == viewModel.NewCheckerID);
            currentChecker.PickupDateTime = DateTime.Now;

            _unit.Save();
            return true;
        }

        public bool JoinCompetition(CheckerJoinCompetitionCreateViewModel viewModel)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var data =
               _repo.GetAll()
               .Where(x => x.CompetitionID == viewModel.CompetitionID && !x.IsDeleted)
               .Where(x => x.UserID == viewModel.UserID)
               .Select(x => new
               {
                   Checker = x,
                   Competition = x.Competition
               }).FirstOrDefault();

            if (!data.Checker.IsBoss && data.Checker.PickupDateTime == null)
            {
                throw new Exception("You must picked up by boss first");
            }

            if (data.Checker == null)
            {
                throw new Exception("this user is not a checker, your you don't have permission to do that");
            }

            data.Checker.JoinDateTime = DateTime.UtcNow;
            _unit.Save();
            if (data.Checker.IsBoss)
            {
                //send push notification to owner to notify him
                var notification = new NotificationCreateViewModel
                {
                    ContentArabic = $"{NotificationArabicKeys.JoinCompetitionAsChecker} {data.Competition.NameArabic}",
                    ContentEnglish = $"{NotificationEnglishKeys.JoinCompetitionAsChecker} {data.Competition.NamEnglish}",
                    NotificationTypeID = (int)TypeOfNotification.JoinCompetition,
                    ArbNotificationType = "اشتراك عضو الي لجنة تمييز المسايقة",
                    EngNotificationType = "Join the competition as checker",
                    SourceID = data.Checker.UserID,
                    DestinationID = data.Competition.UserID,
                    //   SourceName = insertedCompetition.User.Name,
                    CompetitionID = data.Competition.ID,
                    CompetitionImagePath = protocol + "://" + hostName + "/uploads/Competition-Document/" + data.Competition.Image,

                };
                // Task.Run(() =>
                // {
                _notificationService.SendNotifictionForUser(notification);
                // });

            }
            //get number of joined referees
            //var joinedCheckers =
            //_repo.GetAll()
            //    .Where(x => x.CompetitionID == viewModel.CompetitionID)
            //    .Where(x => x.JoinDateTime != null)
            //    .Count();
            ////get boss
            //var boss = _repo.GetAll()
            //    .Where(x => x.CompetitionID == viewModel.CompetitionID && x.IsBoss).FirstOrDefault();
            //if (joinedCheckers == data.Competition.MinimumCheckersCount)
            //{
            //    var notification = new NotificationCreateViewModel
            //    {
            //        ContentArabic = $"{NotificationArabicKeys.CompletedCheckersMinimumCount} {data.Competition.NameArabic}",
            //        ContentEnglish = $"{NotificationEnglishKeys.CompletedCheckersMinimumCount} {data.Competition.NamEnglish}",
            //        NotificationTypeID = (int)TypeOfNotification.JoinCompetition,
            //        ArbNotificationType = $"{NotificationArabicKeys.CompletedCheckersMinimumCount} {data.Competition.NameArabic}",
            //        EngNotificationType = $"{NotificationEnglishKeys.CompletedCheckersMinimumCount} {data.Competition.NamEnglish}",
            //        SourceID = boss.UserID,
            //        DestinationID = data.Competition.UserID,
            //        //   SourceName = insertedCompetition.User.Name,
            //        CompetitionID = data.Competition.ID,
            //        CompetitionImagePath = protocol + "://" + hostName + "/uploads/Competition-Document/" + data.Competition.Image,

            //    };
            //   // Task.Run(() =>
            //  //  {
            //        _notificationService.SendNotifictionForUser(notification);
            //  //  });

            //}
            return true;

        }

        public bool PickupTeam(List<CompetitionCheckerPickupViewModel> viewModels,int loggedUserID)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();
           
            if (viewModels == null)
            {
                throw new Exception("يرجي اختيار اعضاء فريق اللجان");
            }
            var IDs = viewModels.Select(x => x.ID).ToList();
           
            var data =
                    _repo.GetAll().Where(x => IDs.Contains(x.ID)).Select(x => new { 
                        Checker = x,
                        Competition = x.Competition
                    }).ToList();
            if(data.Select(x => x.Competition).FirstOrDefault().CheckerPickupTeamDateTime != null)
            {
                throw new Exception($"لقد قمت باختيار المميزين من قبل");
            }
            //fucking stupid bussiness, it should be maximum
            if (IDs.Count != data.Select(x => x.Competition).FirstOrDefault().MinimumCheckersCount)
            {
                throw new Exception($"يجب اختيار العدد المحدد");
            }
            data.Select(x=>x.Checker).ToList().ForEach(x => x.PickupDateTime = DateTime.UtcNow);
            _competitionRepository.SaveIncluded(new Competition { ID = data.FirstOrDefault().Competition.ID, CheckerPickupTeamDateTime = DateTime.UtcNow }, "CheckerPickupTeamDateTime");
            _unit.Save();
            //send notification to picked checkers
            var notification = new NotificationCreateViewModel
            {
                ContentArabic = $"{NotificationArabicKeys.NewCompetitionAnnounceToChecker} {data.FirstOrDefault().Competition.NameArabic}",
                ContentEnglish = $"{NotificationEnglishKeys.NewCompetitionAnnounceToReferee} {data.FirstOrDefault().Competition.NamEnglish}",
                NotificationTypeID = (int)TypeOfNotification.CheckerRequestForJoinCompetition,
                ArbNotificationType = "الانضمام الي لجنة تمييز المسايقة",
                EngNotificationType = "Join the competition as checker",
                SourceID = loggedUserID,
                //   SourceName = insertedCompetition.User.Name,
                CompetitionID = data.FirstOrDefault().Competition.ID,
                CompetitionImagePath = protocol + "://" + hostName + "/uploads/Competition-Document/" + data.FirstOrDefault().Competition.Image,
              
            };
                _notificationService.SendNotifictionForCheckers(notification, data.Select(x => x.Checker).Select(c => new CompetitionCheckerCreateViewModel { UserID = c.UserID }).ToList());
            return true;
        }
        //  add new checkers instead of the ones who refused joining
        public bool AddNewCheckersToTeam(List<CompetitionCheckerPickupViewModel> viewModels, int loggedUserID)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            if (viewModels == null)
            {
                throw new Exception("يرجي اختيار اعضاء فريق اللجان");
            }
            var IDs = viewModels.Select(x => x.ID).ToList();

            var data =
                    _repo.GetAll().Where(x => IDs.Contains(x.ID)).Select(x => new {
                        NewCheckers = x,
                        Competition = x.Competition,
                        Checkers = x.Competition.CompetitionCheckers
                    }).ToList();
            if (data.Select(x => x.Competition).FirstOrDefault().CheckerPickupTeamDateTime == null)
            {
                throw new Exception($"لم تقم باختيار الفريق");
            }
            var rejectedCheckersCount = data.SelectMany(x => x.Checkers)
                                            .Where(c => c.RejectDateTime != null)
                                            .Count();
            var pickedAndNotRejected = data.SelectMany(x => x.Checkers)
                                            .Where(c => c.PickupDateTime != null && c.RejectDateTime == null)
                                            .Count();
            if (IDs.Count > rejectedCheckersCount)
            {
                throw new Exception("لقد اخترت اكثر من العدد المسموح");
            }
            
            data.Select(x => x.NewCheckers).Where(x=> IDs.Contains(x.ID)).ToList().ForEach(x => x.PickupDateTime = DateTime.UtcNow);
         
            _unit.Save();
            //send notification to picked checkers
            var notification = new NotificationCreateViewModel
            {
                ContentArabic = $"{NotificationArabicKeys.NewCompetitionAnnounceToChecker} {data.FirstOrDefault().Competition.NameArabic}",
                ContentEnglish = $"{NotificationEnglishKeys.NewCompetitionAnnounceToReferee} {data.FirstOrDefault().Competition.NamEnglish}",
                NotificationTypeID = (int)TypeOfNotification.CheckerRequestForJoinCompetition,
                ArbNotificationType = "الانضمام الي لجنة تمييز المسايقة",
                EngNotificationType = "Join the competition as checker",
                SourceID = loggedUserID,
                //   SourceName = insertedCompetition.User.Name,
                CompetitionID = data.FirstOrDefault().Competition.ID,
                CompetitionImagePath = protocol + "://" + hostName + "/uploads/Competition-Document/" + data.FirstOrDefault().Competition.Image,

            };
            _notificationService.SendNotifictionForCheckers(notification, data.Select(x => x.NewCheckers).Select(c => new CompetitionCheckerCreateViewModel { UserID = c.UserID }).ToList());
            return true;
        }

        public bool AddNewMembers(List<CompetitionCheckerPickupViewModel> viewModels, int loggedUserID)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            if (viewModels == null)
            {
                throw new Exception("يرجي اختيار عضو من فريق اللجان");
            }
            var IDs = viewModels.Select(x => x.ID).ToList();
            var data =
                    _repo.GetAll().Where(x => IDs.Contains(x.ID)).Select(x => new {
                        Checker = x,
                        Competition = x.Competition
                    }).ToList();
            var competition = data.FirstOrDefault().Competition;
          
            if (competition.StartedDate != null)
            {
                throw new Exception($"لقد بدا التمييز بالفعل ولا يمكنك الاستبدال ");
            }

            if (competition.CheckerPickupTeamDateTime == null)
            {
                throw new Exception($"يجب ان تختار فريق التمييز");
            }
            //get number of joined checkers 
            var joinedCount =  _repo.GetAll().Where(x => x.CompetitionID == competition.ID).Where(x => x.PickupDateTime != null && x.RejectDateTime == null).Count();
            
            if(joinedCount + IDs.Count != competition.MinimumCheckersCount)
            {
                throw new Exception($"يجب اختيار العدد المحدد");
            }

            data.ForEach(x=> x.Checker.PickupDateTime = DateTime.UtcNow);
            _unit.Save();
            //send notification to picked checkers
            var notification = new NotificationCreateViewModel
            {
                ContentArabic = $"{NotificationArabicKeys.NewCompetitionAnnounceToChecker} {competition.NameArabic}",
                ContentEnglish = $"{NotificationEnglishKeys.NewCompetitionAnnounceToReferee} {competition.NamEnglish}",
                NotificationTypeID = (int)TypeOfNotification.CheckerRequestForJoinCompetition,
                ArbNotificationType = "الانضمام الي لجنة تمييز المسايقة",
                EngNotificationType = "Join the competition as checker",
                SourceID = loggedUserID,
                //   SourceName = insertedCompetition.User.Name,
                CompetitionID = competition.ID,
                CompetitionImagePath = protocol + "://" + hostName + "/uploads/Competition-Document/" + competition.Image,

            };
            _notificationService.SendNotifictionForCheckers(notification, data.Select(x=> new CompetitionCheckerCreateViewModel { UserID = x.Checker.UserID}).ToList());
            return true;
        }
        public CheckersReportViewModel GetPickedCheckers(int competitionID, int loggedUserID)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();
            if (!_groupRepository.GetAll()
                        .Where(x => !x.IsDeleted)
                        .SelectMany(g => g.Allocates)
                        .Where(a => a.CompetitionChecker.CompetitionID == competitionID)
                        .Any())
            {
                var data = _competitionRepository.GetAll()
                   .Where(x => !x.IsDeleted)
                   .Where(x => x.ID == competitionID)
                   .Select(x => new
                   {
                       CamelsCount = x.CamelCompetitions.Where(c => c.CompetitionID == competitionID)
                                    .Where(c => c.CompetitionInvite.SubmitDateTime != null)
                                    .Count(),
                       Checkers = x.CompetitionCheckers.Where(c => c.PickupDateTime != null && c.RejectDateTime == null).Where(c => !c.IsBoss).Select(c => new CompetitionCheckerViewModel
                       {
                           ID = c.ID,
                           UserName = c.User.UserName,
                           DisplayName = c.User.DisplayName,
                           UserImage = protocol + "://" + hostName + "/uploads/User-Document/" + c.User.UserProfile.MainImage,
                           IsBoss = c.IsBoss,
                           HasJoined = c.JoinDateTime.HasValue,
                           HasPicked = c.PickupDateTime.HasValue,
                           CamelsEvaluatedCount = c.Competition.CamelCompetitions.Where(cc => cc.CheckerApprovers.Where(a => a.CompetitionCheckerID == c.ID).Any())
                                                 .Count(),
                           AssignedCamels = c.Allocates.Select(a => a.Group).Select(g => g.CamelGroups).Count()
                       }).ToList(),
                       CheckerBoss = x.CompetitionCheckers
                                            .Where(c => !c.IsDeleted)
                                            .Where(c => c.IsBoss)
                                            .Where(c => c.ChangeByOwnerDateTime == null)
                                            .Select(c => new { c.ID, c.UserID })
                                            .FirstOrDefault()


                   })
                   .FirstOrDefault();

                //check if the loggeduser is boss
                if (!_repo.GetAll().Where(x => x.UserID == loggedUserID && x.CompetitionID == competitionID && x.IsBoss && !x.IsDeleted).Any())
                {
                    throw new Exception("the logged user is not boss checker for this competition");
                }
                var res = new CheckersReportViewModel
                {
                    TotalNumberOfAllCamels = data.CamelsCount,
                    TotalNumberOfEvaluatedCamels = 0,
                    CheckingCompletitionRatio = 0,
                    Checkers = data.Checkers.ToList()
                };


                return res;
            }


            var checkersInCompetition =
                _groupRepository.GetAll()
                        .Where(x => !x.IsDeleted)
                        .SelectMany(g => g.Allocates)
                        .Where(a => a.CompetitionChecker.CompetitionID == competitionID)
                        .Where(a => !a.IsReplaced)
                        .GroupBy(x => x.CompetitionChecker)
                        .Select(g => new CompetitionCheckerViewModel
                        {
                            ID = g.Key.ID,
                            UserName = g.Key.User.UserName,
                            DisplayName = g.Key.User.DisplayName,
                            UserImage = protocol + "://" + hostName + "/uploads/User-Document/" + g.Key.User.UserProfile.MainImage,
                            IsBoss = g.Key.IsBoss,
                            HasJoined = g.Key.JoinDateTime.HasValue,
                            HasPicked = g.Key.PickupDateTime.HasValue,
                            TotalCamelsCount = g.Select(a => a.Group).SelectMany(a => a.CamelCompetitions).Where(cc => cc.CompetitionID == competitionID).Count(),
                            CamelsEvaluatedCount = g.Select(a => a.Group).SelectMany(a => a.CamelCompetitions)
                                                    .Where(a => a.CompetitionID == competitionID)
                                                   .Where(cc => cc.CheckerApprovers.Any(p => p.CompetitionCheckerID == g.Key.ID))
                                                    .Count(),
                            AssignedGroups = g.Select(a => a.Group).Count()
                        })
                        .ToList();

            var totalNumberOfAllCamels = checkersInCompetition.Select(x => x.TotalCamelsCount).ToList().Sum();
            var totalNumberOfEvaluatedCamels = checkersInCompetition.Select(x => x.CamelsEvaluatedCount).ToList().Sum();
            var checkingCompletitionRatio = (totalNumberOfEvaluatedCamels * 100) / totalNumberOfAllCamels;

            var result = new CheckersReportViewModel
            {
                TotalNumberOfAllCamels = totalNumberOfAllCamels,
                TotalNumberOfEvaluatedCamels = totalNumberOfEvaluatedCamels,
                CheckingCompletitionRatio = checkingCompletitionRatio,
                Checkers = checkersInCompetition
            };

            //check if the loggeduser is boss
            if (!_repo.GetAll().Where(x => x.UserID == loggedUserID && x.CompetitionID == competitionID && x.IsBoss && !x.IsDeleted).Any())
            {
                throw new Exception("the logged user is not boss checker for this competition");
            }
            checkersInCompetition.ForEach(c => {
                c.CompletionRatio = ((c.CamelsEvaluatedCount / c.TotalCamelsCount) * 100);
            });
            return result;

        }

        public CheckersReportViewModel GetTeam(CheckerJoinCompetitionCreateViewModel viewModel)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();
            if(!_groupRepository.GetAll()
                        .Where(x => !x.IsDeleted)
                        .SelectMany(g => g.Allocates)
                        .Where(a => a.CompetitionChecker.CompetitionID == viewModel.CompetitionID)
                        .Any())
            {
                var data = _competitionRepository.GetAll()
                   .Where(x => !x.IsDeleted)
                   .Where(x => x.ID == viewModel.CompetitionID)
                   .Select(x => new
                   {
                       MaximumCheckersCount = x.MaximumCheckersCount,
                       MinimumCheckersCount = x.MinimumCheckersCount,
                       IsTeamPicked = x.CheckerPickupTeamDateTime != null,
                       CamelsCount = x.CamelCompetitions.Where(c=>c.CompetitionID == viewModel.CompetitionID)
                                    .Where(c=>c.CompetitionInvite.SubmitDateTime != null)
                                    .Count(),
                       Checkers = x.CompetitionCheckers.Where(c => !x.IsDeleted && !c.IsBoss).Select(c => new CompetitionCheckerViewModel
                       {
                           ID = c.ID,
                           UserName = c.User.UserName,
                           DisplayName = c.User.DisplayName,
                           UserImage = protocol + "://" + hostName + "/uploads/User-Document/" + c.User.UserProfile.MainImage,
                           IsBoss = c.IsBoss,
                           HasJoined = c.JoinDateTime.HasValue,
                           HasPicked = c.PickupDateTime.HasValue,
                           HasRejected = c.RejectDateTime.HasValue,
                           CamelsEvaluatedCount = c.Competition.CamelCompetitions.Where(cc => cc.CheckerApprovers.Where(a => a.CompetitionCheckerID == c.ID).Any())
                                                 .Count(),
                           AssignedCamels = c.Allocates.Select(a => a.Group).Select(g => g.CamelGroups).Count()
                       }).ToList(),
                       CheckerBoss = x.CompetitionCheckers
                                            .Where(c => !c.IsDeleted)
                                            .Where(c => c.IsBoss)
                                            .Where(c => c.ChangeByOwnerDateTime == null)
                                            .Select(c => new { c.ID, c.UserID })
                                            .FirstOrDefault()


                   })
                   .FirstOrDefault();

                //check if the loggeduser is boss
                if (!_repo.GetAll().Where(x => x.UserID == viewModel.UserID && x.CompetitionID == viewModel.CompetitionID && x.IsBoss && !x.IsDeleted).Any())
                {
                    throw new Exception("the logged user is not boss checker for this competition");
                }
                var res = new CheckersReportViewModel
                {
                    TotalNumberOfAllCamels = data.CamelsCount,
                    IsTeamPicked = data.IsTeamPicked,
                    IsAnyMemberRejected = data.Checkers.Any(x => x.HasRejected),
                    AllCheckersAccepted = data.Checkers.Where(x => x.HasJoined).Count() == data.MinimumCheckersCount,
                    IsReplaceAllowed = data.Checkers.Where(x => x.HasPicked && !x.HasRejected).Count() < data.MinimumCheckersCount,
                    TotalNumberOfEvaluatedCamels = 0,
                    CheckingCompletitionRatio = 0,
                    MaximumCheckersCount = data.MaximumCheckersCount,
                    MinimumCheckersCount = data.MinimumCheckersCount,
                    Checkers = data.Checkers.ToList()
                };


                return res;
            }


            var checkersInCompetition = 
                _groupRepository.GetAll()
                        .Where(x => !x.IsDeleted)
                        .SelectMany(g => g.Allocates)
                        .Where(a=> a.CompetitionChecker.CompetitionID == viewModel.CompetitionID)
                        .Where(a=> !a.IsReplaced)
                        .GroupBy(x => x.CompetitionChecker)
                        .Select(g => new CompetitionCheckerViewModel
                        {
                            ID = g.Key.ID,
                            UserName = g.Key.User.UserName,
                            DisplayName = g.Key.User.DisplayName,
                            UserImage = protocol + "://" + hostName + "/uploads/User-Document/" + g.Key.User.UserProfile.MainImage,
                            IsBoss = g.Key.IsBoss,
                            HasJoined = g.Key.JoinDateTime.HasValue,
                            HasPicked = g.Key.PickupDateTime.HasValue,
                            HasRejected = g.Key.RejectDateTime.HasValue,
                            TotalCamelsCount = g.Select(a => a.Group).SelectMany(a => a.CamelCompetitions).Where(cc=>cc.CompetitionID == viewModel.CompetitionID).Count(),
                            CamelsEvaluatedCount = g.Select(a => a.Group).SelectMany(a => a.CamelCompetitions)
                                                    .Where(a => a.CompetitionID == viewModel.CompetitionID)
                                                   .Where(cc => cc.CheckerApprovers.Any(p => p.CompetitionCheckerID == g.Key.ID))
                                                    .Count(),
                            AssignedGroups = g.Select(a => a.Group).Count()
                        })
                        .ToList();

            var totalNumberOfAllCamels = checkersInCompetition.Select(x => x.TotalCamelsCount).ToList().Sum();
            var totalNumberOfEvaluatedCamels = checkersInCompetition.Select(x => x.CamelsEvaluatedCount).ToList().Sum();
            var checkingCompletitionRatio = (totalNumberOfEvaluatedCamels * 100) / totalNumberOfAllCamels;

            var result = new CheckersReportViewModel
            {
                TotalNumberOfAllCamels = totalNumberOfAllCamels,
                TotalNumberOfEvaluatedCamels = totalNumberOfEvaluatedCamels,
                CheckingCompletitionRatio = checkingCompletitionRatio,
                Checkers = checkersInCompetition
            };

            //check if the loggeduser is boss
            if (!_repo.GetAll().Where(x=>x.UserID == viewModel.UserID && x.CompetitionID == viewModel.CompetitionID && x.IsBoss && !x.IsDeleted).Any() )
            {
                throw new Exception("the logged user is not boss checker for this competition");
            }
            checkersInCompetition.ForEach(c => {
                    c.CompletionRatio = ((c.CamelsEvaluatedCount / c.TotalCamelsCount) * 100);
            });
            return result;
         //   return data.Checkers.ToList();
        }

        //public bool AutoAllocate(CheckerJoinCompetitionCreateViewModel viewModel)
        //{
        //    //get all groups of all competitors
        //    var data = _competitionRepository.GetAll()
        //                            .Where(x => x.ID == viewModel.CompetitionID)
        //                            .Where(x => !x.IsDeleted)
        //                            .Select(x => new
        //                            {
        //                                StartedDate = x.StartedDate,
        //                                JoinedGroupIDs = x.CamelCompetitions
        //                                    .Where(c => c.CompetitionInvite.SubmitDateTime.HasValue)
        //                                    .Where(c => !c.IsDeleted)
        //                                    .Select(c => c.GroupID)
        //                                    .Distinct()
        //                                    .ToList(),
        //                                JoinedCheckerIDs = x.CompetitionCheckers
        //                                                    .Where(c=> !c.IsDeleted)
        //                                                    .Where(c=>c.JoinDateTime.HasValue)
        //                                                    .Where(c=>c.PickupDateTime.HasValue)
        //                                                    .Select(c=>c.ID)
        //                                                    .ToList()

        //                            }).FirstOrDefault();
        //    if (!data.StartedDate.HasValue)
        //    {
        //        throw new Exception("competition not started yet");
        //    }               
        //    //add list to store all assigning
        //    List<CompetitionAllocate> Allocates = new List<CompetitionAllocate>();
        //    //assign each group to a checker
        //    //init values 
        //    var checkersCount = data.JoinedCheckerIDs.Count;
        //    int limit = 0;
        //    if(checkersCount%2 == 0)
        //    {
        //        limit = 2;
        //    }
        //    else
        //    {
        //        limit = 3;
        //    }
        //    int i = 0;
        //    int z = 0;
        //    foreach (var groupID in data.JoinedGroupIDs)
        //    {
        //        while (i <= data.JoinedCheckerIDs.Count-1 && z< limit)
        //        {
        //            Allocates.Add(new CompetitionAllocate
        //            {
        //                GroupID = groupID,
        //                CompetitionCheckerID = data.JoinedCheckerIDs[i]
        //            });
        //            z++;
        //            i++;

        //            if (i == (checkersCount - 1))
        //            {
        //                i = 0;
        //            }

        //        }
        //        z = 0;
               


        //    }
        //    //save them in DB
        //    _competitionAllocateRepository.AddRange(Allocates);
        //    _unit.Save();
        //    return true;
        //}
        public bool AutoAllocate(CheckerJoinCompetitionCreateViewModel viewModel)
        {
            //get all groups of all competitors
            var data = _competitionRepository.GetAll()
                                    .Where(x => x.ID == viewModel.CompetitionID)
                                    .Where(x => !x.IsDeleted)
                                    .Select(x => new
                                    {
                                        CheckingAllocationDate = x.CheckersAllocatedDate,
                                        Competition = x,
                                        StartedDate = x.StartedDate,
                                        JoinedGroupIDs = x.CamelCompetitions
                                            .Where(c => c.CompetitionInvite.SubmitDateTime.HasValue)
                                            .Where(c => !c.IsDeleted)
                                            .Select(c => c.GroupID)
                                            .Distinct()
                                            .ToList(),
                                        JoinedCheckerIDs = x.CompetitionCheckers
                                                            .Where(c => !c.IsDeleted)
                                                            .Where(c => c.JoinDateTime.HasValue)
                                                            .Where(c => c.PickupDateTime.HasValue)
                                                            .Select(c => c.ID)
                                                            .ToList()

                                    }).FirstOrDefault();
            
            if (data.CheckingAllocationDate != null)
            {
                throw new Exception("لقد تم توزيع المسابقة بالفعل");
            }
            
            if (data.Competition.CheckersAllocatedDate.HasValue)
            {
                throw new Exception("لقد تم توزيع المسابقة بالفعل");
            }
            if (!data.StartedDate.HasValue)
            {
                throw new Exception($"المسابقة لم تبدأ بعد");
            }
            //add list to store all assigning
            List<CompetitionAllocate> Allocates = new List<CompetitionAllocate>();
            //assign each group to a checker
            //init values 
            var checkersCount = data.JoinedCheckerIDs.Count;
            int i = 0;
            foreach (var groupID in data.JoinedGroupIDs)
            {
               
                    Allocates.Add(new CompetitionAllocate
                    {
                        GroupID = groupID,
                        CompetitionCheckerID = data.JoinedCheckerIDs[i]
                    });
                    i++;
                    if (i == checkersCount )
                    {
                        i = 0;
                    }


            }
            //save them in DB
            _competitionAllocateRepository.AddRange(Allocates);
            //update competition 
            data.Competition.CheckersAllocatedDate = DateTime.UtcNow;
            _unit.Save();
            return true;
        }
        public bool ManualAllocate(List<ManualAllocateCreateViewModel> viewModels)
        {
            var checkerID = viewModels[0].CheckerID;
            var competition = _repo.GetAll().Where(x => x.ID == checkerID).Select(x => x.Competition).FirstOrDefault();
            if (competition.CheckersAllocatedDate.HasValue)
            {
                throw new Exception("competition is already allocated");
            }
            if (!competition.StartedDate.HasValue)
                throw new Exception("competition is not started yet");
            foreach (var item in viewModels)
            {
               var existedBefore = 
                    _competitionAllocateRepository.GetAll()
                         .Where(x => x.GroupID == item.GroupID && x.CompetitionCheckerID == item.CheckerID && !x.IsDeleted).Any();
                if (existedBefore)
                    throw new Exception($"you assigned this group {item.GroupID} with this checker {item.CheckerID} before");

            }
            _competitionAllocateRepository.AddRange(viewModels.Select(x => x.ToModel()).ToList());
            //update competition to be CheckersAllocated
            competition.CheckersAllocatedDate = DateTime.UtcNow;
            _unit.Save();
            return true;
        }

        public bool HasFilter(int competitionID)
        {
            // check if public competition and then check joined competitors and if more than maximum allowed number 
            var groupsCount =
            _camelCompetitionRepository.GetAll()
                    .Where(x => x.CompetitionID == competitionID)
                    .Select(x => x.GroupID).ToList().Distinct().Count();

            var competition = _competitionRepository.GetById(competitionID);
            if (groupsCount <= competition.MaximumCompetitorsCount)
            {
                return false;
            }

            return true;
        }
        public List<GroupViewModel> GetGroupsForFinalFilter(int competitionID, int loggedUserID)
        {

            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            // check if public competition and then check joined competitors and if more than maximum allowed number 
            var groupsCount = 
            _camelCompetitionRepository.GetAll()
                    .Where(x => x.CompetitionID == competitionID)
                    .Select(x => x.GroupID).ToList().Distinct().Count();

            var competition = _competitionRepository.GetById(competitionID);
            if(groupsCount <= competition.MaximumCompetitorsCount)
            {
                throw new Exception("لا يوجد فلترة لهذه المسابقة");
            }
            var groups =
            _competitionAllocateRepository.GetAll()
            .Where(x => x.CompetitionChecker.CompetitionID == competitionID)
            .Where(x => x.IsReplaced == false)
            .Select(x => new GroupViewModel
            {
                ID = x.Group.ID,
                NameArabic = x.Group.NameArabic,
                NameEnglish = x.Group.NameEnglish,
                CamelsCountInGroup = x.Group.CamelGroups.Count(),
                ImagePath = protocol + "://" + hostName + "/uploads/Camel-Document/" + x.Group.Image,
                IsGroupApproved = x.Group.CamelCompetitions.Where(cc => cc.CompetitionID == competitionID).All(c => c.ApprovedByCheckerBossDateTime != null && !c.IsDeleted),
                IsGroupRejected = x.Group.CamelCompetitions.Where(cc => cc.CompetitionID == competitionID).All(c => c.RejectedByCheckerBossDateTime != null && !c.IsDeleted),
                AssignedCheckers = x.Group.Allocates.Where(all => all.CompetitionChecker.CompetitionID == competitionID)
                    .Select(a => new CompetitionCheckerViewModel
                    {
                        ID = a.CompetitionCheckerID,
                        CamelsIApproved = a.CompetitionChecker.CamelsIApproved//.Select(a=).ToList()
                                                .Where(ca => ca.CamelCompetition.GroupID == x.GroupID)
                                                .Where(ca => ca.CompetitionChecker.CompetitionID == competitionID)
                                                .Count(),

                        UserImage = protocol + "://" + hostName + "/uploads/User-Document/" + a.CompetitionChecker.User.UserProfile.MainImage,
                        UserName = a.CompetitionChecker.User.UserName,
                    }).ToList()
            }).ToList().Distinct(new GroupComparer()).ToList();

            groups.ForEach(x => x.AssignedCheckers.ForEach(c =>
            {
                if (c.CamelsIApproved == x.CamelsCountInGroup)
                {
                    c.HasFinisedRating = true;
                }
            }));
            
            groups = groups.Where(g => g.AssignedCheckers.All(c => c.HasFinisedRating)).ToList();

            return groups;

        }
        //for logged checker
        public List<GroupViewModel> GetGroups(int competitionID, int loggedUserID)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            //check if logged user is boss
            var boss = 
            _competitionRepository.GetAll()
                .Where(x => x.ID == competitionID)
                .SelectMany(x => x.CompetitionCheckers)
                .FirstOrDefault(x => x.IsBoss);
            if(boss.UserID == loggedUserID)
            {
                
                if(!_competitionAllocateRepository.GetAll()
                .Where(x => x.CompetitionChecker.CompetitionID == competitionID)
                .Any()
                )
                {
                    return _camelCompetitionRepository.GetAll()
                           .Where(x => x.CompetitionID == competitionID)
                           .GroupBy(x => x.Group)
                           .Select(x => new GroupViewModel
                           {
                               ID = x.Key.ID,
                               NameArabic = x.Key.NameArabic,
                               NameEnglish = x.Key.NameEnglish,
                               CamelsCountInGroup = x.Key.CamelGroups.Count(),
                               ImagePath = protocol + "://" + hostName + "/uploads/Camel-Document/" + x.Key.Image

                           }).ToList();
                }
                    var groups = 
                _competitionAllocateRepository.GetAll()
                .Where(x => x.CompetitionChecker.CompetitionID == competitionID)
                .Where(x => x.IsReplaced == false)
                .Select(x => new GroupViewModel
                {
                    ID = x.Group.ID,
                    NameArabic = x.Group.NameArabic,
                    NameEnglish = x.Group.NameEnglish,
                    CamelsCountInGroup = x.Group.CamelGroups.Count(),
                    ImagePath = protocol + "://" + hostName + "/uploads/Camel-Document/" + x.Group.Image,
                    IsGroupApproved = x.Group.CamelCompetitions.Where(cc=>cc.CompetitionID ==competitionID).All(c=>c.ApprovedByCheckerBossDateTime != null && !c.IsDeleted),
                    IsGroupRejected = x.Group.CamelCompetitions.Where(cc => cc.CompetitionID == competitionID).All(c=>c.RejectedByCheckerBossDateTime != null && !c.IsDeleted),
                    AssignedCheckers = x.Group.Allocates.Where(all=>all.CompetitionChecker.CompetitionID == competitionID)
                        .Select(a=>new CompetitionCheckerViewModel
                        {
                            ID = a.CompetitionCheckerID,
                            CamelsIApproved = a.CompetitionChecker.CamelsIApproved//.Select(a=).ToList()
                                                    .Where(ca => ca.CamelCompetition.GroupID == x.GroupID)
                                                    .Where(ca=> ca.CompetitionChecker.CompetitionID == competitionID)
                                                    .Count(),

                            UserImage = protocol + "://" + hostName + "/uploads/User-Document/"+ a.CompetitionChecker.User.UserProfile.MainImage,
                            UserName = a.CompetitionChecker.User.UserName,
                        }).ToList()
                }).ToList().Distinct(new GroupComparer()).ToList();

                groups.ForEach(x => x.AssignedCheckers.ForEach(c =>
                {
                    if (c.CamelsIApproved == x.CamelsCountInGroup)
                    {
                        c.HasFinisedRating = true;
                    }
                }));
                return groups;

            }
            return
            _competitionAllocateRepository.GetAll()
                .Where(x => x.CompetitionChecker.UserID == loggedUserID)
                .Where(x => x.CompetitionChecker.CompetitionID == competitionID)
                .Where(x => x.IsReplaced == false)
                .Select(x => new GroupViewModel
                {
                    ID = x.Group.ID,
                    NameArabic = x.Group.NameArabic,
                    NameEnglish = x.Group.NameEnglish,
                    ImagePath = protocol + "://" + hostName + "/uploads/Camel-Document/" + x.Group.Image,
                    IsCheckerFinishedRating = x.Group.CamelCompetitions
                                                .Where(c => c.CompetitionID == competitionID && c.GroupID == x.GroupID)
                                                .All(c =>
                                                    c.CheckerApprovers
                                                    .Any(p => p.CompetitionChecker.UserID == loggedUserID 
                                                        && p.CompetitionChecker.CompetitionID == competitionID))
                }).ToList();

        }

        public List<CamelCompetitionViewModel> GetCamels(int competitionID,int groupID, int loggedUserID)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            //check if logged user is boss
            var boss =
            _competitionRepository.GetAll()
                .Where(x => x.ID == competitionID)
                .SelectMany(x => x.CompetitionCheckers)
                .FirstOrDefault(x => x.IsBoss);
            if (boss.UserID == loggedUserID)
            {
                return
               _camelCompetitionRepository.GetAll()
               .Where(x => x.GroupID == groupID)
               .Where(x => x.CompetitionID == competitionID)
              // .Where(x => x.Status == null)
               .Select(x => new CamelCompetitionViewModel
               {
                   ID = x.ID,
                   CamelName = x.Camel.Name,
                   Status = x.Status,
                   Notes= x.BossNotes,
                   CheckerEvaluates = x.CheckerApprovers
                   .Where(a=>a.CompetitionChecker.Allocates.Any(all=>all.GroupID == x.GroupID && !all.IsReplaced))
                   .Select(a => new CheckerApproveViewModel
                   {
                       ID = a.CompetitionCheckerID,     
                       DisplayName = a.CompetitionChecker.User.DisplayName,
                       SubCheckerName = a.CompetitionChecker.User.UserName,
                       CheckerImage = protocol + "://" + hostName + "/uploads/User-Document/" + a.CompetitionChecker.User.UserProfile.MainImage,
                       Notes = a.Notes,
                       Status = a.Status,
                       ReviewRequests = a.Reviews.Where(r=> !r.IsDeleted)
                                            .Where(r=> r.Status == (int) ReviewApproveStatus.Pending)
                                            .Select(r => new ReviewApproveViewModel
                                            {
                                                CheckerID = r.CheckerID,
                                                CheckerName = r.CompetitionChecker.User.UserName,
                                                NewNotes = r.Notes

                                            }).ToList(),
                       Reviews = a.Reviews.Where(r => !r.IsDeleted)
                                            .Where(r => r.Status == (int)ReviewApproveStatus.Reviewed)
                                            .Select(r => new ReviewApproveViewModel
                                            {
                                                CheckerID = r.CheckerID,
                                                CheckerName = r.CompetitionChecker.User.UserName,
                                                NewNotes = r.Notes
                                            }).ToList()
                   }).ToList(),
                   NotEvaluatedByCheckers = x.Group.Allocates.Where(a=> !a.IsReplaced && a.CompetitionChecker.CamelsIApproved.All(p=>p.CamelCompetitionID != x.ID && !p.IsDeleted))
                                        .Select(a=> new CompetitionCheckerViewModel
                                        {
                                            ID = a.CompetitionCheckerID,

                                            UserName = a.CompetitionChecker.User.UserName,
                                            CheckerImage = protocol + "://" + hostName + "/uploads/User-Document/" + a.CompetitionChecker.User.UserProfile.MainImage,

                                        }).ToList(),
                   CamelImages = x.Camel.CamelDocuments.Select(d => new CamelDocumentViewModel
                   {
                       FilePath = protocol + "://" + hostName + "/uploads/Camel-Document/" + d.FileName
                   }).ToList()

               }).ToList();

            }
            else
            {

                return
                _camelCompetitionRepository.GetAll()
                .Where(x => x.GroupID == groupID)
                .Where(x => x.CompetitionID == competitionID)
                .Select(x => new CamelCompetitionViewModel
                {
                    ID = x.ID,
                    CamelName = x.Camel.Name,
                    // i mnow may be there are more than one notes
                    Notes = x.CheckerApprovers.Select(c=>c.Notes).FirstOrDefault(),
                    IsEvaluated = x.CheckerApprovers.Any(c => c.CompetitionChecker.UserID == loggedUserID && !c.IsDeleted),
                    EvaluateStatus = x.CheckerApprovers.Where(c => c.CompetitionChecker.UserID == loggedUserID && !c.IsDeleted).Select(a=>a.Status).FirstOrDefault(),
                    CamelImages = x.Camel.CamelDocuments.Select(d => new CamelDocumentViewModel
                    {
                        FilePath = protocol + "://" + hostName + "/uploads/Camel-Document/" + d.FileName
                    }).ToList()

                }).ToList();
            }
        }

        public bool EvaluateCamel(CheckerApproveCreateViewModel viewModel)
        {
            if (IsCamelEvaluated(viewModel.UserID, viewModel.ID))
            {
                throw new Exception("لقد تمت عملية التحقق مسبقا");
            }
            //get competition from camel competition
            var competitionID = _camelCompetitionRepository.GetAll().Where(x => x.ID == viewModel.ID).Select(x => x.CompetitionID).FirstOrDefault();
            //get competitionChecker and check if not boss
            var checker = _repo.GetAll().Where(x => x.UserID == viewModel.UserID && x.CompetitionID == competitionID)
                            .Where(x => !x.IsBoss && !x.IsDeleted)
                            .FirstOrDefault();

             viewModel.CompetitionCheckerID = checker.ID;

            _checkerApproveRepository.Add(viewModel.ToModel());
            _unit.Save();
            return true;
        }

        public bool ApproveGroups(List<int> groupIDs , int competitionID)
        {
            var groupsCount =
            _camelCompetitionRepository.GetAll()
                    .Where(x => x.CompetitionID == competitionID)
                    .Select(x => x.GroupID).ToList().Distinct().Count();

            var competition = _competitionRepository.GetById(competitionID);
            if (groupsCount > competition.MaximumCompetitorsCount)
            {
                throw new Exception("يوجد فلترة لهذه المسابقة");
            }
            else
            {
                //check if number of groups equal to maximum number
                var count = competition.MaximumCompetitorsCount;
                var ApprovedCount = groupIDs.Count();
                if(ApprovedCount > count)
                {
                    throw new Exception($"يجب فلترة العدد المحدد فقط");
                }
                var camels = 
                    _camelCompetitionRepository.GetAll()
                    .Where(x => groupIDs.Contains(x.GroupID) && x.CompetitionID == competitionID)
                    .ToList();
                camels.ForEach(x => x.ApprovedByCheckerBossDateTime = DateTime.Now);

                var rejectedCamels = _camelCompetitionRepository.GetAll()
                    .Where(x => !groupIDs.Contains(x.GroupID) && x.CompetitionID == competitionID)
                    .ToList();
                rejectedCamels.ForEach(x => x.RejectedByCheckerBossDateTime = DateTime.Now);

                _unit.Save();
            }
            return true;
        }
        public ApproveGroupResultViewModel ApproveGroup(ApproveGroupCreateViewModel viewModel)
        {
            //var groupsCount =
            //_camelCompetitionRepository.GetAll()
            //        .Where(x => x.CompetitionID == viewModel.CompetitionID)
            //        .Select(x => x.GroupID).ToList().Distinct().Count();

            var competition = _competitionRepository.GetById(viewModel.CompetitionID);
            //if (groupsCount > competition.MaximumCompetitorsCount)
            //{
            //    throw new Exception("يوجد فلترة لهذه المسابقة");
            //}
            if (_camelCompetitionRepository.GetAll()
                .Any(x=>x.GroupID == viewModel.GroupID && x.CompetitionID == viewModel.CompetitionID && x.ApprovedByCheckerBossDateTime != null))
            {
                throw new Exception("لقد تم الترشيح من قبل");
            }
            //get number of approved groups and compare to maximum allowed number in case of public competition
            var data = _camelCompetitionRepository.GetAll()
                 .Where(x => x.CompetitionID == viewModel.CompetitionID)
                 .Where(x => x.ApprovedByCheckerBossDateTime != null)
                 .GroupBy(x => x.CompetitionInviteID)
                 .Select(g => new
                 {
                    ID =  g.Key,
                  //  CompetitionType = g.Select(cc=>cc.Competition).FirstOrDefault().CompetitionType,
                  //  AllowedCompetitors = g.Select(cc => cc.Competition).FirstOrDefault().MaximumCompetitorsCount,
                    Count = g.Count()
                 }).ToList();

            if(competition.CompetitionType == (int)CompetitionType.PublicForAll)
            {
                if(data != null && data.Count == competition.MaximumCompetitorsCount)
                {
                    throw new Exception("لا يمكنك ترشيح منقيات اكثر من العدد المحدد");

                }
            }
            var camelsCompetitions = 
            _camelCompetitionRepository.GetAll()
                .Where(x => x.GroupID == viewModel.GroupID && x.CompetitionID == viewModel.CompetitionID)
                .ToList();

            if(camelsCompetitions
                .Any(c => c.Status == (int)CamelCompetitionStatus.PendingReplace || c.Status == (int)CamelCompetitionStatus.Replaced))
            {
                throw new Exception($"يوجد بعض الناقات التي تحتاج الي مراجعة بعد عملية الاستبدال");
            }
            var timeNow = DateTime.UtcNow;
            
            //checker who should review
            var assignedCheckers =
            _competitionAllocateRepository.GetAll()
                .Where(x => x.CompetitionChecker.CompetitionID == viewModel.CompetitionID)
                .Where(x => x.GroupID == viewModel.GroupID)
                .Select(x => x.CompetitionChecker).ToList();

            foreach (var item in camelsCompetitions)
            {

                //checker who should review
                var checkers = 
                _competitionAllocateRepository.GetAll()
                    .Where(x => x.CompetitionChecker.CompetitionID == viewModel.CompetitionID)
                    .Where(x => x.GroupID == viewModel.GroupID)
                    .Select(x => x.CompetitionChecker)
                    .Where(x=> x.CamelsIApproved.Any(a=>a.CamelCompetitionID == item.ID))
                    .ToList();

                if(checkers.Count() != assignedCheckers.Count())
                {
                    throw new Exception($"لم تنتهي مرحلة فريق لجان التمييز");
                }

            }
            camelsCompetitions.ForEach(x => x.ApprovedByCheckerBossDateTime = timeNow);
            _unit.Save();

            var newCount = _camelCompetitionRepository.GetAll()
                                .Where(x => x.CompetitionID == viewModel.CompetitionID)
                                .Where(x => x.ApprovedByCheckerBossDateTime != null)
                                .GroupBy(x => x.GroupID)
                                .Count();
                              
            if ( newCount == competition.MaximumCompetitorsCount)
            {
                var notApprovedGroups =
              _camelCompetitionRepository.GetAll()
              .Where(x => x.CompetitionID == viewModel.CompetitionID && x.ApprovedByCheckerBossDateTime == null)
              .ToList();
                notApprovedGroups.ForEach(x => x.RejectedByCheckerBossDateTime = DateTime.UtcNow);
                _unit.Save();

            }
            var obj = new ApproveGroupResultViewModel
            { 
                Approved = true,
                Count = newCount,
                Remaining = competition.MaximumCompetitorsCount - newCount 
            };
            return obj;

        }

        public bool ReviewApprove(ReviewApproveRequestCreateViewModel viewModel, int loggedUserID)
        {
            if(!_checkerApproveRepository.GetAll()
                .Where(x=> x.ID == viewModel.CheckerApproveID)
                .SelectMany(x=> x.CamelCompetition.Competition.CompetitionCheckers)
                .Where(x=>x.UserID == loggedUserID && x.IsBoss).Any())
            {
                throw new Exception("You can not review approve without being logged by boss");
            }
            _reviewApproveRepository.Add(new ReviewApprove
            {
                CheckerApproveID = viewModel.CheckerApproveID,
                CheckerID = viewModel.CheckerID,
                Status = (int) ReviewApproveStatus.Pending
            });
            _unit.Save();
            return true;
        }
        public bool RejectGroup(ApproveGroupCreateViewModel viewModel)
        {
            //var isExist = _approvedGroupRepository.GetAll()
            //       .Where(x => x.GroupID == viewModel.GroupID && x.CompetitionID == viewModel.CompetitionID && !x.IsDeleted && x.Status == (int)ApprovedGroupStatus.Rejected).Any();
            //if (isExist)
            //    return true;

            //_approvedGroupRepository.Add(new ApprovedGroup
            //{
            //    GroupID = viewModel.GroupID,
            //    CompetitionID = viewModel.CompetitionID,
            //    Status = (int)ApprovedGroupStatus.Rejected
            //});
            //_unit.Save();

            var camelsCompetitions =
         _camelCompetitionRepository.GetAll()
             .Where(x => x.GroupID == viewModel.GroupID && x.CompetitionID == viewModel.CompetitionID)
             .ToList();
            var timeNow = DateTime.UtcNow;
            camelsCompetitions.ForEach(x => x.RejectedByCheckerBossDateTime = timeNow);
            _unit.Save();
            return true;

        }

        public bool ChangeChecker(ChangeCheckerCreateViewModel viewModel)
        {
            var oldAllocate =
            _competitionAllocateRepository.GetAll()
                .Where(x => x.GroupID == viewModel.GroupID)
                .Where(x => x.CompetitionCheckerID == viewModel.OldCheckerID)
                .FirstOrDefault();
            oldAllocate.IsReplaced = true;

            _competitionAllocateRepository.Add(new CompetitionAllocate
            {
                GroupID = viewModel.GroupID,
                CompetitionCheckerID = viewModel.NewCheckerID
            });

            _unit.Save();
            return true;
        }

        public bool IsCamelEvaluated(int userID, int camelCompetitionID)
        {
            return
            _checkerApproveRepository.GetAll()
                .Any(x => x.CompetitionChecker.UserID == userID && x.CamelCompetitionID == camelCompetitionID && !x.IsDeleted);
            
        }
        public bool IsExists(int id)
        {
            return _repo.GetAll().Where(x => x.ID == id).Any();
        }
        public void Delete(int id)
        {
                _repo.Remove(id);
        }

        public List<ReviewApproveViewModel> GetReviewRequests(int competitionID, int loggedUserID)
        {
            return
            _reviewApproveRepository.GetAll()
                .Where(x => x.CompetitionChecker.UserID == loggedUserID)
                .Where(x => x.Status == (int)ReviewApproveStatus.Pending)
                .Select(x => new ReviewApproveViewModel
                {
                    CheckerID = x.CheckerID,
                    CheckerName = x.CompetitionChecker.User.UserName,
                    OldNotes = x.CheckerApprove.Notes,
                    CheckerApproveID = x.CheckerApproveID
                }).ToList();
        }

        public bool AddApproveReview(ReviewApproveCreateViewModel viewModel, int loggedUserID)
        {
            //get request 
           var request =  _reviewApproveRepository.GetById(viewModel.ID);
            if(request.CompetitionChecker?.UserID != loggedUserID)
            {
                throw new Exception("you can not add review with this checker");
            }
            request.Notes = viewModel.Notes;
            request.Status = (int) ReviewApproveStatus.Reviewed;
            _unit.Save();
            return true;
        }

        public List<CompetitionCheckerViewModel> GetAvailableCheckers(int loggedUserID, int competitionID)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();
           
            return
            _repo.GetAll()
                .Where(x => x.CompetitionID == competitionID)
                .Where(x => x.PickupDateTime == null && x.JoinDateTime == null)
                .Select(x => new CompetitionCheckerViewModel
                {
                    ID = x.ID,
                    UserName = x.User.UserName,
                    DisplayName = x.User.DisplayName,
                    UserImage = protocol + "://" + hostName + "/uploads/User-Document/" + x.User.UserProfile.MainImage,
                   
                }).ToList();
        }

        public bool ApproveReplacedCamel(int loggedUserID, int camelCompetitionID)
        {
            //get camel competition and change status to request to replace
            var data = _camelCompetitionRepository.GetAll()
                            .Where(x => x.ID == camelCompetitionID)
                            .Select(x => new
                            {
                                CamelCompetition = x,
                                UserID = x.CompetitionInvite.UserID
                            })
                            .FirstOrDefault();
            //check if loggeduser if boss 
            var isBoss =
                _repo.GetAll()
                .Where(x => x.UserID == loggedUserID && x.CompetitionID == data.CamelCompetition.CompetitionID && x.IsBoss && !x.IsDeleted)
                .Any();

            if (!isBoss)
            {
                throw new Exception($"لا يتم طلب استبدال ناقة الا بواسطة رئيس تمييز");
            }
            data.CamelCompetition.Status = (int)CamelCompetitionStatus.Approved;
            _unit.Save();

            return true;
        }

        public bool ReplaceCamel(ReplaceCamelRequestCreateViewModel viewModel)
        {
            var hasReviews =  _camelCompetitionRepository.GetAll().Where(x => x.ID == viewModel.ID).SelectMany(x => x.CheckerApprovers).Any();

            if (!hasReviews)
            {
                throw new Exception("لم يتم تقييم الناقة من قبل المميزين");
            }
            //get camel competition and change status to request to replace
            var data = _camelCompetitionRepository.GetAll()
                            .Where(x=>x.ID ==viewModel.ID)
                            .Select(x=> new 
                            { 
                                CamelCompetition = x,
                                UserID = x.CompetitionInvite.UserID
                            })
                            .FirstOrDefault();
            //check if loggeduser if boss 
            var isBoss =
                _repo.GetAll()
                .Where(x => x.UserID == viewModel.UserID && x.CompetitionID == data.CamelCompetition.CompetitionID && x.IsBoss && !x.IsDeleted)
                .Any();

            if (!isBoss)
            {
                throw new Exception($"لا يتم طلب استبدال ناقة الا بواسطة رئيس تمييز");
            }
            data.CamelCompetition.Status = (int)CamelCompetitionStatus.PendingReplace;
            data.CamelCompetition.BossNotes = viewModel.Notes;
            _unit.Save();
            //send notification to competitor
            var notifcation = new NotificationCreateViewModel
            {
                ContentArabic = $"تم اعادة طلبك لمراجعة بعض الابل",
                ContentEnglish = $"Your progress has been stopped to review some camels",
                NotificationTypeID = (int)TypeOfNotification.ReplaceCamel,
                SourceID = viewModel.UserID,
                DestinationID = data.UserID,
                CompetitionID = data.CamelCompetition.CompetitionID
            };
            //Task.Run(() =>
            //{
                _notificationService.SendNotifictionForUser(notifcation);
           // });

            return true;
        }


    }

    public class ReplaceCamelRequestCreateViewModel
    {
        public int ID { get; set; }
        public string Notes { get; set; }
        [IgnoreDataMember]
        public int UserID { get; set; }
    }

    public class ApproveGroupResultViewModel
    {
        public bool Approved { get; set; }
        public int Count { get; set; }
        public int Remaining { get; set; }
    }

    public class GroupComparer : IEqualityComparer<GroupViewModel>
    {
        public bool Equals(GroupViewModel x, GroupViewModel y)
        {
            return x.ID == y.ID;
        }

        public int GetHashCode(GroupViewModel obj)
        {
            return 0;
        }
    }
    public class CheckersReportViewModel
    {
        public int TotalNumberOfAllCamels { get; set; }
        public int TotalNumberOfEvaluatedCamels { get; set; }
        public int CheckingCompletitionRatio { get; set; }
        public List<CompetitionCheckerViewModel> Checkers { get; set; }
        public int MaximumCheckersCount { get; internal set; }
        public int MinimumCheckersCount { get; internal set; }
        public bool IsTeamPicked { get; internal set; }
        public bool IsAnyMemberRejected { get; internal set; }
        public bool AllCheckersAccepted { get; internal set; }
        public bool IsReplaceAllowed { get; internal set; }
    }

    public enum ApprovedGroupStatus
    {
        Approved = 1,
        Rejected
    }
}

