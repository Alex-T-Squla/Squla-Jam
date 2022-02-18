
namespace Squla.Core
{
	public interface IParsedFromExternalFile
	{
		bool ParseFromExternalFile (string path);

		bool UpdateFromExternalFile (string path) ;
	}
}
