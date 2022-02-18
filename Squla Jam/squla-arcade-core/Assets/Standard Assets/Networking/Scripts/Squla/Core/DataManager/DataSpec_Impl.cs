using System;
using SimpleJson;
using Squla.Core.IOC;
using Squla.Core.Network;

namespace Squla.Core.DataManager
{
    internal class DataSpec_Impl
    {
        internal readonly Type TargetType;
        private readonly DataSpec dataSpec;
	    private readonly bool isSingleton;
        public string ResponseTypeName { get; private set; }

        public DataSpec_Impl(Type targetType, bool isSingleton, DataSpec dataSpec)
        {
            TargetType = targetType;
	        this.isSingleton = isSingleton;
            ResponseTypeName = dataSpec.name;
            this.dataSpec = dataSpec;
        }

        public System.Object Process(ObjectGraph graph, System.Object instance, JsonObject jsonData, System.Action<System.Object> onReady)
        {
            var dataModel = instance as IDataModel;

	        // call level1 method on the instance.
	        dataModel?.OnModelized(jsonData);

	        var assetBatch = new AssetBatch(()=>
            {
	            dataModel?.OnAssetReady();
	            onReady?.Invoke (instance);
            });
            dataSpec.Process(jsonData, assetBatch);
            assetBatch.Download(graph);

            return instance is IDataNoCacheModel ? null : instance;
        }
    }
}