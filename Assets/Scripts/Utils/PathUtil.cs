using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PathUtil
{
    /// <summary>
    /// Application.persistentDataPath �ȸ�����Ҫ·�� �ƶ���Ψһһ���ɶ�д�������ļ���
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
