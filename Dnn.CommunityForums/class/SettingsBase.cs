//
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
using System.Reflection;
using System.Web;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Modules.ActiveForums.Data;
using DotNetNuke.Security.Permissions;

namespace DotNetNuke.Modules.ActiveForums
{
    public class SettingsBase : PortalModuleBase
    {
        #region Private Members
        private int _forumModuleId = -1;
        private string _LoadView = string.Empty;
        private int _LoadGroupForumID = 0;
        private int _LoadPostID = 0;
        private string _imagePath = string.Empty;
        private string _Params = string.Empty;
        private int _forumTabId = -1;
        #endregion

        #region Public Properties
        internal User ForumUser
        {
            get
            {
                var uc = new UserController();
                return uc.GetUser(PortalId, ForumModuleId);
            }
        }

        internal string UserForumsList
        {
            get
            {
                string forums;
                if (string.IsNullOrEmpty(ForumUser.UserForums))
                {
                    var fc = new ForumController();
                    forums = fc.GetForumsForUser(ForumUser.UserRoles, PortalId, ForumModuleId);
                    ForumUser.UserForums = forums;
                }
                else
                {
                    forums = ForumUser.UserForums;
                }
                return forums;
            }
        }

        public int ForumModuleId
        {
            get
            {
                if (_forumModuleId > 0)
                {
                    return _forumModuleId;
                }
                return ModuleId;
            }
            set
            {
                _forumModuleId = value;
            }
        }

        public int ForumTabId
        {
            get
            {
                return _forumTabId;
            }
            set
            {
                _forumTabId = value;
            }
        }

        public string Params
        {
            get
            {
                return _Params;
            }
            set
            {
                _Params = value;
            }
        }

        public bool UseAjax
        {
            get
            {
                bool tempUseAjax = Request.IsAuthenticated && UserPrefUseAjax;

                return tempUseAjax;
            }
        }

        public int PageId
        {
            get
            {
                int tempPageId = 0;
                if (Request.QueryString[ParamKeys.PageId] != null)
                {
                    if (SimulateIsNumeric.IsNumeric(Request.QueryString[ParamKeys.PageId]))
                    {
                        tempPageId = Convert.ToInt32(Request.QueryString[ParamKeys.PageId]);
                    }
                }
                else if (Request.QueryString["page"] != null)
                {
                    if (SimulateIsNumeric.IsNumeric(Request.QueryString["page"]))
                    {
                        tempPageId = Convert.ToInt32(Request.QueryString["page"]);
                    }
                }
                else if (Params != string.Empty && Params.Contains("PageId"))
                {
                    tempPageId = Convert.ToInt32(Params.Split('=')[1]);
                }
                else
                {
                    tempPageId = 1;
                }
                return tempPageId;
            }
        }

        public bool ShowToolbar { get; set; } = true;
        #endregion

        public UserController UserController
        {
            get
            {
                const string userControllerContextKey = "AF|UserController";
                var userController = HttpContext.Current.Items[userControllerContextKey] as UserController;
                if (userController == null)
                {
                    userController = new UserController();
                    HttpContext.Current.Items[userControllerContextKey] = userController;
                }
                return userController;
            }
        }

        public ForumController ForumController
        {
            get
            {
                const string forumControllerContextKey = "AF|ForumController";
                var forumController = HttpContext.Current.Items[forumControllerContextKey] as ForumController;
                if (forumController == null)
                {
                    forumController = new ForumController();
                    HttpContext.Current.Items[forumControllerContextKey] = forumController;
                }
                return forumController;
            }
        }

        public ForumsDB ForumsDB
        {
            get
            {
                const string forumsDBContextKey = "AF|ForumsDB";
                var forumsDB = HttpContext.Current.Items[forumsDBContextKey] as ForumsDB;
                if (forumsDB == null)
                {
                    forumsDB = new ForumsDB();
                    HttpContext.Current.Items[forumsDBContextKey] = forumsDB;
                }
                return forumsDB;
            }
        }

        #region Public Properties - User Preferences
        public CurrentUserTypes CurrentUserType
        {
            get
            {
                if (Request.IsAuthenticated)
                {
                    if (UserInfo.IsSuperUser)
                    {
                        return CurrentUserTypes.SuperUser;
                    }
                    if (ModulePermissionController.HasModulePermission(ModuleConfiguration.ModulePermissions, "EDIT"))
                    {
                        return CurrentUserTypes.Admin;
                    }
                    if (ForumUser.Profile.IsMod)
                    {
                        return CurrentUserTypes.ForumMod;
                    }
                    return CurrentUserTypes.Auth;
                }
                return CurrentUserTypes.Anon;
            }
        }

        public bool UserIsMod
        {
            get
            {
                if (UserId == -1)
                {
                    return false;
                }
                if (ForumUser != null)
                {
                    return ForumUser.Profile.IsMod;
                }
                return false;
            }
        }

        public string UserDefaultSort
        {
            get
            {
                if (UserId != -1)
                {
                    return ForumUser.Profile.PrefDefaultSort;
                }
                return "ASC";
            }
        }

        public int UserDefaultPageSize
        {
            get
            {
                if (UserId != -1)
                {
                    return ForumUser.Profile.PrefPageSize;
                }
                return MainSettings.PageSize;
            }
        }

