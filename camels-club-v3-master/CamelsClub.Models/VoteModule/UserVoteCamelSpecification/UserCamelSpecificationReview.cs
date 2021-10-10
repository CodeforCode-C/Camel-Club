using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamelsClub.Models
{
    public class UserCamelSpecificationReview : BaseModel
    { 
        public int UserID { get; set; }
        public virtual User User { get; set; }  //judgerID
        public int CamelCompetitionID { get; set; }
        public virtual CamelCompetition CamelCompetition { get; set; }
        public int CamelSpecificationID { get; set; }
        public virtual CamelSpecification CamelSpecification { get; set; }
        public double ActualPercentageValue { get; set; }
    }
}
