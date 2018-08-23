  using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Data.SqlClient;
using System.Xml;

namespace HT
{
    public partial class HT_FTP : Form
    {
        public HT_FTP()
        {
            InitializeComponent();
        }

        string strDir = System.DateTime.Now.ToString("yyyyMMdd");
        string strFileBak = "FILEDIR_AGAIN_Bak";
        string strFiles = "FILEDIR_AGAIN";
        string strSum = "summary";
        string strLog = "LOGDIR";
        string sFiAnge = "FILERANGE";
        string strUrl, strFtpPath, m_path;
        string strFtpUser, strFtpPassword;
        string strDsnPassword, strDsnUser;
        int m_nFtpTimeout;
        string strServerDB;
        string logFile = ".log";
        string logSum = ".txt";
        private OperateDB SFCCONN;
        private string Strcn;
        private string strDB;



        private void HT_FTP_Load(object sender, EventArgs e)
        {
            //获取欲启动进程名
            string strProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
            ////获取版本号 
            //CommonData.VersionNumber = Application.ProductVersion; 
            //检查进程是否已经启动，已经启动则显示报错信息退出程序。 
            if (System.Diagnostics.Process.GetProcessesByName(strProcessName).Length > 1)
            {

                MessageBox.Show("The program is already running......");
                Application.Exit();
                return;
            }

            //調用皮膚
           // skinEngine1.SkinFile = Application.StartupPath + @"Skins\EmeraldColor3.ssk";
            CreateAllFile();

            if (!LoadConfig(Application.StartupPath + "\\config.ini"))
            {
                MessageBox.Show("The configuration file is error......");
                Application.Exit();
                return;
            }

            AddMessage("------------------------------------------------------------------------------------------------------------");
            AddMessage("Start Transfer Time :" + "     " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            timer1.Enabled = true;

        }


        /// <summary>
        /// 創建文件夾
        /// </summary>
        /// <param name="sfile">文件夾名</param>
        private void CreateFile(string sfile)
        {
            if (!Directory.Exists(Application.StartupPath + "\\" + sfile))
            {
                Directory.CreateDirectory(Application.StartupPath + "\\" + sfile);
            }

        }

        /// <summary>
        /// 創建文件夾
        /// </summary>
        /// <param name="sfile">文件夾名</param>
        /// <param name="sfdir">子文件夾名</param>
        private void CreateFileDir(string sfile, string sfdir)
        {
            if (!Directory.Exists(Application.StartupPath + "\\" + sfile))
            {
                Directory.CreateDirectory(Application.StartupPath + "\\" + sfile);
            }
            if (!Directory.Exists(Application.StartupPath + "\\" + sfile + "\\" + sfdir))
            {
                Directory.CreateDirectory(Application.StartupPath + "\\" + sfile + "\\" + sfdir);
            }

        }

        private void CreateAllFile()
        {

            CreateFile(strSum);
            CreateFile(strLog);
            CreateFile(sFiAnge);
            CreateFile(strFiles);
            CreateFile(strFileBak);


            CreateFileDir(strLog, strDir);
            CreateFileDir(strSum, strDir);
            CreateFileDir(strFileBak, strDir);

        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="strpassword">解密前密碼</param>
        /// <returns></returns>
        private static string g_Decode(string strpassword)
        {
            string LStr = string.Empty;
            byte[] Buffer1 = Encoding.ASCII.GetBytes(strpassword);

            for (int i = 0; i < Buffer1.Length; i++)
            {
                int num2 = Convert.ToInt32(Buffer1[i]);
                if ((num2 > 0x36) && (num2 <= 0x7a))
                {
                    num2 -= 0x16;
                }
                else if ((num2 > 0x20) && (num2 <= 0x36))
                {
                    num2 = (100 + num2) - 0x20;
                }
                byte[] Buffer2 = new byte[1] { Convert.ToByte(num2) };
                string text2 = Encoding.ASCII.GetString(Buffer2);
                LStr = LStr + text2;
            }
            return LStr;
        }


        #region readini

        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
            string key, string def, StringBuilder retVal,
            int size, string filePath);//寫INI的API函數的聲明


        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
            string key, string val, string filePath);//讀INI的API函數的聲明

        public static string ReadINI(string iniFileName, string sectionName, string keyName)
        {
            StringBuilder keyValue = new StringBuilder(255);
            int i = GetPrivateProfileString(sectionName, keyName, "", keyValue, 255, iniFileName);
            return (keyValue.ToString().Trim());
        }

