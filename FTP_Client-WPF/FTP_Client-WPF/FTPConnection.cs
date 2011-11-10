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
        private const String ROOT_PATH = "/";

        private String m_ftpServer;
        private String m_ftpPath = "/";
        private int m_ftpPort = 21;
        
        private String m_ftpUserName;
        private String m_ftpUserPassword;
        private NetworkCredential creds;

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
        private FtpWebRequest InitializeNewWebRequest(String path_val = ROOT_PATH)
        {
            Debug.WriteLine("[{0}::{1}] BUSY", ClassFullName, MethodBase.GetCurrentMethod().Name);
            String ftpfullpath = String.Format("ftp://{0}:{1}{2}", m_ftpServer, m_ftpPort, path_val);

            Debug.WriteLine("[{0}::{1}] creating request for: {2}", ClassFullName, MethodBase.GetCurrentMethod().Name, ftpfullpath);
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpfullpath);
            request.Credentials = creds;

            Debug.WriteLine("[{0}::{1}] DONE", ClassFullName, MethodBase.GetCurrentMethod().Name);
            return request;
        }

        /// <summary>
        /// Makes a request to the FTP server for a detailed directory view
        /// </summary>
        /// <returns>List containing each file/directory details in the selected directory</returns>
        public List<DirectoryDetailedView> ListDirectoryContents()
        {
            Debug.WriteLine("[{0}::{1}] BUSY", ClassFullName, MethodBase.GetCurrentMethod().Name);

            // Initialize new 'FtpWebRequest' object and assign method
            FtpWebRequest request = InitializeNewWebRequest();
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
                parsed_dirdetails = ParseDirectoryDetailedView(dirdetails);
            }

            Debug.WriteLine("[{0}::{1}] DONE", ClassFullName, MethodBase.GetCurrentMethod().Name);
            return parsed_dirdetails;
        }


        /// <summary>
        /// Parses the directory detailed view
        /// </summary>
        /// <param name="dirlist">String containing the 'WebRequestMethods.Ftp.ListDirectoryDetails' output</param>
        /// <returns>List containing each file/directory details in the selected directory</returns>
        private List<DirectoryDetailedView> ParseDirectoryDetailedView(String dirlist)
        {
            List<DirectoryDetailedView> entries = new List<DirectoryDetailedView>();
            string[] lines = dirlist.Split('\n');

            foreach (string line in lines)
            {
                if (line.Length > 0)
                {
                    DirectoryDetailedView entry = ParseUnixDirectoryListing(line);
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
        private DirectoryDetailedView ParseUnixDirectoryListing(string text)
        {
            DirectoryDetailedView entry = new DirectoryDetailedView();
            string processstr = text.Trim();
            entry.Flags = processstr.Substring(0, 9);
            entry.IsDirectory = (entry.Flags[0] == 'd');
            processstr = (processstr.Substring(11)).Trim();
            CutSubstringWithTrim(ref processstr, ' ', 0);   //skip 1 part
            entry.Owner = CutSubstringWithTrim(ref processstr, ' ', 0);
            entry.Group = CutSubstringWithTrim(ref processstr, ' ', 0);
            entry.Size = Int64.Parse(CutSubstringWithTrim(ref processstr, ' ', 0));
            entry.CreateTime = DateTime.Parse(CutSubstringWithTrim(ref processstr, ' ', 8));
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
        private string CutSubstringWithTrim(ref string s, char c, int startIndex)
        {
            int pos = s.IndexOf(c, startIndex);
            if (pos < 0) 
                pos = s.Length;
            string retString = s.Substring(0, pos);
            s = (s.Substring(pos)).Trim();
            return retString;
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
