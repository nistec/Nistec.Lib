//licHeader
//===============================================================================================================
// System  : Nistec.Lib - Nistec.Lib Class Library
// Author  : Nissim Trujman  (nissim@nistec.net)
// Updated : 01/07/2015
// Note    : Copyright 2007-2015, Nissim Trujman, All rights reserved
// Compiler: Microsoft Visual C#
//
// This file contains a class that is part of nistec library.
//
// This code is published under the Microsoft Public License (Ms-PL).  A copy of the license should be
// distributed with the code and can be found at the project website: http://nistec.net/license/nistec.cache-license.txt.  
// This notice, the author's name, and all copyright notices must remain intact in all applications, documentation,
// and source files.
//
//    Date     Who      Comments
// ==============================================================================================================
// 10/01/2006  Nissim   Created the code
//===============================================================================================================
//licHeader|
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Nistec.Generic;

namespace Nistec.Runtime
{
    /// <summary>
    /// Represent a wrapper functionality for FileSystemWatcher,
    /// that Listens to the file system change notifications and raises events when a
    /// directory, or file in a directory, changes.
    /// </summary>
    public class SysFileWatcher
    {
        /// <summary>
        /// Default file name.
        /// </summary>
        public const string DefaultFileName = "mcSysWatcher.xml";
        /// <summary>
        /// Default file filter
        /// </summary>
        public const string DefaultFileFilter = "*.xml";
        /// <summary>
        /// Get the system file path.
        /// </summary>
        public string SyncPath { get; private set; }
        /// <summary>
        /// Get the file name to watch.
        /// </summary>
        public string Filename { get; private set; }
        /// <summary>
        /// Get the file filter was specified by user.
        /// </summary>
        public string FileFilter { get; private set; }

        string FullPath()
        {
            return Path.Combine(SyncPath, Filename);
        }
        /// <summary>
        /// File System Event Handler
        /// </summary>
        public event FileSystemEventHandler FileChanged;
        /// <summary>
        /// Occured when file has been changed.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnFileChanged(FileSystemEventArgs e)
        {
            //if (FileChanged != null)
            //    FileChanged(this, e);
        }
        /// <summary>
        /// Initialize a new instance of SysFileWatcher
        /// </summary>
        public SysFileWatcher()
            : this(null, null)
        {

        }
        /// <summary>
        /// Initialize a new instance of SysFileWatcher
        /// </summary>
        /// <param name="fpath"></param>
        public SysFileWatcher(string fpath)
            : this(fpath,null)
        {

        }
        /// <summary>
        /// Initialize a new instance of SysFileWatcher
        /// </summary>
        /// <param name="fpath"></param>
        /// <param name="fileFilter"></param>
        public SysFileWatcher(string fpath, string fileFilter)
        {
            InitSysFileWatcher(fpath,fileFilter);
        }
        /// <summary>
        /// Initialize a new instance of SysFileWatcher
        /// </summary>
        /// <param name="fpath"></param>
        /// <param name="usefileNameAsFilter"></param>
        public SysFileWatcher(string fpath, bool usefileNameAsFilter)
        {
            string fileFilter = null;
            if (usefileNameAsFilter)
            {
                fileFilter = Path.GetFileName(fpath);
            }
            InitSysFileWatcher(fpath, fileFilter);
        }

        private  void InitSysFileWatcher(string fpath, string fileFilter)
        {
            //string fpath = CacheSettings.SyncConfigFile;
            if (string.IsNullOrEmpty(fpath))
            {
                SyncPath = SysNet.GetExecutingAssemblyPath();
                Filename = DefaultFileName;
            }
            else
            {
                Filename = Path.GetFileName(fpath);
                SyncPath = Path.GetDirectoryName(fpath);
            }

            if (string.IsNullOrEmpty(fileFilter))
            {
                FileFilter = DefaultFileFilter;
            }
            else
            {
                FileFilter = fileFilter;
            }


            //you can specify a file type or a specific filename as
            //the second parameter of FileSystemWatcher or *.* for all
            //type of files
            FileSystemWatcher WatchFile = new FileSystemWatcher(SyncPath, FileFilter);

            WatchFile.IncludeSubdirectories = false;
            WatchFile.NotifyFilter = NotifyFilters.LastWrite;

            //WatchFile.Created += new FileSystemEventHandler(this.FileCreated);
            //WatchFile.Renamed += new RenamedEventHandler(this.FileReNamed);
            //WatchFile.Deleted += new FileSystemEventHandler(this.FileDeleted);
            WatchFile.Changed += new FileSystemEventHandler(WatchFile_Changed);

            WatchFile.EnableRaisingEvents = true;
        }

        DateTime lastTimeRead = DateTime.MinValue;
        string lastFileRead = "";
        /// <summary>
        /// Get the last time file was readed.
        /// </summary>
        public DateTime LastTimeRead 
        {
            get { return lastTimeRead; }
        }
        /// <summary>
        /// Get the last file name was readed.
        /// </summary>
        public string LastFileRead
        {
            get { return lastFileRead; }
        }

        internal void WatchFile_Changed(object sender, FileSystemEventArgs e)
        {
            if (FileChanged != null)
            {
                if (Filename.ToLower() == e.Name.ToLower())
                {
                    DateTime lastWriteTime = File.GetLastWriteTime(e.FullPath);

                    if (lastWriteTime != lastTimeRead || lastFileRead != e.FullPath)
                    {
                        lastTimeRead = lastWriteTime;
                        lastFileRead = e.FullPath;
                        FileChanged(this, e);
                        Console.WriteLine(e.ToString());
                    }
                }
            }
            OnFileChanged(e);
        }

        internal void FileCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine(e.Name); //or anything you wish to display
            //do the processing of file and print it to
            //pdf writer port…or to a printer
        }

        internal void FileReNamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine("\nFile Renamed:\n");

            Console.WriteLine("Change Type: {0}", e.ChangeType);
            Console.WriteLine("Full Path: {0}", e.FullPath);
            Console.WriteLine("Name: {0}", e.Name);
            Console.WriteLine("Old Full Path: {0}", e.OldFullPath);
            Console.WriteLine("Old Name: {0}", e.OldName);
        }

        internal void FileDeleted(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine(e.ToString());
        }
  
    }
}
