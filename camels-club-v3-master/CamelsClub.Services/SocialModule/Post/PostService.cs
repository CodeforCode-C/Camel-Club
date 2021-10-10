using CamelsClub.Data.Extentions;
using CamelsClub.Data.UnitOfWork;
using CamelsClub.Models.Enums;
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
using CamelsClub.Localization.Shared;

namespace CamelsClub.Services
{

    public class PostService: IPostService
    {

        private readonly IUnitOfWork _unit;
        private readonly IPostRepository _repo;
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly IPostDocumentRepository _postDocumentrepo;
        private readonly ICommentRepository _commentRepository;
        private readonly INotificationService _notificationService;
        public PostService(IUnitOfWork unit, IPostRepository repo, IPostDocumentRepository postDocumentrepo, ICommentRepository commentRepository, IFriendRequestRepository friendRequestRepository, INotificationService notificationService)
        {
            _unit = unit;
            _repo = repo;
            _postDocumentrepo = postDocumentrepo;
            _commentRepository = commentRepository;
            _friendRequestRepository = friendRequestRepository;
            _notificationService = notificationService;
        }

        public PagingViewModel<PostDetailsViewModel> GetHomePosts(int userID , string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

           
            var query = _repo.GetAll()
                            .Where(post =>post.UserID == userID)
                                          .Where(post=>!post.IsDeleted);
            
            int records = query.Count();
            if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
            int pages = (int)Math.Ceiling((double)records / pageSize);
            int excludedRows = (pageIndex - 1) * pageSize;

            List<PostDetailsViewModel> result = new List<PostDetailsViewModel>();

            var posts = query.Select(obj => new PostDetailsViewModel
            {
                ID = obj.ID,
                Text = obj.Text,
                CreatedDate = obj.CreatedDate,
                UserID = obj.UserID,
                ParentPostID = obj.ParentPostID,
                IsLiked = obj.PostUserActions.Where(x => x.UserID == userID
                                                    && x.ActionID == (int)Actions.Like
                                                    && x.IsActive && !x.IsDeleted).Any(),
                UserName = obj.User.UserName,
                UserImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + obj.User.UserProfile.MainImage,
                Notes = obj.Notes,
                NumberOfLike = obj.PostUserActions
                                     .Where(x=>x.ActionID == (int) Actions.Like)
                                     .Where(x=>!x.IsDeleted)
                                     .Where(x=>x.IsActive)
                                     .Count(),
                NumberOfComments = obj.Comments
                                     .Where(x => x.PostID == obj.ID)
                                     .Where(x => !x.IsDeleted)
                                     .Count(),
                NumberOfShare = obj.PostUserActions
                                     .Where(x => x.ActionID == (int)Actions.Share)
                                     .Where(x => !x.IsDeleted)
                                     .Count(),

                PostStatus = (PostStatus)obj.PostStatus,
                PostType = (PostType)obj.PostType,
                Documents = obj.PostDocuments.Where(doc => !doc.IsDeleted).Select(doc => new DocumentViewModel
                {
                    FilePath = protocol + "://" + hostName + "/uploads/Post-Document/" + doc.FileName,
                    UploadedDate = doc.CreatedDate,
                    FileType = doc.Type

                })




            }).OrderByPropertyName(orderBy, isAscending);
            result = posts.Skip(excludedRows).Take(pageSize).ToList();

            result.ForEach(x =>
            {
                if (x.ParentPostID.HasValue)
                {
                    var parentPost = _repo.GetAll().Where(p => p.ID == x.ParentPostID).Select(p => new
                    {
                        UserID = p.UserID,
                        Text = p.Text,
                        UserName = p.User.UserName,
                        DisplayName = p.User.DisplayName,
                        UserImage = p.User.UserProfile.MainImage,
                        Documents = p.PostDocuments
                    }).FirstOrDefault();
                    if (x.UserID == parentPost.UserID)
                    {
                        x.IsSelfShare = true;
                    }
                    x.SharedText = x.Text;
                    x.Text = parentPost.Text;
                    x.UserID = parentPost.UserID;
                    x.UserName = parentPost.UserName;
                    x.DisplayName = parentPost.DisplayName;
                    x.UserImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + parentPost.UserImage;
                    x.IsShared = true;
                    //x.ParentPostID = null;
                    x.Documents = parentPost.Documents.Select(p => new DocumentViewModel
                    {
                        FilePath = protocol + "://" + hostName + "/uploads/Post-Document/" + p.FileName,
                    }).ToList();


                }
            });

            return new PagingViewModel<PostDetailsViewModel>() { PageIndex = pageIndex, PageSize = pageSize, Result = result, Records = records, Pages = pages };
        }

