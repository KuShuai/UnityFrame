using System;
using System.IO;
using System.Collections;
using System.Configuration;

namespace ExcelToFBS {
    public class Config {
        private  IDictionary _dict;

        public Config(string section) {
            _dict = (IDictionary)ConfigurationManager.GetSection(section);
            if (null == _dict)
                return;

            string[] keys = new string[_dict.Count];
            _dict.Keys.CopyTo(keys, 0);
            foreach(string key in keys) {
                Replace(key);
            }
        }

        public string Replace(string key) {
            string str = (string)_dict[key];
            int p0 = 0;
            int p1 = -1;
            bool change = false;
            while(-1 != (p1 = str.IndexOf('{', p0))) {
                int p2 = FindPair(str, '{', p1 + 1);
                string innerKey = p2 - p1 - 1 > 0 ? str.Substring(p1 + 1, p2 - p1 -1) : null;
                if(null != innerKey) {
                    string innerStr = Replace(innerKey);
                    str = str.Substring(0, p1) + innerStr + str.Substring(p2 + 1);
                    p0 = p1 + innerStr.Length;
                    change = true;
                }
                else {
                    p0 = p1 + 1;
                }
            }

            if (change) {
                _dict[key] = str;
            }

            return str;
        }

        public int FindPair(string s, char p0, int i) {
            char p1 = ' ';
            switch (p0) {
                case '{':
                    p1 = '}';
                    break;
                case '(':
                    p1 = ')';
                    break;
                case '[':
                    p1 = ']';
                    break;
            }

            int depth = 0;
            while (i < s.Length) {
                if (s[i] == p1) {
                    if (depth == 0) {
                        return i;
                    }
                    else {
                        depth--;
                    }
                }
                else if (s[i] == p0) {
                    depth++;
                }
                i++;
            }

            return -1;
        }

        public string this[string key] {
            get { return (string)_dict[key]; }
            set { _dict[key] = value; }
        }
    }

    public class AppConfigs {
        private static AppConfigs _instance = null;
        public static AppConfigs Instance {
            get {
                if (null == _instance) {
                    _instance = new AppConfigs();
                }
                return _instance;
            }
        }

        private Config _config;

        private string _curBase;
        private string _appBase;

        /// <summary>
        /// FBS文件namespace
        /// </summary>
        public string FbsNameSpace;

        /// <summary>
        /// 原始excel表配置路径
        /// </summary>
        public string ExcelPath;

        /// <summary>
        /// 根据excel配置标生成的fbs文件存储路径
        /// </summary>
        public string FBSClientPath;
        public string FBSServerPath;

        /// <summary>
        /// 生成代码文件的路径
        /// </summary>
        public string GenClientCodePath;
        public string GenServerCodePath;

        public string ClientTypePath;

        /// <summary>
        /// 客户端生成的配置代码路径
        /// </summary>
        public string ClientCodePath;

        /// <summary>
        /// 客户端数据文件
        /// </summary>
        public string ClientDataPath;

        /// <summary>
        /// 服务器端生成的配置代码路径
        /// </summary>
        public string ServerCodePath;

        /// <summary>
        /// 服务器数据文件
        /// </summary>
        public string ServerDataPath;

        /// <summary>
        /// flatbuffer可执行文件路径
        /// </summary>
        public string FlatC;

        /// <summary>
        /// 动态编译C#需要引用的dll路径
        /// </summary>
        public string CompileRefPath;

        public AppConfigs() {
            _curBase = Path.GetFullPath("./");
            _appBase = AppDomain.CurrentDomain.BaseDirectory;

            _config = new Config("Paths");

            ExcelPath = GetFullPath(_config["Excel"]);
            FBSClientPath = GetFullPath(_config["FBSClient"]);
            FBSServerPath = GetFullPath(_config["FBSServer"]);
            FbsNameSpace = _config["NameSpace"];
            GenClientCodePath = GetFullPath(_config["GenClientCode"]);
            GenServerCodePath = GetFullPath(_config["GenServerCode"]);
            ClientTypePath = GetFullPath(_config["ClientType"]);
            ClientCodePath = GetFullPath(_config["ClientCode"]);
            ClientDataPath = GetFullPath(_config["ClientData"]);
            ServerCodePath = GetFullPath(_config["ServerCode"]);
            ServerDataPath = GetFullPath(_config["ServerData"]);
            FlatC = GetFullPath(_config["FlatC"]);
            CompileRefPath = GetFullPath(_config["CompileRef"]);
        }

        private string GetFullPath(string path) {
            return Path.GetFullPath(_appBase + path);
        }
    }
}