using System.Collections.Generic;
using CORE;
using UnityEngine;

public class EffectConfig : ConfigBase
{
	#region Class
	public class CEffectCfgData
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
	private Dictionary<string, CEffectCfgData> m_dictCfgData;
	#endregion

	#region Constructor
	public EffectConfig()
	{
		 Init();
	}

	~EffectConfig()
	{
		Release();
	}
	#endregion

	#region Private Function
	private void Init()
	{
		m_strCfgKey = typeof(EffectConfig).Name;
		m_strCfgName = string.Format("{0}.txt", m_strCfgKey);

		if (null == m_dictCfgData)
			m_dictCfgData = new Dictionary<string, CEffectCfgData>();

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

	private CEffectCfgData ParseData(string strCfgData)
	{
		string[] _strArr = strCfgData.Split('\t');
		CEffectCfgData _cEffectCfgData = new CEffectCfgData();
		_cEffectCfgData.key = _strArr[0];
		_cEffectCfgData.id = int.Parse(_strArr[0]);
		_cEffectCfgData.is_passive = int.Parse(_strArr[1]);
		_cEffectCfgData.is_exclusive = int.Parse(_strArr[2]);
		_cEffectCfgData.fragment = int.Parse(_strArr[3]);
		_cEffectCfgData.gold = int.Parse(_strArr[4]);
		_cEffectCfgData.skillcard = _strArr[5];
		_cEffectCfgData.rarity = int.Parse(_strArr[6]);
		return _cEffectCfgData;
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
			 LogMgr.Instance.Log("EffectConfig::LoadConfig is failed. Load txt is empty", LogType.Error);
			 return;
		}

		string[] _strArrText = _strText.Split('\n');

		for (int i = 3, _iCount = _strArrText.Length; i < _iCount; i++)
		{
			if (string.IsNullOrEmpty(_strArrText[i]))
				continue;

			CEffectCfgData _cEffectCfgData = new CEffectCfgData();
			_cEffectCfgData = ParseData(_strArrText[i]);
			m_dictCfgData.Add(_cEffectCfgData.key, _cEffectCfgData);
		}
	}
	#endregion

	#region Public Function
	public CEffectCfgData GetData(int iKey)
	{
		return GetData(iKey.ToString());
	}

	public CEffectCfgData GetData(string strKey)
	{
		CEffectCfgData _cEffectCfgData = null;
		m_dictCfgData.TryGetValue(strKey, out _cEffectCfgData);
		return _cEffectCfgData;
	}

	public CEffectCfgData[] GetAllData()
	{
		return new List<CEffectCfgData>(m_dictCfgData.Values).ToArray();
	}
	#endregion
}
