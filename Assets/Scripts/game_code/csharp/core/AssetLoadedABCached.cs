using System.Collections.Generic;
using UnityEngine;

namespace CORE
{
    public class AssetLoadedABCached
    {
        #region
        public delegate void AssetLoadedABClearCallback(string[] strArrDep, string[] strArrByDep, string strSelfABName);
        #endregion

        #region Private Member Variable
        private List<string> m_listDepReference;
        private List<string> m_listByDepReference;
        private Dictionary<string, AssetLoadedCached> m_dictAssetLoadedCached;
        private AssetLoadedABClearCallback m_deleCallback;
        #endregion

        #region Public Member Variable
        public AssetBundle m_cAssetBundle { get; private set; }
        #endregion

        #region Constructor
        public AssetLoadedABCached(AssetBundle cAssetBundle, AssetLoadedABClearCallback deleCallback)
        {
            Init(cAssetBundle, deleCallback);
        }

        ~AssetLoadedABCached()
        {
            Release();
        }
        #endregion

        #region Private Function
        private void Init(AssetBundle cAssetBundle, AssetLoadedABClearCallback deleCallback)
        {
            if (null == cAssetBundle || null == deleCallback)
            {
                LogMgr.Instance.Log("AssetLoadedABCached::Init is failed. Param is invalid", LogType.Error);
                return;
            }

            m_cAssetBundle = cAssetBundle;

            if (null == m_listDepReference)
                m_listDepReference = new List<string>();

            ClearListDepReference();

            if (null == m_listByDepReference)
                m_listByDepReference = new List<string>();

            ClearListByDepReference();

            if (null == m_dictAssetLoadedCached)
                m_dictAssetLoadedCached = new Dictionary<string, AssetLoadedCached>();

            ClearDictAssetLoadedCached();

            m_deleCallback = deleCallback;
        }

        private void Release()
        {
            ClearListDepReference();
            m_listDepReference = null;
            ClearListByDepReference();
            m_listByDepReference = null;
            ClearDictAssetLoadedCached();
            m_dictAssetLoadedCached = null;

            if (null != m_cAssetBundle)
            {
                m_cAssetBundle.Unload(true);
                m_cAssetBundle = null;
            }
        }

        private void ClearListDepReference()
        {
            if (null == m_listDepReference)
                return;

            m_listDepReference.Clear();
        }

        private void ClearListByDepReference()
        {
            if (null == m_listByDepReference)
                return;

            m_listByDepReference.Clear();
        }

        private void ClearDictAssetLoadedCached()
        {
            if (null == m_dictAssetLoadedCached)
                return;

            var _varDictValue = m_dictAssetLoadedCached.GetEnumerator();
            while (_varDictValue.MoveNext())
            {
                if (null == _varDictValue.Current.Value)
                    continue;

                m_dictAssetLoadedCached[_varDictValue.Current.Key] = null;
            }
            _varDictValue.Dispose();

            m_dictAssetLoadedCached.Clear();
        }

        private AssetLoadedCached GetAssetLoadedCached(string strAssetName)
        {
            AssetLoadedCached _cAssetLoadedCached = null;
            if (false == m_dictAssetLoadedCached.TryGetValue(strAssetName, out _cAssetLoadedCached))
            {
                LogMgr.Instance.Log(string.Format("AssetLoadedABCached::GetAssetLoadedCached is failed. Not find Object. AssetName = {0}", strAssetName), LogType.Error);
                return _cAssetLoadedCached;
            }

            if (null == _cAssetLoadedCached)
            {
                LogMgr.Instance.Log(string.Format("AssetLoadedABCached::GetAssetLoadedCached is failed. Object is null. AssetName = {0}", strAssetName), LogType.Error);
                m_dictAssetLoadedCached.Remove(strAssetName);
                return _cAssetLoadedCached;
            }

            return _cAssetLoadedCached;
        }
        #endregion

        #region Public Function
        public void CachedAsset(string strAssetName, UnityEngine.Object cObj)
        {
            if (string.IsNullOrEmpty(strAssetName) || null == cObj)
            {
                LogMgr.Instance.Log("AssetLoadedABCached::CachedAsset is failed. Param is null reference", LogType.Error);
                return;
            }

            if (m_dictAssetLoadedCached.ContainsKey(strAssetName))
            {
                LogMgr.Instance.Log(string.Format("AssetLoadedABCached::CachedAsset is failed. Repeat cached asset. AssetName = {0}", strAssetName), LogType.Error);
                return;
            }

            m_dictAssetLoadedCached.Add(strAssetName, new AssetLoadedCached(cObj));
        }

