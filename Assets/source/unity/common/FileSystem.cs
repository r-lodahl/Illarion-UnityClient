using Illarion.Client.Common;

namespace Illarion.Client.Unity.Common
{
    public class FileSystem : IFileSystem
    {
        public string UserDirectory {get; private set;}

        public FileSystem(string userDirectory)
        {
            UserDirectory = userDirectory;
        }
    }    
}