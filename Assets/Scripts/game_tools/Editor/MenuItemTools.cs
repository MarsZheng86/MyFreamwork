using System.IO;
using UnityEditor;
using UnityEngine;

public class MenuItemTools : Editor
{
    #region Pirvate Static Variable
    private static string g_strFolderGameRes;
    private static string g_strBuildABPath;
    #endregion

    #region Private Function
    private static string ReplacePathFormat(string strValue)
    {
        return strValue.Replace("\\", "/");
    }

    private static void Init()
    {
        g_strFolderGameRes = "game_res";
        g_strBuildABPath = ReplacePathFormat(Application.streamingAssetsPath);
    }

    private static void ClearABFile()
    {
        if (false == System.IO.Directory.Exists(g_strBuildABPath))
        {
            Debug.LogError("MenuItem_Top::BuildChooseAssetBundle is failed. Not find application.streamingAssetsPath");
            return;
        }

        System.IO.Directory.Delete(g_strBuildABPath, true);
        System.IO.File.Delete(string.Format("{0}.{1}", g_strBuildABPath, ".meta"));
        AssetDatabase.Refresh();
        Debug.Log("Project ABFile Delete Success");
    }

    private static void ClearAssetBundlesName()
    {
        int _iLength = AssetDatabase.GetAllAssetBundleNames().Length;
        string[] _strArrOldABName = new string[_iLength];
        for (int i = 0; i < _iLength; i++)
            _strArrOldABName[i] = AssetDatabase.GetAllAssetBundleNames()[i];

        for (int i = 0; i < _iLength; i++)
            AssetDatabase.RemoveAssetBundleName(_strArrOldABName[i], true);

        Debug.Log("Clear AssetbundlesName Success");
    }

    private static void Pack(string source)
    {
        DirectoryInfo folder = new DirectoryInfo(source);
        FileSystemInfo[] files = folder.GetFileSystemInfos();
        int length = files.Length;
        for (int i = 0; i < length; i++)
        {
            if (files[i] is DirectoryInfo)
            {
                Pack(files[i].FullName);
            }
            else
            {
                if (!files[i].Name.EndsWith(".meta"))
                {
                    fileWithDepends(files[i].FullName);
                }
            }
        }
    }

    static void fileWithDepends(string source)
    {
        Debug.Log("file source " + source);
        string _source = ReplacePathFormat(source);
        string _assetPath = "Assets" + _source.Substring(Application.dataPath.Length);

        Debug.Log(_assetPath);

        //自动获取依赖项并给其资源设置AssetBundleName
        string[] dps = AssetDatabase.GetDependencies(_assetPath);
        foreach (var dp in dps)
        {
            Debug.Log("dp " + dp);
            if (dp.EndsWith(".cs"))
                continue;
            AssetImporter assetImporter = AssetImporter.GetAtPath(dp);
            string pathTmp = dp.Substring("Assets".Length + 1);
            string assetName = pathTmp.Substring(pathTmp.LastIndexOf("/") + 1);
            assetName = assetName.Replace(Path.GetExtension(assetName), ".unity3d");
            Debug.Log(assetName);
            assetImporter.assetBundleName = assetName;
        }

    }

    private static void SetABInfo(string strPath)
    {
        string _strPath = ReplacePathFormat(strPath);
        AssetImporter _cAssetImporter = AssetImporter.GetAtPath(_strPath);
        string _strAssetName = _strPath.Substring(_strPath.LastIndexOf("/") + 1);
        _strAssetName = _strAssetName.Replace(Path.GetExtension(_strAssetName), ".unity3d");
        _cAssetImporter.assetBundleName = _strAssetName;
    }

    private static void ShowWindows(string strWinTitle, string strWinDes, string strBtnDis)
    {
        EditorUtility.DisplayDialog(strWinTitle, strWinTitle, strBtnDis);
    }
    #endregion

    [MenuItem("MyTools/AssetBundleTool/选中文件打包AB")]
    public static void BuildChooseAssetBundle()
    {
        Init();
        ClearABFile();
        ClearAssetBundlesName();

        if (false == Directory.Exists(string.Format("{0}/{1}", g_strBuildABPath, g_strFolderGameRes)))
            Directory.CreateDirectory(string.Format("{0}/{1}", g_strBuildABPath, g_strFolderGameRes));

        Pack(Application.dataPath + "/scripts");

        //for (int i = 0, _iCount = _strSelection.Length; i < _iCount; i++)
        //{
        //    string _strValue = AssetDatabase.GUIDToAssetPath(_strSelection[i]);
        //    SetABInfo(_strValue);
        //}

        BuildPipeline.BuildAssetBundles(string.Format("{0}/{1}", g_strBuildABPath, g_strFolderGameRes), BuildAssetBundleOptions.ChunkBasedCompression, EditorUserBuildSettings.activeBuildTarget);
        AssetDatabase.Refresh();
    }
}
