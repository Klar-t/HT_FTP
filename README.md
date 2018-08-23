# HT_FTP
#### HT_FTP程序
#### [SVN 代碼地址](https://github.com/Klar-t/HT_FTP)
##### 程序邏輯：
- 从数据库中查询数据 生成CSV文件 上传FTP服务器

##### 程序涉及方法
- 生成CSV文件方法：
  -  System.IO.FileInfo fi=new System.IO.FileInfo 提供建立、複製、刪除、移動和開啟檔案的執行個體
  -  System.IO.FileStream fs = new System.IO.FileStream() 提供使用指定路徑、建立模式和讀取/寫入使用權限，來初始化 System.IO.FileStream 類別的新執行個體
  - fi.Directory.Exists  取得值，指出目錄是否存在
  - Create() 創建文件；
  - StreamWriter（）使用預設編碼方式和緩衝區大小 
  - WriteLine（）；Write（）寫入方法
- FTP 方法：
```
 public void Connect(String path)//连接ftp
        {
            // 根据uri创建FtpWebRequest对象
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
            // 指定数据传输类型
            reqFTP.UseBinary = true;
            // ftp用户名和密码
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
        }
```
 - Upload 方法
 
```
public void Upload(string filename) //上面的代码实现了从ftp服务器上载文件的功能
        {
            FileInfo fileInf = new FileInfo(filename);
            string uri = "ftp://" + ftpServerIP + fileInf.Name;
            Connect(uri);//连接         
            // 默认为true，连接不会被关闭
            // 在一个命令之后被执行
            reqFTP.KeepAlive = false;
            // 指定执行什么命令
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            // 上传文件时通知服务器文件的大小
            reqFTP.ContentLength = fileInf.Length;
            // 缓冲大小设置为kb 
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            // 打开一个文件流(System.IO.FileStream) 去读上传的文件
            FileStream fs = fileInf.OpenRead();
            try
            {

                //使用 HTTP Proxy 時，不支援要求的 FTP 命令。
                //要解决以上问题，只需要在代码中指定
                reqFTP.Proxy = GlobalProxySelection.GetEmptyWebProxy();
                //或
                //reqFTP.Proxy = null;


                // 把上传的文件写入流
                Stream strm = reqFTP.GetRequestStream();
                // 每次读文件流的kb
                contentLen = fs.Read(buff, 0, buffLength);
                // 流内容没有结束
                while (contentLen != 0)
                {
                    // 把内容从file stream 写入upload stream 
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                // 关闭两个流
                strm.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "Upload Error");
                throw ex;
            }
        }
```
 - Delete（） 刪除指定的檔案  刪除本地保存文件
