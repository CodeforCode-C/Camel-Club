using CamelsClub.ViewModels;
using CamelsClub.ViewModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamelsClub.Services
{
    public interface IPostService
    {
        //test
        PagingViewModel<PostDetailsViewModel> GetHomePosts(int userID, string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic);
        PagingViewModel<WholePostViewModel> Search(int userID=0,string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic);
        CreatePostViewModel CreatePost(CreatePostViewModel view);
        PostDetailsViewModel GetByID(int loggedUserID,int postId);
        CreatePostViewModel SharePost(SharePostViewModel viewModel);

        bool IsPostExists(int postId);
        CreatePostViewModel UpdatePost(CreatePostViewModel viewModel);
        void DeletePost(int postId, int userId);
    }
}
