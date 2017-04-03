using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FtpLib;

using GATUtils.Logger;

namespace GATUtils.Connection.FileTransfer
{
    public class Ftp : IFtpHandle
    {
        public Ftp()
        {

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

        public Ftp(string ftPserver, string userName, string password, int port = 21)
        {
            try
            {
                _host = ftPserver;
                _user = userName;
                _pass = password;

                _port = port;
                _reconnectionAttempts = 0;
                _Init();
            }
            catch (Exception e)
            {
                //Throw any exceptions that occur during intialization
                throw e;
            }
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
                GatLogger.Instance.AddMessage("Ftp Session {0} connected successfully");
                return true;
            }
            catch (Exception e)
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
                        //Recieve status of server
                        string status = "N/A"; /*string.Format("Current local directory: \"{0}\"\n" +
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
        
        public bool IsConnected
        {
            //STATUS: Not working yet     
            get
            {
                if (_ftpSession == null)
                {
                    return false;
                }
                else
                {
                    try
                    {
                        #region Temporary Workaround
                        //Send a dummy command to test server connection
                        //If an exception is thrown, we are not connected
                        //virtually all servers support help commands
                        string result = _ftpSession.SendCommand("help");
                        return true;
                        #endregion
                        
                       
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine("An exception occurred:\n{0}", e.Message);
                        return false;
                    }
                }
            }
        }
        
        public string CurrentLocalDirectory
        {
            get
            {
                return Directory.GetCurrentDirectory();
            }
        }

        public FtpStatus ConnectionStatus
        {
            get { throw new NotImplementedException(); }
        }

        public string RemoteWorkingDirectory
        {
            get { throw new NotImplementedException(); }
        }

        public string LocalWorkingDirectory
        {
            get { throw new NotImplementedException(); }
        }

        public bool Connect()
        {
            throw new NotImplementedException();
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
                GatLogger.Instance.AddMessage(string.Format("An exception occurred.\n{0}", e.Message), LogMode.LogAndScreen);
            }
            catch (Exception e)
            {
                GatLogger.Instance.AddMessage(string.Format("An exception has occurred:\n{0}", e.Message), LogMode.LogAndScreen);
                
            }

            return true;
        }

        public bool ChangeRemoteWorkingDirectory(string remotePath)
        {
            return _SetRemoteWorkingDirectory(remotePath);
        }

        public bool ChangeLocalWorkingDirectory(string remotePath)
        {
            return _SetLocalWorkingDirectory(remotePath);
        }

        public bool Push(string filePath)
        {
            //if (utils.inDebug)
            //{
            //    Console.WriteLine("Application in Debug Mode file: " + filePath + " has not been sent.");
            //    return true;
            //}
            try
            {
                if (_ftpSession != null)
                {
                    _ftpSession.PutFile(filePath);
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

        public bool Push(string filePath, string destinationPath)
        {
            //if (utils.inDebug)
            //{
            //    Console.WriteLine("Application in Debug Mode file: " + filePath + " has not been sent.");
            //    return true;
            //}

            //Have a problem with this function
            string oldDir = "";
            try
            {
                if (_ftpSession != null)
                {
                    #region Temporary Workaround
                    oldDir = _ftpSession.GetCurrentDirectory();
                    _ftpSession.SetCurrentDirectory(destinationPath);
                    _ftpSession.PutFile(filePath);
                    #endregion
                    //_session.PutFile(filePath, destinationPath);

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
            finally
            {
                _ftpSession.SetCurrentDirectory(oldDir);
            }
            return true;
        }

        /// <summary>
        /// Saves file to application Root directory on Local drive
        /// </summary>
        /// <param name="remoteFilename"></param>
        /// <returns></returns>
        public bool Pull(string remoteFilename)
        {
            return _Pull(remoteFilename, true);
        }

        public bool Pull(string filename, string destination)
        {
            return _Pull(filename, destination, false);
        }

        private bool _Pull(string remoteFilePath, bool overWriteExistingFile = true)
        {
            try
            {
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
                if (_ftpSession != null)
                {
                    
                    _ftpSession.GetFile(remoteFilePath, localDestinationFilePath + "\\" + remoteFilePath, overWriteExistingFile);
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

        public string GetRemoteWorkingDirectory()
        {
            try
            {
                return _ftpSession != null ? _ftpSession.GetCurrentDirectory() : "";
            }
            catch (Exception e)
            {
                GatLogger.Instance.AddMessage(string.Format("An exception has occurred obtaining remote directory path:\n{0}", e.Message), LogMode.LogAndScreen);
                return "";
            }
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
                else
                {
                    return false;
                }
            }
            catch (Exception e)
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
                GatLogger.Instance.AddMessage(string.Format("Exception thrown when checking for file on ftp. \n\t{0}", e.Message), LogMode.LogAndScreen);
                return false;
            }
        }

        public List<IRemoteFile> GetWorkingDirContents(string dir = "")
        {
            //string oldRemoteDirectory = "";
            List<IRemoteFile> remoteFiles = new List<IRemoteFile>();
            try
            {
                if (_ftpSession != null)
                {
                    //oldRemoteDirectory = _session.GetCurrentDirectory();
                    //SetRemoteWorkingDirectory("");
                    FtpFileInfo[] files = _ftpSession.GetFiles();

                    remoteFiles.AddRange(files.Select(file => new FtpFile(file)).Cast<IRemoteFile>());
                    return remoteFiles;
                }
                return new List<IRemoteFile>();
            }
            catch (FtpException e)
            {
                Console.WriteLine("An exception occurred:\n{0}", e.Message);
                return new List<IRemoteFile>();
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception has occurred:\n{0}", e.Message);
                return new List<IRemoteFile>();
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
                GatLogger.Instance.AddMessage(string.Format("Could not connect to ftp after {0} tries.", ct_maxPossibleAttempts),
                                              LogMode.LogAndScreen);
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
                GatLogger.Instance.AddMessage(string.Format("An ftp exception occurred setting active local directory:\n{0}", e.Message), LogMode.LogAndScreen);
                return false;
            }
            catch (Exception e)
            {
                GatLogger.Instance.AddMessage(string.Format("An exception has occurred setting active local directory:\n{0}", e.Message), LogMode.LogAndScreen);
                return false;
            }
        }

        private bool _SetRemoteWorkingDirectory(string newRemoteDirectoryPath)
        {
            string oldRemoteDirectory = "";
            try
            {
                if (_ftpSession != null)
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
                GatLogger.Instance.AddMessage(string.Format("An exception occurred:\n{0}", e.Message), LogMode.LogAndScreen);
                return false;
            }
            catch (Exception e)
            {
                if (_ftpSession != null) _ftpSession.SetCurrentDirectory(oldRemoteDirectory);
                GatLogger.Instance.AddMessage(string.Format("An exception has occurred:\n{0}", e.Message), LogMode.LogAndScreen);
                return false;
            }
        }
        
        private readonly string _host;
        private readonly int _port;

        private readonly string _user;
        private readonly string _pass;

        private int _reconnectionAttempts;
        private const int ct_maxPossibleAttempts = 3;
        private FtpConnection _ftpSession;

        private string _localPath = "/";
    }
}
