using DMS.Business;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace OridinayVerifyPaymentStatus.Model
{
	public class EMail_Services
	{
		public string username { get; set; }
		public string password { get; set; }
		public string senderid { get; set; }
		public string URL { get; set; }
		public string SmtpClient { get; set; }
		public string MailAddress { get; set; }
		public string UserId { get; set; }
		public string Pwd { get; set; }
		public string EnableSsl { get; set; }
		public string Port { get; set; }


		public string BaseAddress { get; set; }
		public string SMSUserId { get; set; }
		public string SMSPassword { get; set; }
		public string UniqueId { get; set; }
		public string clientId { get; set; }

		static String scheduledTime = "20110819 13:26:00";


		

		#region Sms And Email function
		public void sendEMailForPrivatePublication(SmsNEmailData obj, string SSOID)
		{
			try
			{
				EMail_Services _SMS_EMail_Services = new ClsCommon().GetSetSmsNEmailConfiguration();

				var CC_Address = obj.cc;
				var BCC = obj.bcc;
				var mail = new MailMessage();
				var SmtpServer = new SmtpClient(_SMS_EMail_Services.SmtpClient);
				if (!string.IsNullOrEmpty(CC_Address))
				{
					// mail.Bcc.Add(BCC );
					mail.CC.Add(CC_Address);
				}
				if (!string.IsNullOrEmpty(BCC))
				{
					// mail.Bcc.Add(BCC );
					mail.Bcc.Add(BCC);
				}

				mail.From = new MailAddress(_SMS_EMail_Services.MailAddress);
				mail.To.Add(obj.ToEmailId);
				mail.Subject = obj.EmailSubject;
				mail.Body = obj.Content;


				mail.IsBodyHtml = true;
				SmtpServer.Port = Convert.ToInt32(_SMS_EMail_Services.Port);
				//SmtpServer.Port = 25;
				SmtpServer.Credentials = new System.Net.NetworkCredential(_SMS_EMail_Services.MailAddress, _SMS_EMail_Services.Pwd);
				SmtpServer.EnableSsl = Convert.ToBoolean(_SMS_EMail_Services.EnableSsl);
				//disable 2 step verification
				//turn on less secure app
				SmtpServer.Send(mail);
				new ClsCommon().insert_SMSNEmail_HistoryForPrivatePublication(obj, SSOID);
			}
			catch (Exception ex)
			{
				new ServiceLog().OrdinaryCitizenVerifyErrorLog("EMail_Services", "sendEMailForPrivatePublication", "Error", "Exception Error", ex.Message.ToString(), ex.InnerException.Message.ToString());
			}

		}
		
		public void sendSMSForPrivatePublication(SmsNEmailData obj)
		{
			try
			{
				EMail_Services _SMS_EMail_Services = new ClsCommon().GetSetSmsNEmailConfiguration();

				System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls;
				HttpClient client = new HttpClient();
				client.BaseAddress = new Uri(_SMS_EMail_Services.BaseAddress);
				client.DefaultRequestHeaders.Add("username", _SMS_EMail_Services.SMSUserId);
				client.DefaultRequestHeaders.Add("password", _SMS_EMail_Services.SMSPassword);

				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
				MultipartFormDataContent form = new MultipartFormDataContent();
				System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (
				Object _obj, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain,
				System.Net.Security.SslPolicyErrors errors)
				{
					return (true);
				};

				var inputparams = new ExternalSMSApiInfo();
				inputparams.UniqueID = _SMS_EMail_Services.UniqueId;
				inputparams.serviceName = "SMS";
				inputparams.language = "ENG";
				inputparams.message = obj.MsgText;
				inputparams.mobileNo = obj.MobileNo;
				var response = client.PostAsJsonAsync("api/OBD/CreateSMS/Request?client_id=" + _SMS_EMail_Services.clientId, inputparams).Result;
				var asyncResponse = response.Content.ReadAsStringAsync().Result;
				var jsonResponse = JObject.Parse(asyncResponse);
				string status = "Response Code: " + jsonResponse["responseCode"] + "\n\nResponse Message - " + jsonResponse["responseMessage"];
			}
			catch (Exception ex)
			{
				new ServiceLog().OrdinaryCitizenVerifyErrorLog("EMail_Services", "sendSMSForPrivatePublication", "Error", "Exception Error", ex.Message.ToString(), ex.InnerException.Message.ToString());
			}

		}
		#endregion
	}
}
