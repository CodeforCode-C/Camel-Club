
using Autofac.Core;
using CamelsClub.Data.Context;
using CamelsClub.Data.Helpers;
using CamelsClub.Data.UnitOfWork;
using CamelsClub.Repositories;
using CamelsClub.Services;
using CamelsClub.Services.Helpers;
using CamelsClub.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;


namespace CamelsClub.API.Filters
{
    public class AuthorizeUserFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
     
        //private readonly IUserRoleService _userRoleService;
        //private readonly ITokenService _tokenService;
        //private readonly IUnitOfWork _unitOfWork;

        public string Role { get; set; }
        
        public AuthorizeUserFilter()
        {
            //var context = new CamelsClubContext();
            //_unitOfWork = new UnitOfWork(context);
       
            //_tokenService = new TokenService(_unitOfWork , new TokenRepository(context));
            //_userRoleService = new UserRoleService(_unitOfWork, new UserRoleRepository(context));
        }

  
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            
            string iP = HttpContext.Current.Request.UserHostAddress.ToLower();
            bool allowLocal = false;

            if (!HttpContext.Current.Request.IsLocal || !allowLocal)
            {
                bool isAuthorized = false;
                int tokenID = 0; //output parameter
                string accessToken = "";
                try
                {
                    string accessTokenHeaderName = "token";
                    if (actionContext.Request.Headers.Any(header => header.Key == accessTokenHeaderName))
                    {
                        accessToken = actionContext.Request.Headers.GetValues(accessTokenHeaderName).FirstOrDefault();
                        if (!SecurityHelper.IsTokenExpired(accessToken))
                        {
                            int userID = SecurityHelper.GetUserIDFromToken(accessToken);
                            // _userRoleService still see it null
                            if (userID > 0 &&HasRole(userID, Role))//  _userService.HasRole(userID, Role))
                            {
                                //_tokenService.UserID = UserID.ToString();
                                accessToken = SecurityHelper.Encrypt(accessToken);
                                DateTime currentDateTime = DateTime.Now;
                                isAuthorized = IsValidToken(userID, accessToken, out tokenID);

                            }

                        }
                    }



                }
                catch (Exception ex)
                {
                    
                }
                if(tokenID != 0)
                AddTokenLog(tokenID, actionContext.Request.RequestUri.AbsoluteUri,CamelsClub.Services.Helpers.HttpRequestHelper.GetClientIP(), isAuthorized);
                //_tokenService.AddLog(accessToken, isAuthorized, actionContext.Request.RequestUri.AbsoluteUri, actionContext.Request.GetClientIP());
                //_unitOfWork.Save();

                //isAuthorized = true;
                if (!isAuthorized)
                {
                    var resultViewModel = new ResultViewModel<string>();
                    resultViewModel.Success = false;
                    resultViewModel.Message = "Unauthorized";
                    resultViewModel.Authorized = false;
                    actionContext.Response = actionContext.Request.CreateResponse(
                                                                        HttpStatusCode.Unauthorized,
                                                                        resultViewModel,
                                                                        actionContext.ControllerContext.Configuration.Formatters.JsonFormatter);
                    //throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.Unauthorized));
                }
            }

            base.OnActionExecuting(actionContext);
        }

        private bool IsValidToken(int userID, string token, out int tokenID)
        {
            bool isValidToken = false;
            var connection = "Data Source=camels-club-v2.pivotrs.com;Initial Catalog=camels-club-v2;MultipleActiveResultSets=true;Integrated Security=false; User ID=camels-club-v2;Password=F#1#top@@";
            string sql = "IsValidToken";
            using (var cnn = new SqlConnection(connection))
            {
                using (var cmd = new SqlCommand(sql, cnn))
                {
                    cmd.Parameters.Add(new SqlParameter("@token", token));
                    cmd.Parameters.Add(new SqlParameter
                    {
                        ParameterName = "@isValid",
                        IsNullable = false,
                        DbType = System.Data.DbType.Boolean,
                        Direction = System.Data.ParameterDirection.Output
                    });
                    cmd.Parameters.Add(new SqlParameter
                    {
                        ParameterName = "@tokenID",
                        IsNullable = false,
                        DbType = System.Data.DbType.Int32,
                        Direction = System.Data.ParameterDirection.Output
                    });

                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cnn.Open();
                    var rowsAffected = cmd.ExecuteNonQuery();
                    isValidToken = (bool)cmd.Parameters["@isValid"].Value;
                    tokenID = (int)cmd.Parameters["@tokenID"].Value;
                }
                cnn.Close();

            }

            return isValidToken;

        }

        private bool AddTokenLog(int tokenID, string url, string ip, bool isAuthorized)
        {
            var connection = "Data Source=camels-club-v2.pivotrs.com;Initial Catalog=camels-club-v2;MultipleActiveResultSets=true;Integrated Security=false; User ID=camels-club-v2;Password=F#1#top@@";
            string sql = "AddTokenLog";
            
            using (var cnn = new SqlConnection(connection))
            {
                using (var cmd = new SqlCommand(sql, cnn))
                {
                    cmd.Parameters.Add(new SqlParameter("@TokenID", tokenID));
                    cmd.Parameters.Add(new SqlParameter("@IP", ip));
                    cmd.Parameters.Add(new SqlParameter("@URL", url));
                    cmd.Parameters.Add(new SqlParameter("@IsAuthorized", isAuthorized));
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cnn.Open();
                    var rowsAffected = cmd.ExecuteNonQuery();
                }
             cnn.Close();
            }

            return true;

        }

        private bool HasRole(int userID, string role)
        {
            bool hasRole = false;
            var connection = "Data Source=camels-club-v2.pivotrs.com;Initial Catalog=camels-club-v2;MultipleActiveResultSets=true;Integrated Security=false; User ID=camels-club-v2;Password=F#1#top@@";
            string sql = "HasRole";
            using (var cnn = new SqlConnection(connection))
            {
                using (var cmd = new SqlCommand(sql, cnn))
                {
                    cmd.Parameters.Add(new SqlParameter("@role", role));
                    cmd.Parameters.Add(new SqlParameter("@userID", userID));
                    cmd.Parameters.Add(new SqlParameter
                    {
                        ParameterName = "@hasRole",
                        IsNullable = false,
                        DbType = System.Data.DbType.Boolean,
                        Direction = System.Data.ParameterDirection.Output
                    });
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cnn.Open();
                    var rowsAffected = cmd.ExecuteNonQuery();
                    hasRole = (bool)cmd.Parameters["@hasRole"].Value;
                }

                cnn.Close();
            }

            return hasRole;

        }
    }
}