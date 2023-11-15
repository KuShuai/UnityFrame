using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ResourcesLoad;
using UnityEngine;
using XLua;

public class MainStart : MonoBehaviour
{
    private void Awake()
    {
        UIManager.Instance.Init();
        //ResourceManager.Instance.Init();
        LuaScriptManager.Instance.Init();
        //UIManager.OpenUI("", null);
    }
    private long totalDownLoadLength;
    private long enDownLoadLength;

    public void RefreshDownLoadLength(long param)
    {
        if (param != 0 && totalDownLoadLength != 0)
        {
            enDownLoadLength += param;
            float sliderValue = enDownLoadLength * 1.0f / totalDownLoadLength;
            //text.text = enDownLoadLength + "/" + totalDownLoadLength+"("+ (sliderValue * 100).ToString("#0.00") + "%)";
            //slider.value = sliderValue;
        }
    }

    Dictionary<string, string> needDownLoad = new Dictionary<string, string>();
    Dictionary<string, string> localBundle = new Dictionary<string, string>();


    string localVersion = "0";
    string serverVersion = "0";

    private void Start()
    {
        //版本验证
        //StartCoroutine(Start1());
    }

    IEnumerator Start1()
    {

#if UNITY_EDITOR
        if (!File.Exists(Application.streamingAssetsPath + "\\version"))
            File.Create(Application.streamingAssetsPath + "\\version").Close();
#else
        if (!File.Exists(Application.persistentDataPath + "\\version"))
            File.Create(Application.persistentDataPath + "\\version").Close();
#endif
        Debug.LogError(File.Exists(Application.streamingAssetsPath + "\\version") + "==========");
        localBundle.Clear();

#if UNITY_EDITOR
        string[] localVersionInfo = File.ReadAllLines(Application.streamingAssetsPath + "\\version");
#else
        string[] localVersionInfo = File.ReadAllLines(Application.persistentDataPath + "\\version");
#endif
        if (localVersionInfo.Length > 0) {
            localVersion = localVersionInfo[0];
            for (int i = 1; i < localVersionInfo.Length; i++)
            {
                string[] m = localVersionInfo[i].Split(':');
                if (m.Length == 2)
                    localBundle.Add(m[0], m[1]);
            }
        }
#if UNITY_EDITOR
        if (FTPUpAndDown.FtpDownLoadFile("versionftp", Application.streamingAssetsPath + "\\temp") && File.Exists(Application.streamingAssetsPath + "\\temp"))
        {
            string[] tempStream = File.ReadAllLines(Application.streamingAssetsPath + "\\temp");
#else
        if (FTPUpAndDown.FtpDownLoadFile("versionftp", Application.persistentDataPath + "\\temp") && File.Exists(Application.persistentDataPath + "\\temp"))
        {
            string[] tempStream = File.ReadAllLines(Application.persistentDataPath + "\\temp");
#endif
            if (tempStream.Length > 0)
            {
                serverVersion = tempStream[0];
                if (serverVersion == localVersion)
                    Debug.LogError("版本相同");
                else if (int.Parse(serverVersion) > int.Parse(localVersion))
                {
                    Debug.LogError("版本需要更新");
                    //对比版本内容记录更新内容
                    needDownLoad.Clear();
                    for (int i = 1; i < tempStream.Length; i++)
                    {
                        Debug.LogError(tempStream[i]);
                        string[] m = tempStream[i].Split(':');
                        if (m.Length == 2)
                        {
                            if (!localBundle.ContainsKey(m[0]))
                            {
                                needDownLoad.Add(m[0], m[1]);
                            }
                            else if (localBundle[m[0]] != m[1])
                            {
                                needDownLoad[m[0]] = m[1];
                            }
                        }
                    }
                }
                else
                    Debug.LogError("版本错误");
            }

            File.Delete(Application.streamingAssetsPath + "\\temp");


            ShowUpdataInfo();
            yield return new WaitForSeconds(1);
            enDownLoadLength = 0;
            RefreshDownLoadLength(0);
            //更新需要更新的内容
            foreach (var item in needDownLoad)
            {
                yield return new WaitForSeconds(1);
                Debug.LogError(" ===== needDownLoad:" + item.Key + ":" + FTPUpAndDown.FtpGetFileSize(item.Value));
                //string[] info = needDownLoad[i].Split(':');
                string[] fordler = item.Key.Split('\\');

#if UNITY_EDITOR
                string ss = Application.streamingAssetsPath;
#else
              string ss = Application.persistentDataPath;
#endif
                for (int i = 0; i < fordler.Length - 1; i++)
                {
                    if (string.IsNullOrEmpty(fordler[i]))
                        continue;
                    ss += "\\" + fordler[i];
                    if (!Directory.Exists(ss))
                    {
                        Directory.CreateDirectory(ss);
                    }
                }
#if UNITY_EDITOR

                string loclPath = Application.streamingAssetsPath + '\\' + item.Key;
#else
            string loclPath = Application.persistentDataPath + '\\' + item.Key;
#endif

                if (FTPUpAndDown.FtpDownLoadFile(item.Value, loclPath))
                {
                    if (!localBundle.ContainsKey(item.Key))
                        localBundle.Add(item.Key, string.Empty);
                    localBundle[item.Key] = item.Value;
                }
            }
            //刷新version文件
            StringBuilder versionContent = new StringBuilder();
            versionContent.AppendLine(serverVersion);
            foreach (var item in localBundle)
            {
                versionContent.Append(item.Key).Append(":").AppendLine(item.Value);
            }
#if UNITY_EDITOR
            File.WriteAllText(Application.streamingAssetsPath + "\\version", versionContent.ToString());
#else
        File.WriteAllText(Application.persistentDataPath + "\\version", versionContent.ToString());
#endif
        }
        else
        {
            Debug.LogError("未找到服务器版本文件");
        }

        //ABResourceManager.Instance.Init();
        //GameObject.Instantiate(ABResourceManager.Instance.Load("UI/Canvas"));
    }
    void ShowUpdataInfo()
    {
        long alllength = 0;
        Dictionary<string, long> allNeedLoadInfo = new Dictionary<string, long>();
        foreach (var item in needDownLoad)
        {
            long length = FTPUpAndDown.FtpGetFileSize(item.Value.Replace('\\', '/'));
            allNeedLoadInfo.Add(item.Key, length);
            alllength += length;
        }
        totalDownLoadLength = alllength;
    }


}
