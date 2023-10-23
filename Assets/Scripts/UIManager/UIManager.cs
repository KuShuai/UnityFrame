using System.Collections;
using System.Collections.Generic;
using ResourcesLoad;
using UnityEngine;
using XLua;

public class UIManager : MonoSingleton<UIManager>, IMonoSingleton
{
    public const string UIPanel = "UIPanel";

    private static Dictionary<string, int> uiLayer = new Dictionary<string, int>()
    {
        { "Back",5000},
        { "Normal",10000},
        { "Top",15000}
    };

    /// <summary>
    /// UI根节点
    /// </summary>
    private static GameObject _root;

    private static Camera _uiCamera;

    /// <summary>
    /// 所有打开的UI
    /// </summary>
    public static Dictionary<string, GameObject> _dictUI;
    private static List<UIPanel> _panelStack;
    private static bool _reorder;

    public void Init()
    {
        _dictUI = new Dictionary<string, GameObject>();
        _panelStack = new List<UIPanel>();
        _reorder = false;
    }

    private void Update()
    {
        OnPanelStackChanged();
    }

    private void OnPanelStackChanged()
    {
        if (!_reorder)
        {
            return;
        }
        _reorder = false;

        int fixed_order = 1000;
        int normal_order = 20000;
        int top_order = 30000;

        for (int i = 0; i < _panelStack.Count; i++)
        {
            var panel = _panelStack[i];
                
            if (panel._uiConfig.layer == (int)UILayer.Fixed)
            {
                panel.SetOrder(fixed_order++);
            }
            else if (panel._uiConfig.layer == (int)UILayer.Normal)
            {
                panel.SetOrder(normal_order++);
            }
            else if (panel._uiConfig.layer == (int)UILayer.Top)
            {
                panel.SetOrder(top_order++);
            }
            else
            {
                panel.SetOrder(-10);
            }
        }
    }

    public void SingletonInit()
    {
    }

    /// <summary>
    /// 获取创建UI根节点
    /// </summary>
    private static void CreateRoot()
    {
        if (_root == null)
        {
            GameObject obj = ResourceManager.Instance.Load(RSPathUtil.UI("UIRoot")) as GameObject;

            if (obj == null)
            {
                Debug.LogErrorFormat("找不到UI：{0}", "UIRoot");
                return;
            }

            _root = Instantiate(obj);
            DontDestroyOnLoad(_root);
        }

        if (_root != null && _uiCamera == null)
        {
            _uiCamera = _root.GetComponentInChildren<Camera>();
        }
    }

    private static GameObject CreateUIObj(string name,object param)
    {
        GameObject obj = ResourceManager.Instance.Load(RSPathUtil.UI(name)) as GameObject;
        if (obj == null)
        {
            return null;
        }

        GameObject clone = Instantiate(obj);
        clone.name = name;
        Canvas canvas = clone.GetComponent<Canvas>();
        if (canvas == null)
        {
            Debug.LogErrorFormat("UI对象缺少canvas组件：{0}", name);
            return null;
        }

        canvas.worldCamera = _uiCamera;

        canvas.sortingLayerName = "UI";
        var panel = clone.GetComponent<UIPanel>();
        if (panel != null)
        {
            panel.SetCanvas(canvas);
            panel.OnShow(param);
            panel._Start?.Action(panel);
        }

        //添加屏幕适配组件
        //if (clone.GetComponent<UIAdaptive>() == null)
        //{
        //    clone.AddComponent<UIAdaptive>();
        //}

        return clone;
    }

    private static void AttachToParent(GameObject obj, GameObject root)
    {
        Vector3 localScale = obj.transform.localScale;
        obj.transform.SetParent(root.transform);

        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = localScale;
    }

    public static UIPanel OpenUIPanel(string name,object param = null)
    {
        UIPanel panel = OpenUI<UIPanel>(name,param);
        return panel;
    }

