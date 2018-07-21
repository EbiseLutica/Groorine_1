namespace Groorine.Api
{
	public interface IFileSystem
	{
		IFolder BaseFolder { get; }
		IFolder LocalFolder { get; }
	}
}
