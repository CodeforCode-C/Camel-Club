using CamelsClub.Data.Context;
using CamelsClub.Data.Extentions;
using CamelsClub.Data.UnitOfWork;
using CamelsClub.Localization.Shared;
using CamelsClub.Models;
using CamelsClub.Repositories;
using CamelsClub.Services.Helpers;
using CamelsClub.ViewModels;
using CamelsClub.ViewModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;

namespace CamelsClub.Services
{
    public class CompetitionService : ICompetitionService
    {
        private readonly IUnitOfWork _unit;
        private readonly ICompetitionRepository _repo;
        private readonly ICamelCompetitionRepository _competitonCamelRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly ICompetitionInviteRepository _competitonInviteRepository;
        private readonly ICompetitionRefereeRepository _competitonRefereeRepository;
        private readonly ICompetitionRewardRepository _competitonRewardRepository;
        private readonly ICompetitionGeneralConditionRepository _competitonGeneralConditionRepository;
        private readonly ICompetitionTeamRewardRepository _competitonTeamRewardRepository;
        private readonly ICompetitionConditionRepository _competitonConditionRepository;
        private readonly ICompetitionCheckerRepository _competitonCheckerRepository;
        private readonly ICompetitionSpecificationRepository _competitonSpecificationRepository;

        private readonly INotificationService _notificationService;
        public CompetitionService(IUnitOfWork unit,
                                  ICompetitionRepository repo,
                                  IPostRepository postRepository,
                                  IUserRepository userRepository,
                                  ICamelCompetitionRepository competitonCamelRepository,
                                  ICompetitionTeamRewardRepository competitonTeamRewardRepository,
                                  ICompetitionGeneralConditionRepository competitonGeneralConditionRepository,
                                  ICompetitionSpecificationRepository competitonSpecificationRepository,
                                  ICompetitionInviteRepository competitonInviteRepository,
                                  ICompetitionRefereeRepository competitonRefereeRepository,
                                  ICompetitionRewardRepository competitonRewardRepository,
                                  INotificationService notificationService,
                                  ICompetitionConditionRepository competitonConditionRepository,
                                  ICompetitionCheckerRepository competitonCheckerRepository
                                 )
        {
            _unit = unit;
            _repo = repo;
            _userRepository = userRepository;
            _postRepository = postRepository;
            _competitonCamelRepository = competitonCamelRepository;
            _competitonGeneralConditionRepository = competitonGeneralConditionRepository;
            _competitonSpecificationRepository = competitonSpecificationRepository;
            _competitonInviteRepository = competitonInviteRepository;
            _competitonRefereeRepository = competitonRefereeRepository;
            _competitonRewardRepository = competitonRewardRepository;
            _competitonTeamRewardRepository = competitonTeamRewardRepository;
            _competitonConditionRepository = competitonConditionRepository;
            _competitonCheckerRepository = competitonCheckerRepository;
            _notificationService = notificationService;
        }
        public PagingViewModel<CompetitionViewModel> Search(int userID = 0, string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var query = _repo.GetAll().Where(comp => !comp.IsDeleted)
                .Where(x => userID == 0 || x.UserID == userID);



            int records = query.Count();
            if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
            int pages = (int)Math.Ceiling((double)records / pageSize);
            int excludedRows = (pageIndex - 1) * pageSize;

            List<CompetitionViewModel> result = new List<CompetitionViewModel>();

            var competitions = query.Select(obj => new CompetitionViewModel
            {
                ID = obj.ID,
                NameArabic = obj.NameArabic,
                NamEnglish = obj.NamEnglish,
                Address = obj.Address,
                CamelsCount = obj.CamelsCount,
                From = obj.From,
                To = obj.To,
                CategoryArabicName = obj.Category.NameArabic,
                CategoryEnglishName = obj.Category.NameEnglish,
                CategoryID = obj.CategoryID,
                UserName = obj.User.UserName,
                ImagePath = protocol + "://" + hostName + "/uploads/Competition-Document/" + obj.Image,
                CompetitionType = obj.CompetitionType,
                //Camels = obj.CamelCompetitions.Where(c => !c.IsDeleted).Select(CamelComp => new CamelViewModel
                //{
                //    ID = CamelComp.CamelID,
                //    BirthDate = CamelComp.Camel.BirthDate,
                //    CategoryArabicName = CamelComp.Camel.Category.NameArabic,
                //    CategoryEnglishName = CamelComp.Camel.Category.NameEnglish,
                //    CategoryID = CamelComp.Camel.CategoryID,
                //    UserName = CamelComp.Camel.User.Name,
                //    Details = CamelComp.Camel.Details,
                //    FatherName = CamelComp.Camel.FatherName,
                //    Location = CamelComp.Camel.Location,
                //    MotherName = CamelComp.Camel.MotherName,
                //    Name = CamelComp.Camel.Name,
                //    GenderID = CamelComp.Camel.GenderConfigDetailID,
                //    GenderName = CamelComp.Camel.GenderConfigDetail.NameArabic,
                //    camelDocuments = CamelComp.Camel.CamelDocuments.Select(x=>new CamelDocumentViewModel
                //    {
                //        FilePath = 
                //           protocol + "://" + hostName + "/uploads/Camel-Document/" + obj.Image,
                //        FileType = x.Type

                //    }).ToList()
                //}),
                Rewards = obj.CompetitionRewards.Where(x => !x.IsDeleted).Select(x => new CompetitionRewardViewModel
                {
                    ID = x.ID,
                    NameArabic = x.NameArabic,
                    NamEnglish = x.NameEnglish,
                    SponsorText = x.SponsorText
                }),

                Invites = obj.CompetitionInvites.Where(x => !x.IsDeleted).Select(x => new CompetitionInviteViewModel
                {
                    ID = x.ID,
                    UserName = x.User.UserName,
                    UserImage = obj.User.UserProfile.MainImage != null ?
                       protocol + "://" + hostName + "/uploads/User-Document/" + obj.User.UserProfile.MainImage : "",

                }),
                Referees = obj.CompetitionReferees.Where(x => !x.IsDeleted).Select(x => new CompetitionRefereeViewModel
                {
                    ID = x.ID,
                    UserName = x.User.UserName,
                    UserImage = obj.User.UserProfile.MainImage != null ?
                       protocol + "://" + hostName + "/uploads/User-Document/" + obj.User.UserProfile.MainImage : "",
                    IsBoss = x.IsBoss
                }),
                Checkers = obj.CompetitionCheckers.Where(x => !x.IsDeleted).Select(x => new CompetitionCheckerViewModel
                {
                    ID = x.ID,
                    UserName = x.User.UserName,
                    UserImage = obj.User.UserProfile.MainImage != null ?
                       protocol + "://" + hostName + "/uploads/User-Document/" + obj.User.UserProfile.MainImage : "",
                    IsBoss = x.IsBoss
                }),
                Conditions = obj.CompetitionConditions.Where(x => !x.IsDeleted).Select(x => new CompetitionConditionViewModel
                {
                    ID = x.ID,
                    TextArabic = x.TextArabic,
                    TextEnglish = x.TextEnglish
                })

            }).OrderByPropertyName(orderBy, isAscending);

            result = competitions.Skip(excludedRows).Take(pageSize).ToList();
            var res = new PagingViewModel<CompetitionViewModel>() { PageIndex = pageIndex, PageSize = pageSize, Result = result, Records = records, Pages = pages };
            return res;
        }