        public PagingViewModel<WholePostViewModel> Search(int userID=0 , string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var query = _repo.GetAll()
                .Where(post => !post.IsDeleted)
                .Where(p => p.ParentPostID == null || !p.ParentPost.IsDeleted)
                //post has not reported by this user
                .Where(p => !p.IssueReports.Where(i => i.UserID == userID).Any())
                //user created the post must not be blocked by logged user
              //  .Where(x => !x.User.FriendsBlockedMe.Any(b => b.UserID == userID));
                .Where(x => !x.User.FromFriendRequests.Any(b => b.ToUserID == userID && b.Status == (int)FriendRequestStatus.Blocked))
                .Where(x => !x.User.ToFriendRequests.Any(b => b.FromUserID == userID && b.Status == (int)FriendRequestStatus.Blocked));
                // get posts which owners put notification setting to public
              //  .Where(x => x.User.UserNotificationSettings.Where(n => n.NotificationSettingID == (int)NotificationSettingsKey.ViewPosts && n.NotificationSettingValueID == 7).Any());
                

            int records = query.Count();
            if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
            int pages = (int)Math.Ceiling((double)records / pageSize);
            int excludedRows = (pageIndex - 1) * pageSize;
          
            List<WholePostViewModel> result = new List<WholePostViewModel>();


            var posts = query.Select(obj => new WholePostViewModel
            {
                ID = obj.ID,
                Text = obj.Text,
                UserID = obj.UserID,
                ParentPostID = obj.ParentPostID,
                IsLiked = obj.PostUserActions.Where(x=>x.UserID == userID 
                                                    && x.ActionID==(int)Actions.Like
                                                    &&x.IsActive && !x.IsDeleted).Any(), 
                CreatedDate = obj.CreatedDate,
                UserName = obj.User.UserName,
                DisplayName = obj.User.DisplayName,
                UserImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + obj.User.UserProfile.MainImage,
                Notes = obj.Notes,
                LastComment = obj.Comments.Where(x => !x.IsDeleted).OrderByDescending(c=>c.ID).Select(c=>new CommentViewModel {
                    ID = c.ID,
                    Text = c.Text,
                    UserName = c.User.UserName,
                    UserID = c.UserID,
                    DisplayName = c.User.DisplayName,
                    UserProfileImagePath = c.User.UserProfile != null ? protocol + "://" + hostName + "/uploads/User-Document/" + c.User.UserProfile.MainImage : "",
                    
                }).FirstOrDefault(),
                Comments = obj.Comments.Where(x => !x.IsDeleted).OrderByDescending(c => c.ID).Select(c => new CommentViewModel
                {
                    ID = c.ID,
                    Text = c.Text,
                    UserName = c.User.UserName,
                    UserID = c.UserID,
                    DisplayName = c.User.DisplayName,
                    UserProfileImagePath = c.User.UserProfile != null ? protocol + "://" + hostName + "/uploads/User-Document/" + c.User.UserProfile.MainImage : "",

                }).ToList(),
                NumberOfLike = obj.PostUserActions
                                     .Where(x => x.ActionID == (int)Actions.Like)
                                     .Where(x => !x.IsDeleted)
                                     .Where(x => x.IsActive)
                                     .Count(),
                NumberOfComments = obj.Comments
                                     .Where(x => x.PostID == obj.ID)
                                     .Where(x => !x.IsDeleted)
                                     .Count(),
                NumberOfShare = obj.PostUserActions
                                     .Where(x => x.ActionID == (int)Actions.Share)
                                     .Where(x => !x.IsDeleted)
                                     .Count(),

                PostStatus = (PostStatus)obj.PostStatus,
                PostType = (PostType)obj.PostType,
                Documents = obj.PostDocuments.Where(doc => !doc.IsDeleted).Select(doc => new DocumentViewModel
                {
                    FilePath = protocol + "://" + hostName + "/uploads/Post-Document/" + doc.FileName,
                    UploadedDate = doc.CreatedDate,
                    FileType = doc.Type
                })
                
            }).OrderByPropertyName(orderBy, isAscending);

            result = posts.Skip(excludedRows).Take(pageSize).ToList();
            result.ForEach(x =>
            {
                if (x.ParentPostID.HasValue)
                {
                    var parentPost = _repo.GetAll().Where(p => p.ID == x.ParentPostID).Select(p => new
                    {
                        UserID = p.UserID,
                        UserName = p.User.UserName,
                        Text = p.Text,
                        DisplayName = p.User.DisplayName,
                        UserImage = p.User.UserProfile.MainImage,
                        Documents = p.PostDocuments
                    }).FirstOrDefault();
                    if (x.UserID == parentPost.UserID)
                    {
                        x.IsSelfShare = true;
                    }
                    x.SharedText = x.Text;
                    x.Text = parentPost.Text;

                    x.SharedUserID = x.UserID;
                    x.SharedDisplayName = x.DisplayName;
                    x.SharedUserName = x.UserName;
                    x.SharedUserImage = x.UserImagePath;

                    x.UserID = parentPost.UserID;
                    x.UserName = parentPost.UserName;
                    x.DisplayName = parentPost.DisplayName;
                    x.UserImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + parentPost.UserImage;
                    x.IsShared = true;
                    //x.ParentPostID = null;
                    x.Documents = parentPost.Documents.Select(p => new DocumentViewModel
                    {
                        FilePath = protocol + "://" + hostName + "/uploads/Post-Document/" + p.FileName,
                    }).ToList();


                }
            });

            return new PagingViewModel<WholePostViewModel>() { PageIndex = pageIndex, PageSize = pageSize, Result = result, Records = records, Pages = pages };
        }

