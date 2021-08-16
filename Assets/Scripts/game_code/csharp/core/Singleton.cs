using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour  where T : new() 
{
    #region Const
    private const string SINGLETONROOTNAME = "_SingletonObject_";
    #endregion

    #region Instance Variable
    private static T g_Instance;

    public static T Instance
    {
        get
        {
            if (null == g_Instance)
                CreateObject();

            return g_Instance;
        }
    }
    #endregion

    #region Private Function
    public static void CreateObject()
    {
        GameObject _goSingletonRoot = GameObject.Find(SINGLETONROOTNAME);

        if (null == _goSingletonRoot)
        {
            _goSingletonRoot = new GameObject(SINGLETONROOTNAME);
            DontDestroyOnLoad(_goSingletonRoot);
        }

        Transform _tranTRoot = _goSingletonRoot.transform.Find(typeof(T).Name);

        if (null == _tranTRoot)
        {
            GameObject _goTRoot = new GameObject(typeof(T).Name, typeof(T));
            _tranTRoot = _goTRoot.transform;
            _tranTRoot.transform.parent = _goSingletonRoot.transform;
        }

        g_Instance = _tranTRoot.GetComponent<T>();
    }

    public virtual void StartSingleton() { }
    #endregion
}