public class ProjectSettings : Singleton<ProjectSettings>
{
    #region Public Const
    #endregion

    #region Public Member Variable
    public EBuildType m_eBuildType = EBuildType.DEVELOPER;
    public bool m_bDev = true;
    public bool m_bDevLog = true;
    #endregion
}