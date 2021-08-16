namespace BATTLE
{
    public class BattleMgr : Singleton<BattleMgr>
    {
        #region Delegate
        public delegate void StartBattleCallback();
        #endregion

        #region Private Member Variable
        private int m_iBattleCfgId;
        private BattleCharMgr m_cBattleCharMgr;
        #endregion

        #region Constructor
        public BattleMgr() { }

        ~BattleMgr() { }
        #endregion

        #region Inherit
        public void Awake()
        {
            Init();
        }

        private void Update()
        {

        }

        public void OnDestroy()
        {
            Release();
        }
        #endregion

        #region Private Function
        private void Init()
        {
            m_iBattleCfgId = 0;
            m_cBattleCharMgr = new BattleCharMgr();
        }

        private void Release()
        {
            m_iBattleCfgId = 0;
            m_cBattleCharMgr = null;
        }
        #endregion

        #region Public Function
        public void StartBattle(int iBattleCfgId, StartBattleCallback deleCallback)
        {
            m_iBattleCfgId = iBattleCfgId;

            // TODO.. create battle character and monster
            // TODO.. create skill object

            if (null != deleCallback)
                deleCallback.Invoke();
        }

        public void EndBattle()
        {
            // TODO.. remove character 
        }

        public void PauseBattleStatus(bool bPause)
        {
            if (true == bPause)
            {

            }
            else
            {

            }
        }
        #endregion
    }
}

