using System.IO;
using System.Threading.Tasks;

namespace GroorineCore.Api
{
	public interface IFile
	{
		string Name { get; }
		string Path { get; }
		Task<Stream> OpenAsync(FileAccessMode fileAccess);
	}
}