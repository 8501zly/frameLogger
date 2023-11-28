using System;
using System.IO;

namespace FrameLogger
{
    [LogTrackArg(typeof(uint), (byte)LogTrackArgId.UInt32, new[]{"uint", "UInt32", "System.UInt32"})]
    public class UintLogTrackArg : ALogTrackArg
    {
        private uint m_value;
        
        public override byte LogTrackArgType => (byte)LogTrackArgId.UInt32;

        public UintLogTrackArg()
        {
        }
        
        public UintLogTrackArg(uint value)
        {
            m_value = value;
        }
        
        public static ILogTrackArg Create(uint value)
        {
            ILogTrackArg arg = new UintLogTrackArg(value);
            return arg;
        }
        
        public override long Sum()
        {
            return m_value;
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
            
            m_value = BitConverter.ToUInt32(bytes, 0);
        }
        
        public override string ToString()
        {
            return m_value.ToString();
        }
    }
}