using System.Collections.Generic;
using System.Linq;
using SimpleJson;
using Squla.Core.DataManager;
using Squla.Core.Modelize;

namespace Squla.Core.Network
{
    public abstract class BasePropertySpec
    {
        internal string name;

        public BasePropertySpec (string name)
        {
            this.name = name;
        }

        internal abstract void Process(System.Object source, AssetBatch assetBatch);
    }

    public class DataSpec: BasePropertySpec
    {
        internal string onSuccessCommand;

        private List<BasePropertySpec> specs;
        private List<BasePropertySpec> nestedSpecs;

        public DataSpec (string name): base(name)
        {
        }

        public DataSpec OnSuccess(string command)
        {
            onSuccessCommand = command;
            return this;
        }

        public DataSpec Image (string name, string cacheName)
        {
            Init();
            specs.Add (new ImageAssetSpec (name, cacheName));
            return this;
        }

        public DataSpec Audio (string name, string cacheName)
        {
            Init();
            specs.Add (new AudioAssetSpec (name, cacheName));
            return this;
        }

        public DataSpec Nested(string name)
        {
            var nested = new DataSpec(name);
            if (nestedSpecs == null)
                nestedSpecs = new List<BasePropertySpec>();
            nestedSpecs.Add (nested);
            return nested;
        }

        private void Init()
        {
            if (specs == null)
                specs = new List<BasePropertySpec> ();
        }

        internal override void Process(System.Object source, AssetBatch assetBatch)
        {
	        if (specs == null)
		        return;

            var asObject = source as JsonObject;
            if (asObject != null) {
                for (var i = 0; i < specs.Count; i++) {
                    specs[i].Process(source, assetBatch);
                }

                ProcessNested(asObject, assetBatch);
            }

            var asArray = source as JsonArray;
            if (asArray != null) {
                for (var i = 0; i < asArray.Count; i++) {
                    Process(asArray[i], assetBatch);
                }
            }
        }

        private void ProcessNested(JsonObject source, AssetBatch assetBatch)
        {
            if (nestedSpecs == null)
                return;

            for (var i = 0; i < nestedSpecs.Count; i++) {
                var spec = (DataSpec)nestedSpecs[i];
                if (!source.ContainsKey(spec.name))
                    continue;

                spec.Process(source[spec.name], assetBatch);
            }
        }
    }

    public abstract class AssetPropertySpec: BasePropertySpec
    {
        protected readonly string cacheName;

        protected AssetPropertySpec (string name, string cacheName): base(name)
        {
            this.cacheName = cacheName;
        }

        protected string GetValue(System.Object source)
        {
            var json = source as JsonObject;
            if (json == null)
                return string.Empty;

            if (!json.ContainsKey(name))
                return string.Empty;

            var s = json[name] as string;
            return s ?? string.Empty;
        }
    }

    public class AudioAssetSpec : AssetPropertySpec
    {
        public AudioAssetSpec (string name, string cacheName) : base (name, cacheName)
        {
        }

        internal override void Process(System.Object source, AssetBatch assetBatch)
        {
            assetBatch.AddAudioUrl(cacheName, GetValue(source));
        }
    }

    public class ImageAssetSpec : AssetPropertySpec
    {
        public ImageAssetSpec (string name, string cacheName): base(name, cacheName)
        {
        }

        internal override void Process(System.Object source, AssetBatch assetBatch)
        {
            assetBatch.AddImageUrl(cacheName, GetValue(source));
        }
    }
}