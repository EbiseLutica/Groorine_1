using System.IO;
using System.Threading.Tasks;

namespace Groorine.Api
{
	public interface IFile
	{
		string Name { get; }
		string Path { get; }
		Task<Stream> OpenAsync(FileAccessMode fileAccess);
	}
}