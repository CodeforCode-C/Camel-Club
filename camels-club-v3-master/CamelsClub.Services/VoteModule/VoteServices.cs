using CamelsClub.Data.Extentions;
using CamelsClub.Data.UnitOfWork;
using CamelsClub.Models;
using CamelsClub.Repositories;
using CamelsClub.ViewModels;
using CamelsClub.ViewModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CamelsClub.Services
{
    public class VoteService : IVoteService
    {
        private readonly IUnitOfWork _unit;
        private readonly ICamelCompetitionRepository _repo;
        private readonly ICompetitionRepository _competitionRepository;
        private readonly ICamelRepository _camelRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly ICompetitionInviteRepository _competitionInviteRepository;
        private readonly IUserCamelReviewRepository _userCamelReviewRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;

        public VoteService(IUnitOfWork unit,
                                       ICamelCompetitionRepository repo,
                                       IGroupRepository groupRepository,
                                       ICamelRepository camelRepository,
                                       IUserCamelReviewRepository userCamelReviewRepository,
                                       ICompetitionRepository competitionRepository,
                                       ICompetitionInviteRepository competitionInviteRepository,
                                       IUserRepository userRepository,
                                       INotificationService notificationService)
        {
            _unit = unit;
            _repo = repo;
            _competitionRepository = competitionRepository;
            _camelRepository = camelRepository;
            _groupRepository = groupRepository;
            _userCamelReviewRepository = userCamelReviewRepository;
            _competitionInviteRepository = competitionInviteRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
        }




        // GetCompetitorGroups by competitionID & competitorID
        public List <GroupViewModel> GetCompetitorGroups(int competitionID, int competitorID)
        {
            return
            _repo.GetAll()
                .Where(x => x.CompetitionInviteID == competitorID && x.CompetitionID == competitionID)
                .GroupBy(x => x.Group)
                .Select(g => new GroupViewModel
                {
                    ID = g.Key.ID,
                    NameArabic = g.Key.NameArabic,
                    NameEnglish = g.Key.NameEnglish
                }).ToList();
        }

        // GetCompetitorGroups by competitionID & competitorID
        public List<VoteCamelCompetitionViewModel> GetGroupCamels(int competitionID, int groupID, int loggedUserId)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var camels = 
            _repo.GetAll()
                .Where(x =>  x.CompetitionID == competitionID && x.GroupID == groupID)
                .Select(x => new VoteCamelCompetitionViewModel
                {
                    ID = x.ID,
                    CamelName = x.Camel.Name,
                    CamelImages = x.Camel.CamelDocuments.Select(d => new CamelDocumentViewModel
                    {
                        FilePath = protocol + "://" + hostName + "/uploads/Camel-Document/" + d.FileName
                    }).ToList()

                }).ToList();
            var camelIDs = camels.Select(x => x.ID).ToList();

            // user reviews for this competition
            var userReviews = 
            _userCamelReviewRepository.GetAll()
                .Where(x => x.UserID == loggedUserId && camelIDs.Contains(x.CamelCompetitionID));
            foreach (var item in camels)
            {
                item.IsEvaluated = userReviews.Any(r => r.CamelCompetitionID == item.ID);
            }

            return camels;
        }

        /// <summary>
        /// Get Competition that can be voted, must be started and not finished
        /// </summary>
        /// <returns></returns>
        public List<CompetitionViewModel> GetCompetitons()
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            return
            _competitionRepository.GetAll()
                .Where(x => x.StartedDate != null && x.Published == null)
                .Select(x => new CompetitionViewModel
                {
                    ID = x.ID,
                    NameArabic = x.NameArabic,
                    NamEnglish = x.NamEnglish,
                    ImagePath = protocol + "://" + hostName + "/uploads/Competition-Document/" +x.Image
                }).ToList();
        }
        // get groups in compitions
        public List<GroupViewModel> GetCompetitionGroups(int competitionID)
        {
            return
            _repo.GetAll()
                .Where(x => x.CompetitionID == competitionID)
                .GroupBy(x => x.Group)
                .Select(g => new GroupViewModel
                {
                    ID = g.Key.ID,
                    NameArabic = g.Key.NameArabic,
                    NameEnglish = g.Key.NameEnglish
                }).ToList();
        }
        //  Get Competitors in Competition that voted
        public List<CompetitionInviteViewModel> GetCompetitionCompetitors(int competitionID)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            return
            _repo.GetAll()
                .Where(x => x.CompetitionID == competitionID)
                .GroupBy(x => x.CompetitionInvite)
                .Select(g => new CompetitionInviteViewModel
                {
                    ID = g.Key.ID,
                    DisplayName = g.Key.User.DisplayName,
                    UserName = g.Key.User.UserName,
                    UserImage = protocol + "://" + hostName + "/uploads/User-Document/"+ g.Key.User.UserProfile.MainImage
                }).ToList();
        }

        public bool EvaluateCamel(UserCamelReviewCreateViewModel viewModel)
        {

            foreach (var camelSpec in viewModel.CamelsSpecificationValues)
            {

                _userCamelReviewRepository.Add(
                     new UserCamelSpecificationReview
                     {
                         CamelCompetitionID = viewModel.CamelCompetitionID,
                         UserID = viewModel.UserID,
                         CamelSpecificationID = camelSpec.CamelSpecificationID,
                         ActualPercentageValue = camelSpec.SpecificationValue

                     });
            }

            _unit.Save();
            return true;
        }

    }

    public class VoteCamelCompetitionViewModel
    {
        public int ID { get; set; }
        public string CamelName { get; set; }
        public List<CamelDocumentViewModel> CamelImages { get; set; }
        public bool IsEvaluated { get; set; }
    }
}

