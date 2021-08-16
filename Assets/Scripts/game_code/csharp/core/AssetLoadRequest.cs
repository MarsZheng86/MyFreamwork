using System.Collections.Generic;
using UnityEngine;

namespace CORE
{
    public class AssetLoadRequest
    {
        #region Public Delegate
        public delegate void AssetLoadedCallBack(UnityEngine.Object[] cObj);
        #endregion

        #region Private Member Variable
        private AssetLoadedCallBack m_deleCallback;
        private List<UnityEngine.Object> m_listLoadedAsset;
        #endregion

        #region Public Member Variable
        public string m_strABName { get; private set; }
        public string[] m_strArrAssetName { get; private set; }
        #endregion

        #region Constructor
        public AssetLoadRequest(string strABName, string[] strArrAssetName, AssetLoadedCallBack deleCallback)
        {
            Init(strABName, strArrAssetName, deleCallback);
        }

        ~AssetLoadRequest()
        {
            Release();
        }
        #endregion

        #region Privat FunctionB
        private void Init(string strABName, string[] strArrAssetName, AssetLoadedCallBack deleCallback)
        {
            m_strABName = strABName;
            ExcludeSameAssetName(strArrAssetName);
            m_deleCallback = deleCallback;

            if (null == m_listLoadedAsset)
                m_listLoadedAsset = new List<Object>();

            ClearListLoadedAsset();
        }

        private void Release()
        {
            m_strABName = null;
            m_strArrAssetName = null;
            m_deleCallback = null;
            ClearListLoadedAsset();
            m_listLoadedAsset = null;
        }

        private void ClearListLoadedAsset()
        {
            if (null == m_listLoadedAsset)
                return;

            m_listLoadedAsset.Clear();
        }

        private void ExcludeSameAssetName(string[] strArrAssetName)
        {
            if (false == Utility.CheckArrayValidity(strArrAssetName))
                return;

            List<string> _list = new List<string>();
            for (int i = 0, iCount = strArrAssetName.Length; i < iCount; i++)
            {
                if (true == _list.Contains(strArrAssetName[i]))
                    continue;

                _list.Add(strArrAssetName[i]);
            }

            m_strArrAssetName = _list.ToArray();
        }
        #endregion

        #region Public Function
        public bool CheckValidity()
        {
            bool _bResult = false;
            if (string.IsNullOrEmpty(m_strABName))
            {
                LogMgr.Instance.Log("AssetLoadRequest::CheckValidity is failed. ABName is invalid", LogType.Error);
                return _bResult;
            }

            _bResult = Utility.CheckArrayValidity(m_strArrAssetName);
            if (false == _bResult)
                LogMgr.Instance.Log("ssetLoadRequest::CheckValidity is failed. ArrAssetName is invalid", LogType.Error);

            return _bResult;
        }

        public void AddLoadedAsset(UnityEngine.Object cObj)
        {
            if (true == m_listLoadedAsset.Contains(cObj))
                return;

            m_listLoadedAsset.Add(cObj);
        }

        public void RemoveLoadedAsset(UnityEngine.Object cObj)
        {
            if (false == m_listLoadedAsset.Contains(cObj))
                return;

            m_listLoadedAsset.Remove(cObj);
        }

        public void DoDeleCallback()
        {
            if (null == m_deleCallback)
                return;

            m_deleCallback.Invoke(m_listLoadedAsset.ToArray());
        }

        public void Abandon()
        {
            ClearListLoadedAsset();
        }
        #endregion
    }
}