        public bool UserPrefHideSigs
        {
            get
            {
                if (UserId != -1)
                {
                    try
                    {
                        return ForumUser.Profile.PrefBlockSignatures;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
                return false;
            }
        }

        public bool UserPrefHideAvatars
        {
            get
            {
                if (UserId != -1)
                {
                    return ForumUser.Profile.PrefBlockAvatars;
                }
                return false;
            }
        }

        public bool UserPrefJumpLastPost
        {
            get
            {
                if (UserId != -1)
                {
                    return ForumUser.Profile.PrefJumpLastPost;
                }
                return false;
            }
        }
        public bool UserPrefUseAjax
        {
            get
            {
                if (UserId != -1)
                {
                    return ForumUser.Profile.PrefUseAjax;
                }
                return false;
            }
        }

        public bool UserPrefShowReplies
        {
            get
            {
                if (UserId != -1)
                {
                    return ForumUser.Profile.PrefDefaultShowReplies;
                }
                return false;
            }
        }

        public bool UserPrefTopicSubscribe
        {
            get
            {
                if (UserId != -1)
                {
                    return ForumUser.Profile.PrefTopicSubscribe;
                }
                return false;
            }
        }
        #endregion

        #region Public ReadOnly Properties
        public Framework.CDefault BasePage
        {
            get
            {
                return (Framework.CDefault)Page;
            }
        }
        public static SettingsInfo GetModuleSettings(int ModuleId)
        {
            SettingsInfo objSettings = (SettingsInfo)DataCache.SettingsCacheRetrieve(ModuleId,string.Format(CacheKeys.MainSettings, ModuleId));
            if (objSettings == null && ModuleId > 0)
            {
                objSettings = new SettingsInfo { MainSettings = new DotNetNuke.Entities.Modules.ModuleController().GetModule(ModuleId).ModuleSettings };
                DataCache.SettingsCacheStore(ModuleId,string.Format(CacheKeys.MainSettings, ModuleId), objSettings);
            }
            return objSettings;
            
        }
        public SettingsInfo MainSettings
        {
            get
            {
                ForumModuleId = _forumModuleId <= 0 ? ForumModuleId : _forumModuleId;
                return GetModuleSettings(ForumModuleId);
            }
        }

        public string ImagePath
        {
            get
            {
                return Page.ResolveUrl(string.Concat(MainSettings.ThemeLocation, "/images"));
            }
        }

        public string GetViewType
        {
            get
            {
                if (Request.Params[ParamKeys.ViewType] != null)
                {
                    return Request.Params[ParamKeys.ViewType].ToUpperInvariant();
                }
                if (Request.Params["view"] != null)
                {
                    return Request.Params["view"].ToUpperInvariant();
                }
                return null;
            }
        }

        public TimeSpan TimeZoneOffset
        {
            /* AF now stores datetime in UTC, so this method returns timezoneoffset for current user if available or from portal settings as fallback */
            get
            {
                return Utilities.GetTimeZoneOffsetForUser(UserInfo);      
            }
        }

        #endregion

        #region Protected Methods
        protected DateTime GetUserDate(DateTime displayDate)
        {
            return Utilities.GetUserDate(displayDate, ModuleId, Convert.ToInt32(TimeZoneOffset.TotalMinutes));
        }

        protected string GetServerDateTime(DateTime DisplayDate)
        {
            //Dim newDate As Date 
            string dateString;
            try
            {
                dateString = DisplayDate.ToString(string.Concat(MainSettings.DateFormatString, " ", MainSettings.TimeFormatString));
                return dateString;
            }
            catch (Exception ex)
            {
                dateString = DisplayDate.ToString();
                return dateString;
            }
        }
        #endregion

        #region Public Methods
        public string NavigateUrl(int TabId)
        {
            return Utilities.NavigateUrl(TabId);
        }
        public string NavigateUrl(int TabId, string ControlKey, params string[] AdditionalParameters)
        {
            return Utilities.NavigateUrl(TabId, ControlKey, AdditionalParameters);
        }
        private string[] AddParams(string param, string[] currParams)
        {
            var tmpParams = new[] { param };
            int intLength = tmpParams.Length;
            Array.Resize(ref tmpParams, (intLength + currParams.Length));
            currParams.CopyTo(tmpParams, intLength);
            return tmpParams;
        }

        public void RenderMessage(string Title, string Message)
        {
            RenderMessage(Utilities.GetSharedResource(Title), Message, string.Empty, null);
        }
        public void RenderMessage(string Message, string ErrorMsg, Exception ex)
        {
            RenderMessage(Utilities.GetSharedResource("[RESX:Error]"), Message, ErrorMsg, ex);
        }
        public void RenderMessage(string Title, string Message, string ErrorMsg, Exception ex)
        {
            var im = new Controls.InfoMessage {Message = string.Concat(Utilities.GetSharedResource(Message), "<br />")};
            if (ex != null)
            {
                im.Message = im.Message + ex.Message;
            }
            if (ex != null)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, ex);
            }

        }

        protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

            if (Request.Params["view"] != null)
            {
                string sUrl;
                string sParams = string.Empty;
                
                if (Request.Params["forumid"] != null)
                {
                    if (SimulateIsNumeric.IsNumeric(Request.Params["ForumId"]))
                    {
                        sParams = string.Concat(ParamKeys.ForumId, "=", Request.Params["ForumId"]);
                    }
                }
 
                if (Request.Params["postid"] != null)
                {
                    if (SimulateIsNumeric.IsNumeric(Request.Params["postid"]))
                    {
                        sParams += string.Concat("|", ParamKeys.TopicId, "=", Request.Params["postid"]);
                    }
                }
                
                sParams += string.Concat("|", ParamKeys.ViewType, "=", Request.Params["view"]);
                sUrl = NavigateUrl(TabId, "", sParams.Split('|'));

                Response.Status = "301 Moved Permanently";
                Response.AddHeader("Location", sUrl);
            }

            ServicesFramework.Instance.RequestAjaxAntiForgerySupport();

        }
        #endregion
    }
}