using System;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;
using UnityEngine;

namespace FrameLogger
{
    public class EvolutionManager
    {
#pragma warning disable 414

        /// <summary>
        /// 返回当前帧的Id+arg总值
        /// </summary>
        /// <returns></returns>
        public static ulong GetCurrentFrameHash()
        {
            return (ulong)Math.Abs(s_sum);
        }

        /// <summary>
        /// 存全日志
        /// </summary>
        public static void SetAllEvolution(bool allEvolution, bool overlap = true)
        {
            if (s_logAllEvolution && !overlap)
            {
                return;
            }

            s_logAllEvolution = allEvolution;
        }

        public static bool GetAllEvolutionStatus()
        {
            return s_logAllEvolution;
        }

        /// <summary>
        /// lua检查是否记录日志
        /// </summary>
        /// <returns></returns>
        public static bool CheckEvolution()
        {
            return true;
        }
        
        private static int s_frameCount;
        private static bool s_logAllEvolution;

        private const int CACHE_FRAME_COUNT = 300;
        private const int CACHE_FRAME_MAX = 28000;

        //日志ID序列
        private static List<ushort> s_currLogTrackIds;

        //日志参数序列
        private static List<ILogTrackArg> s_currLogTrackArgs;

        private static FrameLogEntity s_currFrameLogEntity;

        private static CircleQueue<FrameLogEntity> s_allFrameLog;

        private static bool s_start;

        //用于校验不同步
        private static long s_sum = 1;
        
        public static void Init(bool logAll)
        {
            s_logAllEvolution = logAll;
            
            if (s_logAllEvolution)
            {
                s_allFrameLog = new CircleQueue<FrameLogEntity>(CACHE_FRAME_MAX);
            }
            else
            {
                s_allFrameLog = new CircleQueue<FrameLogEntity>(CACHE_FRAME_COUNT);
            }

            s_frameCount = 0;
            s_start = true;
            s_sum = 1;
            s_currLogTrackIds = null;
            s_currLogTrackArgs = null;
            s_currFrameLogEntity = null;
            NextFrameLogTrack();
        }

        public static void Clear()
        {
            s_start = false;
            s_frameCount = 0;
            s_allFrameLog?.Clear();
        }

        private static bool Check()
        {
            if (!s_start)
            {
                return false;
            }
            
            // if (s_logAllEvolution)
            // {
            //     return CGame.Instance.battle.executedTicks <= CACHE_FRAME_MAX;
            // }

            return true;
        }

        public static void NextFrame()
        {
            if (!Check())
            {
                return;
            }

            s_frameCount++;
            NextFrameLogTrack();
        }

        private static void NextFrameLogTrack()
        {
            if (s_currFrameLogEntity != null)
            {
                s_currFrameLogEntity.hash = GetCurrentFrameHash();
            }

            var logTrackFrame = s_allFrameLog.GetNext();

            if (logTrackFrame == null)
            {
                logTrackFrame = new FrameLogEntity();
                s_allFrameLog.Enqueue(logTrackFrame);
            }

            logTrackFrame.frameIndex = s_frameCount;
            s_currLogTrackIds = logTrackFrame.ids;
            s_currLogTrackIds.Clear();
            s_currLogTrackArgs = logTrackFrame.args;
            s_currLogTrackArgs.Clear();

            s_currFrameLogEntity = logTrackFrame;
        }

        public static void LogAllEvolution(string name)
        {
            if (s_logAllEvolution)
            {
                SaveEvolution(name);
            }
        }
        
