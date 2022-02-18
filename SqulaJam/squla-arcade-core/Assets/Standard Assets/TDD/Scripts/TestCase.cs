using System;
using SimpleJson;
using Squla.Core.TDD;

namespace Squla.Core.TDD_Impl
{
    internal class TestCase
    {
        private readonly IMessageSender sender;
	    public readonly Action run;

        public TestCase(string name, Action run, int timeout, int delay, IMessageSender sender)
        {
            this.name = name;
	        this.run = run;
	        this.timeout = timeout;
	        this.delay = delay;
            this.sender = sender;
            sender.Send(new JsonObject {
                {"type", MessageTypes.TestCase_Add},
                {"name", name},
                {"timeout", timeout}
            });
        }

	    public string name { get; private set; }

	    public int timeout { get; private set; }

	    public int delay { get; private set; }

        public void Assert(bool success, string msg, params object[] args)
        {
            sender.Send(new JsonObject {
                {"type", MessageTypes.TestCase_Assert},
                {"success", success},
                {"msg", string.Format(msg, args)}
            });
        }

        public void Ended()
        {
            sender.Send(new JsonObject {
                {"type", MessageTypes.TestCase_Ended}
            });
        }

	    public void Run()
	    {
		    run();
	    }
    }
}
