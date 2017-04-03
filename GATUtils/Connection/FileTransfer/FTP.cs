using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FtpLib;
using GATUtils.Connection.Email;
using GATUtils.Logger;
using GATUtils.Utilities.FileUtils;

namespace GATUtils.Connection.FileTransfer
{
    public class Ftp : IFtpHandle
    {
        public FtpStatus ConnectionStatus
        {
            get
            {
                try
                {
                    //Send a dummy command to test server connection
                    //If an exception is thrown, we are not connected
                    //virtually all servers support help commands
                    _ftpSession.SendCommand("help");
                    return FtpStatus.Connected;
                }
                catch
                {
                    return FtpStatus.Disconnected;
                }
            }
        }

        public string RemoteWorkingDirectory
        {
            get { return _GetRemoteWorkingDirectory(); }
        }

        public string LocalWorkingDirectory
        {
            get { return _localPath; }
        }
        
        public Ftp(string server, string username, string password, int port = 21, string rootDir = null, string connectionName = "FtpConnection")
        {
            try
            {
                _host = server;
                _user = username;
                _pass = password;
                _port = port;
                _rootDir = rootDir;
                _connectionName = connectionName;

                _reconnectionAttempts = 0;
                _Init();

                GatLogger.Instance.AddMessage(string.Format("FTP Handle successfully created for {0}@{1} [{2}]", _user, _host, _connectionName), LogMode.LogAndScreen);
            }
            catch (Exception e)
            {
                string failMessage = string.Format("Ftp {0}@{1} [{2}] experienced the following issue", _user, _host,
                                                   _connectionName);
                GatLogger.Instance.AddMessage(failMessage);
                SystemMailer.Instance.SendDevEmail(e, failMessage);
                throw;
            }
        }

        ~Ftp()
        {
            //Disconnect
            if (_ftpSession != null)
            {
                Disconnect();
                //_ftpSession.Dispose();
                GC.Collect();
                _ftpSession = null;
            }
        }

        public bool Connect()
        {
            return ConnectionStatus == FtpStatus.Connected || _Reconnect();
        }

        public bool Disconnect()
        {
            try
            {
                _ftpSession.Close();
                _ftpSession.Dispose();
                _ftpSession = null;
                GC.Collect();
            }
            catch (FtpException e)
            {
                string failMessage = string.Format("An exception occurred closing {1}@{2} [{3}].\n{0}", e.Message, _user,
                                                   _host, _connectionName);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace " + e.StackTrace);
                SystemMailer.Instance.SendDevEmail(e, failMessage);
            }
            catch (Exception e)
            {
                string failMessage = string.Format("An exception has occurred closing {1}@{2} [{3}].\n{0}", e.Message,
                                                   _user, _host, _connectionName);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace " + e.StackTrace);
                SystemMailer.Instance.SendDevEmail(e, failMessage);

            }

            return true;
        }

        private bool _Reconnect()
        {
            try
            {
                if (_ftpSession != null)
                {
                    _ftpSession.Close();
                    _ftpSession.Dispose();
                    _ftpSession = null;
                    GC.Collect();
                }

                _ftpSession = new FtpConnection(_host, _port, _user, _pass);
                _ftpSession.Open();
                _ftpSession.Login();
                GatLogger.Instance.AddMessage(string.Format("Ftp Session {0}@{1} [{2}] connected successfully", _user, _host, _connectionName));
                return true;
            }
            catch (Exception)
            {
                if (_ftpSession != null)
                {
                    _ftpSession.Close();
                    _ftpSession.Dispose();
                    _ftpSession = null;
                    GC.Collect();
                }
            }
            
            return false;
        }

        public bool ChangeRemoteWorkingDirectory(string remotePath)
        {
            return _SetRemoteWorkingDirectory(remotePath);
        }

        public bool ChangeLocalWorkingDirectory(string remotePath)
        {
            return _SetLocalWorkingDirectory(remotePath);
        }

        public bool ResetToRootDirectory()
        {
            return ChangeRemoteWorkingDirectory(_rootDir);
        }

        public bool Push(string localFilename)
        {
            return Push(localFilename, null);
        }

        public bool Push(string filePath, string destinationPath = null)
        {
            string oldDir = "";
            try
            {
                Connect();
                oldDir = _ftpSession.GetCurrentDirectory();

                if (!string.IsNullOrEmpty(destinationPath))
                    _ftpSession.SetCurrentDirectory(destinationPath);
                
                _ftpSession.PutFile(filePath);
            }
            catch (FtpException e)
            {
                string failMessage = string.Format("FTP Exception while pushing files to {0}@{1} [{3}]: {2}", _user, _host, e.Message, _connectionName);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage(e.StackTrace);
                SystemMailer.Instance.SendDevEmail(e, failMessage);
                return false;
            }
            catch (Exception e)
            {
                string failMessage = string.Format("FTP General Exception while pushing files to {0}@{1} [{3}]: {2}", _user, _host, e.Message, _connectionName);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage(e.StackTrace);
                SystemMailer.Instance.SendDevEmail(e, failMessage);
                return false;
            }  
            finally
            {
                if(!string.IsNullOrEmpty(destinationPath))
                    _ftpSession.SetCurrentDirectory(oldDir);
            }
            return true;
        }

