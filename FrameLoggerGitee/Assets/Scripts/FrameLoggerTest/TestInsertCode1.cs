namespace FrameDebuggerTest
{
    public class SomeInsertCode1
    {
        public int mapObjectId;

        public void Test1()
        {FrameLogger.EvolutionManager.LogTrack(4);
            int a = 0;
            FrameLogger.EvolutionManager.LogTrack(1, a); /*我是谁，我在哪*/
        }
        
        public void Test12(TestEnum testEnum)
        {FrameLogger.EvolutionManager.LogTrack(3,(int)testEnum);
            int a = 0;
            FrameLogger.EvolutionManager.LogTrack(2, a); /*我是谁，我在哪*/
        }
    }
}