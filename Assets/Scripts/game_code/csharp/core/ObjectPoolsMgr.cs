using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolsMgr : Singleton<ObjectPoolsMgr>
{
    #region Private Struct
    private struct SResult
    {
        public bool m_bResult;
        public Object m_obj;
    }
    #endregion

    #region Private Member Variable
    private GameObject m_goResPoolRoot;                                                     //  池资源节点
    private Dictionary<string, List<GameObject>> m_dictInstancePools;                       //  对象实力池
    private Dictionary<string, GameObject> m_dictResGameObjectPools;                        //  对象缓存池
    #endregion

    #region Constructor
    public ObjectPoolsMgr() { }

    ~ObjectPoolsMgr() { }
    #endregion

    #region Inherit
    public void Awake()
    {
        Init();
    }

    public void OnDestroy()
    {
        Release();
    }
    #endregion

    #region Private Function
    private void Init()
    {
        if (null == m_goResPoolRoot)
            m_goResPoolRoot = new GameObject("_PoolsObject_");

        if (null == m_dictResGameObjectPools)
            m_dictResGameObjectPools = new Dictionary<string, GameObject>();

        if (null == m_dictInstancePools)
            m_dictInstancePools = new Dictionary<string, List<GameObject>>();
    }

    private void Release()
    {
        ClearInstancePools();
        ClearResGameObjectPools();
    }

    private void ClearInstancePools()
    {
        if (null == m_dictInstancePools)
        {
            LogMgr.Instance.Log("ObjectPoolsMgr::ClearInstancePools is failed. m_dictInstancePools is null reference", LogType.Error);
            return;
        }

        if (0 == m_dictInstancePools.Count)
        {
            LogMgr.Instance.Log("ObjectPoolsMgr::ClearInstancePools is failed. m_dictInstancePools is clear");
            return;
        }

        var _varDictObject = m_dictInstancePools.GetEnumerator();
        while(_varDictObject.MoveNext())
        {
            for (int i = 0, count = _varDictObject.Current.Value.Count; i < count; i++)
                DestroyImmediate(_varDictObject.Current.Value[i]);
        }

        m_dictInstancePools.Clear();
    }

    private void ClearResGameObjectPools()
    {
        if (null == m_dictResGameObjectPools)
        {
            LogMgr.Instance.Log("ObjectPoolsMgr::ClearResGameObjectPools is failed. m_dictResGameObjectPools is null reference", LogType.Error);
            return;
        }

        if (0 == m_dictResGameObjectPools.Count)
        {
            LogMgr.Instance.Log("ObjectPoolsMgr::ClearResGameObjectPools is failed. m_dictResGameObjectPools is clear");
            return;
        }

        var _varDictObject = m_dictResGameObjectPools.GetEnumerator();
        while (_varDictObject.MoveNext())
            DestroyImmediate(_varDictObject.Current.Value);

        m_dictResGameObjectPools.Clear();
    }

    private void AddInstanceGameObject(string strNameKey, GameObject goObject)
    {
        List<GameObject> _listGameObject = null;
        if (true == m_dictInstancePools.TryGetValue(strNameKey, out _listGameObject))
        {
            _listGameObject.Add(goObject);
            return;
        }

        _listGameObject = new List<GameObject>();
        _listGameObject.Add(goObject);
        m_dictInstancePools.Add(strNameKey, _listGameObject);
    }

    private bool RemoveInstanceGameObject(string strNameKey, GameObject goObject)
    {
        bool _bResult = false;        

        List<GameObject> _listGameObject = null;
        if (true == m_dictInstancePools.TryGetValue(strNameKey, out _listGameObject))
        {
            if (true == _listGameObject.Contains(goObject))
            {
                _listGameObject.Remove(goObject);
                DestroyImmediate(goObject);
                _bResult = true;
            }
        }

        return _bResult;
    }
    
    private bool ModifyInstanceGameObject(string strNameKey, GameObject goObject, bool bActive)
    {
        bool _bResult = false;
        List<GameObject> _listGameObject = null;
        if (true == m_dictInstancePools.TryGetValue(strNameKey, out _listGameObject))
        {
            if (true == _listGameObject.Contains(goObject))
            {
                if (goObject.activeInHierarchy != bActive)
                {
                    goObject.SetActive(bActive);
                    _bResult = true;
                }
            }
        }

        return _bResult;
    }

    private SResult FindUsableInstanceGameObject(string strNameKey)
    {
        SResult _sResult = new SResult();
        _sResult.m_bResult = false;

        if (null == m_dictInstancePools)
        {
            LogMgr.Instance.Log("ObjectPoolsMgr::CheckUsableInstanceGameObject is failed. m_dictInstancePools is null reference", LogType.Error);
            return _sResult;
        }

        List<GameObject> _listGameObject = null;
        if (true == m_dictInstancePools.TryGetValue(strNameKey, out _listGameObject))
        {
            for (int i = 0, count = _listGameObject.Count; i < count; i++)
            {
                if (false == _listGameObject[i].activeInHierarchy)
                {
                    _sResult.m_bResult = true;
                    _sResult.m_obj = _listGameObject[i];
                    break;
                }
            }
        }

        return _sResult;
    }

    private GameObject CreateInstanceGameobject(string strNameKey)
    {
        GameObject _goTemp = null;
        if (true == m_dictResGameObjectPools.TryGetValue(strNameKey, out _goTemp))
            _goTemp = Instantiate(_goTemp) as GameObject;
        // 如果没有资源，就需要加载资源，加载资源回掉处理以后，在创建资源并返回

        AddInstanceGameObject(strNameKey, _goTemp);
        return _goTemp;
    }
    #endregion

    #region Public Function
    public GameObject GetInstanceGameObject(string strNameKey)
    {
        if (true == string.IsNullOrEmpty(strNameKey))
        {
            LogMgr.Instance.Log("ObjectPoolsMgr::GetInstanceGameObject is failed. Param is invalid", LogType.Error);
            return null;
        }

        if (null == m_dictInstancePools)
        {
            LogMgr.Instance.Log("ObjectPoolsMgr::GetInstanceGameObject is failed. m_dictInstancePools is null reference", LogType.Error);
            return null;
        }

        SResult _sResult = FindUsableInstanceGameObject(strNameKey);
        if (true == _sResult.m_bResult)
            return _sResult.m_obj as GameObject;

        GameObject _goTemp = CreateInstanceGameobject(strNameKey);
        return _goTemp;
    }

    public bool DeleteInstanceGameObject(string strNameKey, GameObject goObject)
    {
        bool _bResult = false;
        if (true == string.IsNullOrEmpty(strNameKey) || null == goObject)
        {
            LogMgr.Instance.Log("ObjectPoolsMgr::DeleteInstanceGameObject is failed. Param is invalid", LogType.Error);
            return _bResult;
        }

        if (null == m_dictInstancePools)
        {
            LogMgr.Instance.Log("ObjectPoolsMgr::DeleteInstanceGameObject is failed. m_dictInstancePools is null reference", LogType.Error);
            return _bResult;
        }

        _bResult = RemoveInstanceGameObject(strNameKey, goObject);
        return _bResult;
    }

    public bool ChangeInstanceGameObject(string strNameKey, GameObject goObject, bool bActive)
    {
        bool _bResult = false;
        if (true == string.IsNullOrEmpty(strNameKey) || null == goObject)
        {
            LogMgr.Instance.Log("ObjectPoolsMgr::ChangeInstanceGameObject is failed. Param is invalid", LogType.Error);
            return _bResult;
        }

        if (null == m_dictInstancePools)
        {
            LogMgr.Instance.Log("ObjectPoolsMgr::ChangeInstanceGameObject is failed. m_dictInstancePools is null reference", LogType.Error);
            return _bResult;
        }

        _bResult = ModifyInstanceGameObject(strNameKey, goObject, bActive);
        return _bResult;
    }
    #endregion
}