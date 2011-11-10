using FTP_Client_WPF;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Testing
{
    /// <summary>
    /// Summary description for Test_FTPConnection
    /// </summary>
    [TestClass]
    public class Test_FTPConnection
    {
        private FTPConnection m_ftpConn;

        public Test_FTPConnection()
        {
            m_ftpConn = new FTPConnection();
            m_ftpConn.SetCredentials("mirhciulica", "test");
            m_ftpConn.FtpServer = "127.0.0.1";
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestListDirectoryContents()
        {
            m_ftpConn.ListDirectoryContents();
        }
    }
}
