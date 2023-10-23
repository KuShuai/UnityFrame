using System.Collections.Generic;
using System.IO;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.VersionControl;
#endif
using UnityEngine;

namespace ResourcesLoad
{
    public class DeployResourceManager:ResourceManager
    {
        private Dictionary<int, Object> _resource = new Dictionary<int, Object>();
        
        public override void Init()
        {
            base.Init();
        }

        private AssetBundle _LoadBundle(string bundle_name)
        {
            bundle_name = ResourceManagerConfig.FormatString("Bundles/{0}",bundle_name);
            string bundle_load_path = ResourceManagerConfig.FormatString("{0}/{1}", ResourceManagerConfig.StreamingAssetsPath, bundle_name);
            if (string.IsNullOrEmpty(bundle_load_path))
            {
                Debug.LogErrorFormat("get load bundle path failed :{0}",bundle_name);
                return null;
            }

            var bundle = AssetBundle.LoadFromFile(bundle_load_path);
            if (bundle == null)
            {
                Debug.LogErrorFormat("load AsserBundle failed :{0}",bundle_load_path);
                return null;
            }

            return bundle;
        }

        
        public override Object Load(string asset_path)
        {
            Object obj = null;
            int path_hash = asset_path.GetHashCode();
            if (_resource.TryGetValue(path_hash,out obj))
            {
                if (obj == null)
                    _resource.Remove(path_hash);
                else
                    return obj;
            }

            string full_path = GetAssetDataFileName(asset_path);
            if (full_path == null)
            {
                Debug.LogErrorFormat("get assetdatabase name failed :{0}",full_path);
                return null;
            }

#if UNITY_EDITOR
            obj = AssetDatabase.LoadAssetAtPath(full_path, typeof(UnityEngine.Object));
#endif
            if (obj == null)
            {
                Debug.LogErrorFormat("Load asset failed from assetdatabasel :{0}",full_path);
                return null;
            }
            _resource.Add(path_hash, obj);
            return obj;
        }
        
        private string GetAssetDataFileName(string asset_path)
        {
            string result = string.Empty;
            int fs_pos = asset_path.LastIndexOf("/");
            if (fs_pos == -1)
                return result;

            string asset_folder = asset_path.Remove(fs_pos, asset_path.Length - fs_pos);
            string asset_name = asset_path.Substring(fs_pos + 1, asset_path.Length - fs_pos - 1);

            string search_path = string.Format("Assets/AssetBundleResources/{0}/", asset_folder);

            if (!Directory.Exists(search_path))
                return result;

            string[] files = Directory.GetFiles(search_path, string.Format("{0}*.*", asset_name), SearchOption.TopDirectoryOnly);

            foreach (var file_name in files)
            {
                string name = Path.GetFileNameWithoutExtension(file_name);
                if (asset_name == name)
                {
                    string ext = Path.GetExtension(file_name);
                    return string.Format("Assets/AssetBundleResources/{0}{1}", asset_path, ext);
                }
            }

            return result;
        }

        public override bool LoadLuaScript(string asset_name, out byte[] content)
        {
            //加载本地lua文件
            string full_path =
                ResourceManagerConfig.FormatString("{0}/../XLua/{1}.lua", Application.dataPath, asset_name);
            if (File.Exists(full_path))
            {
                Debug.Log(full_path);
                content = File.ReadAllBytes(full_path);
                return true;
            }

            content = null;
            return false;
        }


    }
}