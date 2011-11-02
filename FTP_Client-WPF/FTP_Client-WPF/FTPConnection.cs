using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace FTP_Client_WPF
{
    class FTPConnection
    {
        private String m_ftpUrl;
        private String m_ftpUserName;
        private String m_ftpUserPassword;

        public void FTPConnection(String url_val, String username_val, String password_val)
        {
            m_ftpUrl = url_val;
            m_ftpUserName = username_val;
            m_ftpUserPassword = password_val;
        }

        public void Login()
        {

        }

        public void ListContents()
        {

        }

        public void Download()
        {

        }

        public void Upload()
        {

        }

        public void Delete()
        {

        }

        public void Rename()
        {

        }

        public void CreateDirectory()
        {

        }
    }
}
