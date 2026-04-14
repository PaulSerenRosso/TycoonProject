using NUnit.Framework;
using Logger;
using Tests.Editor.EditorTestUtility;

namespace Tests
{
    /// <summary>
    /// Base class for all tests that need a test logger
    /// </summary>
    public abstract class EditorBaseTest
    {
        public TestLogger TestLogger;

        [SetUp]
        public virtual void BaseSetUp()
        {
            TestLogger = new TestLogger();
            Log.Default = TestLogger; 
        }
        

   
    }
}
