using System;

namespace FrameLogger.Editor
{
    public class LogArgTypeConfig
    {
        public Type argType;
        public string insertCodeStr;

        public LogArgTypeConfig(Type type, string insertCodeStr)
        {
            argType = type;
            this.insertCodeStr = insertCodeStr;
        }

        public string GetLogCodeStr(string name)
        {
            return insertCodeStr.Replace("#NAME#", name);
        }
    }
}