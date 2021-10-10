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
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unit;
        private readonly IRolePagePermissionRepository _repo;
        private readonly IRoleRepository _roleRepository;

        public RoleService(IUnitOfWork unit,
                            IRoleRepository roleRepository,
                            IRolePagePermissionRepository repo)
        {
            _unit = unit;
            _roleRepository = roleRepository;
            _repo = repo;
        }

        public bool AddRole(RoleCreateViewModel viewModel)
        {
            var role = _roleRepository.Add(new Role
            {
                NameArabic = viewModel.Name,
                NameEnglish = viewModel.Name
            });
            foreach (var page in viewModel.Pages)
            {
                foreach (var permission in page.Permissions)
                {
                    _repo.Add(new RolePagePermission
                    {
                        RoleID = role.ID,
                        PermissionID = permission.ID,
                        PageID = page.ID
                    });
                }
            }

            _unit.Save();
            return true;
        }
    }
}

