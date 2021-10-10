using CamelsClub.Data.Extentions;
using CamelsClub.Data.UnitOfWork;
using CamelsClub.Models;
using CamelsClub.Models.Enums;
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
    public class PostUserActionService : IPostUserActionService
    {
        private readonly IUnitOfWork _unit;
        private readonly IPostUserActionRepository _repo;
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IPostUserActionRepository _postUserActionRepository;
        public PostUserActionService(IUnitOfWork unit, IPostUserActionRepository repo, IPostUserActionRepository postUserActionRepository, ICommentRepository commentRepository, INotificationService notificationService, IUserRepository userRepository)
        {
            _unit = unit;
            _repo = repo;
            _commentRepository = commentRepository;
            _postUserActionRepository = postUserActionRepository;
            _notificationService = notificationService;
            _userRepository = userRepository;
        }
        public PagingViewModel<PostUserActionViewModel> Search(int postID = 0 ,  string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var query = _repo.GetAll().
                            Where(x => !x.IsDeleted).
                            Where(x => x.IsActive)
                  ;




            int records = query.Count();
            if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
            int pages = (int)Math.Ceiling((double)records / pageSize);
            int excludedRows = (pageIndex - 1) * pageSize;

            List<PostUserActionViewModel> result = new List<PostUserActionViewModel>();

            var posts = query.Select(obj => new PostUserActionViewModel
            {
                ID = obj.ID,
                ActionID = obj.ActionID,    
                PostID = obj.PostID ,
                UserID = obj.UserID
            }).OrderByPropertyName(orderBy, isAscending);

            result = posts.Skip(excludedRows).Take(pageSize).ToList();
            return new PagingViewModel<PostUserActionViewModel>() { PageIndex = pageIndex, PageSize = pageSize, Result = result, Records = records, Pages = pages };
        }

        public PagingViewModel<LikeViewModel> GetLikes(int postID, int pageIndex, int pageSize)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var query = _repo.GetAll()
                            .Where(x => x.PostID == postID)
                            .Where(x => x.ActionID == (int)Actions.Like)
                            .Where(x => x.IsActive);


            int records = query.Count();
            if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
            int pages = (int)Math.Ceiling((double)records / pageSize);
            int excludedRows = (pageIndex - 1) * pageSize;

            List<LikeViewModel> result = new List<LikeViewModel>();

            var posts = query.Select(obj => new LikeViewModel
            {
                ID = obj.ID,
                ActionID = obj.ActionID,
                UserID = obj.UserID,
                UserName = obj.User.UserName,
                DisplayName = obj.User.DisplayName,
                Image = obj.User.UserProfile.MainImage != null ? protocol + "://" + hostName + "/uploads/User-Document/" + obj.User.UserProfile.MainImage : null
            }).OrderByDescending(x => x.ID);

            result = posts.Skip(excludedRows).Take(pageSize).ToList();
            return new PagingViewModel<LikeViewModel>() { PageIndex = pageIndex, PageSize = pageSize, Result = result, Records = records, Pages = pages };

        }


        //it acts as Delete , but in new form
        public bool Add(PostUserActionCreateViewModel viewModel)
        {
            PostUserAction res;
            var postUserAction =
                _repo.GetAll().FirstOrDefault(x => !x.IsDeleted &&
                                    x.PostID == viewModel.PostID &&
                                    x.UserID == viewModel.UserID &&
                                    x.ActionID == (int)Actions.Like
                                    );
            if(postUserAction != null) 
            {
                res= _repo.SaveIncluded(new PostUserAction
                {
                    ID = postUserAction.ID,
                    IsActive = !postUserAction.IsActive
                }, "IsActive");

                _unit.Save();

                return res.IsActive;
            }
            else
            {
                res=_repo.Add(viewModel.ToModel());
                _unit.Save();
                List<int> usersIDs = new List<int>();
                var usersWhoCommented = _commentRepository.GetAll().Where(x => x.PostID == viewModel.PostID).Select(x => x.UserID).ToList();
                var usersWhoLiked = _postUserActionRepository.GetAll()
                        .Where(x => x.PostID == viewModel.PostID && x.ActionID == (int)Actions.Like).Select(x => x.UserID).ToList();

                if (viewModel.UserID != viewModel.UserID)
                {
                    usersIDs.Add(viewModel.UserID);
                }
                usersIDs.AddRange(usersWhoCommented);
                usersIDs.AddRange(usersWhoLiked);
                usersIDs = usersIDs.Distinct().ToList();
                usersIDs = usersIDs.Where(x => x != viewModel.UserID).ToList();

                //var usersIDs = _friendRepository.FriendUsersIDs(viewModel.UserID);
                var user = _userRepository.GetById(viewModel.UserID);
                var notifcation = new NotificationCreateViewModel
                {
                    ContentArabic = $"{user.DisplayName} قام بالاعجاب بالمنشور الخاص بك",
                    ContentEnglish = $"{user.DisplayName} has liked Your Post",
                    NotificationTypeID = 7,
                    SourceID = viewModel.UserID,
                   // DestinationID = res.Post.UserID,
                    PostID = viewModel.PostID,
                    ActionID=viewModel.ActionID

                };

                _notificationService.SendNotifictionForFriends(notifcation, usersIDs);

               // _notificationService.SendNotifictionForUser(notifcation);
                return true;
            }
            
        }

        public void Edit(PostUserActionCreateViewModel viewModel)
        {
            _repo.SaveIncluded(viewModel.ToModel(), "ActionID");
            
        }

        public PostUserActionViewModel GetByID(int id)
        {

            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var postUserAction = _repo.GetAll().Where(postAction => postAction.ID == id)
                .Select(obj => new PostUserActionViewModel
            {
                ID = obj.ID,
                UserID = obj.UserID,
                PostID = obj.PostID,
                ActionID = obj.ActionID
            }).FirstOrDefault();
            
            return postUserAction;

        }


        public bool IsExists(int id)
        {
            return _repo.GetAll().Where(x => x.ID == id).Any();
        }


      }

    public class LikeViewModel
    {
        public int ID { get; set; }
        public int ActionID { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Image { get; set; }
    }
}

