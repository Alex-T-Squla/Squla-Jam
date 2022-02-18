namespace Squla.Core.TDD
{
	public sealed class MessageTypes
	{
		public const string TestSuite_Count = "TestSuite_Count";
		public const string TestSuite_Selected = "TestSuite_Selected";
		public const string TestSuite_AllDone = "TestSuite_AllDone";
		public const string TestCase_Add = "TestCase_Add";
		public const string TestCase_Selected = "TestCase_Selected";
		public const string TestCase_Assert = "TestCase_Assert";
		public const string TestCase_Ended = "TestCase_Ended";
		public const string TestCase_Exception = "TestCase_Exception";
	}
}