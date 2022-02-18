using SimpleJson;

namespace Squla.Core.Network
{
    public interface IDataModel
    {
	    void OnBeforeModelize ();
        void OnModelized(JsonObject data);
        void OnAssetReady();
    }
}