    public static UIPanel OpenUIPanel_LUA(int panelEnum,object param, object load_paramter = null)
    {
        var ui_config = LuaScriptManager.Instance.GetUIConfig(panelEnum);
        if (ui_config == null)
        {
            Debug.LogError("ui config is null panelEnum is " + panelEnum);
            return null;
        }
        string panelName = ui_config.name;

        UIPanel uiPanel = OpenUIPanel(panelName,param);
        if (uiPanel == null)
        {
            return null;
        }

        uiPanel.SetUp(panelEnum, ui_config, load_paramter);

        return uiPanel;
    }

    public static GameObject OpenUI(string asset_name,object param)
    {
        Debug.LogFormat("uimanager.cs open ui weidget :{0} {1}", asset_name, Time.frameCount);

        var uiName = WidgetName(asset_name);

        //代表已经打开
        if (_dictUI.ContainsKey(uiName))
            return null;
        CreateRoot();
        if (_root == null)
            return null;

        GameObject obj = CreateUIObj(uiName,param);
        if (obj == null)
            return null;

        AttachToParent(obj, _root);

        obj.name = uiName;

        _dictUI.Add(uiName, obj);
        _panelStack.Add(obj.GetComponent<UIPanel>());

        //刷新层级
        RequestReorderPanel();

        return obj;
    }

    public static void RequestReorderPanel()
    {
        _reorder = true;
    }

    public static T OpenUI<T>(string uiName, object param) where T : MonoBehaviour
    {
        GameObject obj = OpenUI(uiName,param);
        if (obj == null)
        {
            return null;
        }
        T ret = obj.GetComponent<T>();
        return ret;
    }

   //public static T OpenUI<T>(UIPanelEnum panelEnum,object param)where T : MonoBehaviour
   //{
   //    UIConfig uiConfig = UIConfigSingleton.Instance.GetUIConfig(panelEnum);
   //    if (uiConfig == null)
   //    {
   //        return null;
   //    }
   //    return OpenUI<T>(uiConfig.name,param);
   //}

    public static void CloseUI_LUA(int name,bool removeQuene = true,bool immediate = false)
    {
        try
        {
            var uiconfig = LuaScriptManager.Instance.GetUIConfig(name);
            string panelName = uiconfig.name;
            CloseUI(panelName, removeQuene,immediate);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(name);
            Debug.LogError(ex.Message);
        }
    }

    //public static void CloseUI(UIPanelEnum uiEnum, bool removeQuene = true)
    //{
    //    string uiName = UIConfigSingleton.Instance.GetUIConfig(uiEnum).name;
    //    CloseUI(uiName, removeQuene);
    //}

    public static void CloseUI(string uiName, bool removeQuene = true,bool immediate = false)
    {
        GameObject obj = null;

        if (!_dictUI.TryGetValue(uiName,out obj))
        {
            Debug.LogErrorFormat("没有找到对应关闭的UI ：{0}",uiName);
            return;
        }
        Debug.Log(uiName);
        _dictUI.Remove(uiName);
        _panelStack.Remove(obj.GetComponent<UIPanel>());

        if (obj == null)
        {
            Debug.LogErrorFormat("所关闭的UI已经销毁{0}", uiName);
            return;
        }

        UIPanel panel = obj.GetComponent<UIPanel>();
        if (panel != null)
        {
            panel.Close();
        }
        if (immediate)
        {
            DestroyImmediate(panel.gameObject);
        }
        else
        {
            Destroy(panel.gameObject);
        }

        RequestReorderPanel();

        ResourceManager.Instance.Unload(RSPathUtil.UI(uiName));
    }

    private static string WidgetName(string asset_name)
    {
        string[] assetname = asset_name.Split('/');
        string uiname = asset_name;
        if (assetname.Length > 0)
        {
            uiname = assetname[assetname.Length - 1];
        }
        return uiname;
    }

    public void SingletonDestory()
    {
    }

}
