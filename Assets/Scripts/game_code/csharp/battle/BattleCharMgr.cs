using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BATTLE
{
    public class BattleCharMgr
    {
        #region Private Member Variable
        private Dictionary<long, BattleCharacter> m_dictBattleChar;
        #endregion

        #region Constructor
        public BattleCharMgr()
        {
            Init();
        }

        ~BattleCharMgr()
        {
            Release();
        }
        #endregion

        #region Private Function
        private void Init()
        {
            if (null == m_dictBattleChar)
                m_dictBattleChar = new Dictionary<long, BattleCharacter>();

            ClearDictBattleChar();
        }

        private void Release()
        {
            ClearDictBattleChar();
            m_dictBattleChar = null;
        }

        private void ClearDictBattleChar()
        {
            if (null == m_dictBattleChar)
                return;

            var _varDictObject = m_dictBattleChar.GetEnumerator();
            while(_varDictObject.MoveNext())
            {
                if (null != _varDictObject.Current.Value)
                {
                    BattleCharacter _cBattleChar = _varDictObject.Current.Value;
                    _cBattleChar = null;
                }
            }

            m_dictBattleChar.Clear();
        }
        #endregion

        #region Public Function
        public void AddCharacter(long lInstnaceId, BattleCharacter cBattleChar)
        {
            if (true == m_dictBattleChar.ContainsKey(lInstnaceId))
            {
                LogMgr.Instance.Log("BattleCharMgr::AddCharacter is failed. Repeat add character", LogType.Error);
                return;
            }

            m_dictBattleChar.Add(lInstnaceId, cBattleChar);
        }

        public void RemoveCharacter(long lInstanceId)
        {
            BattleCharacter _cBattleChar = null;
            if (false == m_dictBattleChar.TryGetValue(lInstanceId, out _cBattleChar))
            {
                LogMgr.Instance.Log(string.Format("BattleCharMgr::RemoveCharacter is failed. Not find Object. InstanceId =  {0}", lInstanceId), LogType.Error);
                return;
            }

            m_dictBattleChar.Remove(lInstanceId);
        }

        public BattleCharacter FindCharacter(long lInstanceId)
        {
            BattleCharacter _cBattleChar = null;
            if (false == m_dictBattleChar.TryGetValue(lInstanceId, out _cBattleChar))
            {
                LogMgr.Instance.Log(string.Format("BattleCharMgr::FindCharacter is failed. Not find Object. InstanceId =  {0}", lInstanceId), LogType.Error);
                return _cBattleChar;
            }

            return _cBattleChar;
        }

        public void CreateBattleCharacter(int iCfgId)
        {

        }
        #endregion
    }
}