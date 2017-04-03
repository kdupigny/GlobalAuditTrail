using System;
using FtpLib;
using GATUtils.Logger;

namespace GATUtils.Connection.FileTransfer
{
    public class FtpFile : IRemoteFile
    {
        public string Filename { get { return _fileName; } }
        public string FilePath { get { return _filePath; } }
        public long FileSize
        {
            get
            {
                if (_size == 0) 
                    try
                    {
                        _size = _fileInfo.FtpConnection.GetFileSize(_fileName);
                    }
                    catch (Exception)
                    {
                        GatLogger.Instance.AddMessage(string.Format("Could not resolve size of FTP file {0}", _filePath));
                        _size = 0;
                    }

                return _size;
            }
        }
        public DateTime? CreationDate { get { return _creationDate; } }
        public DateTime? LastWriteTime { get { return _lastWriteTime; } }
        public DateTime? LastAccessTime { get { return _lastAcessTime; } }

        public FtpFile(FtpFileInfo fileInfo)
        {
            _fileName = fileInfo.Name;
            try
            {
                _filePath = fileInfo.FullName;
            }
            catch (Exception)
            {
                GatLogger.Instance.AddMessage(string.Format("Could not resolve path FTP file {0}, setting to current Directory", _fileName));
                _filePath = string.Format("./{0}",_fileName);
            }
            
            _creationDate = fileInfo.CreationTime;
            _lastAcessTime = fileInfo.LastAccessTime;
            _lastWriteTime = fileInfo.LastWriteTime;

            _fileInfo = fileInfo;
        }

        public FtpFile(Tamir.SharpSsh.jsch.ChannelSftp.LsEntry fileInfo)
        {
            _fileName = fileInfo.getFilename();
            _filePath = fileInfo.getLongname();
            _size = fileInfo.getAttrs().getSize();

            _creationDate = DateTime.Parse(fileInfo.getAttrs().getMtimeString());
            _lastAcessTime = DateTime.Parse(fileInfo.getAttrs().getAtimeString());
            _lastWriteTime = _lastAcessTime;

        }

        private readonly FtpFileInfo _fileInfo;
       
        private readonly string _fileName;
        private readonly string _filePath;
        private long _size;

        private readonly DateTime? _creationDate;
        private readonly DateTime? _lastWriteTime;
        private readonly DateTime? _lastAcessTime;

    }
}