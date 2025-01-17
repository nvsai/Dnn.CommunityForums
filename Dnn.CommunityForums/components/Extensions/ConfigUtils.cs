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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Xml;
namespace DotNetNuke.Modules.ActiveForums
{
	public static class ConfigUtils
    {
        private const string _web_config_system_webserver_modules_node_XPath = "//system.webServer/modules"; 
		private const string _ForumsRewriterTypeNameFull = "DotNetNuke.Modules.ActiveForums.ForumsReWriter, DotNetNuke.Modules.ActiveForums";
        private const string _ForumsRewriterTypeNameShort = "ForumsReWriter";
        private const string _attribute_name = "name";
        private const string _attribute_type = "type";
        private const string _attribute_precondition = "preCondition";
        private const string _attribute_runAllManagedModulesForAllRequests = "runAllManagedModulesForAllRequests";
        public static bool IsRewriterInstalled(string configPath)
        {
            bool isInstalled = false;
            try
            {
                XmlDocument xDoc = new XmlDocument();
                XmlReaderSettings readerSettings = new XmlReaderSettings
                {
                    IgnoreWhitespace = true,
                    IgnoreComments = true,
                    CloseInput = true
                };
                using (XmlReader reader = XmlReader.Create(configPath, readerSettings))
                {
                    xDoc.Load(reader);
                    reader.Close();
                }
                if (xDoc != null)
                {
                    System.Xml.XmlNode xRoot = xDoc.DocumentElement;
                    System.Xml.XmlNode xNode = xRoot.SelectSingleNode(_web_config_system_webserver_modules_node_XPath);
                    if (xNode != null)
                    {
                        foreach (XmlNode n in xNode.ChildNodes)
                        {
                            if ((n.Attributes[_attribute_name].Value == _ForumsRewriterTypeNameShort) && (n.Attributes[_attribute_type].Value == _ForumsRewriterTypeNameFull))
                            {
                                isInstalled = true;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            return isInstalled;

        }
        public static bool InstallRewriter(string configPath)
		{
			if (IsRewriterInstalled(configPath))
			{
				return true;
			}
			try
			{
				XmlDocument xDoc = new XmlDocument();
				xDoc.Load(configPath);
				if (xDoc != null)
				{
					System.Xml.XmlNode xRoot = xDoc.DocumentElement;
					System.Xml.XmlNode xNode = xRoot.SelectSingleNode(_web_config_system_webserver_modules_node_XPath);
					if (xNode != null)
					{
						if (xNode.Attributes[_attribute_runAllManagedModulesForAllRequests] == null)
						{
							XmlAttribute attribute_runAllManagedModulesForAllRequests = xDoc.CreateAttribute(_attribute_runAllManagedModulesForAllRequests);
                            attribute_runAllManagedModulesForAllRequests.Value = "true";
							xNode.Attributes.Append(attribute_runAllManagedModulesForAllRequests);
						}						
						XmlElement xNewNode = xDoc.CreateElement("add");
						XmlAttribute xAttrib = xDoc.CreateAttribute(_attribute_name);
						xAttrib.Value = _ForumsRewriterTypeNameShort;
						xNewNode.Attributes.Append(xAttrib);
						xAttrib = xDoc.CreateAttribute(_attribute_type);
						xAttrib.Value = _ForumsRewriterTypeNameFull;
						xNewNode.Attributes.Append(xAttrib);
						xAttrib = xDoc.CreateAttribute(_attribute_precondition);
						xAttrib.Value = "managedHandler";
						xNewNode.Attributes.Append(xAttrib);
						xNode.PrependChild(xNewNode);
						xDoc.Save(configPath);
					}
				}
				return true;
			}
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
				return false;
			}
		}
		public static bool UninstallRewriter(string configPath)
		{
            if (!IsRewriterInstalled(configPath))
            {
                return true;
            }
            try
			{
				XmlDocument xDoc = new XmlDocument();
				xDoc.Load(configPath);
				if (xDoc != null)
				{
					System.Xml.XmlNode xRoot = xDoc.DocumentElement;
					System.Xml.XmlNode xNode = xRoot.SelectSingleNode(_web_config_system_webserver_modules_node_XPath);
					if (xNode != null)
					{
						foreach (XmlNode n in xNode.ChildNodes)
						{
							if (n.Attributes[_attribute_name].Value == _ForumsRewriterTypeNameShort)
							{
								xNode.RemoveChild(n);
								break;
							}
						}
						xDoc.Save(configPath);
					}
				}
				return true;
			}
			catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return false;
			}
		}
	}
}

