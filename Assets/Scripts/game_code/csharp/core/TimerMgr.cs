using System;
using System.Collections.Generic;
using UnityEngine;

public class TimerMgr : Singleton<TimerMgr>
{
    #region Const
    private const int DEFAULT_TIME_COUNT = 10;
    #endregion

    #region Class
    private class CDelayedAction
    {
        public bool m_bDefault;
        public long m_lInstanceId;
        public float m_fElapseTime;
        public float m_fDelayTime;
        public Action m_cAction;
        public bool m_bScale;
        public int m_iCount;
        public bool m_bPause;
    }
    #endregion

    #region Private Member Variable
    private List<long> m_listDefaultInstanceId;
    private Dictionary<long, CDelayedAction> m_dictDelayedActions;
    private List<long> m_listDeleteDelayed;
    private bool m_bPauseTimerMgr;
    #endregion

    #region Constructor
    public TimerMgr() { }

    ~TimerMgr() { }
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

    public void LateUpdate()
    {
        if (true == m_bPauseTimerMgr)
            return;

        CalculateTime();
    }
    #endregion

    #region Private Function
    private void Init()
    {
        if (null == m_listDefaultInstanceId)
            m_listDefaultInstanceId = new List<long>();

        ClearListDefaultInstanceId();

        if (null == m_dictDelayedActions)
            m_dictDelayedActions = new Dictionary<long, CDelayedAction>();

        ClearDictDelayedActions();
        CreateDefualtTimer();

        if (null == m_listDeleteDelayed)
            m_listDeleteDelayed = new List<long>();

        ClearListDeleteDelayed();

        m_bPauseTimerMgr = false;
    }

    private void Release()
    {
        ClearDictDelayedActions();
        m_dictDelayedActions = null;
        ClearListDefaultInstanceId();
        m_listDefaultInstanceId = null;
        ClearListDeleteDelayed();
        m_listDeleteDelayed = null;
    }

    private void ClearDictDelayedActions()
    {
        if (null == m_dictDelayedActions)
            return;

        var _varDictValue = m_dictDelayedActions.GetEnumerator();
        while(_varDictValue.MoveNext())
        {
            if (null == _varDictValue.Current.Value)
                continue;

            _varDictValue.Current.Value.m_cAction = null;

        }
        _varDictValue.Dispose();

        m_dictDelayedActions.Clear();
    }

    private void ClearListDefaultInstanceId()
    {
        if (null == m_listDefaultInstanceId)
            return;

        m_listDefaultInstanceId.Clear();
    }

    private void ClearListDeleteDelayed()
    {
        if (null == m_listDeleteDelayed)
            return;

        m_listDeleteDelayed.Clear();
    }

    private void CreateDefualtTimer()
    {
        for (int i = 0; i < DEFAULT_TIME_COUNT; i++)
        {
            CDelayedAction _cDelayAction = new CDelayedAction();
            _cDelayAction.m_lInstanceId = Utility.CreateInstanceId();
            _cDelayAction.m_bDefault = true;
            _cDelayAction.m_fElapseTime = 0f;
            _cDelayAction.m_fDelayTime = 0f;
            _cDelayAction.m_cAction = null;
            _cDelayAction.m_bScale = false;
            _cDelayAction.m_iCount = 0;
            _cDelayAction.m_bPause = false;
            m_dictDelayedActions.Add(_cDelayAction.m_lInstanceId, _cDelayAction);
            m_listDefaultInstanceId.Add(_cDelayAction.m_lInstanceId);
        }
    }

    private void RefreshDelayedAction(long lInstanceId)
    {
        if (false == m_dictDelayedActions.ContainsKey(lInstanceId))
        {
            LogMgr.Instance.Log("TimerMgr::RefreshDelayedAction is failed. Not find key", LogType.Error);
            return;
        }

        m_dictDelayedActions[lInstanceId].m_fElapseTime = 0;
        m_dictDelayedActions[lInstanceId].m_fDelayTime = 0;
        m_dictDelayedActions[lInstanceId].m_cAction = null;
        m_dictDelayedActions[lInstanceId].m_bScale = false;
        m_dictDelayedActions[lInstanceId].m_iCount = 0;
        m_dictDelayedActions[lInstanceId].m_bPause = false;
    }

    private long GetValidityTimerKey()
    {
        long _lResultInstanceId = 0;

        for (int i = 0, _iCount = m_listDefaultInstanceId.Count; i < _iCount; i++)
        {
            if (false == m_dictDelayedActions.ContainsKey(m_listDefaultInstanceId[i]))
            {
                LogMgr.Instance.Log("TimerMgr::GetValidityTimerKey is failed. Not find defualt instance id", LogType.Error);
                continue;
            }

            if (null == m_dictDelayedActions[m_listDefaultInstanceId[i]].m_cAction)
            {
                _lResultInstanceId = m_listDefaultInstanceId[i];
                break;
            }
        }

        return _lResultInstanceId;
    }

