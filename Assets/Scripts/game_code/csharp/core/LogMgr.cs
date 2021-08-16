using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class LogMgr : Singleton<LogMgr>
{
    #region Private Class
    private class LogItem
    {
        public string m_strOutput;
        public string m_strStack;
        public LogType m_eType;
    }
    #endregion

    #region Private Member Variable
    private readonly Queue<LogItem> m_queueLog = new Queue<LogItem>();
    private readonly object m_cLogQueueSync = new object();
    private readonly ManualResetEvent m_cLogCtrl = new ManualResetEvent(false);
    private Thread m_threadLog;
    private volatile bool m_bLogThreadRunning = true;
    private StreamWriter m_streamLogWriter = null;
    #endregion

    #region Constructor
    public LogMgr(){ }

    ~LogMgr() { }
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
        // create log thread
        if (m_threadLog == null)
        {
            m_threadLog = new Thread(new ThreadStart(LogThreadFunc))
            {
                Name = "LogMgr"
            };

            // create log stream writer
            System.DateTime _now = System.DateTime.Now;
            string sFileName = string.Format("log_{0}_{1}_{2}_{3}_{4}_{5}.txt", _now.Year, _now.Month, _now.Day, _now.Hour, _now.Minute, _now.Second);
            string outputPath = Path.Combine(Application.persistentDataPath, sFileName);
            m_streamLogWriter = new StreamWriter(new FileStream(outputPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                AutoFlush = true
            };

            // add log listener
            Application.logMessageReceivedThreaded += HandleLog;
            m_threadLog.Start();
        }
    }

    public void Release()
    {
        // release log thread
        if (m_threadLog != null)
        {
            m_bLogThreadRunning = false;

            lock (m_cLogQueueSync)
            {
                m_cLogCtrl.Set();
            }

            if (m_threadLog.ThreadState == ThreadState.Running)
            {
                m_threadLog.Join();
            }

            m_threadLog = null;
        }

        // close log stream writer
        if (m_streamLogWriter != null)
        {
            m_streamLogWriter.Close();
            m_streamLogWriter = null;
        }

        // remove log listener
        Application.logMessageReceivedThreaded -= HandleLog;
    }

    private void HandleLog(string output, string stack, LogType type)
    {
        LogItem item = new LogItem
        {
            m_strOutput = output,
            m_strStack = stack,
            m_eType = type
        };

        lock (m_cLogQueueSync)
        {
            m_queueLog.Enqueue(item);
            m_cLogCtrl.Set();
        }
    }

    private void LogThreadFunc()
    {
        while (m_bLogThreadRunning)
        {
            m_cLogCtrl.WaitOne();

            do
            {
                lock (m_cLogQueueSync)
                {
                    if (m_queueLog.Count == 0 && m_bLogThreadRunning)
                    {
                        m_cLogCtrl.Reset();
                        break;
                    }

                    while (m_queueLog.Count > 0)
                    {
                        LogItem item = m_queueLog.Dequeue();
                        ExecuteLogItem(item);
                    }
                }
            } while (false);
        }
    }

    private void ExecuteLogItem(LogItem item)
    {
        m_streamLogWriter.WriteLine(item.m_strOutput);
        if (item.m_eType != LogType.Log && item.m_eType != LogType.Warning)
        {
            m_streamLogWriter.WriteLine(item.m_strStack);
        }
    }
    #endregion

    #region Public Function
    public void Log(string strLogText, LogType eType = LogType.Log)
    {
        if (false == ProjectSettings.Instance.m_bDevLog)
        {
            if (LogType.Error != eType)
                return;
        }

        switch(eType)
        {
            case LogType.Log:
            case LogType.Assert:
            case LogType.Exception:
                Debug.Log(strLogText);
                break;
            case LogType.Warning:
                Debug.LogWarning(strLogText);
                break;
            case LogType.Error:
                Debug.LogError(strLogText);
                break;
            default:
                break;
        }
    }
    #endregion
}