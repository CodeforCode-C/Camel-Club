using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamelsClub.Models
{
    public class RolePagePermissionConfiguration : EntityTypeConfiguration<RolePagePermission>
    {
        public RolePagePermissionConfiguration()
        {
            this.ToTable("RolePagePermission", "Identity");
            this.HasKey<int>(x => x.ID);
            this.Property(x => x.CreatedDate).IsRequired();
            this.HasRequired<Role>(x => x.Role)
                .WithMany(x => x.RolePagePermissions)
                .HasForeignKey<int>(x => x.RoleID)
                .WillCascadeOnDelete(false);


            this.HasRequired<Page>(x => x.Page)
                .WithMany(x => x.RolePagePermissions)
                .HasForeignKey<int>(x => x.PageID)
                .WillCascadeOnDelete(false);


            this.HasRequired<Permission>(x => x.Permission)
                           .WithMany(x => x.RolePagePermissions)
                           .HasForeignKey<int>(x => x.PermissionID)
                            .WillCascadeOnDelete(false);

        }
    }
}
