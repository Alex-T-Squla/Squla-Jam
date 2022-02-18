namespace Squla.Core.TDD
{
    public interface ITestSuite
    {
	    string name { get; }

	    void Run();
    }
}
