using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using GATUtils.Connection.Email;
using GATUtils.Logger;
using GATUtils.Utilities.FileUtils;

namespace GATUtils.Connection.FileTransfer
{
    public class FileTransferObject
    {
        public enum TransferProtocol { Ftp, Sftp }

        public string Name { get { return _name; } }
        public TransferProtocol ConnectionProtocol { get { return _protocol; } }

        public string HostAddress { get { return _host; } }
        public string Username { get { return _user; } }
        public string Password { get { return _pass; } }
        public int ConnectionPort { get { return _port; } }

        public string DefaultRootDirectory { get { return _rootDir; } }
        public string WorkingDirectory { get { return Handle.RemoteWorkingDirectory; } }

        public IFtpHandle Handle { get { return _transferHandle ?? _BuildTransferHandle(); } }

        public FileTransferObject(string name, TransferProtocol protocol, string host, string username, string pass, int port, string rootDirectory = null)
        {
            _name = name;
            _protocol = protocol;
            _host = host;
            _user = username;
            _pass = pass;
            _port = port;
            _rootDir = rootDirectory;
        }

        ~FileTransferObject()
        {
            if (_transferHandle != null)
            {
                _transferHandle.Disconnect();
                _transferHandle = null;
            }    
        }

        public FileTransferObject(XmlNode settings)
        {
            string temp;
            foreach (XmlNode innerNode in settings)
            {
                switch (innerNode.Name)
                {
                    case "Name":
                        _name = innerNode.InnerText.Trim();
                        break;
                    case "Protocol":
                        _protocol = TransferProtocol.Ftp;
                        temp = innerNode.InnerText.Trim();
                        if (temp.ToUpper().Equals("SFTP"))
                            _protocol = TransferProtocol.Sftp;
                        break;
                    case "Host":
                        _host = innerNode.InnerText.Trim();
                        break;
                    case "User":
                        _user = innerNode.InnerText.Trim();
                        break;
                    case "Pass":
                        _pass = innerNode.InnerText.Trim();
                        break;
                    case "Port":
                        temp = innerNode.InnerText.Trim();
                        if (!int.TryParse(temp, out _port))
                        {
                            switch (_protocol)
                            {
                                case TransferProtocol.Ftp:
                                    _port = 21;
                                    break;
                                case TransferProtocol.Sftp:
                                    _port = 22;
                                    break;
                            }
                            string message =
                                string.Format("Bad port value '{0}'. Using default port for {1} connection [{2}]", temp,
                                              _protocol, _port);
                            GatLogger.Instance.AddMessage(message);
                            SystemMailer.Instance.SendDevEmail(new Exception(message), "Issue creating File Transfer handle");
                        }
                        break;
                    case "RemoteDirectory":
                        _rootDir = innerNode.InnerText.Trim();
                        break;
                }
            }
        }

        public void PushFileList (List<string> fileList, string remoteDestination = null)
        {
            if (!FileTransferControl.Instance.IsActice)
            {
                string message =
                    string.Format("File Transfer Control is inactive (Transfer Handle '{0}').  {1} files Not Sent.",
                                  Name, fileList.Count);
                GatLogger.Instance.AddMessage(message, LogMode.Screen);
                string files = fileList.Aggregate(string.Empty, (current, file) => current + string.Format("\n\t{0}", Path.GetFileName(file)));
                GatLogger.Instance.AddMessage(message + files);
                return;
            }

            GatLogger.Instance.AddMessage(string.Format("Pushing {0} files to {1} [{2}]", fileList.Count, Name, string.IsNullOrEmpty(remoteDestination) ? DefaultRootDirectory : remoteDestination), LogMode.LogAndScreen);

            foreach (string file in fileList)
            {
                Handle.Push(file, remoteDestination);
            }

            GatLogger.Instance.AddMessage(string.Format("Push Complete."));
        }

        public List<string> PullFilesLike(string mask, string destinationFolder)
        {
            List<string> downloadedFiles = new List<string>();
            if (!FileTransferControl.Instance.IsActice)
            {
                string message =
                    string.Format("File Transfer Control is inactive (Transfer Handle '{0}').  {1} No files Acquired.",
                                  Name);
                GatLogger.Instance.AddMessage(message, LogMode.LogAndScreen);
                return downloadedFiles;
            }

            List<IRemoteFile> remoteDirContent = Handle.GetWorkingDirContents();
            foreach (IRemoteFile remoteFile in remoteDirContent)
            {
                if (remoteFile.Filename.Contains(mask))
                {
                    GatLogger.Instance.AddMessage(string.Format("Pulling {0} to {1}", remoteFile.Filename, destinationFolder));
                    Handle.Pull(remoteFile.Filename, destinationFolder);
                    downloadedFiles.Add(GatFile.Path(destinationFolder, string.Empty, remoteFile.Filename));
                }
            }

            GatLogger.Instance.AddMessage(string.Format("Pulling Files with mask complete. {0} file(s) found", downloadedFiles.Count));
            return downloadedFiles;
        }

        private IFtpHandle _BuildTransferHandle ()
        {
            switch (ConnectionProtocol)
            {
                case TransferProtocol.Ftp:
                    _transferHandle = new Ftp(HostAddress, Username, Password, ConnectionPort, DefaultRootDirectory, Name);
                    break;
                case TransferProtocol.Sftp:
                    _transferHandle = new Sftp(HostAddress, Username, Password, ConnectionPort, DefaultRootDirectory, Name);
                    break;
            }

            return _transferHandle;
        }

        private readonly string _name;
        private readonly TransferProtocol _protocol;
        
        private readonly string _host;
        private readonly string _user;
        private readonly string _pass;
        private readonly int _port;

        private readonly string _rootDir;

        private IFtpHandle _transferHandle;
    }
}
