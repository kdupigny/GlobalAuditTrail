using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GATUtils.Logger;
using Tamir.SharpSsh.jsch;
using System.IO;
using GATUtils.Connection.Email;
using GATUtils.Utilities.FileUtils;

namespace GATUtils.Connection.FileTransfer
{
    public class Sftp : IFtpHandle
    {
        public FtpStatus ConnectionStatus
        {
            get
            {
                if (_sftpSession != null && _sftpSession.isConnected())
                {
                    return FtpStatus.Connected;
                }
                return FtpStatus.Disconnected;
            }
        }
        
        public string RemoteWorkingDirectory
        {
            get
            {
                try
                {
                    return _sftpChannel.pwd();
                } catch
                {
                    return string.Empty;
                }
            }
        }
        
        public string LocalWorkingDirectory
        {
            get
            {
                try
                {
                    return _sftpChannel.lpwd();
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="server">The address of the FTP server</param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="rootDir"></param>
        /// <param name="port"></param>
        /// <param name="connectionName"></param>
        public Sftp(string server, string username, string password, int port = 22, string rootDir = null, string connectionName = "SftpConnection")
        {
            _host = server;
            _user = username;
            _pass = password;
            _port = port;
            _rootDir = rootDir;
            _connectionName = connectionName;

            _Init();
        }
        
        ~Sftp()
        {
            //Disconnect
            if (_sftpSession != null)
            {
                Disconnect();
                //_ftpSession.Dispose();
                GC.Collect();
                _sftpSession = null;
            }
        }

        public bool Connect()
        {
            return ConnectionStatus == FtpStatus.Connected || _Reconnect();
        }

        /// <summary>
        /// Attempts to re-establish a connection with a remote server
        /// </summary>
        /// <returns>true if re-connection succeded or false if re-connection did not succed</returns>
        private bool _Reconnect()
        {
            try
            {
                if (_sftpSession != null && !_sftpSession.isConnected())
                {
                    //Attempt to reconnect to server
                    if (_sftpChannel != null)
                        _sftpChannel.disconnect();

                    if (_channel != null)
                        _channel.disconnect();

                    _sftpChannel = null;
                    _channel = null;
                    _sftpSession = null;
                    _jsch = null;

                    _jsch = new JSch();
                    _sftpSession = _jsch.getSession(_user, _host, 22);
                    _sftpSession.setUserInfo(_ui);
                    _sftpSession.setPassword(_pass);
                    _sftpSession.connect();

                    _channel = _sftpSession.openChannel("sftp");
                    _channel.connect();

                    _sftpChannel = (ChannelSftp)_channel;

                    _ChangeRemoteWorkingDirectory(_rootDir);
                    return true;
                }
            }
            catch (SftpException sftp)
            {
                string failMessage = string.Format("Sftp Exception encountered attempting to reconnect {1}@{2} [{3}]: {0}", sftp.message,_user,_host,_connectionName);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace: " + sftp.StackTrace);
                SystemMailer.Instance.SendDevEmail(sftp, failMessage);
                return false;
            }
            catch (Exception ex)
            {
                string failMessage;
                if (ex.Message.Contains("actively refused"))
                    failMessage = string.Format("SFTP error check machine {0}@{1} [{2}] it may be inactive! {3}", _user, _host, _connectionName, ex.Message);
                else
                    failMessage = "An exception occurred while attempting to reconnect on host " + _user + "@" + _host +
                                  " [" + _connectionName + "] "  + ex.Message;

                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace: " + ex.StackTrace);
                SystemMailer.Instance.SendDevEmail(ex, failMessage);
                return false;
            }
            return false;
        }

        /// <summary>
        /// Shutdown the connection to the remote server
        /// </summary>
        public bool Disconnect()
        {
            if (_sftpSession.isConnected())
            {
                _sftpSession.disconnect();
                _channel.disconnect();
                _sftpChannel.disconnect();
                GatLogger.Instance.AddMessage(string.Format("We successfully disconnected {0}@{1} [{2}]", _user, _host, _connectionName));
            }

            return true;
        }

        public bool ChangeRemoteWorkingDirectory(string remotePath)
        {
            return _ChangeRemoteWorkingDirectory(remotePath);
        }

        public bool ChangeLocalWorkingDirectory(string localPath)
        {
            return _ChangeLocalWorkingDirectory(localPath);
        }

        public bool ResetToRootDirectory()
        {
            return ChangeRemoteWorkingDirectory(_rootDir);
        }

        public bool Push(string localFilename)
        {
            return Push(localFilename, null);
        }

        /// <summary>
        /// Uploads a file to a specified destination
        /// </summary>
        /// <param name="filePath">The source directory</param>
        /// <param name="destinationPath">The destination of the remote directory</param>
        /// <returns></returns>
        public bool Push(string filePath, string destinationPath)
        {
            while (!_VertifyConnection())
            {
                Thread.Sleep(500);
            }

            try
            {
                if (string.IsNullOrEmpty(destinationPath))
                    _sftpChannel.put(filePath, RemoteWorkingDirectory, ChannelSftp.OVERWRITE);
                else
                    _sftpChannel.put(filePath, destinationPath, ChannelSftp.OVERWRITE);
                return true;
            }
            catch (SftpException f)
            {
                string failMessage = string.Format("SFTP TAMIR Exception while pushing files to {0}@{1}: {2}", _user, _host, f.message);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage(f.StackTrace);
                SystemMailer.Instance.SendDevEmail(f, failMessage);
                return false;
            }
            catch (Exception e)
            {
                string failMessage = string.Format("SFTP general Exception while pushing files to {0}@{1}: {2}", _user, _host, e.Message);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage(e.StackTrace);
                SystemMailer.Instance.SendDevEmail(e, failMessage);
                return false;
            }
        }

        public bool Pull(string remoteFilename)
        {
            return Pull(remoteFilename, null);
        }

        /// <summary>
        /// Downloads specified file to the destination file path on the local machine
        /// </summary>
        /// <param name="filePath">The location of the file on the remote computer relative to the root directory</param>
        /// <param name="destinationFolder">Where to put the file locally on this computer</param>
        /// <returns></returns>
        public bool Pull(string filePath, string destinationFolder)
        {
            while (!_VertifyConnection())
            {
                Thread.Sleep(500);
            }

            string oldLocalDirectory=string.Empty;
            try
            {
                int mode = ChannelSftp.OVERWRITE;
                oldLocalDirectory = LocalWorkingDirectory;
                
                if (!string.IsNullOrEmpty(destinationFolder))
                    ChangeLocalWorkingDirectory(destinationFolder);

                _sftpChannel.get(filePath, LocalWorkingDirectory);
            }
            catch (SftpException f)
            {
                string failMessage = string.Format("SFTP TAMIR Exception while pulling files from {0}@{1}: {2}", _user, _host, f.message);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage(f.StackTrace);
                SystemMailer.Instance.SendDevEmail(f, failMessage);
                return false;
            }
            catch (Exception e)
            {
                string failMessage = string.Format("SFTP general Exception while pushing files from {0}@{1}: {2}", _user, _host, e.Message);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage(e.StackTrace);
                SystemMailer.Instance.SendDevEmail(e, failMessage);
                return false;
            }
            finally
            {
                if (!string.IsNullOrEmpty(destinationFolder))
                    ChangeLocalWorkingDirectory(oldLocalDirectory);
            }

            return true;
        }

        public List<IRemoteFile> GetWorkingDirContents(string dir = "")
        {
            ArrayList dirContents = _GetWorkingDirectoryContentsList(dir);
            return (from ChannelSftp.LsEntry file in dirContents where !file.getAttrs().isDir() select new FtpFile(file)).Cast<IRemoteFile>().ToList();
        }

        public bool FileExists(string remoteFilepath)
        {
            return _PathExist(remoteFilepath);
        }

        public bool DirectoryExists(string remotePath)
        {
            return _PathExist(remotePath);
        }

        /// <summary>
        /// Initializes connection to a remote Server with given credentials
        /// </summary>
        private void _Init()
        {
            if (_jsch != null)
            {
                try
                {
                    _sftpSession.disconnect();
                    _sftpSession = null;
                    _jsch = null;
                }
                catch (Exception e)
                {
                    _sftpSession = null;
                    _jsch = null;
                    string failMessage = string.Format("SFTP connection  {0}@{1} [{2}] could not be started ({3})",
                                                       _user, _host, _connectionName, e.Message);
                    GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                    GatLogger.Instance.AddMessage(e.StackTrace);
                    SystemMailer.Instance.SendDevEmail(e, failMessage);
                }

                //wait half a second before reconnecting so we dont flood FTP server with continued failed attempts
                Thread.Sleep(500);
            }

            try
            {
                _jsch = new JSch();
                _sftpSession = _jsch.getSession(_user, _host, _port);

                _ui = new SftpUserInfo(); // this is needed for the passkey
                _sftpSession.setUserInfo(_ui);

                _sftpSession.setPassword(_pass);
                _sftpSession.connect();

                _channel = _sftpSession.openChannel("sftp");
                _channel.connect();

                _sftpChannel = (ChannelSftp)_channel;
                //_ins = Console.OpenStandardInput();
                //_outs = Console.Out;

                _ChangeRemoteWorkingDirectory(_rootDir);
                _ChangeLocalWorkingDirectory(GatFile.Path(Dir.Temp, string.Empty));
            }
            catch (Exception e)
            {
                if (!_reportOnce)
                {
                    string failMsg = string.Format("SFTP Connection is failing ({2}@{0}:{1}) \n\t{3}", _host, _port,
                                                   _user, e.Message);
                    GatLogger.Instance.AddMessage(failMsg, LogMode.LogAndScreen);;
                    GatLogger.Instance.AddMessage(string.Format("Exception Stack: {0}", e.StackTrace));
                    _reportOnce = true;

                    SystemMailer.Instance.SendDevEmail(e, failMsg);
                }
            }
        }

        /// <summary>
        /// Specifies if the session is still connected
        /// <para>If not an attempt to reconnect will occur</para>
        /// </summary>
        /// <returns>true if the connection is established
        /// 
        /// <para>false if the connection was not established</para>
        /// 
        /// </returns>
        private bool _VertifyConnection()
        {
            if (!_sftpSession.isConnected())
            {
                if (!_Reconnect())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns list of current working directory Contents
        /// </summary>
        /// <returns></returns>
        private ArrayList _GetWorkingDirectoryContentsList(string dir)
        {
            while (!_VertifyConnection())
            {
                Thread.Sleep(500);
            }

            string oldDirectory = string.Empty;
            try
            {
                oldDirectory = _sftpChannel.pwd();
                if (!string.IsNullOrEmpty(dir))
                    ChangeRemoteWorkingDirectory(dir);

                return _sftpChannel.ls(_sftpChannel.pwd());
            }
            catch (SftpException sftp)
            {
                string failMessage = string.Format("Sftp Exception encountered reading directory on {1}@{2} [{3}]: {0}", sftp.message, _user, _host, _connectionName);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace: " + sftp.StackTrace);

                SystemMailer.Instance.SendDevEmail(sftp, failMessage);
                return null;
            }
            catch (Exception e)
            {
                string failMessage = string.Format("Exception encountered reading directory on {1}@{2} [{3}]: {0}", e.Message, _user, _host, _connectionName);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace: " + e.StackTrace);
                SystemMailer.Instance.SendDevEmail(e, failMessage);
                return null;
            }
            finally
            {
                if (!string.IsNullOrEmpty(dir))
                    ChangeRemoteWorkingDirectory(oldDirectory);
            }
        }

        /// <summary>
        /// Checks if a path exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns>true if the specified path exists or false if the path does not exist</returns>
        private bool _PathExist(string path)
        {
            while (!_VertifyConnection())
            {
                Thread.Sleep(500);
            }

            try
            {
                _sftpChannel.ls(_rootDir + path);
                return true;
            }
            catch (SftpException sftp)
            {
                GatLogger.Instance.AddMessage(string.Format("Sftp Exception encountered validating path: {0}", sftp.message), LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("InnerException: " + sftp.InnerException);
                GatLogger.Instance.AddMessage("StackTrace: " + sftp.StackTrace);
                return false;
            }
            catch (Exception e)
            {
                GatLogger.Instance.AddMessage("An error occurred while checking path existence\n" + e.Message +
                    "\n" + e.StackTrace, LogMode.LogAndScreen);
                return false;
            }
        }

        /// <summary>
        /// Checks if a file exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns>true if the specified path exists or false if the path does not exist</returns>
        private bool _FileExist(string path)
        {
            while (!_VertifyConnection())
            {
                Thread.Sleep(500);
            }

            try
            {
                List<IRemoteFile> files = GetWorkingDirContents(path);
                if (files.Any(file => file.Filename.Equals(Path.GetFileName(path))))
                {
                    return true;
                }
            }
            catch (SftpException sftp)
            {
                GatLogger.Instance.AddMessage(string.Format("Sftp Exception encountered validating path: {0}", sftp.message), LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("InnerException: " + sftp.InnerException);
                GatLogger.Instance.AddMessage("StackTrace: " + sftp.StackTrace);
            }
            catch (Exception e)
            {
                GatLogger.Instance.AddMessage("An error occurred while checking path existence\n" + e.Message +
                    "\n" + e.StackTrace, LogMode.LogAndScreen);
            }
                
            return false;
        }

        /// <summary>
        /// Chnages the local working directory to the directory specified by 
        /// <para>the localDirectoryPath" parameter</para>
        /// </summary>
        /// <param name="localDirectoryPath">The path of the new working directory</param>
        /// <returns>true if the local working directory is changed or 
        /// <para>false is the working directory remains unchanged</para>
        /// </returns>
        private bool _ChangeLocalWorkingDirectory(string localDirectoryPath)
        {
            try
            {
                _sftpChannel.lcd(localDirectoryPath);
            }
            catch (SftpException sftp)
            {
                string failMessage = string.Format("An SFTP exception occurred changing Local directory on SFTP connection {1}@{2} [{3}]:\n{0}", sftp.Message, _user, _host, _connectionName);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace: " + sftp.StackTrace);
                SystemMailer.Instance.SendDevEmail(sftp, failMessage);
                return false;
            }
            catch (Exception ex)
            {
                string failMessage = string.Format("An exception occurred changing Local directory on SFTP connection {1}@{2} [{3}]:\n{0}", ex.Message, _user, _host, _connectionName);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                SystemMailer.Instance.SendDevEmail(ex, failMessage);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Changes the current remote working directory to the specified directory
        /// <para>specified by "remoteDirectoryPath</para>
        /// </summary>
        /// <param name="remoteDirectoryPath">The path of the new remote working directory</param>
        /// <returns>true if the working directory is changed or false if the current 
        /// <para>directory remains unchanged</para>
        /// </returns>
        private bool _ChangeRemoteWorkingDirectory(string remoteDirectoryPath)
        {
            while (!_VertifyConnection())
            {
                Thread.Sleep(500);
            }

            try
            {
                if (!string.IsNullOrEmpty(remoteDirectoryPath))
                    _sftpChannel.cd(remoteDirectoryPath);
            }
            catch (SftpException sftp)
            {
                string failMessage = string.Format("An SFTP exception occurred changing Remote directory on SFTP connection {1}@{2} [{3}]:\n{0}", sftp.Message, _user, _host, _connectionName);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace: " + sftp.StackTrace);
                SystemMailer.Instance.SendDevEmail(sftp, failMessage);
                return false;
            }
            catch (Exception ex)
            {
                string failMessage = string.Format("An exception occurred changing Remote directory on SFTP connection {1}@{2} [{3}]:\n{0}", ex.Message, _user, _host, _connectionName);
                GatLogger.Instance.AddMessage(failMessage, LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("StackTrace: " + ex.StackTrace);
                SystemMailer.Instance.SendDevEmail(ex, failMessage);
                return false;
            }

            return true;
        }

        JSch _jsch;

        private bool _reportOnce;

        private readonly string _host;
        private readonly string _user;
        private readonly string _pass;
        private readonly string _rootDir;
        private readonly string _connectionName;

        private readonly int _port = 22;
        Session _sftpSession;
        ChannelSftp _sftpChannel;

        // username and password will be given via UserInfo interface.
        UserInfo _ui; // this is needed for the passkey
        Channel _channel;

        //Stream _ins;
        //TextWriter _outs;
       
        
    }
}