        //public List<CommentViewModel> GetComments(int postId)
        //{
        //    string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
        //    string hostName = HttpContext.Current.Request.Url.Authority.ToString();

        //    var post =  _repo.GetAll().FirstOrDefault(x => x.ID == postId);
        //    List<CommentViewModel> list =
        //             new List<CommentViewModel>();
        //    foreach (var item in post.Comments)
        //    {
        //        list.Add(new CommentViewModel
        //        {
        //            ID = item.ID,
        //            Text = item.Text,
        //            CreatedDate = item.CreatedDate,
        //            Documents = item.CommentDocuments.Select(d => new DocumentViewModel
        //            {
        //                FilePath = protocol + "://" + hostName + "/uploads/Post-Document/" + d.FileName,
        //                UploadedDate = d.CreatedDate

        //            }),
        //            Replies = _commentRepository.GetAll()
        //                        .Where(x=>x.ParentCommentID == item.ID)
        //                        .Select( x => new CommentViewModel
        //                        {
        //                            ID = x.ID,
        //                            Text = x.Text,
        //                            CreatedDate = x.CreatedDate,
        //                            Documents = x.CommentDocuments.Select(d => new DocumentViewModel
        //                            {
        //                                FilePath = protocol + "://" + hostName + "/uploads/Post-Document/" + d.FileName,
        //                                UploadedDate = d.CreatedDate

        //                            }),
        //                        })
        //        });
        //    }
        //    return list;
        //}
        public CreatePostViewModel CreatePost (CreatePostViewModel view)
        {
            
            var insertedPost = _repo.Add(view.ToPostModel());
            
            if (view.Files != null && view.Files.Count>0)
            {
                foreach (var file in view.Files)
                {
                    _postDocumentrepo.Add(new PostDocument {

                        FileName = file.FileName,
                        Type=view.PostType == PostType.Image? "Image":"Video",
                        PostID=insertedPost.ID

                    }); 
                }
            }
            List<int> userIDs = new List<int>();
            var friends = _friendRequestRepository.GetAll()
                            .Where(x => x.FromUserID == view.UserID || x.ToUserID == view.UserID)
                            .Where(x => x.Status == (int)FriendRequestStatus.Approved)
                            .Select(x => new { Usr1 = x.FromUserID, Usr2 = x.ToUserID})
                            .ToList();
            friends.ForEach(x =>
            {
                if (x.Usr1 == view.UserID)
                    userIDs.Add(x.Usr2);
                else
                    userIDs.Add(x.Usr1);
            });
            _unit.Save();

            var user = _repo.GetAll().Where(x => x.ID == insertedPost.ID).Select(p => p.User).FirstOrDefault();
            var notifcation =  new NotificationCreateViewModel
            {
                ContentArabic = $"{NotificationArabicKeys.NewPost} {user.UserName}",
                ContentEnglish = $"{NotificationEnglishKeys.NewPost} {user.UserName}",
                NotificationTypeID = 2,
                SourceID = view.UserID,
                PostID= insertedPost.ID,
               
            };

            _notificationService.SendNotifictionForFriends(notifcation, userIDs);




        
            return new CreatePostViewModel {
                ID = insertedPost.ID
            };

        }

