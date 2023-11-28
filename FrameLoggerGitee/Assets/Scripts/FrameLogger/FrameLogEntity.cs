using System.Collections.Generic;

namespace FrameLogger
{
    public interface ILogTrackArg
    {
        byte LogTrackArgType { get; }
        byte[] Serialize();
        void Deserialize(byte[] bytes);
        long Sum();
    }

    public abstract class ALogTrackArg : ILogTrackArg
    {
        byte[] ILogTrackArg.Serialize()
        {
            return OnSerialize();
        }

        void ILogTrackArg.Deserialize(byte[] bytes)
        {
            OnDeserialize(bytes);
        }

        public abstract long Sum();

        public abstract byte LogTrackArgType { get; }

        protected abstract byte[] OnSerialize();
        protected abstract void OnDeserialize(byte[] bytes);
    }

    /// <summary>
    ///     用于记录每一帧的log
    /// </summary>
    public class FrameLogEntity
    {
        //本帧所有的函数参数
        public readonly List<ILogTrackArg> args = new List<ILogTrackArg>(256);

        //本帧所有的函数id
        public readonly List<ushort> ids = new List<ushort>(128);

        //帧号
        public int frameIndex;

        //该帧的hash值
        public ulong hash;
    }
}