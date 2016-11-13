using System.Collections.Generic;
using System.Threading.Tasks;

namespace GroorineCore.Api
{
	public interface IFolder
	{
		string Name { get; }
		string Path { get; }

		Task<IFile> GetFileAsync(string name);
		Task<IList<IFile>> GetFilesAsync();
		Task<IFolder> GetFolderAsync(string name);
		Task<IList<IFolder>> GetFoldersAsync();
	}
}