        public PagingViewModel<CompetitionViewModel> GetMyCompetitons(int userId, string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var query = _repo.GetAll().Where(comp => !comp.IsDeleted)
                .Where(x => x.UserID == userId);



            int records = query.Count();
            if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
            int pages = (int)Math.Ceiling((double)records / pageSize);
            int excludedRows = (pageIndex - 1) * pageSize;

            List<CompetitionViewModel> result = new List<CompetitionViewModel>();

            var competitions = query.Select(obj => new CompetitionViewModel
            {
                ID = obj.ID,
                NameArabic = obj.NameArabic,
                NamEnglish = obj.NamEnglish,
                Address = obj.Address,
                CamelsCount = obj.CamelsCount,
                From = obj.From,
                To = obj.To,
                CategoryArabicName = obj.Category.NameArabic,
                CategoryEnglishName = obj.Category.NameEnglish,
                CategoryID = obj.CategoryID,
                UserName = obj.User.UserName,
                ImagePath = protocol + "://" + hostName + "/uploads/Competition-Document/" + obj.Image,
                CompetitionType = obj.CompetitionType,
                Rewards = obj.CompetitionRewards.Where(x => !x.IsDeleted).Select(x => new CompetitionRewardViewModel
                {
                    ID = x.ID,
                    NameArabic = x.NameArabic,
                    NamEnglish = x.NameEnglish,
                    SponsorText = x.SponsorText
                }),

                Invites = obj.CompetitionInvites.Where(x => !x.IsDeleted).Select(x => new CompetitionInviteViewModel
                {
                    ID = x.ID,
                    UserName = x.User.UserName,
                    UserImage = x.User.UserProfile.MainImage != null ?
                       protocol + "://" + hostName + "/uploads/User-Document/" + x.User.UserProfile.MainImage : "",

                }),
                Referees = obj.CompetitionReferees.Where(x => !x.IsDeleted).Select(x => new CompetitionRefereeViewModel
                {
                    ID = x.ID,
                    UserName = x.User.UserName,
                    UserImage = x.User.UserProfile.MainImage != null ?
                       protocol + "://" + hostName + "/uploads/User-Document/" + x.User.UserProfile.MainImage : "",
                    IsBoss = x.IsBoss
                }),
                Checkers = obj.CompetitionCheckers.Where(x => !x.IsDeleted).Select(x => new CompetitionCheckerViewModel
                {
                    ID = x.ID,
                    UserName = x.User.UserName,
                    UserImage = x.User.UserProfile.MainImage != null ?
                       protocol + "://" + hostName + "/uploads/User-Document/" + x.User.UserProfile.MainImage : "",
                    IsBoss = x.IsBoss
                }),
                Conditions = obj.CompetitionConditions.Where(x => !x.IsDeleted).Select(x => new CompetitionConditionViewModel
                {
                    ID = x.ID,
                    TextArabic = x.TextArabic,
                    TextEnglish = x.TextEnglish
                })

            }).OrderByPropertyName(orderBy, isAscending);

            result = competitions.Skip(excludedRows).Take(pageSize).ToList();
            var res = new PagingViewModel<CompetitionViewModel>() { PageIndex = pageIndex, PageSize = pageSize, Result = result, Records = records, Pages = pages };
            return res;
        }

        public bool Add(CompetitionCreateViewModel viewModel)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            using (var dbContext = new CamelsClubContext())
            using (var _unit = new UnitOfWork(dbContext))
            {
                var _repo = new CompetitionRepository(dbContext);
                var _competitonInviteRepository = new CompetitionInviteRepository(dbContext);
                var _competitonRefereeRepository = new CompetitionRefereeRepository(dbContext);
                var _competitonRewardRepository = new CompetitionRewardRepository(dbContext);
                var _competitonConditionRepository = new CompetitionConditionRepository(dbContext);
                var _competitonCheckerRepository = new CompetitionCheckerRepository(dbContext);
                var _competitonSpecificationRepository = new CompetitionSpecificationRepository(dbContext);
                var _competitonTeamRewardRepository = new CompetitionTeamRewardRepository(dbContext);
                var _notificationRepository = new NotificationRepository(dbContext);
                ValidateCompetition(viewModel);

                var insertedCompetition = _repo.Add(viewModel.ToModel());

                if (viewModel.CompetitionType == CompetitionType.PublicForAll)
                {
                    //code is taken to seperate API
                }
                else if (viewModel.CompetitionType == CompetitionType.PrivateForInvites)
                {
                    if (viewModel.Invites != null)
                    {
                        viewModel.Invites.ForEach(x => x.CompetitionID = insertedCompetition.ID);
                        _competitonInviteRepository.AddRange(viewModel.Invites.Select(x => x.ToModel()));
                    }
                    else
                    {
                        throw new Exception("يجب اضافة متسابقين");

                    }

                }
                else
                {
                    throw new Exception("يجب اختيار نوع المسابقة");
                }

                if (viewModel.Referees != null)
                {
                    viewModel.Referees.ForEach(x => x.CompetitionID = insertedCompetition.ID);
                    _competitonRefereeRepository.AddRange(viewModel.Referees.Select(x => x.ToModel()));
                }
                if (viewModel.Rewards != null)
                {
                    viewModel.Rewards.ForEach(x => x.CompetitionID = insertedCompetition.ID);
                    _competitonRewardRepository.AddRange(viewModel.Rewards.Select(x => x.ToModel()));

                }

                if (viewModel.Conditions != null)
                {
                    viewModel.Conditions.ForEach(x => x.CompetitionID = insertedCompetition.ID);
                    _competitonConditionRepository.AddRange(viewModel.Conditions.Select(x => x.ToModel()));
                }

                if (viewModel.Checkers != null)
                {
                    viewModel.Checkers.ForEach(x => x.CompetitionID = insertedCompetition.ID);
                    _competitonCheckerRepository.AddRange(viewModel.Checkers.Select(x => x.ToModel()));
                }
                if (viewModel.Specifications != null)
                {
                    if (viewModel.Specifications.Select(x => x.MaxAllowedValue).Sum() != 100)
                    {
                        throw new Exception("قيم اعضاء الابل التي سيتم التحكيم عليها يجب ان تساوى 100");
                    }
                    viewModel.Specifications.ForEach(x => x.CompetitionID = insertedCompetition.ID);
                    _competitonSpecificationRepository.AddRange(viewModel.Specifications.Select(x => x.ToModel()));
                }

                if (viewModel.TeamRewards != null)
                {
                    viewModel.TeamRewards.ForEach(x => x.CompetitionID = insertedCompetition.ID);
                    _competitonTeamRewardRepository.AddRange(viewModel.TeamRewards.Select(x => x.ToModel()));
                }

                _unit.Save();
                var comp = _repo.GetAll().Where(com => com.ID == insertedCompetition.ID).FirstOrDefault();
                var comImg = protocol + "://" + hostName + "/uploads/Competition-Document/" + insertedCompetition.Image;

                //send for checker boss
                if (viewModel.Checkers != null && viewModel.Checkers.Count > 0)

                {
                    var notifcation = new NotificationCreateViewModel
                    {
                        ID = insertedCompetition.ID,
                        ContentArabic = $"{NotificationArabicKeys.NewCompetitionAnnounceToChecker}{comp.NameArabic}",
                        ContentEnglish = $"{NotificationEnglishKeys.NewCompetitionAnnounceToChecker}{comp.NamEnglish}",
                        NotificationTypeID = (int)TypeOfNotification.CheckerRequestForJoinCompetition,
                        ArbNotificationType = "الانضمام الي لجنة التمييز",
                        EngNotificationType = "Join the competition as Checker boss",
                        SourceID = viewModel.UserID,
                        DestinationID = viewModel.Checkers.Where(x => x.IsBoss).FirstOrDefault().UserID,
                        //    SourceName = insertedCompetition.User.Name,
                        CompetitionImagePath = comImg,
                        CompetitionID = insertedCompetition.ID,

                    };
                    _notificationRepository.Add(notifcation.ToModel());
                    _unit.Save();
                    Task.Run(() =>
                    {
                        _notificationService.SendNotifictionForUserWithoutCallingDB(notifcation);
                    });
                }
                if (viewModel.Referees != null && viewModel.Referees.Count > 0)

                {
                    var notifcation = new NotificationCreateViewModel
                    {
                        ID = insertedCompetition.ID,
                        ContentArabic = $"{NotificationArabicKeys.NewCompetitionAnnounceToReferee} {comp.NameArabic}",
                        ContentEnglish = $"{NotificationEnglishKeys.NewCompetitionAnnounceToReferee} {comp.NamEnglish}",
                        NotificationTypeID = (int)TypeOfNotification.RefereeRequestForJoinCompetition,
                        ArbNotificationType = "الانضمام الي لجنة تحكيم المسايقة",
                        EngNotificationType = "Join the competition as Referee boss",
                        SourceID = viewModel.UserID,
                        DestinationID = viewModel.Referees.Where(x => x.IsBoss).FirstOrDefault().UserID,
                        //   SourceName = insertedCompetition.User.Name,
                        CompetitionID = insertedCompetition.ID,
                        CompetitionImagePath = protocol + "://" + hostName + "/uploads/Competition-Document/" + insertedCompetition?.Image,
                        UserImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + insertedCompetition.User?.UserProfile?.MainImage,

                    };
                    _notificationRepository.Add(notifcation.ToModel());
                    _unit.Save();
                    Task.Run(() =>
                    {
                        _notificationService.SendNotifictionForUserWithoutCallingDB(notifcation);
                    });

                }
            }
            //only send to boss checker




            //if (!viewModel.Conditions.Any())
            //{
            //    throw new Exception($"يجب ادخال شروط لدخول المسابقة");
            //}



            return true;
        }

