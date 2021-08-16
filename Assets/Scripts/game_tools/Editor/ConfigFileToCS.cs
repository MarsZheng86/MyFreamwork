using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ConfigFileToCS
{
    public static string m_strTxtFilePath = Application.dataPath + "/scripts/game_res/99_config/";
    public static string m_strCSFilePath = Application.dataPath + "/scripts/game_code/csharp/configcs/";

    [MenuItem("MyTools/CreateConfigCS/自动生成配置表CS文件")]
    public static void CreateConfigCS()
    {
        DirectoryInfo _cFolder = new DirectoryInfo(m_strTxtFilePath);
        FileSystemInfo[] _cArrInfo = _cFolder.GetFileSystemInfos();
        for (int i = 0, _iCount = _cArrInfo.Length; i < _iCount; i++)
        {
            if (!_cArrInfo[i].Name.EndsWith(".meta"))
                ChangeCS(_cArrInfo[i].FullName, _cArrInfo[i].Name.Replace(".txt", ""));
        }

        AssetDatabase.Refresh();
    }

    public static void ChangeCS(string strFilePath, string strPathname)
    {
        try
        {
            string _strLine;
            System.IO.StreamReader sr = new System.IO.StreamReader(strFilePath, Encoding.Default);
            if (!System.IO.Directory.Exists(m_strCSFilePath))
                System.IO.Directory.CreateDirectory(m_strCSFilePath);

            string _strFilePath = m_strCSFilePath + strPathname + ".cs";
            System.IO.StreamWriter _SW = new System.IO.StreamWriter(_strFilePath, false);
            _SW.WriteLine("using System.Collections.Generic;");
            _SW.WriteLine("using CORE;");
            _SW.WriteLine("using UnityEngine;");
            _SW.Write("\n");

            _SW.WriteLine(string.Format("public class {0} : ConfigBase", strPathname));
            _SW.WriteLine("{");
            _SW.WriteLine("\t#region Class");

            _strLine = string.Format("\tpublic class C{0}CfgData", strPathname.Replace("Config", ""));
            _SW.WriteLine(_strLine);
            _SW.WriteLine("\t{");

            _strLine = sr.ReadLine();
            string[] _strArrType = _strLine.Split('\t');
            _strLine = sr.ReadLine();
            string[] _strArrDis = _strLine.Split('\t');
            _strLine = sr.ReadLine();
            string[] _strArrKey = _strLine.Split('\t');
            _SW.WriteLine("\t\tpublic string key;" + "\t//\tstring key");

            for (int i = 0; i < _strArrType.Length; i++)
            {
                if (!string.IsNullOrEmpty(_strArrType[i]))
                    _SW.WriteLine("\t\tpublic " + _strArrType[i].ToString() + " " + _strArrKey[i] + ";\t// " + _strArrDis[i]);
            }

            _SW.WriteLine("\t}");
            _SW.WriteLine("\t#endregion");
            _SW.WriteLine();
            _SW.WriteLine("\t#region Private Member Variable");
            _SW.WriteLine("\tprivate Dictionary<string, C" + strPathname.Replace("Config", "") + "CfgData> m_dictCfgData;");
            _SW.WriteLine("\t#endregion");
            _SW.WriteLine("");

            _SW.WriteLine("\t#region Constructor");
            _SW.WriteLine("\tpublic " + strPathname + "()");
            _SW.WriteLine("\t{");
            _SW.WriteLine("\t\t Init();");
            _SW.WriteLine("\t}");
            _SW.WriteLine("");
            _SW.WriteLine("\t~" + strPathname + "()");
            _SW.WriteLine("\t{");
            _SW.WriteLine("\t\tRelease();");
            _SW.WriteLine("\t}");
            _SW.WriteLine("\t#endregion");
            _SW.WriteLine("");

            _SW.WriteLine("\t#region Private Function");
            _SW.WriteLine("\tprivate void Init()");
            _SW.WriteLine("\t{");
            _SW.WriteLine("\t\tm_strCfgKey = typeof(" + strPathname + ").Name;");
            _SW.WriteLine("\t\tm_strCfgName = string.Format(\"" + "{0}.txt" + "\", m_strCfgKey);");
            _SW.WriteLine("");
            _SW.WriteLine("\t\tif (null == m_dictCfgData)");
            _SW.WriteLine("\t\t\tm_dictCfgData = new Dictionary<string, C" + strPathname.Replace("Config", "") + "CfgData>();");
            _SW.WriteLine();
            _SW.WriteLine("\t\tLoadConfig();");
            _SW.WriteLine("\t}");
            _SW.WriteLine();
            _SW.WriteLine("\tprivate void Release()");
            _SW.WriteLine("\t{");
            _SW.WriteLine("\t\tClearDictCfgData();");
            _SW.WriteLine("\t}");
            _SW.WriteLine();
            _SW.WriteLine("\tprivate void ClearDictCfgData()");
            _SW.WriteLine("\t{");
            _SW.WriteLine("\t\tif (null == m_dictCfgData)");
            _SW.WriteLine("\t\t\treturn;");
            _SW.WriteLine();
            _SW.WriteLine("\t\tm_dictCfgData.Clear();");
            _SW.WriteLine("\t}");
            _SW.WriteLine();
            _SW.WriteLine("\tprivate C" + strPathname.Replace("Config", "") + "CfgData ParseData(string strCfgData)");
            _SW.WriteLine("\t{");
            _SW.WriteLine("\t\tstring[] _strArr = strCfgData.Split(" + "\'\\t\');");
            _SW.WriteLine("\t\tC" + strPathname.Replace("Config", "") + "CfgData _c" + strPathname.Replace("Config", "") + "CfgData = new C" + strPathname.Replace("Config", "") + "CfgData();");
            _SW.WriteLine("\t\t_c" + strPathname.Replace("Config", "") + "CfgData.key = _strArr[0];");
            for (int i = 0; i < _strArrType.Length; i++)
            {
                if (string.IsNullOrEmpty(_strArrType[i]))
                    continue;

                if (false == _strArrType[i].Equals("string"))
                    _SW.WriteLine("\t\t_c" + strPathname.Replace("Config", "") + "CfgData." + _strArrKey[i] + " = " + _strArrType[i] + ".Parse(_strArr[" + i.ToString() + "]);");
                else
                    _SW.WriteLine("\t\t_c" + strPathname.Replace("Config", "") + "CfgData." + _strArrKey[i] + " = " + "_strArr[" + i.ToString() + "];");
            }
            _SW.WriteLine("\t\treturn _c" + strPathname.Replace("Config", "") + "CfgData;");
            _SW.WriteLine("\t}");
            _SW.WriteLine("\t#endregion");

            _SW.WriteLine();
            _SW.WriteLine("\t#region Protected Override");
            _SW.WriteLine("\tprotected override void LoadConfig()");
            _SW.WriteLine("\t{");
            _SW.WriteLine("\t\tstring _strFilePath = string.Format(\"{0}/{1}\", Utility.GetConfigFilePath(), m_strCfgName);");
            _SW.WriteLine("\t\tstring _strText = string.Empty;");
            _SW.WriteLine();
            _SW.WriteLine("\t\ttry");
            _SW.WriteLine("\t\t{");
            _SW.WriteLine("\t\t\t_strText = System.IO.File.ReadAllText(_strFilePath);");
            _SW.WriteLine("\t\t}");
            _SW.WriteLine("\t\tcatch (System.Exception ex)");
            _SW.WriteLine("\t\t{");
            _SW.WriteLine("\t\t\tLogMgr.Instance.Log(ex.Message, LogType.Error);");
            _SW.WriteLine("\t\t}");
            _SW.WriteLine();
            _SW.WriteLine("\t\tif (string.IsNullOrEmpty(_strText))");
            _SW.WriteLine("\t\t{");
            _SW.WriteLine("\t\t\t LogMgr.Instance.Log(\"" + strPathname + "::LoadConfig is failed. Load txt is empty\", LogType.Error);");
            _SW.WriteLine("\t\t\t return;");
            _SW.WriteLine("\t\t}");
            _SW.WriteLine();
            _SW.WriteLine("\t\tstring[] _strArrText = _strText.Split(\'\\n\');");
            _SW.WriteLine();
            _SW.WriteLine("\t\tfor (int i = 3, _iCount = _strArrText.Length; i < _iCount; i++)");
            _SW.WriteLine("\t\t{");
            _SW.WriteLine("\t\t\tif (string.IsNullOrEmpty(_strArrText[i]))");
            _SW.WriteLine("\t\t\t\tcontinue;");
            _SW.WriteLine();
            _SW.WriteLine("\t\t\tC" + strPathname.Replace("Config", "") + "CfgData _c" + strPathname.Replace("Config", "") + "CfgData = new C" + strPathname.Replace("Config", "") + "CfgData();");
            _SW.WriteLine("\t\t\t_c" + strPathname.Replace("Config", "") + "CfgData = ParseData(_strArrText[i]);");
            _SW.WriteLine("\t\t\tm_dictCfgData.Add(_c" + strPathname.Replace("Config", "") + "CfgData.key, _c" + strPathname.Replace("Config", "") + "CfgData);");
            _SW.WriteLine("\t\t}");
            _SW.WriteLine("\t}");
            _SW.WriteLine("\t#endregion");
            _SW.WriteLine();

            _SW.WriteLine("\t#region Public Function");
            _SW.WriteLine("\tpublic C" + strPathname.Replace("Config", "") + "CfgData GetData(int iKey)");
            _SW.WriteLine("\t{");
            _SW.WriteLine("\t\treturn GetData(iKey.ToString());");
            _SW.WriteLine("\t}");
            _SW.WriteLine();
            _SW.WriteLine("\tpublic C" + strPathname.Replace("Config", "") + "CfgData GetData(string strKey)");
            _SW.WriteLine("\t{");
            _SW.WriteLine("\t\tC" + strPathname.Replace("Config", "") + "CfgData _c" + strPathname.Replace("Config", "") + "CfgData = null;");
            _SW.WriteLine("\t\tm_dictCfgData.TryGetValue(strKey, out _c" + strPathname.Replace("Config", "") + "CfgData);");
            _SW.WriteLine("\t\treturn _c" + strPathname.Replace("Config", "") + "CfgData;");
            _SW.WriteLine("\t}");
            _SW.WriteLine();
            _SW.WriteLine("\tpublic C" + strPathname.Replace("Config", "") + "CfgData[] GetAllData()");
            _SW.WriteLine("\t{");
            _SW.WriteLine("\t\treturn new List<C" + strPathname.Replace("Config", "") + "CfgData>(m_dictCfgData.Values).ToArray();");
            _SW.WriteLine("\t}");
            _SW.WriteLine("\t#endregion");
            
            _SW.WriteLine("}");
            _SW.Close();
            _SW.Dispose();
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }
}