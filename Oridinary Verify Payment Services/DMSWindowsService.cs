using DMS.Business;
using Newtonsoft.Json;
using OridinayVerifyPaymentStatus.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace DMS_WindowsService
{
	public partial class DMSWindowsService : ServiceBase
	{
		ServiceLog ServiceErrorLog = null;
		int NumOfThreads = 16;
		public DMSWindowsService()
		{
			InitializeComponent();
			ServiceErrorLog = new ServiceLog();
			NumOfThreads = 16;
		}

		public void OnDebug()
		{
			OnStart(null);
		}
		Thread T1 = null;
		protected override void OnStart(string[] args)
		{
			ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "OnStart", "Start", "Service Running", "", "");

			T1 = new Thread(new ThreadStart(FirstThreadFunc));
			//FirstThreadFunc();
			T1.Start();
			ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "OnStart", "On Start Executed", "Service Completed", "", "");
		}

		protected override void OnStop()
		{
			ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "OnStop", "On Stop Executed", "Service Stop", "", "");
		}

		private void FirstThreadFunc()
		{
            //string DecryptedData = EncodingDecoding.DecryptForEmitra("h4ToaLDlaWDTnP8E/Iq8vK3OAETm+RlXmpxiwU3ex67iwvC/L470KV6oWlQjx96mFITu2viheXpL236GHd/hDOPTG4EazYQO/2bzVRA5qQha6sLBmDxn7dVcUOUYU+MwkPryqEQlxgirV//9ugk7GLP0eUutHCkOM7awas6cR6Wlf9UilHIAp3Ayrfv4gZO5Chilm6jA1aiUvRJCtDN8eVVLmkEwvu8LknWy918yrbANb+S4yWE5VGWNl18+MvSbAYEOvpRXrqOyHZk99oDivvy13+7uo+1fnQEWKQdQcASOMSL1qTkmNcdqOBpXjbq2f4ew0Afw9uDoai025ncT5/zJcF4vn74d4C0KBe2xYoQrrf2f+Pwl3X7BfWW1TVa7PdyTurBnUKoZKHUgGusEE4ATlmTRGvcoGyuAKNvd4s4QtRPJjY63r+d3RC85kTFpMjX7dQM7ZU9e1dSWwDAasFkuD+AyinYQalXUM1x7EXKorcFfXKUyJ58x8i4D9UCuB9qBkf6Vxs9+WAfTAM9IMYGFkvXMxB9y2ZAdRNckhU1FQ857FcZVo2hHf+v73lVS1WIUXllQw/NIJ4X458bnFoSf4w/7K6e0cD4Gc9zJVj0=", "EB85C3FD7532A15CAB971E246581C");
            var dt = new ClsCommon().VerifyRequestTransactionReq();

			if (dt != null && dt.Rows.Count > 0)
			{
				ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "FirstThreadFunc", "Start", "Payment Verify Start", "Total record:-"+dt.Rows.Count.ToString(), "");
				Parallel.For(0, dt.Rows.Count, new ParallelOptions { MaxDegreeOfParallelism = NumOfThreads }, i =>
				{
					WorkingOnDataRowForOrdinaryVerifyPaymentStatus(dt, i);
				});

				//for (int i = 0; i < dt.Rows.Count; i++)
				//{
				//	WorkingOnDataRowForOrdinaryVerifyPaymentStatus(dt, i);
				//}
				ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "FirstThreadFunc", "Payment Verify Completed", " Payment Verify Completed", "", "");
				
			}
			else
				ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "FirstThreadFunc", "On Payment Verify Completed", "NO recored found.", "", "");
		}

		private void WorkingOnDataRowForOrdinaryVerifyPaymentStatus(DataTable dt, int i)
		{
			try
			{
				var CitizenRequestId = "";
				var MainReferenceNo = "";
				var PayableAmount = "";
				var AddtionalReferenceNo = "";
				var FeeType = "";
				var PaymentMode = "";
				CitizenRequestId = dt.Rows[i]["CitizenRequestID"].ToString();
				MainReferenceNo = dt.Rows[i]["TemplateReferenceId"].ToString();
				PayableAmount = dt.Rows[i]["ReqAmount"].ToString();
				AddtionalReferenceNo = dt.Rows[i]["AdditionalTemplateReferenceId"].ToString();
				FeeType = dt.Rows[i]["FeeType"].ToString();
				PaymentMode = dt.Rows[i]["TransactionType"].ToString();

				if (Convert.ToInt32(PaymentMode) == Convert.ToInt32(EmitraPaymentMode.OnlinePayment))
				{
					if (FeeType == PaymentType.MainPayment.ToString())
					{
						VerifyOnlineTransaction(CitizenRequestId, MainReferenceNo, PayableAmount, FeeType);
					}
					else
					{
						VerifyOnlineTransaction(CitizenRequestId, AddtionalReferenceNo, PayableAmount, FeeType);
					}
				}
				else if (Convert.ToInt32(PaymentMode) == Convert.ToInt32(EmitraPaymentMode.KioskPayment))
				{
					if (FeeType == PaymentType.MainPayment.ToString())
					{
						VerifyKioskTransaction(CitizenRequestId, MainReferenceNo, FeeType);
					}
					else
					{
						VerifyKioskTransaction(CitizenRequestId, AddtionalReferenceNo, FeeType);
					}

				}
			}
			catch (Exception ex)
			{
				ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "WorkingOnDataRowForOrdinaryVerifyPaymentStatus", "Error", "Exception Error", ex.Message.ToString(), ex.InnerException.Message.ToString());
			}
		}


		public void VerifyOnlineTransaction(string CitizenRequestId, string citizenReferenceNo, string PayableAmount, string FeeType)
		{
			try
			{
				EmitraOnlinePaymentRequest obj = new EmitraOnlinePaymentRequest();
				BPPayment bPayment = new BPPayment();
				var TemplateID = bPayment.GetTempLateByRequestID(Convert.ToInt32(CitizenRequestId)).Rows[0]["TemplateId"].ToString();
				string serviceName =GetPGOnlineServiceName(Convert.ToInt32(TemplateID));
				var emitra = bPayment.GetEmitraConfiguration(EmitraConfigurationType.Online.ToString(), serviceName).Tables[0];
				
				obj.MERCHANTCODE = Convert.ToString(emitra.Rows[0]["MerchantCode"]);
				obj.SERVICEID = Convert.ToString(emitra.Rows[0]["SERVICEID"]);
				obj.PRN = citizenReferenceNo;
				obj.AMOUNT = PayableAmount;
				string VerifivationUrl = Convert.ToString(emitra.Rows[0]["OnlineTransactionverificationURL"]);
				ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "VerifyOnlineTransaction", "calling before", "Call OnlinePaymentVerifyTransaction API Start", "CitizenRequestId:" + CitizenRequestId, "");				
				var response = new HttpWebRequestResponse().OnlinePaymentVerifyTransaction(obj, VerifivationUrl);
				ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "VerifyOnlineTransaction", "calling After", "Call OnlinePaymentVerifyTransaction API end", "CitizenRequestId:" + CitizenRequestId, "");
		
				if (response.success == "True" || response.success == "true")
				{
					var paymentTransactionDTO = new PaymentTransactionDTO();
					var emitraResponseDTO = new EmitraResponseDTO();
					string DecryptedData = EncodingDecoding.DecryptForEmitra(response.data.ENCDATA, emitra.Rows[0]["EncryptionKey"].ToString());
					var lstdata = DecryptedData.Replace("::", "&").Split('&').ToList();
					var ObjPGResponse = ClsCommon.Transactionfield(lstdata);

                    try
                    {
                        string json = JsonConvert.SerializeObject(ObjPGResponse);
                        ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "VerifyOnlineTransaction", "Extrenal API", "Call OnlinePaymentVerifyTransaction API Start", "CitizenRequestId:" + CitizenRequestId, json);
                    }
                    catch { }

                    ObjPGResponse.UDF1 = new BPPayment().GetSSOIDByCitizenRequestID(ObjPGResponse.UDF1).Tables[0].Rows[0]["SSOID"].ToString();


					emitraResponseDTO.CreatedBy = Convert.ToInt32(1);
					emitraResponseDTO.SSOID = ObjPGResponse.UDF1;
					emitraResponseDTO.EmitraResponse = DecryptedData;
					emitraResponseDTO.ReqId = CitizenRequestId;
					emitraResponseDTO.PaymentType = FeeType;
					new BPPayment().InsertEmitraResponse(emitraResponseDTO);

					paymentTransactionDTO.TransactionMessage = ObjPGResponse.RESPONSEMESSAGE;
					paymentTransactionDTO.RECEIPTNO = ObjPGResponse.RECEIPTNO;
					paymentTransactionDTO.PAYMENTMODEBID = ObjPGResponse.PAYMENTMODEBID;
					paymentTransactionDTO.RPPTXNID = ObjPGResponse.RPPTXNID;
					paymentTransactionDTO.KioskConfirmation = Convert.ToInt32(KioskConfirmation.NotFromKiosk);
					paymentTransactionDTO.Status = new ClsCommon().TransactionStatusByResponceCode(ObjPGResponse.RESPONSECODE);
					paymentTransactionDTO.ReqId = CitizenRequestId;
					paymentTransactionDTO.TransactionType = Convert.ToInt32(EmitraPaymentMode.OnlinePayment);
					paymentTransactionDTO.SSOID = ObjPGResponse.UDF1;
					paymentTransactionDTO.Bank = ObjPGResponse.PAYMENTMODE;
					paymentTransactionDTO.EmitraTransactionId = ObjPGResponse.TRANSACTIONID;
					paymentTransactionDTO.TransactionTime = ObjPGResponse.EMITRATIMESTAMP;

					paymentTransactionDTO.ReqAmount = Convert.ToDecimal(ObjPGResponse.AMOUNT);
					paymentTransactionDTO.ResponseAmount = Convert.ToDecimal(ObjPGResponse.PAIDAMOUNT);
					paymentTransactionDTO.EmitraCommision = Convert.ToDecimal(ObjPGResponse.PAIDAMOUNT) > 0 ? Convert.ToDecimal(ObjPGResponse.PAIDAMOUNT) - Convert.ToDecimal(ObjPGResponse.AMOUNT) : 0;
					paymentTransactionDTO.PaymentType = FeeType;
					paymentTransactionDTO.ServiceName = serviceName;


					if (paymentTransactionDTO.Status == EmitraDetails.PENDING)
					{
                        ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "VerifyOnlineTransaction", "Pending", "This citizen payment is pending", "CitizenRequestId:" + CitizenRequestId, "");

					}
					else if (paymentTransactionDTO.Status == EmitraDetails.FAILED)
					{
						new BPPayment().InsertPaymentTransaction(paymentTransactionDTO);
						AddPrivatePublicationStaus(paymentTransactionDTO.ReqId, OrdinaryMetaDataAction.EmitraFailure.ToString(), paymentTransactionDTO.SSOID, OrdinaryMetaDataRemarks.OK.ToString());
						ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "VerifyOnlineTransaction", paymentTransactionDTO.Status, "This citizen payment is failed", "CitizenRequestId:" + CitizenRequestId, ObjPGResponse.RESPONSEMESSAGE);
					}
					else if (paymentTransactionDTO.Status == EmitraDetails.SUCCESS)
					{
						new BPPayment().InsertPaymentTransaction(paymentTransactionDTO);
						AddPrivatePublicationStaus(paymentTransactionDTO.ReqId, OrdinaryMetaDataAction.EmitraSuccess.ToString(), paymentTransactionDTO.SSOID, OrdinaryMetaDataRemarks.OK.ToString());
						if (FeeType == PaymentType.MainPayment.ToString())
							SendEmailAndSMSForPrivatePublication(OrdinaryMetaDataAction.ApplicationSubmitted.ToString(), CitizenRequestId, paymentTransactionDTO.SSOID, "", "", "", "0", paymentTransactionDTO.PaymentType);
						else
							SendEmailAndSMSForPrivatePublication(AdditionalPaymentStatus.CitizenAdditionalFeePaid.ToString(), CitizenRequestId, paymentTransactionDTO.SSOID, "", "", "", "0", paymentTransactionDTO.PaymentType);
						ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "VerifyOnlineTransaction", paymentTransactionDTO.Status, "This citizen payment is Successfully update.", "CitizenRequestId:" + CitizenRequestId, ObjPGResponse.RESPONSEMESSAGE);
					}
				}
				else
				{
					ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "VerifyOnlineTransaction", response.success, "This citizen payment is processing..", "CitizenRequestId:" + CitizenRequestId, response.message);
				}
			}
			catch (Exception ex)
			{
				ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "VerifyOnlineTransaction", "Error", "Exception Error", ex.Message.ToString(), ex.InnerException.Message.ToString());
			}
		}

		public void VerifyKioskTransaction(string CitizenRequestId, string citizenReferenceNo, string FeeType)
		{
			try
			{
				EmitraKioskRequest obj = new EmitraKioskRequest();
				BPPayment bPayment = new BPPayment();
				var TemplateID = bPayment.GetTempLateByRequestID(Convert.ToInt32(CitizenRequestId)).Rows[0]["TemplateId"].ToString();
				string serviceName = GetPGOnlineServiceName(Convert.ToInt32(TemplateID));
				var ds = new BPPayment().GetEmitraConfiguration(EmitraConfigurationType.Kiosk.ToString(), serviceName, Convert.ToInt32(TemplateID));
				if (ds != null && ds.Tables.Count > 0)
				{
					DataTable emitra = ds.Tables[0];
					DataTable emitraComm = ds.Tables[1];
					obj.MERCHANTCODE = Convert.ToString(emitra.Rows[0]["MerchantCode"]);
					obj.SERVICEID = Convert.ToString(emitra.Rows[0]["SERVICEID"]);
					obj.REQUESTID = citizenReferenceNo;
					obj.VERIFICAION_URL = Convert.ToString(emitra.Rows[0]["KIOSKTransactionverificationURL"]);

					ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "VerifyKioskTransaction", "calling before", "Call OnlinePaymentVerifyTransaction API Start", "CitizenRequestId:" + CitizenRequestId, "");

                    var emitraKisokResponse = new HttpWebRequestResponse().KioskPaymentVerifyTransaction(obj, Convert.ToString(emitra.Rows[0]["EncryptionKey"]));
					ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "VerifyKioskTransaction", "calling After", "Call OnlinePaymentVerifyTransaction API end", "CitizenRequestId:" + CitizenRequestId, "");


					if (emitraKisokResponse.TRANSACTIONSTATUS == "SUCCESS")
					{
						var paymentTransactionDTO = new PaymentTransactionDTO();
						var emitraResponseDTO = new EmitraResponseDTO();
						string SSOID = new BPPayment().GetSSOIDByCitizenRequestID(CitizenRequestId).Tables[0].Rows[0]["SSOID"].ToString();

						emitraResponseDTO.CreatedBy = Convert.ToInt32(1);
						emitraResponseDTO.SSOID = SSOID;
						emitraResponseDTO.EmitraResponse = emitraKisokResponse.RESPONSE;
						emitraResponseDTO.ReqId = CitizenRequestId;
						emitraResponseDTO.PaymentType = FeeType;
						new BPPayment().InsertEmitraResponse(emitraResponseDTO);

						paymentTransactionDTO.Status = emitraKisokResponse.TRANSACTIONSTATUS;
						paymentTransactionDTO.ReqId = CitizenRequestId;
						paymentTransactionDTO.EmitraTransactionId = emitraKisokResponse.TRANSACTIONID;
						paymentTransactionDTO.TransactionTime = emitraKisokResponse.EMITRATIMESTAMP;
						paymentTransactionDTO.SSOID = SSOID;
						paymentTransactionDTO.TransactionMessage = emitraKisokResponse.MSG;
						paymentTransactionDTO.TransactionType = Convert.ToInt32(EmitraPaymentMode.KioskPayment);
						paymentTransactionDTO.RECEIPTNO = emitraKisokResponse.RECEIPTNO;
						paymentTransactionDTO.Bank = emitraKisokResponse.PAYMENTMODE;

						paymentTransactionDTO.ReqAmount = (Convert.ToDecimal(emitraKisokResponse.TRANSAMT) - (Convert.ToInt32(emitraComm.Rows[0]["SubHeadCommision_Price"])));
						paymentTransactionDTO.ResponseAmount = Convert.ToDecimal(emitraKisokResponse.TRANSAMT);
						paymentTransactionDTO.EmitraCommision = (Convert.ToInt32(emitraComm.Rows[0]["SubHeadCommision_Price"]));
						paymentTransactionDTO.KioskConfirmation = Convert.ToInt32(KioskConfirmation.Success);
						paymentTransactionDTO.PaymentType = FeeType;
						paymentTransactionDTO.ServiceName = serviceName;
						new BPPayment().InsertPaymentTransaction(paymentTransactionDTO);
						AddPrivatePublicationStaus(paymentTransactionDTO.ReqId, OrdinaryMetaDataAction.EmitraSuccess.ToString(), paymentTransactionDTO.SSOID, OrdinaryMetaDataRemarks.OK.ToString());
						if (FeeType == PaymentType.MainPayment.ToString())
							SendEmailAndSMSForPrivatePublication(OrdinaryMetaDataAction.ApplicationSubmitted.ToString(), CitizenRequestId, paymentTransactionDTO.SSOID, "", "", "", "0", paymentTransactionDTO.PaymentType);
						else
							SendEmailAndSMSForPrivatePublication(AdditionalPaymentStatus.CitizenAdditionalFeePaid.ToString(), CitizenRequestId, paymentTransactionDTO.SSOID, "", "", "", "0", paymentTransactionDTO.PaymentType);
						ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "VerifyKioskTransaction", paymentTransactionDTO.Status, emitraKisokResponse.MSG, "CitizenRequestId:" + CitizenRequestId, "");
					}
					else
					{
						ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "VerifyKioskTransaction", emitraKisokResponse.TRANSACTIONSTATUS, "This citizen payment is processing..", "CitizenRequestId:" + CitizenRequestId, emitraKisokResponse.MSG);
					}
				}
			}
			catch (Exception ex)
			{
				ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "VerifyKioskTransaction", "Error", "Exception Error", ex.Message.ToString(), ex.InnerException.Message.ToString());
			}
		}

		public void AddPrivatePublicationStaus(string requestID, string status, string SSOID, string remarks)
		{
			if (!String.IsNullOrEmpty(requestID))
			{
				new BPPayment().AddUpdateStatusForPrivatePublication(Convert.ToString(requestID), status, SSOID, remarks);
			}
		}

		public void SendEmailAndSMSForPrivatePublication(string TemplateType, string CitizenRequestID, string SSOID, string RejectionResion, string Volumn, string Number, string TotalPageCount = "0", string paymentType = "")
		{
			try
			{
				var smsAndEmailObjt = new ClsCommon().GetUserInfoForSmsEmailForPrivatePublication(TemplateType.ToString(), CitizenRequestID, paymentType);
				var Email = string.Empty;
				var sms = string.Empty;
				var EmailTemplate = string.Empty;
				var smsTemplate = string.Empty;
				if (smsAndEmailObjt.Tables.Count > 0)
				{
					DataTable dt = smsAndEmailObjt.Tables[0];
					for (int i = 0; i < dt.Rows.Count; i++)
					{
						var smsandemail = new SmsNEmailData();
						smsandemail = GetSmsAndEmailString(dt.Rows[i], TemplateType, smsandemail, RejectionResion, Volumn, Number, TotalPageCount);
						smsandemail.ToEmailId = Convert.ToString(dt.Rows[i]["Email ID"]);
						smsandemail.cc = Convert.ToString(dt.Rows[i]["cc"]);
						smsandemail.bcc = Convert.ToString(dt.Rows[i]["bcc"]);
						smsandemail.ReferanceNumber = Convert.ToString(CitizenRequestID);
						smsandemail.MobileNo = Convert.ToString(dt.Rows[i]["Mobile Number"]).Split(',').ToList();

						new EMail_Services().sendEMailForPrivatePublication(smsandemail, SSOID);
						new EMail_Services().sendSMSForPrivatePublication(smsandemail);
					}
				}
			}
			catch (Exception ex)
			{
				ServiceErrorLog.OrdinaryCitizenVerifyErrorLog("DMSWindowsService", "SendEmailAndSMSForPrivatePublication", "Error", "Exception Error", ex.Message.ToString(), ex.InnerException.Message.ToString());
			}

		}

		public SmsNEmailData GetSmsAndEmailString(DataRow dr, string TemplateType, SmsNEmailData smsandemail, string RejectionResion = "", string Volumn = "", string Number = "", string TotalPageCount = "")
		{
			var emailTemplate = string.Empty;

			if (TemplateType.ToString() == OrdinaryMetaDataAction.ApplicationSubmitted.ToString())
			{
				if (Convert.ToString(dr["UserType"]) == "Citizen")
				{
					smsandemail.Content = String.Format(Convert.ToString(dr["EmailTemplate"]), Convert.ToString(dr["CitizenRequestID"]), Convert.ToString(dr["TemplateReferenceId"]), Convert.ToString(dr["templateName"]), Convert.ToString(dr["ReqAmount"]), Convert.ToString(dr["EmitraTransactionId"]), Convert.ToString(dr["paymentdate"]));
					smsandemail.EmailSubject = string.Format(Convert.ToString(dr["EmailSubject"]), Convert.ToString(dr["templateName"]), Convert.ToString(dr["CitizenRequestID"]));
					smsandemail.MsgText = String.Format(Convert.ToString(dr["EmailTemplate"]), Convert.ToString(dr["CitizenRequestID"]), Convert.ToString(dr["TemplateReferenceId"]), Convert.ToString(dr["templateName"]), Convert.ToString(dr["ReqAmount"]), Convert.ToString(dr["EmitraTransactionId"]), Convert.ToString(dr["paymentdate"]));
				}
				else if (Convert.ToString(dr["UserType"]) == "Publisher")
				{
					smsandemail.Content = String.Format(Convert.ToString(dr["EmailTemplate"]), Convert.ToString(dr["CitizenRequestID"]), Convert.ToString(dr["TemplateReferenceId"]), Convert.ToString(dr["templateName"]), Convert.ToString(dr["SSOID"]), Convert.ToString(dr["ReqAmount"]), Convert.ToString(dr["EmitraTransactionId"]), Convert.ToString(dr["paymentdate"]));
					smsandemail.EmailSubject = string.Format(Convert.ToString(dr["EmailSubject"]), Convert.ToString(dr["templateName"]), Convert.ToString(dr["CitizenRequestID"]));
					smsandemail.MsgText = String.Format(Convert.ToString(dr["EmailTemplate"]), Convert.ToString(dr["CitizenRequestID"]), Convert.ToString(dr["TemplateReferenceId"]), Convert.ToString(dr["templateName"]), Convert.ToString(dr["SSOID"]), Convert.ToString(dr["ReqAmount"]), Convert.ToString(dr["EmitraTransactionId"]), Convert.ToString(dr["paymentdate"]));
				}
			}

			else if (TemplateType.ToString() == AdditionalPaymentStatus.CitizenAdditionalFeePaid.ToString())
			{
				if (Convert.ToString(dr["UserType"]) == "Citizen")
				{
					smsandemail.Content = String.Format(Convert.ToString(dr["EmailTemplate"]), TotalPageCount, Convert.ToString(dr["CitizenRequestID"]), Convert.ToString(dr["templateName"]), Convert.ToString(dr["ReqAmount"]), Convert.ToString(dr["EmitraTransactionId"]), Convert.ToString(dr["paymentdate"]));
					smsandemail.EmailSubject = string.Format(Convert.ToString(dr["EmailSubject"]), Convert.ToString(dr["templateName"]), Convert.ToString(dr["CitizenRequestID"]));
					smsandemail.MsgText = String.Format(Convert.ToString(dr["EmailTemplate"]), TotalPageCount, Convert.ToString(dr["CitizenRequestID"]), Convert.ToString(dr["templateName"]), Convert.ToString(dr["ReqAmount"]), Convert.ToString(dr["EmitraTransactionId"]), Convert.ToString(dr["paymentdate"]));
				}
				else if (Convert.ToString(dr["UserType"]) == "Publisher")
				{
					smsandemail.Content = String.Format(Convert.ToString(dr["EmailTemplate"]), TotalPageCount, Convert.ToString(dr["CitizenRequestID"]), Convert.ToString(dr["templateName"]), Convert.ToString(dr["ReqAmount"]), Convert.ToString(dr["EmitraTransactionId"]), Convert.ToString(dr["paymentdate"]));
					smsandemail.EmailSubject = string.Format(Convert.ToString(dr["EmailSubject"]), Convert.ToString(dr["templateName"]), Convert.ToString(dr["CitizenRequestID"]));
					smsandemail.MsgText = String.Format(Convert.ToString(dr["EmailTemplate"]), TotalPageCount, Convert.ToString(dr["CitizenRequestID"]), Convert.ToString(dr["templateName"]), Convert.ToString(dr["ReqAmount"]), Convert.ToString(dr["EmitraTransactionId"]), Convert.ToString(dr["paymentdate"]));
				}
			}
			return smsandemail;
		}


		public string GetPGOnlineServiceName(int num)
		{
			string status = "";
			switch (num)
			{
				case 1:
					status = OrdinaryCitizenEmitraServiceName.OrdinaryOnlineCitizenNameChange.ToString();
					break;

				case 2:
					status = OrdinaryCitizenEmitraServiceName.OrdinaryOnlinePartnershipNameChange.ToString();
					break;

				case 3:
					status = OrdinaryCitizenEmitraServiceName.OrdinaryOnlineGovernmentEmployeeNameChange.ToString();
					break;
			}
			return status;
		}
	}
}
