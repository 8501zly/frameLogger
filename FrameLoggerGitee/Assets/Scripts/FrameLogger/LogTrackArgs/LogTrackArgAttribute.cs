using System;
using System.Collections.Generic;

namespace FrameLogger
{
    public class LogTrackArgAttribute : Attribute
    {
        public Type argType;
        public byte typeId;
        public HashSet<string> systemTypeNames;
        
        public LogTrackArgAttribute(Type argType, byte typeId, string[] systemTypeNames)
        {
            this.argType = argType;
            this.typeId = typeId;
            
            this.systemTypeNames = new HashSet<string>();
            foreach (var n in systemTypeNames)
            {
                this.systemTypeNames.Add(n);
            }
        }
    }
}