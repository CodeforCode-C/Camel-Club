using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamelsClub.Models
{
    public class SharedPostConfiguration : EntityTypeConfiguration<SharedPost>
    {
        public SharedPostConfiguration()
        {
            this.ToTable("SharedPost", "User");
           
            this.HasKey<int>(x => x.ID);
          
            this.Property(x => x.CreatedDate).IsRequired();
           
            this.HasRequired<Post>(x => x.Post)
                .WithMany(x => x.SharedUsers)
                .HasForeignKey<int>(x => x.PostID)
                .WillCascadeOnDelete(false);

            this.HasRequired<User>(x => x.User)
                .WithMany(x => x.SharedPosts)
                .HasForeignKey<int>(x => x.UserID)
                .WillCascadeOnDelete(false);
        }
    }
}
