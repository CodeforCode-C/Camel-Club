using CamelsClub.Data.Extentions;
using CamelsClub.Data.Helpers;
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
    public class UserService: IUserService
    {
        private readonly IUnitOfWork _unit;
        private readonly IUserRepository _repo;
        private readonly IUserRoleService _userRoleService;
        private readonly IUserConfirmationMessageRepository _userConfirmationMessagerepo;
        private readonly ITokenRepository _tokenRepository;
        private readonly ICompetitionCheckerRepository _competitionCheckerRepository;
        private readonly IFriendRequestRepository _friendRequestRepository;
        private readonly ICompetitionRepository _competitionRepository;
        private readonly ICompetitionRefereeRepository _competitionRefereeRepository;
        private readonly ICompetitionInviteRepository _competitionInviteRepository;
        public UserService(IUnitOfWork unit , 
            IUserRepository repo,
            IUserRoleService userRoleService,
            ITokenRepository tokenRepository,
            ICompetitionRepository competitionRepository,
            IFriendRequestRepository friendRequestRepository,
            ICompetitionCheckerRepository competitionCheckerRepository,
            ICompetitionRefereeRepository competitionRefereeRepository,
            IUserConfirmationMessageRepository userConfirmationMessagerepo,
            ICompetitionInviteRepository competitionInviteRepository
            )
        {
            _unit = unit;
            _userRoleService = userRoleService;
            _repo = repo;
            _tokenRepository = tokenRepository;
            _friendRequestRepository = friendRequestRepository;
            _competitionRepository = competitionRepository;
            _competitionCheckerRepository = competitionCheckerRepository;
            _competitionRefereeRepository = competitionRefereeRepository;
            _userConfirmationMessagerepo = userConfirmationMessagerepo;
            _competitionInviteRepository = competitionInviteRepository;
        }

        public bool SignOut(int userID, string accessToken)
        {
            var encryptedToken = SecurityHelper.Encrypt(accessToken);
            var item = _tokenRepository.GetAll()
                .FirstOrDefault(i => !i.IsDeleted && i.UserID == userID && i.TokenGUID == encryptedToken);
            if (item != null)
            {
                item.Active = false;
                item.LoggedOutDate = DateTime.Now;
                _unit.Save();
                return true;
            }
            return false;
        }
        public ConfirmationMessageViewModel Register(CreateUserViewModel viewModel)
        {
            var notEncryptedPhone = viewModel.MobileNumber;
            if (!string.IsNullOrWhiteSpace(viewModel.Email))
            {
                viewModel.Email =
                SecurityHelper.Encrypt(viewModel.Email.ToLower().Trim());

                if (_repo.IsEmailAlreadyExists(viewModel.Email))
                     throw new Exception(Resource.EmailAreadyExist);

                
            }
            //viewModel.MobileNumber =
            // SecurityHelper.Encrypt(viewModel.MobileNumber.ToLower().Trim());
           
            if (_repo.IsPhoneAlreadyExists(viewModel.MobileNumber))
                throw new Exception(Resource.PhoneNumberAreadyExist);

            viewModel.UserName = viewModel.UserName.ToLower();
            if (_repo.IsUserNameAlreadyExists(viewModel.UserName))
                throw new Exception(Resource.UserNameAlreadyExist);

            viewModel.NID =
                   SecurityHelper.Encrypt(viewModel.NID.ToLower().Trim());

                var insertedUser = _repo.Add(viewModel.ToUserModel());

                _userRoleService.InsertUserRole(insertedUser.ID, Roles.User);

                var verificationCode = SecurityHelper.GetRandomNumber().ToString();

            while (_userConfirmationMessagerepo.GetAll()
                .Any(x => x.Code == verificationCode && !x.IsDeleted && x.UserID == insertedUser.ID))
            {
                verificationCode = SecurityHelper.GetRandomNumber().ToString();
            }
            _userConfirmationMessagerepo
                .Add(new UserConfirmationMessage
                    {
                        UserID = insertedUser.ID,
                        Code = verificationCode.ToString(),
                        IsDeleted = false,
                        CreatedBy = insertedUser.ID.ToString(),
                        CreatedDate = DateTime.UtcNow,
                        
                    });
                _unit.Save();
            var userViewModel = new ConfirmationMessageViewModel { UserID = insertedUser.ID, Code = verificationCode , UserName = viewModel.UserName, Phone =  notEncryptedPhone};
                return userViewModel;

            }

        //public bool SetUserName(List<ChangeUserNameViewModel> viewModels)
        //{
        //    var decrypted = SecurityHelper.Decrypt("B0EaWq/xzU9wUnoc0mZwpA==");
        //    foreach (var item in viewModels)
        //    {
        //        var encryptedPhone = SecurityHelper.Encrypt(item.Phone);
        //        var user = _repo.GetAll().Where(x => x.Phone == encryptedPhone).FirstOrDefault();
        //        if(user != null)
        //        {
        //            user.UserName = item.UserName;
        //            user.DisplayName = item.DisplayName;

        //        }
        //        else
        //        {
        //            _repo.Add(new User
        //            {
        //                Phone = encryptedPhone,
        //                UserName = item.UserName,
        //                DisplayName = item.DisplayName,
        //                NID = SecurityHelper.Encrypt("123456789")
                       
        //            });
        //        }

        //    }
        //    _unit.Save();

        //    return true;

        //}
        //public class ChangeUserNameViewModel
        //{
        //    public string UserName { get; set; }
        //    public string DisplayName { get; set; }
        //    public string Phone { get; set; }
        //}
        public bool IsUserNameExists(string userName)
        {
            return _repo.IsUserNameAlreadyExists(userName);
        }
        public ConfirmationMessageViewModel Login(string phone)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            if (string.IsNullOrWhiteSpace(phone))
                throw new Exception(Resource.PhoneRequired);

          //  phone = SecurityHelper.Encrypt(phone.ToLower().Trim());

            if (!_repo.IsPhoneAlreadyExists(phone))
                throw new Exception(Resource.UserNotFound);

            var user = _repo.GetUserByPhone(phone);

            if (user == null)
                throw new Exception(Resource.UserNotFound);

            var userProfile = _repo.GetAll().Where(x => x.ID == user.ID).Select(x => x.UserProfile).FirstOrDefault();
            var profileImg = userProfile != null ? protocol + "://" + hostName + "/uploads/User-Document/" + userProfile.MainImage : "";
            var VerificationCode = SecurityHelper.GetRandomNumber();

            _userConfirmationMessagerepo
                    .Add( new UserConfirmationMessage
                          {
                             UserID = user.ID,
                             Code = VerificationCode.ToString() ,
                             CreatedBy = user.ID.ToString() ,
                             CreatedDate = DateTime.UtcNow 
                    });
                
                return new ConfirmationMessageViewModel { UserID = user.ID, Code = VerificationCode.ToString(),DisplayName = user.DisplayName, UserName = user.UserName, ProfileImagePath = profileImg};
            
        }

        public ConfirmationMessageViewModel AdminLogin(AdminLoginCreateViewModel viewModel)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();
            viewModel.Email = SecurityHelper.Encrypt(viewModel.Email.ToLower());
            viewModel.Password = SecurityHelper.GetHashedString(viewModel.Password);
            var user = _repo.GetAll().Where(x => x.Email == viewModel.Email && x.Password == viewModel.Password)
                .Select(x =>
             new ConfirmationMessageViewModel
             {
                 UserID = x.ID,
                 UserName = x.UserName,
                 ProfileImagePath = x.UserProfile.MainImage != null? protocol + "://" + hostName + "/uploads/User-Document/" + x.UserProfile.MainImage:"",

             }).FirstOrDefault();
            
            if(user == null)
            {
                throw new Exception("Invalid email or password");
            }
            return user;
        }

        public PagingViewModel<UserViewModel> Search(string text, string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var query = _repo.GetAll()
                .Where(x => !x.IsDeleted)
                .Where(x => x.DisplayName.Contains(text) || x.Phone.Contains(text))
                .Where(x => x.UserName != "admin");                                                     



            int records = query.Count();
            if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
            int pages = (int)Math.Ceiling((double)records / pageSize);
            int excludedRows = (pageIndex - 1) * pageSize;
            
            List<UserViewModel> result = new List<UserViewModel>();

            var users = query.OrderByPropertyName(orderBy, isAscending)
                      .Skip(excludedRows).Take(pageSize).ToList();
            foreach (var item in users)
            {
                result.Add(new UserViewModel
                {
                    ID = item.ID,
                    UserMainImagePath = item.UserProfile?.MainImage != null && item.UserProfile?.MainImage != "" ? protocol + "://" + hostName + "/uploads/User-Document/" + item?.UserProfile?.MainImage : "",
                    UserName = item.UserName,
                    DisplayName = item.DisplayName,
                    Email = string.IsNullOrWhiteSpace(item.Email)?item.Email : SecurityHelper.Decrypt(item.Email),
                    Phone = item.Phone//SecurityHelper.Decrypt(item.Phone)
                });

            }
            foreach (var item in result)
            {
                var nameParts = item.DisplayName.Split(' ');
                for (int i = 0; i < nameParts.Length; i++)
                {
                    if (nameParts[nameParts.Length-1-i].Contains(text))
                    {
                        item.SyllableContainsText = nameParts.Length - 1 - i;
                    }
                }
                
            }
            result = result.OrderBy(x => x.SyllableContainsText).ToList();
            return new PagingViewModel<UserViewModel>() { PageIndex = pageIndex, PageSize = pageSize, Result = result, Records = records, Pages = pages };

        }

        public PagingViewModel<AdminViewModel> GetAllAdminUsers(string text, string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var query = _repo.GetAll()
                .Where(x => !x.IsDeleted)
                .Where(x => x.DisplayName.Contains(text))
                .Where(x => x.UserRoles.Any(r=>r.RoleID == (int)Roles.Admin));



            int records = query.Count();
            if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
            int pages = (int)Math.Ceiling((double)records / pageSize);
            int excludedRows = (pageIndex - 1) * pageSize;

            List<AdminViewModel> result = new List<AdminViewModel>();

            var users = query.OrderByPropertyName(orderBy, isAscending)
                      .Skip(excludedRows).Take(pageSize).ToList();
            foreach (var item in users)
            {
                result.Add(new AdminViewModel
                {
                    ID = item.ID,
                    UserMainImagePath = item.UserProfile?.MainImage != null && item.UserProfile?.MainImage != "" ? protocol + "://" + hostName + "/uploads/User-Document/" + item?.UserProfile?.MainImage : "",
                    UserName = item.UserName,
                    DisplayName = item.DisplayName,
                    Email = string.IsNullOrWhiteSpace(item.Email) ? item.Email : SecurityHelper.Decrypt(item.Email),
                    Phone = item.Phone,//SecurityHelper.Decrypt(item.Phone),
                    Role = Roles.Admin.ToString()
                });

            }
            foreach (var item in result)
            {
                var nameParts = item.DisplayName.Split(' ');
                for (int i = 0; i < nameParts.Length; i++)
                {
                    if (nameParts[nameParts.Length - 1 - i].Contains(text))
                    {
                        item.SyllableContainsText = nameParts.Length - 1 - i;
                    }
                }

            }
            result = result.OrderBy(x => x.SyllableContainsText).ToList();
            return new PagingViewModel<AdminViewModel>() { PageIndex = pageIndex, PageSize = pageSize, Result = result, Records = records, Pages = pages };

        }
        public PagingViewModel<UserSearchViewModel> FindAvailableUsersToBeCheckers(int competitionID,int userID, string text, string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic)
        {
            //get checkers UserIDs
            var checkerUserIDs = _competitionRepository.GetAll().Where(x=> x.ID == competitionID)
                                    .SelectMany(x=> x.CompetitionCheckers)
                                    .Select(x => x.UserID).ToList();

            var refereeUserIDs = _competitionRepository.GetAll().Where(x => x.ID == competitionID)
                                    .SelectMany(x => x.CompetitionReferees)
                                   .Select(x => x.UserID).ToList();

            var competitors = _competitionInviteRepository.GetAll()
                                  .Where(x => x.CompetitionID == competitionID)
                                   .Select(x => x.UserID).ToList();

            checkerUserIDs.AddRange(refereeUserIDs);
            checkerUserIDs.AddRange(competitors);
            return FindUsers(userID, text, orderBy, isAscending, pageIndex, pageSize, Languages.Arabic, ExcluededUserIDs: checkerUserIDs);

        }

        public PagingViewModel<UserSearchViewModel> FindAvailableUsersToBeReferees(int competitionID, int userID, string text, string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic)
        {
            //get checkers UserIDs
            //get checkers UserIDs
            var checkerUserIDs = _competitionRepository.GetAll().Where(x => x.ID == competitionID)
                                    .SelectMany(x => x.CompetitionCheckers)
                                    .Select(x => x.UserID).ToList();

            var refereeUserIDs = _competitionRepository.GetAll().Where(x => x.ID == competitionID)
                                    .SelectMany(x => x.CompetitionReferees)
                                   .Select(x => x.UserID).ToList();

            var competitors = _competitionInviteRepository.GetAll()
                                  .Where(x => x.CompetitionID == competitionID)
                                   .Select(x => x.UserID).ToList();

            refereeUserIDs.AddRange(checkerUserIDs);
            refereeUserIDs.AddRange(competitors);

            return FindUsers(userID, text, orderBy, isAscending, pageIndex, pageSize, Languages.Arabic, ExcluededUserIDs: refereeUserIDs);

        }

        // get my friendRequests
        public PagingViewModel<UserSearchViewModel> FindUsers(int userID ,string text, string orderBy = "ID", bool isAscending = false, int pageIndex = 1, int pageSize = 20, Languages language = Languages.Arabic, List<int> ExcluededUserIDs = null)
        {
            string protocol = HttpContext.Current.Request.Url.Scheme.ToString();
            string hostName = HttpContext.Current.Request.Url.Authority.ToString();

            var lowerText = !string.IsNullOrWhiteSpace(text) ? text.ToLower() : null ;
            var query = _repo.GetAll()
                .Where(x => !x.IsDeleted)
                .Where(x => x.ID != userID);
              
            if(ExcluededUserIDs != null)
            {
               query =  query.Where(x => !ExcluededUserIDs.Contains(x.ID));
            }
            if(lowerText != null)
            {
                query = query.Where(x =>
                                    x.DisplayName.Contains(text) ||
                                    x.UserName.Contains(text) ||
                                    x.DisplayName.Contains(lowerText) ||
                                    x.UserName.Contains(lowerText) ||
                                    x.Phone == lowerText);
                            

            }
            query = query.Where(x => !x.FromFriendRequests.Any(fr => fr.ToUserID == userID && fr.Status == (int)FriendRequestStatus.Blocked))
                    .Where(x => !x.ToFriendRequests.Any(fr => fr.FromUserID == userID && fr.Status == (int)FriendRequestStatus.Blocked));


            int records = query.Count();
            if (records <= pageSize || pageIndex <= 0) pageIndex = 1;
            int pages = (int)Math.Ceiling((double)records / pageSize);
            int excludedRows = (pageIndex - 1) * pageSize;


         
            List<UserSearchViewModel> result = new List<UserSearchViewModel>();

            var users =
            query.OrderByPropertyName(orderBy, isAscending)
                      .Skip(excludedRows).Take(pageSize).
                      Select(x => new UserSearchViewModel {
                          ID = x.ID,
                          UserMainImagePath = protocol + "://" + hostName + "/uploads/User-Document/" + x.UserProfile.MainImage,
                          UserName = x.UserName,
                          DisplayName = x.DisplayName,
                          HasRelation = x.FromFriendRequests.Where(r => r.ToUserID == userID && r.Status != (int)FriendRequestStatus.NoFriendRequest).Any() ||
                                         x.ToFriendRequests.Where(r => r.FromUserID == userID && r.Status != (int)FriendRequestStatus.NoFriendRequest).Any(),
                          IsLoggedIDComesFirst = x.ID > userID,

                          //FriendRequest1 = x.FromFriendRequests.Where(r => r.ToUserID == userID).Select(r=> r.Status).FirstOrDefault(),
                          //FriendRequest2 = x.ToFriendRequests.Where(r => r.FromUserID == userID).FirstOrDefault(),
                          //HasSentFriendReuestToMine = x.FromFriendRequests
                          //                           .Where(r=> !r.IsDeleted)
                          //                          .Where(r => r.ToUserID == userID && r.Status == (int)FriendRequestStatus.Pending)
                          //                        //  .Where(r => r.Status == (int)FriendRequestStatus.Pending )
                          //                          .Any() ,
                          //HasReceivedFriendRequestFromMine = x.ToFriendRequests
                          //                              .Where(r => !r.IsDeleted)
                          //                              .Where(r => r.FromUserID == userID)
                          //                              .Where(r => r.Status == (int)FriendRequestStatus.Pending)
                          //                              .Any() ,
                          //IsFriend =  x.FromFriendRequests.Where(f=>!f.IsDeleted && f.Status == (int)FriendRequestStatus.Approved)
                          //              .Where(f => f.ToUserID == userID).Any() 
                          //           || x.ToFriendRequests.Where(f => !f.IsDeleted && f.Status == (int)FriendRequestStatus.Approved)
                          //              .Where(f => f.FromUserID == userID).Any(),
                          //IsBlocked = x.FromFriendRequests.Where(f => !f.IsDeleted && f.Status == (int)FriendRequestStatus.Blocked)
                          //              .Where(f => f.ToUserID == userID).Any()
                          //           || x.ToFriendRequests.Where(f => !f.IsDeleted && f.Status == (int)FriendRequestStatus.Blocked)
                          //              .Where(f => f.FromUserID == userID).Any()
                      }).ToList();
           
            
            result = users.ToList();

            foreach (var x in result)
            {
                if (x.HasRelation)
                {
                    if (x.IsLoggedIDComesFirst)
                    {
                        var sourceUserID = x.ID;
                        var request = _friendRequestRepository.GetAll().Where(r => r.FromUserID == userID && r.ToUserID == sourceUserID).FirstOrDefault();
                        x.Status = request.Status;
                        if(x.Status == (int)FriendRequestStatus.Blocked)
                        {
                            x.IsExcluded = true;
                        }
                        x.ActionByLoggedUser = userID == request.ActionBy;
                    }
                    else
                    {
                        var sourceUserID = x.ID;
                        var request = _friendRequestRepository.GetAll().Where(r => r.FromUserID == sourceUserID && r.ToUserID == userID).FirstOrDefault();
                        x.Status = request.Status;
                        if (x.Status == (int)FriendRequestStatus.Blocked)
                        {
                            x.IsExcluded = true;
                        }
                        x.ActionByLoggedUser = userID == request.ActionBy;

                    }
                }
               
            }
            result = result.Where(x => !x.IsExcluded).ToList();
            foreach (var item in result)
            {

                var nameParts = item.DisplayName.Split(' ');
                for (int i = 0; i < nameParts.Length; i++)
                {
                    if (nameParts[nameParts.Length - 1 - i].Contains(text))
                    {
                        item.SyllableContainsText = nameParts.Length - 1 - i;
                    }
                }

            }
            result = result.OrderBy(x => x.SyllableContainsText).ToList();

            foreach (UserSearchViewModel item in result)
            {
                var firstPart = item.DisplayName.Split(' ')[0].ToCharArray();
                for (int i = 0; i < firstPart.Length; i++)
                {
                    if (firstPart[firstPart.Length - 1 - i].ToString().Contains(text))
                    {
                        item.LetterNoInFirstWord = firstPart.Length - 1 - i;
                    }
                }
                if(item.DisplayName.Split(' ').Length > 1)
                {
                    var secondPart = item.DisplayName.Split(' ')[1].ToCharArray();
                    for (int i = 0; i < secondPart.Length; i++)
                    {
                        if (secondPart[secondPart.Length - 1 - i].ToString().Contains(text))
                        {
                            item.LetterNoInSecondWord = firstPart.Length - 1 - i;
                        }
                    }
                }
            }
            result = result.OrderBy(x => x.SyllableContainsText)
                .ThenBy(x => x.LetterNoInFirstWord)
                .ThenBy(x => x.LetterNoInSecondWord).ToList();
            return new PagingViewModel<UserSearchViewModel>() { PageIndex = pageIndex, PageSize = pageSize, Result = result, Records = records, Pages = pages };
        }


        public bool IsExistUser(int userId)
        {       
            return _repo.GetAll()
                    .Where(u => u.ID == userId)
                    .Any();
        }

    }

    public class AdminViewModel
    {
        public int ID { get; set; }
        public string UserMainImagePath { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public int SyllableContainsText { get; internal set; }
    }

    public class UserViewModel 
    {
        public int ID { get; set; }
        public string UserMainImagePath { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public int SyllableContainsText { get; set; }
        public string Email { get; internal set; }
        public string Phone { get; internal set; }
        public DateTime CreatedDate { get; internal set; }
    }

    public class UserSearchViewModel
    {
        public int ID { get; set; }
        public string UserMainImagePath { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        [IgnoreDataMember]
        public int SyllableContainsText { get; set; }
        [IgnoreDataMember]
        public int LetterNoInFirstWord { get; set; } = 100;
        [IgnoreDataMember]
        public int LetterNoInSecondWord { get; set; } = 100;

        //public bool HasSentFriendReuestToMine { get; set; }
        //public bool HasReceivedFriendRequestFromMine { get; set; }
        //// is friend to logged user
        //public bool IsFriend { get; set; }
        //// is blocked by logged user
        //[IgnoreDataMember]
        //public bool IsBlocked { get; set; }
        public int Status { get; internal set; }
        public bool ActionByLoggedUser { get; internal set; }
        public bool IsLoggedIDComesFirst { get; internal set; }
        public bool HasRelation { get; internal set; }
        public bool IsExcluded { get; internal set; }
    }
}
