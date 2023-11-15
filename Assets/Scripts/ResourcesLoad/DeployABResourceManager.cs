using System.Collections.Generic;
using UnityEngine;

namespace ResourcesLoad
{
    public class DeployABResourceManager:ResourceManager
    {
        private Dictionary<int, Object> _resource = new Dictionary<int, Object>();
        private Dictionary<int, AssetBundle> _bundles = new Dictionary<int, AssetBundle>();
        
        /// <summary>
        /// 所有包的加载路径
        /// </summary>
        private Dictionary<int, string> _bundle_index = new Dictionary<int, string>();
        
        /// <summary>
        /// 包之间的依赖关系
        /// </summary>
        private AssetBundleManifest _manifest = null;

        public override void Init()
        {
            base.Init();
           AssetBundle index_bundle = _LoadBundle(ResourceManagerConfig.kIndexFileName);
           LoadIndexFile(index_bundle.LoadAsset<TextAsset>(ResourceManagerConfig.kIndexFileName));
           index_bundle.Unload(false);
           index_bundle = null;
           
           Debug.Log("begin to load mainfest");
           
           AssetBundle manifest_bundle = _LoadBundle("Bundles");
           _manifest = manifest_bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
           manifest_bundle.Unload(false);
           manifest_bundle = null;
            
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

        private void LoadIndexFile(TextAsset index_file)
        {
            Debug.Log("begin to load index file");
            string[] lines = index_file.text.Split('\n');
            char[] trim = new char[] {'\r', '\n'};
            if (lines != null && lines.Length > 0)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim(trim);
                    if (string.IsNullOrEmpty(line))
                        continue;
                    string[] pair = line.Split(':');
                    if (pair != null && pair.Length == 2)
                    {
                        _bundle_index.Add(pair[0].GetHashCode(), pair[1]);
                    }
                }
            }

            if (_bundle_index.Count != 0)
                Debug.LogFormat("index file loaded final count is {0}",_bundle_index.Count);
            else
                Debug.LogError("index file failed to load");
        }

        public class AssetLoadInfo
        {
            public string assetPath;
            public string mainBundle;
            public string[] dependencies;//依赖
        }

        private AssetLoadInfo GetAssetLoadInfo(string asset_path)
        {
            AssetLoadInfo rt = new AssetLoadInfo();
            rt.assetPath = asset_path;
            rt.mainBundle = GetAssetBundleFileName(asset_path);
            rt.dependencies = _manifest.GetAllDependencies(rt.mainBundle);
            return rt;
        }

        private string GetAssetBundleFileName(string asset_path)
        {
            int asset_hash = asset_path.GetHashCode();
            string bundle_name;
            if (_bundle_index.TryGetValue(asset_hash,out bundle_name))
            {
                return bundle_name;
            }
            return string.Empty;
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
            var assetLoadInfo = GetAssetLoadInfo(asset_path);

            var mainBundle = _LoadBundleSync(assetLoadInfo.mainBundle.ToLower());
            obj = mainBundle.LoadAsset(asset_path);
            _resource.Add(path_hash, obj);
            return obj;
        }

        public override bool LoadLuaScript(string asset_name, out byte[] content)
        {
            string path = RSPathUtil.LuaScript(asset_name);
            Debug.Log(path);
            var asset = Load<TextAsset>(path);
            content = asset != null ? asset.bytes : null;
            return asset != null;
        }

        private AssetBundle _LoadBundleSync(string bundle_name)
        {
            AssetBundle bundle = _GetBundle(bundle_name);
            if (bundle == null)
            {
                var bd_hash = bundle_name.GetHashCode();

                string bundle_load_path = _FormatBundleLoadPath(bundle_name);
                Debug.Log(bundle_load_path+"    "+bundle_name);
                bundle = AssetBundle.LoadFromFile(bundle_load_path);

                if (bundle != null)
                {
                    _bundles.Add(bd_hash,bundle);
                }

            }
            else
            {
                Debug.LogFormat("Bundle is null :"+bundle_name);
            }
            return bundle;
        }

        private AssetBundle _GetBundle(string bundle_name)
        {
            AssetBundle rt = null;
            var bd_hush = bundle_name.GetHashCode();
            _bundles.TryGetValue(bd_hush, out rt);
            return rt;
        }

        private string _FormatBundleLoadPath(string bundle_name)
        {
            bundle_name = ResourceManagerConfig.FormatString("/Bundles/{0}", bundle_name);
            string bundle_load_path = ResourceManagerConfig.PersistentDataPath + bundle_name;
            return bundle_load_path;
        }

    }
}