        public CreatePostViewModel SharePost(SharePostViewModel viewModel)
        {
            var sharedPost = _repo.GetById(viewModel.PostID);
            var newPost = new Post
            {
                Text = viewModel.Text,
                PostStatus = sharedPost.PostStatus,
                PostType = sharedPost.PostType,
                UserID = viewModel.UserID,
                ParentPostID = viewModel.PostID
            };
            
            var insertedPost = _repo.Add(newPost);

            //List<int> userIDs = new List<int>();
            //var friends = _friendRequestRepository.GetAll()
            //                .Where(x => x.FromUserID == viewModel.UserID || x.ToUserID == viewModel.UserID)
            //                .Where(x => x.Status == (int)FriendRequestStatus.Approved)
            //                .Select(x => new { Usr1 = x.FromUserID, Usr2 = x.ToUserID })
            //                .ToList();
            //friends.ForEach(x =>
            //{
            //    if (x.Usr1 == viewModel.UserID)
            //        userIDs.Add(x.Usr2);
            //    else
            //        userIDs.Add(x.Usr1);
            //});
            _unit.Save();

            //var user = _repo.GetAll().Where(x => x.ID == insertedPost.ID).Select(p => p.User).FirstOrDefault();
            //var notifcation = new NotificationCreateViewModel
            //{
            //    ContentArabic = $"{NotificationArabicKeys.NewPost} {user.UserName}",
            //    ContentEnglish = $"{NotificationEnglishKeys.NewPost} {user.UserName}",
            //    NotificationTypeID = 2,
            //    SourceID = viewModel.UserID,
            //    PostID = insertedPost.ID,

            //};

            //_notificationService.SendNotifictionForFriends(notifcation, userIDs);
            return new CreatePostViewModel
            {
                ID = insertedPost.ID
            };

        }

        public PostDetailsViewModel GetByID(int loggedUserID , int postId)
        {

            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var PostData = _repo.GetAll().Where(post => post.ID == postId).Select(obj => new PostDetailsViewModel
            {
                ID = obj.ID,
                Text = obj.Text,
                UserID = obj.UserID,
                IsLiked = obj.PostUserActions.Where(x => x.UserID == loggedUserID
                                                    && x.ActionID == (int)Actions.Like
                                                    && x.IsActive && !x.IsDeleted).Any(),
                CreatedDate = obj.CreatedDate,
                ParentPostID = obj.ParentPostID,
                UserName = obj.User.UserName,
                DisplayName = obj.User.DisplayName,
                UserImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + obj.User.UserProfile.MainImage,
                Notes = obj.Notes,
                LastComment = obj.Comments.OrderByDescending(c => c.ID).Select(c => new CommentViewModel
                {
                    ID = c.ID,
                    Text = c.Text,
                    UserName = c.User.UserName,
                    DisplayName = c.User.DisplayName,
                    UserProfileImagePath = c.User.UserProfile != null ? protocol + "://" + hostName + "/uploads/User-Document/" + c.User.UserProfile.MainImage : "",

                }).FirstOrDefault(),
                Comments = obj.Comments.OrderByDescending(c => c.ID).Select(c => new CommentViewModel
                {
                    ID = c.ID,
                    Text = c.Text,
                    UserName = c.User.UserName,
                    DisplayName = c.User.DisplayName,
                    UserProfileImagePath = c.User.UserProfile != null ? protocol + "://" + hostName + "/uploads/User-Document/" + c.User.UserProfile.MainImage : "",

                }).ToList(),
                NumberOfLike = obj.PostUserActions
                                     .Where(x => x.ActionID == (int)Actions.Like)
                                     .Where(x => !x.IsDeleted)
                                     .Where(x => x.IsActive)
                                     .Count(),
                NumberOfComments = obj.Comments
                                     .Where(x => x.PostID == obj.ID)
                                     .Where(x => !x.IsDeleted)
                                     .Count(),
                NumberOfShare = obj.PostUserActions
                                     .Where(x => x.ActionID == (int)Actions.Share)
                                     .Where(x => !x.IsDeleted)
                                     .Count(),

                PostStatus = (PostStatus)obj.PostStatus,
                PostType = (PostType)obj.PostType,
                Documents = obj.PostDocuments.Where(doc => !doc.IsDeleted).Select(doc => new DocumentViewModel
                {
                    FilePath = protocol + "://" + hostName + "/uploads/Post-Document/" + doc.FileName,
                    UploadedDate = doc.CreatedDate,
                    FileType = doc.Type
                })

            }).FirstOrDefault();

            if (PostData.ParentPostID.HasValue)
            {
                var parentPost = _repo.GetAll().Where(p => p.ID == PostData.ParentPostID).Select(p => new
                {
                    UserID = p.UserID,
                    Text = p.Text,
                    UserName = p.User.UserName,
                    DisplayName = p.User.DisplayName,
                    UserImage = p.User.UserProfile.MainImage,
                    Documents = p.PostDocuments
                }).FirstOrDefault();
                if (PostData.UserID == parentPost.UserID)
                {
                    PostData.IsSelfShare = true;
                }
                PostData.SharedText = PostData.Text;
                PostData.Text = parentPost.Text;
                PostData.UserID = parentPost.UserID;
                PostData.UserName = parentPost.UserName;
                PostData.DisplayName = parentPost.DisplayName;
                PostData.UserImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + parentPost.UserImage;
                PostData.IsShared = true;
                PostData.ParentPostID = null;
                PostData.Documents = parentPost.Documents.Select(p => new DocumentViewModel
                {
                    FilePath = protocol + "://" + hostName + "/uploads/Post-Document/" + p.FileName,
                }).ToList();

            }
                return PostData;

        }


