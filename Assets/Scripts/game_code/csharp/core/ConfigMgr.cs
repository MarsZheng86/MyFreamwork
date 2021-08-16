using System.Collections.Generic;
using UnityEngine;

namespace CORE
{
    public class ConfigMgr : Singleton<ConfigMgr>
    {
        #region Private Member Variable
        private Dictionary<string, ConfigBase> m_dictConfig;
        #endregion

        #region Constructor
        public ConfigMgr() { }
        ~ConfigMgr() { }
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
            if (null == m_dictConfig)
                m_dictConfig = new Dictionary<string, ConfigBase>();

            ClearDictConfig();

            InitSkillCfg();
            InitCharacterCfg();
        }

        private void Release()
        {
            ClearDictConfig();
        }

        private void ClearDictConfig()
        {
            if (null == m_dictConfig)
                return;

            m_dictConfig.Clear();
        }

        private void InitSkillCfg()
        {
            SkillConfig _cSkillConfig = new SkillConfig();
            m_dictConfig.Add(_cSkillConfig.m_strCfgKey, _cSkillConfig);
        }

        private void InitCharacterCfg()
        {
            CharacterConfig _cCharacterConfig = new CharacterConfig();
            m_dictConfig.Add(_cCharacterConfig.m_strCfgKey, _cCharacterConfig);
        }
        #endregion

        #region Public Function
        public ConfigBase GetConfigDict<T>()
        {
            ConfigBase _cConfigBase = null;
            if (false == m_dictConfig.ContainsKey(typeof(T).Name))
            {
                LogMgr.Instance.Log(string.Format("ConfigMgr::GetConfigDict is failed. Not find Object. Key name = {0}", typeof(object).Name), LogType.Error);
                return _cConfigBase;
            }

            m_dictConfig.TryGetValue(typeof(T).Name, out _cConfigBase);
            return _cConfigBase;
        }
        #endregion
    }
}