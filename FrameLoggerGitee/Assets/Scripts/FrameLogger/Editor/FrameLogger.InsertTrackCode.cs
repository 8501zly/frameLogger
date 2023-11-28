using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace FrameLogger.Editor
{
    /// <summary>
    ///     每行日志代码的信息
    /// </summary>
    public class LogCodeInfo
    {
        //参数名称 逗号分隔
        public string argNames;

        //参数类型 逗号分隔
        public string argTypes;

        //手动插入代码注释
        public string comment;

        //文件名
        public string fileName;

        //函数名
        public string funcName;

        //每行日志代码的唯一Id
        public int hashId;

        //生成的插入函数头的代码
        public string logTrackCode;

        //0：自动插入； 1：手动插入
        public int type;

        //有效参数个数
        public int validArgCount;

        //有效参数名称 逗号分隔
        public string validArgNames;
    }

    public static partial class LogUtil
    {
        private static readonly UTF8Encoding s_utf8Encoding = new UTF8Encoding(true);

        //匹配类名
        private static Regex s_regexClassName = new Regex(@"(?<=class)\s+\w+(?=[\s\n]*{)");

        //整个函数的匹配模式串
        private static readonly Regex s_regexFuncAll = new Regex(@"(public|private|protected)((\s+(static|override|virtual)*\s+)|\s+)\w+(<\w+>)*(\[\])*\s+\w+(<\w+>)*\s*\(([^\)]+\s*)?\)\s*\{[^\{\}]*(((?'Open'\{)[^\{\}]*)+((?'-Open'\})[^\{\}]*)+)*(?(Open)(?!))\}");

        //函数头的匹配模式串
        private static readonly Regex s_regexFuncHead = new Regex(@"(public|private|protected)((\s+(static|override|virtual)*\s+)|\s+)\w+(<\w+>)*(\[\])*\s+\w+(<\w+>)*\s*\(([^\)]+\s*)?\)");

        //匹配函数头之后的第一个大括号模式串
        private static readonly Regex s_regexLeftBrace = new Regex(@"\{");

        //第一句代码模式串
        private static readonly Regex s_regexFirstCode = new Regex(@"(\{\s*[^;]+;)(\s*\/\*(.)*\*\/)?");

        //插入的日志代码模式串
        private static readonly Regex s_regexLogTrackCode = new Regex(@"\s*(FrameLogger.EvolutionManager.)(LogTrack)\(([^;]+\s*)?\)(\})?\s*;");

        //匹配日志代码的id,
        //Input: FSPDebug.LogTrack(0,name2,age);
        //FullMatch: 0,
        private static readonly Regex s_regexLogTrackCodeId = new Regex(@"(?<=\(\s*)\s*(\d+)\s*");

        //不需要日志代码模式串
        private static readonly Regex s_regexIgnoreTrackCode = new Regex(@"\{\s*(FrameLogger.EvolutionManager.)(IgnoreTrack)\(([^\)]+\s*)?\)(\})?");

        //匹配手动插入的日志代码
        private static readonly Regex s_regexHandInsertTrackCode = new Regex(@"(?:FrameLogger.EvolutionManager.)(?:LogTrack)\(([^;]+\s*)?\)(?:\})?\s*;\s*\/\*(.)*\*\/");

        //匹配自动插入的日志代码
        //Input: FrameLogger.EvolutionManager.LogTrack(0, handler); #FuncName#
        //Group1: FuncName
        private static Regex s_regexAutoInsertTrackCode = new Regex(@"\s*(?:FrameLogger.EvolutionManager.)(?:LogTrack)\((?:[^\)]+\s*)?\)(\})?\s*;\s*\#(\w*)\#");

        ///传入func head 匹配函数的名称
        private static readonly Regex s_regexFuncName = new Regex(@"(\w+(<\w+>)*\s*)\(");

        ///匹配所有的函数参数类型以及名称
        private static readonly Regex s_regexAllFuncArgTypeAndName = new Regex(@"((?:\w|\d|_|<[^<]*>|(?:out\s*)|\[|\]|\,)+)\s*([\w|_|\d]+)\s*(?:=\s*[\w|\d]+)*(?:,|\))");

        private static readonly string s_baseDir = Application.dataPath;

        //存储hashId,以及函数信息
        private static FrameLoggerPdbFile s_trackPdb;

        private static readonly List<string> s_allUsedArgTypes = new List<string>();

        [MenuItem("战斗系统工具/FrameLogger/生成日志代码与符号表")]
        private static void InsertLogTrackCode()
        {
            s_trackPdb = new FrameLoggerPdbFile();
            s_trackPdb.ReadOldLogPdb(LogUtilConfig.s_logPdbFilePath);

            var allSearchFile = SearchFileUtil.GetAllSearchFile();

            //设置需要插入代码的文件夹路径
            foreach (var f in allSearchFile)
            {
                GenerateLogPdb(s_baseDir + f);
            }

            //额外处理的手动插入的日志
            foreach (var f in LogUtilConfig.s_specSearchFilePaths)
            {
                ResolveHandInsertCode(Application.dataPath + f);
            }

            s_trackPdb.CreateLogPdb(LogUtilConfig.s_logPdbFilePath);
        }

        /// <summary>
        ///     解析某个文件的自动日志和手动日志的信息，加入db中
        /// </summary>
        /// <param name="fullPath"></param>
        private static void GenerateLogPdb(string fullPath)
        {
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"文件不存在:{fullPath}");

                return;
            }

            //第一次处理，处理手动插入的日志
            ResolveHandInsertCode(fullPath);

            //第二次处理，自动插入日志
            InsertLogTrackAuto(fullPath);
        }

        /// <summary>
        ///     插入自动生成的日志代码，并将信息加入到db数据中
        /// </summary>
        private static void InsertLogTrackAuto(string fullPath)
        {
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"文件不存在:{fullPath}");

                return;
            }

            //解析文件名
            var fileName = Path.GetFileName(fullPath);

            //文件名过滤
            if (LogUtilConfig.CheckIgnoreFileName(fullPath))
            {
                return;
            }

            var text = File.ReadAllText(fullPath, s_utf8Encoding);
            var matches = s_regexFuncAll.Matches(text);
            var cnt = matches.Count;

            for (var i = cnt - 1; i >= 0; i--)
            {
                //某个具体函数
                var matchFuncAll = matches[i];

                //匹配到函数头
                var matchFuncHead = s_regexFuncHead.Match(text, matchFuncAll.Index, matchFuncAll.Length);

                //根据函数头 得到函数信息
                var funcHeadInfo = GetFunHeadInfo(matchFuncHead.Value);

                if (LogUtilConfig.CheckIgnoreFuncName(funcHeadInfo.funcName))
                {
                    continue;
                }

                if (!LogUtilConfig.CheckChooseFuncName(funcHeadInfo.funcName))
                {
                    continue;
                }

                funcHeadInfo.fileName = fileName;

                //匹配到函数的左大括号
                var matchLeftBrace = s_regexLeftBrace.Match(text, matchFuncAll.Index, matchFuncAll.Length);

                //匹配成功
                if (matchLeftBrace.Success)
                {
                    var len = matchFuncAll.Index + matchFuncAll.Length - (matchLeftBrace.Index + matchLeftBrace.Length);

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

                        //首行代码为日志代码
                        if (logTrackCodeMatch.Success)
                        {
                            text = text.Remove(matchFirstCode.Index + logTrackCodeMatch.Index, logTrackCodeMatch.Length);
                            var args = logTrackCodeMatch.Groups[3].Value;

                            //取出第一个参数hashId
                            int id = int.Parse(args.Contains(",") ? args.Substring(0, args.IndexOf(',')).Trim() : args);

                            if (id > 0 && s_trackPdb.CheckIdExist((ushort)id))
                            {
                                s_trackPdb.AddItem((ushort)id, funcHeadInfo);
                                funcHeadInfo.logTrackCode = ReplaceHashId(funcHeadInfo.logTrackCode, (ushort)id);
                                text = text.Insert(matchLeftBrace.Index + matchLeftBrace.Length, funcHeadInfo.logTrackCode);
                            }
                            else
                            {
                                var newId = s_trackPdb.AddItem(funcHeadInfo);
                                funcHeadInfo.logTrackCode = ReplaceHashId(funcHeadInfo.logTrackCode, newId);
                                text = text.Insert(matchLeftBrace.Index + matchLeftBrace.Length, funcHeadInfo.logTrackCode);
                            }
                        }
                        else
                        {
                            var id = s_trackPdb.AddItem(funcHeadInfo);
                            funcHeadInfo.logTrackCode = ReplaceHashId(funcHeadInfo.logTrackCode, id);
                            text = text.Insert(matchLeftBrace.Index + matchLeftBrace.Length, funcHeadInfo.logTrackCode);
                        }
                    }
                }
            }

            File.WriteAllText(fullPath, text, s_utf8Encoding);
        }

        /// <summary>
        ///     处理手动插入的日志代码，并将信息加入到db数据中
        /// </summary>
        private static void ResolveHandInsertCode(string fullPath)
        {
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"文件不存在:{fullPath}");

                return;
            }

            //解析文件名
            var fileName = Path.GetFileName(fullPath);

            if (LogUtilConfig.CheckIgnoreFileName(fullPath))
            {
                return;
            }

            var fullText = File.ReadAllText(fullPath, s_utf8Encoding);

            var matchCollection = s_regexFuncAll.Matches(fullText);

            for (var i = 0; i < matchCollection.Count; i++)
            {
                var funcMatch = matchCollection[i];

                //匹配到函数头
                var matchFuncHead = s_regexFuncHead.Match(fullText, funcMatch.Index, funcMatch.Length);

                //函数头信息
                var funcHeadInfo = GetFunHeadInfo(matchFuncHead.Value);

                funcHeadInfo.fileName = fileName;

                //先匹配该函数内是否有手动插入的日志代码
                var handInsertMatches = s_regexHandInsertTrackCode.Matches(funcMatch.Value);

                for (var j = 0; j < handInsertMatches.Count; j++)
                {
                    var handInsertCodeMatch = handInsertMatches[j];
                    var args = handInsertCodeMatch.Groups[1].Value;
                    var comment = handInsertCodeMatch.Groups[2].Value;

                    //取出第一个参数hashId
                    var id = int.Parse(args.Contains(",") ? args.Substring(0, args.IndexOf(',')).Trim() : args);

                    if (args.Contains(","))
                    {
                        args = args.Remove(0, args.IndexOf(',') + 1).Trim();
                        funcHeadInfo.validArgNames = args;
                        funcHeadInfo.validArgCount = args.Split(',').Length;
                    }
                    else
                    {
                        funcHeadInfo.validArgNames = null;
                        funcHeadInfo.validArgCount = 0;
                    }

                    funcHeadInfo.comment = comment;
                    funcHeadInfo.type = 1;

                    if (id > 0 && s_trackPdb.CheckIdExist((ushort)id))
                    {
                        s_trackPdb.AddItem((ushort)id, funcHeadInfo);
                    }
                    else
                    {
                        var hashId = s_trackPdb.AddItem(funcHeadInfo);
                        fullText = ReplaceHashId(fullText, funcMatch.Index + handInsertMatches[j].Index, handInsertMatches[j].Length, hashId);
                    }
                }
            }

            File.WriteAllText(fullPath, fullText, s_utf8Encoding);
        }

        /// <summary>
        ///     根据函数头 自动生成日志代码
        ///     只提取int 和 Fix类型的参数
        /// </summary>
        /// <param name="funcHead"></param>
        private static LogCodeInfo GetFunHeadInfo(string funcHead)
        {
            var funcHeadInfo = new LogCodeInfo();

            //定义默认插入的日志代码
            var codeText = "FrameLogger.EvolutionManager.LogTrack(0);";

            //1. 提取函数名
            var funcNameMatch = s_regexFuncName.Match(funcHead);

            if (funcNameMatch.Success)
            {
                funcHeadInfo.funcName = funcNameMatch.Groups[1].Value;
            }

            //2. 提取所有的参数类型以及参数名称, 同时顺便生成日志代码
            var allArgMatch = s_regexAllFuncArgTypeAndName.Matches(funcHead);

            for (var i = 0; i < allArgMatch.Count; i++)
            {
                var match = allArgMatch[i];
                var type = match.Groups[1].Value;
                var name = match.Groups[2].Value;
                funcHeadInfo.argTypes += type;
                funcHeadInfo.argNames += name;

                if (i < allArgMatch.Count - 1)
                {
                    funcHeadInfo.argTypes += ",";
                    funcHeadInfo.argNames += ",";
                }

                if (!s_allUsedArgTypes.Contains(type))
                {
                    s_allUsedArgTypes.Add(type);
                }

                if (LogTrackArgFactory.IsArgTypeSupported(type))
                {
                    codeText = codeText.Insert(codeText.Length - 2, $",{name}");
                    funcHeadInfo.validArgNames += name + ",";
                    funcHeadInfo.validArgCount++;
                }
                else if (LogUtilConfig.s_customArgTypesConfigs.TryGetValue(type, out var argMsg))
                {
                    codeText = codeText.Insert(codeText.Length - 2, $",{argMsg.GetLogCodeStr(name)}");
                    funcHeadInfo.validArgNames += name + ",";
                    funcHeadInfo.validArgCount++;
                }
            }

            if (!string.IsNullOrEmpty(funcHeadInfo.validArgNames))
            {
                funcHeadInfo.validArgNames = funcHeadInfo.validArgNames.Remove(funcHeadInfo.validArgNames.Length - 1, 1);
            }

            funcHeadInfo.logTrackCode = codeText;

            return funcHeadInfo;
        }

        /// <summary>
        ///     替换掉日志代码中的id字段
        /// </summary>
        /// <param name="logTrackCode"></param>
        /// <param name="hashId"></param>
        /// <returns></returns>
        private static string ReplaceHashId(string logTrackCode, ushort hashId)
        {
            var idMatch = s_regexLogTrackCodeId.Match(logTrackCode);

            if (idMatch.Success)
            {
                logTrackCode = logTrackCode.Remove(idMatch.Index, idMatch.Length);
                logTrackCode = logTrackCode.Insert(idMatch.Index, hashId.ToString());
            }

            return logTrackCode;
        }

        /// <summary>
        ///     替换日志代码中的id
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="start">匹配起始位置</param>
        /// <param name="end">匹配结束位置</param>
        /// <param name="hashId"></param>
        /// <returns></returns>
        private static string ReplaceHashId(string text, int start, int end, ushort hashId)
        {
            var idMatch = s_regexLogTrackCodeId.Match(text, start, end);

            if (idMatch.Success)
            {
                text = text.Remove(idMatch.Index, idMatch.Length);
                text = text.Insert(idMatch.Index, hashId.ToString());
            }

            return text;
        }
    }
}