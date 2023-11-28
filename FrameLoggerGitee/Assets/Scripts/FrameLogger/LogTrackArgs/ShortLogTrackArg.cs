using System;

namespace FrameLogger
{
    [LogTrackArg(typeof(short), (byte)LogTrackArgId.Int16, new[]{"short", "Int16", "System.Int16"})]
    public class ShortLogTrackArg : ALogTrackArg
    {
        private short m_value;
        
        public override byte LogTrackArgType => (byte)LogTrackArgId.Int16;
        
        public ShortLogTrackArg()
        {
        }

        public ShortLogTrackArg(short value)
        {
            m_value = value;
        }

        public static ILogTrackArg Create(short value)
        {
            ILogTrackArg arg = new ShortLogTrackArg(value);
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

            m_value = BitConverter.ToInt16(bytes, 0);
        }
        
        public override string ToString()
        {
            return m_value.ToString();
        }
    }
}