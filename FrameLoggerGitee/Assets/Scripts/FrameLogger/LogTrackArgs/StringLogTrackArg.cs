using System.Text;

namespace FrameLogger
{
    [LogTrackArg(typeof(string), (byte)LogTrackArgId.String, new[]{"string", "String", "System.String"})]
    public class StringLogTrackArg : ALogTrackArg
    {
        private string m_value;

        public override byte LogTrackArgType => (byte)LogTrackArgId.String;
        
        public StringLogTrackArg()
        {
        }
        
        public StringLogTrackArg(string value)
        {
            m_value = value;
        }

        public static ILogTrackArg Create(string value)
        {
            ILogTrackArg arg = new StringLogTrackArg(value);
            return arg;
        }

        public override long Sum()
        {
            return CRC32.CalcCRC(m_value);
        }

        protected override byte[] OnSerialize()
        {
            return Encoding.UTF8.GetBytes(m_value);
        }

        protected override void OnDeserialize(byte[] bytes)
        {
            m_value = Encoding.UTF8.GetString(bytes);
        }
        
        public override string ToString()
        {
            return m_value.ToString();
        }
    }
}