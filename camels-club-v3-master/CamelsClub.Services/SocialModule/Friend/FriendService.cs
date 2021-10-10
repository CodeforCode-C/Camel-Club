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
    public class FriendService : IFriendService
    {
        private readonly IUnitOfWork _unit;
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly IFriendRepository _friendRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBlockedFriendRepository _blockedFriendRepository;

        public FriendService(IUnitOfWork unit,
                                    IFriendRequestRepository friendRequestRepository ,
                                    IUserRepository userRepository,
                                    IBlockedFriendRepository blockedFriendRepository ,
                                    IFriendRepository friendRepository )
        {
            _unit = unit;
            _userRepository = userRepository;
            _friendRequestRepository = friendRequestRepository;
            _friendRepository = friendRepository;
            _blockedFriendRepository = blockedFriendRepository;
        }

        public PagingViewModel<ClearedFriendViewModel> GetBlockedFriends(int userID = 0, string search = "", string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var query = _friendRequestRepository.GetAll()
                            .Where(x => x.Status == (int)FriendRequestStatus.Blocked)
                            .Where(x => x.ActionBy == userID)
                             .Where(x => search == "" || x.FromUser.DisplayName.Contains(search) || x.ToUser.DisplayName.Contains(search));

            int records = query.Count();
            if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
            int pages = (int)Math.Ceiling((double)records / pageSize);
            int excludedRows = (pageIndex - 1) * pageSize;

            List<FriendViewModel> result = new List<FriendViewModel>();

            var requests = query.Select(obj => new FriendViewModel
            {
                ID = obj.ID,
                UserName = obj.FromUser.UserName,
                DisplayName = obj.FromUser.DisplayName,
                UserID = obj.FromUserID,
                UserMainImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + obj.FromUser.UserProfile.MainImage,
                FriendMainImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + obj.ToUser.UserProfile.MainImage,
                FriendUserName = obj.ToUser.UserName,
                FriendDisplayName = obj.ToUser.DisplayName,
                FriendUserID = obj.ToUserID

            }).OrderByDescending(x => x.ID);

            result = requests.Skip(excludedRows).Take(pageSize).ToList();
            //return final list
            List<ClearedFriendViewModel> list = new List<ClearedFriendViewModel>();
            foreach (var item in result)
            {
                if (userID == item.UserID)
                {
                    list.Add(new ClearedFriendViewModel
                    {
                        ID = item.ID,
                        UserID = item.FriendUserID,
                        UserName = item.FriendUserName,
                        DisplayName = item.FriendDisplayName,
                        MainImagePath = item.FriendMainImagePath
                    });
                }
                else if (userID == item.FriendUserID)
                {
                    list.Add(new ClearedFriendViewModel
                    {
                        ID = item.ID,
                        UserID = item.UserID,
                        UserName = item.UserName,
                        DisplayName = item.DisplayName,
                        MainImagePath = item.UserMainImagePath
                    });
                }
            }
            return new PagingViewModel<ClearedFriendViewModel>() { PageIndex = pageIndex, PageSize = pageSize, Result = list, Records = records, Pages = pages };
        }


        // get my friends
        public PagingViewModel<ClearedFriendViewModel> Search(int userID=0 , string search="", string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var query = _friendRequestRepository.GetAll()
                            .Where(x => x.FromUserID == userID || x.ToUserID == userID)
                            .Where(x => search == "" || x.ToUser.Phone== search || x.FromUser.Phone == search || x.FromUser.DisplayName.Contains(search) || x.ToUser.DisplayName.Contains(search) || x.FromUser.UserName.Contains(search) || x.ToUser.UserName.Contains(search))
                            .Where(x => x.Status == (int)FriendRequestStatus.Approved);
                            //.Where(x=> x.Status == (int)FriendRequestStatus.UnSeen && x.ActionBy != userID)


            int records = query.Count();
            if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
            int pages = (int)Math.Ceiling((double)records / pageSize);
            int excludedRows = (pageIndex - 1) * pageSize;

            List<FriendViewModel> result = new List<FriendViewModel>();

            var requests = query.Select(obj => new FriendViewModel
            {
                ID = obj.ID,
                UserName = obj.FromUser.UserName,
                DisplayName = obj.FromUser.DisplayName,
                UserID = obj.FromUserID,
                UserMainImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + obj.FromUser.UserProfile.MainImage,
                FriendMainImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + obj.ToUser.UserProfile.MainImage,
                FriendUserName = obj.ToUser.UserName,
                FriendDisplayName = obj.ToUser.DisplayName,
                FriendUserID = obj.ToUserID 
             
            }).OrderByDescending(x=>x.ID);

            result = requests.Skip(excludedRows).Take(pageSize).ToList();
            //return final list
            List<ClearedFriendViewModel> list = new List<ClearedFriendViewModel>();
            foreach (var item in result)
            {
                if(userID == item.UserID)
                {
                    list.Add(new ClearedFriendViewModel
                    {
                        ID = item.ID,
                        UserID = item.FriendUserID,
                        UserName = item.FriendUserName,
                        DisplayName = item.FriendDisplayName,
                        MainImagePath = item.FriendMainImagePath
                    });
                }else if(userID == item.FriendUserID)
                {
                    list.Add(new ClearedFriendViewModel
                    {
                        ID = item.ID,
                        UserID = item.UserID,
                        UserName = item.UserName,
                        DisplayName = item.DisplayName,
                        MainImagePath = item.UserMainImagePath
                    });
                }
            }
            return new PagingViewModel<ClearedFriendViewModel>() { PageIndex = pageIndex, PageSize = pageSize, Result = list, Records = records, Pages = pages };
        }
        
        public FriendViewModel GetByID(int id)
        {

            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var friend = _friendRepository.GetAll().Where(x => x.ID == id)
                .Select(obj => new FriendViewModel
            {
                    ID = obj.ID,
                    FriendMainImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + obj.FriendUser.UserProfile.MainImage,
                    FriendUserName = obj.FriendUser.UserName,
                    FriendUserID = obj.FriendUserID
                }).FirstOrDefault();
            
            return friend;

        }
        
        public void Delete(int id)
        {
             _friendRepository.Remove(id);
        }

        public bool UnFollow(BlockedFriendCreateViewModel viewModel)
        {
            var loggedUserID = viewModel.UserID;
            if(viewModel.UserID > viewModel.BlockedFriendID)
            {
                var temp = viewModel.UserID;
                viewModel.UserID = viewModel.BlockedFriendID;
                viewModel.BlockedFriendID = temp;
            }
            var friendRequest = 
            _friendRequestRepository.GetAll()
                        .Where(x => x.FromUserID == viewModel.UserID && x.ToUserID == viewModel.BlockedFriendID) 
                     //   .Where(x => x.Status == (int)FriendRequestStatus.Approved)
                        .FirstOrDefault();
            
            if(friendRequest != null)
            {
                friendRequest.Status = (int)FriendRequestStatus.Blocked;
                friendRequest.ActionBy = loggedUserID;
            }
            else
            {
                _friendRequestRepository.Add(new FriendRequest
                {
                    FromUserID = viewModel.UserID,
                    ToUserID = viewModel.BlockedFriendID,
                    ActionBy = loggedUserID,
                    Status = (int)FriendRequestStatus.Blocked
                    
                });
            }
            _unit.Save();

            return true;
        }
        // it init the friend request to no friend request which is same as no friend request physically
        public bool ReFollow(BlockedFriendCreateViewModel viewModel)
        {
            var loggedUserID = viewModel.UserID;
            if (viewModel.UserID > viewModel.BlockedFriendID)
            {
                var temp = viewModel.UserID;
                viewModel.UserID = viewModel.BlockedFriendID;
                viewModel.BlockedFriendID = temp;
            }

            var friendRequest =
            _friendRequestRepository.GetAll()
                        .Where(x => x.FromUserID == viewModel.UserID && x.ToUserID == viewModel.BlockedFriendID)
                        .Where(x => x.Status == (int)FriendRequestStatus.Blocked)
                        .FirstOrDefault();

            if (friendRequest != null)
            {
                friendRequest.Status = (int)FriendRequestStatus.NoFriendRequest;
                friendRequest.ActionBy = loggedUserID;
                _unit.Save();
            }

            return true;
        }

    }


}

