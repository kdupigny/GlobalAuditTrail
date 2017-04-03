using System.Collections.Generic;

namespace GATUtils.Connection.FileTransfer
{
    public interface IFtpHandle
    {
        FtpStatus ConnectionStatus { get; }
        string RemoteWorkingDirectory { get; }
        string LocalWorkingDirectory { get; }

        bool Connect();
        bool Disconnect();

        bool ChangeRemoteWorkingDirectory(string remotePath);
        bool ChangeLocalWorkingDirectory(string remotePath);

        bool FileExists(string remoteFilepath);
        bool DirectoryExists(string remotePath);

        bool Push(string localFilename);
        bool Push(string localFilename, string remoteFilename);
        
        bool Pull(string remoteFilename);
        bool Pull(string remoteFilename, string localFilename);

        List<IRemoteFile> GetWorkingDirContents(string dir = "");
    }
}