        //保存不同步文件到本地
        private static void SaveEvolution(string uuid, FileMode mode = FileMode.Create)
        {
            string path;
#if UNITY_EDITOR
            if (Application.isMobilePlatform)//手機端暫未接入bi
            {
                return;
            }
            
            path = $"{Application.dataPath}/../evo";
#else //bi
            return;
#endif
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.GetFullPath(path);
            var name = $"{path}/{uuid}.bin";
            
            Debug.Log($"Frame Evolution To Log {name}");
            
            using(var fs = new FileStream(name, mode, FileAccess.Write))
            {
                using(var binaryWriter = new BinaryWriter(fs))
                {
                    int cur = s_allFrameLog.Front;

                    while (cur != s_allFrameLog.Tail)
                    {
                        var logTrackFrame = s_allFrameLog.Data[cur];
                        cur = (cur + 1) % s_allFrameLog.Length;
                        binaryWriter.Write(logTrackFrame.frameIndex);
                        binaryWriter.Write(logTrackFrame.hash);
                        binaryWriter.Write(logTrackFrame.ids.Count);

                        foreach (ushort id in logTrackFrame.ids)
                        {
                            binaryWriter.Write(id);
                        }

                        binaryWriter.Write(logTrackFrame.args.Count);

                        foreach (var arg in logTrackFrame.args)
                        {
                            binaryWriter.Write(arg.LogTrackArgType);
                            
                            var bytes = arg.Serialize();
                            binaryWriter.Write(bytes.Length);
                            binaryWriter.Write(bytes);
                        }
                    }
                }
            }
        }
        
