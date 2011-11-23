using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;

namespace FTP_Client_WPF
{

    public class DirectoryDetailedView
    {
        public DateTime CreateTime;
        public string Flags;
        public string Group;
        public bool IsDirectory;
        public string Name;
        public string Owner;
        public Int64 Size;
    }

    public class FTPConnection
    {
        private const String FORWARD_SLASH = "/";


        private String m_ftpServer;
        private int m_ftpPort = 21;
        
        private String m_ftpUserName;
        private String m_ftpUserPassword;
        private NetworkCredential creds;

        private String m_currentWorkingDir = FORWARD_SLASH;

        private Encoding m_encode;

        public FTPConnection()
        {
            m_encode = System.Text.Encoding.GetEncoding("utf-8");
        }


        /// <summary>
        /// Getter / Setter for m_ftpServer
        /// </summary>
        public String FtpServer
        {
            get { return m_ftpServer; }
            set { m_ftpServer = value; }
        }


        /// <summary>
        /// Getter / Setter for m_ftpPort
        /// </summary>
        public int FtpPort
        {
            get { return m_ftpPort; }
            set { m_ftpPort = value; }
        }


        /// <summary>
        /// Getter - returns if the current working directory is root ("/")
        /// </summary>
        public bool IsRootDirectory
        {
            get { return m_currentWorkingDir == FORWARD_SLASH; }
        }


        /// <summary>
        /// Sets the ftp login details and assigns them in a 'Networkcredendtial' object
        /// </summary>
        /// <param name="username_val">m_ftpUserName</param>
        /// <param name="password_val">m_ftpUserPassword</param>
        public void SetCredentials(String username_val, String password_val)
        {
            m_ftpUserName = username_val;
            m_ftpUserPassword = password_val;
            creds = new NetworkCredential(m_ftpUserName, m_ftpUserPassword);
        }


        /// <summary>
        /// Getter / Setter for m_currentWorkingDir
        /// </summary>
        public string CurrentDirectory
        {
            get { return m_currentWorkingDir; }
            set { m_currentWorkingDir = _setCurrentDirectory(value); }
        }


