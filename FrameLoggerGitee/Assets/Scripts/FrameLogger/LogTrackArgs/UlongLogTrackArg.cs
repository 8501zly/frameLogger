using System;

namespace FrameLogger
{
    [LogTrackArg(typeof(ulong), (byte)LogTrackArgId.UInt64, new[]{"ulong", "UInt64", "System.UInt64"})]
    public class ULongLogTrackArg : ALogTrackArg
    {
        private ulong m_value;
        
        public override byte LogTrackArgType => (byte)LogTrackArgId.UInt64;

        public ULongLogTrackArg()
        {
        }
        
        public ULongLogTrackArg(ulong value)
        {
            m_value = value;
        }

        public static ILogTrackArg Create(ulong value)
        {
            ILogTrackArg arg = new ULongLogTrackArg(value);
            return arg;
        }

        public override long Sum()
        {
            return (long)m_value;
        }

        protected override byte[] OnSerialize()
        {
            var byteArray = BitConverter.GetBytes(m_value);

            if (BitConverter.IsLittleEndian) Array.Reverse(byteArray);

            return byteArray;
        }

        protected override void OnDeserialize(byte[] bytes)
        {
            // if (BitConverter.IsLittleEndian)
            // {
            //     Array.Reverse(bytes);
            // }

            m_value = BitConverter.ToUInt64(bytes, 0);
        }
        
        public override string ToString()
        {
            return m_value.ToString();
        }
    }
}