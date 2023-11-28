using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.BZip2;
using UnityEditor;
using UnityEngine;

namespace FrameLogger.Editor
{
    public static partial class LogUtil
    {
        private static readonly Dictionary<ushort, LogTrackJsonData> s_logTrackCodeInfos = new Dictionary<ushort, LogTrackJsonData>();

        private static int s_totalCount;
        private static int s_finishCount;

        [MenuItem("战斗系统工具/FrameLogger/反序列化 %w")]
        public static void SelectBinaryLogDataFile()
        {
            var evoFolderPath = Application.dataPath + "/../evo";
            s_finishCount = 0;

            if (Directory.Exists(evoFolderPath))
            {
                var files = Directory.GetFiles(evoFolderPath, "*.bin");
                
                //先加载db数据到内存
                LoadPdb(LogUtilConfig.s_logPdbFilePath);

                s_totalCount = files.Length;
                
                try
                {
                    for (var i = 0; i < files.Length; i++)
                    {
                        var file = files[i];

                        if (file.EndsWith(".bin"))
                        {
                            var filePath = Path.Combine(evoFolderPath, file);
                            var filename = Path.GetFileNameWithoutExtension(file);
                            var inverseFileOutPutFolderPath = Path.Combine(evoFolderPath, filename + ".txt");
                            Debug.Log("Begin:" + filename);

                            //反序列化解析文件到指定文件中
                            ReLogDataSerialize(filePath, inverseFileOutPutFolderPath, false);
                            Debug.Log("Finish:" + filename);
                            s_finishCount++;
                            ShowProgress();
                        }
                    }
                }
                catch (Exception e)
                {
                    EditorUtility.ClearProgressBar();
                    Console.WriteLine(e);

                    throw;
                }

                EditorUtility.ClearProgressBar();
                
                OpenFolder(evoFolderPath);
            }
        }

        private static void ShowProgress()
        {
            EditorUtility.DisplayProgressBar("反序列化", $"{s_finishCount}/{s_totalCount}", s_finishCount / (float)s_totalCount);
        }

        /// <summary>
        /// 加载json个数的db信息
        /// </summary>
        /// <param name="file"></param>
        private static void LoadPdb(string file)
        {
            s_logTrackCodeInfos.Clear();
            var jsonData = LitJson.JsonMapper.ToObject(File.ReadAllText(file));

            for (var i = 0; i < jsonData.Count; i++)
            {
                var data = jsonData[i];
                var logTrackCodeInfo = new LogTrackJsonData();

                logTrackCodeInfo.ID = ushort.Parse(data["ID"].ToString());
                logTrackCodeInfo.FileName = data["FileName"] != null ? data["FileName"].ToString() : "";
                logTrackCodeInfo.FuncName = data["FuncName"] != null ? data["FuncName"].ToString() : "";
                logTrackCodeInfo.ArgTypes = data["ArgTypes"] != null ? data["ArgTypes"].ToString() : "";
                logTrackCodeInfo.ArgNames = data["ArgNames"] != null ? data["ArgNames"].ToString() : "";
                logTrackCodeInfo.ValidArgNames = data["ValidArgNames"] != null ? data["ValidArgNames"].ToString() : "";
                logTrackCodeInfo.ValidArgCount = int.Parse(data["ValidArgCount"].ToString());
                logTrackCodeInfo.Comments = data["Comments"] != null ? data["Comments"].ToString() : "";
                s_logTrackCodeInfos.Add(logTrackCodeInfo.ID, logTrackCodeInfo);
            }
        }