        //上传不同步文件到ftp
        private static void SaveEvolutionAndCompress(string uuid)
        {
            using (var compressStream = new MemoryStream())
            {
                using (var ms = new MemoryStream())
                {
                    using (var binaryWriter = new BinaryWriter(ms))
                    {
                        int cur = s_allFrameLog.Front;

                        while (cur != s_allFrameLog.Tail)
                        {
                            var logTrackFrame = s_allFrameLog.Data[cur];
                            cur = (cur + 1) % s_allFrameLog.Length;
                            binaryWriter.Write(logTrackFrame.frameIndex);
                            binaryWriter.Write(logTrackFrame.hash);
                            binaryWriter.Write(logTrackFrame.ids.Count);

                            foreach (ushort id in logTrackFrame.ids)
                            {
                                binaryWriter.Write(id);
                            }

                            binaryWriter.Write(logTrackFrame.args.Count);

                            foreach (var arg in logTrackFrame.args)
                            {
                                byte[] bytes = arg.Serialize();
                                binaryWriter.Write(bytes.Length);
                                binaryWriter.Write(bytes);
                            }
                        }
                        
                        ms.Position = 0;
                        BZip2.Compress(ms, compressStream, false, 9);
                    }
                }
                
                string path = $"{Application.dataPath}/../evo";
                
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                
                path = Path.GetFullPath(path);
                var name = $"{path}/{uuid}.bin";
            
                Debug.Log($"Frame Evolution To Log {name}");
            
                using(var fs = new FileStream(name, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(compressStream.ToArray(), 0, (int)compressStream.Length);
                }
            }
        }
        
#region 记录函数日志

        private static void CreateLogTrackArg<T>(T arg)
        {
            var args = LogTrackArgFactory.CreateLogTrackArg(arg);
            s_currLogTrackArgs.Add(args);
            s_sum += args.Sum();
        }
        
        public static void LogTrack<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(ushort hash, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15)
        {
            if (!Check())
            {
                return;
            }
            
            s_currLogTrackIds.Add(hash);
            s_sum += hash;
            
            CreateLogTrackArg(arg1);
            CreateLogTrackArg(arg2);
            CreateLogTrackArg(arg3);
            CreateLogTrackArg(arg4);
            CreateLogTrackArg(arg5);
            CreateLogTrackArg(arg6);
            CreateLogTrackArg(arg7);
            CreateLogTrackArg(arg8);
            CreateLogTrackArg(arg9);
            CreateLogTrackArg(arg10);
            CreateLogTrackArg(arg11);
            CreateLogTrackArg(arg12);
            CreateLogTrackArg(arg13);
            CreateLogTrackArg(arg14);
            CreateLogTrackArg(arg15);
        }
        
        public static void LogTrack<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(ushort hash, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
        {
            if (!Check())
            {
                return;
            }
            
            s_currLogTrackIds.Add(hash);
            s_sum += hash;
            
            CreateLogTrackArg(arg1);
            CreateLogTrackArg(arg2);
            CreateLogTrackArg(arg3);
            CreateLogTrackArg(arg4);
            CreateLogTrackArg(arg5);
            CreateLogTrackArg(arg6);
            CreateLogTrackArg(arg7);
            CreateLogTrackArg(arg8);
            CreateLogTrackArg(arg9);
            CreateLogTrackArg(arg10);
            CreateLogTrackArg(arg11);
            CreateLogTrackArg(arg12);
            CreateLogTrackArg(arg13);
            CreateLogTrackArg(arg14);
        }
        
        public static void LogTrack<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(ushort hash, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
        {
            if (!Check())
            {
                return;
            }
            
            s_currLogTrackIds.Add(hash);
            s_sum += hash;
            
            CreateLogTrackArg(arg1);
            CreateLogTrackArg(arg2);
            CreateLogTrackArg(arg3);
            CreateLogTrackArg(arg4);
            CreateLogTrackArg(arg5);
            CreateLogTrackArg(arg6);
            CreateLogTrackArg(arg7);
            CreateLogTrackArg(arg8);
            CreateLogTrackArg(arg9);
            CreateLogTrackArg(arg10);
            CreateLogTrackArg(arg11);
            CreateLogTrackArg(arg12);
            CreateLogTrackArg(arg13);
        }
        
        public static void LogTrack<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(ushort hash, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
        {
            if (!Check())
            {
                return;
            }
            
            s_currLogTrackIds.Add(hash);
            s_sum += hash;
            
            CreateLogTrackArg(arg1);
            CreateLogTrackArg(arg2);
            CreateLogTrackArg(arg3);
            CreateLogTrackArg(arg4);
            CreateLogTrackArg(arg5);
            CreateLogTrackArg(arg6);
            CreateLogTrackArg(arg7);
            CreateLogTrackArg(arg8);
            CreateLogTrackArg(arg9);
            CreateLogTrackArg(arg10);
            CreateLogTrackArg(arg11);
            CreateLogTrackArg(arg12);
        }
        
        public static void LogTrack<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(ushort hash, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
        {
            if (!Check())
            {
                return;
            }
            
            s_currLogTrackIds.Add(hash);
            s_sum += hash;
            
            CreateLogTrackArg(arg1);
            CreateLogTrackArg(arg2);
            CreateLogTrackArg(arg3);
            CreateLogTrackArg(arg4);
            CreateLogTrackArg(arg5);
            CreateLogTrackArg(arg6);
            CreateLogTrackArg(arg7);
            CreateLogTrackArg(arg8);
            CreateLogTrackArg(arg9);
            CreateLogTrackArg(arg10);
            CreateLogTrackArg(arg11);
        }
        
        public static void LogTrack<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(ushort hash, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
        {
            if (!Check())
            {
                return;
            }
            
            s_currLogTrackIds.Add(hash);
            s_sum += hash;
            
            CreateLogTrackArg(arg1);
            CreateLogTrackArg(arg2);
            CreateLogTrackArg(arg3);
            CreateLogTrackArg(arg4);
            CreateLogTrackArg(arg5);
            CreateLogTrackArg(arg6);
            CreateLogTrackArg(arg7);
            CreateLogTrackArg(arg8);
            CreateLogTrackArg(arg9);
            CreateLogTrackArg(arg10);
        }
        
        public static void LogTrack<T1, T2, T3, T4, T5, T6, T7, T8, T9>(ushort hash, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
        {
            if (!Check())
            {
                return;
            }
            
            s_currLogTrackIds.Add(hash);
            s_sum += hash;
            
            CreateLogTrackArg(arg1);
            CreateLogTrackArg(arg2);
            CreateLogTrackArg(arg3);
            CreateLogTrackArg(arg4);
            CreateLogTrackArg(arg5);
            CreateLogTrackArg(arg6);
            CreateLogTrackArg(arg7);
            CreateLogTrackArg(arg8);
            CreateLogTrackArg(arg9);
        }
        
        public static void LogTrack<T1, T2, T3, T4, T5, T6, T7, T8>(ushort hash, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            if (!Check())
            {
                return;
            }
            
            s_currLogTrackIds.Add(hash);
            s_sum += hash;
            
            CreateLogTrackArg(arg1);
            CreateLogTrackArg(arg2);
            CreateLogTrackArg(arg3);
            CreateLogTrackArg(arg4);
            CreateLogTrackArg(arg5);
            CreateLogTrackArg(arg6);
            CreateLogTrackArg(arg7);
            CreateLogTrackArg(arg8);
        }
        
        public static void LogTrack<T1, T2, T3, T4, T5, T6, T7>(ushort hash, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            if (!Check())
            {
                return;
            }
            
            s_currLogTrackIds.Add(hash);
            s_sum += hash;
            
            CreateLogTrackArg(arg1);
            CreateLogTrackArg(arg2);
            CreateLogTrackArg(arg3);
            CreateLogTrackArg(arg4);
            CreateLogTrackArg(arg5);
            CreateLogTrackArg(arg6);
            CreateLogTrackArg(arg7);
        }
        
        public static void LogTrack<T1, T2, T3, T4, T5, T6>(ushort hash, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            if (!Check())
            {
                return;
            }
            
            s_currLogTrackIds.Add(hash);
            s_sum += hash;
            
            CreateLogTrackArg(arg1);
            CreateLogTrackArg(arg2);
            CreateLogTrackArg(arg3);
            CreateLogTrackArg(arg4);
            CreateLogTrackArg(arg5);
            CreateLogTrackArg(arg6);
        }
        
        public static void LogTrack<T1, T2, T3, T4, T5>(ushort hash, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            if (!Check())
            {
                return;
            }
            
            s_currLogTrackIds.Add(hash);
            s_sum += hash;
            
            CreateLogTrackArg(arg1);
            CreateLogTrackArg(arg2);
            CreateLogTrackArg(arg3);
            CreateLogTrackArg(arg4);
            CreateLogTrackArg(arg5);
        }
        
        public static void LogTrack<T1, T2, T3, T4>(ushort hash, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (!Check())
            {
                return;
            }
            
            s_currLogTrackIds.Add(hash);
            s_sum += hash;
            
            CreateLogTrackArg(arg1);
            CreateLogTrackArg(arg2);
            CreateLogTrackArg(arg3);
            CreateLogTrackArg(arg4);
        }
        
        public static void LogTrack<T1, T2, T3>(ushort hash, T1 arg1, T2 arg2, T3 arg3)
        {
            if (!Check())
            {
                return;
            }
            
            s_currLogTrackIds.Add(hash);
            s_sum += hash;
            
            CreateLogTrackArg(arg1);
            CreateLogTrackArg(arg2);
            CreateLogTrackArg(arg3);
        }
        
        public static void LogTrack<T1, T2>(ushort hash, T1 arg1, T2 arg2)
        {
            if (!Check())
            {
                return;
            }
            
            s_currLogTrackIds.Add(hash);
            s_sum += hash;
            
            CreateLogTrackArg(arg1);
            CreateLogTrackArg(arg2);
        }
        
        public static void LogTrack<T1>(ushort hash, T1 arg1)
        {
            if (!Check())
            {
                return;
            }
            
            s_currLogTrackIds.Add(hash);
            s_sum += hash;
            
            CreateLogTrackArg(arg1);
        }
        
        public static void LogTrack(ushort hash)
        {
            if (!Check())
            {
                return;
            }
            
            s_currLogTrackIds.Add(hash);
            s_sum += hash;
        }

#endregion

#pragma warning restore 414
    }
}
