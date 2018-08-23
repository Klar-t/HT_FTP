using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;

namespace HT
{
    class OperateDB
    {
        public OperateDB()
        {
            //
            // TODO: 在此加入建構函式的程式碼
            //
        }
       
        private SqlConnection sCn = new SqlConnection();
        private SqlCommand sCmd = new SqlCommand();
        private SqlDataAdapter sAdp = new SqlDataAdapter();
        public OperateDB(SqlConnection cn)
        {
            //
            // TODO: 在此加入建構函式的程式碼
            try
            {

                sCn = cn;
                if (sCn.State.ToString().ToUpper() == "CLOSED")
                {
                    sCn.Open();
                }

            }
            catch (Exception ex)
            {
                string strErroe = ex.Message;
            }
            //
        }


        public void OperateDBClose()
        {
            //
            // TODO: 在此加入建構函式的程式碼
            try
            {


                if (sCn.State.ToString().ToUpper() == "OPEN")
                {
                    sCn.Close();
                }

            }
            catch (Exception ex)
            {
                string strErroe = ex.Message;
            }
            //
        }

        public System.Data.DataTable DoSelectSQL(string strSQL)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            try
            {
                dt = new System.Data.DataTable();
                sCmd.Connection = sCn;
                sCmd.CommandText = strSQL;
                sAdp.SelectCommand = sCmd;

                sAdp.Fill(dt);

            }
            catch (Exception ex)
            {
                string strError = ex.Message;
            }
            return (dt);
        }
        public System.Data.DataTable DoUpdateSQL(string strSQL)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            try
            {
                dt = new System.Data.DataTable();
                sCmd.Connection = sCn;
                sCmd.CommandText = strSQL;
                sAdp.SelectCommand = sCmd;

                sAdp.Update(dt);

            }
            catch (Exception ex)
            {
                string strError = ex.Message;
            }
            return (dt);
        }

        public string DoScalar(string strSQL)
        {
            Object o = new object();
            try
            {

                sCmd.Connection = sCn;
                sCmd.CommandText = strSQL;
                o = sCmd.ExecuteScalar();

            }
            catch (Exception ex)
            {
                string strError = ex.Message;
            }
            return (o.ToString());
        }


        public void DoNonQuerySQL(string strSQL)
        {
            try
            {
                System.Data.DataTable dt = new System.Data.DataTable();
                sCmd.Connection = sCn;
                sCmd.CommandText = strSQL;
                sCmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void BeginTrans()
        {
            try
            {
                sCmd.Transaction = sCn.BeginTransaction();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void Commit()
        {
            try
            {
                sCmd.Transaction.Commit();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void RollBack()
        {
            try
            {
                sCmd.Transaction.Rollback();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