        private void ValidateCompetition(CompetitionCreateViewModel viewModel)
        {
            var currentDate = DateTime.Now;

            var refereeUserIDs = viewModel.Referees.Select(x => x.UserID).ToList();
            var checkerUserIDs = viewModel.Checkers.Select(x => x.UserID).ToList();
            var competitorUserIDs = viewModel.Invites.Select(x => x.UserID).ToList();

            if (refereeUserIDs.Any(r => checkerUserIDs.Contains(r)))
            {
                throw new Exception($"لا يمكن اشتراك نفس الشخص كمتسابق ومميز في نفس المسابقة");
            }
            if (competitorUserIDs.Any(r => checkerUserIDs.Contains(r)))
            {
                throw new Exception($"لا يمكن اشتراك نفس الشخص كمتسابق ومميز في نفس المسابقة");
            }

            if (viewModel.Referees.Where(x => x.IsBoss == true).Count() > 1 || viewModel.Referees.Where(x => x.IsBoss == true).Count() == 0)
            {
                throw new Exception("يجب ان يكون هناك رئيس تحكييم واحد فقط");
            }
            if (viewModel.Checkers.Where(x => x.IsBoss == true).Count() > 1 || viewModel.Checkers.Where(x => x.IsBoss == true).Count() == 0)
            {
                throw new Exception("يجب ان يكون هناك رئيس تمييز واحد فقط");
            }
            // we removed one to encouter the boss
            if (viewModel.Checkers.Count() - 1 > viewModel.MaximumCheckersCount  || viewModel.Referees.Count() - 1 > viewModel.MaximumRefereesCount )
            {
                throw new Exception("لقد تجاوز عدد الحكام والمميزين الحد المسموح به للمسابقة");
            }
            if (viewModel.From.Date < currentDate.Date)
            {
                throw new Exception("يجب ان يكون تاريخ بدء المسابقة اكبر من تاريخ اليوم");
            }
            if (viewModel.From > viewModel.To)
            {
                throw new Exception("يجب ان يكون تاريخ انتهاء المسابقة اكبر من تاريخ بدء المسابقة");
            }
            if (viewModel.CompetitorsEndJoinDate > viewModel.To || viewModel.CompetitorsEndJoinDate < viewModel.From)
            {
                throw new Exception("تاريخ انتهاء المتسابقين لا يتماشي مع تاريخ بدء وانتهاء المسابقة");
            }
            if (viewModel.RefereesVotePercentage + viewModel.PeopleVotePercentage != 100)
            {
                throw new Exception("نسبة التصويت غير صحيحة");

            }
            if (viewModel.Rewards.Distinct(new RewardsComparer()).Count() != viewModel.Rewards.Count())
            {
                throw new Exception("هناك تكرار في جوائز المركز الواحد");
            }
            if (viewModel.TeamRewards.Count() > 0)
            {
                foreach (var item in viewModel.TeamRewards)
                {
                    if (string.IsNullOrWhiteSpace(item.TextArabic) || item.AssignedTo > 4 || item.AssignedTo < 1)
                    {
                        throw new Exception("empty values in Team rewards");
                    }
                }
            }


        }

        public void Edit(CompetitionEditViewModel viewModel)
        {
            var currentDate = DateTime.Now;
            if (viewModel.From.Date < currentDate.Date)
            {
                throw new Exception(Resource.InvalidCompetitionStartDate);
            }
            if (viewModel.From > viewModel.To)
            {
                throw new Exception(Resource.InvalidCompetitionDates);
            }
            _repo.Edit(viewModel.ToModel());
            //edit Conditions
            var oldCompetitionConditionIds =
                _competitonConditionRepository
                .GetAll()
                .Where(x => x.CompetitionID == viewModel.ID)
                .Where(x => !x.IsDeleted)
                .Select(x => x.ID)
                .ToList();

            var newCompetitionConditions =
                viewModel.Conditions.Where(x => x.ID == 0).ToList();
            newCompetitionConditions.ForEach(x => x.CompetitionID = viewModel.ID);

            var editedCompetitionConditions =
                viewModel.Conditions
                .Where(x => x.ID != 0 && oldCompetitionConditionIds.Contains(x.ID))
                .ToList();

            var deletedCompetitionConditionIds =
                oldCompetitionConditionIds.
                Where(x => !editedCompetitionConditions.Select(i => i.ID).Contains(x))
                .ToList();

            _competitonConditionRepository.AddRange(newCompetitionConditions.Select(x => x.ToModel()));
            _competitonConditionRepository.RemoveRange(deletedCompetitionConditionIds);
            _competitonConditionRepository.EditRange(editedCompetitionConditions.Select(x => x.ToModel()));

            //edit invitees
            var oldCompetitionInviteIds =
                _competitonInviteRepository
                .GetAll()
                .Where(x => x.CompetitionID == viewModel.ID)
                .Where(x => !x.IsDeleted)
                .Select(x=>x.ID)
                .ToList();

            var newCompetitionInvites =
                viewModel.Invites.Where(x => x.ID == 0).ToList();
            newCompetitionInvites.ForEach(x => x.CompetitionID = viewModel.ID);

            var editedCompetitionInvites =
                viewModel.Invites
                .Where(x => x.ID != 0 && oldCompetitionInviteIds.Contains(x.ID))
                .ToList();

            var deletedCompetitionInviteIds =
                oldCompetitionInviteIds.
                Where(x => !editedCompetitionInvites.Select(i => i.ID).Contains(x))
                .ToList();

            _competitonInviteRepository.AddRange(newCompetitionInvites.Select(x => x.ToModel()));
            _competitonInviteRepository.RemoveRange(deletedCompetitionInviteIds);
            _competitonInviteRepository.EditRange(editedCompetitionInvites.Select(x => x.ToModel()));
            //edit Checkeres
            var oldCompetitionCheckerIds =
                _competitonCheckerRepository
                .GetAll()
                .Where(x => x.CompetitionID == viewModel.ID)
                .Where(x => !x.IsDeleted)
                .Select(x => x.ID)
                .ToList();

            var newCompetitionCheckers =
                viewModel.Checkers.Where(x => x.ID == 0).ToList();
            newCompetitionCheckers.ForEach(x => x.CompetitionID = viewModel.ID);

            var editedCompetitionCheckers =
                viewModel.Checkers
                .Where(x => x.ID != 0 && oldCompetitionCheckerIds.Contains(x.ID))
                .ToList();

            var deletedCompetitionCheckerIds =
                oldCompetitionCheckerIds.
                Where(x => !editedCompetitionCheckers.Select(i => i.ID).Contains(x))
                .ToList();

            _competitonCheckerRepository.AddRange(newCompetitionCheckers.Select(x => x.ToModel()));
            _competitonCheckerRepository.RemoveRange(deletedCompetitionCheckerIds);
            _competitonCheckerRepository.EditRange(editedCompetitionCheckers.Select(x => x.ToModel()));

            //edit referees
            var oldCompetitionRefereeIds =
                _competitonRefereeRepository
                .GetAll()
                .Where(x => x.CompetitionID == viewModel.ID)
                .Where(x => !x.IsDeleted)
                .Select(x => x.ID)
                .ToList();

            var newCompetitionReferees =
                viewModel.Referees.Where(x => x.ID == 0).ToList();
            newCompetitionReferees.ForEach(x => x.CompetitionID = viewModel.ID);

            var editedCompetitionReferees =
                viewModel.Referees
                .Where(x => x.ID != 0 && oldCompetitionRefereeIds.Contains(x.ID))
                .ToList();

            var deletedCompetitionRefereeIds =
                oldCompetitionRefereeIds.
                Where(x => !editedCompetitionReferees.Select(i => i.ID).Contains(x))
                .ToList();

            _competitonRefereeRepository.AddRange(newCompetitionReferees.Select(x => x.ToModel()));
            _competitonRefereeRepository.RemoveRange(deletedCompetitionRefereeIds);
            _competitonRefereeRepository.EditRange(editedCompetitionReferees.Select(x => x.ToModel()));

           
            _unit.Save();
        }
      