        public UnityEngine.Object TryGetAssetObject(string strAssetName)
        {
            UnityEngine.Object _cResult = null;
            if (string.IsNullOrEmpty(strAssetName))
            {
                LogMgr.Instance.Log("AssetLoadedABCached::TryGetAssetObject is failed. Param is null reference", LogType.Error);
                return _cResult;
            }

            AssetLoadedCached _cAssetLoadedCached = null;
            if (false == m_dictAssetLoadedCached.TryGetValue(strAssetName, out _cAssetLoadedCached))
            {
                //LogMgr.Instance.Log(string.Format("AssetLoadedABCached::TryGetAssetObject is failed. Not find Object. AssetName = {0}", strAssetName), LogType.Error);
                return _cResult;
            }

            if (null == _cAssetLoadedCached)
            {
                LogMgr.Instance.Log(string.Format("AssetLoadedABCached::TryGetAssetObject is failed. Object is null. AssetName = {0}", strAssetName), LogType.Error);
                m_dictAssetLoadedCached.Remove(strAssetName);
                return _cResult;
            }

            _cResult = _cAssetLoadedCached.m_cAssetObject;
            return _cResult;
        }

        public void AddAssetReference(string strAssetName)
        {
            if (string.IsNullOrEmpty(strAssetName))
            {
                LogMgr.Instance.Log("AssetLoadedABCached::AddAssetReference is failed. Param is null reference", LogType.Error);
                return;
            }

            AssetLoadedCached _cAssetLoadedCached = GetAssetLoadedCached(strAssetName);
            if (null == _cAssetLoadedCached)
                return;

            _cAssetLoadedCached.AddRef();
        }

        public void SubAssetReference(string strAssetName)
        {
            if (string.IsNullOrEmpty(strAssetName))
            {
                LogMgr.Instance.Log("AssetLoadedABCached::SubAssetReference is failed. Param is null reference", LogType.Error);
                return;
            }

            AssetLoadedCached _cAssetLoadedCached = GetAssetLoadedCached(strAssetName);
            if (null == _cAssetLoadedCached)
                return;

            _cAssetLoadedCached.SubRef();
        }

        public void AddABDepReference(string strABName)
        {
            if (string.IsNullOrEmpty(strABName))
            {
                LogMgr.Instance.Log("AssetLoadedABCached::AddABDepReference is failed. Param is null reference", LogType.Error);
                return;
            }

            if (false == m_listDepReference.Contains(strABName))
                m_listDepReference.Add(strABName);
        }

        public void SubABDepReference(string strABName)
        {
            if (string.IsNullOrEmpty(strABName))
            {
                LogMgr.Instance.Log("AssetLoadedABCached::SubABDepReference is failed. Param is null reference", LogType.Error);
                return;
            }

            if (false == m_listDepReference.Contains(strABName))
            {
                LogMgr.Instance.Log(string.Format("AssetLoadedABCached::SubABDepReference is failed. Not find Object. ABName = {0}", strABName), LogType.Error);
                return;
            }

            m_listDepReference.Remove(strABName);
        }

        public void AddABByDepReference(string strABName)
        {
            if (string.IsNullOrEmpty(strABName))
            {
                LogMgr.Instance.Log("AssetLoadedABCached::AddABByDepReference is failed. Param is null reference", LogType.Error);
                return;
            }

            if (false == m_listByDepReference.Contains(strABName))
                m_listByDepReference.Add(strABName);
        }

        public void SubABByDepReference(string strABName)
        {
            if (string.IsNullOrEmpty(strABName))
            {
                LogMgr.Instance.Log("AssetLoadedABCached::SubABByDepReference is failed. Param is null reference", LogType.Error);
                return;
            }

            if (false == m_listByDepReference.Contains(strABName))
            {
                LogMgr.Instance.Log(string.Format("AssetLoadedABCached::SubABByDepReference is failed. Not find Object. ABName = {0}", strABName), LogType.Error);
                return;
            }

            m_listByDepReference.Remove(strABName);
        }

        public bool CheckValidity(bool bRecycleNow)
        {
            bool _bResult = false;
            var _varDictValue = m_dictAssetLoadedCached.GetEnumerator();
            while (_varDictValue.MoveNext())
            {
                if (null == _varDictValue.Current.Value)
                    continue;

                if (_varDictValue.Current.Value.CheckValidity(bRecycleNow))
                {
                    _bResult = true;
                    break;
                }
            }
            _varDictValue.Dispose();

            if (false == _bResult)
                m_deleCallback(m_listDepReference.ToArray(), m_listByDepReference.ToArray(), m_cAssetBundle.name);

            if (false == _bResult)
                _bResult = m_listByDepReference.Count == 0 ? false : true;

            if (false == _bResult)
                _bResult = m_listDepReference.Count == 0 ? false : true;

            return _bResult;
        }
        #endregion
    }
}