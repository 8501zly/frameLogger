using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using LitJson;
using UnityEngine;

namespace FrameLogger.Editor
{
	public class LogTrackJsonData
	{
		//日志ID
		public ushort ID { get; set; }
		//子路径
		public string FileName { get; set; }

		//函数名
		public string FuncName { get; set; }
		//参数类型
		public string ArgTypes { get; set; }
		//参数名字
		public string ArgNames { get; set; }

		//有效参数名字
		public string ValidArgNames { get; set; }
		//有效参数个数
		public int ValidArgCount { get; set; }
		//手动插入日志注释
		public string Comments { get; set; }
	}

	public class FrameLoggerPdbFile
	{
		private readonly Dictionary<ushort, bool> m_existLogIdDict = new Dictionary<ushort, bool>();

		//维护字典，对应要生成的LogPdb
		private readonly Dictionary<ushort, LogTrackJsonData> m_logTrackCodeInfoDic = new Dictionary<ushort, LogTrackJsonData>();
		//记录返回的ID
		private ushort m_returnID = 0;

		public bool CheckIdExist(ushort id)
		{
			if (m_logTrackCodeInfoDic.ContainsKey(id))
			{
				return false;
			}
			return true;
		}

		public void AddItem(ushort id, LogCodeInfo codeInfo)
		{
			var info = new LogTrackJsonData();
			info.ID = id;
			info.FileName = codeInfo.fileName;
			info.FuncName = codeInfo.funcName;
			info.ArgTypes = codeInfo.argTypes;
			info.ArgNames = codeInfo.argNames;
			info.ValidArgNames = codeInfo.validArgNames;
			info.ValidArgCount = codeInfo.validArgCount;
			info.Comments = codeInfo.comment;
			m_logTrackCodeInfoDic.Add(info.ID, info);
		}

		//返回最小hashId
		public ushort AddItem(LogCodeInfo codeInfo)
		{
			//从字典中取得尚未被占用的最小ID替换并返回
			while (true)
			{
				m_returnID++;
				if (!m_existLogIdDict.ContainsKey(m_returnID) && !m_logTrackCodeInfoDic.ContainsKey(m_returnID))
				{
					break;
				}
			}

			var info = new LogTrackJsonData();
			info.ID = m_returnID;
			info.FileName = codeInfo.fileName;
			info.FuncName = codeInfo.funcName;
			info.ArgTypes = codeInfo.argTypes;
			info.ArgNames = codeInfo.argNames;
			info.ValidArgNames = codeInfo.validArgNames;
			info.ValidArgCount = codeInfo.validArgCount;
			info.Comments = codeInfo.comment;
			m_logTrackCodeInfoDic.Add(info.ID, info);
			m_existLogIdDict.Add(info.ID, true);
			return m_returnID;
		}

		//写入LogPdb.json文件
		public void CreateLogPdb(string fullPath)
		{
			if (m_logTrackCodeInfoDic.Count == 0)
			{
				return;
			}
			string logPdbText = "[";
			//将新字典转换为Json文本
			foreach (var item in m_logTrackCodeInfoDic)
			{
				logPdbText += JsonMapper.ToJson(item.Value);
				logPdbText += ",\n";
			}
			//去除最后的,\n
			logPdbText = logPdbText.Substring(0, logPdbText.Length - 2);
			logPdbText += "\n]";
			//创建并写入LogPdb.json文件
			File.WriteAllText(fullPath, logPdbText);
		}

		public void ReadOldLogPdb(string fullPath)
		{
			m_existLogIdDict.Clear();
			if (!File.Exists(fullPath))
			{
				return;
			}
			var readAllText = File.ReadAllText(fullPath);
			var idMatch = new Regex("\"ID\":(\\d+)");
			var matchCollection = idMatch.Matches(readAllText);
			for (var i = 0; i < matchCollection.Count; i++)
			{
				var id = ushort.Parse(matchCollection[i].Groups[1].Value);
				m_existLogIdDict.Add(id, true);
			}
		}

		public void DeleteLogPdb(string fullPath)
		{
			if (!File.Exists(fullPath))
			{
				return;
			}
			File.Delete(fullPath);
		}
	}
}