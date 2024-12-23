using System;
using System.IO;
using UnityEngine;
using ResourcesLoad;
using UnityEditor;
using Object = UnityEngine.Object;

namespace ResourcesLoad
{
    public class ResourceManager : MonoBehaviour
    {
        private static ResourceManager instance;

        public delegate void ProcTextAsssetRes(byte[] bytes);

        public static ResourceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    #if UNITY_EDITOR
                    bool Deploy_AB = EditorPrefs.GetBool("Deploy_AB", false);
                    if (Deploy_AB)
                        instance = Util.CreateDonDestroyObj<DeployABResourceManager>("Resource|DeployABResourceManager");
                    else
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
            Init();
        }

        public void LoadAndProcTextAsset(string path,ProcTextAsssetRes proc)
        {
            TextAsset file = Load(path) as TextAsset;
            if (file != null)
            {
                try
                {
                    proc(file.bytes);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
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