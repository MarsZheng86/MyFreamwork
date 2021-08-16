using System.Collections.Generic;
using CORE;
using UnityEngine;

public class BuffConfig : ConfigBase
{
	#region Class
	public class CBuffCfgData
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
	private Dictionary<string, CBuffCfgData> m_dictCfgData;
	#endregion

	#region Constructor
	public BuffConfig()
	{
		 Init();
	}

	~BuffConfig()
	{
		Release();
	}
	#endregion

	#region Private Function
	private void Init()
	{
		m_strCfgKey = typeof(BuffConfig).Name;
		m_strCfgName = string.Format("{0}.txt", m_strCfgKey);

		if (null == m_dictCfgData)
			m_dictCfgData = new Dictionary<string, CBuffCfgData>();

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

	private CBuffCfgData ParseData(string strCfgData)
	{
		string[] _strArr = strCfgData.Split('\t');
		CBuffCfgData _cBuffCfgData = new CBuffCfgData();
		_cBuffCfgData.key = _strArr[0];
		_cBuffCfgData.id = int.Parse(_strArr[0]);
		_cBuffCfgData.is_passive = int.Parse(_strArr[1]);
		_cBuffCfgData.is_exclusive = int.Parse(_strArr[2]);
		_cBuffCfgData.fragment = int.Parse(_strArr[3]);
		_cBuffCfgData.gold = int.Parse(_strArr[4]);
		_cBuffCfgData.skillcard = _strArr[5];
		_cBuffCfgData.rarity = int.Parse(_strArr[6]);
		return _cBuffCfgData;
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
			 LogMgr.Instance.Log("BuffConfig::LoadConfig is failed. Load txt is empty", LogType.Error);
			 return;
		}

		string[] _strArrText = _strText.Split('\n');

		for (int i = 3, _iCount = _strArrText.Length; i < _iCount; i++)
		{
			if (string.IsNullOrEmpty(_strArrText[i]))
				continue;

			CBuffCfgData _cBuffCfgData = new CBuffCfgData();
			_cBuffCfgData = ParseData(_strArrText[i]);
			m_dictCfgData.Add(_cBuffCfgData.key, _cBuffCfgData);
		}
	}
	#endregion

	#region Public Function
	public CBuffCfgData GetData(int iKey)
	{
		return GetData(iKey.ToString());
	}

	public CBuffCfgData GetData(string strKey)
	{
		CBuffCfgData _cBuffCfgData = null;
		m_dictCfgData.TryGetValue(strKey, out _cBuffCfgData);
		return _cBuffCfgData;
	}

	public CBuffCfgData[] GetAllData()
	{
		return new List<CBuffCfgData>(m_dictCfgData.Values).ToArray();
	}
	#endregion
}
