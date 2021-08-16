using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CORE
{
    public class AssetMgr : Singleton<AssetMgr>
    {
        #region Const
        private const string ASSETPATH = "Assets/scripts/game_res/{0}/{1}";
        private string BASEPATH = "{0}/game_res/{1}";
        private string MANIFESTABNAME = "game_res";
        #endregion

        #region Private Member Variable
        private string ABDATAPATH = Application.streamingAssetsPath;// Application.persistentDataPath;
        private string m_strAssetName;
        private string m_strFullPath;
        private bool m_bRecycleNow;
        private float m_fTimeStamp;
        private AssetLoadedABCached m_cAssetLoadedABCached;
        private AssetLoadRequest m_cAssetLoadReq;
        private AssetBundle m_cManiAssetBundle;
        private AssetBundleManifest m_cManifest;
        private Queue<AssetLoadRequest> m_queueAssetLoadRequest;
        private Queue<AssetLoadRequest> m_queueAssetUnLoadRequest;
        private Dictionary<string, AssetLoadedABCached> m_dictAssetLoadedABCached;
        #endregion

        #region Constructor
        public AssetMgr() { }
        ~AssetMgr() { }
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
            m_bRecycleNow = false;

            if (null == m_queueAssetLoadRequest)
                m_queueAssetLoadRequest = new Queue<AssetLoadRequest>();

            ClearQueueAssetLoadRequest();

            if (null == m_queueAssetUnLoadRequest)
                m_queueAssetUnLoadRequest = new Queue<AssetLoadRequest>();

            ClearQueueAssetUnLoadRequest();

            if (null == m_dictAssetLoadedABCached)
                m_dictAssetLoadedABCached = new Dictionary<string, AssetLoadedABCached>();

            ClearDictAssetLoadedABCached();

            try
            {
                string _strTest = string.Format(BASEPATH, ABDATAPATH, MANIFESTABNAME);
                m_cManiAssetBundle = AssetBundle.LoadFromFile(string.Format(BASEPATH, ABDATAPATH, MANIFESTABNAME));
                m_cManifest = m_cManiAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            }
            catch (System.Exception ex)
            {
                LogMgr.Instance.Log(ex.Message, LogType.Error);
                LogMgr.Instance.Log("AssetLoadedABCachedMgr::Init is failed. Load Manifest failed.", LogType.Error);
                return;
            }

            StartCoroutine(DoWork());
        }

        private void Release()
        {
            StopAllCoroutines();
            m_strAssetName = null;
            m_strFullPath = null;
            m_bRecycleNow = false;
            m_fTimeStamp = 0f;
            m_cAssetLoadedABCached = null;
            m_cAssetLoadReq = null;

            if (null != m_cManiAssetBundle)
            {
                m_cManiAssetBundle.Unload(true);
                m_cManiAssetBundle = null;
            }

            m_cManifest = null;

            ClearQueueAssetLoadRequest();
            m_queueAssetLoadRequest = null;
            ClearDictAssetLoadedABCached();
            m_dictAssetLoadedABCached = null;
        }

        private void ClearQueueAssetLoadRequest()
        {
            if (null == m_queueAssetLoadRequest)
                return;

            m_queueAssetLoadRequest.Clear();
        }

        private void ClearQueueAssetUnLoadRequest()
        {
            if (null == m_queueAssetUnLoadRequest)
                return;

            m_queueAssetUnLoadRequest.Clear();
        }

        private void ClearDictAssetLoadedABCached()
        {
            if (null == m_dictAssetLoadedABCached)
                return;

            // 这里要清楚具体的内容
            m_dictAssetLoadedABCached.Clear();
        }

        private void DeveloperLoad()
        {
            for (int i = 0, iCount = m_cAssetLoadReq.m_strArrAssetName.Length; i < iCount; i++)
            {
                m_strAssetName = m_cAssetLoadReq.m_strArrAssetName[i];
                m_strFullPath = string.Format(ASSETPATH, m_cAssetLoadReq.m_strABName, m_strAssetName);
#if UNITY_EDITOR
                UnityEngine.Object _cObject = UnityEditor.AssetDatabase.LoadAssetAtPath(m_strFullPath, typeof(UnityEngine.Object));

                if (null == _cObject)
                {
                    LogMgr.Instance.Log(string.Format("AssetMgr::DeveloperLoad is failed. Not find asset. Path = {0}", m_strFullPath), LogType.Error);
                    continue;
                }

                m_cAssetLoadReq.AddLoadedAsset(_cObject);
#endif
            }
        }

        private IEnumerator DoWork()
        {
            while(true)
            {
                yield return null;
                yield return StartCoroutine(DoLoad());
                DoUnload();
                if (DoRecycle())
                {
                    yield return Resources.UnloadUnusedAssets();
                    System.GC.Collect();
                }
            }
        }

        private IEnumerator DoLoad()
        {
            while (0 < m_queueAssetLoadRequest.Count)
            {
                m_cAssetLoadReq = m_queueAssetLoadRequest.Dequeue();
                if (null == m_cAssetLoadReq)
                    continue;

                if (false == m_cAssetLoadReq.CheckValidity())
                    continue;

                switch (ProjectSettings.Instance.m_eBuildType)
                {
                    case EBuildType.DEVELOPER:
                        DeveloperLoad();
                        break;
                    case EBuildType.RELEASE:
                        {
                            bool _bSuccess = true;
                            m_cAssetLoadedABCached = null;
                            m_dictAssetLoadedABCached.TryGetValue(m_cAssetLoadReq.m_strABName, out m_cAssetLoadedABCached);
                            if (null == m_cAssetLoadedABCached)
                            {
                                AssetBundleCreateRequest _cABCreateReq = null;
                                string[] _strArrDepAB = m_cManifest.GetAllDependencies(m_cAssetLoadReq.m_strABName);
                                if (true == Utility.CheckArrayValidity(_strArrDepAB))
                                {
                                    // 当前的关联引用AB拥有内容
                                    for (int i = 0, _iCount = _strArrDepAB.Length; i < _iCount; i++)
                                    {
                                        m_cAssetLoadedABCached = null;
                                        m_dictAssetLoadedABCached.TryGetValue(_strArrDepAB[i], out m_cAssetLoadedABCached);
                                        if (null != m_cAssetLoadedABCached)
                                        {
                                            m_cAssetLoadedABCached.AddABByDepReference(m_cAssetLoadReq.m_strABName);
                                            continue;
                                        }

                                        try
                                        {
                                            m_strFullPath = string.Format(BASEPATH, ABDATAPATH, _strArrDepAB[i]);
                                            _cABCreateReq = AssetBundle.LoadFromFileAsync(m_strFullPath);
                                        }
                                        catch (System.Exception ex)
                                        {
                                            LogMgr.Instance.Log(ex.Message);
                                        }

                                        yield return _cABCreateReq;

                                        if (null == _cABCreateReq || null == _cABCreateReq.assetBundle)
                                        {
                                            LogMgr.Instance.Log(string.Format("加载关联AssetBundle失败。主AB = {0} 关联AB = {1}", m_cAssetLoadReq.m_strABName, _strArrDepAB[i]), LogType.Error);
                                            _bSuccess = false;
                                            continue;
                                        }

                                        m_cAssetLoadedABCached = new AssetLoadedABCached(_cABCreateReq.assetBundle, OnCallbackAssetABCachedClear);
                                        m_cAssetLoadedABCached.AddABByDepReference(m_cAssetLoadReq.m_strABName);
                                        m_dictAssetLoadedABCached.Add(_strArrDepAB[i], m_cAssetLoadedABCached);
                                    }

                                    if (false == _bSuccess)
                                    {
                                        for (int i = 0, _iCount = _strArrDepAB.Length; i < _iCount; i++)
                                        {
                                            m_cAssetLoadedABCached = null;
                                            m_dictAssetLoadedABCached.TryGetValue(_strArrDepAB[i], out m_cAssetLoadedABCached);
                                            if (null != m_cAssetLoadedABCached)
                                                m_cAssetLoadedABCached.SubABByDepReference(m_cAssetLoadReq.m_strABName);
                                        }
                                        break;
                                    }
                                }

                                try
                                {
                                    m_strFullPath = string.Format(BASEPATH, ABDATAPATH, m_cAssetLoadReq.m_strABName);
                                    _cABCreateReq = AssetBundle.LoadFromFileAsync(m_strFullPath);
                                }
                                catch (System.Exception ex)
                                {
                                    LogMgr.Instance.Log(ex.Message);
                                }

                                yield return _cABCreateReq;

                                if (null == _cABCreateReq || null == _cABCreateReq.assetBundle)
                                {
                                    LogMgr.Instance.Log(string.Format("加载主AB失。主AB = {0}", m_cAssetLoadReq.m_strABName), LogType.Error);
                                    _bSuccess = false;
                                }

                                if (false == _bSuccess)
                                {
                                    for (int i = 0, _iCount = _strArrDepAB.Length; i < _iCount; i++)
                                    {
                                        m_cAssetLoadedABCached = null;
                                        m_dictAssetLoadedABCached.TryGetValue(_strArrDepAB[i], out m_cAssetLoadedABCached);
                                        if (null != m_cAssetLoadedABCached)
                                            m_cAssetLoadedABCached.SubABByDepReference(m_cAssetLoadReq.m_strABName);
                                    }
                                    break;
                                }

                                m_cAssetLoadedABCached = new AssetLoadedABCached(_cABCreateReq.assetBundle, OnCallbackAssetABCachedClear);

                                for (int i = 0, _iCount = _strArrDepAB.Length; i < _iCount; i++)
                                    m_cAssetLoadedABCached.AddABDepReference(_strArrDepAB[i]);

                                m_dictAssetLoadedABCached.Add(m_cAssetLoadReq.m_strABName, m_cAssetLoadedABCached);
                            }

                            for (int i = 0, _iCount = m_cAssetLoadReq.m_strArrAssetName.Length; i < _iCount; i++)
                            {
                                AssetBundleRequest _cAssetBundleReq = null;
                                m_strAssetName = m_cAssetLoadReq.m_strArrAssetName[i];
                                if (null == m_cAssetLoadedABCached.TryGetAssetObject(m_strAssetName))
                                {
                                    if (false == m_cAssetLoadedABCached.m_cAssetBundle.Contains(m_strAssetName))
                                    {
                                        LogMgr.Instance.Log(string.Format("检查Asset失败。 AssetBundle {0} 里面没有包含 Asset {1}", m_cAssetLoadReq.m_strABName, m_strAssetName), LogType.Error);
                                        continue;
                                    }

                                    try
                                    {
                                        _cAssetBundleReq = m_cAssetLoadedABCached.m_cAssetBundle.LoadAssetAsync(m_strAssetName);
                                    }
                                    catch (System.Exception ex)
                                    {
                                        LogMgr.Instance.Log(ex.Message);
                                    }

                                    yield return _cAssetBundleReq;

                                    if (null == _cAssetBundleReq || null == _cAssetBundleReq.asset)
                                    {
                                        LogMgr.Instance.Log(string.Format("加载Asset失败。 AB = {0}  Asset = {1}", m_cAssetLoadReq.m_strABName, m_strAssetName), LogType.Error);
                                        continue;
                                    }

                                    m_cAssetLoadedABCached.CachedAsset(m_strAssetName, _cAssetBundleReq.asset);
                                }

                                m_cAssetLoadReq.AddLoadedAsset(_cAssetBundleReq.asset);
                                m_cAssetLoadedABCached.AddAssetReference(m_strAssetName);
                            }
                        }
                        break;
                }
                m_cAssetLoadReq.DoDeleCallback();
            }
        }

        private void DoUnload()
        {
            while(0 < m_queueAssetUnLoadRequest.Count)
            {
                AssetLoadRequest _cAssetLoadReq = m_queueAssetUnLoadRequest.Dequeue();
                AssetLoadedABCached _cAssetLoadedABCached = null;
                if (false == m_dictAssetLoadedABCached.TryGetValue(_cAssetLoadReq.m_strABName, out _cAssetLoadedABCached))
                {
                    LogMgr.Instance.Log(string.Format("AssetMgr::DoUnload is failed. Not find Object. ABName = {0}", _cAssetLoadReq.m_strABName), LogType.Error);
                    continue;
                }

                if (null == _cAssetLoadedABCached)
                {
                    LogMgr.Instance.Log(string.Format("AssetMgr::DoUnload is failed. Object is invalid . ABName = {0}", _cAssetLoadReq.m_strABName), LogType.Error);
                    continue;
                }

                for (int i = 0, _iCount = _cAssetLoadReq.m_strArrAssetName.Length; i < _iCount; i ++)
                    _cAssetLoadedABCached.SubAssetReference(_cAssetLoadReq.m_strArrAssetName[i]);
            }
        }

        private bool DoRecycle()
        {
            bool _bResult = false;
            if (!m_bRecycleNow && Time.realtimeSinceStartup - m_fTimeStamp < 60f)
            {
                if (0 == m_dictAssetLoadedABCached.Count)
                {
                    m_bRecycleNow = false;
                    m_fTimeStamp = Time.realtimeSinceStartup;
                    return _bResult;
                }

                var _varDictValue = m_dictAssetLoadedABCached.GetEnumerator();
                while(_varDictValue.MoveNext())
                {
                    if (null == _varDictValue.Current.Value)
                    {
                        LogMgr.Instance.Log(string.Format("AssetMgr::DoRecycle is failed. Dictionary value is null. key = {0}", _varDictValue.Current.Key), LogType.Error);
                        continue;
                    }

                    if (false == _varDictValue.Current.Value.CheckValidity(m_bRecycleNow))
                    {
                        m_dictAssetLoadedABCached.Remove(_varDictValue.Current.Key);
                        _bResult = true;
                    }
                }
                _varDictValue.Dispose();
            }

            return _bResult;
        }

        private void OnCallbackAssetABCachedClear(string[] strArrDep, string[] strArrByDep, string selfABName)
        {
            AssetLoadedABCached _cAssetLoadedABCacheSelf = null;
            m_dictAssetLoadedABCached.TryGetValue(selfABName, out _cAssetLoadedABCacheSelf);
            if (null == _cAssetLoadedABCacheSelf)
            {
                LogMgr.Instance.Log(string.Format("AssetMgr::OnCallbackAssetABCachedClear is failed. Not find Object. ABName = {0}", selfABName), LogType.Error);
                return;
            }

            if (true == Utility.CheckArrayValidity(strArrDep))
            {
                for (int i = 0, _iCount = strArrDep.Length; i < _iCount; i++)
                {
                    AssetLoadedABCached _cAssetLoadedABCached = null;
                    m_dictAssetLoadedABCached.TryGetValue(strArrDep[i], out _cAssetLoadedABCached);
                    if (null == _cAssetLoadedABCached)
                    {
                        LogMgr.Instance.Log(string.Format("AssetMgr::OnCallbackAssetABCachedClear is failed. Not find Object. ABName = {0}", strArrByDep[i]), LogType.Error);
                        continue;
                    }

                    _cAssetLoadedABCached.SubABByDepReference(selfABName);
                    _cAssetLoadedABCacheSelf.SubABDepReference(strArrDep[i]);
                }
            }

            if (true == Utility.CheckArrayValidity(strArrByDep))
            {
                for (int i = 0, _iCount = strArrByDep.Length; i < _iCount; i ++)
                {
                    AssetLoadedABCached _cAssetLoadedABCached = null;
                    m_dictAssetLoadedABCached.TryGetValue(strArrByDep[i], out _cAssetLoadedABCached);
                    if (null == _cAssetLoadedABCached)
                    {
                        LogMgr.Instance.Log(string.Format("AssetMgr::OnCallbackAssetABCachedClear is failed. Not find Object. ABName = {0}", strArrByDep[i]), LogType.Error);
                        continue;
                    }

                    _cAssetLoadedABCached.SubABDepReference(selfABName);
                    _cAssetLoadedABCacheSelf.SubABByDepReference(strArrByDep[i]);
                }
            }
        }
        #endregion

        #region Public Function
        public void Load(string strABName, string[] strArrAssetName, AssetLoadRequest.AssetLoadedCallBack deleCallback)
        {
            if (string.IsNullOrEmpty(strABName) || false == Utility.CheckArrayValidity(strArrAssetName))
            {
                LogMgr.Instance.Log("AssetMgr::Load is failed. Param is null reference", LogType.Error);
                return;
            }

            AssetLoadRequest _req = new AssetLoadRequest(strABName, strArrAssetName, deleCallback);
            m_queueAssetLoadRequest.Enqueue(_req);
        }

        public void UnLoad(string strABName, string[] strArrAssetName, AssetLoadRequest.AssetLoadedCallBack deleCallback)
        {
            if (string.IsNullOrEmpty(strABName) || false == Utility.CheckArrayValidity(strArrAssetName))
            {
                LogMgr.Instance.Log("AssetMgr::UnLoad is failed. Param is null reference", LogType.Error);
                return;
            }

            AssetLoadRequest _req = new AssetLoadRequest(strABName, strArrAssetName, deleCallback);
            m_queueAssetUnLoadRequest.Enqueue(_req);
        }

        public void DeepRecycle()
        {
            m_bRecycleNow = true;
        }
        #endregion
    }
}