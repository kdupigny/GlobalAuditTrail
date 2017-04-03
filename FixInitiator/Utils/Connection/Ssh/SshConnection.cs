using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using GATUtils.Connection.FileTransfer;
using GATUtils.Logger;
using Tamir.SharpSsh.jsch;

namespace GATUtils.Connection.Ssh
{
    public class SshConnection
    {
        // username and password will be given via UserInfo interface.
        UserInfo _ui; // this is needed for the passkey
        Channel _channel;
        ChannelSftp _sftpChannel;

        //Stream _ins;
        //TextWriter _outs;
       
        /// <summary>
        /// Default Constructor
        /// </summary>
        public SshConnection()
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
        /// <param name="isFTP"></param>
        /// <param name="port"></param>
        public SshConnection(string ftPserver, string username, string password, string rootDir, int port = 22)
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
        public SshConnection(string ftPserver, string username, string password, int port = 22)
        {
            _host = ftPserver;
            _user = username;
            _pass = password;
            _port = port;
            
            _Init();
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
                    Console.WriteLine("Attempting to disconnect");
                    _sftpSession.disconnect();
                    _sftpSession = null;
                    _jsch = null;
                }
                catch (Exception e)
                {
                    _sftpSession = null;
                    _jsch = null;
                    Console.WriteLine("There was a failure while restarting connection " + e.Message);
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

                this.changeRemoteWorkingDirectory(_rootDir);
            }
            catch (Exception e)
            {
                if (!_reportOnce && _failCount >= 500)
                {
                    string failMsg = string.Format("SFTP Connection is failing ({2}@{0}:{1}) \n\t{3}", _host, _port,
                                                   _user, e.Message);
                    GatLogger.Instance.AddMessage(failMsg, LogMode.LogAndScreen);
                    GatLogger.Instance.AddMessage(string.Format("Inner Exception: {0}", e.InnerException));
                    GatLogger.Instance.AddMessage(string.Format("Exception Stack: {0}", e.StackTrace));
                    _reportOnce = true;
                }
                else
                {
                    _failCount++;
                }
            }
        }

