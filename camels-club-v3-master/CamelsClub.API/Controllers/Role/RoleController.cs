using CamelsClub.API.Filters;
using CamelsClub.Data.UnitOfWork;
using CamelsClub.Localization.Shared;
using CamelsClub.Services;
using CamelsClub.ViewModels;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace CamelsClub.API.Controllers.Comment
{
    public class RoleController : BaseController
    {
        private readonly IUnitOfWork _unit;
        private readonly IRoleService _roleService;

        public RoleController(IUnitOfWork unit, IRoleService roleService)
        {
            _unit = unit;
            _roleService = roleService;

        }

        [HttpPost]
        [AuthorizeUserFilter(Role ="Admin")]
        [Route("api/Role/Add")]
        public ResultViewModel<bool> Add(RoleCreateViewModel viewModel)
        {
            try
            {
                var resultViewModel = new ResultViewModel<bool>();

                resultViewModel.Data = _roleService.AddRole(viewModel);
                resultViewModel.Success = true;
                resultViewModel.Message = Resource.DataLoaded;
                return resultViewModel;

            }
            catch (Exception ex)
            {
                return new ResultViewModel<bool>(ex.Message);
            }
        }

    }

       
}
