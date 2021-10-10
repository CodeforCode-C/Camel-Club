using CamelsClub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamelsClub.ViewModels
{
    public class NotificationViewModel
    {
        public int ID { get; set; }
        public string Content { get; set; }
        public bool IsSeen { get; set; }
        public int NotificationTypeID { get; set; }
        public string NotificationType { get; set; }
        public int SourceID { get; set; }
        public string SourceName { get; set; }
        public int DestinationID { get; set; }
        public string DestinationName { get; set; }
        public int? CompetitionID { get; set; }
        public int? CommentID { get; set; }
        public int? PostID { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool HasJoinedCompetition { get; set; }
        public bool IsBossChecker { get; set; }
        public bool IsChecker { get; set; }
        public bool IsBossReferee { get; set; }
        public bool IsReferee { get; set; }
        public bool IsInvitedUser { get; set; }
        public string SourceUserImage { get; set; }
        public string DestinationDisplayName { get; set; }
        //public bool IsFriend { get; set; }
        //public bool HasReceivedFriendRequestFromMine { get; set; }
        //public bool HasSentFriendReuestToMine { get; set; }
        //public bool IsBlocked { get; set; }
        public DateTime? CompetitorsEndDateJoin { get; set; }
        public bool? ExpiredCompetition { get; set; }
        public string SourceDisplayName { get; set; }
        public int Status { get; set; }
        public bool HasRelation { get; set; }
        public bool IsLoggedIDComesFirst { get; set; }
        public bool ActionByLoggedUser { get; set; }
        public string CompetitionImage { get; set; }
        public string CompetitionNameArabic { get; set; }
        public string CompetitionNameEnglish { get; set; }
        public bool HasRejectedCompetition { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsPublished { get; set; }
        public bool IsPostExist { get; set; }
    }
}

