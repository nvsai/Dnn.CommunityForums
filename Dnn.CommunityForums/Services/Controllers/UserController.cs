﻿//
// Community Forums
// Copyright (c) 2013-2021
// by DNN Community
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//
using System;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.UI.UserControls;
using DotNetNuke.Web.Api;
using static DotNetNuke.Modules.ActiveForums.Handlers.HandlerBase;

namespace DotNetNuke.Modules.ActiveForums.Services.Controllers
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public class UserController : ControllerBase<UserController>
    {
        /// <summary>
        /// Fired by UI while user is online to update user's profile with 
        /// </summary>
        /// <param>none</param>
        /// <returns></returns>
        /// <remarks>https://dnndev.me/API/ActiveForums/User/UpdateUserIsOnline</remarks>
        [HttpPost]
        [DnnAuthorize]
        public HttpResponseMessage UpdateUserIsOnline()
        {
            try
            {
                if (UserInfo.UserID > 0)
                {
                    DataProvider.Instance().Profiles_UpdateActivity(PortalSettings.PortalId, ActiveModule.ModuleID, UserInfo.UserID);
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            return Request.CreateResponse(HttpStatusCode.OK);
        }
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetUsersOnline()
        {
            {
                UsersOnline uo = new UsersOnline();
                string sOnlineList = uo.GetUsersOnline(PortalSettings.PortalId, ActiveModule.ModuleID, UserInfo);
                IDataReader dr = DataProvider.Instance().Profiles_GetStats(PortalSettings.PortalId, ActiveModule.ModuleID, 2);
                int anonCount = 0;
                int memCount = 0;
                int memTotal = 0;
                while (dr.Read())
                {
                    anonCount = Convert.ToInt32(dr["Guests"]);
                    memCount = Convert.ToInt32(dr["Members"]);
                    memTotal = Convert.ToInt32(dr["MembersTotal"]);
                }
                dr.Close();
                string sUsersOnline = null;
                sUsersOnline = Utilities.GetSharedResource("[RESX:UsersOnline]");
                sUsersOnline = sUsersOnline.Replace("[USERCOUNT]", memCount.ToString());
                sUsersOnline = sUsersOnline.Replace("[TOTALMEMBERCOUNT]", memTotal.ToString());
                return Request.CreateResponse(HttpStatusCode.OK, sUsersOnline + " " + sOnlineList);
            }
        }
    }
}