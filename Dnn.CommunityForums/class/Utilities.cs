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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;
using System.Reflection;
using System.Web.UI.WebControls;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Security.Roles;
using DotNetNuke.Framework;

namespace DotNetNuke.Modules.ActiveForums
{
    public abstract partial class Utilities
    {
        internal static CultureInfo DateTimeStringCultureInfo = new CultureInfo("en-US", true);


        /// <summary>
        /// Calculates a friendly display string based on an input timespan
        /// </summary>
        public static string HumanFriendlyDate(DateTime displayDate, int ModuleId, int timeZoneOffset)
        {
            var newDate = DateTime.Parse(GetDate(displayDate, ModuleId, timeZoneOffset));
            var ts = new TimeSpan(DateTime.Now.Ticks - newDate.Ticks);
            var delta = ts.TotalSeconds;
            if (delta <= 1)
                return GetSharedResource("[RESX:TimeSpan:SecondAgo]");

            if (delta < 60)
                return string.Format(GetSharedResource("[RESX:TimeSpan:SecondsAgo]"), ts.Seconds);

            if (delta < 120)
                return GetSharedResource("[RESX:TimeSpan:MinuteAgo]");

            if (delta < (45 * 60))
                return string.Format(GetSharedResource("[RESX:TimeSpan:MinutesAgo]"), ts.Minutes);

            if (delta < (90 * 60))
                return GetSharedResource("[RESX:TimeSpan:HourAgo]");

            if (delta < (24 * 60 * 60))
                return string.Format(GetSharedResource("[RESX:TimeSpan:HoursAgo]"), ts.Hours);

            if (delta < (48 * 60 * 60))
                return GetSharedResource("[RESX:TimeSpan:DayAgo]");

            if (delta < (72 * 60 * 60))
                return string.Format(GetSharedResource("[RESX:TimeSpan:DaysAgo]"), ts.Days);

            if (delta < Convert.ToDouble(new TimeSpan(24 * 32, 0, 0).TotalSeconds))
                return GetSharedResource("[RESX:TimeSpan:MonthAgo]");

            if (delta < Convert.ToDouble(new TimeSpan(((24 * 30) * 11), 0, 0).TotalSeconds))
                return string.Format(GetSharedResource("[RESX:TimeSpan:MonthsAgo]"), Math.Ceiling(ts.Days / 30.0));

            if (delta < Convert.ToDouble(new TimeSpan(((24 * 30) * 18), 0, 0).TotalSeconds))
                return GetSharedResource("[RESX:TimeSpan:YearAgo]");

            return string.Format(GetSharedResource("[RESX:TimeSpan:YearsAgo]"), Math.Ceiling(ts.Days / 365.0));

        }

        internal static string ParseTokenConfig(int moduleId, string template, string group, ControlsConfig config)
        {
            if (string.IsNullOrEmpty(template))
                return string.Empty;

            if (!(template.Contains(Globals.ControlRegisterTag)))
                template = Globals.ControlRegisterTag + template;

            template = ParseSpacer(template);

            var li = DotNetNuke.Modules.ActiveForums.Controllers.TokenController.TokensList(moduleId,group);
            if (li != null)
                template = li.Aggregate(template, (current, tk) => current.Replace(tk.TokenTag, tk.TokenReplace));

            template = template.Replace("[PARAMKEYS:GROUPID]", ParamKeys.GroupId);
            template = template.Replace("[PARAMKEYS:FORUMID]", ParamKeys.ForumId);
            template = template.Replace("[PARAMKEYS:TOPICID]", ParamKeys.TopicId);
            template = template.Replace("[PARAMKEYS:VIEWTYPE]", ParamKeys.ViewType);
            template = template.Replace("[PARAMKEYS:QUOTEID]", ParamKeys.QuoteId);
            template = template.Replace("[PARAMKEYS:REPLYID]", ParamKeys.ReplyId);
            template = template.Replace("[VIEWS:TOPICS]", Views.Topics);
            template = template.Replace("[VIEWS:TOPIC]", Views.Topic);
            template = template.Replace("[PAGEID]", config.PageId.ToString());
            template = template.Replace("[SITEID]", config.PortalId.ToString());
            template = template.Replace("[INSTANCEID]", config.ModuleId.ToString());
            template = template.Replace("[PORTALID]", config.PortalId.ToString());
            template = template.Replace("[MODULEID]", config.ModuleId.ToString());

            return template;
        }

        internal static string ParseSecurityTokens(string template, string userRoles)
        {
            const string pattern = @"(\[AF:SECURITY:(.+?):(.+?)\])(.|\n)*?(\[/AF:SECURITY:(.+?):(.+?)\])";

            var sKey = string.Empty;
            var sReplace = string.Empty;

            var regExp = new Regex(pattern);
            var matches = regExp.Matches(template);
            foreach (Match match in matches)
            {
                var sRoles = match.Groups[3].Value;
                if (Permissions.HasAccess(sRoles, userRoles))
                {
                    template = template.Replace(match.Groups[1].Value, string.Empty);
                    template = template.Replace(match.Groups[5].Value, string.Empty);
                }
                else
                {
                    template = template.Replace(match.Value, string.Empty);
                }
            }
            return template;
        }