        public bool IsPostExists(int postId)
        {
            return _repo.GetAll().Where(post => post.ID == postId).Any();
        }


        public CreatePostViewModel UpdatePost (CreatePostViewModel viewModel)
        {
            var edited =_repo.Edit(viewModel.ToPostModel());

                if (viewModel.Files!= null && viewModel.Files.Count>0)
                {
                    _postDocumentrepo.RemoveMany(doc => doc.PostID == viewModel.ID);
                    foreach (var f in viewModel.Files)
                    {
                        _postDocumentrepo.Add(new PostDocument
                        {
                            FileName = f.FileName,
                             Type = viewModel.PostType == PostType.Image ? "Image" : "Video",

                        });
                    }
                }
            _unit.Save();
            return new CreatePostViewModel
            {
                ID = edited.ID
            };
            
        }

        public void DeletePost (int postId, int userId)
        {
            var postData = _repo.GetAll().Where(post => post.ID == postId && post.UserID == userId).FirstOrDefault();
            if (postData != null)
            {
                var PostDocuments = _postDocumentrepo.GetAll().Where(doc => doc.PostID == postId);
                if (PostDocuments.Count()>0)
                {
                    _postDocumentrepo.RemoveMany(doc => doc.PostID == postId);
                }

                var PostComments=_commentRepository.GetAll().Where(doc => doc.PostID == postId);
                if(PostComments.Count()>0)
                {
                    _commentRepository.RemoveMany(doc => doc.PostID == postId);
                }

                _repo.RemoveByIncluded(postData);
            }
        }
    }

    public class WholePostViewModel
    {
        public int ID { get; set; }
        public string Text { get; set; }
        public int UserID { get; set; }
        public bool IsLiked { get; set; }
        public DateTime CreatedDate { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string UserImagePath { get; set; }
        public string Notes { get; set; }
        public CommentViewModel LastComment { get; set; }
        public List<CommentViewModel> Comments { get; set; }
        public int NumberOfLike { get; set; }
        public int NumberOfComments { get; set; }
        public int NumberOfShare { get; set; }
        public PostStatus PostStatus { get; set; }
        public PostType PostType { get; set; }
        public IEnumerable<DocumentViewModel> Documents { get; set; }
        public int SharedUserID { get; internal set; }
        public string SharedDisplayName { get; internal set; }
        public string SharedUserName { get; internal set; }
        public string SharedUserImage { get; internal set; }
        public int? ParentPostID { get; internal set; }
        public bool IsShared { get; internal set; }
        public bool IsSelfShare { get; internal set; }
        public string SharedText { get; internal set; }
    }
}
