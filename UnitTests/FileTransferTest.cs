using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using GATInterface.Forms.PositionData;
using GATUtils.Connection.FileTransfer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using GATUtils.Utilities.FileUtils;

namespace UnitTests
{
    [TestClass]
    public class FileTransferTest
    {
        [TestMethod]
        public void FtpConnectionTest()
        {
            IFtpHandle ftpConnection = new Ftp("54.245.114.32", "DailyFiles", "d@yDump!", 21, "/Temp", "ConnectionTest");
            Assert.AreEqual((int)ftpConnection.ConnectionStatus, (int)FtpStatus.Connected);

            List<IRemoteFile> files = ftpConnection.GetWorkingDirContents();
            string localPath = ftpConnection.LocalWorkingDirectory;
            string remotePath = ftpConnection.RemoteWorkingDirectory;

            ftpConnection.Pull(files.First().Filename);
            string[] localFiles = Directory.GetFiles(localPath);
            Assert.AreNotEqual(0, localFiles.Count());
            Assert.AreEqual(files.First().Filename, Path.GetFileName(localFiles[0]));

            ftpConnection.ChangeRemoteWorkingDirectory("Archive");
            Assert.AreNotEqual(remotePath, ftpConnection.RemoteWorkingDirectory);
            files = ftpConnection.GetWorkingDirContents();
            ftpConnection.Pull(files.First().Filename);
            localFiles = Directory.GetFiles(localPath);
            Assert.AreEqual(2, localFiles.Count());

            ftpConnection.ChangeLocalWorkingDirectory(GatFile.Path(Dir.Data, string.Empty));
            localPath = ftpConnection.LocalWorkingDirectory;
            ftpConnection.Pull(files.First().Filename);
            localFiles = Directory.GetFiles(localPath);
            Assert.AreNotEqual(0, localFiles.Count());
            
            localPath = GatFile.Path(Dir.Root, string.Empty);
            ftpConnection.Pull(files.First().Filename, localPath);
            localFiles = Directory.GetFiles(localPath);
            Assert.AreNotEqual(0, localFiles.Count());

            ftpConnection.ResetToRootDirectory();
            Assert.AreEqual(remotePath, ftpConnection.RemoteWorkingDirectory);

            files = ftpConnection.GetWorkingDirContents();
            foreach (var file in files)
            {
                ftpConnection.Pull(file.Filename);
                ftpConnection.Push(Path.Combine(ftpConnection.LocalWorkingDirectory, file.Filename), "Archive");
            }
        }

        [TestMethod]
        public void SftpConnectionTest()
        {
            IFtpHandle ftpConnection = new Sftp("96.126.108.162", "elliot", "e258G741", 22, "/home/elliot/sub2", "ConnectionTest");
            //new Ftp("54.245.114.32", "DailyFiles", "d@yDump!", 21, "/Temp", "ConnectionTest");
            Assert.AreEqual((int)ftpConnection.ConnectionStatus, (int)FtpStatus.Connected);

            List<IRemoteFile> files = ftpConnection.GetWorkingDirContents();
            string localPath = ftpConnection.LocalWorkingDirectory;
            string remotePath = ftpConnection.RemoteWorkingDirectory;

            ftpConnection.Pull(files.First().Filename);
            string[] localFiles = Directory.GetFiles(localPath);
            Assert.AreNotEqual(0, localFiles.Count());
            //Assert.IsTrue(files.Any(file => file.Filename.Equals(Path.GetFileName(localFiles[0]))));

            ftpConnection.ChangeRemoteWorkingDirectory("folder");
            Assert.AreNotEqual(remotePath, ftpConnection.RemoteWorkingDirectory);
            files = ftpConnection.GetWorkingDirContents();
            Assert.AreEqual(0, files.Count());
            ftpConnection.Push(localFiles.First());
            files = ftpConnection.GetWorkingDirContents();

            ftpConnection.ChangeLocalWorkingDirectory(GatFile.Path(Dir.Data, string.Empty));
            localPath = ftpConnection.LocalWorkingDirectory;
            ftpConnection.Pull(files.First().Filename);
            localFiles = Directory.GetFiles(localPath);
            Assert.AreNotEqual(0, localFiles.Count());

            localPath = GatFile.Path(Dir.Root, string.Empty);
            ftpConnection.Pull(files.First().Filename, localPath);
            localFiles = Directory.GetFiles(localPath);
            Assert.AreNotEqual(0, localFiles.Count());

            ftpConnection.ResetToRootDirectory();
            Assert.AreEqual(remotePath, ftpConnection.RemoteWorkingDirectory);

            files = ftpConnection.GetWorkingDirContents();
            foreach (var file in files)
            {
                ftpConnection.Pull(file.Filename);
                ftpConnection.Push(Path.Combine(ftpConnection.LocalWorkingDirectory, file.Filename), "folder");
            }
        }

        [TestMethod]
        public void PositionRetrievalTest()
        {
            Dictionary<string, OvernightPositionObject> result =
                OvernightPositionManager.Instance.GetAccountPositions("71101");
            Assert.IsNotNull(result);
        }
    }
}
