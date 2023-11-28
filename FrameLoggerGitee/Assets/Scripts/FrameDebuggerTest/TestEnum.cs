namespace FrameDebuggerTest
{
    public enum TestEnum
    {
        A, B
    }

    public struct FP
    {
        public long rawValue;
        public override string ToString()
        {
            return rawValue.ToString();
        }
    }
    
    public struct FPVector2
    {
        public FP x;
        public FP y;
        public override string ToString()
        {
            return $"({x},{y})";
        }
    }    
    
    public struct FPVector3
    {
        public FP x;
        public FP y;
        public FP z;
        public override string ToString()
        {
            return $"({x},{y},{z})";
        }
    }

    public class SomeLogicObject
    {
        public int id;
    }
    
    public class SomeMapLogicObject
    {
        public int mapObjectId;
    }
}