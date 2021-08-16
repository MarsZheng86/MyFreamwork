using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

public static class Utility
{
    public static StringBuilder g_strStringBuilder = new StringBuilder();

    public static string GetStringByStringBuilder(params string[] strValue)
    {
        g_strStringBuilder.Clear();

        for (int i = 0, _iCount = strValue.Length; i < _iCount; i++)
            g_strStringBuilder.Append(strValue[i]);

        return g_strStringBuilder.ToString();
    }
    
    public static bool CheckArrayValidity<T>(T[] obj)
    {
        bool _bResult = false;
        int _iLength = obj == null ? 0 : obj.Length;

        if (0 < _iLength)
            _bResult = true;

        return _bResult;
    }

    public static bool CheckListValidity<T>(List<T> listObject)
    {        
        return CheckArrayValidity(listObject.ToArray());
    }

    public static long CreateInstanceId()
    {
        Guid _cGuid = Guid.NewGuid();
        byte[] _byArrValue =_cGuid.ToByteArray();
        return BitConverter.ToInt64(_byArrValue, 0);
    }

    public static string GetConfigFilePath()
    {
        string _strPath = string.Empty;

#if UNITY_EDITOR
        _strPath = string.Format("{0}/scripts/game_res/99_config/", Application.dataPath);
#else
        _strPath = string.Format("{0}/scripts/game_res/99_config/", Application.persistentDataPath);
#endif

        return _strPath;
    }
}
