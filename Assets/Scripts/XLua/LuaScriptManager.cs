using System;
using ResourcesLoad;
using UnityEngine;
using UnityEngine.Rendering;

namespace XLua
{
    public class LuaScriptManager :Singleton<LuaScriptManager>,ISingleton
    {
        private LuaEnv _lua = null;
        
        private LuaFunction get_uiconfig;
        public void SingletonInit()
        {
            
        }

        public void Init()
        {
            _lua = new LuaEnv();
            _lua.AddLoader(LuaLoader);

            
            byte[] lua_code;
            ResourceManager.Instance.LoadLuaScript("Main", out lua_code);
            DoString_WithException(lua_code, "Main", _lua.Global);
            _lua.Global.Get("GetUIConfig", out get_uiconfig);
        }

        public bool DoString_WithException(byte[] chunk,string chunkName ="chunk",LuaTable env = null)
        {
            try
            {
                _lua.DoString(chunk, chunkName, env);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("xlua exception:{0}:{1}", e.Message, e.StackTrace);
            }

            return false;
        }

        private byte[] LuaLoader(ref string filePath)
        {
            filePath = filePath.Replace('.', '/')+".lua";
            byte[] lua_code;
            ResourceManager.Instance.LoadLuaScript(filePath, out lua_code);
            return lua_code;
        }

        public LuaTable LoadScript(string script_path)
        {
            if (!string.IsNullOrEmpty(script_path))
            {
                byte[] lua_code;
                if (ResourceManager.Instance.LoadLuaScript(script_path,out lua_code))
                {
                    LuaTable scriptEnv = _lua.NewTable();

                    LuaTable meta = _lua.NewTable();
                    meta.Set("__index",_lua.Global);
                    scriptEnv.SetMetaTable(meta);
                    meta.Dispose();

                    string chunk_name = script_path;
                    DoString_WithException(lua_code, chunk_name, scriptEnv);
                    return scriptEnv;
                }
            }
            Debug.LogErrorFormat("load script failed path:{0}",script_path);
            return null;
        }

        public UIConfig GetUIConfig(int id)
        {
            UIConfig uIConfig = get_uiconfig?.Func<int, UIConfig>(id);

            if (uIConfig == null && id == 0)
            {
                uIConfig = new UIConfig()
                {
                    _enum = 0,
                    name = "Panel",
                    layer = 3,
                    fullscreen = false
                };
            }

            return uIConfig;
        }
        
        public void SingletonDestory()
        {
            
        }
    }
}