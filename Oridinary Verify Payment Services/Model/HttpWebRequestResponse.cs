using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using DMS.Business;

namespace OridinayVerifyPaymentStatus.Model
{

	public class HttpWebRequestResponse
	{
		public BaseResponce OnlinePaymentVerifyTransaction(EmitraOnlinePaymentRequest data, string VerifivationUrl)
		{
			BaseResponce responseObj = new BaseResponce();
			try
			{
				HttpResponseMessage response = new HttpResponseMessage();

				using (var client = new HttpClient())
				{

					client.DefaultRequestHeaders.Clear();
					client.DefaultRequestHeaders.Add("X-Api-Name", "PAYMENT_STATUS");
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					response = client.PostAsJsonAsync(VerifivationUrl, data).Result;

					// Verification  
					if (response.IsSuccessStatusCode)
					{
						// Reading Response.  
						string result = response.Content.ReadAsStringAsync().Result;
						responseObj = JsonConvert.DeserializeObject<BaseResponce>(result);
					}
				}
			}
			catch (System.OperationCanceledException ex)
			{
				new ServiceLog().OrdinaryCitizenVerifyErrorLog("HttpWebRequestResponse", "OnlinePaymentVerifyTransaction", "Error", "Exception Error " + "Emitra Reference No: " + data.PRN, ex.Message.ToString(), ex.InnerException.Message.ToString());
			}
			catch (HttpRequestException ex)
			{
				new ServiceLog().OrdinaryCitizenVerifyErrorLog("HttpWebRequestResponse", "OnlinePaymentVerifyTransaction", "Error", "Exception Error " + "Emitra Reference No: " + data.PRN, ex.Message.ToString(), ex.InnerException.Message.ToString());
			}
			catch (Exception ex)
			{
				new ServiceLog().OrdinaryCitizenVerifyErrorLog("HttpWebRequestResponse", "OnlinePaymentVerifyTransaction", "Error", "Exception Error " + "Emitra Reference No: " + data.PRN, ex.Message.ToString(), ex.InnerException.Message.ToString());
			}
			return responseObj;
		}

		public EmitraKioskResponse KioskPaymentVerifyTransaction(EmitraKioskRequest paymentRequest, string EncryptionPassword)
		{
			try
			{
				EmitraKioskVerifyTransactionRequest request = new EmitraKioskVerifyTransactionRequest();
				request.MERCHANTCODE = paymentRequest.MERCHANTCODE;
				request.SERVICEID = paymentRequest.SERVICEID;
				request.REQUESTID = paymentRequest.REQUESTID;
				request.SSOTOKEN = "0";
				request.CHECKSUM = new ClsCommon().GetCheckSum(new EmitraKioskVerifyTransactionChecksum { MERCHANTCODE = paymentRequest.MERCHANTCODE, REQUESTID = paymentRequest.REQUESTID, SSOTOKEN = "0" });

				string postData = ("encData=" + OridinayVerifyPaymentStatus.Model.EncodingDecoding.Encrypt(JsonConvert.SerializeObject(request), EncryptionPassword));

				var http = (HttpWebRequest)WebRequest.Create(new Uri(paymentRequest.VERIFICAION_URL));
				http.Method = "POST";
				http.Accept = "application/json";
				http.ContentType = "application/x-www-form-urlencoded";

				//Start Writing Post parameters to request object
				string parsedContent = postData.ToString();
				ASCIIEncoding encoding = new ASCIIEncoding();
				Byte[] bytes = encoding.GetBytes(parsedContent);
				Stream newStream = http.GetRequestStream();
				newStream.Write(bytes, 0, bytes.Length);
				newStream.Close();

				Stopwatch timer = new Stopwatch();
				timer.Start();

				//Read Response for posting done
				var response = http.GetResponse();
				var stream = response.GetResponseStream();
				var sr = new StreamReader(stream);
				//var content = sr.ReadToEnd();
				string Result = sr.ReadToEnd();
				string responsString = EncodingDecoding.Decrypt(Result, EncryptionPassword);
				EmitraKioskResponse emitraResponse = JsonConvert.DeserializeObject<EmitraKioskResponse>(responsString.ToString());
				emitraResponse.TRANSAMT = string.IsNullOrEmpty(emitraResponse.TRANSAMT) ? emitraResponse.AMT : emitraResponse.TRANSAMT;
				emitraResponse.RESPONSE = responsString;
				return emitraResponse;
			}
			catch (Exception ex)
			{
				new ServiceLog().OrdinaryCitizenVerifyErrorLog("HttpWebRequestResponse", "KioskPaymentVerifyTransaction", "Error", "Exception Error " + "Emitra Reference No: " + paymentRequest.REQUESTID, ex.Message.ToString(), ex.InnerException.Message.ToString());
				var res = new EmitraKioskResponse();
				return res;
			}
		}
	}
}
