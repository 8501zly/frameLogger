using System;

namespace FrameLogger
{
    public class CircleQueue<T>
    {
        public CircleQueue(int capacity)
        {
            //会额外使用一个位置标志队列满
            Length = capacity + 1;
            Data = new T[Length];
            Front = 0;
            Tail = 0;
        }

        //队列数组
        public T[] Data { get; }

        //数组长度
        public int Length { get; }

        //队列首部索引
        public int Front { get; private set; }

        //队列尾部索引
        public int Tail { get; private set; }

        public void Enqueue(T value)
        {
            //队列满
            if (IsFull()) Front = (Front + 1) % Length;

            Data[Tail] = value;
            Tail = (Tail + 1) % Length;
        }

        public T Dequeue()
        {
            if (IsEmpty()) return default;

            var result = Data[Front];
            Front = (Front + 1) % Length;

            return result;
        }

        public bool IsEmpty()
        {
            return Front == Tail;
        }

        public bool IsFull()
        {
            return (Tail + 1) % Length == Front;
        }

        public T GetNext()
        {
            var value = default(T);

            if (IsEmpty()) return default;

            if (IsFull()) Front = (Front + 1) % Length;

            value = Data[Tail];

            if (value != null) Tail = (Tail + 1) % Length;

            return value;
        }

        public int QueueLength()
        {
            return (Tail + Length - Front) % Length;
        }

        public void Clear()
        {
            Front = 0;
            Tail = 0;
            Array.Clear(Data, 0, Data.Length);
        }
    }
}