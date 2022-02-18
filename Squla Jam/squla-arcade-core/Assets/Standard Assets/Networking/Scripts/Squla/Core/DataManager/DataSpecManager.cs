using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SimpleJson;
using Squla.Core.IOC;
using Squla.Core.Modelize;
using Squla.Core.Network;
using UnityEngine;

namespace Squla.Core.DataManager
{
	static class TypeHelper
	{
		public static bool HasMethod(this Type type, string methodName)
		{
			return type.GetMethod(methodName) != null;
		}
	}

    internal class DataSpecManager
    {
        private static readonly Type specType = typeof(DataSpec);

        private readonly Dictionary<string, DataSpec_Impl> dataSpecs = new Dictionary<string, DataSpec_Impl>();
        private readonly Dictionary<string, System.Object> cache = new Dictionary<string, object>();
        private readonly ObjectGraph graph;
        private readonly IModelizer modelizer;

        public DataSpecManager(ObjectGraph graph, IModelizer modelizer)
        {
            this.graph = graph;
            this.modelizer = modelizer;
        }

        public void Flush ()
        {
	        cache.Clear ();
        }

        public System.Object Sync_GET(string url)
        {
	        cache.TryGetValue(url, out var instance);
	        return instance;
        }

        public System.Object ProcessResponse(string url, JsonObject jsonData, System.Action<System.Object> onReady=null)
        {
	        if (!jsonData.TryGetValue("type", out var targetTypeNameOut)) {
		        if (Debug.isDebugBuild) {
			        Debug.LogError ($"{url} response type doesn't have `type` attribute in response");
		        }
		        return null;
            }

            var targetTypeName = (string) targetTypeNameOut;
            if (!dataSpecs.ContainsKey(targetTypeName)) {
	            if (Debug.isDebugBuild) {
					Debug.LogError($"{targetTypeName} response type is not registered");
	            }
                return null;
            }

            var dataSpecImpl = dataSpecs[targetTypeName];

            if (!cache.TryGetValue (url, out var instance)) {
		        instance = graph.Get(dataSpecImpl.TargetType);
	        }

			var dataModel = instance as IDataModel;
	        dataModel?.OnBeforeModelize ();
	        modelizer.Modelize(instance, jsonData);

            var resp = dataSpecImpl.Process(graph, instance, jsonData, onReady);
            if (resp != null) {
	            cache [url] = resp;
            }
	        return resp;
        }

	    public void ProcessPreFetchedData(JsonObject jsonObject)
	    {
		    if (!jsonObject.ContainsKey(Constants.k_PRE_FETCHED_DATA))
			    return;

		    var asList = (JsonArray) jsonObject[Constants.k_PRE_FETCHED_DATA];
            jsonObject.Remove(Constants.k_PRE_FETCHED_DATA);

		    for (var i = 0; i < asList.Count; i++) {
			    var item = (JsonObject) asList[i];
			    var href = (string) item[Constants.k_HREF];
				var data = (JsonObject) item[Constants.k_DATA];
			    ProcessResponse(href, data);
		    }
	    }
	    
	    public void DetectAndCreateDataSpecs()
	    {
		    //Takes about 1s extra on iPodTouch
		    Type[] specTypes = AppDomain.CurrentDomain.GetAssemblies()
			    .SelectMany(t => t.GetTypes())
			    .Where(t => t.IsClass && !t.IsInterface && !t.IsAbstract && t.HasMethod("ProvideSpec")).ToArray();

		    for (var i=0; i<specTypes.Length; i++) {
			    CreateDataSpecImpl(specTypes[i]);
		    }
	    }

	    private void CreateDataSpecImpl(Type specType)
        {
            var methodInfos = specType.GetMethods(BindingFlags.Static | BindingFlags.Public);
            var found = false;

	        var singletonAttr = (Singleton)Attribute.GetCustomAttribute (specType, typeof(Singleton));

            for (var j = 0; j < methodInfos.Length; j++) {
                var mInfo = methodInfos[j];

                var pInfos = mInfo.GetParameters();
                if (pInfos.Length != 0)
                    continue;

                if (mInfo.ReturnType == DataSpecManager.specType) {
                    var dataSpec = (DataSpec) mInfo.Invoke(null, null);
	                var impl = new DataSpec_Impl(specType, singletonAttr != null, dataSpec);
                    dataSpecs[impl.ResponseTypeName] = impl;
                    found = true;
                    break;
                }
            }

            if (!found) {
                Debug.LogError($"{specType.FullName} doesn't have static method with return type of {DataSpecManager.specType.FullName}");
            }
        }
    }
}