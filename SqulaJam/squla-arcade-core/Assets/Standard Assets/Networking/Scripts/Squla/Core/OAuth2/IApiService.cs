
namespace Squla.Core.Network
{
	public interface IApiService
	{
		void GET (string url, ApiSuccess apiSuccess, ApiError apiError = null);

		void POST (string url, byte[] data, string contentType, ApiSuccess apiSuccess, ApiError apiError = null);

		void GET (GETRequest request);

		void POST (POSTRequest request);
	}
}
