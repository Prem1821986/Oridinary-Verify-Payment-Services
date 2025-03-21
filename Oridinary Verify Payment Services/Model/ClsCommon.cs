using DMS.Business;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OridinayVerifyPaymentStatus.Model
{
	class ClsCommon
	{
		public static string CurrentEmailCredentialsInUse = "CurrentEmailCredentialsInUse";
		public static string CurrentSMSServiceInUse = "CurrentSMSServiceInUse";
		public static string connection = ConfigurationManager.ConnectionStrings["ConnectionStringName"].ConnectionString.ToString();

		public DataTable VerifyRequestTransactionReq()
		{
			DataTable dt = new DataTable();
			SqlConnection Conn = null;
			try
			{
				using (Conn = new SqlConnection(ClsCommon.connection))
				{
					using (SqlCommand cmd = new SqlCommand("Sp_Get_VerifyPendingTransactionRequest", Conn))
					{
						Conn.Open();
						cmd.CommandType = CommandType.StoredProcedure;
						SqlDataAdapter da = new SqlDataAdapter(cmd);
						da.Fill(dt);
					}
				}
			}
			catch (Exception ex)
			{
				new ServiceLog().OrdinaryCitizenVerifyErrorLog("ClsCommon", "VerifyRequestTransactionReq", "Error", "Exception Error", ex.Message.ToString(), ex.InnerException.Message.ToString());
			}
			finally
			{
				Conn.Close();
			}
			return dt;
		}

		public DataSet GetUserInfoForSmsEmailForPrivatePublication(string argContext, string argDocumentId, string paymentType)
		{
			DataSet ds = new DataSet();
			SqlConnection Conn = null;
			try
			{
				using (Conn = new SqlConnection(ClsCommon.connection))
				{
					using (SqlCommand cmd = new SqlCommand("sp_GetUserInfoForSmsEmail_ForPrivatePublication", Conn))
					{
						Conn.Open();
						cmd.Parameters.AddWithValue("@Context", argContext);
						cmd.Parameters.AddWithValue("@CitizenRequestID", argDocumentId);
						cmd.Parameters.AddWithValue("@PaymentType", paymentType);
						cmd.CommandType = CommandType.StoredProcedure;
						var da = new SqlDataAdapter(cmd);
						da.Fill(ds);
						return ds;
					}
				}
			}
			catch (Exception ex)
			{
				new ServiceLog().OrdinaryCitizenVerifyErrorLog("ClsCommon", "VerifyRequestTransactionReq", "Error", "Exception Error", ex.Message.ToString(), ex.InnerException.Message.ToString());
			}
			finally
			{
				Conn.Close();
			}
			return ds;
		}

		public string TransactionStatusByResponceCode(string ResponceCode)
		{
			string status = "";
			switch (ResponceCode)
			{
				case "200":
					status = "SUCCESS";
					break;
				case "300":
					status = "FAILED";
					break;
				case "600":
					status = "PENDING";
					break;
				case "ERR0011":
					status = "No merchant is available.";
					break;
				case "ERR0012":
					status = "An error has been occurred while deciphering your request.";
					break;
				case "ERR0013":
					status = "An error has been occurred while extracting transaction details.";
					break;
				case "ERR0101":
					status = "MERCHANTCODE field must not be blank";
					break;
				case "ERR0102":
					status = "Length of MERCHANTCODE field must be between {min} and {max} characters.";
					break;
				case "ERR0103":
					status = "MERCHANTCODE field must be alphanumeric.";
					break;
				case "ERR0106":
					status = "SERVICEID field must not be null.";
					break;
				case "ERR0107":
					status = "SERVICEID field is invalid. ";
					break;

			}
			return status;
		}

		public static PGResponse Transactionfield(List<string> lstdata)
		{
			var obj = new PGResponse();
			foreach (var item in lstdata)
			{
				var lstinnerdata = item.Split('=').ToArray();
				switch (lstinnerdata[0])
				{
					case "PRN":
						obj.PRN = lstinnerdata[1];
						break;
					case "REQTIMESTAMP":
						obj.REQTIMESTAMP = lstinnerdata[1];
						break;
					case "AMOUNT":
						obj.AMOUNT = (lstinnerdata[1]);
						break;
					case "RECEIPTNO":
						obj.RECEIPTNO = lstinnerdata[1];
						break;
					case "TRANSACTIONID":
						obj.TRANSACTIONID = lstinnerdata[1];
						break;
					case "PAIDAMOUNT":
						obj.PAIDAMOUNT = (lstinnerdata[1]);
						break;
					case "EMITRATIMESTAMP":
						obj.EMITRATIMESTAMP = lstinnerdata[1];
						break;
					case "RPPTXNID":
						obj.RPPTXNID = lstinnerdata[1];
						break;
					case "RPPTIMESTAMP":
						obj.RPPTIMESTAMP = lstinnerdata[1];
						break;
					case "PAYMENTMODE":
						obj.PAYMENTMODE = lstinnerdata[1];
						break;
					case "PAYMENTMODEBID":
						obj.PAYMENTMODEBID = lstinnerdata[1];
						break;
					case "RESPONSECODE":
						obj.RESPONSECODE = lstinnerdata[1];
						break;
					case "RESPONSEMESSAGE":
						obj.RESPONSEMESSAGE = lstinnerdata[1];
						break;
					case "UDF1":
						obj.UDF1 = lstinnerdata[1];
						break;
					case "UDF2":
						obj.UDF2 = lstinnerdata[1];
						break;
					case "CHECKSUM":
						obj.CHECKSUM = lstinnerdata[1];
						break;

				}
			}
			return obj;
		}

		public string GetCheckSum(object checksum)
		{
			string retVal = CreateMD5(JsonConvert.SerializeObject(checksum));
			return retVal;
		}

		public string CreateMD5(string input)
		{
			// Use input string to calculate MD5 hash
			using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
			{
				byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
				byte[] hashBytes = md5.ComputeHash(inputBytes);

				// Convert the byte array to hexadecimal string
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				for (int i = 0; i < hashBytes.Length; i++)
				{
					//sb.Append(hashBytes[i].ToString("X2"));
					sb.Append(hashBytes[i].ToString("x2"));
				}
				return sb.ToString();
			}
		}

		public void insert_SMSNEmail_HistoryForPrivatePublication(SmsNEmailData obj, string SSOID)
		{
			DataSet ds = new DataSet();
			SqlConnection Conn = null;
			try
			{
				using (Conn = new SqlConnection(ClsCommon.connection))
				{
					using (SqlCommand cmd = new SqlCommand("sp_InsertSmsNEmailTracker", Conn))
					{
						Conn.Open();
						cmd.Parameters.AddWithValue("@ToEmailId", obj.ToEmailId);
						cmd.Parameters.AddWithValue("@ToMobileNumber", obj.ToMobileNumber);
						cmd.Parameters.AddWithValue("@EmailSubject", obj.EmailSubject);
						cmd.Parameters.AddWithValue("@cc", obj.cc);
						cmd.Parameters.AddWithValue("@bcc", obj.bcc);
						cmd.Parameters.AddWithValue("@SmsFrom", obj.SmsFrom);
						cmd.Parameters.AddWithValue("@MailFrom", obj.MailFrom);
						cmd.Parameters.AddWithValue("@Content", obj.Content);
						cmd.Parameters.AddWithValue("@Context", obj.Context);
						cmd.Parameters.AddWithValue("@ContentType", obj.ContentType);
						cmd.Parameters.AddWithValue("@ReferanceNumber", obj.ReferanceNumber);
						cmd.Parameters.AddWithValue("@SSOID", SSOID);
						cmd.CommandType = CommandType.StoredProcedure;
						var da = new SqlDataAdapter(cmd);
						da.Fill(ds);
					}
				}
			}
			catch (Exception ex)
			{
				new ServiceLog().OrdinaryCitizenVerifyErrorLog("ClsCommon", "VerifyRequestTransactionReq", "Error", "Exception Error", ex.Message.ToString(), ex.InnerException.Message.ToString());
			}
			finally
			{
				Conn.Close();
			}
		}

		public DataTable Master_Config(String Key, bool GetAllConfig)
		{
			DataTable dt = new DataTable();
			SqlConnection Conn = null;
			try
			{
				using (Conn = new SqlConnection(ClsCommon.connection))
				{
					using (SqlCommand cmd = new SqlCommand("sp_GetConfig", Conn))
					{
						Conn.Open();
						cmd.Parameters.AddWithValue("@Key", Key);
						cmd.Parameters.AddWithValue("@GetAllConfig", GetAllConfig);
						cmd.CommandType = CommandType.StoredProcedure;
						SqlDataAdapter da = new SqlDataAdapter(cmd);
						da.Fill(dt);
					}
				}
			}
			catch (Exception ex)
			{
				new ServiceLog().OrdinaryCitizenVerifyErrorLog("ClsCommon", "VerifyRequestTransactionReq", "Error", "Exception Error", ex.Message.ToString(), ex.InnerException.Message.ToString());
			}
			finally
			{
				Conn.Close();
			}
			return dt;
		}

		public EMail_Services GetSetSmsNEmailConfiguration()
		{
			try
			{
				var SetSmsNEmailConfiguration = new EMail_Services();
				var dtSms = new DataTable();
				var CurrentEmailConfigInUse = Master_Config(CurrentEmailCredentialsInUse, false);
				var CurrentSMSConfigInUse = Master_Config(CurrentSMSServiceInUse, false);
				var dtEmail = new DataTable();
				var DtCurrentEmailConfiguration = Master_Config(CurrentEmailConfigInUse.Rows[0]["TargetValue"].ToString(), false);
				dtEmail = Master_Config(DtCurrentEmailConfiguration.Rows[0]["SourceKey"].ToString(), false);

				var DtCurrentSmsConfiguration = Master_Config(CurrentSMSConfigInUse.Rows[0]["TargetValue"].ToString(), false);
				dtSms = Master_Config(DtCurrentSmsConfiguration.Rows[0]["SourceKey"].ToString(), false);

				for (int i = 0; i < dtSms.Rows.Count; i++)
				{
					if (dtSms.Rows[i]["SourceValue"].ToString() == "UniqueId")
					{
						SetSmsNEmailConfiguration.UniqueId = dtSms.Rows[i]["TargetValue"].ToString();
					}
					if (dtSms.Rows[i]["SourceValue"].ToString() == "UserId")
					{
						SetSmsNEmailConfiguration.SMSUserId = dtSms.Rows[i]["TargetValue"].ToString();
					}
					if (dtSms.Rows[i]["SourceValue"].ToString() == "Password")
					{
						SetSmsNEmailConfiguration.SMSPassword = dtSms.Rows[i]["TargetValue"].ToString();
					}
					if (dtSms.Rows[i]["SourceValue"].ToString() == "BaseAddress")
					{
						SetSmsNEmailConfiguration.BaseAddress = dtSms.Rows[i]["TargetValue"].ToString();
					}
					if (dtSms.Rows[i]["SourceValue"].ToString() == "clientId")
					{
						SetSmsNEmailConfiguration.clientId = dtSms.Rows[i]["TargetValue"].ToString();
					}
				}
				for (int i = 0; i < dtEmail.Rows.Count; i++)
				{
					if (dtEmail.Rows[i]["SourceValue"].ToString() == "SmtpClient")
					{
						SetSmsNEmailConfiguration.SmtpClient = dtEmail.Rows[i]["TargetValue"].ToString();
					}
					if (dtEmail.Rows[i]["SourceValue"].ToString() == "MailAddress")
					{
						SetSmsNEmailConfiguration.MailAddress = dtEmail.Rows[i]["TargetValue"].ToString();
					}
					if (dtEmail.Rows[i]["SourceValue"].ToString() == "UserId")
					{
						SetSmsNEmailConfiguration.UserId = dtEmail.Rows[i]["TargetValue"].ToString();
					}
					if (dtEmail.Rows[i]["SourceValue"].ToString() == "Password")
					{
						SetSmsNEmailConfiguration.Pwd = dtEmail.Rows[i]["TargetValue"].ToString();
					}
					if (dtEmail.Rows[i]["SourceValue"].ToString() == "Port")
					{
						SetSmsNEmailConfiguration.Port = dtEmail.Rows[i]["TargetValue"].ToString();
					}
					if (dtEmail.Rows[i]["SourceValue"].ToString() == "SSL")
					{
						SetSmsNEmailConfiguration.EnableSsl = dtEmail.Rows[i]["TargetValue"].ToString();
					}
				}

				return SetSmsNEmailConfiguration;
			}
			catch (Exception ex)
			{
				new ServiceLog().OrdinaryCitizenVerifyErrorLog("ClsCommon", "VerifyRequestTransactionReq", "Error", "Exception Error", ex.Message.ToString(), ex.InnerException.Message.ToString());
				return null;
			}

		}
	}

	public struct OrdinaryCitizenEmitraServiceName
	{
		public static string OrdinaryKioskGovernmentEmployeeNameChange = "Ordinary Gazette Notification Government Employee Name Change";
		public static string OrdinaryKioskPartnershipNameChange = "Ordinary Gazette Notification Partnership Amendment";
		public static string OrdinaryKioskCitizenNameChange = "Ordinary Gazette Notification Citizen Name Change";
		public static string OrdinaryOnlineGovernmentEmployeeNameChange = "Ordinary Gazette Notification Government Employee Name Change";
		public static string OrdinaryOnlinePartnershipNameChange = "Ordinary Gazette Notification Partnership Amendment";
		public static string OrdinaryOnlineCitizenNameChange = "Ordinary Gazette Notification Citizen Name Change";
	}

	public class EmitraOnlinePaymentRequest
	{
		public string MERCHANTCODE { get; set; }
		public string SERVICEID { get; set; }
		public string PRN { get; set; }
		public string AMOUNT { get; set; }
	}

	public enum OrdinaryMetaDataAction
	{
		ApplicationSave,
		ApplicationSubmitted,
		EmitraSuccess,
		EmitraFailure,
		EmitraPending,
		OrdinaryMetaDataAdded,
		OrdinaryMetaDataUpdated,
		DocumentReadyForFinalPublish,
		InProgressPublish,
		DocumentPublished,
		DocumentVerified,
		Rejected,
		REFUND
	}
	public enum AdditionalPaymentStatus
	{
		PnsAdditionalFeeRequest,
		CitizenAdditionalFeePaid,
		CitizenAdditionalFeeNotPaid,
		CitizenAdditionalFeePending,
		No
	}

	public struct OrdinaryMetaDataRemarks
	{
		public static string OK = "OK";
		public static string Approved = "Approved";
		public static string FinalPublished = "Final Published";
		public static string PaymentRefunded = "Payment Refunded";

	}


	public enum EmitraConfigurationType
	{
		Online,
		Kiosk
	}

	public enum KioskConfirmation
	{
		Pending = 0,
		Failure = 1,
		Success = 2,
		NotFromKiosk = 4
	}

	public enum EmitraPaymentMode
	{
		DemandDraft = 1,
		Challan = 2,
		OnlinePayment = 3,
		KioskPayment = 4
	}
	public enum PaymentType
	{
		MainPayment,
		AdditionalPayment
	}

	public struct HttpWebMethod
	{
		public static string PostData = "POST";
		public static string GetData = "GET";
		public static string DeleteData = "DEL";
		public static string PutData = "PUT";
	}

	public struct WebContentType
	{
		public static string MultiPartFormData = "multipart/form-data";
		public static string applicationjson = "application/json";
	}

	public class BaseResponce
	{

		public string success { get; set; }
		public string message { get; set; }
		public EmitraOnlinePymentResponse data { get; set; }

	}

	public enum CommisionPrice
	{
		KioskPrice = 60
	}

	public class EmitraOnlinePymentResponse
	{
		public string MERCHANTCODE { get; set; }
		public string SERVICEID { get; set; }
		public string STATUS { get; set; }
		public string ENCDATA { get; set; }
	}

	public class PaymentTransactionDTO
	{
		public PaymentTransactionDTO()
		{
			Id = 0; DocId = 0; PaymentTypeId = 0; EmitraTransactionId = ""; TransactionMessage = ""; TransactionType = 0; ReqId = ""; TransactionTime = ""; ReqAmount = 0; ResponseAmount = 0; UserName = ""; Status = ""; Bank = ""; BankIdNumber = ""; EmitraCommision = 0; CreatedBy = 0; UpdatedBy = 0; PAYMENTMODEBID = ""; RECEIPTNO = ""; SSOID = ""; RPPTXNID = ""; PaymentType = ""; ServiceName = "";

		}
		public int Id { get; set; }
		public int DocId { get; set; }
		public Int64 CitizenAmandmentId { get; set; }
		public int PaymentTypeId { get; set; }
		public string EmitraTransactionId { get; set; }
		public string TransactionMessage { get; set; }
		public int TransactionType { get; set; }
		public string ReqId { get; set; }
		public string TransactionTime { get; set; }
		public decimal ReqAmount { get; set; }
		public decimal ResponseAmount { get; set; }
		public string UserName { get; set; }
		public string Status { get; set; }
		public string Bank { get; set; }
		public string BankIdNumber { get; set; }
		public decimal EmitraCommision { get; set; }
		public int CreatedBy { get; set; }
		public int UpdatedBy { get; set; }
		public int KioskConfirmation { get; set; }
		public string RECEIPTNO { get; set; }
		public string PAYMENTMODEBID { get; set; }
		public string RPPTXNID { get; set; }
		public string SSOID { get; set; }
		public string PaymentType { get; set; }
		public string ServiceName { get; set; }
	}

	public class PGResponse
	{
		public string MERCHANTCODE { get; set; }
		public string REQTIMESTAMP { get; set; }
		public string PRN { get; set; }
		public string AMOUNT { get; set; }
		public string PAIDAMOUNT { get; set; }
		public string SERVICEID { get; set; }
		public string TRANSACTIONID { get; set; }
		public string RECEIPTNO { get; set; }
		public string EMITRATIMESTAMP { get; set; }
		public string STATUS { get; set; }
		public string PAYMENTMODE { get; set; }
		public string PAYMENTMODEBID { get; set; }
		public string PAYMENTMODETIMESTAMP { get; set; }
		public string RESPONSECODE { get; set; }
		public string RESPONSEMESSAGE { get; set; }
		public string UDF1 { get; set; }
		public string UDF2 { get; set; }
		public string CHECKSUM { get; set; }

		//added new field by naresh
		public string RPPTXNID { get; set; }
		public string RPPTIMESTAMP { get; set; }

	}

	public class EmitraKioskVerifyTransactionRequest
	{
		public string MERCHANTCODE { get; set; }
		public string SERVICEID { get; set; }
		public string REQUESTID { get; set; }
		public string SSOTOKEN { get; set; }
		public string CHECKSUM { get; set; }
	}

	public class EmitraResponseDTO
	{
		public int Id { get; set; }
		public string ReqId { get; set; }
		public DateTime AddDate { get; set; }
		public string EmitraResponse { get; set; }
		public string EmitraRequest { get; set; }
		public int CreatedBy { get; set; }
		public string SSOID { get; set; }
		public string PaymentType { get; set; }
	}

	public class EmitraKioskRequest
	{
		public string BASEURL { get; set; }
		public Int16 SERVICERESPONSETIME { get; set; }
		public string VERIFICAION_URL { get; set; }

		public string MERCHANTCODE { get; set; }
		public string REQTIMESTAMP { get; set; }
		public string SERVICEID { get; set; }
		public string SUBSERVICEID { get; set; }
		public string REVENUEHEAD { get; set; }
		public string CONSUMERKEY { get; set; }
		public string CONSUMERNAME { get; set; }
		public string SSOID { get; set; }
		public string OFFICECODE { get; set; }
		public string COMMTYPE { get; set; }
		public string SSOTOKEN { get; set; }
		public string CHECKSUM { get; set; }
		public string MSG { get; set; }
		public string REQUESTID { get; set; }
		public string AMOUNT { get; set; }
	}

	public class EmitraKioskResponse
	{
		public string RESPONSE { get; set; }
		public int TIMEELAPSED { get; set; }
		public string REQUESTID { get; set; }
		public string TRANSACTIONSTATUSCODE { get; set; }
		public string RECEIPTNO { get; set; }
		public string TRANSACTIONID { get; set; }
		public string TRANSAMT { get; set; }
		public string TRANSACTIONSTATUS { get; set; }
		public string MSG { get; set; }
		public string CHECKSUM { get; set; }
		public string EMITRATIMESTAMP { get; set; }

		public string AMT { get; set; }
		public string TRANSACTIONDATE { get; set; }
		public string SSOTOKEN { get; set; }

		public string MERCHANTCODE { get; set; }
		public string SERVICEID { get; set; }
		public string CONSUMERKEY { get; set; }
		public string STATUS { get; set; }
		public string PAYMENTMODE { get; set; }
	}

	public class EmitraKioskVerifyTransactionChecksum
	{
		public string MERCHANTCODE { get; set; }
		public string REQUESTID { get; set; }
		public string SSOTOKEN { get; set; }
	}

	public struct EmitraDetails
	{
		public static string MERCHANTCODE = "MERCHANTCODE";
		public static string PRN = "PRN";
		public static string STATUS = "STATUS";
		public static string ENCDATA = "ENCDATA";
		public static string _EncryptionKey;

		public static string EncryptionKey
		{
			get { return _EncryptionKey = new BPPayment().GetEmitraConfiguration(EmitraConfigurationType.Online.ToString(), "").Tables[0].Rows[0]["EncryptionKey"].ToString(); }
			set { _EncryptionKey = value; }
		}

		//public static string EncryptionKey = "mYp3s6v9y$B&E)H@McQfTjWnZr4t7w!z";
		public static string PENDING = "PENDING";
		public static string FAILED = "FAILED";
		public static string SUCCESS = "SUCCESS";
		public static string REFUND = "REFUND";
	}

	public class SmsNEmailData
	{
		public string attachment { get; set; }
		public string ToEmailId { get; set; }
		public string ToMobileNumber { get; set; }

		public string EmailSubject { get; set; }
		public string cc { get; set; }
		public string bcc { get; set; }
		public string SmsFrom { get; set; }
		public string MailFrom { get; set; }
		public string Content { get; set; }
		public string Context { get; set; }
		public string ContentType { get; set; }
		public string ReferanceNumber { get; set; }
		public List<string> MobileNo { get; set; }
		public string MsgText { get; set; }
		public string ETemplateID { get; set; }
	}
	public class ExternalSMSApiInfo
	{
		public string UniqueID { get; set; }
		public string serviceName { get; set; }
		public string language { get; set; }
		public string message { get; set; }
		public List<string> mobileNo { get; set; }
		public string templateID { get; set; }
	}
}
