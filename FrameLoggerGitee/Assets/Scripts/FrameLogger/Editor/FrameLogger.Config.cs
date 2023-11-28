using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace FrameLogger.Editor
{
    public static class LogUtilConfig
    {
        //日志DB生成目录
        public static readonly string s_logPdbFilePath = $"{Application.dataPath}"
                                                         + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar
                                                         + "PdbLog_09_Dev.json";

        /// <summary>
        ///     选择需要自动插入日志代码的文件夹目录
        /// </summary>
        public static readonly string[] s_searchPaths =
        {
            @"\Scripts\FrameLoggerTest",
        };

        //额外手动生成日志的文件目录，如过该目录已经配忽略，又在这个目录的某个文件中手动插入了日志，则需要在这里添加这个文件
        public static readonly string[] s_specSearchFilePaths =
        {
            @"\Scripts\FrameDebuggerTest\TestInsertCode.cs"
        };

        /// <summary>
        /// 选择要生成的日志代码函数，开启 CHOOSE_FUNC 宏生效，否则走ignore模式
        /// </summary>
        public static readonly string[] s_chooseFuncNames =
        {
            "Release",
            "Reset",
            "Action"
        };

        //要屏蔽的文件夹路径(路径默认从Assets下开始)
        public static readonly string[] s_ignoreFolders =
        {
            @"\Scripts\FrameDebuggerTest",
        };

        //要屏蔽的文件名,以 Scripts文件夹开始，Scripts文件夹位于Assets文件夹下
        public static readonly string[] s_ignoreFiles =
        {
            @"\Scripts\FrameDebuggerTest\TestEnum.cs",
        };

        //要屏蔽的方法名称
        public static readonly string[] s_ignoreFuncNames =
        {
            "Update",
            "Deserialize",
            "Serialize",
            "GreaterThanZero",
            "GetValue",
            "ContainsValue",
            "Get",
            "Has",
            "Load",
            "GetObjectsList",
            "FixedTick",
            "AfterSingleTick",
            "BeginFixedTick",
            "AfterFixedTick",
            "Reset",
            "Init",
            "EndFixedTick",
            "Tick",
            "LateTick",
            "Execute",
            "DoExecute",
            "BeginExecute",
            "LoadData",
            "OnTick",
            "Clear",
            "Dispose",
            "Release",
            "OnFixTick",
            "OnFixedTick",
            "ExportData",
            "ToString",
            "ClearPerFrame",
        };

        /// <summary>
        /// 需要序列化存储的参数
        /// </summary>
        public static readonly Dictionary<string, LogArgTypeConfig> s_customArgTypesConfigs = new Dictionary<string, LogArgTypeConfig>
        {
            { nameof(FrameDebuggerTest.TestEnum), new LogArgTypeConfig(typeof(FrameDebuggerTest.TestEnum), $"(int){"#NAME#"}") },
            
            { nameof(FrameDebuggerTest.FP), new LogArgTypeConfig(typeof(FrameDebuggerTest.FP), $"{"#NAME#"}.rawValue") },
            { nameof(FrameDebuggerTest.FPVector2), new LogArgTypeConfig(typeof(FrameDebuggerTest.FPVector2), $"{"#NAME#"}.ToString()") },
            { nameof(FrameDebuggerTest.FPVector3), new LogArgTypeConfig(typeof(FrameDebuggerTest.FPVector3), $"{"#NAME#"}.ToString()") },
            
            { nameof(FrameDebuggerTest.SomeLogicObject), new LogArgTypeConfig(typeof(FrameDebuggerTest.SomeLogicObject), $"{"#NAME#"}?.id ?? 0") },
            { nameof(FrameDebuggerTest.SomeMapLogicObject), new LogArgTypeConfig(typeof(FrameDebuggerTest.SomeMapLogicObject), $"{"#NAME#"}?.mapObjectId ?? 0") }
        };

        public static bool CheckIgnoreFileName(string fullPath)
        {
            foreach (var t in s_ignoreFiles)
            {
                if (fullPath.EndsWith(t))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     函数名忽略
        /// </summary>
        /// <param name="funcName"></param>
        /// <returns></returns>
        public static bool CheckIgnoreFuncName(string funcName)
        {
            foreach (var t in s_ignoreFuncNames)
            {
                if (t.Equals(funcName))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CheckChooseFuncName(string funcName)
        {
#if CHOOSE_FUNC
            foreach (var t in s_chooseFuncNames)
            {
                if (t.Equals(funcName))
                {
                    return true;
                }
            }
#else
            if (!s_ignoreFuncNames.Contains(funcName))
            {
                return true;
            }
#endif

            return false;
        }
    }
}