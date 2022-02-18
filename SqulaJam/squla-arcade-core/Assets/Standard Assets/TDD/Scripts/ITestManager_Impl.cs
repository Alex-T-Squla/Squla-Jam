using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJson;
using Squla.Core.IOC;
using Squla.Core.Modelize;
using Squla.Core.TDD;
using TMPro;
using UnityEngine;
using WebSocketSharp;

namespace Squla.Core.TDD_Impl
{
	[SingletonModule]
	public class ITestManager_Impl : MonoBehaviourV2, ITestManager, IMessageSender, ITestSuiteRunner
	{
		[Inject]
		private IConfiguration config;

		[Inject]
		private IModelizer modelizer;

		[Inject("ManualRun")]
		private bool manualRun;

		[InjectComponent ("title")]
		private TextMeshProUGUI title;

		[SerializeField]
		private GameObject container;

		[SerializeField]
		private GameObject nextButton;

		[SerializeField]
		private GameObject previousButton;

		private WebSocket webSocket;

		private Queue<string> tempQueue;

		private List<TestCase> testCases;
		private int currentIndex;

		protected override void AfterAwake()
		{
			if (!string.IsNullOrEmpty(config.WS_TestServerUrl))
				Initialize();

			tempQueue = new Queue<string>();
			testCases = new List<TestCase>();

			container.SetActive(manualRun);

			graph.RegisterModule (this);

			Application.logMessageReceived += OnLogMessage;
		}

		[Provides]
		[Singleton]
		public ITestManager provideTestManager()
		{
			return this;
		}

		[Provides]
		[Singleton]
		public ITestSuiteRunner provideTestSuiteRunner()
		{
			return this;
		}

		private void OnDestroy()
		{
			if (webSocket != null)
				webSocket.Close();
		}

		public void Clear()
		{
			testCases.Clear();
			currentIndex = 0;
			UpdateStatus();
		}

		public void CreateTestCase (string name, Action run, int timeout=5, int delay=0)
		{
			var testCase = new TestCase(name, run, timeout, delay, this);
			testCases.Add(testCase);

			if (testCases.Count == 1) {
				ShowCurrent();
			} else {
				UpdateStatus();
			}
		}

		public void Assert(bool success, string msg, params object[] args)
		{
			Send(new JsonObject {
				{"type", MessageTypes.TestCase_Assert},
				{"success", success},
				{"msg", string.Format(msg, args)}
			});
		}

		public void TestCaseEnded()
		{
			Send(new JsonObject {
				{"type", MessageTypes.TestCase_Ended}
			});

			if (!manualRun) {
				StartCoroutine(HandleTestCaseEnded());
			}
		}

		public void Send(JsonObject msg)
		{
			var strMsg = SimpleJSON.SerializeObject(msg);
			logger.Debug("---{0}", strMsg);

			if (webSocket != null && webSocket.ReadyState == WebSocketState.Open) {
				webSocket.Send(strMsg);
			}
			else {
				tempQueue.Enqueue(strMsg);
			}
		}

		private void OnLogMessage (string condition, string stackTrace, LogType type)
		{
			if (!(type == LogType.Exception || type == LogType.Error))
				return;

			Send (new JsonObject {
				{"type", MessageTypes.TestCase_Exception},
				{"condition", condition},
				{"stack_trace", stackTrace},
				{"log_type", type.ToString()}
			});
		}

		private void Update()
		{
			if (nextButton.activeInHierarchy && Input.GetKeyUp(KeyCode.RightArrow)) {
				WhenNextClicked();
			}

			if (previousButton.activeInHierarchy && Input.GetKeyUp(KeyCode.LeftArrow)) {
				WhenPreviousClicked();
			}
		}

		public void WhenPreviousClicked ()
		{
			currentIndex--;
			ShowCurrent();
		}

		public void WhenNextClicked ()
		{
			currentIndex++;
			ShowCurrent();
		}

		private void Initialize()
		{
			var url = string.Format("{0}/v1/topic/test-manager", config.WS_TestServerUrl);
			webSocket = new WebSocket (url);

			webSocket.OnOpen += OnOpen;
			webSocket.OnClose += OnClose;
			webSocket.OnError += OnError;
			webSocket.OnMessage += OnMessage;

			webSocket.ConnectAsync ();
		}

		private void OnOpen (object sender, EventArgs e)
		{
			logger.Debug ("WebSocketClient | OnOpen");

			lock (tempQueue) {
				while (tempQueue.Count > 0) {
					var msg = tempQueue.Dequeue();
					webSocket.Send(msg);
				}
			}
		}

		private void OnClose (object sender, CloseEventArgs e)
		{
			logger.Debug ("WebSocketClient | OnClose | Code:{0}, Reason:{1}", e.Code, e.Reason);
		}

		private void OnError (object sender, ErrorEventArgs e)
		{
			logger.Debug("WebSocketClient | OnError = {0}", e.Message);
		}

		private void OnMessage (object sender, MessageEventArgs e)
		{
			logger.Debug("WebSocketClient | OnMessage = {0}", e.Data);
		}

		private readonly WaitForSeconds sleep = new WaitForSeconds(0.5f);
		private IEnumerator StartAutoRun(bool noDelay=false)
		{
			var delay = testCases[currentIndex].delay;
			if (noDelay)
				yield return null;
			else if (delay > 0)
				yield return new WaitForSeconds(delay);
			else
				yield return sleep;
			ShowCurrent();
		}

		private void ShowCurrent()
		{
			title.text = testCases[currentIndex].name;
			Send(new JsonObject {
				{"type", MessageTypes.TestCase_Selected},
				{"name", testCases[currentIndex].name}
			});

			UpdateStatus();

			testCases[currentIndex].Run();
		}

		private void UpdateStatus()
		{
			previousButton.SetActive (currentIndex > 0);
			nextButton.SetActive (currentIndex < testCases.Count - 1);
		}

		public event Action End;

		private IEnumerator HandleTestCaseEnded()
		{
			yield return null;

			testCases.RemoveAt(0);

			if (testCases.Count > 0) {
				StartCoroutine(StartAutoRun());
			}

			if (End != null && testCases.Count == 0)
				End();
		}
	}
}
