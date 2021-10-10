using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamelsClub.Services
{
    public static class NotificationArabicKeys
    {
        public static string NewPost { get; private set; } = "تمت عملية اضافة منشور من قبل";
        public static string NewComment { get; private set; } = "هناك تعليق على منشور خاص بك";
       // public static string NewCommentToCommenters { get; private set; } = "هناك تعليق على منشور قمت بالتعليق عليه";
        public static string NewCompetitionAnnounceToCompetitor { get; private set; } = "هناك مسابقة جديدة، ان ترغب المشاركة بها";
        public static string NewCompetitionAnnounceToChecker { get; private set; } = "تم دعوتك للاشتراك فى تمييز المسابقة ";
        public static string NewCompetitionAnnounceToReferee { get; private set; } = "تم دعوتك للاشتراك في تحكييم  ";
        public static string PublishCompetition { get; private set; } = "تم الانتهاء من مسابقة ";
        public static string JoinCompetitionAsReferee { get; private set; } = "رئيس التحكيم وافق على مسابقة";
        public static string JoinCompetitionAsChecker { get; private set; } = "رئيس التمييز وافق على مسابقة";
        public static string RejectCompetitionAsChecker { get; private set; } = " رئيس التمييز لم يوافق على مسابقة";
        public static string RejectCompetitionAsReferee { get; private set; } = "رئيس التحكيم لم يوافق على مسابقة";
        public static string CompletedRefereesMinimumCount { get; private set; } = "تم اعتماد فريق لجان التحكيم بعد الموافقة";
        public static string CompletedCheckersMinimumCount { get; private set; } = "تم اعتماد فريق لجان التمميز بعد الموافقة";

    }
}
