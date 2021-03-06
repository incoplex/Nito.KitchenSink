﻿// <copyright file="FtpConnection.cs" company="Nito Programs">
//     Copyright (c) 2010 Nito Programs.
// </copyright>

namespace Nito.KitchenSink.WinInet
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A connection to an FTP server. This class has thread affinity except for the <see cref="Dispose"/> method, which may be called by any thread. Other threads may call <see cref="Dispose"/> to cancel long-running operations.
    /// </summary>
    public sealed class FtpConnection : IDisposable
    {
        /// <summary>
        /// The current working directory on the remote FTP server, if known. May be <c>null</c>.
        /// </summary>
        private string currentDirectory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnection"/> class.
        /// </summary>
        /// <param name="process">The name of the process or component making use of this FTP connection (used for logging).</param>
        public FtpConnection(string process)
        {
            this.InternetHandle = new InternetOpenHandle(process);
        }

        /// <summary>
        /// Occurs when progress is made on a requested operation. This delegate may be invoked from within the methods on this class.
        /// </summary>
        public event Action<InternetCallbackEventArgs> Progress;

        /// <summary>
        /// File transfer options.
        /// </summary>
        public enum FileTransferType : int
        {
            /// <summary>
            /// Transfers file as ASCII.
            /// </summary>
            Ascii = 1,

            /// <summary>
            /// Transfers file as binary.
            /// </summary>
            Binary = 2,
        }

        /// <summary>
        /// Gets the underlying internet handle.
        /// </summary>
        public InternetOpenHandle InternetHandle { get; private set; }

        /// <summary>
        /// Gets the underlying FTP handle. Returns <c>null</c> until <see cref="O:Nito.KitchenSink.WinInet.FtpConnection.Connect"/> is invoked.
        /// </summary>
        public FtpHandle FtpHandle { get; private set; }

        /// <summary>
        /// Gets or sets the current directory on the remote FTP server. This may be set to an absolute or relative directory. <see cref="O:Nito.KitchenSink.WinInet.FtpConnection.Connect"/> must be called before reading or writing this property.
        /// </summary>
        public string CurrentDirectory
        {
            get
            {
                if (this.currentDirectory == null)
                {
                    this.currentDirectory = this.FtpHandle.GetCurrentDirectory();
                }

                return this.currentDirectory;
            }

            set
            {
                this.FtpHandle.SetCurrentDirectory(value);
                this.currentDirectory = null;
            }
        }

        /// <summary>
        /// Connects to the specified FTP server.
        /// </summary>
        /// <param name="serverName">Name of the server to which to connect.</param>
        /// <param name="serverPort">The server port to which to connect.</param>
        /// <param name="username">The username to use for authentication.</param>
        /// <param name="password">The password to use for authentication.</param>
        /// <param name="internetConnectFlags">The connection flags, such as <see cref="InternetConnectHandle.Flags.Passive"/> for passive FTP.</param>
        public void Connect(string serverName, int serverPort, string username, string password, InternetConnectHandle.Flags internetConnectFlags)
        {
            this.FtpHandle = new FtpHandle(this.InternetHandle, serverName, serverPort, username, password, internetConnectFlags);
            this.FtpHandle.StatusCallback = (args) =>
            {
                if (this.Progress != null)
                {
                    this.Progress(args);
                }
            };
        }

        /// <summary>
        /// Connects to the specified FTP server.
        /// </summary>
        /// <param name="serverName">Name of the server to which to connect.</param>
        /// <param name="serverPort">The server port to which to connect.</param>
        /// <param name="username">The username to use for authentication.</param>
        /// <param name="password">The password to use for authentication.</param>
        public void Connect(string serverName, int serverPort, string username, string password)
        {
            this.Connect(serverName, serverPort, username, password, InternetConnectHandle.Flags.None);
        }

        /// <summary>
        /// Connects to the specified FTP server.
        /// </summary>
        /// <param name="serverName">Name of the server to which to connect.</param>
        /// <param name="username">The username to use for authentication.</param>
        /// <param name="password">The password to use for authentication.</param>
        /// <param name="internetConnectFlags">The connection flags, such as <see cref="InternetConnectHandle.Flags.Passive"/> for passive FTP.</param>
        public void Connect(string serverName, string username, string password, InternetConnectHandle.Flags internetConnectFlags)
        {
            this.Connect(serverName, 0, username, password, internetConnectFlags);
        }

        /// <summary>
        /// Connects to the specified FTP server.
        /// </summary>
        /// <param name="serverName">Name of the server to which to connect.</param>
        /// <param name="username">The username to use for authentication.</param>
        /// <param name="password">The password to use for authentication.</param>
        public void Connect(string serverName, string username, string password)
        {
            this.Connect(serverName, 0, username, password, InternetConnectHandle.Flags.None);
        }

        /// <summary>
        /// Connects to the specified FTP server.
        /// </summary>
        /// <param name="serverName">Name of the server to which to connect.</param>
        /// <param name="internetConnectFlags">The connection flags, such as <see cref="InternetConnectHandle.Flags.Passive"/> for passive FTP.</param>
        public void Connect(string serverName, InternetConnectHandle.Flags internetConnectFlags)
        {
            this.Connect(serverName, 0, null, null, internetConnectFlags);
        }

        /// <summary>
        /// Connects to the specified FTP server.
        /// </summary>
        /// <param name="serverName">Name of the server to which to connect.</param>
        public void Connect(string serverName)
        {
            this.Connect(serverName, 0, null, null, InternetConnectHandle.Flags.None);
        }

        /// <summary>
        /// Connects to the specified FTP server.
        /// </summary>
        /// <param name="serverName">Name of the server to which to connect.</param>
        /// <param name="serverPort">The server port to which to connect.</param>
        /// <param name="internetConnectFlags">The connection flags, such as <see cref="InternetConnectHandle.Flags.Passive"/> for passive FTP.</param>
        public void Connect(string serverName, int serverPort, InternetConnectHandle.Flags internetConnectFlags)
        {
            this.Connect(serverName, serverPort, null, null, internetConnectFlags);
        }

        /// <summary>
        /// Connects to the specified FTP server.
        /// </summary>
        /// <param name="serverName">Name of the server to which to connect.</param>
        /// <param name="serverPort">The server port to which to connect.</param>
        public void Connect(string serverName, int serverPort)
        {
            this.Connect(serverName, serverPort, null, null, InternetConnectHandle.Flags.None);
        }

        /// <summary>
        /// Creates the specified directory on the remote FTP server. <see cref="O:Nito.KitchenSink.WinInet.FtpConnection.Connect"/> must be called before invoking this method.
        /// </summary>
        /// <param name="directory">The directory to create.</param>
        public void CreateDirectory(string directory)
        {
            this.FtpHandle.CreateDirectory(directory);
        }

        /// <summary>
        /// Deletes the specified file on the remote FTP server. <see cref="O:Nito.KitchenSink.WinInet.FtpConnection.Connect"/> must be called before invoking this method.
        /// </summary>
        /// <param name="fileName">The file to delete.</param>
        public void DeleteFile(string fileName)
        {
            this.FtpHandle.DeleteFile(fileName);
        }

        /// <summary>
        /// Downloads the specified remote file from the FTP server, saving it at a local path and filename. <see cref="O:Nito.KitchenSink.WinInet.FtpConnection.Connect"/> must be called before invoking this method.
        /// </summary>
        /// <param name="remoteFile">The remote file to download.</param>
        /// <param name="localFile">The local path and filename to which to save the file.</param>
        /// <param name="failIfExists">Whether to fail if the local file specified by <paramref name="localFile"/> already exists.</param>
        /// <param name="type">The type of file to transfer.</param>
        public void GetFile(string remoteFile, string localFile, bool failIfExists, FileTransferType type)
        {
            this.FtpHandle.GetFile(remoteFile, localFile, failIfExists, (FtpHandle.GetFileFlags)type);
        }

        /// <summary>
        /// Uploads the specified local file to the FTP server, saving it at a remote path and filename. <see cref="O:Nito.KitchenSink.WinInet.FtpConnection.Connect"/> must be called before invoking this method.
        /// </summary>
        /// <param name="localFile">The local file to upload.</param>
        /// <param name="remoteFile">The remote path and filename to which to save the file.</param>
        /// <param name="type">The type of file to transfer.</param>
        public void PutFile(string localFile, string remoteFile, FileTransferType type)
        {
            this.FtpHandle.PutFile(localFile, remoteFile, (FtpHandle.PutFileFlags)type);
        }

        /// <summary>
        /// Removes the specified directory from the remote FTP server. <see cref="O:Nito.KitchenSink.WinInet.FtpConnection.Connect"/> must be called before invoking this method.
        /// </summary>
        /// <param name="directory">The directory to remove.</param>
        public void RemoveDirectory(string directory)
        {
            this.FtpHandle.RemoveDirectory(directory);
        }

        /// <summary>
        /// Renames the specified file on the FTP server. <see cref="O:Nito.KitchenSink.WinInet.FtpConnection.Connect"/> must be called before invoking this method.
        /// </summary>
        /// <param name="oldName">The old file name.</param>
        /// <param name="newName">The new file name.</param>
        public void RenameFile(string oldName, string newName)
        {
            this.FtpHandle.RenameFile(oldName, newName);
        }

        /// <summary>
        /// Finds matching files on the remote FTP server. <see cref="O:Nito.KitchenSink.WinInet.FtpConnection.Connect"/> must be called before invoking this method.
        /// </summary>
        /// <param name="search">The search string, which may include wildcards and/or directory information.</param>
        /// <returns>All files matching the query on the remote FTP server.</returns>
        public IList<FtpDirectoryEntry> FindFiles(string search)
        {
            return this.FtpHandle.FindFiles(search);
        }

        /// <summary>
        /// Retrieves a directory listing of all files from the current working directory on the remote FTP server. <see cref="O:Nito.KitchenSink.WinInet.FtpConnection.Connect"/> must be called before invoking this method.
        /// </summary>
        /// <returns>All files in the current working directory on the remote FTP server.</returns>
        public IList<FtpDirectoryEntry> FindFiles()
        {
            return this.FtpHandle.FindFiles();
        }

        /// <summary>
        /// Sends a command directly to the FTP server.
        /// </summary>
        /// <param name="command">The command to send to the FTP server.</param>
        public void SendCommand(string command)
        {
            this.FtpHandle.SendCommand(command);
        }

        /// <summary>
        /// Closes the FTP connection, aborting any operations that are in progress on another thread. This may be used to abort a  <see cref="O:Nito.KitchenSink.WinInet.FtpConnection.Connect"/> operation as well.
        /// </summary>
        public void Dispose()
        {
            if (this.FtpHandle != null)
            {
                this.FtpHandle.Dispose();
            }

            this.InternetHandle.Dispose();
        }
    }
}
