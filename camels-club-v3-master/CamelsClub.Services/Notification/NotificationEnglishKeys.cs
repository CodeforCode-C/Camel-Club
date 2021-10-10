using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamelsClub.Services
{
    public static class NotificationEnglishKeys
    {
        public static string NewPost { get; private set; } = "new post has been added by ";
        public static string NewComment { get; private set; } = "new comment has been added on this post";
        public static string NewCompetitionAnnounceToCompetitor { get; private set; } = "there is a competitor to join ";
        public static string NewCompetitionAnnounceToChecker { get; private set; } = "you have been invited as checker to join this competition ";
        public static string NewCompetitionAnnounceToReferee { get; private set; } = "you have been invited as referee to join this competition ";
        public static string PublishCompetition { get; private set; } = "results is published for competition ";
        public static string JoinCompetitionAsReferee { get; private set; } = "One Of Referees has joined to Competition";
        public static string CompletedRefereesMinimumCount { get; private set; } = "Referees have been Approved for joining competition";
        public static string CompletedCheckersMinimumCount { get; private set; } = "Checkers have been Approved for joining competition";
        public static string JoinCompetitionAsChecker { get; private set; } = "one of checkers has joined to competition";
        public static string RejectCompetitionAsChecker { get; private set; } = "one of checkers has rejected competition";
        public static string RejectCompetitionAsReferee { get; private set; } = "one of referees has rejected competition";

    }
}