        public static void WriteINI(string iniFileName, string sectionName, string keyName, string keyValue)
        {
            WritePrivateProfileString(sectionName, keyName, keyValue, iniFileName);
        }

        #endregion


        /// <summary>
        /// 讀ini 值
        /// </summary>
        /// <param name="KeyName">KeyName</param>
        /// <param name="FieldName">FieldName</param>
        /// <returns>rtn</returns>
        public static string GetStringByKey(string KeyName, string FieldName)
        {
            return ReadINI(Application.StartupPath + "\\config.ini", KeyName, FieldName);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            CreateAllFile();
            createcsvandftp();
                    //FtpStratFile();
           

        }
        private void createcsvandftp()
        {
            //string sql = "select distinct a.skuno as Skuno,lik.codename as CodeName,"
            // + "ltrim(rtrim(m.sysserialno)) as Sysserailno,m.createdate as Createdate,x.lasteditby as TestBy,failureeventpoint as Eventpoint,"
            // + "isnull(b.failcode,'') as FailSymptom,"
            // + "isnull(e.cisco_code,'') as Rootcause,Errcode.codedesc1 as FailureCode,"
            // + "isnull(f.defectcodename,'') as Actioncode,"
            // + "isnull(c.process,'') Process,"
            // + "isnull(repairlink.cserialno,'') Cserialno,"
            // + "isnull(c.location,'') Location,m.Repairby,m.lasteditby,m.lasteditdt as Repairdate,"
            // + "isnull(c.componentcode,'') as Componentcode,"
            // + "isnull(c.vendorcode,'') Vendorcode,"
            // + "isnull(c.datacode,'') as Datacode,"
            // + "isnull(c.lotcode,'') as Lotcode,"
            // + "isnull(c.solution,'') as Solution,"
            // + "isnull(c.description,'') as Description,"
            // + "isnull(c.partdes,'') as Partdes,  m.workorderno AS Workorderno "
            // + "from sfcrepairmain m(nolock) "
            // + "left join mfautotestrecordset x(nolock) on m.sysserialno=x.sysserialno "
            // + "and (m.createdate=dateadd(hour,15,x.testdate) or m.createdate=dateadd(hour,16,x.testdate)) "
            // + "left join mfworkorder a(nolock) on m.workorderno=a.workorderno "
            // + "left join (select sysserialno,createdate,max(failcode) as failcode,max(faillocation) as faillocation "
            // + "from sfcrepairfailcode(nolock) group by sysserialno,createdate) b on m.sysserialno=b.sysserialno and m.createdate=b.createdate "
            // + "left join sfcrepairaction c(nolock) on m.sysserialno=c.sysserialno and m.createdate=c.createdate "
            // + "left join sfcfailurecodeinfo e(nolock) on c.rootcause=e.codename "
            // + "left join rc_defectcode f(nolock) on c.actioncode=f.defectcategory "
            // + "left join mfsysevent g(nolock) on m.sysserialno=g.sysserialno "
            // + "left join sfcrepairpcbalink repairlink(nolock) on c.sysserialno=repairlink.sysserialno "
            // + "and c.createdate=repairlink.createdate and c.location=repairlink.location "
            // + "left join (select distinct codename,codedesc1 from sfcfailurecodeinfo(nolock)) errcode on c.rootcause=errcode.codename "
            // + "left join sfccodelike lik(nolock) on a.skuno=lik.skuno  where lik.skuno in "
            // + "(select CONTROLVALUE from sfccontrol where controlname='N9500SKUNO') "
            // + "and g.productionline in('DA2S1A','DA2S2A') and M.failureeventpoint NOT IN('XRAY')";
            string sql = "EXEC saverepairdata_sp";
            ConnectionDB();
            AddMessage("connect to database");
            DataTable DT= SFCCONN.DoSelectSQL(sql);
            string time =( DateTime.Now.ToString("s").Replace("T","_")).Replace(":","");
            for (int i = 0; i < DT.Rows.Count; i++)
            {
                try
                {
                    string filename = "REPAIR_" + DT.Rows[i]["Sysserailno"].ToString() + "_" + time + ".csv";
                    System.IO.FileInfo fi = new System.IO.FileInfo(Application.StartupPath + "\\" + strFiles + "\\" + filename);
                    if (!fi.Directory.Exists)
                    {
                        fi.Directory.Create();
                    }

                    System.IO.FileStream fs = new System.IO.FileStream(Application.StartupPath + "\\" + strFiles + "\\" + filename, System.IO.FileMode.Create,
                        System.IO.FileAccess.Write);
                    System.IO.StreamWriter writer = new System.IO.StreamWriter(fs, System.Text.Encoding.UTF8);
                    writer.WriteLine("Skuno,CodeName,Sysserailno,TestBy,Eventpoint,FailSymptom,Rootcause,FailureCode,Actioncode,Process,Cserialno,Location,Repairby,lasteditby,Repairdate,Componentcode,Vendorcode,Datacode,Lotcode,Solution,Description,Partdes,Workorderno");
                    writer.Write(CSVStringConvert(DT.Rows[i]["Skuno"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["CodeName"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["Sysserailno"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["TestBy"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["Eventpoint"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["FailSymptom"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["Rootcause"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["FailureCode"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["Actioncode"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["Process"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["Cserialno"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["Location"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["Repairby"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["lasteditby"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["Repairdate"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["Componentcode"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["Vendorcode"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["Datacode"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["Lotcode"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["Solution"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["Description"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["Partdes"].ToString()) + ",");
                    writer.Write(CSVStringConvert(DT.Rows[i]["Workorderno"].ToString()));
                    writer.WriteLine();    
                    writer.Close();
                    AddMessage(DT.Rows[i]["Sysserailno"].ToString() + " " + DT.Rows[i]["Skuno"].ToString());
                    AddMessage("create "+filename+"  success");
                    FtpUpDown ftpUp = new FtpUpDown();
                    ftpUp.FTPUpDown(strUrl, strFtpUser, strFtpPassword);
                    ftpUp.Upload(Application.StartupPath + "\\" + strFiles + "\\" + filename);
                    AddMessage("upload ftp " + filename + "  success");
                    File.Delete(Application.StartupPath + "\\" + strFiles + "\\" + filename);
                    AddMessage("delete " + filename + "  success");

                }
                catch (Exception e)
                {
                    AddMessage(e.Message);
                    continue;
                }
            }
           
        }
        public string CSVStringConvert(string input)
        {
            string output = "\"" + (input == null ? "" : input).Replace("\"", "\"\"") + "\"";
            return output;
        }
       



        private bool LoadConfig(string intfile)
        {
            try
            {
                strUrl = ReadINI(intfile, "ftp", "url");
                if (strUrl == "")
                {
                    AddMessage("ERROR: URL Missing.");
                    return false;
                }
                else
                {
                    int i;
                    i = strUrl.IndexOf("://");
                    if (i < 2)
                    {
                        AddMessage("ERROR: URL Missing.");
                        return false;
                    }
                    else
                    {
                        strUrl = strUrl.Substring(i + 3, strUrl.Length - i - 3);
                        i = strUrl.IndexOf("/");
                        if (i > 0)
                        {
                            strFtpPath = strUrl.Substring(i, strUrl.Length - i);
                        }
                    }
                }
                m_path = ReadINI(intfile, "ftp", "path");
                strFtpUser = ReadINI(intfile, "ftp", "user");
                if (strFtpUser == "")
                {
                    AddMessage("ERROR: No FTP user provided.");
                    return false;
                }
                strFtpPassword = ReadINI(intfile, "ftp", "password");
                if (strFtpPassword == "")
                {
                    AddMessage("ERROR: No password provided for FTP user.");
                    return true;
                }
                else
                {
                    strFtpPassword = g_Decode(strFtpPassword);
                }
                m_nFtpTimeout = Convert.ToInt32(ReadINI(intfile, "ftp", "timeout"));
                if (m_nFtpTimeout < 5000)
                {
                    m_nFtpTimeout = 5000;
                    AddMessage("WARNING: FTP connection timeout must not be less than 5 seconds.");
                }
                strServerDB = ReadINI(intfile, "dsn", "dsn");
                if (strServerDB == "")
                {
                    AddMessage("ERROR: DataBase Missing.");
                    return false;
                }
                strDsnUser = ReadINI(intfile, "dsn", "user");
                if (strDsnUser == "")
                {
                    AddMessage("ERROR: No DB user provided.");
                    return true;
                }
                strDsnPassword = ReadINI(intfile, "dsn", "password");
                if (strDsnPassword == "")
                {
                    AddMessage("ERROR: No DB password provided.");
                    return true;
                }
                else
                {
                    strDsnPassword = g_Decode(strDsnPassword);
                }

                strDB = ReadINI(intfile, "dsn", "server");
                if (strDB == "")
                {
                    AddMessage("ERROR: No DB Server.");
                }
                else
                {
                    strDB = g_Decode(strDB);
                }


                txtURL.Text = "ftp://" + strUrl;
            }
            catch (Exception ex)
            {
                AddMessage("ERROR:" + "" + ex + "");
                return false;
            }
            return true;
        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            createcsvandftp();
           // FtpStratFile();
        }

        /// <summary>
        /// 寫日記 
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="pMsg"></param>
        void WriteLog(string FileName, string pMsg)
        {
            FileInfo MyFile = new FileInfo(@FileName);
            StreamWriter sw;
            if (!MyFile.Exists)
            {
                sw = MyFile.CreateText();
            }
            else
            {
                sw = new StreamWriter(@FileName, true, System.Text.Encoding.UTF8);
            }

            sw.WriteLine(pMsg);
            sw.Close();
        }


        void AddMessage(string pMsg)
        {
            this.libMssage.Items.Add(pMsg);
            libMssage.SelectedIndex = libMssage.Items.Count - 1;
            WriteLog(pMsg);
        }

        public void CopyFiles(string varFromDirectory, string varToDirectory)
        {
            //实现从一个目录下完整拷贝到另一个目录下。 
            Directory.CreateDirectory(varToDirectory);
            if (!Directory.Exists(varFromDirectory))
            {
                AddMessage("Sorry, Do you want to copy the directory does not exist ");
                return;
            }

            string[] directories = Directory.GetDirectories(varFromDirectory);//取文件夹下所有文件夹名，放入数组； 
            if (directories.Length > 0)
            {
                foreach (string d in directories)
                {
                    CopyFiles(d, varToDirectory + d.Substring(d.LastIndexOf("\\")));//递归拷贝文件和文件夹 
                }
            }

            string[] files = Directory.GetFiles(varFromDirectory);//取文件夹下所有文件名，放入数组； 
            if (files.Length > 0)
            {
                foreach (string s in files)
                {
                    File.Copy(s, varToDirectory + s.Substring(s.LastIndexOf("\\")), true);
                }
            }
        }



        private void WriteLog(string logMsg)
        {
            StreamWriter objWriter = null;
            string mylog;

            mylog = Application.StartupPath + "\\" + strLog + "\\" + strDir + "\\" + strDir + logFile;

            if (!System.IO.File.Exists(mylog))
            {
                objWriter = new System.IO.StreamWriter(System.IO.File.Open(mylog, System.IO.FileMode.OpenOrCreate));
            }
            else
            {
                objWriter = new System.IO.StreamWriter(System.IO.File.Open(mylog, System.IO.FileMode.Append));
            }

            objWriter.WriteLine(logMsg);
            objWriter.Flush();
            objWriter.Close();
        }

        #region ConnDB
        /// <summary>
        /// 數據庫連接
        /// </summary>
        /// <param name="x"></param>
        private void ConnectionDB()
        {
            Strcn = "server=" + strDB + "; uid= " + strDsnUser + ";pwd=" + strDsnPassword + ";database=" + strServerDB + "";
            SqlConnection sfcStrcn = new SqlConnection(Strcn);
            SFCCONN = new OperateDB(sfcStrcn);
        }

        #endregion


        private void WriteSum(string logMsg)
        {
            StreamWriter objWriter = null;
            string mylog;

            mylog = Application.StartupPath + "\\" + strSum + "\\" + strDir + "\\" + strSum + logSum;

            if (!System.IO.File.Exists(mylog))
            {
                objWriter = new System.IO.StreamWriter(System.IO.File.Open(mylog, System.IO.FileMode.OpenOrCreate));
            }
            else
            {
                objWriter = new System.IO.StreamWriter(System.IO.File.Open(mylog, System.IO.FileMode.Append));
            }

            objWriter.WriteLine(logMsg);
            objWriter.Flush();
            objWriter.Close();
        }

        private void txtURL_TextChanged(object sender, EventArgs e)
        {

        }

       

    }
}
