using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public class UpLoadFile :Editor
{
    private static StringBuilder _stringBuilder = new StringBuilder(1024);
    private static string StreamingAssetsPath = string.Format("{0}/../streaming", Application.dataPath);
    private static string StreamingAssetsPath2 = string.Format("{0}/../", Application.dataPath);

    [MenuItem("Bundle/UpLoad")]
    static void UpLoadFiles()
    {
        _stringBuilder.Clear();
        PlayerSettings.bundleVersion = (int.Parse(PlayerSettings.bundleVersion) + 1).ToString();
        //_stringBuilder.AppendLine(File.ReadAllText(string.Format("{0}/{1}", StreamingAssetsPath, "version")));
        _stringBuilder.AppendLine(UnityEditor.PlayerSettings.bundleVersion);
        string[] files = Directory.GetFiles(StreamingAssetsPath, "*", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            Debug.Log(files[i]+"000");
            if (files[i].EndsWith(".assetbundle"))
            {
                //上传并记录
                //string md5 = AssetBundleMD5.GetPathMd5(files[i]);
                //_stringBuilder.Append(md5);

                string fileName = files[i].Replace(StreamingAssetsPath, "");
                string[] fileNames = fileName.Split('\\');
                Debug.Log(fileName);
                fileName = "";
                for (int n = 0; n < fileNames.Length - 1; n++)
                {
                    fileName += fileNames[n] + "\\";
                }
                string md5 = GetPathMd5(files[i]);
                fileName += md5;
                _stringBuilder.Append(files[i].Replace(StreamingAssetsPath2, "")).Append(":").AppendLine(md5);
                Debug.Log(files[i]+"        "+fileName);
                FTPUpAndDown.UpLoadFile(files[i], fileName);
            }
        }
        File.WriteAllText(StreamingAssetsPath+ "/version", _stringBuilder.ToString());

        FTPUpAndDown.UpLoadFile(StreamingAssetsPath + "/version", "versionftp");
    }


    private static MD5 md5 = new MD5CryptoServiceProvider();
    public static string GetPathMd5(string path)
    {
        //return ResourceManagerConfig.FormatString("{0}.assetbundle", AssetDatabase.AssetPathToGUID(path));
        //MD5 加密
        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        byte[] md5Byte = md5.ComputeHash(fs);
        fs.Close();
        string str = GetMD5(md5Byte) + ".assetbundle";
        return str;
    }

    public static string GetMD5(byte[] retVal)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < retVal.Length; i++)
        {
            sb.Append(retVal[i].ToString("x2"));//转化成小写的16进制 输出俩位，不足的补0
        }
        return sb.ToString();
    }


}
