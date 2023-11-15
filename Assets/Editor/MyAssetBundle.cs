using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ResourcesLoad;
using UnityEditor;
using UnityEngine;

public class MyAssetBundle : Editor
{
    /// <summary>
    /// 构建的assetbundle
    /// </summary>
    private static string BundleExportFolder = "Bundles";

    /// <summary>
    /// pc、ios可读写 android 只读
    /// </summary>
    private static string BundleExportPath = string.Format("{0}/{1}", ResourceManagerConfig.StreamingAssetsPath, BundleExportFolder);

    private static StringBuilder IndexFileContent = new StringBuilder();
    
    [MenuItem("Bundle/My/Clear")]
    static void ClearBundle()
    {
        //BundleExportPath = Application.dataPath + "/../streaming/Bundles" 删除这个目录下的所有内容bundle包
        if (Directory.Exists(BundleExportPath))
        {
            Directory.Delete(BundleExportPath, true);
        }
        AssetDatabase.Refresh();
    }

    //[MenuItem("Bundle/My/Deleindex")]
    //static void ClearIndexInfo()
    //{
    //    string[] files = Directory.GetFiles(ResourceManagerConfig.StreamingAssetsPath, "index*");
    //    for (int i = 0; i < files.Length; i++)
    //    {
    //        File.Delete(files[i]);
    //    }
    //    AssetDatabase.Refresh();
    //}

