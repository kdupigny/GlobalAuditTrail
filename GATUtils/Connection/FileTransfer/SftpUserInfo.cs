using System;
using Tamir.SharpSsh.jsch;

namespace GATUtils.Connection.FileTransfer
{
    class SftpUserInfo : UserInfo
    {
        public String getPassword() { return _passwd; }
        public bool promptYesNo(String str) { return true; }

        public String getPassphrase() { return null; }
        public bool promptPassphrase(String message) { return true; }
        public bool promptPassword(String message)
        {
            if (_firstTime)
            {
                _firstTime = false;
                _passwd = "";
                return true;
            }
            return false;
        }

        public void showMessage(String message) { }
        
        private String _passwd;
        bool _firstTime = true;
    }
}