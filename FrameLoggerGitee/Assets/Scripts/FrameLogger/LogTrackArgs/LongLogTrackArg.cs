using System;

namespace FrameLogger
{
    [LogTrackArg(typeof(long), (byte)LogTrackArgId.Int64, new[]{"long", "Int64", "System.Int64"})]
    public class LongLogTrackArg : ALogTrackArg
    {
        private long m_value;
        
        public override byte LogTrackArgType => (byte)LogTrackArgId.Int64;
        
        public LongLogTrackArg()
        {
        }

        public LongLogTrackArg(long value)
        {
            m_value = value;
        }

        public static ILogTrackArg Create(long value)
        {
            ILogTrackArg arg = new LongLogTrackArg(value);
            return arg;
        }

        public override long Sum()
        {
            return m_value;
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

            m_value = BitConverter.ToInt64(bytes, 0);
        }
        
        public override string ToString()
        {
            return m_value.ToString();
        }
    }
}