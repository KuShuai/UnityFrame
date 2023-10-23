using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PathUtil
{
    /// <summary>
    /// Application.persistentDataPath 热更新重要路径 移动段唯一一个可读写操作的文件夹
    /// </summary>
    public static string PersistentDataPath = Application.persistentDataPath + "/";

    public static string AssetPath(string uri)
    {
#if USE_RESOURCE
        return uri;
#else
        string persistPath = PersistentDataPath + uri;
        if (File.Exists(persistPath))
        {
            return persistPath;
        }
        return uri;
#endif
    }
}
