using System.IO;
using UnityEditor;

namespace FrameLogger.Editor
{
	public static partial class LogUtil
	{
		[MenuItem("战斗系统工具/FrameLogger/删除日志代码")]
		private static void DeleteAllInsertCode()
		{
			s_trackPdb = new FrameLoggerPdbFile();
			var allSearchFile = SearchFileUtil.GetAllSearchFile();

			//设置需要插入代码的文件夹路径
			for (var i = 0; i < allSearchFile.Count; i++)
			{
				DeleteAutoInsertCode(s_baseDir+allSearchFile[i]);
			}
			s_trackPdb.DeleteLogPdb(LogUtilConfig.s_logPdbFilePath);
		}

		private static void DeleteAutoInsertCode(string fullSubPath)
		{
			var text = File.ReadAllText(fullSubPath, s_utf8Encoding);
			var matches = s_regexFuncAll.Matches(text);
			int cnt = matches.Count;
			for (int j = cnt - 1; j >= 0; j--)
			{
				//某个具体函数
				var matchFuncAll = matches[j];

				//匹配到函数的左大括号
				var matchLeftBrace = s_regexLeftBrace.Match(text, matchFuncAll.Index, matchFuncAll.Length);
				//匹配成功
				if (matchLeftBrace.Success)
				{
					int len = matchFuncAll.Index + matchFuncAll.Length - (matchLeftBrace.Index + matchLeftBrace.Length);
					//找到第一行代码
					var matchFirstCode = s_regexFirstCode.Match(text, matchLeftBrace.Index, len);
					if (matchFirstCode.Success)
					{
						//如果第一行代码是 忽略日志
						if (s_regexIgnoreTrackCode.IsMatch(matchFirstCode.Value) || s_regexHandInsertTrackCode.IsMatch(matchFirstCode.Value))
						{
							continue;
						}

						var logTrackCodeMatch = s_regexLogTrackCode.Match(matchFirstCode.Value);
						//首行代码为日志代码，删掉
						if (logTrackCodeMatch.Success)
						{
							text = text.Remove(matchFirstCode.Index + logTrackCodeMatch.Index, logTrackCodeMatch.Length);
						}
					}
				}
			}
			File.WriteAllText(fullSubPath, text, s_utf8Encoding);
		}
	}
}