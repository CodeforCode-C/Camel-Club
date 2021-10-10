using CamelsClub.ViewModels;
using CamelsClub.ViewModels.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamelsClub.Services
{
    public interface IPostUserActionService
    {
        PagingViewModel<PostUserActionViewModel> Search(int postID = 0 , string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic);
        bool Add(PostUserActionCreateViewModel view);
        void Edit(PostUserActionCreateViewModel viewModel);

        PostUserActionViewModel GetByID(int id);
        bool IsExists(int id);
        PagingViewModel<LikeViewModel> GetLikes(int postID, int pageIndex, int pageSize);
    }
}