    [MenuItem("Bundle/My/BuildLua")]
    static void MyABundleLua()
    {
        Dictionary<string, string> indexInfo = new Dictionary<string, string>();
        string BundlePath = "Assets/AssetBundleResources/LuaScripts";

        string luaSrcPath = string.Format("{0}/../XLua/", Application.dataPath);
        string luaPrefabPath = string.Format("{0}/AssetBundleResources/XLua/", Application.dataPath);

        //将LuaScripts删除
        if (Directory.Exists(BundleExportPath+"/luascripts"))
        {
            Directory.Delete(BundleExportPath + "/luascripts", true);
        }
        AssetDatabase.Refresh();

        CopyLuaDirectory(luaSrcPath, luaPrefabPath);
        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

        string[] dirss = Directory.GetDirectories(BundlePath, "*", SearchOption.AllDirectories);
        List<string> dirs = new List<string>();
        for (int i = 0; i < dirss.Length; i++)
        {
            dirs.Add(dirss[i]);
        }
        dirs.Add(BundlePath);
        for (int i = 0; i < dirs.Count; i++)
        {
            string[] files = Directory.GetFiles(dirs[i], "*");
            if (files.Length == 0)
                continue;

            AssetBundleBuild build = new AssetBundleBuild();
            string[] filesInfo = dirs[i].Split('\\');
            //build.assetBundleName = filesInfo[filesInfo.Length - 1] + "/" + filesInfo[filesInfo.Length - 1] + ".assetbundle";
            build.assetBundleName = "LuaScripts/LuaScripts.assetbundle";
            List<string> assetNames = new List<string>();
            List<string> addressableNames = new List<string>();
            for (int n = 0; n < files.Length; n++)
            {
                if (Path.GetExtension(files[n]).EndsWith(".meta"))
                {
                    continue;
                }
                Debug.LogFormat(files[n]);
                string addressableName = GetAddressableName(files[n]);
                assetNames.Add(files[n]);
                addressableNames.Add(addressableName);
                indexInfo.Add(addressableName, build.assetBundleName);
            }
            build.assetNames = assetNames.ToArray();
            build.addressableNames = addressableNames.ToArray();

            builds.Add(build);
        }
        Debug.LogError(builds.Count);

        //Reset Index Text
        {
            Dictionary<string, string> buildInfo = new Dictionary<string, string>();
            string index_file_path = string.Format("{0}{1}.txt", "Assets/AssetBundleResources/", ResourceManagerConfig.kIndexFileName);
            string[] allInfo = File.ReadAllText(index_file_path).Split('\n');
            Debug.LogError("--------"+allInfo.Length);
            for (int i = 0; i < allInfo.Length; i++)
            {
                if (!string.IsNullOrEmpty(allInfo[i]))
                {
                    string[] info = allInfo[i].Split(':');
                    if (info.Length == 2)
                    {
                        buildInfo.Add(info[0], info[1]);
                    }
                }
            }
            foreach (var item in indexInfo)
            {
                if (buildInfo.ContainsKey(item.Key))
                {
                    buildInfo[item.Key] = item.Value;
                }
                else
                {
                    buildInfo.Add(item.Key, item.Value);
                }
            }
            IndexFileContent.Clear();
            foreach (var item in buildInfo)
            {
                IndexFileContent.AppendFormat("{0}:{1}", item.Key, item.Value);
                IndexFileContent.AppendLine();
            }
            File.WriteAllText(index_file_path, IndexFileContent.ToString());

            AssetBundleBuild indexbuild = new AssetBundleBuild();
            indexbuild.assetBundleName = ResourceManagerConfig.kIndexFileName;
            indexbuild.assetNames = new string[] { index_file_path };
            indexbuild.addressableNames = new string[] { ResourceManagerConfig.kIndexFileName };

            builds.Add(indexbuild);
        }

        BuildPipeline.BuildAssetBundles(BundleExportPath, builds.ToArray(), BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        //BuildPipeline.BuildAssetBundles(BundleExportPath, builds.ToArray(), BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        AssetDatabase.Refresh();
    }

    [MenuItem("Bundle/My/Build")]
    static void MyABundleAll(){

        IndexFileContent.Clear();
        IndexFileContent.AppendFormat("{0}", PlayerSettings.bundleVersion);
        IndexFileContent.AppendLine();
        
        IndexFileContent.Clear();
        
        string BundlePath = "Assets/AssetBundleResources";
        
        string luaSrcPath = string.Format("{0}/../XLua/", Application.dataPath);
        string luaPrefabPath = string.Format("{0}/AssetBundleResources/XLua/", Application.dataPath);

        if (Directory.Exists(BundleExportPath))
        {
            Directory.Delete(BundleExportPath, true);
        }
        Directory.CreateDirectory(BundleExportPath);


        // lua
        CopyLuaDirectory(luaSrcPath, luaPrefabPath);

        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();

        string[] allDir = Directory.GetDirectories(BundlePath, "*");
        for (int i = 0; i < allDir.Length; i++)
        {
            string[] files = Directory.GetFiles(allDir[i], "*", SearchOption.AllDirectories);
            //if (allDir[i].Contains("LuaScripts"))
            //{
            //    files = Directory.GetFiles(allDir[i], "*", SearchOption.AllDirectories);
            //}
            if (files.Length == 0)
            {
                continue;
            }
            string[] dirValue = allDir[i].Split('\\');
            AssetBundleBuild build = new AssetBundleBuild();
            List<string> assetNames = new List<string>();
            List<string> addressableNames = new List<string>();
            //string md5 = AssetBundleMD5.GetPathMd5(allDir[i]);
            build.assetBundleName = dirValue[dirValue.Length - 1] + "/" + dirValue[dirValue.Length - 1] + ".assetbundle";

            for (int n = 0; n < files.Length; n++)
            {
                if (Path.GetExtension(files[n]) == ".meta")
                {
                    continue;
                }

                string addressableName = GetAddressableName(files[n]);

                assetNames.Add(files[n]);
                addressableNames.Add(addressableName);
                
                IndexFileContent.AppendFormat("{0}:{1}", addressableName, build.assetBundleName);
                IndexFileContent.AppendLine();
            }

            build.assetNames= assetNames.ToArray();
            build.addressableNames = addressableNames.ToArray();

            builds.Add(build);
        }

        {
            string index_file_path = string.Format("{0}{1}.txt", "Assets/AssetBundleResources/", ResourceManagerConfig.kIndexFileName);
            File.WriteAllText(index_file_path, IndexFileContent.ToString());

            AssetBundleBuild indexbuild = new AssetBundleBuild();
            indexbuild.assetBundleName = ResourceManagerConfig.kIndexFileName;
            indexbuild.assetNames = new string[] { index_file_path };
            indexbuild.addressableNames = new string[] { ResourceManagerConfig.kIndexFileName };

            builds.Add(indexbuild);
        }
        BuildPipeline.BuildAssetBundles(BundleExportPath, builds.ToArray(), BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

        Debug.Log("build path :"+ BundleExportPath + "build success count " + builds.Count);
    }

    private static string GetAddressableName(string file_path)
    {
        string addressable_name = file_path;
        addressable_name = addressable_name.Replace("Assets/AssetBundleResources\\", "").Replace("Assets/AssetBundleResources/", "").Replace('\\', '/');
        int dot_pos = addressable_name.LastIndexOf('.');
        if (dot_pos != -1)
        {
            int count = addressable_name.Length - dot_pos;
            addressable_name = addressable_name.Remove(dot_pos, count);
        }
        return addressable_name;
    }

    /// <summary>
    /// 将一个目录下的所有文件夹复制到另一个文件夹中并以.lua.txt为后缀
    /// </summary>
    /// <param name="SourcePath"></param>
    /// <param name="DestinationPath"></param>
    /// <returns></returns>
    private static bool CopyLuaDirectory(string SourcePath,string DestinationPath)
    {
        bool ret = false;

        try
        {
            SourcePath = SourcePath.EndsWith(@"/", System.StringComparison.CurrentCulture) ? SourcePath : SourcePath + @"/";
            DestinationPath = DestinationPath.EndsWith(@"/", System.StringComparison.CurrentCulture) ? DestinationPath : DestinationPath + @"/";

            if (Directory.Exists(SourcePath))
            {
                if (Directory.Exists(DestinationPath) == true)
                {
                    Directory.Delete(DestinationPath, true);
                }
                Directory.CreateDirectory(DestinationPath);

                foreach (var item in Directory.GetFiles(SourcePath))
                {
                    FileInfo fileInfo = new FileInfo(item);
                    string file_name = Path.GetFileNameWithoutExtension(fileInfo.Name);
                    fileInfo.CopyTo(DestinationPath + file_name + ".lua.txt");
                }
                foreach (string item in Directory.GetDirectories(SourcePath))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(item);
                    if (CopyLuaDirectory(item, DestinationPath + dirInfo.Name) == false)
                    {
                        ret = false;
                    }
                }
                ret = true;
            }
        }
        catch (System.Exception ex)
        {
            ret = false;
        }

        AssetDatabase.Refresh();
        return ret;
    }
}
