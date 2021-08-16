using System.Collections.Generic;
using CORE;
using UnityEngine;

public class CharacterConfig : ConfigBase
{
	#region Class
	public class CCharacterCfgData
	{
		public string key;	//	string key
		public int id;	// id
		public int is_passive;	// 是否被动
		public int is_exclusive;	// “是否专属”
		public int fragment;	// 消耗碎片数量
		public int gold;	// 消耗金币
		public string skillcard;	// 对应的技能卡id
		public int rarity;	// 技能稀有度
	}
	#endregion

	#region Private Member Variable
	private Dictionary<string, CCharacterCfgData> m_dictCfgData;
	#endregion

	#region Constructor
	public CharacterConfig()
	{
		 Init();
	}

	~CharacterConfig()
	{
		Release();
	}
	#endregion

	#region Private Function
	private void Init()
	{
		m_strCfgKey = typeof(CharacterConfig).Name;
		m_strCfgName = string.Format("{0}.txt", m_strCfgKey);

		if (null == m_dictCfgData)
			m_dictCfgData = new Dictionary<string, CCharacterCfgData>();

		LoadConfig();
	}

	private void Release()
	{
		ClearDictCfgData();
	}

	private void ClearDictCfgData()
	{
		if (null == m_dictCfgData)
			return;

		m_dictCfgData.Clear();
	}

	private CCharacterCfgData ParseData(string strCfgData)
	{
		string[] _strArr = strCfgData.Split('\t');
		CCharacterCfgData _cCharacterCfgData = new CCharacterCfgData();
		_cCharacterCfgData.key = _strArr[0];
		_cCharacterCfgData.id = int.Parse(_strArr[0]);
		_cCharacterCfgData.is_passive = int.Parse(_strArr[1]);
		_cCharacterCfgData.is_exclusive = int.Parse(_strArr[2]);
		_cCharacterCfgData.fragment = int.Parse(_strArr[3]);
		_cCharacterCfgData.gold = int.Parse(_strArr[4]);
		_cCharacterCfgData.skillcard = _strArr[5];
		_cCharacterCfgData.rarity = int.Parse(_strArr[6]);
		return _cCharacterCfgData;
	}
	#endregion

	#region Protected Override
	protected override void LoadConfig()
	{
		string _strFilePath = string.Format("{0}/{1}", Utility.GetConfigFilePath(), m_strCfgName);
		string _strText = string.Empty;

		try
		{
			_strText = System.IO.File.ReadAllText(_strFilePath);
		}
		catch (System.Exception ex)
		{
			LogMgr.Instance.Log(ex.Message, LogType.Error);
		}

		if (string.IsNullOrEmpty(_strText))
		{
			 LogMgr.Instance.Log("CharacterConfig::LoadConfig is failed. Load txt is empty", LogType.Error);
			 return;
		}

		string[] _strArrText = _strText.Split('\n');

		for (int i = 3, _iCount = _strArrText.Length; i < _iCount; i++)
		{
			if (string.IsNullOrEmpty(_strArrText[i]))
				continue;

			CCharacterCfgData _cCharacterCfgData = new CCharacterCfgData();
			_cCharacterCfgData = ParseData(_strArrText[i]);
			m_dictCfgData.Add(_cCharacterCfgData.key, _cCharacterCfgData);
		}
	}
	#endregion

	#region Public Function
	public CCharacterCfgData GetData(int iKey)
	{
		return GetData(iKey.ToString());
	}

	public CCharacterCfgData GetData(string strKey)
	{
		CCharacterCfgData _cCharacterCfgData = null;
		m_dictCfgData.TryGetValue(strKey, out _cCharacterCfgData);
		return _cCharacterCfgData;
	}

	public CCharacterCfgData[] GetAllData()
	{
		return new List<CCharacterCfgData>(m_dictCfgData.Values).ToArray();
	}
	#endregion
}
