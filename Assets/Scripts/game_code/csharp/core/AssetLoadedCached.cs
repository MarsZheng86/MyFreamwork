using UnityEngine;

namespace CORE
{
    public class AssetLoadedCached
    {
        #region Private Member Variable
        private int m_iAssetRefCount;
        private float m_fLatestTimeStamp;
        #endregion

        #region Public Member Variable
        public UnityEngine.Object m_cAssetObject { get; private set; }
        #endregion

        #region Constructor
        public AssetLoadedCached(UnityEngine.Object cObject)
        {
            Init(cObject);
        }

        ~AssetLoadedCached()
        {
            Release();
        }
        #endregion

        #region Private Function
        private void Init(UnityEngine.Object cObject)
        {
            if (null == cObject)
            {
                LogMgr.Instance.Log("AssetLoadedCached::Init is failed. Param is null reference", LogType.Error);
                return;
            }

            m_iAssetRefCount = 0;
            m_fLatestTimeStamp = cObject == null ? 0 : Time.realtimeSinceStartup;
            m_cAssetObject = cObject;
        }

        private void Release()
        {
            m_iAssetRefCount = 0;
            m_fLatestTimeStamp = 0;
            m_cAssetObject = null;
        }
        #endregion

        #region Public Function
        public void AddRef()
        {
            m_iAssetRefCount++;
        }

        public void SubRef()
        {
            if (0 >= m_iAssetRefCount)
            {
                LogMgr.Instance.Log("AssetLoadedCached::SubRef is failed. Reference count is failed.", LogType.Error);
                return;
            }

            m_iAssetRefCount--;
            if (0 == m_iAssetRefCount)
                m_fLatestTimeStamp = Time.realtimeSinceStartup;
        }

        public bool CheckValidity(bool bRecycleNow)
        {
            if (0 < m_iAssetRefCount)
                return false;
            else
                return bRecycleNow ? false : (Time.realtimeSinceStartup - m_fLatestTimeStamp < 60f);
        }
        #endregion
    }
}