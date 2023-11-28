using System;

namespace FrameLogger
{
    [LogTrackArg(typeof(int), (byte)LogTrackArgId.Int32, new[] { "int", "Int32", "System.Int32" })]
    public class IntLogTrackArg : ALogTrackArg
    {
        private int m_value;

        public override byte LogTrackArgType => (byte)LogTrackArgId.Int32;

        public IntLogTrackArg()
        {
        }

        public IntLogTrackArg(int value)
        {
            m_value = value;
        }

        public static ILogTrackArg Create(int value)
        {
            ILogTrackArg arg = new IntLogTrackArg(value);
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

            m_value = BitConverter.ToInt32(bytes, 0);
        }

        public override string ToString()
        {
            return m_value.ToString();
        }
    }
}