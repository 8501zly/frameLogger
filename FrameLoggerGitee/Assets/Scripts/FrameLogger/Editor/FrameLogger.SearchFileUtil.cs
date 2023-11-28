using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FrameLogger.Editor
{
    /// <summary>
    /// 提供文件搜索服务
    /// </summary>
    public static class SearchFileUtil
    {
        /// <summary>
        /// 返回所有配置的需要自动插入代码的文件路径
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAllSearchFile()
        {
            var result = new List<string>();
            foreach (var t in LogUtilConfig.s_searchPaths)
            {
                result.AddRange(SearchAllFilesWithSpecifiedSuffix(Application.dataPath,  t, ".cs"));
            }

            return result;
        }

        /// <summary>
        /// 在指定路径下搜索所有指定后缀的文件(子文件夹下也会被搜索)
        /// 会剔除被屏蔽的文件夹路径
        /// </summary>
        /// <param name="baseDir">参数1:父路径</param>
        /// <param name="searchPath">参数2:搜索目录</param>
        /// <param name="allSuffix">参数3:要搜索的后缀(以逗号隔开,如.txt,.cs)</param>
        /// <returns>返回值:所有找到的子路径</returns>
        public static List<string> SearchAllFilesWithSpecifiedSuffix(string baseDir,string searchPath, string allSuffix)
        {
            List<string> subPaths = new List<string>();
            //剔除所有屏蔽文件夹
            bool ignoreSign = false;
            foreach (var ignoreFolder in LogUtilConfig.s_ignoreFolders)
            {
                if (ignoreFolder == searchPath)
                {
                    ignoreSign = true;
                    break;
                }
            }
            if (ignoreSign == false)
            {
                DirectoryInfo info = new DirectoryInfo(baseDir + searchPath);
                //文件
                FileInfo[] files = info.GetFiles();
                //目录
                DirectoryInfo[] dirs = info.GetDirectories();
                //处理子文件
                foreach (FileInfo file in files)
                {
                    if (File.Exists(file.FullName))
                    {
                        int suffixIndex = file.FullName.LastIndexOf('.');
                        //提取后缀(带.)
                        string suffix = file.FullName.Substring(suffixIndex);
                        bool matchSuccess = false;
                        //匹配后缀
                        foreach (string targetSuffix in allSuffix.Split(','))
                        {
                            if (suffix == targetSuffix)
                            {
                                matchSuccess = true;
                                break;
                            }
                        }
                        //匹配成功，记录下子路径
                        if (matchSuccess)
                        {
                            string subPath = file.FullName.Substring(baseDir.Length, file.FullName.Length - baseDir.Length);
                            subPaths.Add(subPath);
                        }
                    }
                }
                //处理子目录
                foreach (DirectoryInfo dir in dirs)
                {
                    //直接输入子文件路径会进入这里导致死循环，需要做一层保护
                    if (Directory.Exists(dir.FullName))
                    {
                        //递归处理
                        string deeperSubPathFolder = dir.FullName.Substring(baseDir.Length, dir.FullName.Length - baseDir.Length);
                        subPaths.AddRange(SearchAllFilesWithSpecifiedSuffix(baseDir, deeperSubPathFolder, allSuffix));
                    }
                }
            }

            return subPaths;
        }
    }
}
