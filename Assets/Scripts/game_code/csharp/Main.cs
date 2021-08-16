using CORE;
using UnityEngine;

public class Main : MonoBehaviour
{
    #region Inherit
    public void Awake()
    {
        StartSingleton();
    }

    public void Start()
    {
        PrintoutProjectPath();
        Register();
    }

    public void Update()
    {
    }

    public void OnDestroy()
    {
    }

    public void OnApplicationQuit()
    {
        Logout();
        OverSingleton();
    }
    #endregion

    #region Private Function
    private void Register()
    {
        Application.lowMemory += OnLowMemory;
    }

    private void Logout()
    {
        Application.lowMemory -= OnLowMemory;
    }

    private void OnLowMemory()
    {
        AssetMgr.Instance.DeepRecycle();
    }

    private void StartSingleton()
    {
        ProjectSettings.Instance.StartSingleton();
        LogMgr.Instance.StartSingleton();
        TimerMgr.Instance.StartSingleton();
        AssetMgr.Instance.StartSingleton();
        ConfigMgr.Instance.StartSingleton();
    }

    private void OverSingleton()
    {
        Object.DestroyImmediate(ProjectSettings.Instance.gameObject);
        Object.DestroyImmediate(LogMgr.Instance.gameObject);
        Object.DestroyImmediate(TimerMgr.Instance.gameObject);
        Object.DestroyImmediate(AssetMgr.Instance.gameObject);
        Object.DestroyImmediate(ConfigMgr.Instance.gameObject);
    }

    private void PrintoutProjectPath()
    {
        LogMgr.Instance.Log("----------------------------------------------------------------------");
        LogMgr.Instance.Log("platform =" + Application.platform.ToString());
        LogMgr.Instance.Log("dataPath =" + Application.dataPath);
        LogMgr.Instance.Log("persistentDataPath =" + Application.persistentDataPath);
        LogMgr.Instance.Log("streamingAssetsPath =" + Application.streamingAssetsPath);
        LogMgr.Instance.Log("temporaryCachePath =" + Application.temporaryCachePath);
        LogMgr.Instance.Log("----------------------------------------------------------------------");
    }
    #endregion
}