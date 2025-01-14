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
using System.Collections.Generic;
using DotNetNuke.Services.Scheduling;

namespace DotNetNuke.Modules.ActiveForums.Queue
{
	public class Controller
	{
		public static void Add(string emailFrom, string emailTo, string emailSubject, string emailBody, string emailBodyPlainText, string emailCC, string emailBcc)
		{
			Add(-1, emailFrom, emailTo, emailSubject, emailBody, emailBodyPlainText, emailCC, emailBcc);
		}
		public static void Add(int portalId, string emailFrom, string emailTo, string emailSubject, string emailBody, string emailBodyPlainText, string emailCC, string emailBcc)
		{
			try
			{
				DataProvider.Instance().Queue_Add(portalId, emailFrom, emailTo, emailSubject, emailBody, emailBodyPlainText, emailCC, emailBcc);
			}
			catch (Exception ex)
			{
				DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
			}
		}
	}

	public class Scheduler : SchedulerClient
	{
		public Scheduler(ScheduleHistoryItem objScheduleHistoryItem)
		{
			ScheduleHistoryItem = objScheduleHistoryItem;
		}

		public override void DoWork()
		{
			try
			{
			    var intQueueCount = ProcessQueue();
				ScheduleHistoryItem.Succeeded = true;
				ScheduleHistoryItem.AddLogNote(string.Concat("Processed ", intQueueCount, " messages"));
			}
			catch (Exception ex)
			{
				ScheduleHistoryItem.Succeeded = false;
				ScheduleHistoryItem.AddLogNote(string.Concat("Process Queue Failed. ", ex));
				Errored(ref ex);
				DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
			}
		}

		private static int ProcessQueue()
		{
			var intQueueCount = 0;
			try
			{
				var dr = DataProvider.Instance().Queue_List();
				while (dr.Read())
				{
					intQueueCount += 1;
					var objEmail = new Message
					                   {
										PortalId = Convert.ToInt32(dr["PortalId"]),
										Subject = dr["EmailSubject"].ToString(),
										SendFrom = dr["EmailFrom"].ToString(),
										SendTo = dr["EmailTo"].ToString(),
										Body = dr["EmailBody"].ToString(),
										BodyText = dr["EmailBodyPlainText"].ToString(),
					                   };

				    var canDelete = objEmail.SendMail();
					if (canDelete)
					{
						try
						{
							DataProvider.Instance().Queue_Delete(Convert.ToInt32(dr["Id"]));
						}
						catch (Exception ex)
						{
							DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
						}
					}
					else
					{
						intQueueCount = intQueueCount - 1;
					}
				}
				dr.Close();
				dr.Dispose();

				return intQueueCount;
			}
			catch (Exception ex)
			{
				DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
				return -1;
			}
        }
    }

	public class Message
	{
		public int PortalId;
		public string Subject;
		public string SendFrom;
		public string SendTo;
		public string Body;
		public string BodyText;

		public bool SendMail()
		{
			try
			{
				var subs = new List<SubscriptionInfo>();
				var si = new SubscriptionInfo { Email = SendTo, DisplayName = string.Empty, LastName = string.Empty, FirstName = string.Empty };
			    subs.Add(si);
				var oEmail = new Email
				{
					PortalId = PortalId,
					UseQueue = false,
					Recipients = subs,
					Subject = Subject,
					From = SendFrom,
					BodyText = BodyText,
					BodyHTML = Body,
				                 };
			    try
				{
					var objThread = new System.Threading.Thread(oEmail.Send);
					objThread.Start();
					return true;
				}
				catch (Exception ex)
				{
					DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
					return false;
				}
			}
			catch (Exception ex)
			{
				DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
				return false;
			}
		}
	}

}