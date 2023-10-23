using System;
using System.Text;
using UnityEngine;

namespace ResourcesLoad
{
    public class ResourceManagerConfig
    {
        #if UNITY_EDITOR
            public static string PersistentDataPath = String.Format("{0}/../data", Application.dataPath);//服务器资源下载路径，资源加载路径
            public static string StreamingAssetsPath = String.Format("{0}/../streaming", Application.dataPath);//本地资源打包路径
        #else
            public static string PersistentDataPath = Application.persistentDataPath;
            public static string StreamingAssetsPath = Application.streamingAssetsPath;
        #endif

        
        public const string kIndexFileName = @"index";
        
        public static StringBuilder _stringBuilder = new StringBuilder(1024);
        
        public static string FormatString(string format,object arg0)
        {
            _stringBuilder.Clear();
            return _stringBuilder.AppendFormat(format, arg0).ToString();
        }
        
        public static string FormatString(string format,object arg0,object arg1)
        {
            _stringBuilder.Clear();
            return  _stringBuilder.AppendFormat(format,arg0,arg1).ToString();
        }
        
        public static string FormatString(string format,object arg0,object arg1,object arg2)
        {
            _stringBuilder.Clear();
            return _stringBuilder.AppendFormat(format, arg0, arg1, arg2).ToString();
        }
        
        public static string FormatString(string format,params object[] args)
        {
            _stringBuilder.Clear();
            return _stringBuilder.AppendFormat(format, args).ToString();
        }
    }
}