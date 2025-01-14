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
using System.Collections;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI;
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Modules.ActiveForums
{
    public class ActiveAdminBase : DotNetNuke.Entities.Modules.PortalModuleBase
    {
        private string _currentView = string.Empty;
        private DateTime _CacheUpdatedTime;
        public const string RequiredImage = Globals.ModulePath + "images/error.gif";
        
        #region Constants
        internal const string ViewKey = "afcpView";
        internal const string ParamKey = "afcpParams";
        internal const string DefaultView = "home";
        #endregion

        public string Params { get; set; } = string.Empty;
        public bool IsCallBack { get; set; }

        public string HostURL
        {
            get
            {
                object obj = DataCache.SettingsCacheRetrieve(ModuleId, string.Format(CacheKeys.HostUrl, ModuleId));
                if (obj == null)
                {
                    string sURL;
                    if (Request.IsSecureConnection)
                    {
                        sURL = string.Concat("https://", Common.Globals.GetDomainName(Request), "/");
                    }
                    else
                    {
                        sURL = string.Concat("http://", Common.Globals.GetDomainName(Request), "/");
                    }
                    DataCache.SettingsCacheStore(ModuleId,string.Format(CacheKeys.HostUrl, ModuleId), sURL, DateTime.UtcNow.AddMinutes(30));
                    return sURL;
                }
                return Convert.ToString(obj);
            }
        }
        
        protected string GetSharedResource(string key)
        {
            return Utilities.GetSharedResource(key, true);
        }
        
        public SettingsInfo MainSettings
        {
            get
            {
                return new SettingsInfo { MainSettings = new ModuleController().GetModule(moduleID: ModuleId).ModuleSettings };
            }
        }
        
        protected override void OnInit(EventArgs e)
        {
 	        base.OnInit(e);
            LocalResourceFile = Globals.ControlPanelResourceFile;
        }

        public string LocalizeControl(string controlText)
        {
            return Utilities.LocalizeControl(controlText, true);
        }
        
        public Controls.ClientTemplate GetLoadingTemplateSmall()
        {
            var template = new Controls.ClientTemplate {ID = "LoadingTemplate"};
            template.Controls.Add(new LiteralControl(string.Concat("<div style=\"text-align:center;font-family:Tahoma;font-size:10px;\"><img src=\"", Page.ResolveUrl("~/DesktopModules/ActiveForums/images/spinner.gif"), "\" align=\"absmiddle\" alt=\"Loading\" />Loading...</div>")));
            return template;
        }
        public void BindTemplateDropDown(DropDownList drp, Templates.TemplateTypes TemplateType, string DefaultText, string DefaultValue)
        {
            var tc = new TemplateController();
            drp.DataTextField = "Title";
            drp.DataValueField = "TemplateID";
            drp.DataSource = tc.Template_List(PortalId, ModuleId, TemplateType);
            drp.DataBind();
            drp.Items.Insert(0, new ListItem(DefaultText, DefaultValue));
        }
        public string CurrentView
        {
            get
            {
                if (Session[ViewKey] != null)
                {
                    return Session[ViewKey].ToString();
                }
                if (_currentView != string.Empty)
                {
                    return _currentView;
                }
                return DefaultView;
            }
            set
            {
                Session[ViewKey] = value;
                _currentView = value;
            }
        }
        
    }
}