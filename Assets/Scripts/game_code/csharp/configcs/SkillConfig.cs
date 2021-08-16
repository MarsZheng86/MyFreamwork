using System.Collections.Generic;
using CORE;
using UnityEngine;

public class SkillConfig : ConfigBase
{
	#region Class
	public class CSkillCfgData
	{
		public string key;	//	string key
		public int id;	// "id"
		public int is_passive;	// "是否被动"
		public int is_exclusive;	// “是否专属”
		public int fragment;	// "消耗碎片数量"
		public int gold;	// "消耗金币"
		public string skillcard;	// "对应的技能卡id"
		public int rarity;	// "技能稀有度"
	}
	#endregion

	#region Private Member Variable
	private Dictionary<string, CSkillCfgData> m_dictCfgData;
	#endregion

	#region Constructor
	public SkillConfig()
	{
		 Init();
	}

	~SkillConfig()
	{
		Release();
	}
	#endregion

	#region Private Function
	private void Init()
	{
		m_strCfgKey = typeof(SkillConfig).Name;
		m_strCfgName = string.Format("{0}.txt", m_strCfgKey);

		if (null == m_dictCfgData)
			m_dictCfgData = new Dictionary<string, CSkillCfgData>();

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

	private CSkillCfgData ParseData(string strCfgData)
	{
		string[] _strArr = strCfgData.Split('\t');
		CSkillCfgData _cSkillCfgData = new CSkillCfgData();
		_cSkillCfgData.key = _strArr[0];
		_cSkillCfgData.id = int.Parse(_strArr[0]);
		_cSkillCfgData.is_passive = int.Parse(_strArr[1]);
		_cSkillCfgData.is_exclusive = int.Parse(_strArr[2]);
		_cSkillCfgData.fragment = int.Parse(_strArr[3]);
		_cSkillCfgData.gold = int.Parse(_strArr[4]);
		_cSkillCfgData.skillcard = _strArr[5];
		_cSkillCfgData.rarity = int.Parse(_strArr[6]);
		return _cSkillCfgData;
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
			 LogMgr.Instance.Log("SkillConfig::LoadConfig is failed. Load txt is empty", LogType.Error);
			 return;
		}

		string[] _strArrText = _strText.Split('\n');

		for (int i = 3, _iCount = _strArrText.Length; i < _iCount; i++)
		{
			if (string.IsNullOrEmpty(_strArrText[i]))
				continue;

			CSkillCfgData _cSkillCfgData = new CSkillCfgData();
			_cSkillCfgData = ParseData(_strArrText[i]);
			m_dictCfgData.Add(_cSkillCfgData.key, _cSkillCfgData);
		}
	}
	#endregion

	#region Public Function
	public CSkillCfgData GetData(int iKey)
	{
		return GetData(iKey.ToString());
	}

	public CSkillCfgData GetData(string strKey)
	{
		CSkillCfgData _cSkillCfgData = null;
		m_dictCfgData.TryGetValue(strKey, out _cSkillCfgData);
		return _cSkillCfgData;
	}

	public CSkillCfgData[] GetAllData()
	{
		return new List<CSkillCfgData>(m_dictCfgData.Values).ToArray();
	}
	#endregion
}
