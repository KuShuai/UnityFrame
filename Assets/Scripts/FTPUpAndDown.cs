using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

public class FTPUpAndDown 
{
    public static void UpLoadFile(string filePath,string fileName)
    {
        if (fileName.StartsWith("\\"))
            fileName = fileName.Substring(1, fileName.Length - 1);

        //新建文件夹
        string[] files = fileName.Replace("\\", "/").Split('/');
        for (int i = 0; i < files.Length-1; i++)
        {
            string file = "";
            for (int y = 0; y < i+1; y++)
            {
                file += "/" + files[y];
            }
            if (!string.IsNullOrEmpty(file))
            {
                if (!FtpMakeDir(file))
                {
                    return;
                }
            }
        }

        FtpWebRequest req = WebRequest.Create(new Uri("ftp://192.168.3.127:1321/" + fileName)) as FtpWebRequest;
        Debug.LogError("ftp://192.168.3.127:1321/" + fileName);
        NetworkCredential n = new NetworkCredential("Administrator", "123456");
        req.Credentials = n;
        req.Proxy = null;
        req.KeepAlive = false;
        req.Method = WebRequestMethods.Ftp.UploadFile;
        req.UseBinary = true;
        Stream upLoadStream = req.GetRequestStream();

        try
        {
            using (FileStream file = File.OpenRead(filePath))
            {
                byte[] bytes = new byte[2048];
                int contentLength = file.Read(bytes, 0, bytes.Length);

                while (contentLength != 0)
                {
                    upLoadStream.Write(bytes, 0, contentLength);
                    contentLength = file.Read(bytes, 0, bytes.Length);
                }

                file.Close();
                upLoadStream.Close();
            }

            Debug.Log("上传成功");
        }
        catch (Exception)
        {
        }
    }
    /// <summary>
    /// 创建文件夹
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private static bool FtpMakeDir(string fileName)
    {
        if (fileName.StartsWith("/"))
        {
            fileName = fileName.Remove(0, 1);
        }
        if (FtpDirExist(fileName))
        {
            return true;
        }
        string ftpPath = "ftp://192.168.3.127:1321/" + fileName;
        FtpWebRequest req = WebRequest.Create(ftpPath) as FtpWebRequest;

        NetworkCredential n = new NetworkCredential("Administrator", "123456");
        req.Credentials = n;

        req.Method = WebRequestMethods.Ftp.MakeDirectory;
        //req.EnableSsl = true;//false时，所有的数据和命令都会是明文
        try
        {
            req.UsePassive = true;//获取或设置客户端应用程序的数据传输过程的行为。
            req.UseBinary = true;
            req.KeepAlive = false;
            FtpWebResponse response = (FtpWebResponse)req.GetResponse();
            response.Close();
        }
        catch (Exception ex)
        {
            Debug.LogError("FTP 创建文件错误" + ftpPath + "||" + ex.GetType() + "|" + ex.Message);
            req.Abort(); //终止异步FTP操作
            return false;
        }

        return true;
    }

