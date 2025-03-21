using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using OridinayVerifyPaymentStatus.Model;

namespace DMS.Business
{
	public class ServiceLog
	{
		public void OrdinaryCitizenVerifyErrorLog(string ClassName, string functionName,  string Status, string Message, string ExcptionMessage, string InnerMassage)
		{
			SqlConnection Conn = null;
			try
			{
				using (Conn = new SqlConnection(ClsCommon.connection))
				{
					using (SqlCommand cmd = new SqlCommand("sp_insertCitizenOrdinaryVerifyPaymentErrorLog", Conn))
					{
						Conn.Open();
						cmd.CommandType = CommandType.StoredProcedure;
						cmd.Parameters.AddWithValue("@ClassName", ClassName);
						cmd.Parameters.AddWithValue("@functionName", functionName);
						cmd.Parameters.AddWithValue("@Status", Status);
						cmd.Parameters.AddWithValue("@Massgae", Message);
						cmd.Parameters.AddWithValue("@InnerMassage", InnerMassage);
						cmd.Parameters.AddWithValue("@ExceptionType", ExcptionMessage);						
						cmd.ExecuteNonQuery();
					}
				}
			}
			catch (Exception ex)
			{

			}
			finally
			{
				Conn.Close();
			}
		}
	}
}