        internal static string GetTemplate(string filePath)
        {
            var sPath = filePath;

            if (!(sPath.Contains(@"\\")) && !(sPath.Contains(@":\")))
                sPath = DotNetNuke.Modules.ActiveForums.Utilities.MapPath(filePath);

            var sContents = string.Empty;
            if (File.Exists(sPath))
            {
                try
                {
                    var objStreamReader = File.OpenText(sPath);
                    sContents = objStreamReader.ReadToEnd();
                    objStreamReader.Close();
                }
                catch (Exception exc)
                {
                    sContents = exc.Message;
                }
            }

            return sContents;
        }
        internal static string BuildToolbar(int forumModuleId, int forumTabId, int moduleId, int tabId, CurrentUserTypes currentUserType)
        {
            string sToolbar =
                Convert.ToString(
                    DataCache.SettingsCacheRetrieve(forumModuleId, string.Format(CacheKeys.Toolbar, forumModuleId)));
            if (string.IsNullOrEmpty(sToolbar))
            {

                string templateFilePathFileName =
                    DotNetNuke.Modules.ActiveForums.Utilities.MapPath(path: SettingsBase.GetModuleSettings(forumModuleId).TemplatePath + "ToolBar.txt");
                if (!System.IO.File.Exists(templateFilePathFileName))
                {
                    templateFilePathFileName = DotNetNuke.Modules.ActiveForums.Utilities.MapPath(Globals.TemplatesPath + "ToolBar.txt");
                    if (!System.IO.File.Exists(templateFilePathFileName))
                    {
                        templateFilePathFileName =
                            DotNetNuke.Modules.ActiveForums.Utilities.MapPath(Globals.DefaultTemplatePath + "ToolBar.txt");
                    }
                }
                sToolbar = Utilities.GetFileContent(templateFilePathFileName);
                sToolbar = sToolbar.Replace("[TRESX:", "[RESX:");
                sToolbar = Utilities.ParseToolBar(template: sToolbar, forumTabId: forumTabId, forumModuleId: forumModuleId, tabId: tabId, moduleId: moduleId, currentUserType: currentUserType);
                DataCache.SettingsCacheStore(ModuleId: forumModuleId, cacheKey: string.Format(CacheKeys.Toolbar, forumModuleId), sToolbar);
            }

            return sToolbar;
        }
        internal static string ParseToolBar(string template, int forumTabId, int forumModuleId, int tabId, int moduleId,
            CurrentUserTypes currentUserType, int forumId = 0)
        {
            var ctlUtils = new ControlUtils();

            if (HttpContext.Current != null && HttpContext.Current.Request.IsAuthenticated)
            {
                template = template.Replace("[AF:TB:NotRead]", string.Format("<a href=\"{0}\"><i class=\"fa fa-file fa-fw fa-grey\"></i>&nbsp;[RESX:NotRead]</a>", ctlUtils.BuildUrl(tabId, moduleId, string.Empty, string.Empty, -1, -1, -1, -1, "notread", 1, -1, -1)));
                template = template.Replace("[AF:TB:MyTopics]", string.Format("<a href=\"{0}\"><i class=\"fa fa-files-o fa-fw fa-grey\"></i>&nbsp;[RESX:MyTopics]</a>", ctlUtils.BuildUrl(tabId, moduleId, string.Empty, string.Empty, -1, -1, -1, -1, "mytopics", 1, -1, -1)));
                template = template.Replace("[AF:TB:MySettings]", string.Format("<a href=\"{0}\"><i class=\"fa fa-cog fa-fw fa-blue\"></i>&nbsp;[RESX:MySettings]</a>", ctlUtils.BuildUrl(tabId, moduleId, string.Empty, string.Empty, -1, -1, -1, -1, "afprofile", 1, -1, -1)));

                if (currentUserType == CurrentUserTypes.Admin || currentUserType == CurrentUserTypes.SuperUser)
                    template = template.Replace("[AF:TB:ControlPanel]", string.Format("<a href=\"{0}\"><i class=\"fa fa-bars fa-fw fa-blue\"></i>&nbsp;[RESX:ControlPanel]</a>", NavigateUrl(forumTabId, "EDIT", "mid=" + forumModuleId)));
                else
                    template = template.Replace("[AF:TB:ControlPanel]", string.Empty);

                if (currentUserType == CurrentUserTypes.ForumMod || currentUserType == CurrentUserTypes.SuperUser || currentUserType == CurrentUserTypes.Admin)
                    template = template.Replace("[AF:TB:ModList]", string.Format("<a href=\"{0}\"><i class=\"fa fa-wrench fa-fw fa-blue\"></i>&nbsp;[RESX:Moderate]</a>", NavigateUrl(tabId, "", ParamKeys.ViewType + "=modtopics")));
                else
                    template = template.Replace("[AF:TB:ModList]", string.Empty);
            }
            else
            {
                template = template.Replace("[AF:TB:NotRead]", string.Empty);
                template = template.Replace("[AF:TB:MyTopics]", string.Empty);
                template = template.Replace("[AF:TB:ModList]", string.Empty);
                template = template.Replace("[AF:TB:ControlPanel]", string.Empty);
            }

            template = template.Replace("[AF:TB:Unanswered]", string.Format("<a href=\"{0}\"><i class=\"fa fa-file fa-fw fa-blue\"></i>&nbsp;[RESX:Unanswered]</a>", ctlUtils.BuildUrl(tabId, moduleId, string.Empty, string.Empty, -1, -1, -1, -1, "unanswered", 1, -1, -1)));
            template = template.Replace("[AF:TB:ActiveTopics]", string.Format("<a href=\"{0}\"><i class=\"fa fa-fire fa-fw fa-grey\"></i>&nbsp;[RESX:ActiveTopics]</a>", ctlUtils.BuildUrl(tabId, moduleId, string.Empty, string.Empty, -1, -1, -1, -1, "activetopics", 1, -1, -1)));
            template = template.Replace("[AF:TB:Forums]", string.Format("<a href=\"{0}\"><i class=\"fa fa-comment fa-fw fa-blue\"></i>&nbsp;[RESX:FORUMS]</a>", NavigateUrl(tabId)));

            // Search popup
            var searchUrl = NavigateUrl(tabId, string.Empty, new[] { ParamKeys.ViewType + "=search", "f=" + forumId });
            var advancedSearchUrl = NavigateUrl(tabId, string.Empty, new[] { ParamKeys.ViewType + "=searchadvanced", "f=" + forumId });
            var searchText = forumId > 0 ? "[RESX:SearchSingleForum]" : "[RESX:SearchAllForums]";

            template = template.Replace("[AF:TB:Search]", string.Format(@"<span class='aftb-search' data-searchUrl='{0}'><span class='aftb-search-link'><span><i class='fa fa-search fa-fw fa-blue'></i>&nbsp;{2}</span><span class='ui-icon ui-icon-triangle-1-s'></span></span><span class='aftb-search-popup'><input type='text' placeholder='Search for...' maxlength='50'><button>[RESX:Search]</button><br /><a href='{1}'>[RESX:SearchAdvanced]</a><input type='radio' name='afsrt' value='0' checked='checked' />[RESX:SearchByTopics]<input type='radio' name='afsrt' value='1' />[RESX:SearchByPosts]</span></span>", HttpUtility.HtmlEncode(searchUrl), HttpUtility.HtmlEncode(advancedSearchUrl), searchText));

            // These are no longer used in 5.0
            template = template.Replace("[AF:TB:MyProfile]", string.Empty);
            template = template.Replace("[AF:TB:MySettings]", string.Empty);
            template = template.Replace("[AF:TB:MemberList]", string.Empty);

            return template;
        }

        public static string CleanStringForUrl(string text)
        {
            text = text.Trim();
            text = text.Replace(":", string.Empty);
            text = Regex.Replace(text, @"[^\w]", "-");
            text = Regex.Replace(text, @"([-]+)", "-");
            if (text.EndsWith("-"))
                text = text.Substring(0, text.Length - 1);

            return text;
        }
        internal static bool HasFloodIntervalPassed(int floodInterval, User user, Forum forumInfo)
        {
            /* flood interval check passes if
            1) flood interval <= 0 (disabled)
            2) user is unauthenticated; if not, captcha is enabled for anonymous users
            3) user is an admin or superuser
            4) user is designated as a trusted user for the forum
            5) user has moderator (edit, delete, approve) permissions for the forum
            6) time span for since user's last post or reply exceeds flood interval
            */
            return floodInterval <= 0
                   || user == null
                   || user.IsAdmin
                   || user.IsSuperUser
                   || Utilities.IsTrusted((int)forumInfo.DefaultTrustValue, userTrustLevel: user.TrustLevel, Permissions.HasPerm(forumInfo.Security.Trust, user.UserRoles), forumInfo.AutoTrustLevel, user.PostCount)
                   || Permissions.HasPerm(forumInfo.Security.ModApprove, user.UserRoles)
                   || Permissions.HasPerm(forumInfo.Security.ModEdit, user.UserRoles)
                   || Permissions.HasPerm(forumInfo.Security.ModDelete, user.UserRoles)
                   || SimulateDateDiff.DateDiff(SimulateDateDiff.DateInterval.Second, user.Profile.DateLastPost, DateTime.UtcNow) > floodInterval
                   || SimulateDateDiff.DateDiff(SimulateDateDiff.DateInterval.Second, user.Profile.DateLastReply, DateTime.UtcNow) > floodInterval;
        }
        public static bool IsTrusted(int forumTrustLevel, int userTrustLevel, bool isTrustedRole, int autoTrustLevel = 0, int userPostCount = 0)
        {
            // Never trust users with trust level -1 (This overrides everything)
            if (userTrustLevel == -1)
                return false;

            // Always trust users with trust level 1 or in a trusted role or the forum trusts by default
            if (userTrustLevel == 1 || isTrustedRole || forumTrustLevel > 0)
                return true;

            // Check to see if the user should be trusted based on post count settings
            if (autoTrustLevel > 0 && userPostCount >= autoTrustLevel)
                return true;

            // If we get this far, the user must not be trusted.
            return false;
        }

        public static DateTime NullDate()
        {
            var nfi = new CultureInfo("en-US", false).DateTimeFormat;
            return DateTime.Parse("1/1/1900", nfi).ToUniversalTime();
        }
        public static DotNetNuke.Entities.Portals.PortalSettings GetPortalSettings()
        {
            try
            {
                if (HttpContext.Current?.Items["PortalSettings"] != null)
                {
                    return (DotNetNuke.Entities.Portals.PortalSettings)(HttpContext.Current.Items["PortalSettings"]);
                }
                else
                {
                    return ServiceLocator<IPortalController, PortalController>.Instance.GetCurrentPortalSettings();
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex); 
                return null; 
            }
        }
        public static DotNetNuke.Entities.Portals.PortalSettings GetPortalSettings(int portalId)
        {
            try
            {
                PortalSettings portalSettings = null;
                if (HttpContext.Current?.Items["PortalSettings"] != null)
                {
                    portalSettings.PortalAlias = DotNetNuke.Entities.Portals.PortalAliasController.Instance.GetPortalAliasesByPortalId(portalId).FirstOrDefault();
                    if (portalSettings.PortalId != portalId)
                    {
                        portalSettings = null;
                    }
                }
                if (portalSettings == null)
                {
                    portalSettings = new PortalSettings(portalId);
                    PortalSettingsController psc = new DotNetNuke.Entities.Portals.PortalSettingsController();
                    psc.LoadPortalSettings(portalSettings);
                }
                return portalSettings;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return null;
            }
        }
       public static string GetHost()
        {
            string strHost;
            if (HttpContext.Current.Request.IsSecureConnection)
            {
                strHost = (string.Concat(Common.Globals.AddHTTP(Common.Globals.GetDomainName(HttpContext.Current.Request)), "/")).Replace("http://", "https://");
            }
            else
            {
                strHost = string.Concat(Common.Globals.AddHTTP(Common.Globals.GetDomainName(HttpContext.Current.Request)), "/");
            }
            return strHost.ToLowerInvariant();
        }

        public static string NavigateUrl(int tabId)
        {
            return Common.Globals.NavigateURL(tabId);
        }

        public static string NavigateUrl(int tabId, int portalId, string controlKey, params string[] additionalParameters)
        {
            return NavigateUrl(tabId, controlKey, string.Empty, portalId, additionalParameters);
        }
        public static string NavigateUrl(int tabId, string controlKey, params string[] additionalParameters)
        {
            int portalId = DotNetNuke.Entities.Tabs.TabController.Instance.GetTab(tabId, DotNetNuke.Common.Utilities.Null.NullInteger).PortalID;
            return NavigateUrl(tabId, controlKey, string.Empty, portalId, additionalParameters);
        }
        public static string NavigateUrl(int tabId, string controlKey, List<string> additionalParameters)
        {
            string[] parameters = new string[additionalParameters.Count];
            for (int i = 0; i < additionalParameters.Count; i++)
            {
                parameters[i] = additionalParameters[i];
            }
            return NavigateUrl(tabId, controlKey, parameters);
        }

        public static string NavigateUrl(int tabId, string controlKey, string pageName, int portalId, params string[] additionalParameters)
        {
            var currParams = additionalParameters.ToList(); 
            string s = Common.Globals.NavigateURL(tabId, controlKey, currParams.ToArray());
            if (portalId == -1 || string.IsNullOrWhiteSpace(pageName))
                return s;

            var tc = new TabController();
            var ti = tc.GetTab(tabId, portalId, false);
            var sURL = currParams.Aggregate(Common.Globals.ApplicationURL(tabId), (current, p) => current + ("&" + p));

            pageName = CleanStringForUrl(pageName);
            PortalSettings portalSettings = DotNetNuke.Modules.ActiveForums.Utilities.GetPortalSettings(portalId);
            s = Common.Globals.FriendlyUrl(ti, sURL, pageName, portalSettings);
            return s;
        }

        public static void BindEnum(DropDownList pDDL, Type enumType, string pColValue, bool addEmptyValue)
        {
            BindEnum(pDDL, enumType, pColValue, addEmptyValue, false, -1);
        }

        public static void BindEnum(DropDownList pDDL, Type enumType, string pColValue, bool addEmptyValue, bool localize, int excludeIndex)
        {
            pDDL.Items.Clear();

            var values = Enum.GetValues(enumType);

            if (addEmptyValue)
                pDDL.Items.Add(new ListItem(string.Empty, "-1"));

            for (var i = 0; i < values.Length; i++)
            {
                if (i == excludeIndex)
                    continue;

                var key = Convert.ToInt32(Enum.Parse(enumType, values.GetValue(i).ToString()));
                var text = Convert.ToString(Enum.Parse(enumType, values.GetValue(i).ToString()));
                if (localize)
                    text = string.Concat("[RESX:", text, "]");

                pDDL.Items.Add(new ListItem(text, key.ToString()));
            }

            if (pColValue != string.Empty)
                pDDL.SelectedValue = Enum.Parse(enumType, pColValue).ToString();
        }

        internal static string PrepareForEdit(int portalId, int moduleId, string themePath, string text, bool allowHTML, EditorTypes editorType)
        {
            if (!allowHTML)
            {
                text = text.Replace("&#91;", "[");
                text = text.Replace("&#93;", "]");
                text = text.Replace("<br>", System.Environment.NewLine);
                text = text.Replace("<br />", System.Environment.NewLine);
                text = text.Replace("<BR>", System.Environment.NewLine);
                text = RemoveFilterWords(portalId, moduleId, themePath, text);

                return text;
            }

            if (editorType == EditorTypes.TEXTBOX)
            {
                text = text.Replace("&#91;", "[");
                text = text.Replace("&#93;", "]");
                text = text.Replace("<br>", System.Environment.NewLine);
                text = text.Replace("<br />", System.Environment.NewLine);
                text = text.Replace("<BR>", System.Environment.NewLine);
                text = RemoveFilterWords(portalId, moduleId, themePath, text);

                return text;
            }

            return text;
        }

        private static string ReplaceLink(Match match, string currentSite, string text)
        {
            const int maxLengthAutoLinkLabel = 47;
            const string outSite = "<a href=\"{0}\" target=\"_blank\" rel=\"nofollow\">{1}</a>";
            const string inSite = "<a href=\"{0}\">{1}</a>";
            var url = match.Value;
            if (url.ToLowerInvariant().Contains("jpg") || url.ToLowerInvariant().Contains("gif") || url.ToLowerInvariant().Contains("png") || url.ToLowerInvariant().Contains("jpeg"))
                return url;

            //
            // Ignore it when there is a preceeding a or img.
            //
            var xStart = 0;
            if ((match.Index - 10) > 0)
                xStart = match.Index - 10;

            if (text.Substring(xStart, 10).ToLowerInvariant().Contains("href"))
                return url;

            if (text.Substring(xStart, 10).ToLowerInvariant().Contains("src"))
                return url;

            if (text.Substring(xStart, 10).ToLowerInvariant().Contains("="))
                return url;

            var urlText = match.Value;
            if (urlText.Length > maxLengthAutoLinkLabel)
                urlText = string.Concat(match.Value.Substring(0, maxLengthAutoLinkLabel - 22), "...", match.Value.Substring(match.Value.Length - 20));

            return url.ToLowerInvariant().Contains(currentSite.ToLowerInvariant()) ? string.Format(inSite, url, urlText) : string.Format(outSite, url, urlText);
        }

        public static string AutoLinks(string text, string currentSite)
        {
            var original = text;
            if (!(string.IsNullOrEmpty(text)))
            {
                const string encodedHref = "&lt;a.*?href=[\"'](?<url>.*?)[\"'].*?&gt;(http[s]?.*?)&lt;/a&gt;"; // Encoded href regex

                // Replace encoded url with decoded url
                foreach (Match m in Regex.Matches(text, encodedHref, RegexOptions.IgnoreCase))
                    text = text.Replace(m.Value, HttpUtility.HtmlDecode(m.Value));

                const string regHref = "<a.*?href=[\"'](?<url>.*?)[\"'].*?>(?<http>http[s]?.*?)</a>";

                // Remove all exiting <A> anchors, so they will be treated by the ReplaceLink function. (adding target=_blank & nofollow)
                foreach (Match m in Regex.Matches(text, regHref, RegexOptions.IgnoreCase))
                    text = text.Replace(m.Value, m.Groups["http"].Value.Contains("...") ? m.Groups["url"].Value : m.Groups["http"].Value);

                // Handle Empty string
                if (string.IsNullOrEmpty(text))
                {
                    return original;
                }

                // Look for http(s) URLs  that are not perceded by a quote or <a>.
                String strRegexUrl = @"(?<!['""]+|<a.*?>\s*)http[s]?://([\w+?\.\w+])+([a-zA-Z0-9\~\!\@\\#\$\%\^\&amp;\*\(\)_\-\=\+\\\/\?\.\:\;\'\,]*)?";

                // Create auto link
                text = Regex.Replace(text, strRegexUrl, m => ReplaceLink(m, currentSite, text), RegexOptions.IgnoreCase);

                if (string.IsNullOrEmpty(text))
                {
                    return original;
                }
            }
            return text;
        }

        public static string CleanString(int portalId, string text, bool allowHTML, EditorTypes editorType, bool useFilter, bool allowScript, int moduleId, string themePath, bool processEmoticons)
        {

            var sClean = text;

            // If HTML is not allowed or if this comes from the TextBox editor (quick reply), the HTML needs to be encoded.
            if (sClean != string.Empty)
            {

                sClean = editorType == EditorTypes.TEXTBOX ? CleanTextBox(portalId, sClean, allowHTML, useFilter, moduleId, themePath, processEmoticons) : CleanEditor(portalId, sClean, useFilter, moduleId, themePath, processEmoticons);

                var regExp = new Regex(@"(<a [^>]*>)(?'url'(\S*?))(</a>)", RegexOptions.IgnoreCase);
                var matches = regExp.Matches(sClean);
                foreach (Match match in matches)
                {
                    var sNewURL = match.Groups[0].Value;
                    var sStart = match.Groups[1].Value;
                    var sText = match.Groups[2].Value;
                    var sEnd = match.Groups[3].Value;

                    if (sText.Length > 55)
                        sClean = sClean.Replace(sNewURL, sStart + sText.Substring(0, 35) + "..." + sText.Substring(sText.Length - 10) + sEnd);
                }

                if (!allowScript)
                {
                    sClean = sClean.Replace("&#91;", "[");
                    sClean = sClean.Replace("&#93;", "]");
                    sClean = XSSFilter(sClean);
                }

                sClean = sClean.Replace("[", "&#91;");
                sClean = sClean.Replace("]", "&#93;");
            }

            return sClean;
        }

        private static string CleanTextBox(int portalId, string text, bool allowHTML, bool useFilter, int moduleId, string themePath, bool processEmoticons)
        {

            var strMessage = HTMLEncode(text);

            if (strMessage != string.Empty)
            {
                if (strMessage.ToUpper().Contains("[CODE]") | strMessage.ToUpper().Contains("<CODE"))
                {
                    var codes = new List<string>();
                    var i = 0;
                    var objRegEx = new Regex(@"(\[CODE\](.*?)\[\/CODE\])", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    var matches = objRegEx.Matches(strMessage);
                    foreach (Match m in matches)
                    {
                        strMessage = strMessage.Replace(m.Value, string.Concat("[CODEHOLDER", i, "]"));
                        codes.Add(m.Value);
                        i += 1;
                    }

                    strMessage = Regex.Replace(strMessage, GetCaseInsensitiveSearch("<form"), "&lt;form&gt;");
                    strMessage = Regex.Replace(strMessage, GetCaseInsensitiveSearch("</form>"), "&lt;/form&gt;");
                    if (useFilter)
                        strMessage = FilterWords(portalId, moduleId, themePath, strMessage, processEmoticons);

                    strMessage = HTMLEncode(strMessage);
                    strMessage = Regex.Replace(strMessage, System.Environment.NewLine, " <br /> ");

                    i = 0;
                    foreach (var s in codes)
                    {
                        strMessage = strMessage.Replace(string.Concat("[CODEHOLDER", i, "]"), HttpUtility.HtmlEncode(s));
                        i += 1;
                    }
                }
                else
                {
                    strMessage = Regex.Replace(strMessage, GetCaseInsensitiveSearch("<form"), "&lt;form&gt;");
                    strMessage = Regex.Replace(strMessage, GetCaseInsensitiveSearch("</form>"), "&lt;/form&gt;");
                    if (!allowHTML)
                        strMessage = HTMLEncode(strMessage);

                    if (useFilter)
                        strMessage = FilterWords(portalId, moduleId, themePath, strMessage, processEmoticons);

                    strMessage = Regex.Replace(strMessage, System.Environment.NewLine, " <br /> ");
                }
                strMessage = strMessage.Replace("[", "&#91;");
                strMessage = strMessage.Replace("]", "&#93;");
            }

            return strMessage;
        }

        private static string CleanEditor(int portalId, string text, bool useFilter, int moduleId, string themePath, bool processEmoticons)
        {

            var strMessage = text;

            if ((strMessage.ToUpper().IndexOf("<CODE", StringComparison.Ordinal)) >= 0)
            {
                var intStart = strMessage.ToUpper().IndexOf("<CODE", StringComparison.Ordinal);
                var intEnd = strMessage.ToUpper().IndexOf("</CODE>", StringComparison.Ordinal) + 7;
                var sCode = strMessage.Substring(intStart, intEnd - intStart);
                strMessage = strMessage.Replace(sCode, "[CODEHOLDER]");
                if (useFilter)
                    strMessage = FilterWords(portalId, moduleId, themePath, strMessage, processEmoticons);

                strMessage = Regex.Replace(strMessage, GetCaseInsensitiveSearch("<form"), "&lt;form&gt;");
                strMessage = Regex.Replace(strMessage, GetCaseInsensitiveSearch("</form>"), "&lt;/form&gt;");

                strMessage = strMessage.Replace("[CODEHOLDER]", sCode);
            }
            else
            {
                if (useFilter)
                    strMessage = FilterWords(portalId, moduleId, themePath, strMessage, processEmoticons);

                strMessage = Regex.Replace(strMessage, GetCaseInsensitiveSearch("<form"), "&lt;form&gt;");
                strMessage = Regex.Replace(strMessage, GetCaseInsensitiveSearch("</form>"), "&lt;/form&gt;");
            }
            return strMessage;
        }

        public static string GetCaseInsensitiveSearch(string strSearch)
        {
            var strReturn = string.Empty;
            foreach (var chrCurrent in strSearch)
            {
                var chrLower = char.ToLower(chrCurrent);
                var chrUpper = char.ToUpper(chrCurrent);
                if (chrUpper == chrLower)
                    strReturn = strReturn + chrCurrent;
                else
                    strReturn = string.Concat(strReturn, "[", chrLower, chrUpper, "]");
            }

            return strReturn;
        }

        public static string FilterWords(int portalId, int moduleId, string themePath, string strMessage, bool processEmoticons, bool removeHTML = false)
        {
            if (removeHTML)
            {
                var newSubject = StripHTMLTag(strMessage);
                if (newSubject == string.Empty)
                {
                    newSubject = strMessage.Replace("<", string.Empty);
                    newSubject = newSubject.Replace(">", string.Empty);
                }
                strMessage = newSubject;
            }

            var dr = DataProvider.Instance().Filters_List(portalId, moduleId, 0, 100000, "ASC", "FilterId");
            dr.NextResult();

            while (dr.Read())
            {
                var sReplace = dr["Replace"].ToString();
                var sFind = dr["Find"].ToString();
                switch (dr["FilterType"].ToString().ToUpper())
                {
                    case "MARKUP":
                        strMessage = strMessage.Replace(sFind, sReplace.Trim());
                        break;

                    case "EMOTICON":
                        if (processEmoticons)
                        {
                            if (sReplace.IndexOf("/emoticons", StringComparison.Ordinal) >= 0)
                                sReplace = string.Format("<img src='{0}{1}' align=\"absmiddle\" border=\"0\" class=\"afEmoticon\" />", themePath, sReplace);

                            strMessage = strMessage.Replace(sFind, sReplace);
                        }
                        break;

                    case "REGEX":
                        strMessage = Regex.Replace(strMessage, sFind.Trim(), sReplace, RegexOptions.IgnoreCase);
                        break;
                }

            }
            dr.Close();

            return strMessage;
        }

        public static string RemoveFilterWords(int portalId, int moduleId, string themePath, string strMessage)
        {
            var dr = DataProvider.Instance().Filters_List(portalId, moduleId, 0, 100000, "ASC", "FilterId");
            dr.NextResult();

            while (dr.Read())
            {
                var sReplace = dr["Replace"].ToString();
                var sFind = dr["Find"].ToString();
                switch (dr["FilterType"].ToString().ToUpper())
                {
                    case "MARKUP":
                        strMessage = strMessage.Replace(sReplace, sFind.Trim());
                        break;

                    case "EMOTICON":
                        if (sReplace.IndexOf("/emoticons", StringComparison.Ordinal) >= 0)
                        {
                            sReplace = string.Format("<img src='{0}{1}' align=\"absmiddle\"  border=\"0\"  class=\"afEmoticon\" />", themePath, sReplace);
                            strMessage = strMessage.Replace(sReplace, sFind);
                        }
                        break;
                }

            }
            dr.Close();

            strMessage = ManageImagePath(strMessage);

            return strMessage;
        }

        public static string ImportFilter(int portalID, int moduleID)
        {
            string @out;
            try
            {
                var myFile = DotNetNuke.Modules.ActiveForums.Utilities.MapPath(string.Concat(Globals.DefaultTemplatePath, "/Filters.txt"));
                if (File.Exists(myFile))
                {
                    StreamReader objStreamReader;
                    try
                    {
                        objStreamReader = File.OpenText(myFile);
                    }
                    catch (Exception exc)
                    {
                        @out = exc.Message;
                        return @out;
                    }
                    var strFilter = objStreamReader.ReadLine();
                    while (strFilter != null)
                    {
                        var row = Regex.Split(strFilter, ",,");
                        var sFind = row[0].Substring(1, row[0].Length - 2);
                        var sReplace = row[1].Trim(' ');
                        sReplace = sReplace.Substring(1, sReplace.Length - 2);
                        var sType = row[2].Substring(1, row[2].Length - 2);
                        DataProvider.Instance().Filters_Save(portalID, moduleID, -1, sFind, sReplace, sType);
                        strFilter = objStreamReader.ReadLine();
                    }
                    objStreamReader.Close();
                    @out = "Success";
                }
                else
                {
                    @out = string.Concat("File Not Found<br />Path:", myFile);
                }
            }
            catch (Exception exc)
            {
                @out = exc.Message;
            }

            return @out;
        }

        public static bool InputIsValid(string body)
        {
            if (string.IsNullOrEmpty(body))
                return false;

            body = body.Trim();

            if (string.IsNullOrEmpty(StripHTMLTag(body)) && !(body.ToUpper().Contains("<CODE")))
                return false;

            return !string.IsNullOrEmpty(body.Replace("&nbsp;", string.Empty));
        }

        public static string HTMLEncode(string strMessage = "")
        {
            if (strMessage != string.Empty)
            {
                strMessage = strMessage.Replace(">", "&gt;");
                strMessage = strMessage.Replace("<", "&lt;");
            }

            return strMessage;
        }

        public static string ParsePre(string strMessage)
        {
            var objRegEx = new Regex("<pre>(.*?)</pre>");
            strMessage = "<code>" + HTMLDecode(objRegEx.Replace(strMessage, "$1")) + "</code>";
            return strMessage;
        }

        public static string HTMLDecode(string strMessage)
        {
            strMessage = strMessage.Replace("&gt;", ">");
            strMessage = strMessage.Replace("&lt;", "<");
            return strMessage;
        }

        public static string StripHTMLTag(string sText)
        {
            if (string.IsNullOrEmpty(sText))
                return string.Empty;

            const string pattern = @"<(.|\n)*?>";
            return Regex.Replace(sText, pattern, string.Empty).Trim();
        }

        public static bool HasHTML(string sText)
        {
            if (string.IsNullOrEmpty(sText))
                return false;

            const string pattern = @"<(.|\n)*?>";
            return Regex.IsMatch(sText, pattern);
        }

        public static string StripTokens(string sText)
        {
            sText = sText.Replace("AF:DIR:", "AFHOLD:");
            const string pattern = @"(\[AF:.+?\])";
            sText = Regex.Replace(sText, pattern, string.Empty);
            sText = Regex.Replace(sText, @"(\[/AF:.+?\])", string.Empty);
            sText = Regex.Replace(sText, @"(\[RESX:.+?\])", string.Empty);
            sText = sText.Replace("[ATTACHMENTS]", string.Empty);
            sText = sText.Replace("[MODEDITDATE]", string.Empty);
            sText = sText.Replace("[SIGNATURE]", string.Empty);
            sText = sText.Replace("AFHOLD:", "AF:DIR:");

            return sText;
        }

        public static string XSSFilter(string sText = "", bool removeHTML = false)
        {
            const string pattern = "<style.*/*>|</style>|<script.*/*>|</script>|<[a-zA-Z][^>]*=[`'\"]+javascript:\\w+.*[`'\"]+>|<\\w+[^>]*\\son\\w+.*[ /]*>|<[a-zA-Z][^>].*=javascript:.*>|<\\w+[^>]*[\\x00-\\x20]*=[\\x00-\\x20]*[`'\"]*[\\x00-\\x20]*j[\\x00-\\x20]*a[\\x00-\\x20]*v[\\x00-\\x20]*a[\\x00-\\x20]*s[\\x00-\\x20]*c[\\x00-\\x20]*r[\\x00-\\x20]*i[\\x00-\\x20]*p[\\x00-\\x20]*t[\\x00-\\x20]*(.|\\n)*?";
            foreach (Match m in Regex.Matches(sText, pattern, RegexOptions.IgnoreCase))
                sText = sText.Replace(m.Value, StrongEncode(m.Value));

            if (removeHTML)
                sText = StripHTMLTag(sText);

            return sText;
        }

        public static string StripExecCode(string sText)
        {
            var i = 0;
            while (i < sText.Length)
            {
                var aspTag = new System.Web.RegularExpressions.TagRegex();
                var m = aspTag.Match(sText, i);
                if (m.Success)
                {
                    i = m.Index + m.Length;
                    var tag = m.Value;
                    var aspRunAt = new System.Web.RegularExpressions.RunatServerRegex();
                    var mInner = aspRunAt.Match(m.Value, 0);
                    if (mInner.Success)
                    {
                        sText = sText.Replace(tag, HttpUtility.HtmlEncode(m.Value));
                        var endTag = new System.Web.RegularExpressions.EndTagRegex();
                        m = endTag.Match(sText, i);
                        if (m.Success)
                        {
                            i = m.Index + m.Length;
                            sText = sText.Replace(m.Value, HttpUtility.HtmlEncode(m.Value));
                        }
                    }

                    continue;
                }

                i += 1;
            }

            foreach (var m in Regex.Matches(sText, "<%(?!@)(?<code>.*?)%>", RegexOptions.IgnoreCase))
            {
                sText = sText.Replace(m.ToString(), HttpUtility.HtmlEncode(m.ToString().Replace("<br />", System.Environment.NewLine)));
            }

            foreach (var m in Regex.Matches(sText, "<!--\\s*#(?i:include)\\s*(?<pathtype>[\\w]+)\\s*=\\s*[\"']?(?<filename>[^\\\"']*?)[\"']?\\s*-->", RegexOptions.IgnoreCase))
            {
                sText = sText.Replace(m.ToString(), StrongEncode(m.ToString()));
            }

            sText = sText.Replace("<!--", "&lt;&#33;&#45;&#45;");
            sText = sText.Replace("-->", "&#45;&#45;&gt;");

            return sText;
        }

        public static string StrongEncode(string text)
        {
            return text.ToCharArray().Aggregate(string.Empty, (current, s) => current + ("&#" + Convert.ToInt32(s) + ";"));
        }

        public static string StrongDecode(string text)
        {
            var @out = string.Empty;
            foreach (Match m in Regex.Matches(text, "&#[a-zA-Z0-9];"))
            {
                var scode = m.Value.Replace("&#", string.Empty).Replace(";", string.Empty);
                text = text.Replace(m.Value, scode);
            }
            return text;
        }

        public static string CheckSqlString(string input)
        {
            input = input.Replace("\\", string.Empty);
            input = input.Replace("[", string.Empty);
            input = input.Replace("]", string.Empty);
            input = input.Replace("(", string.Empty);
            input = input.Replace(")", string.Empty);
            input = input.Replace("{", string.Empty);
            input = input.Replace("}", string.Empty);
            input = input.Replace("'", "''");
            input = input.Replace("UNION", string.Empty);
            input = input.Replace("TABLE", string.Empty);
            input = input.Replace("WHERE", string.Empty);
            input = input.Replace("DROP", string.Empty);
            input = input.Replace("EXECUTE", string.Empty);
            input = input.Replace("EXEC ", string.Empty);
            input = input.Replace("FROM ", string.Empty);
            input = input.Replace("CMD ", string.Empty);
            input = input.Replace(";", string.Empty);
            input = input.Replace("--", string.Empty);

            return input;
        }

        internal static string CleanName(string name)
        {
            var currentName = name;
            if (name == "-1")
                return "-1";

            if (string.IsNullOrEmpty(name))
                return string.Empty;

            name = name.Trim();
            var chars = "_$%#@!*?;:~`+=()[]{}|\\'<>,/^&\".".ToCharArray();
            name = name.Replace(". ", ".");
            name = name.Replace(" .", ".");
            name = name.Replace("- ", "-");
            name = name.Replace(" -", "-");
            name = name.Replace(".", "-");
            name = name.Replace(" ", "-");
            for (var i = 0; i < chars.Length; i++)
            {
                var strChar = chars.GetValue(i).ToString();
                if (name.Contains(strChar))
                    name = name.Replace(strChar, string.Empty);
            }
            name = name.Replace("--", "-");
            name = name.Replace("---", "-");
            name = name.Replace("----", "-");
            name = name.Replace("-----", "-");
            name = name.Replace("----", "-");
            name = name.Replace("---", "-");
            name = name.Replace("--", "-");
            name = name.Trim('-');

            return name.Length > 0 ? name : currentName;
        }
        internal static bool IsRewriteLoaded()
        {
            return ConfigUtils.IsRewriterInstalled(System.Web.Hosting.HostingEnvironment.MapPath("~/web.config"));
        }
        internal static bool UseFriendlyURLs(int ModuleId)
        {
            return IsRewriteLoaded() && SettingsBase.GetModuleSettings(ModuleId).URLRewriteEnabled;
        }

        /// <summary>
        /// Get the template as a string from the specified path
        /// </summary>
        /// <param name="filePath">Physical path to file</param>
        /// <returns>String</returns>
        /// <remarks></remarks>
        internal static string GetFile(string filePath)
        {
            var sContents = string.Empty;
            if (File.Exists(filePath))
            {
                try
                {
                    using (var sr = new StreamReader(filePath))
                    {
                        sContents = sr.ReadToEnd();
                        sr.Close();
                    }
                }
                catch (Exception exc)
                {
                    sContents = exc.Message;
                }
            }
            return sContents;
        }
        internal static string MapPath(string path)
        {
            try
            {
                /* handle situations where method is called without an HttpContext */
                return (HttpContext.Current != null) ? HttpContext.Current.Server.MapPath(path) : System.Web.Hosting.HostingEnvironment.MapPath(path);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return path;
            }
        }





       
        
        
        public static string ManageImagePath(string sHTML, Uri hostUri)
        {
            string hostWithScheme = hostUri.AbsoluteUri.Replace(hostUri.PathAndQuery, string.Empty).ToLowerInvariant();

            var iStart = sHTML.IndexOf("src='/", StringComparison.Ordinal);
            while (iStart != -1)
            {
                sHTML = sHTML.Insert(iStart + 5, hostWithScheme);
                iStart = sHTML.IndexOf("src='/", StringComparison.Ordinal);
            }

            iStart = sHTML.IndexOf("src=\"/", StringComparison.Ordinal);
            while (iStart != -1)
            {
                sHTML = sHTML.Insert(iStart + 5, hostWithScheme);
                iStart = sHTML.IndexOf("src=\"/", StringComparison.Ordinal);
            }

            return sHTML;
        }
		internal static string GetFileContent(string filePath)
        {
            var sPath = filePath;
            if (!(sPath.Contains(@":\")) && !(sPath.Contains(@"\\")))
                sPath = DotNetNuke.Modules.ActiveForums.Utilities.MapPath(sPath);

            var sContents = string.Empty;
            if (File.Exists(sPath))
            {
                try
                {
                    var objStreamReader = File.OpenText(sPath);
                    sContents = objStreamReader.ReadToEnd();
                    objStreamReader.Close();
                }
                catch (Exception exc)
                {
                    sContents = exc.Message;
                }
            }
            return sContents;
        }
        public static string GetDate(DateTime displayDate, int mid, int offset)
        {
            string dateStr;

            try
            {
                var mUserOffSet = 0;
                var mainSettings = SettingsBase.GetModuleSettings(mid);
                var mServerOffSet = mainSettings.TimeZoneOffset;
                var newDate = displayDate.AddMinutes(-mServerOffSet);

                newDate = newDate.AddMinutes(offset);

                var dateFormat = mainSettings.DateFormatString;
                var timeFormat = mainSettings.TimeFormatString;
                var formatString = string.Concat(dateFormat, " ", timeFormat);

                try
                {
                    dateStr = newDate.ToString(formatString);
                }
                catch
                {
                    dateStr = displayDate.ToString();
                }

                return dateStr;
            }
            catch (Exception ex)
            {
                dateStr = displayDate.ToString();
                return dateStr;
            }
        }

        public static DateTime GetUserDate(DateTime displayDate, int mid, int offset)
        {
            var mainSettings = SettingsBase.GetModuleSettings(mid);
            var mServerOffSet = mainSettings.TimeZoneOffset;
            var newDate = displayDate.AddMinutes(-mServerOffSet);

            return newDate.AddMinutes(offset);
        }
        
        public string GetUserFormattedDate(DateTime date, PortalInfo portalInfo, UserInfo userInfo)
        {
            return GetUserFormattedDateTime(date, portalInfo.PortalID, userInfo.UserID);
        }
        
        public static string GetUserFormattedDateTime(DateTime dateTime, int portalId, int userId, string format)
        {
            CultureInfo userCultureInfo = GetCultureInfoForUser(portalId, userId);
            TimeZoneInfo userTimeZoneInfo = GetTimeZoneInfoForUser(portalId, userId);
            return GetUserFormattedDateTime(dateTime, userCultureInfo, userTimeZoneInfo.BaseUtcOffset, format);
        }
        public static string GetUserFormattedDateTime(DateTime dateTime, int portalId, int userId)
        {
            CultureInfo userCultureInfo = GetCultureInfoForUser(portalId, userId);
            TimeZoneInfo userTimeZoneInfo = GetTimeZoneInfoForUser(portalId, userId);
            return GetUserFormattedDateTime(dateTime, userCultureInfo, userTimeZoneInfo.BaseUtcOffset);
        }
        public static string GetUserFormattedDateTime(DateTime dateTime, CultureInfo userCultureInfo, TimeSpan timeZoneOffset)
        {
            return GetUserFormattedDateTime(dateTime, userCultureInfo, timeZoneOffset, "g");
        }
        public static string GetUserFormattedDate(DateTime date, CultureInfo userCultureInfo, TimeSpan timeZoneOffset)
        {
            return GetUserFormattedDateTime(date, userCultureInfo, timeZoneOffset, "d");
        }
        public static string GetUserFormattedDate(DateTime date, CultureInfo userCultureInfo, TimeSpan timeZoneOffset, string format)
        {
            return GetUserFormattedDateTime(date, userCultureInfo, timeZoneOffset, format);
        }
        public static string GetUserFormattedDateTime(DateTime dateTime, CultureInfo userCultureInfo, TimeSpan timeZoneOffset, string format)
        {
            try
            {
                return dateTime.Add(timeZoneOffset).ToString(format, userCultureInfo);
            }
            catch (Exception ex)
            {
                return dateTime.ToString(format, CultureInfo.CurrentCulture);
            }
        }
        
        public static CultureInfo GetCultureInfoForUser(int portalId, int userId)
        {
            return GetCultureInfoForUser(DotNetNuke.Entities.Users.UserController.Instance.GetUser(portalId, userId));
        }
        public static CultureInfo GetCultureInfoForUser(UserInfo userInfo)
        {
            try
            {
                if (userInfo != null && userInfo.UserID > 0 && userInfo.Profile.PreferredLocale != null)
                {
                    return CultureInfo.GetCultureInfo(userInfo.Profile.PreferredLocale);
                }
                else
                {
                    return CultureInfo.GetCultureInfo(ServiceLocator<IPortalController, PortalController>.Instance.GetCurrentPortalSettings().CultureCode);
                }
            }
            catch
            {
                return CultureInfo.CurrentCulture;
            }
        }
        public static TimeZoneInfo GetTimeZoneInfoForUser(int portalId, int userId)
        {
            return GetTimeZoneInfoForUser(DotNetNuke.Entities.Users.UserController.Instance.GetUser(portalId, userId));
        }
        public static TimeZoneInfo GetTimeZoneInfoForUser(UserInfo userInfo)
        {
            /* AF now stores datetime in UTC, so this method returns timezoneoffset for current user if available or from portal settings as fallback */

            try
            {
                if (userInfo != null && userInfo.Profile != null && userInfo.Profile.PreferredTimeZone != null)
                {
                    return userInfo.Profile.PreferredTimeZone;
                }
                else
                {
                    return ServiceLocator<IPortalController, PortalController>.Instance.GetCurrentPortalSettings().TimeZone;
                }
            }
            catch
            {
                return TimeZoneInfo.Utc;
            }
        }
        public static TimeSpan GetTimeZoneOffsetForUser(UserInfo userInfo)
        {
            return GetTimeZoneInfoForUser(userInfo).BaseUtcOffset;
        }
        public static TimeSpan GetTimeZoneOffsetForUser(int PortalId, int UserId)
        {
            return GetTimeZoneOffsetForUser( new DotNetNuke.Entities.Users.UserController().GetUser(PortalId,UserId));
        }
        public static DateTime GetUserFormattedDate(DateTime displayDate, int mid, TimeSpan offset)
        {
            return displayDate.AddMinutes(offset.TotalMinutes);
        }
        
        public static string GetLastPostSubject(int lastPostID, int parentPostID, int forumID, int tabID, string subject, int length, int pageSize, int replyCount, bool canRead)
        {
            var sb = new StringBuilder();
            var postId = lastPostID;
            subject = StripHTMLTag(subject);
            subject = subject.Replace("[", "&#91");
            subject = subject.Replace("]", "&#93");
            if (lastPostID != 0)
            {
                if (subject.Length > length & length > 0)
                    subject = subject.Substring(0, length) + "...";

                if (parentPostID != 0)
                    lastPostID = parentPostID;

                if (replyCount > 1)
                {
                    var intPages = Convert.ToInt32(Math.Ceiling(replyCount / (double)pageSize));
                    if (canRead)
                    {
                        string[] Params = { ParamKeys.ForumId + "=" + forumID, ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.TopicId + "=" + lastPostID, ParamKeys.PageJumpId + "=" + intPages };
                        sb.AppendFormat("<a href=\"{0}#{1}\" rel=\"nofollow\">{2}</a>", Common.Globals.NavigateURL(tabID, string.Empty, Params), postId, HTMLEncode(subject));
                    }
                    else
                    {
                        sb.Append(HTMLEncode(subject));
                    }
                }
                else
                {
                    if (canRead)
                    {
                        string[] Params = { ParamKeys.ViewType + "=" + Views.Topic, ParamKeys.ForumId + "=" + forumID, ParamKeys.TopicId + "=" + lastPostID };
                        sb.AppendFormat("<a href=\"{0}#{1}\" rel=\"nofollow\">{2}</a>", Common.Globals.NavigateURL(tabID, string.Empty, Params), postId, HTMLEncode(subject));
                    }
                    else
                    {
                        sb.Append(HTMLEncode(subject));
                    }

                }
            }

            return sb.ToString();
        }

        public static string ParseSpacer(string template)
        {
            var spacerTemplate = string.Format("<img src=\"{0}\" alt=\"--\" width=\"$2\" height=\"$1\" />", System.Web.VirtualPathUtility.ToAbsolute(string.Concat(Globals.ModuleImagesPath, "spacer.gif")));

            const string expression = @"\[SPACER\:(\d+)\:(\d+)\]";

            return Regex.Replace(template, expression, spacerTemplate, RegexOptions.IgnoreCase);
        }

        internal static string GetSqlString(string sqlFile)
        {
            var resourceLocation = sqlFile;
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLocation);

            if (stream == null)
                return null;

            var sr = new StreamReader(stream);
            var contents = sr.ReadToEnd();
            sr.Close();
            return contents;
        }

        public static string LocalizeControl(string controlText)
        {
            return LocalizeControl(controlText, string.Empty, false, false);
        }

        public static string LocalizeControl(string controlText, string resourceFile)
        {
            return LocalizeControl(controlText, resourceFile, false, false);
        }

        public static string LocalizeControl(string controlText, bool isAdmin)
        {
            return LocalizeControl(controlText, string.Empty, isAdmin, false);
        }

        public static string LocalizeControl(string controlText, bool isAdmin, bool scriptSafe)
        {
            return LocalizeControl(controlText, string.Empty, isAdmin, scriptSafe);
        }

        private static string LocalizeControl(string controlText, string resourceFile, bool isAdmin, bool scriptSafe)
        {
            controlText = controlText.Replace(" class=afquote", " class=\"afquote\"");

            var i = 0;
            var intStart = 0;
            var intEnd = 0;
            const string pattern = @"(\[RESX:.+?\])";
            var regExp = new Regex(pattern);
            var matches = regExp.Matches(controlText);
            foreach (Match match in matches)
            {
                var sKey = match.Value;
                string sReplace;

                if (isAdmin)
                    sReplace = GetSharedResource(match.Value, true);
                else if (resourceFile != string.Empty)
                    sReplace = GetSharedResource(match.Value, resourceFile);
                else
                    sReplace = GetSharedResource(match.Value);

                var newValue = match.Value;
                if (!(string.IsNullOrEmpty(sReplace)))
                    newValue = sReplace;

                if (scriptSafe)
                {
                    newValue = HttpUtility.HtmlEncode(newValue).Replace("'", @"\'");
                    newValue = newValue.Replace("[", @"\[").Replace("]", @"\]");
                    newValue = JSON.EscapeJsonString(newValue);
                }

                controlText = controlText.Replace(sKey, newValue);
            }

            return controlText;
        }

        public static string GetSharedResource(string key, string resourceFile)
        {
            return DotNetNuke.Services.Localization.Localization.GetString(key, string.Concat(Globals.ModulePath, "App_LocalResources/", resourceFile, ".resx"));
        }

        public static string GetSharedResource(string key, bool isAdmin = false)
        {
            string sValue = DotNetNuke.Services.Localization.Localization.GetString(key, isAdmin ? Globals.ControlPanelResourceFile : Globals.SharedResourceFile);
            return string.IsNullOrEmpty(sValue) ? key : sValue;
        }

        public static string FormatFileSize(int fileSize)
        {
            try
            {
                if (fileSize >= 1073741824)
                    return (fileSize / 1024.0 / 1024.0 / 1024.0).ToString("#0.00") + " GB";

                if (fileSize >= 1048576)
                    return (fileSize / 1024.0 / 1024.0).ToString("#0.00") + " MB";

                if (fileSize >= 1024)
                    return (fileSize / 1024.0).ToString("#0.00") + " KB";

                if (fileSize < 1024)
                    return string.Concat(fileSize, " Bytes");
            }
            catch (Exception ex)
            {
                return "0 Bytes";
            }

            return "0 Bytes";
        }

        public static object ConvertFromHashTableToObject(Hashtable ht, object infoObject)
        {
            var myType = infoObject.GetType();
            var myProperties = myType.GetProperties((BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
            foreach (var pItem in myProperties)
            {
                var sValue = string.Empty;
                var sKey = pItem.Name.ToLower();
                foreach (string k in ht.Keys)
                {
                    if (k.ToLowerInvariant() != sKey.ToLowerInvariant())
                        continue;

                    sValue = ht[k].ToString();
                    break;
                }

                if (string.IsNullOrEmpty(sValue))
                    continue;

                object obj = null;
                switch (pItem.PropertyType.ToString())
                {
                    case "System.Int16":
                        obj = Convert.ToInt32(sValue);
                        break;
                    case "System.Int32":
                        obj = Convert.ToInt32(sValue);
                        break;
                    case "System.Int64":
                        obj = Convert.ToInt64(sValue);
                        break;
                    case "System.Single":
                        obj = Convert.ToSingle(sValue);
                        break;
                    case "System.Double":
                        obj = Convert.ToDouble(sValue);
                        break;
                    case "System.Decimal":
                        obj = Convert.ToDecimal(sValue);
                        break;
                    case "System.DateTime":
                        obj = Convert.ToDateTime(sValue);
                        break;
                    case "System.String":
                    case "System.Char":
                        obj = Convert.ToString(sValue);
                        break;
                    case "System.Boolean":
                        obj = Convert.ToBoolean(sValue);
                        break;
                    case "System.Guid":
                        obj = new Guid(sValue);

                        break;
                }
                if (obj != null)
                {
                    infoObject.GetType().GetProperty(pItem.Name).SetValue(infoObject, obj, BindingFlags.Public | BindingFlags.NonPublic, null, null, null);
                }
            }

            return infoObject;
        }

        internal static string GetFileResource(string resourceName)
        {
            string contents = null;
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (s != null)
                {
                    using (var sr = new StreamReader(s))
                    {
                        contents = sr.ReadToEnd();
                        sr.Close();
                    }
                    s.Close();
                }
            }
            return contents;
        }

        public static List<DotNetNuke.Entities.Users.UserInfo> GetListOfModerators(int portalId, int forumId)
        {
            var rp = RoleProvider.Instance();
            var uc = new DotNetNuke.Entities.Users.UserController();
            var fc = new ForumController();
            var fi = fc.Forums_Get(forumId, -1, false, true);
            if (fi == null)
                return null;

            var mods = new List<DotNetNuke.Entities.Users.UserInfo>();
            SubscriptionInfo si = null;
            var modApprove = fi.Security.ModApprove;
            var modRoles = modApprove.Split('|')[0].Split(';');

            foreach (var r in modRoles)
            {
                if (string.IsNullOrEmpty(r))
                    continue;

                var rid = Convert.ToInt32(r);
                var rName = DotNetNuke.Security.Roles.RoleController.Instance.GetRoleById(portalId, rid).RoleName;
                foreach (DotNetNuke.Entities.Users.UserRoleInfo usr in rp.GetUserRoles(portalId, null, rName))
                {
                    var ui = uc.GetUser(portalId, usr.UserID);

                    if (!(mods.Contains(ui)))
                    {
                        mods.Add(ui);
                    }
                }
            }

            return mods;
        }

        public static bool SafeConvertBool(object value, bool defaultValue = false)
        {
            if (value == null)
                return defaultValue;

            if (value is bool)
                return (bool)value;

            var s = value as string;
            if (s != null)
            {
                switch (s)
                {
                    case "0":
                        return false;
                    case "1":
                        return true;
                    default:
                        bool parsedValue;
                        return bool.TryParse(s, out parsedValue) ? parsedValue : defaultValue;
                }
            }

            try
            {
                return Convert.ToBoolean(value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static int SafeConvertInt(object value, int defaultValue = 0)
        {
            if (value == null)
                return defaultValue;

            if (value is int)
                return (int)value;

            var s = value as string;
            if (s != null)
            {
                int parsedValue;
                return int.TryParse(s, out parsedValue) ? parsedValue : defaultValue;
            }

            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static long SafeConvertLong(object value, long defaultValue = 0)
        {
            if (value == null)
                return defaultValue;

            if (value is long)
                return (long)value;

            var s = value as string;
            if (s != null)
            {
                long parsedValue;
                return long.TryParse(s, out parsedValue) ? parsedValue : defaultValue;
            }

            try
            {
                return Convert.ToInt64(value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static double SafeConvertDouble(object value, double defaultValue = 0.0)
        {
            if (value == null)
                return defaultValue;

            if (value is int)
                return (int)value;

            var s = value as string;
            if (s != null)
            {
                double parsedValue;
                return double.TryParse(s, out parsedValue) ? parsedValue : defaultValue;
            }

            try
            {
                return Convert.ToDouble(value);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static DateTime SafeConvertDateTime(object value, DateTime? defaultValue = null)
        {
            if (value == null)
                return defaultValue.HasValue ? defaultValue.Value : NullDate();

            if (value is DateTime)
                return (DateTime)value;

            var s = value as string;
            if (s != null)
            {
                DateTime parsedValue;

                if (DateTime.TryParse(s, DateTimeStringCultureInfo, DateTimeStyles.AssumeLocal, out parsedValue))
                    return parsedValue;
            }

            try
            {
                return Convert.ToDateTime(value);
            }
            catch (Exception)
            {
                return defaultValue.HasValue ? defaultValue.Value : NullDate();
            }
        }

        public static string SafeConvertString(object value, string defaultValue = null)
        {
            return value == null ? defaultValue : value.ToString();
        }

        public static string SafeTrim(string input)
        {
            return input == null ? null : input.Trim();
        }

        public static void SelectListItemByValue(ListControl dropDownList, object value)
        {
            if (dropDownList == null)
                return;

            dropDownList.ClearSelection();

            var selectedItem = dropDownList.Items.FindByValue(value == null ? string.Empty : value.ToString());

            if (selectedItem != null)
                selectedItem.Selected = true;
        }

        public static void SelectListItemByValue(ListControl dropDownList, object value, object defaultValue)
        {
            if (dropDownList == null)
                return;

            dropDownList.ClearSelection();

            var selectedItem = dropDownList.Items.FindByValue(value == null ? (defaultValue == null ? string.Empty : defaultValue.ToString()) : value.ToString());

            if (selectedItem != null)
                selectedItem.Selected = true;
        }
    }
}