        /// <summary>
        /// Shutdown the connection to the remote server
        /// </summary>
        public void disconnect()
        {
            if (_sftpSession.isConnected())
            {
                _sftpSession.disconnect();
                _channel.disconnect();
                _sftpChannel.disconnect();
                //Console.WriteLine("We successfully disconnected");
            }
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
                _sftpChannel.get(_rootDir + filePath, Directory.GetCurrentDirectory());               
            }
            catch (SftpException f)
            {
                //Console.WriteLine("Sftp TAMIR FAILED: " + f.message + "\n" + f.StackTrace);
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
                _sftpChannel.get(filePath , destinationFilePath);
            }
            catch (SftpException f)
            {
                //Console.WriteLine("Sftp TAMIR FAILED: " + f.message + "\n" + f.StackTrace);
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
        /// Deletes a remote file from a path starting from the root directory
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool RemoveFile(string filePath)
        {
            while (!_VertifyConnection())
            {
                Thread.Sleep(500);
            }

            try
            {                
                // Pull File in user's home dir using OVERWRITE mode
                _sftpChannel.rm(_rootDir + filePath);
            }
            catch (SftpException f)
            {
                Console.WriteLine("Sftp TAMIR FAILED: " + f.message + "\n" + f.StackTrace);
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
        /// Chnages the local working directory to the directory specified by 
        /// <para>the localDirectoryPath" parameter</para>
        /// </summary>
        /// <param name="localDirectoryPath">The path of the new working directory</param>
        /// <returns>true if the local working directory is changed or 
        /// <para>false is the working directory remains unchanged</para>
        /// </returns>
        public bool changeLocalWorkingDirectory(string localDirectoryPath)
        {
            try
            {
                _sftpChannel.lcd(localDirectoryPath);
            }
            catch (SftpException sftp)
            {
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while changing local directory:\n{0}", ex);
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
        public bool changeRemoteWorkingDirectory(string remoteDirectoryPath)
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
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DirectoryPath">The new directory to create starting from the root directory</param>
        /// <returns></returns>
        public bool createNewDirectory(string DirectoryPath)
        {
            while (!_VertifyConnection())
            {
                Thread.Sleep(500);
            }

            try
            {
                _sftpChannel.mkdir(_rootDir + DirectoryPath); 
            }
            catch (SftpException sftp)
            {
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while trying to create the directory:\n{0}", ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Removes a Directory
        /// </summary>
        /// <param name="directoryPath">The path of the directory to remove relative to the root directory</param>
        /// <returns>true if the directory was removed or false if the directory was not removed</returns>
        public bool removeDirectory(string directoryPath)
        {
            while (!_VertifyConnection())
            {
                Thread.Sleep(500);
            }

            try
            {
                _sftpChannel.rmdir(_rootDir + directoryPath);
            }
            catch (SftpException sftp)
            {
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception occurred while removing directory:\n" + ex.Message);
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Returns the list of contents in the directory specified by the "directoryPath"
        /// <para>parameter</para>
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        public ArrayList getRemoteDirectoryContentsList(string directoryPath)
        {
            while(!_VertifyConnection()) 
            {
                Thread.Sleep(500);
            }

            try
            {                
                return _sftpChannel.ls(_rootDir + directoryPath); 
            }
            catch (SftpException sftp)
            {
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception has occurred while Displaying directory contents:\n{0}", ex);
                return null;
            }            
        }

        /// <summary>
        /// Gets the current working Directory
        /// </summary>
        /// <returns></returns>
        public string getcurrentRemoteWorkingDirectoryPath()
        {
            if (!_VertifyConnection())
            {
                Thread.Sleep(500);
            }
            
            try
            {
                return _sftpChannel.pwd();  
            }
            catch (SftpException sftp)
            {
                return "";
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occurred while retriving working directory\n" + e);
                return "";
            }
        }

        /// <summary>
        /// Gets the current working directory
        /// </summary>
        /// <returns>The current local working directory path</returns>
        public string getcurrentLocalWorkingDirectoryPath()
        {
            return Directory.GetCurrentDirectory();
        }


        public List<IRemoteFile> GetWrkingDirContents(string dir = "")
        {
            ArrayList dirContents = _GetWorkingDirectoryContentsList();
            List<IRemoteFile> files = new List<IRemoteFile>();

            //pull appropriate files from the server
            foreach (Tamir.SharpSsh.jsch.ChannelSftp.LsEntry file in dirContents)
            {
                files.Add(new FtpFile(file));
            }

            return files;
        }

        /// <summary>
        /// Checks if a path exists
        /// </summary>
        /// <param name="path"></param>
        /// <returns>true if the specified path exists or false if the path does not exist</returns>
        public bool pathExist(string path)
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
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred while checking path existance\n" + e.Message +
                    "\n" + e.StackTrace);
                return false;
            }
        }

        /// <summary>
        /// Attempts to re-establish a connection with a remote server
        /// </summary>
        /// <returns>true if re-connection succeded or false if re-connection did not succed</returns>
        private bool _Reconnect()
        {
            try
            {
                if (!_sftpSession.isConnected())
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

                    this.changeRemoteWorkingDirectory(_rootDir);
                    return true;
                }
            }
            catch (SftpException sftp)
            {
                //Sftp Connection
                return false;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("actively refused"))
                    Console.WriteLine("**** FTP error Check machine " + _host + " it may be inactive ******");
                else 
                    Console.WriteLine("An exception occurred while attempting to reconnect on host " + _host + ":" + _user + " :\n" + ex.Message);
                return false;
            }
            return false;
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
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception occurred while attempting to list directory contents:\n" + e);
                return null;
            }
        }

        JSch _jsch = null;

        private bool _reportOnce = false;
        private int _failCount = 0;

        private string _host;
        private string _user;
        private string _pass;
        private string _rootDir;

        private readonly int _port = 22;
        Session _sftpSession;

        
    }
}