        public bool Pull(string remoteFilename)
        {
            return Pull(remoteFilename, null);
        }

        public bool Pull(string filename, string destinationFolder)
        {
            return _Pull(filename, destinationFolder);
        }

        private bool _Pull(string remoteFilePath, bool overWriteExistingFile = true)
        {
            try
            {
                Connect();
                if (_ftpSession != null)
                {
                    _ftpSession.GetFile(remoteFilePath, overWriteExistingFile);
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (FtpException e)
            {
                Console.WriteLine("An exception has occurred:\n{0}", e.Message);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception has occurred:\n{0}", e.Message);
                return false;
            }
        }

        private bool _Pull(string remoteFilePath, string localDestinationFilePath, bool overWriteExistingFile = true)
        {
            try
            {
                Connect();
                if (_ftpSession != null)
                {
                    if (string.IsNullOrEmpty(localDestinationFilePath))
                        localDestinationFilePath = LocalWorkingDirectory;

                    _ftpSession.GetFile(remoteFilePath, localDestinationFilePath + "\\" + remoteFilePath, overWriteExistingFile);
                    return true;
                }
                return false;
            }
            catch (FtpException e)
            {
                string failMessage = string.Format("An FTP Exception occurred pulling file {3} from {0}@{1} [{2}]. {4}", _user,
                                                   _host, _connectionName, remoteFilePath, e.Message);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace: " + e.StackTrace);
                SystemMailer.Instance.SendDevEmail(e, failMessage);
                return false;
            }
            catch (Exception e)
            {
                string failMessage = string.Format("An Exception occurred pulling file {3} from {0}@{1} [{2}]. {4}", _user,
                                                   _host, _connectionName, remoteFilePath, e.Message);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace: " + e.StackTrace);
                SystemMailer.Instance.SendDevEmail(e, failMessage);
                return false;
            }
        }

        public List<IRemoteFile> GetWorkingDirContents(string dir = "")
        {
            string oldRemoteDirectory = "";
            List<IRemoteFile> remoteFiles = new List<IRemoteFile>();
            try
            {
                if (_ftpSession != null)
                {
                    oldRemoteDirectory = _ftpSession.GetCurrentDirectory();
                    if (!string.IsNullOrEmpty(dir))
                        ChangeRemoteWorkingDirectory(dir);

                    FtpFileInfo[] files = _ftpSession.GetFiles();

                    remoteFiles.AddRange(files.Select(file => new FtpFile(file)).Cast<IRemoteFile>());
                    return remoteFiles;
                }
                return new List<IRemoteFile>();
            }
            catch (FtpException e)
            {
                dir = string.IsNullOrEmpty(dir) ? RemoteWorkingDirectory : dir;
                string failMessage = string.Format("An FTP Exception occurred getting file list from {0}@{1} [{2}] Directory:{3}. {4}", _user,
                                                   _host, _connectionName, dir, e.Message);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace: " + e.StackTrace);
                SystemMailer.Instance.SendDevEmail(e, failMessage);
                return new List<IRemoteFile>();
            }
            catch (Exception e)
            {
                dir = string.IsNullOrEmpty(dir) ? RemoteWorkingDirectory : dir;
                string failMessage = string.Format("An Exception occurred getting file list from {0}@{1} [{2}] Directory:{3}. {4}", _user,
                                                   _host, _connectionName, dir, e.Message);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace: " + e.StackTrace);
                SystemMailer.Instance.SendDevEmail(e, failMessage);
                return new List<IRemoteFile>();
            }
            finally
            {
                if (!string.IsNullOrEmpty(dir))
                    ChangeRemoteWorkingDirectory(oldRemoteDirectory);
            }
        }
  
        public bool RemoveFile(string remoteFilePath)
        {
            try
            {
                if (_ftpSession != null)
                {
                    _ftpSession.RemoveFile(remoteFilePath);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (FtpException e)
            {
                Console.WriteLine("An exception occurred:\n{0}", e.Message);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception has occurred:\n{0}", e.Message);
                return false;
            }
        }
        
        public bool VertifyConnection()
        {
            //TODO: Add code to handle Connection vertification
            return false;
        }

        public bool DirectoryExists(string remoteDirectoryPath)
        {
            //Checks if a given directory exists on the connecred server
            try
            {
                if (_ftpSession != null)
                {
                    return _ftpSession.DirectoryExists(remoteDirectoryPath);
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool FileExists(string remoteFilePath)
        {
            try
            {
                if (_ftpSession != null)
                {
                    return _ftpSession.FileExists(remoteFilePath);
                }
                return false;
            }
            catch (Exception e)
            {
                string failMessage =
                    string.Format("Exception thrown when checking for file on ftp {1}@{2} [{3}]. \n\t{0}", e.Message,
                                  _user, _host, _connectionName);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace " + e.StackTrace);
                SystemMailer.Instance.SendDevEmail(e, failMessage);
                return false;
            }
        }

        private void _Init()
        {
            bool connected;
            if(!(connected = _Reconnect()))
            {   
                while (_reconnectionAttempts < ct_maxPossibleAttempts)
                {
                    _reconnectionAttempts++;
                    connected = _Reconnect();
                    if (connected)
                    {
                        break;
                    }
                    continue;
                }
            }

            if (!connected)
            {
                GatLogger.Instance.AddMessage(string.Format("Could not connect to ftp {1}@{2} [{3}] after {0} tries.", ct_maxPossibleAttempts, _user, _host, _connectionName),
                                              LogMode.LogAndScreen);
            }
            else
            {
                _SetRemoteWorkingDirectory(_rootDir);
                _SetLocalWorkingDirectory(GatFile.Path(Dir.Temp, string.Empty));
            }
        }

        private bool _SetLocalWorkingDirectory(string newLocalDirectoryPath)
        {
            try
            {
                if (_ftpSession != null && Directory.Exists(newLocalDirectoryPath))
                {
                    _localPath = newLocalDirectoryPath;
                    _ftpSession.SetLocalDirectory(_localPath);
                    return true;
                }
                return false;
            }
            catch (FtpException e)
            {
                string failMessage =
                    string.Format("An ftp exception on {1}@{2} [{3}] occurred setting active local directory:\n{0}",
                                  e.Message, _user, _host, _connectionName);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace " + e.StackTrace);
                SystemMailer.Instance.SendDevEmail(e, failMessage);
                return false;
            }
            catch (Exception e)
            {
                string failMessage =
                    string.Format("An exception has occurred on {1}@{2} [{3}] setting active local directory:\n{0}",
                                  e.Message, _user, _host, _connectionName);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace " + e.StackTrace);
                SystemMailer.Instance.SendDevEmail(e, failMessage);
                return false;
            }
        }

        private bool _SetRemoteWorkingDirectory(string newRemoteDirectoryPath)
        {
            string oldRemoteDirectory = "";
            try
            {
                if (_ftpSession != null && !string.IsNullOrEmpty(newRemoteDirectoryPath))
                {
                    oldRemoteDirectory = _ftpSession.GetCurrentDirectory();
                    _ftpSession.SetCurrentDirectory(newRemoteDirectoryPath);
                    return true;
                }
                return false;
            }
            catch (FtpException e)
            {
                if (_ftpSession != null) _ftpSession.SetCurrentDirectory(oldRemoteDirectory);

                string failMessage = string.Format("An exception occurred changing directory on FTP connection {1}@{2} [{3}]:\n{0}", e.Message, _user, _host, _connectionName);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace: " + e.StackTrace);
                SystemMailer.Instance.SendDevEmail(e, failMessage);
                return false;
            }
            catch (Exception e)
            {
                if (_ftpSession != null) _ftpSession.SetCurrentDirectory(oldRemoteDirectory);

                string failMessage = string.Format("An exception occurred changing directory on FTP connection {1}@{2} [{3}]:\n{0}", e.Message, _user, _host, _connectionName);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace: " + e.StackTrace);
                SystemMailer.Instance.SendDevEmail(e, failMessage);
                return false;
            }
        }
        
        private string _GetRemoteWorkingDirectory()
        {
            try
            {
                return _ftpSession != null ? _ftpSession.GetCurrentDirectory() : "";
            }
            catch (Exception e)
            {
                string failMessage = string.Format("An exception occurred obtaining remote directory path on FTP connection {1}@{2} [{3}]:\n{0}", e.Message, _user, _host, _connectionName);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace: " + e.StackTrace);
                SystemMailer.Instance.SendDevEmail(e, failMessage);
                return "";
            }
        }
        private readonly string _host;
        private readonly int _port;

        private readonly string _user;
        private readonly string _pass;
        private readonly string _rootDir;

        private int _reconnectionAttempts;
        private const int ct_maxPossibleAttempts = 3;
        private FtpConnection _ftpSession;

        private string _localPath = "/";

        private readonly string _connectionName;

        #region OldCode
        
        public string CurrentStatus
        {
            get
            {
                try
                {
                    if (_ftpSession == null)
                    {
                        return "Status: Not connected.";
                    }
                    else
                    {
                        //Receive status of server
                        const string status = "N/A"; /*string.Format("Current local directory: \"{0}\"\n" +
                            "Current Remote directory: \"{1}\"\n" +
                            "Connected? : {2}","N/A",_session.GetCurrentDirectory(),(isConnected ? "Yes" : "No"));*/

                        return status;


                    }
                }
                catch (FtpException e)
                {
                    Console.WriteLine("An exception occurred:\n{0}", e.Message);
                    return "Server exception";
                }
                catch (Exception e)
                {
                    Console.WriteLine("An exception occurred:\n{0}", e.Message);
                    return "Server exception";
                }
            }
        }

        #endregion OldCode
    }
}
