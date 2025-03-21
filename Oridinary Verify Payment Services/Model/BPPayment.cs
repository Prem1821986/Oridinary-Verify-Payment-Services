using DMS.Business;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OridinayVerifyPaymentStatus.Model
{
	class BPPayment
	{
		public DataTable GetTempLateByRequestID(Int32 citizenRequestID)
		{
			DataTable dt = new DataTable();
			SqlConnection Conn = null;
			try
			{
				using (Conn = new SqlConnection(ClsCommon.connection))
				{
					using (SqlCommand cmd = new SqlCommand("SpGetTemplateIDByCitizenRequestID", Conn))
					{
						Conn.Open();
						cmd.Parameters.AddWithValue("@CitizenRequestID", citizenRequestID);
						cmd.CommandType = CommandType.StoredProcedure;
						SqlDataAdapter da = new SqlDataAdapter(cmd);
						da.Fill(dt);
					}
				}
			}
			catch (Exception ex)
			{
				new ServiceLog().OrdinaryCitizenVerifyErrorLog("BPPayment", "GetTempLateByRequestID", "Error", "Exception Error", ex.Message.ToString(), ex.InnerException.Message.ToString());
			}
			finally
			{
				Conn.Close();
			}
			return dt;
		}

		public DataSet GetEmitraConfiguration(string serviceType, string serviceName, int TemplateID = 0)
		{
			DataSet ds = new DataSet();
			SqlConnection Conn = null;
			try
			{
				using (Conn = new SqlConnection(ClsCommon.connection))
				{
					using (SqlCommand cmd = new SqlCommand("SpGetEmitraConfiguration", Conn))
					{
						Conn.Open();
						cmd.Parameters.AddWithValue("@ServiceType", serviceType);
						cmd.Parameters.AddWithValue("@ServiceName", serviceName);
						cmd.Parameters.AddWithValue("@templateID", TemplateID);
						cmd.CommandType = CommandType.StoredProcedure;
						SqlDataAdapter da = new SqlDataAdapter(cmd);
						da.Fill(ds);
					}
				}
			}
			catch (Exception ex)
			{
				new ServiceLog().OrdinaryCitizenVerifyErrorLog("BPPayment", "GetEmitraConfiguration", "Error", "Exception Error", ex.Message.ToString(), ex.InnerException.Message.ToString());
			}
			finally
			{
				Conn.Close();
			}
			return ds;
		}

		public DataSet GetSSOIDByCitizenRequestID(string RequestID)
		{
			DataSet ds = new DataSet();
			SqlConnection Conn = null;
			try
			{
				using (Conn = new SqlConnection(ClsCommon.connection))
				{
					using (SqlCommand cmd = new SqlCommand("SP_GetSSOIDBYCITIZENREQUESTID", Conn))
					{
						Conn.Open();
						cmd.Parameters.AddWithValue("@RequestID", RequestID);
						cmd.CommandType = CommandType.StoredProcedure;
						SqlDataAdapter da = new SqlDataAdapter(cmd);
						da.Fill(ds);
					}
				}
			}
			catch (Exception ex)
			{
				new ServiceLog().OrdinaryCitizenVerifyErrorLog("BPPayment", "GetSSOIDByCitizenRequestID", "Error", "Exception Error", ex.Message.ToString(), ex.InnerException.Message.ToString());
			}
			finally
			{
				Conn.Close();
			}
			return ds;
		}

		public Int32 InsertEmitraResponse(EmitraResponseDTO model)
		{
			DataTable dt = new DataTable();
			SqlConnection Conn = null;
			Int32 result = 0;
			try
			{
				using (Conn = new SqlConnection(ClsCommon.connection))
				{
					using (SqlCommand cmd = new SqlCommand("SpSetEmitraRequestResponse", Conn))
					{
						Conn.Open();
						cmd.Parameters.AddWithValue("@ReqId", model.ReqId);
						cmd.Parameters.AddWithValue("@EmitraRequest", model.EmitraRequest);
						cmd.Parameters.AddWithValue("@EmitraResponse", model.EmitraResponse);
						cmd.Parameters.AddWithValue("@PaymentType", model.PaymentType);
						//cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);
						if (model.CreatedBy > 0)
						{
							cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);
						}
						else
						{
							cmd.Parameters.AddWithValue("@CreatedBy", 0);
						}
						if (model.SSOID != "" && model.SSOID != null)
						{
							cmd.Parameters.AddWithValue("@SSOID", model.SSOID);
						}
						else
						{
							cmd.Parameters.AddWithValue("@SSOID", "");
						}
						cmd.CommandType = CommandType.StoredProcedure;
						result = cmd.ExecuteNonQuery();
					}
				}
			}
			catch (Exception ex)
			{
				new ServiceLog().OrdinaryCitizenVerifyErrorLog("BPPayment", "InsertEmitraResponse", "Error", "Exception Error", ex.Message.ToString(), ex.InnerException.Message.ToString());
			}
			finally
			{
				Conn.Close();
			}
			return result;
		}

		public Int32 InsertPaymentTransaction(PaymentTransactionDTO model)
		{
			DataTable dt = new DataTable();
			SqlConnection Conn = null;
			Int32 result = 0;

			try
			{
				using (Conn = new SqlConnection(ClsCommon.connection))
				{
					using (SqlCommand cmd = new SqlCommand("SpInsertEmitraPaymentTransaction", Conn))
					{
						Conn.Open();
						cmd.Parameters.AddWithValue("@EmitraTransactionId", model.EmitraTransactionId);
						cmd.Parameters.AddWithValue("@CitizenAmandmentId", model.CitizenAmandmentId);
						cmd.Parameters.AddWithValue("@TransactionMessage", model.TransactionMessage);
						cmd.Parameters.AddWithValue("@KioskConfirmation", model.KioskConfirmation);
						cmd.Parameters.AddWithValue("@EmitraCommision", model.EmitraCommision);
						cmd.Parameters.AddWithValue("@TransactionType", model.TransactionType);
						cmd.Parameters.AddWithValue("@ResponseAmount", model.ResponseAmount);
						cmd.Parameters.AddWithValue("@BankIdNumber", model.BankIdNumber);
						cmd.Parameters.AddWithValue("@ReqAmount", model.ReqAmount);
						//cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy);
						//cmd.Parameters.AddWithValue("@UpdatedBy", model.UpdatedBy);
						cmd.Parameters.AddWithValue("@SSOID", model.SSOID);
						//cmd.Parameters.AddWithValue("@UserName", model.UserName);
						cmd.Parameters.AddWithValue("@Status", model.Status);
						cmd.Parameters.AddWithValue("@ReqId", model.ReqId);
						cmd.Parameters.AddWithValue("@DocId", model.DocId);
						cmd.Parameters.AddWithValue("@Bank", model.Bank);
						cmd.Parameters.AddWithValue("@RECEIPTNO", model.RECEIPTNO);
						cmd.Parameters.AddWithValue("@PAYMENTMODEBID", model.PAYMENTMODEBID);
						cmd.Parameters.AddWithValue("@RPPTXNID", model.RPPTXNID);
						cmd.Parameters.AddWithValue("@PaymentType", model.PaymentType);
						cmd.Parameters.AddWithValue("@ServiceName", model.ServiceName);
						cmd.CommandType = CommandType.StoredProcedure;
						result = cmd.ExecuteNonQuery();
					}
				}
			}
			catch (Exception ex)
			{
				new ServiceLog().OrdinaryCitizenVerifyErrorLog("BPPayment", "InsertPaymentTransaction", "Error", "Exception Error", ex.Message.ToString(), ex.InnerException.Message.ToString());
			}
			finally
			{
				Conn.Close();
			}
			return result;
		}

		public void AddUpdateStatusForPrivatePublication(string RequestNumberGenerated, string status, string SSOID, string remarks)
		{
			DataTable dt = new DataTable();
			SqlConnection Conn = null;
			int parentid = 0;
			try
			{				
				using (Conn = new SqlConnection(ClsCommon.connection))
				{
					Conn.Open();
					using (SqlCommand cmd = new SqlCommand("SP_Publisher_INSERT_DOCUMENTSTATUS_PARENT", Conn))
					{
						cmd.Parameters.AddWithValue("@REQUESTSTATUS", status);
						cmd.Parameters.AddWithValue("@CITIZENREQUESTID", RequestNumberGenerated);
						cmd.Parameters.AddWithValue("@CREATEDBY", -1);
						cmd.Parameters.AddWithValue("@UPDATEDBY", -1);
						cmd.Parameters.AddWithValue("@SSOID", SSOID);
						cmd.CommandType = CommandType.StoredProcedure;
						SqlDataAdapter da = new SqlDataAdapter(cmd);
						da.Fill(dt);
						parentid = Convert.ToInt32(dt.Rows[0]["ID"]);
					}
					if (dt != null && dt.Rows.Count > 0)
					{
						DataTable dt1 = new DataTable();
						using (SqlCommand cmd1 = new SqlCommand("SP_Publisher_INSERT_DOCUMENTSTATUS", Conn))
						{
							cmd1.Parameters.AddWithValue("@ParentID", parentid);
							cmd1.Parameters.AddWithValue("@REQUESTSTATUS", status);
							cmd1.Parameters.AddWithValue("@CITIZENREQUESTID", RequestNumberGenerated);
							cmd1.Parameters.AddWithValue("@SSOID", SSOID);
							cmd1.Parameters.AddWithValue("@CREATEDBY", -1);
							cmd1.Parameters.AddWithValue("@UPDATEDBY", -1);
							cmd1.Parameters.AddWithValue("@Reamrks", remarks);
							cmd1.CommandType = CommandType.StoredProcedure;
							SqlDataAdapter da1 = new SqlDataAdapter(cmd1);
							da1.Fill(dt1);
						}
					}
				}
			}
			catch (Exception ex)
			{
				new ServiceLog().OrdinaryCitizenVerifyErrorLog("BPPayment", "AddUpdateStatusForPrivatePublication", "Error", "Exception Error", ex.Message.ToString(), ex.InnerException.Message.ToString());
			}
			finally
			{
				Conn.Close();
			}
		}

	}
}
