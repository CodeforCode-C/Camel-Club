using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamelsClub.Models
{
    public class CompetitionGeneralConditionConfiguration : EntityTypeConfiguration<CompetitionGeneralCondition>
    {
        public CompetitionGeneralConditionConfiguration()
        {
            this.ToTable("CompetitionGeneralCondition", "Competition");
            this.HasKey<int>(x => x.ID);
            this.Property(x => x.CreatedDate).IsRequired();
            this.Property(x => x.TextArabic).IsRequired();
        }
    }
}