        public int GetSuspendCompetitionsCount(int userID)
        {
            var dateToday = DateTime.UtcNow;
            var count = _repo.GetAll()
                // .Where(c => c.To <= dateToday)
                .Where(c => c.StartedDate == null)
                .Where(c => c.CompetitionInvites
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.JoinDateTime == null && i.RejectDateTime == null) ||
                            c.CompetitionReferees
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && !i.IsBoss && i.PickupDateTime != null && i.JoinDateTime == null && i.RejectDateTime == null) ||
                           c.CompetitionReferees
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.IsBoss && i.JoinDateTime == null && i.RejectDateTime == null) ||
                            c.CompetitionCheckers
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && !i.IsBoss && i.PickupDateTime != null && i.JoinDateTime == null && i.RejectDateTime == null) ||
                            c.CompetitionCheckers
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.IsBoss && i.JoinDateTime == null && i.RejectDateTime == null))
                .Where(c => c.CompetitorsEndJoinDate >= dateToday)
                .Count();
           
            return count;
        }
        public List<CompetitionInviteViewModel> GetCompetitors(int competitionID)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            return _competitonInviteRepository.GetAll().Where(x => x.CompetitionID == competitionID && !x.IsDeleted)
                    .Where(x => x.JoinDateTime != null && x.SubmitDateTime != null).Select(x => new CompetitionInviteViewModel
                    {
                        ID = x.ID,
                        UserName = x.User.UserName,
                        DisplayName = x.User.DisplayName,
                        UserImage = x.User.UserProfile.MainImage != null ?
                             protocol + "://" + hostName + "/uploads/User-Document/" + x.User.UserProfile.MainImage : "",
                        HasJoined = x.JoinDateTime != null && x.SubmitDateTime != null,
                        FinalScore = x.FinalScore,

                    }).ToList();

        }

        public List<CompetitionRefereeViewModel> GetReferees(int competitionID)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            return _competitonRefereeRepository.GetAll().Where(x => x.CompetitionID == competitionID && !x.IsDeleted)
                                                          .Where(x => (x.JoinDateTime != null && x.Competition.StartedDate != null) ||
                                                                    //pickup is not done
                                                                    x.Competition.RefereePickupTeamDateTime == null ||
                                                                    // pickup is done and not rejected 
                                                                    x.Competition.RefereePickupTeamDateTime != null && x.RejectDateTime == null)
                                                       .Select(x => new CompetitionRefereeViewModel
                                                       {
                                                           ID = x.ID,
                                                           UserName = x.User.UserName,
                                                           DisplayName = x.User.DisplayName,
                                                           UserImage = x.User.UserProfile.MainImage != null ?
                                                               protocol + "://" + hostName + "/uploads/User-Document/" + x.User.UserProfile.MainImage : "",
                                                           IsBoss = x.IsBoss,
                                                           HasJoined = x.PickupDateTime != null && x.JoinDateTime != null,
                                                           HasPicked = x.PickupDateTime != null
                                                       }).ToList();

        }

        public List<CompetitionCheckerViewModel> GetCheckers(int competitionID)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            return _competitonCheckerRepository.GetAll().Where(x =>x.CompetitionID == competitionID &&  !x.IsDeleted)
                                                        //competition started
                                                        .Where(x => (x.JoinDateTime != null && x.Competition.StartedDate != null) ||
                                                                    //pickup is not done
                                                                    x.Competition.CheckerPickupTeamDateTime == null ||
                                                                    // pickup is done and not rejected 
                                                                    x.Competition.CheckerPickupTeamDateTime != null && x.RejectDateTime == null)
                                                        .Select(x => new CompetitionCheckerViewModel
                                                        {
                                                            ID = x.ID,
                                                            UserName = x.User.UserName,
                                                            DisplayName = x.User.DisplayName,
                                                            UserImage = x.User.UserProfile.MainImage != null ?
                                                               protocol + "://" + hostName + "/uploads/User-Document/" + x.User.UserProfile.MainImage : "",
                                                            IsBoss = x.IsBoss,
                                                            HasJoined = x.PickupDateTime != null && x.JoinDateTime != null,
                                                            HasPicked = x.PickupDateTime != null
                                                        }).ToList();

        }
        public V2CompetitionViewModel GetByID(int userID,int id)
        {

            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

     
            var competition = _repo.GetAll().Where(comp => comp.ID == id)
                .Select(obj => new V2CompetitionViewModel
                {
                    ID = obj.ID,
                    NameArabic = obj.NameArabic,
                    CategoryNameArabic = obj.Category.NameArabic,
                    CategoryNameEnglish = obj.Category.NameEnglish,
                    CamelsCount = obj.CamelsCount,
                    RefereeBossName = obj.CompetitionReferees.Where(x => x.IsBoss).Select(x=> x.User.DisplayName).FirstOrDefault(),
                    CheckerBossName = obj.CompetitionCheckers.Where(x => x.IsBoss).Select(x=> x.User.DisplayName).FirstOrDefault(),
                    From = obj.From,
                    To = obj.To,
                    Address = obj.Address,
                    NameEnglish = obj.NamEnglish,
                    Published = obj.Published,
                    CreatedDate = obj.CreatedDate,
                    ShowReferees = obj.ShowReferees,
                    ShowCheckers = obj.ShowCheckers,
                    ShowCompetitors = obj.ShowCompetitors,
                    IsCheckerPickupTeam = obj.CheckerPickupTeamDateTime != null,
                    IsRefereePickupTeam = obj.RefereePickupTeamDateTime != null,
                    UserName = obj.User.UserName,
                    VoteType = (int)obj.VoteType,
                    ImagePath = protocol + "://" + hostName + "/uploads/Competition-Document/" + obj.Image,
                    CompetitionType = obj.CompetitionType,
                    IsCheckersAllocated = obj.CheckersAllocatedDate != null,
                    IsRefereesAllocated = obj.RefereesAllocatedDate != null,
                    MinimumCheckersCount =  obj.MinimumCheckersCount,
                    MaximumCheckersCount = obj.MaximumCheckersCount,
                    MaximumRefereesCount = obj.MaximumRefereesCount,
                    MinimumRefereesCount = obj.MinimumRefereesCount,
                    MinimumCompetitorsCount = obj.MinimumCompetitorsCount,
                    MaximumCompetitorsCount = obj.MaximumCompetitorsCount,
                    Completed = obj.Completed,
                    IsCompetitorsInvited = obj.CompetitorsInvitedDate != null,
                    StartedDate = obj.StartedDate,
                    UserID = obj.UserID,
                    ConditionText = obj.ConditionText,
                    IsChecker = obj.CompetitionCheckers
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && !i.IsBoss),
                    IsCheckerBoss = obj.CompetitionCheckers
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.IsBoss && i.ChangeByOwnerDateTime == null),
                    IsReferee = obj.CompetitionReferees
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && !i.IsBoss),
                    IsBossReferee = obj.CompetitionReferees
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.IsBoss && i.ChangeByOwnerDateTime == null),
                    IsCompetitor = obj.CompetitionInvites
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID),
                    HasPicked = obj.CompetitionCheckers
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && !i.IsBoss && i.PickupDateTime != null)
                                    || obj.CompetitionReferees
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && !i.IsBoss && i.PickupDateTime != null)
                                    ,
                    HasJoined = obj.CompetitionCheckers
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.IsBoss != true && i.PickupDateTime != null && i.JoinDateTime != null) ||
                                   obj.CompetitionCheckers
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.IsBoss == true && i.JoinDateTime != null) ||
                                    obj.CompetitionReferees
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.IsBoss == false && i.PickupDateTime != null && i.JoinDateTime != null) ||
                                    obj.CompetitionReferees
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.IsBoss == true && i.JoinDateTime != null) ||
                                     obj.CompetitionInvites
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.JoinDateTime != null),
                    HasRejected = obj.CompetitionCheckers
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.PickupDateTime != null && i.RejectDateTime != null) ||
                                    obj.CompetitionReferees
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.IsBoss == false && i.PickupDateTime != null && i.RejectDateTime != null) ||
                                    obj.CompetitionReferees
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.IsBoss == true && i.RejectDateTime != null) ||

                                    obj.CompetitionInvites
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.RejectDateTime != null),

                    Rewards = obj.CompetitionRewards.Where(x => !x.IsDeleted).Select(x => new CompetitionRewardViewModel
                    {
                        ID = x.ID,
                        NameArabic = x.NameArabic,
                        NamEnglish = x.NameEnglish,
                        Value = x.Notes,
                        SponsorText = x.SponsorText,
                        Logo = protocol + "://" + hostName + "/uploads/Competition-Document/" + x.Logo,
                        Rank = (Rank)x.Rank

                    }),
                    TeamRewards = obj.CompetitionTeamRewards.Where(x => !x.IsDeleted).Select(x => new CompetitionTeamRewardViewModel
                    {
                        ID = x.ID,
                        AssignedTo = x.AssignedTo,
                        TextArabic = x.TextArabic,
                        TextEnglish = x.TextEnglish
                    }).ToList(),
                    CheckerPickupTeamDateTime = obj.CheckerPickupTeamDateTime,
                    RefereePickupTeamDateTime = obj.RefereePickupTeamDateTime,
                }).FirstOrDefault();

            //get competition boss
            var checkerBoss = _competitonCheckerRepository.GetAll().Where(x => x.CompetitionID == id).Where(x => x.IsBoss).FirstOrDefault();
            var refereeBoss = _competitonRefereeRepository.GetAll().Where(x => x.CompetitionID == id).Where(x => x.IsBoss).FirstOrDefault();
            if(checkerBoss == null || checkerBoss.RejectDateTime != null)
            {
                competition.IsCheckBossRejected = true;
            }
            if (refereeBoss == null || refereeBoss.RejectDateTime != null)
            {
                competition.IsRefereeBossRejected = true;
            }
            if (competition.StartedDate != null)
            {
                //checking
                var numberOfRegisteredCamels = _competitonCamelRepository.GetAll()
                                                        .Where(x => x.CompetitionID == id).Count();
                var numberOfFinishedCamelsByCheckerBoss = _competitonCamelRepository.GetAll()
                                                        .Where(x => x.CompetitionID == id)
                                                        .Where(x => x.ApprovedByCheckerBossDateTime != null || x.RejectedByCheckerBossDateTime != null)
                                                        .Count();
                if(numberOfRegisteredCamels != 0)
                    competition.CheckingCompletionPercentage = (numberOfFinishedCamelsByCheckerBoss * 100) / numberOfRegisteredCamels;

                //referee
                var numberOfApprovedCamelsByChecker = _competitonCamelRepository.GetAll()
                                                        .Where(x => x.CompetitionID == id)
                                                        .Where(x => x.ApprovedByCheckerBossDateTime != null)
                                                        .Count();
                var numberOfFinishedCamelsByRefereeBoss = _competitonCamelRepository.GetAll()
                                                        .Where(x => x.CompetitionID == id)
                                                        .Where(x => x.ApprovedByRefereeBossDateTime != null)
                                                        .Count();
                if(numberOfApprovedCamelsByChecker!= 0)
                    competition.RefereeingCompletionPercentage = (numberOfFinishedCamelsByRefereeBoss * 100) / numberOfApprovedCamelsByChecker;

            }
            //dashboard
            competition.ModuleCompletion = new ModuleComplateViewModel();
            //if (competition.UserID == userID)
            //{

            if(competition.StartedDate != null)
            {
                competition.CheckingIsFinished = _competitonCamelRepository.GetAll()
                                                                .Where(x => x.CompetitionID == competition.ID)
                                                                .All(x => x.ApprovedByCheckerBossDateTime != null || x.RejectedByCheckerBossDateTime != null);


            }
            competition.ModuleCompletion = new ModuleComplateViewModel
            {
                CheckingModuleDone = _competitonCheckerRepository.GetAll()
                                                        .Where(x => x.CompetitionID == competition.ID)
                                                        .Where(x => !x.IsDeleted)
                                                        //competition started
                                                        .Where(x => (x.JoinDateTime != null && competition.StartedDate != null) ||
                                                                    //pickup is not done
                                                                    competition.CheckerPickupTeamDateTime == null ||
                                                                    // pickup is done and not rejected 
                                                                    competition.CheckerPickupTeamDateTime != null && x.RejectDateTime == null)
                                        .Where(x => x.JoinDateTime != null)
                                        .Count() 
                                        >=
                                        competition.MinimumCheckersCount,
                RefereeModuleDone = _competitonRefereeRepository.GetAll()
                                                       .Where(x => !x.IsDeleted)
                                                        .Where(x => x.CompetitionID == competition.ID)
                                                          .Where(x => (x.JoinDateTime != null && competition.StartedDate != null) ||
                                                                    //pickup is not done
                                                                    competition.RefereePickupTeamDateTime == null ||
                                                                    // pickup is done and not rejected 
                                                                    competition.RefereePickupTeamDateTime != null && x.RejectDateTime == null).
                                                                    Where(x => x.JoinDateTime != null)
                                                                    .Count() 
                                                                    >= competition.MinimumRefereesCount,
                InviteCompetitorsModuleDone = competition.IsCompetitorsInvited ,
                RatingModuleDone = competition.Completed != null,
                PublishModuleDone = competition.Published != null

            };
      
            //}
            if ((competition.Completed != null && competition.UserID == userID) || competition.Published != null)
            {

                competition.Winners =
                    _competitonInviteRepository.GetAll()
                    .Where(x => x.CompetitionID == competition.ID)
                     .Where(x => x.JoinDateTime != null && x.SubmitDateTime != null)
                     .Where(i=> i.FinalScore != null)
                     .OrderByDescending(cc => cc.FinalScore).Select(cc => new CompetitionWinnerViewModel
                     {
                         Percentage = cc.FinalScore,
                         UserName = cc.User.UserName,
                         DisplayName = cc.User.DisplayName,
                         UserImage = cc.User.UserProfile.MainImage != null ?
                           protocol + "://" + hostName + "/uploads/User-Document/" + cc.User.UserProfile.MainImage : "",
                         GroupName = cc.CamelCompetitions.FirstOrDefault().Group.NameArabic,
                         GroupImage = protocol + "://" + hostName + "/uploads/Camel-Document/" + cc.CamelCompetitions.FirstOrDefault().Group.Image,
                        
                     }).Skip(0).Take(5).ToList();
                for (int i = 0; i < competition.Winners.Count(); i++)
                {
                    competition.Winners.ElementAt(i).Rank = (Rank)(i + 1);
                    var reward = competition.Rewards.Where(x => x.Rank == (Rank)(i + 1)).FirstOrDefault();
                    if(reward != null)
                    {
                        competition.Winners.ElementAt(i).RewardTextArabic = reward.NameArabic;
                        competition.Winners.ElementAt(i).Value = reward.Value;
                        competition.Winners.ElementAt(i).Sponsor = reward.SponsorText;
                        competition.Winners.ElementAt(i).Logo = reward.Logo;
                        competition.Winners.ElementAt(i).RewardTextEnglish = reward.NamEnglish;

                    }
                }
            }
            competition.Created = competition.UserID == userID;
            return competition;
        }
        public PagingViewModel<V2CompetitionViewModel> GetCurrentInvolvedCompetitions(int userID, string orderBy, bool isAscending, int pageIndex, int pageSize, Languages language)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var dateToday = DateTime.Now.Date;
            var query = _repo.GetAll()
               // .Where(c => c.To <= dateToday)
                .Where(c => c.CompetitionInvites
                                    .Where(i => !i.IsDeleted && i.RejectDateTime == null)
                                    //we add CompetitorsInvitedDate to ensure that competitors are allowed to view competition
                                    .Any(i => i.UserID == userID && i.RejectDateTime == null && i.JoinDateTime == null && c.CompetitorsInvitedDate != null && c.StartedDate == null && c.CompetitorsEndJoinDate >= dateToday)  ||
                                    c.CompetitionInvites
                                    .Where(i => !i.IsDeleted)
                                    //we add CompetitorsInvitedDate to ensure that competitors are allowed to view competition
                                    .Any(i => i.UserID == userID &&  i.JoinDateTime != null && c.CompetitorsInvitedDate != null && c.CompetitorsInvitedDate != null ) ||
                            c.CompetitionReferees
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && !i.IsBoss && i.PickupDateTime != null && i.RejectDateTime ==null) ||
                           c.CompetitionReferees
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.IsBoss && i.RejectDateTime == null) ||
                            c.CompetitionCheckers
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && !i.IsBoss && i.PickupDateTime != null && i.RejectDateTime == null) ||
                            c.CompetitionCheckers
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.IsBoss && i.RejectDateTime == null));
            int records = query.Count();
            if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
            int pages = (int)Math.Ceiling((double)records / pageSize);
            int excludedRows = (pageIndex - 1) * pageSize;

            List<V2CompetitionViewModel> result = new List<V2CompetitionViewModel>();

            var data =
                query
                .Select(obj => new V2CompetitionViewModel
                {
                    ID = obj.ID,
                    NameArabic = obj.NameArabic,
                    NameEnglish = obj.NamEnglish,
                    Address = obj.Address,
                    ShowReferees = obj.ShowReferees,
                    ShowCheckers = obj.ShowCheckers,
                    ShowCompetitors = obj.ShowCompetitors,
                    UserName = obj.User.UserName,
                    ImagePath = protocol + "://" + hostName + "/uploads/Competition-Document/" + obj.Image,
                    CompetitionType = obj.CompetitionType,
                    IsCheckersAllocated = obj.CheckersAllocatedDate != null,
                    IsRefereesAllocated = obj.RefereesAllocatedDate != null,
                    IsChecker = obj.CompetitionCheckers
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && !i.IsBoss),
                    IsCheckerBoss = obj.CompetitionCheckers
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.IsBoss && i.ChangeByOwnerDateTime == null),
                    IsReferee = obj.CompetitionReferees
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && !i.IsBoss),
                    IsBossReferee = obj.CompetitionReferees
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.IsBoss && i.ChangeByOwnerDateTime == null),
                    IsCompetitor = obj.CompetitionInvites
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID),
                    HasPicked =    obj.CompetitionCheckers
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && !i.IsBoss && i.PickupDateTime != null)
                                    || obj.CompetitionReferees
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && !i.IsBoss && i.PickupDateTime != null) 
                                    ,
                    HasJoined =    obj.CompetitionCheckers
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.IsBoss != true && i.PickupDateTime != null && i.JoinDateTime != null) ||
                                   obj.CompetitionCheckers
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.IsBoss == true && i.JoinDateTime != null) ||
                                    obj.CompetitionReferees
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID &&i.IsBoss ==false&& i.PickupDateTime != null && i.JoinDateTime != null) ||
                                    obj.CompetitionReferees
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.IsBoss == true && i.JoinDateTime != null)||
                                     obj.CompetitionInvites
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.JoinDateTime != null),
                    HasRejected = obj.CompetitionCheckers
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.PickupDateTime != null && i.RejectDateTime != null) ||
                                    obj.CompetitionReferees
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.IsBoss == false && i.PickupDateTime != null && i.RejectDateTime != null) ||
                                    obj.CompetitionReferees
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID  && i.IsBoss == true  && i.RejectDateTime != null) ||

                                    obj.CompetitionInvites
                                    .Where(i => !i.IsDeleted)
                                    .Any(i => i.UserID == userID && i.RejectDateTime != null)
           
                 }).OrderByPropertyName(orderBy, isAscending);
            result = data.Skip(excludedRows).Take(pageSize).ToList();
            var res=  new PagingViewModel<V2CompetitionViewModel>() { PageIndex = pageIndex, PageSize = pageSize, Result = result, Records = records, Pages = pages };
            return res;
        }
        public bool PublishCompetition(int userID, int competitionID)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var data = _repo.GetAll().Where(x => x.ID == competitionID && x.UserID == userID && x.Completed != null).Select(x=>new { 
            Competition = x,
            CompetitionUser = x.User,
            Competiters = x.CompetitionInvites
            //,
          //  Checkers = x.CompetitionCheckers.Where(c=> c.JoinDateTime != null).ToList(),
          }).FirstOrDefault();
            if(data.Competition.Published != null)
            {
                throw new Exception("لقد تم النشر من قبل");

            }
            if (data.Competition.Completed == null)
            {
                throw new Exception("رئيس التحكيم لم يعتمد النتائج بعد");
            }
            var comImg = protocol + "://" + hostName + "/uploads/Competition-Document/" + data.Competition.Image;

            if (data.Competition != null)
            {
                if (data.Competiters != null && data.Competiters.Count > 0)

                {
                    var notifcation = new NotificationCreateViewModel
                    {
                        ContentArabic = $"{NotificationArabicKeys.PublishCompetition} {data.Competition.NameArabic}",
                        ContentEnglish = $"{NotificationEnglishKeys.PublishCompetition}{data.Competition.NamEnglish}",
                        NotificationTypeID = 17,
                        EngNotificationType = $"{NotificationEnglishKeys.PublishCompetition}{data.Competition.NamEnglish}",
                        ArbNotificationType = $"{NotificationArabicKeys.PublishCompetition} {data.Competition.NameArabic}",
                        SourceID = data.Competition.UserID,
                         SourceName = data.CompetitionUser.UserName,
                        CompetitionImagePath = comImg,
                        CompetitionID = data.Competition.ID,

                    };

                    _notificationService.SendNotifictionForInvites(notifcation, data.Competiters.Select(x=> new CompetitionInviteCreateViewModel { 
                     UserID = x.UserID,
                     CompetitionID = x.CompetitionID
                    }).ToList());

                    _repo.SaveIncluded(new Competition { ID = competitionID, Published = DateTime.UtcNow }, "Published");
                    _unit.Save();
                }
                //make a post
                //var newPost = _postRepository.Add(new Post
                //{
                //    UserID = userID,
                //    PostStatus = (int)PostStatus.SharedWithPublic,
                //    PostType = (int)PostType.Image,
                //    PostDocuments = new List<PostDocument>()
                //    {
                //        new PostDocument
                //        {
                //             FileName = data.Competition.Image,
                //             Type = "Image" 
                //        }

                //    },
                //    Text = $"{data.Competition.NameArabic} تم نشر مسابقة " 
                //});
                //FileHelper.MoveFileFromTempPathToAnotherFolder(data.Competition.Image, "Post-Document");
                //_unit.Save();

                return true;
            }
            return false;
        }

        public bool ChangeRefereeBoss(ChangeRefereeBossCreateViewModel viewModel)
        {
            var referees = 
                _competitonRefereeRepository.GetAll()
                    .Where(x => x.CompetitionID == viewModel.CompetitionID)
                    .Where(x => x.ID == viewModel.OldRefereeID || x.ID == viewModel.NewRefereeID)
                    .ToList();
            var oldReferee = referees.Where(x => x.ID == viewModel.OldRefereeID).FirstOrDefault();
            if (!oldReferee.IsBoss)
            {
                throw new Exception($"this referee is not boss");
            }
            oldReferee.ChangeByOwnerDateTime = DateTime.UtcNow;
            referees.Where(x=> x.ID == viewModel.NewRefereeID).FirstOrDefault().IsBoss = true;

            _unit.Save();

            return true;
        }

        public bool ChangeCheckerBoss(ChangeCheckerBossCreateViewModel viewModel)
        {
            var checkers =
                _competitonCheckerRepository.GetAll()
                    .Where(x => x.CompetitionID == viewModel.CompetitionID)
                    .Where(x => x.ID == viewModel.OldCheckerID || x.ID == viewModel.NewCheckerID)
                    .ToList();
            var oldChecker = checkers.Where(x => x.ID == viewModel.OldCheckerID).FirstOrDefault();
            if (!oldChecker.IsBoss)
            {
                throw new Exception("this referee is not boss");
            }
           
            oldChecker.ChangeByOwnerDateTime = DateTime.UtcNow;
            checkers.Where(x => x.ID == viewModel.NewCheckerID).FirstOrDefault().IsBoss = true;

            _unit.Save();

            return true;
        }

        public bool AddNewCheckerBoss(NewCheckerBossCreateViewModel viewModel)
        {
            var competition = _repo.GetAll()
                                    .Where(x => x.ID == viewModel.CompetitionID).FirstOrDefault();

            if(competition.StartedDate != null)
            {
                throw new Exception("المسابقة بدأت بالفعل ولا يمكنك تغيير رئيس التمييز");
            }

            var checkers =
                _competitonCheckerRepository.GetAll()
                    .Where(x => x.CompetitionID == viewModel.CompetitionID)
                    .ToList();
            var oldChecker = checkers.Where(x => x.IsBoss).FirstOrDefault();
            if (oldChecker != null)
            {
                throw new Exception("this referee is not boss");
            }
            //oldChecker.IsBoss = false;

            //you have to check if selected user is not in referees list or competitors list
            _competitonCheckerRepository.Add(new CompetitionChecker
            {
                UserID = viewModel.UserID,
                CompetitionID = viewModel.CompetitionID,
                IsBoss = true
            });
          
            _unit.Save();
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var comImg = protocol + "://" + hostName + "/uploads/Competition-Document/" + competition.Image;

            var notifcation = new NotificationCreateViewModel
            {
                ID = competition.ID,
                ContentArabic = $"{NotificationArabicKeys.NewCompetitionAnnounceToChecker}{competition.NameArabic}",
                ContentEnglish = $"{NotificationEnglishKeys.NewCompetitionAnnounceToChecker}{competition.NamEnglish}",
                NotificationTypeID = (int)TypeOfNotification.CheckerRequestForJoinCompetition,
                ArbNotificationType = "الانضمام الي لجنة التمييز",
                EngNotificationType = "Join the competition as Checker boss",
                SourceID = competition.UserID,
                DestinationID = viewModel.UserID,
                //    SourceName = insertedCompetition.User.Name,
                CompetitionImagePath = comImg,
                CompetitionID = competition.ID,

            };

            _notificationService.SendNotifictionForUser(notifcation);

            return true;
        }

        public bool AddNewRefereeBoss(NewRefereeBossCreateViewModel viewModel)
        {
            var competition = _repo.GetAll()
                                    .Where(x => x.ID == viewModel.CompetitionID).FirstOrDefault();

            if (competition.StartedDate != null)
            {
                throw new Exception("المسابقة بدأت بالفعل ولا يمكنك تغيير رئيس التمييز");
            }

            var checkers =
                _competitonRefereeRepository.GetAll()
                    .Where(x => x.CompetitionID == viewModel.CompetitionID)
                    .ToList();
            var oldChecker = checkers.Where(x => x.IsBoss).FirstOrDefault();
            if (oldChecker != null)
            {
                throw new Exception("this referee is not boss");
            }
           // oldChecker.IsBoss = false;
           
            _competitonRefereeRepository.Add(new CompetitionReferee
            {
                UserID = viewModel.UserID,
                CompetitionID = viewModel.CompetitionID,
                IsBoss = true
            });

            _unit.Save();
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var comImg = protocol + "://" + hostName + "/uploads/Competition-Document/" + competition.Image;

            var notifcation = new NotificationCreateViewModel
            {
                ID = competition.ID,
                ContentArabic = $"{NotificationArabicKeys.NewCompetitionAnnounceToReferee}{competition.NameArabic}",
                ContentEnglish = $"{NotificationEnglishKeys.NewCompetitionAnnounceToReferee}{competition.NamEnglish}",
                NotificationTypeID = (int)TypeOfNotification.CheckerRequestForJoinCompetition,
                ArbNotificationType = $"{NotificationArabicKeys.NewCompetitionAnnounceToReferee}{competition.NameArabic}",
                EngNotificationType = $"{NotificationEnglishKeys.NewCompetitionAnnounceToReferee}{competition.NamEnglish}",
                SourceID = competition.UserID,
                DestinationID = viewModel.UserID,
                //    SourceName = insertedCompetition.User.Name,
                CompetitionImagePath = comImg,
                CompetitionID = competition.ID,

            };

            _notificationService.SendNotifictionForUser(notifcation);

            return true;
        }
        public bool InviteCompetitors(int userID, int competitionID)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var data = _repo.GetAll().Where(x => x.ID == competitionID)
                        .Select(x => new
                        {
                            Competition = x,
                            Checkers = x.CompetitionCheckers,
                            Referees = x.CompetitionReferees
                        }).FirstOrDefault();
            if(data.Competition.CompetitorsInvitedDate != null)
            {
                throw new Exception("لقد قمت بدعوة المتسابقين من قبل");
            }
            //add competitors first
            var checkerUserIDs = data.Checkers.Select(c => c.UserID).ToList();
            var refereeUserIDs = data.Referees.Select(c => c.UserID).ToList();
            var competitors = _userRepository.GetAll()
                             .Where(x => x.ID != data.Competition.UserID)
                             .Where(x => !x.IsDeleted)
                             .Where(x => !checkerUserIDs.Contains(x.ID))
                             .Where(x => !refereeUserIDs.Contains(x.ID))
                             .Select(x => new CompetitionInviteCreateViewModel
                             {
                                 UserID = x.ID
                             })
                             .ToList();
            competitors.ForEach(x => x.CompetitionID = competitionID);
            
            _competitonInviteRepository.AddRange(competitors.Select(x => x.ToModel()));

            if (data.Competition.UserID != userID)
            {
                throw new Exception("لا يمكنك دعوة المتسابقين الا اذا كنت منشي للمسابقة");
            }
            if (data.Competition.MaximumCheckersCount <
                data.Checkers.Where(c => c.JoinDateTime != null && c.PickupDateTime != null).Count())
               // ||
               //data.Competition.MinimumCheckersCount >
               // data.Checkers.Where(c => c.JoinDateTime != null && c.PickupDateTime != null).Count())
            {
                throw new Exception("لم يتم اعتماد فرق اللجان");

                //     throw new Exception("لقد تجاوز عدد المميزين المعتمدين الحد الاقصي المسموح به");
            }
            if (data.Competition.MinimumCheckersCount >
                data.Checkers.Where(c => c.JoinDateTime != null && c.PickupDateTime != null).Count())
            {
                throw new Exception("لم يتم اعتماد فرق اللجان");
               // throw new Exception("لم يتجاوز عدد المميزين المعتمدين الحد الادني المسموح به");

            }
            if (data.Competition.MaximumRefereesCount <
                data.Referees.Where(c => c.JoinDateTime != null && c.PickupDateTime != null).Count())
               // ||
               //data.Competition.MinimumRefereesCount >
               // data.Referees.Where(c => c.JoinDateTime != null && c.PickupDateTime != null).Count())

            {
                throw new Exception("لم يتم اعتماد فرق اللجان");

                // throw new Exception("لقد تجاوز عدد الحكام المعتمدين الحد الاقصي المسموح به");
            }
            if (data.Competition.MinimumRefereesCount >
                data.Referees.Where(c => c.JoinDateTime != null && c.PickupDateTime != null).Count())
            {
                throw new Exception("لم يتم اعتماد فرق اللجان");

                // throw new Exception("لم يتجاوز عدد الحكام المعتمدين الحد الادني المسموح به");

            }


            if (data.Referees.Where(c => c.JoinDateTime != null && c.PickupDateTime != null).Count() % 2 == 0)
            {
                throw new Exception("يجب ان يكون عدد المحكممين فرديا");
            }
            if (data.Referees.Where(c => c.JoinDateTime != null && c.PickupDateTime != null).Count() == 1)
            {
                throw new Exception("لا يمكن بدء المسابقة بمحكم واحد فقط");
            }


            var comImg = protocol + "://" + hostName + "/uploads/Competition-Document/" + data.Competition.Image;
            if (competitors != null && competitors.Count > 0)
            {
                var notifcation = new NotificationCreateViewModel
                {
                    ID = data.Competition.ID,
                    ContentArabic = $"{NotificationArabicKeys.NewCompetitionAnnounceToCompetitor}" + ' ' + $"{ data.Competition.NameArabic }",
                    ContentEnglish = $"{NotificationEnglishKeys.NewCompetitionAnnounceToCompetitor}" + ' ' + $"{ data.Competition.NamEnglish}",
                    NotificationTypeID = 1,
                    ArbNotificationType = $"{NotificationArabicKeys.NewCompetitionAnnounceToCompetitor}" + ' ' + $"{ data.Competition.NameArabic}",
                    EngNotificationType = $"{NotificationEnglishKeys.NewCompetitionAnnounceToCompetitor}" + ' ' + $"{ data.Competition.NamEnglish}",
                    SourceID = userID,
                    //  SourceName = insertedCompetition.User.Name,
                    CompetitionImagePath = comImg,
                    CompetitionID = data.Competition.ID,

                };
                
                _notificationService.SendNotifictionForInvites(notifcation, 
                    competitors.Select(x => new CompetitionInviteCreateViewModel { UserID = x.UserID}).ToList());
            }
            //mark competition as invited
            data.Competition.CompetitorsInvitedDate = DateTime.Now;
            _unit.Save();
            return true;
        }

        public bool StartRefereeing(int userID, int competitionID)
        {
            var data = _repo.GetAll().Where(x => x.ID == competitionID)
                     .Select(x => new
                     {
                         Competition = x,
                         Checkers = x.CompetitionCheckers,
                         Referees = x.CompetitionReferees,
                         Competitors = x.CompetitionInvites
                     }).FirstOrDefault();
            if (data.Competition.UserID != userID)
            {
                throw new Exception("لا يمكنك بدء المسابقة الا اذا كنت منشي للمسابقة");
            }
            var isCheckingFinished =
                  _competitonCamelRepository.GetAll()
                  .Where(x => x.CompetitionID == competitionID)
                  .All(x => x.RejectedByCheckerBossDateTime != null || x.ApprovedByCheckerBossDateTime != null);
            if (!isCheckingFinished)
            {
                throw new Exception("لم ينتهي التمييز بعد");
            }
            data.Competition.RefereeingStarted = DateTime.Now;
            _unit.Save();
            return true;
        }

        public bool StartCompetition(int userID, int competitionID)
        {
            var data = _repo.GetAll().Where(x => x.ID == competitionID)
                        .Select(x => new
                        {
                            Competition = x,
                            Checkers = x.CompetitionCheckers,
                            Referees = x.CompetitionReferees,
                            Competitors = x.CompetitionInvites
                        }).FirstOrDefault();
            if(data.Competition.UserID != userID)
            {
                throw new Exception("لا يمكنك بدء المسابقة الا اذا كنت منشي للمسابقة");
            }
            if (data.Competition.MaximumCheckersCount < 
                data.Checkers.Where(c=>c.JoinDateTime != null && c.PickupDateTime != null).Count())
               // ||
               //data.Competition.MinimumCheckersCount >
               // data.Checkers.Where(c => c.JoinDateTime != null && c.PickupDateTime != null).Count())
            {
                throw new Exception("لم يتم اعتماد فرق اللجان");

                //throw new Exception("لقد تجاوز عدد المميزين المعتمدين الحد الاقصي المسموح به");
            }
            if (data.Competition.MinimumCheckersCount >
                data.Checkers.Where(c => c.JoinDateTime != null && c.PickupDateTime != null).Count())
            {
                throw new Exception("لم يتم اعتماد فرق اللجان");

                // throw new Exception("لم يتجاوز عدد المميزين المعتمدين الحد الادني المسموح به");

            }

            if (data.Competition.MaximumRefereesCount <
                data.Referees.Where(c => c.JoinDateTime != null && c.PickupDateTime != null).Count())
            // ||
            //data.Competition.MinimumRefereesCount >
            // data.Referees.Where(c => c.JoinDateTime != null && c.PickupDateTime != null).Count())

            {
                throw new Exception("لم يتم اعتماد فرق اللجان");

                //throw new Exception("لقد تجاوز عدد الحكام المعتمدين الحد الاقصي المسموح به");
            }
            if (data.Competition.MinimumRefereesCount >
                data.Referees.Where(c => c.JoinDateTime != null && c.PickupDateTime != null).Count())
            {
                //throw new Exception("لم يتجاوز عدد الحكام المعتمدين الحد الادني المسموح به");
                throw new Exception("لم يتم اعتماد فرق اللجان");

            }

            if (data.Referees.Where(c => c.JoinDateTime != null && c.PickupDateTime != null).Count() % 2 == 0)
            {
                throw new Exception("يجب ان يكون عدد المحكممين فرديا");

            }
            if (data.Referees.Where(c => c.JoinDateTime != null && c.PickupDateTime != null).Count()  == 1)
            {
                throw new Exception("لا يمكن بدء المسابقة بمحكم واحد فقط");

            }
            //if competition is public , we will let competitors to join without limit and then filter in checking phase
            if (data.Competition.MaximumCompetitorsCount <
                data.Competitors.Where(c => c.JoinDateTime != null && c.SubmitDateTime != null).Count() &&
                data.Competition.CompetitionType == (int) CompetitionType.PrivateForInvites)
            {
                throw new Exception("عدد المتسابقين المشتركين تجاوز العدد المحدد للمسابقة");
            }

            if(data.Competition.MinimumCompetitorsCount >
                data.Competitors.Where(c => c.JoinDateTime != null && c.SubmitDateTime != null).Count() &&
                data.Competition.CompetitionType == (int)CompetitionType.PrivateForInvites)
            {
                throw new Exception("عدد المتسابقين المشتركين لم يكتمل للعدد المحدد للمسابقة");
            }
            if(data.Competition.CompetitionType == (int)CompetitionType.PublicForAll &&
                 data.Competition.MinimumCompetitorsCount >
                data.Competitors.Where(c => c.JoinDateTime != null && c.SubmitDateTime != null).Count())
            {
                throw new Exception("عدد المتسابقين المشتركين لم يكتمل للعدد المحدد للمسابقة");
            }

            data.Competition.StartedDate = DateTime.UtcNow;
            _unit.Save();
            return  true;
        }

        public CompetitionEditViewModel GetEditableByID(int id)
        {

            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var competition = _repo.GetAll().Where(comp => comp.ID == id)
                .Select(obj => new CompetitionEditViewModel
                {
                    ID = obj.ID,
                    NameArabic = obj.NameArabic,
                    NameEnglish = obj.NamEnglish,
                    Address = obj.Address,
                    CamelsCount = obj.CamelsCount,
                    From = obj.From,
                    To = obj.To,
                     CategoryID = obj.CategoryID,
                     CompetitionType = (CompetitionType)obj.CompetitionType,
                     Image = obj.Image,
                     MaximumCompetitorsCount = obj.MaximumCompetitorsCount,
                     PeopleVotePercentage = obj.PeopleVotePercentage,
                     MaximumRefereesCount = obj.MaximumRefereesCount,
                     RefereesVotePercentage = obj.RefereesVotePercentage,
                    Invites = obj.CompetitionInvites.Select(x=> new CompetitionInviteEditViewModel
                    {
                        ID = x.ID,
                        CompetitionID = x.CompetitionID,
                        UserID = x.UserID
                    }).ToList(),
                    Conditions = obj.CompetitionConditions.Select(x => new CompetitionConditionEditViewModel
                    {
                        ID = x.ID,
                        CompetitionID = x.CompetitionID,
                        TextArabic = x.TextArabic,
                        TextEnglish = x.TextEnglish
                    }).ToList(),
                    Rewards = obj.CompetitionRewards.Select(x => new CompetitionRewardEditViewModel
                    {
                        ID = x.ID,
                        CompetitionID = x.CompetitionID,
                        NameArabic = x.NameArabic,
                        SponsorID = x.SponsorID,
                        NamEnglish = x.NameEnglish,
                        SponsorText = x.SponsorText
                    }).ToList(),
                    Referees = obj.CompetitionReferees.Select(x => new CompetitionRefereeEditViewModel
                    {
                        ID = x.ID,
                        CompetitionID = x.CompetitionID,
                        UserID = x.UserID
                    }).ToList(),
                    Checkers = obj.CompetitionCheckers.Select(x => new CompetitionCheckerEditViewModel
                    {
                        ID = x.ID,
                        CompetitionID = x.CompetitionID,
                        UserID = x.UserID
                    }).ToList(),
                    
                }).FirstOrDefault();

            return competition; 
        }

        public bool IsExists(int id)
        {
            return _repo.GetAll().Where(x => x.ID == id).Any();
        }
        public bool IsAllowedToEdit(int id)
        {
            var notAllowed = 
            _competitonCamelRepository.GetAll()
                .Any(x => x.CompetitionID == id && !x.IsDeleted);

            return !notAllowed;
        }

        public void Delete(int id)
        {
            var competition = _repo.GetAll().Where(comp => comp.ID == id).FirstOrDefault();
          
                _repo.RemoveByIncluded(competition);
            
        }
        public List<CompetitionGeneralConditionViewModel> GetHelpingConditions()
        {
           return
            _competitonGeneralConditionRepository.GetAll().Select(x => new CompetitionGeneralConditionViewModel 
            { 
                TextArabic = x.TextArabic
            }).ToList();
        }

        public List<CompetitionGeneralConditionViewModel> GetCompetitionConditions(int competitionID)
        {
            return
             _competitonConditionRepository.GetAll().Where(x=> x.ID == competitionID).Select(x => new CompetitionGeneralConditionViewModel
             {
                 TextArabic = x.TextArabic
             }).ToList();
        }

        public List<PublishedCompetitionViewModel> GetPublishedCompetitions(int userID)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var competitions = _repo.GetAll()
              // .Where(c => c.To <= dateToday)
              .Where(x => x.Published != null)
              //.Where(c => c.UserID == userID ||
              //            c.CompetitionInvites
              //                    .Where(i => !i.IsDeleted)
              //                    .Any(i => i.UserID == userID ) ||
              //            c.CompetitionReferees
              //                    .Where(i => !i.IsDeleted)
              //                    .Any(i => i.UserID == userID && !i.IsBoss && i.PickupDateTime != null && i.RejectDateTime == null) ||
              //           c.CompetitionReferees
              //                    .Where(i => !i.IsDeleted)
              //                    .Any(i => i.UserID == userID && i.IsBoss && i.RejectDateTime == null) ||
              //            c.CompetitionCheckers
              //                    .Where(i => !i.IsDeleted)
              //                    .Any(i => i.UserID == userID && !i.IsBoss && i.PickupDateTime != null && i.RejectDateTime == null) ||
              //            c.CompetitionCheckers
              //                    .Where(i => !i.IsDeleted)
              //                    .Any(i => i.UserID == userID && i.IsBoss && i.RejectDateTime == null))
              .Select(x => new PublishedCompetitionViewModel
              {
                  ID = x.ID,
                  NameArabic = x.NameArabic,
                  NameEnglish = x.NamEnglish,
                  Image = protocol + "://" + hostName + "/uploads/Competition-Document/" + x.Image
              }).ToList();

            return competitions;
        }

        //private void AssignInvitesToCheckers(int competitionID)
        //{
        //    //get number of submitted checkers
        //    var submittedCheckers =
        //            _competitonCheckerRepository.GetAll()
        //                .Where(x => x.CompetitionID == competitionID && !x.IsBoss && x.SubmitDateTime.HasValue)
        //                .ToList();
        //    var submittedCheckersCount = submittedCheckers.Count;

        //    //get number of submitted invites
        //    var submittedInvites = _competitonInviteRepository.GetAll()
        //                                .Where(x => x.CompetitionID == competitionID && x.SubmitDateTime.HasValue)
        //                                .ToList();
        //    var submittedInvitesCount = submittedInvites.Count;
        //    //start to assign
        //    var numberOfInvitesPerChecker = submittedInvitesCount / submittedCheckersCount;
        //    foreach (var item in submittedCheckers)
        //    {
        //        int i = 0;
        //        while (i < numberOfInvitesPerChecker)
        //        {
        //            submittedInvites[i++].CheckerID = item.ID;
        //        }
        //    }
        //    submittedInvites
        //        .Where(x => x.CheckerID == null)
        //        .ToList()
        //        .ForEach(x => x.CheckerID = submittedCheckers[submittedCheckers.Count - 1].ID);

        //}

    }

    public class CompetitionGeneralConditionViewModel
    {
        public string TextArabic { get; set; }

    }

    public class PublishedCompetitionViewModel
    {
        public int ID { get; set; }
        public string NameArabic { get; set; }
        public string NameEnglish { get; set; }
        public string Image { get; set; }
    }

    public class RewardsComparer : IEqualityComparer<CompetitionRewardCreateViewModel>
    {
        public bool Equals(CompetitionRewardCreateViewModel x, CompetitionRewardCreateViewModel y)
        {
            return x.Rank == y.Rank;
        }

        public int GetHashCode(CompetitionRewardCreateViewModel obj)
        {
            return 0;
        }

       
    }
}