    private static bool FtpDirExist(string fileName)
    {
        if (fileName.StartsWith("/"))
        {
            fileName = fileName.Remove(0, 1);
        }
        string[] paths = fileName.Split('/');
        fileName = "";
        for (int i = 0; i < paths.Length - 1; i++)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                fileName += "/";
            }
            fileName += paths[i];
        }
        string folderName = paths[paths.Length - 1];
        string ftpPath = "ftp://192.168.3.127:1321/" + fileName;
        FtpWebRequest req = WebRequest.Create(ftpPath) as FtpWebRequest;

        NetworkCredential n = new NetworkCredential("Administrator","123456");
        req.Credentials = n;

        req.Method = WebRequestMethods.Ftp.ListDirectory;//用于获取FTP服务器上的文件的简短列表
        //ListDirectoryDetails "05-08-2023  04:18PM       <DIR>          Bundles\r\n"
        //ListDirectory "Bundles\r\n"
        //req.EnableSsl = true;//false时，所有的数据和命令都会是明文
        try
        {
            req.UsePassive = true;//获取或设置客户端应用程序的数据传输过程的行为。
            req.UseBinary = true;
            req.KeepAlive = false;

            FtpWebResponse response = req.GetResponse() as FtpWebResponse;
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string line = reader.ReadToEnd();
            if (line == null)
            {
                Debug.Log("不存在");
                return false;
            }
            else if (line.IndexOf(folderName) > -1)
            {
                Debug.Log("存在");
                return true;
            }
            else
            {
                Debug.Log(line);
                Debug.LogError("目录不存在" + ftpPath + "FoderName" + folderName);
            }
            response.Close();
        }
        catch (Exception ex)
        {
            Debug.LogError("FTP 目录异常" + ftpPath + "||" + ex.GetType() + "|" + ex.Message);
            req.Abort(); //终止异步FTP操作
            return false;
        }

        return false;
    }
    public static bool FtpDownLoadFile(string fileName, string localPath)
    {
        if (fileName.StartsWith("/") || fileName.StartsWith("\\"))
        {
            fileName = fileName.Remove(0, 1);
        }

        FtpWebRequest req = FtpWebRequest.Create(new Uri("ftp://192.168.3.127:1321/" + fileName)) as FtpWebRequest;
        //Debug.LogWarning("download file :ftp://192.168.3.127:1321/" + fileName);
        NetworkCredential n = new NetworkCredential("Administrator", "123456");
        req.Credentials = n;
        req.Proxy = null;
        req.KeepAlive = false;
        req.Method = WebRequestMethods.Ftp.DownloadFile;
        req.UseBinary = true;
        FtpWebResponse res = req.GetResponse() as FtpWebResponse;
        Stream downLoadStream = res.GetResponseStream();

        try
        {
            using (FileStream file = File.Create(localPath))
            {
                byte[] bytes = new byte[2048];
                int contentLength = downLoadStream.Read(bytes, 0, bytes.Length); ;
                while (contentLength != 0)
                {
                    Debug.Log("=====!"+contentLength);
                    //////////////////////////////
                    //if (Main.Instance != null)
                    //    Main.Instance.RefreshDownLoadLength(contentLength);
                    file.Write(bytes, 0, contentLength);
                    contentLength = downLoadStream.Read(bytes, 0, contentLength);
                }

                file.Close();
                downLoadStream.Close();
            }
            Debug.Log("下载成功" + fileName + "   loacalPath:" + localPath);
            return true;
        }
        catch (Exception e)
        {
            Debug.Log("下载失败" + fileName + "error:" + e.Message);
        }
        return false;
    }

    public static List<long> FtpGetFileSize(List<string> fileName) {
        List<long> rt = new List<long>();
        for (int i = 0; i < fileName.Count; i++)
        {
            rt.Add(FtpGetFileSize(fileName[i]));
        }
        return rt;
    }

    public static long FtpGetFileSize(string fileName)
    {
        if (fileName.StartsWith("/") || fileName.StartsWith("\\"))
        {
            fileName = fileName.Remove(0, 1);
        }
        try
        {
            fileName = "ftp://192.168.3.127:1321/" + fileName;
            FtpWebRequest req = FtpWebRequest.Create(new Uri(fileName)) as FtpWebRequest;
            NetworkCredential n = new NetworkCredential("Administrator", "123456");
            req.Credentials = n;
            req.Proxy = null;
            req.KeepAlive = false;
            req.Method = WebRequestMethods.Ftp.GetFileSize;
            req.UseBinary = true;
            FtpWebResponse res = req.GetResponse() as FtpWebResponse;
            Stream downLoadStream = res.GetResponseStream();
            long size = res.ContentLength;

            downLoadStream.Close();
            res.Close();
            return size;
        }
        catch (Exception e)
        {
            Debug.LogErrorFormat("Get File {0} Size Error {1}", fileName, e.Message);
            return 0;
        }

    }
}