    private void CalculateTime()
    {
        m_listDeleteDelayed.Clear();

        var _varDictValue = m_dictDelayedActions.GetEnumerator();
        while(_varDictValue.MoveNext())
        {
            if (null == _varDictValue.Current.Value)
            {
                LogMgr.Instance.Log("TimerMgr is failed. Timer is null", LogType.Error);
                continue;
            }

            if (null == _varDictValue.Current.Value.m_cAction)
                continue;

            if (true == _varDictValue.Current.Value.m_bPause)
                continue;

            _varDictValue.Current.Value.m_fElapseTime += _varDictValue.Current.Value.m_bScale ? Time.deltaTime : Time.unscaledDeltaTime;
            if (_varDictValue.Current.Value.m_fElapseTime >= _varDictValue.Current.Value.m_fDelayTime)
            {
                _varDictValue.Current.Value.m_cAction.Invoke();
                _varDictValue.Current.Value.m_fElapseTime -= _varDictValue.Current.Value.m_fDelayTime;
                _varDictValue.Current.Value.m_iCount--;

                if (0 == _varDictValue.Current.Value.m_iCount)
                {
                    if (true == _varDictValue.Current.Value.m_bDefault)
                        RefreshDelayedAction(_varDictValue.Current.Key);
                    else
                        m_listDeleteDelayed.Add(_varDictValue.Current.Key);
                }
            }
        }
        _varDictValue.Dispose();

        for (int i = 0, _iCount = m_listDeleteDelayed.Count; i < _iCount; i++)
            m_dictDelayedActions.Remove(m_listDeleteDelayed[i]);
    }
    #endregion

    #region Public Function
    public long AddTimer(float fDelayTime, Action cActionCallback, int iCount = 1, bool bScale = false)
    {
        long _lInstanceId = 0;
        CDelayedAction _cDelayedAction = null;
        if (null == cActionCallback)
        {
            LogMgr.Instance.Log("TimerMgr::AddTimer is failed. Param is null reference", LogType.Error);
            return _lInstanceId;
        }

        _lInstanceId = GetValidityTimerKey();
        m_dictDelayedActions.TryGetValue(_lInstanceId, out _cDelayedAction);

        if(null == _cDelayedAction)
        {
            _cDelayedAction = new CDelayedAction();
            _cDelayedAction.m_lInstanceId = Utility.CreateInstanceId();
            _cDelayedAction.m_bDefault = false;
            m_dictDelayedActions.Add(_cDelayedAction.m_lInstanceId, _cDelayedAction);
        }

        _cDelayedAction.m_fElapseTime = 0f;
        _cDelayedAction.m_fDelayTime = fDelayTime;
        _cDelayedAction.m_cAction = cActionCallback;
        _cDelayedAction.m_bScale = bScale;
        _cDelayedAction.m_iCount = iCount;
        _cDelayedAction.m_bPause = false;
        _lInstanceId = _cDelayedAction.m_lInstanceId;
        return _lInstanceId;
    }

    public bool DeleteTimer(long lInstanceId, bool bCallback = false)
    {
        bool _bResult = false;
        CDelayedAction _cDelayedAction = null;
        m_dictDelayedActions.TryGetValue(lInstanceId, out _cDelayedAction);

        if (null == _cDelayedAction)
        {
            LogMgr.Instance.Log("TimerMgr::DeleteTimer is failed. Not find object.", LogType.Error);
            return _bResult;
        }

        if (true == bCallback)
            _cDelayedAction.m_cAction.Invoke();

        _cDelayedAction.m_cAction = null;

        if(false == _cDelayedAction.m_bDefault)
            m_dictDelayedActions.Remove(_cDelayedAction.m_lInstanceId);
        else
            RefreshDelayedAction(_cDelayedAction.m_lInstanceId);

        _bResult = true;
        return _bResult;
    }

    public bool CheckValidityInstanceId(long lInstanceId)
    {
        if (m_dictDelayedActions.ContainsKey(lInstanceId))
            return true;
        else
            return false;
    }

    public void PauseTimerMgr(bool bPause)
    {
        if (m_bPauseTimerMgr == bPause)
            return;

        m_bPauseTimerMgr = bPause;
    }

    public void PauseTimer(long lInstanceId, bool bPause)
    {
        CDelayedAction _cDelayedAction = null;
        m_dictDelayedActions.TryGetValue(lInstanceId, out _cDelayedAction);

        if (null == _cDelayedAction)
        {
            LogMgr.Instance.Log("TimerMgr::PauseTimer is failed. Not find key", LogType.Error);
            return;
        }

        if (_cDelayedAction.m_bPause == bPause)
            return;

        _cDelayedAction.m_bPause = bPause;
    }
    #endregion
}