        /// <summary>
        /// 反序列化，并根据pdb，写入目标文件
        /// </summary>
        /// <param name="path">序列化文件</param>
        /// <param name="outputLogDataPath"></param>
        /// <param name="compress"></param>
        private static void ReLogDataSerialize(string path, string outputLogDataPath, bool compress)
        {
            if (!File.Exists(path))
            {
                return;
            }

            //proto 反序列化
            var ltfList = new List<FrameLogEntity>();

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var bzipHeader = new byte[2];
                fs.Read(bzipHeader, 0, 2);
                fs.Position = 0;

                BinaryReader binaryReader = null;

                //解压处理
                if (bzipHeader[0] == 'B' && bzipHeader[1] == 'Z')
                {
                    var decompressStream = new MemoryStream();

                    BZip2.Decompress(fs, decompressStream, false);
                    binaryReader = new BinaryReader(decompressStream);

                    decompressStream.Position = 0;
                }
                //不解压处理
                else
                {
                    binaryReader = new BinaryReader(fs);
                }

                while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
                {
                    var logTrackFrame = new FrameLogEntity();

                    if (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
                    {
                        logTrackFrame.frameIndex = binaryReader.ReadInt32();
                    }

                    if (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
                    {
                        logTrackFrame.hash = binaryReader.ReadUInt64();
                    }

                    if (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
                    {
                        var idCount = binaryReader.ReadInt32();

                        for (var i = 0; i < idCount; i++)
                        {
                            logTrackFrame.ids.Add(binaryReader.ReadUInt16());
                        }
                    }

                    if (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
                    {
                        var argCount = binaryReader.ReadInt32();

                        for (var i = 0; i < argCount; i++)
                        {
                            byte argType = binaryReader.ReadByte();
                            int len = binaryReader.ReadInt32();
                            byte[] bytes = binaryReader.ReadBytes(len);
                            
                            var arg = LogTrackArgFactory.CreateLogTrackArg(argType, bytes);
                            logTrackFrame.args.Add(arg);
                        }
                    }
                    
                    ltfList.Add(logTrackFrame);
                }
            }

            var stringBuilder = new StringBuilder();

            try
            {
                //解析每帧的数据
                for (var i = 0; i < ltfList.Count; i++)
                {
                    DeserializeLogDataFrame(ltfList[i], stringBuilder);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            finally
            {
                //写入文件
                if (stringBuilder.Length > 0)
                {
                    //写数据
                    using (var fs = new FileStream(outputLogDataPath, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        var by = Encoding.UTF8.GetBytes(stringBuilder.ToString());
                        fs.Write(by, 0, by.Length);
                    }
                }
            }
        }

        /// <summary>
        /// 解析每帧的数据
        /// </summary>
        private static void DeserializeLogDataFrame(FrameLogEntity frameLog, StringBuilder content)
        {
            content.AppendFormat("********FrameIndex:{0}********{1}", frameLog.frameIndex, "\r\n");

            //当前参数的index
            var argIndex = 0;
            
            var args = new List<ILogTrackArg>();
            content.AppendLine($"Hash:{frameLog.hash}");
            content.AppendLine($"ID Count:{frameLog.ids.Count}");
            content.AppendLine($"Args Count:{frameLog.args.Count}");

            for (var i = 0; i < frameLog.ids.Count; i++)
            {
                var id = frameLog.ids[i];
                
                //函数信息
                if (!s_logTrackCodeInfos.ContainsKey(id))
                {
                    Debug.LogError("未找到Id：" + id);
                }

                var logTrackCodeInfo = s_logTrackCodeInfos[id];

                //有效参数个数
                var validArgCount = logTrackCodeInfo.ValidArgCount;

                //从参数列表中获取参数
                var j = argIndex;
                args.Clear();

                for (; j < argIndex + validArgCount; j++)
                {
                    if (j >= frameLog.args.Count)
                    {
                        break;
                    }

                    args.Add(frameLog.args[j]);
                }

                argIndex = j;
                content.AppendLine(HandlerLogMsg(logTrackCodeInfo, args));
            }
        }

        /// <summary>
        /// 根据log数据生成一条日志文本
        /// </summary>
        /// <param name="logTrackJsonData"></param>
        /// <param name="argsList"></param>
        /// <returns></returns>
        private static string HandlerLogMsg(LogTrackJsonData logTrackJsonData, List<ILogTrackArg> argsList)
        {
            var debugLogStr = new StringBuilder();

            if (logTrackJsonData != null)
            {
                debugLogStr.AppendFormat("ID:{0} {1}---{2}($)  ", logTrackJsonData.ID, logTrackJsonData.FileName, logTrackJsonData.FuncName);

                if (logTrackJsonData.ArgNames.Equals(string.Empty))
                {
                    //替换参数值
                    debugLogStr = debugLogStr.Replace("$", "");
                }
                else
                {
                    var argAry = logTrackJsonData.ArgNames.Split(',');
                    var argCount = argAry.Length;

                    if (argCount == 1)
                    {
                        debugLogStr = debugLogStr.Replace("$", argAry[0]);
                    }
                    else
                    {
                        var sb = new StringBuilder();

                        for (var i = 0; i < argCount; i++)
                        {
                            sb.AppendFormat("{0},", argAry[i]);
                        }

                        //移除最后一逗号
                        sb = sb.Remove(sb.Length - 1, 1);
                        debugLogStr = debugLogStr.Replace("$", sb.ToString());
                    }
                }

                if (!string.IsNullOrEmpty(logTrackJsonData.ValidArgNames))
                {
                    var validArgNames = logTrackJsonData.ValidArgNames.Split(',');

                    for (var i = 0; i < validArgNames.Length; i++)
                    {
                        if (i >= argsList.Count)
                        {
                            continue;
                        }

                        debugLogStr.Append($"{validArgNames[i]} = {argsList[i]} ;");
                    }
                }

                debugLogStr.Append(logTrackJsonData.Comments);
            }

            return debugLogStr.ToString();
        }
    }
}
