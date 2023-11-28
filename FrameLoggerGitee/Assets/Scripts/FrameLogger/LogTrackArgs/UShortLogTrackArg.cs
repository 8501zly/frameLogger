using System;

namespace FrameLogger
{
    [LogTrackArg(typeof(ushort), (byte)LogTrackArgId.UInt16, new[]{"ushort", "UInt16", "System.UInt16"})]
    public class UShortLogTrackArg : ALogTrackArg
    {
        private ushort m_value;
        
        public override byte LogTrackArgType => (byte)LogTrackArgId.UInt16;

        public UShortLogTrackArg()
        {
        }
        
        public UShortLogTrackArg(ushort value)
        {
            m_value = value;
        }

        public static ILogTrackArg Create(ushort value)
        {
            ILogTrackArg arg = new UShortLogTrackArg(value);
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

            m_value = BitConverter.ToUInt16(bytes, 0);
        }
        
        public override string ToString()
        {
            return m_value.ToString();
        }
    }
}