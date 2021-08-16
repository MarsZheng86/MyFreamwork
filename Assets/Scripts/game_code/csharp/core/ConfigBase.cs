namespace CORE
{
    public abstract class ConfigBase
    {
        #region Public Member Variable
        public string m_strCfgKey;
        #endregion

        #region Protected Member Variable
        protected string m_strCfgName;
        #endregion

        #region Constructor
        public ConfigBase () { }

        ~ConfigBase() { }
        #endregion

        #region Protected Function
        protected abstract void LoadConfig();
        #endregion
    }
}