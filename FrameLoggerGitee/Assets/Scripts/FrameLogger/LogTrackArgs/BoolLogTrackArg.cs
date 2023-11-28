using System;

namespace FrameLogger
{
    [LogTrackArg(typeof(bool), (byte)LogTrackArgId.Boolean, new[]{"bool", "Boolean", "System.Boolean"})]
    public class BoolLogTrackArg : ALogTrackArg
    {
        private bool m_value;

        public override byte LogTrackArgType => (byte)LogTrackArgId.Boolean;
        
        public BoolLogTrackArg()
        {
        }
        
        public BoolLogTrackArg(bool value)
        {
            m_value = value;
        }
        
        public static ILogTrackArg Create(bool value)
        {
            ILogTrackArg arg = new BoolLogTrackArg(value);
            return arg;
        }
        
        public override long Sum()
        {
            return m_value ? 1 : 0;
        }
        
        protected override byte[] OnSerialize()
        {
            var byteArray = BitConverter.GetBytes(m_value);
            
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(byteArray);
            }

            return byteArray;
        }

        protected override void OnDeserialize(byte[] bytes)
        {
            // if (BitConverter.IsLittleEndian)
            // {
            //     Array.Reverse(bytes);
            // }
            
            m_value = BitConverter.ToBoolean(bytes, 0);
        }

        public override string ToString()
        {
            return m_value ? "1" : "0";
        }
    }
}