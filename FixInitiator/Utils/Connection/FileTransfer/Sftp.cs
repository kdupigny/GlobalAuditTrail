using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GATUtils.Logger;
using Tamir.SharpSsh.jsch;
using System.IO;

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
        /// Default Constructor
        /// </summary>
        public Sftp()
        {
            _host = "69.164.218.32";
            _user = "kevin";
            _pass = "k123D#@!";
            _rootDir = "/home/trade_files/";
            _Init();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftPserver">The address of the FTP server</param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="rootDir"></param>
        /// <param name="port"></param>
        public Sftp(string ftPserver, string username, string password, string rootDir, int port = 22)
        {
            _host = ftPserver;
            _user = username;
            _pass = password;
            _port = port;
            _rootDir = rootDir;

            _Init();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ftPserver">The address of the FTP server</param>
        /// <param name="username">Sftp session Username</param>
        /// <param name="password">Sftp session Password</param>
        /// <param name="port">Connect port defaults to 22 if omitted</param>
        public Sftp(string ftPserver, string username, string password, int port = 22)
        {
            _host = ftPserver;
            _user = username;
            _pass = password;
            _port = port;
            
            _Init();
        }
        
        public bool Connect()
        {
            if (_sftpSession != null && !_sftpSession.isConnected())
            {
                return true;
            }

            return _Reconnect();
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
                GatLogger.Instance.AddMessage("We successfully disconnected");
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

        public bool FileExists(string remoteFilepath)
        {
            return _PathExist(remoteFilepath);
        }

        public bool DirectoryExists(string remotePath)
        {
            return _PathExist(remotePath);
        }

        /// <summary>
        /// Uploads a file to root directory
        /// </summary>
        /// <param name="localFilePath"></param>
        /// <returns></returns>
        public bool Push(string localFilePath)
        {
            if (!_VertifyConnection())
            {
                Thread.Sleep(500);
            }

            try
            {
                int mode = ChannelSftp.OVERWRITE;
                // Put File in user's home dir using OVERWRITE mode
                _sftpChannel.put(localFilePath, _rootDir, mode);
            }
            catch (SftpException f)
            {
                Console.WriteLine("Sftp TAMIR FAILED: " + f.message + "\n" + f.StackTrace, true);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception TAMIR FAILED: " + e.Message + "\n" + e.StackTrace, true);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Uploads a file to a specified destination
        /// </summary>
        /// <param name="filePath">The source directory</param>
        /// <param name="destinationPath">The destination of the remote directory</param>
        /// <returns></returns>
        public bool Push(string filePath, string destinationPath)
        {
            //if (utils.inDebug)
            //{
            //    Console.WriteLine("Application in Debug Mode file: " + filePath + " has not been sent.");
            //    return true;
            //}

            while (!_VertifyConnection())
            {
                Thread.Sleep(500);
            }

            try
            {
                _sftpChannel.put(filePath, destinationPath, ChannelSftp.OVERWRITE);
                return true;
            }
            catch (SftpException sftp)
            {
                GatLogger.Instance.AddMessage(string.Format("Sftp Exception encountered pushing the file: {0}", sftp.message), LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("InnerException: " + sftp.InnerException);
                GatLogger.Instance.AddMessage("StackTrace: " + sftp.StackTrace);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occurred while uploading the file\n" + e.Message +
                    "\n" + e.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Downloads specified file from the server to application directory
        /// </summary>
        /// <param name="filePath">The path of the file located on the server starting from the root directory</param>
        /// <returns></returns>
        public bool Pull(string filePath)
        {
            while (!_VertifyConnection())
            {
                Thread.Sleep(500);
            }

            try
            {                
                int mode = ChannelSftp.OVERWRITE;
                // Pull File in user's home dir using OVERWRITE mode
                _sftpChannel.get(filePath, mode);               
            }
            catch (SftpException sftp)
            {
                GatLogger.Instance.AddMessage(string.Format("Sftp Exception encountered getting file: {0}", sftp.message), LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("InnerException: " + sftp.InnerException);
                GatLogger.Instance.AddMessage("StackTrace: " + sftp.StackTrace);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception TAMIR FAILED: " + e.Message + "\n" + e.StackTrace);
                return false;
            }            

            return true;
        }

        /// <summary>
        /// Downloads specified file to the destination file path on the local machine
        /// </summary>
        /// <param name="filePath">The location of the file on the remote computer relative to the root directory</param>
        /// <param name="destinationFilePath">Where to put the file locally on this computer</param>
        /// <returns></returns>
        public bool Pull(string filePath, string destinationFilePath)
        {
            while (!_VertifyConnection())
            {
                Thread.Sleep(500);
            }

            try
            {              
                int mode = ChannelSftp.OVERWRITE;
                // Pull File in user's home dir using OVERWRITE mode                
                ChangeLocalWorkingDirectory(destinationFilePath);
                _sftpChannel.get(filePath, mode);
            }
            catch (SftpException sftp)
            {
                GatLogger.Instance.AddMessage(string.Format("Sftp Exception encountered getting file: {0}", sftp.message), LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("InnerException: " + sftp.InnerException);
                GatLogger.Instance.AddMessage("StackTrace: " + sftp.StackTrace);
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception TAMIR FAILED: " + e.Message + "\n" + e.StackTrace);
                return false;
            }

            return true;
        }

        public List<IRemoteFile> GetWorkingDirContents(string dir = "")
        {
            ArrayList dirContents = _GetWorkingDirectoryContentsList();

            return (from ChannelSftp.LsEntry file in dirContents select new FtpFile(file)).Cast<IRemoteFile>().ToList();
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
                    GatLogger.Instance.AddMessage("Attempting to disconnect SFTP connection ", LogMode.LogAndScreen);
                    _sftpSession.disconnect();
                    _sftpSession = null;
                    _jsch = null;
                }
                catch (Exception e)
                {
                    _sftpSession = null;
                    _jsch = null;
                    GatLogger.Instance.AddMessage("There was a failure while restarting connection " + e.Message);
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
            }
            catch (Exception e)
            {
                if (!_reportOnce)
                {
                    string failMsg = string.Format("SFTP Connection is failing ({2}@{0}:{1}) \n\t{3}", _host, _port,
                                                   _user, e.Message);
                    GatLogger.Instance.AddMessage(failMsg, LogMode.LogAndScreen);
                    GatLogger.Instance.AddMessage(string.Format("Inner Exception: {0}", e.InnerException));
                    GatLogger.Instance.AddMessage(string.Format("Exception Stack: {0}", e.StackTrace));
                    _reportOnce = true;
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
        private ArrayList _GetWorkingDirectoryContentsList()
        {
            while (!_VertifyConnection())
            {
                Thread.Sleep(500);
            }

            try
            {
                return _sftpChannel.ls(_sftpChannel.pwd());
            }
            catch (SftpException sftp)
            {

                GatLogger.Instance.AddMessage(string.Format("Sftp Exception encountered reading directory: {0}", sftp.message), LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("InnerException: " + sftp.InnerException);
                GatLogger.Instance.AddMessage("StackTrace: " + sftp.StackTrace);
                return null;
            }
            catch (Exception e)
            {
                GatLogger.Instance.AddMessage("An exception occurred while attempting to list directory contents:\n" + e, LogMode.LogAndScreen);
                return null;
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
                    if (_sftpChannel != null )
                    _sftpChannel.disconnect();

                    if(_channel != null)
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
                GatLogger.Instance.AddMessage(string.Format("Sftp Exception encountered attempting to reconnect: {0}", sftp.message), LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("InnerException: " + sftp.InnerException);
                GatLogger.Instance.AddMessage("StackTrace: " + sftp.StackTrace);
                return false;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("actively refused"))
                    GatLogger.Instance.AddMessage("**** FTP error Check machine " + _host + " it may be inactive ******", LogMode.LogAndScreen);
                else
                    GatLogger.Instance.AddMessage("An exception occurred while attempting to reconnect on host " + _host + ":" + _user + " :\n" + ex.Message, LogMode.LogAndScreen);
                return false;
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
                GatLogger.Instance.AddMessage(string.Format("Sftp Exception encountered changing local directory: {0}", sftp.message), LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("InnerException: " + sftp.InnerException);
                GatLogger.Instance.AddMessage("StackTrace: " + sftp.StackTrace);
                return false;
            }
            catch (Exception ex)
            {
                GatLogger.Instance.AddMessage(string.Format("An error occurred while changing local directory:\n{0}", ex), LogMode.LogAndScreen);
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
                _sftpChannel.cd(remoteDirectoryPath);
            }
            catch (SftpException sftp)
            {
                GatLogger.Instance.AddMessage(string.Format("Sftp Exception encountered: {0}", sftp.message), LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("InnerException: " + sftp.InnerException);
                GatLogger.Instance.AddMessage("StackTrace: " + sftp.StackTrace);
                return false;
            }
            catch (Exception ex)
            {
                GatLogger.Instance.AddMessage(string.Format("Exception in Sftp encountered: {0}", ex.Message), LogMode.LogAndScreen);
                GatLogger.Instance.AddMessage("InnerException: " + ex.InnerException);
                GatLogger.Instance.AddMessage("StackTrace: " + ex.StackTrace);
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
