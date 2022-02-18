using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJson;
using Squla.Core.IOC;
using Squla.Core.Logging;
using UnityEngine;


namespace Squla.Core.Network
{
    public class ScreenHUD_RequestManager
    {
        public bool IsErrored { get; private set; }

        private readonly IScreenHUD_UI hudUX;

        private readonly SmartLogger logger = SmartLogger.GetLogger<ScreenHUD_RequestManager> ();

        private readonly Queue<JsonRequest> erroredRequests = new Queue<JsonRequest> ();

        private readonly Dictionary<string, JsonRequest> runningTransactions = new Dictionary<string, JsonRequest> ();

        private readonly JsonRequestManager requestManager;

        private readonly SpinnerModel spinnerModel;

        public event System.Action Completed;
        public event System.Action Aborted;
        public event System.Action BeforeRetry;

        [Inject]
        public ScreenHUD_RequestManager(IScreenHUD_UI hudUX, JsonRequestManager requestManager, SpinnerModel spinnerModel)
        {
            this.hudUX = hudUX;
            this.requestManager = requestManager;
            this.spinnerModel = spinnerModel;
            hudUX.Retry += OnHudUXRetry;
            hudUX.Abort += OnHudUXAbort;
        }

        public bool Execute (JsonRequest request)
        {
            if (request.background) {
                ExecuteRequest(request);
                return runningTransactions.Count != 0;
            }

            if (runningTransactions.ContainsKey (request.url)) {
                logger.Error("Execute '{0} {1}' is already running. Nesting is not possible.", request.Method, request.url);
                return true;
            }

            runningTransactions.Add (request.url, request);
	        spinnerModel.Begin(request.url);

	        logger.Debug ("Execute: {0}: {1} {2}", runningTransactions.Count, request.Method, request.url);

            ExecuteRequest(request);
            return true;
        }

        private void ExecuteRequest(JsonRequest request)
        {
            var origOnComplete = request.onComplete;
            request.onComplete = (req, code, resp) => {
                if (RequestCompletePhase1(req))
                    if (RequestCompletePhase2(req, code, resp, origOnComplete))
                        RequestCompletePhase3(req);
                return true;
            };
            requestManager.Execute (request);
        }

        private bool RequestCompletePhase1(JsonRequest request)
        {
            if (request.background)
                return true;

            var key = request.url;
            if (!runningTransactions.ContainsKey(key)) {
                logger.Info("RequestCompletePhase1 was already executed for '{0}'", key);
                return false;
            }

            return true;
        }

        private bool RequestCompletePhase2(JsonRequest request, int statusCode, JsonObject response,
            Func<JsonRequest, int, JsonObject, bool> origOnComplete)
        {
            var isHandled = false;
            if (statusCode != 0 && statusCode < 500) {
	            // WARNING: Exception happening in executing origOnComplete() won't unblock the HUD.
                isHandled = origOnComplete (request, statusCode, response);
            }

            if (isHandled) {
                if (!request.background) {
                    var key = request.url;
                    runningTransactions.Remove(key);
                    spinnerModel.End(key);
                }
            } else {
                logger.Debug ("Request failed due to connection or other errors: {0} {1}", request.Method, request.url);

                erroredRequests.Enqueue (request);
                if (!IsErrored) {
                    IsErrored = true;
                    logger.Debug ("Should only see this once");
                    hudUX.ChangeToErrored (statusCode, response);
                }
            }

            return !request.background;
        }

        /// <summary>
        /// This method won't be executed for background requests.
        /// When all the queued requests are completed with in this transaction.
        /// this method will call Completed event handlers.
        /// </summary>
        /// <param name="request"></param>
        private void RequestCompletePhase3 (JsonRequest request)
        {
            var key = request.url;
            var count = runningTransactions.Count;
            logger.Debug ("OnRequestComplete: Transaction {0} with key: {1}", count, key);

            if (count > 0) {
                if (logger.IsDebugLevel) {
                    logger.Debug ("OpenRequests: {0}", string.Join (",", runningTransactions.Keys.ToArray ()));
                }
            } else {
                logger.Debug ("AllRequests completed");
                if (Completed != null)
	                Completed();
            }
        }

        public void TerminateAll()
        {
            logger.Debug ("Terminating all ({0}) requests", runningTransactions.Count);
            ReleaseRequests (runningTransactions.Values.ToList ());
            ReleaseRequests (erroredRequests);
			spinnerModel.Clear();
            runningTransactions.Clear();
            IsErrored = false;
        }

        private void OnHudUXAbort ()
        {
            logger.Debug ("Aborting {0} transaction(s) will be aborted", erroredRequests.Count);
            ReleaseRequests (erroredRequests);

            runningTransactions.Clear ();
            IsErrored = false;

            if (Aborted != null)
	            Aborted();
        }

        private void OnHudUXRetry ()
        {
            logger.Debug ("Retrying {0} errored requests", erroredRequests.Count);
            var list = erroredRequests.ToArray();
            erroredRequests.Clear();
            IsErrored = false;

            spinnerModel.ResetTime();
            var retry = BeforeRetry;
            if (retry != null)
                retry();

            for (var i=0; i<list.Length; i++){
                var request = list[i];
                logger.Debug ("Retrying: {0}", request.url);
                
                if (request.background) {
	                runningTransactions.Add (request.url, request);
	                spinnerModel.Begin(request.url);
	                request.background = false;
                }

                requestManager.Execute (request);
            }
        }

        private void ReleaseRequests (IList<JsonRequest> requests)
        {
            while (requests.Count > 0) {
                var req = requests [0];
	            logger.Debug("aborting request {0}", req.url);
                req.Abort();
                requests.RemoveAt (0);
            }
        }

        private void ReleaseRequests (Queue<JsonRequest> requests)
        {
            while (requests.Count > 0) {
                var req = requests.Dequeue();
	            logger.Debug("aborting request {0}", req.url);
                req.Abort();
            }
        }
    }
}
