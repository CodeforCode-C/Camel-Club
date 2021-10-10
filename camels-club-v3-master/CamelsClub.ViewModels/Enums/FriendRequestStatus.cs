using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamelsClub.ViewModels.Enums
{
    public enum FriendRequestStatus
    {
        NoFriendRequest = 0,
        Pending = 1,
        Approved = 2,
        Declined = 3,
        Blocked = 4,
        UnSeen = 5
    }
}
