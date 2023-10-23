using System;
using System.IO;
using UnityEngine;
using ResourcesLoad;
using Object = UnityEngine.Object;

namespace ResourcesLoad
{
    public class ResourceManager : MonoBehaviour
    {
        private static ResourceManager instance;

        public static ResourceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    #if UNITY_EDITOR
                    instance = Util.CreateDonDestroyObj<DeployResourceManager>("Resource|DeployResourceManager");
                    #else
                    instance = Util.CreateDonDestroyObj<DeployABResourceManager>("Resource|DeployABResourceManager");
                    #endif
                }
                return instance;
            }
        }

        private void Awake()
        {
        }

        public virtual void Init() { }

        public T Load<T>(string asset_name) where T : Object
        {
            Object obj = Load(asset_name);
            return obj as T;
        }

        public virtual Object Load(string asset_name)
        {
            return null;
        }
        public virtual void Unload(string asset_path) { }
        
        public virtual bool LoadLuaScript(string asset_name,out byte[] content)
        {
            content = null;
            return false;
        }
    }
}