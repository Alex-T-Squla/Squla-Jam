using System;
using SimpleJson;

namespace Squla.Core.TDD
{
    public interface ITestManager
    {
	    void Clear();

	    void CreateTestCase (string name, Action run, int timeout=5, int delay=0);

	    void Assert(bool success, string msg, params object[] args);

	    void TestCaseEnded();

	    void Send(JsonObject msg);
    }
}
