using System;
using System.Collections;
using Squla.Core.Logging;
using Squla.Core.ZeroQ;
using UnityEngine;

namespace Squla.Core.IOC
{
    public class MonoBehaviourV2 : MonoBehaviour
    {
        protected SmartLogger logger;

        protected ObjectGraph graph;

        protected Bus bus;

        protected void Awake()
        {
            logger = SmartLogger.GetLogger(GetType().FullName);

            graph = ObjectGraph.main;
            if (graph == null) {
                logger.Error($"ObjectGraph is not initialized at {gameObject.name}.");
            } else {
                graph.Resolve(this);
                bus = graph.Get<Bus>();
                AfterAwake();
                bus.Register(this);
            }
        }

        protected virtual void AfterAwake()
        {
        }

        protected virtual void OnEnable()
        {
            // We still register this onEnable to be able to update any model data that's there
            if (bus == null) {
                Debug.LogError(
                    $"{gameObject.name}.{GetType()} bus in null, On Awake registration didn't happen, ignoring the registration to the bus");
            } else {
                bus.Register(this);
            }
        }

        protected virtual void OnDisable()
        {
            if (bus != null) {
                // It is unbelievable: OnDisable is called before calling Awake in production.
                // If someone can find the reason I am curious to know.
                bus.UnRegister(this);
            } else if (gameObject != null) {
                Debug.LogError(
                    $"{gameObject.name}.{GetType()} bus in null, On Awake registration didn't happen, ignoring the unregister to the bus");
            }
        }

        IEnumerator ExecuteInNextFrameWorker(Action callback)
        {
            yield return new WaitForEndOfFrame();

            callback();
        }

        public void ExecuteInNextFrame(Action callback)
        {
            StartCoroutine(ExecuteInNextFrameWorker(callback));
        }
    }
}