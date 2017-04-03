using System;

namespace GATUtils.Connection.FileTransfer
{
    public interface IRemoteFile
    {
        string Filename { get; }
        string FilePath { get; }
        long FileSize { get; }

        DateTime? CreationDate { get; }
        DateTime? LastWriteTime { get; }
        DateTime? LastAccessTime { get; }
    }
}