        /// <summary>
        /// Makes a request to the FTP server for a detailed directory view
        /// </summary>
        /// <returns>List containing each file/directory details in the selected directory</returns>
        public List<DirectoryDetailedView> ListDirectoryContents()
        {
            Debug.WriteLine("[{0}::{1}] BUSY", ClassFullName, MethodBase.GetCurrentMethod().Name);

            // Initialize new 'FtpWebRequest' object and assign method
            FtpWebRequest request = _initializeNewWebRequest();
            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            String dirdetails = string.Empty;
            try
            {
                Debug.WriteLine("[{0}::{1}] {2}", ClassFullName, MethodBase.GetCurrentMethod().Name, "Response stream request");
                // Making response stream request
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();

                // Obtain a 'Stream' object associated with the response object.
                Stream ReceiveStream = response.GetResponseStream();
                // Send the 'Stream' object to a StreamReader with proper encoding
                StreamReader readStream = new StreamReader(ReceiveStream, m_encode);

                Debug.WriteLine("[{0}::{1}] {2}", ClassFullName, MethodBase.GetCurrentMethod().Name, "Response stream received. Reading it");
                // reading response stream
                dirdetails = readStream.ReadToEnd();
                response.Close();
            }
            catch (WebException e)
            {
                Debug.WriteLine("[{0}::{1}] ERROR: {2}", ClassFullName, MethodBase.GetCurrentMethod().Name, e.ToString());
            }

            Debug.WriteLine("[{0}::{1}] {2}", ClassFullName, MethodBase.GetCurrentMethod().Name, "Parsing response stream");
            // parse dirdetails if not empty 
            List<DirectoryDetailedView> parsed_dirdetails = null;
            if (dirdetails != string.Empty)
            {
                parsed_dirdetails = _parseDirectoryDetailedView(dirdetails);
            }

            Debug.WriteLine("[{0}::{1}] DONE", ClassFullName, MethodBase.GetCurrentMethod().Name);
            return parsed_dirdetails;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public void Download()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public void Upload()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public void Delete()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public void Rename()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name=""></param>
        /// <returns></returns>
        public void CreateDirectory()
        {
            throw new NotImplementedException();
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // PRIVATE Functions
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Returns current class name for use in Debug output
        /// </summary>
        private string ClassFullName
        {
            get { return this.GetType().Name; }
        }

        /// <summary>
        /// Initializes a 'FtpWebRequest' object in the path given by path_val with creds as 'Credentials'
        /// </summary>
        /// <param name="path_val">path where the new connection will be opened</param>
        /// <returns>'FtpWebRequest' object</returns>
        private FtpWebRequest _initializeNewWebRequest(String path_val = "")
        {
            Debug.WriteLine("[{0}::{1}] BUSY", ClassFullName, MethodBase.GetCurrentMethod().Name);

            Debug.WriteLine("[{0}::{1}] creating request for: {2}", ClassFullName, MethodBase.GetCurrentMethod().Name, _getUrl(path_val));
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(_getUrl(path_val));
            request.Credentials = creds;
            request.KeepAlive = false;

            Debug.WriteLine("[{0}::{1}] DONE", ClassFullName, MethodBase.GetCurrentMethod().Name);
            return request;
        }


        /// <summary>
        /// Parses the directory detailed view
        /// </summary>
        /// <param name="dirlist">String containing the 'WebRequestMethods.Ftp.ListDirectoryDetails' output</param>
        /// <returns>List containing each file/directory details in the selected directory</returns>
        private List<DirectoryDetailedView> _parseDirectoryDetailedView(String dirlist)
        {
            List<DirectoryDetailedView> entries = new List<DirectoryDetailedView>();
            string[] lines = dirlist.Split('\n');

            foreach (string line in lines)
            {
                if (line.Length > 0)
                {
                    DirectoryDetailedView entry = _parseUnixDirectoryListing(line);
                    if (entry.Name != "." && entry.Name != "..")
                        entries.Add(entry);
                }
            }
            return entries;
        }

        /// <summary>
        /// Parses a line from a UNIX-format listing
        /// </summary>
        /// <param name="text">text to be parsed (as: dr-xr-xr-x   1 owner    group               216132 Nov 25  2011 dir)</param>
        /// <returns>'DirectoryDetailedView' object</returns>
        private DirectoryDetailedView _parseUnixDirectoryListing(string text)
        {
            DirectoryDetailedView entry = new DirectoryDetailedView();
            string processstr = text.Trim();
            entry.Flags = processstr.Substring(0, 9);
            entry.IsDirectory = (entry.Flags[0] == 'd');
            processstr = (processstr.Substring(11)).Trim();
            _cutSubstringWithTrim(ref processstr, ' ', 0);   //skip 1 part
            entry.Owner = _cutSubstringWithTrim(ref processstr, ' ', 0);
            entry.Group = _cutSubstringWithTrim(ref processstr, ' ', 0);
            entry.Size = Int64.Parse(_cutSubstringWithTrim(ref processstr, ' ', 0));
            entry.CreateTime = DateTime.Parse(_cutSubstringWithTrim(ref processstr, ' ', 8));
            entry.Name = processstr;   //Rest of the part is name
            return entry;
        }

        /// <summary>
        /// Removes the token ending in the specified character
        /// </summary>
        /// <param name="s">string reference</param>
        /// <param name="c">char to be searched in 's' at which the string will be splitted</param>
        /// <param name="startIndex">from where the search will start</param>
        /// <returns>removed string</returns>
        private string _cutSubstringWithTrim(ref string s, char c, int startIndex)
        {
            int pos = s.IndexOf(c, startIndex);
            if (pos < 0) 
                pos = s.Length;
            string retString = s.Substring(0, pos);
            s = (s.Substring(pos)).Trim();
            return retString;
        }


        /// <summary>
        /// Sets the given directory to the current directory based on the following:
        /// * If the directory name is "..", the current directory goes up one level
        /// * If the directory name starts with "/", it replaces the current directory path
        /// * If the directory name doesn't start with any of ".." and "/", the new directory relative to the current one
        /// </summary>
        /// <param name="dir_val">The directory to set</param>
        /// <returns>The new current working directory</returns>
        private string _setCurrentDirectory(string dir_val)
        {
            // remove white spaces at the start and end of string
            dir_val = dir_val.Trim();

            if (dir_val == "..")
            {
                // the current directory goes up one level
                int pos = m_currentWorkingDir.LastIndexOf(FORWARD_SLASH);
                if (pos <= 0) 
                    return FORWARD_SLASH;
                else
                    return m_currentWorkingDir.Substring(0, pos);
            }
            else if (dir_val.StartsWith(FORWARD_SLASH))
            {
                // replaces current directory path
                return dir_val;
            }
            else
            {
                // relative to current directory
                if (m_currentWorkingDir == FORWARD_SLASH)
                    return m_currentWorkingDir + dir_val;
                else
                    return m_currentWorkingDir + FORWARD_SLASH + dir_val;
            }
        }

        /// <summary>
        /// Returns the domain, current path and specified directory as a URL.
        /// </summary>
        /// <param name="path_val">Partial directory or filename applied to the current working directory.</param>
        private string _getUrl(string path_val)
        {
            if (path_val.Length == 0)
                return String.Format("ftp://{0}:{1}{2}", m_ftpServer, m_ftpPort, m_currentWorkingDir);
            else
                return String.Format("ftp://{0}:{1}{2}/{3}", m_ftpServer, m_ftpPort, m_currentWorkingDir, path_val);
        }
    }
}
