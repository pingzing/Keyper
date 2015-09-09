﻿using CodeStore8UI.Model;
using CodeStore8UI.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace CodeStore8UI
{
    public class FileService : ServiceBase
    {
        private bool _initialized = false;

        public enum FileLocation { Local, Roamed };

        private ObservableCollection<IBindableStorageFile> _localFiles;        
        public ObservableCollection<IBindableStorageFile> LocalFiles
        {
            get
            {
                if (_initialized)
                {
                    return _localFiles;
                }
                else
                {
                    throw new ServiceNotInitializedException($"{nameof(FileService)} was not initialized before access was attempted.");
                }
            }
            private set
            {
                if(_localFiles == value)
                {
                    return;
                }
                if(!_initialized)
                {
                    throw new ServiceNotInitializedException($"{nameof(FileService)} was not initialized before access was attempted.");
                }

                _localFiles = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<IBindableStorageFile> _roamedFiles;
        public ObservableCollection<IBindableStorageFile> RoamedFiles
        {
            get
            {
                if(_initialized)
                {
                    return _roamedFiles;
                }
                else
                {
                    throw new ServiceNotInitializedException($"{nameof(FileService)} was not initialized before access was attempted.");
                }                
            }
            private set
            {
                if(_roamedFiles == value)
                {
                    return;
                }
                if(!_initialized)
                {
                    throw new ServiceNotInitializedException($"{nameof(FileService)} was not initialized before access was attempted.");
                }

                _roamedFiles = value;
                RaisePropertyChanged();
            }
        }

        public async Task StopRoamingFile(BindableStorageFile file)
        {           
            //UI
            LocalFiles.Add(file);
            file.IsRoamed = false;
            RoamedFiles.Remove(file);

            //Backing values
            await FileUtilities.MoveFileToRoamingAsync((StorageFile)file.BackingFile);            
        }

        public async Task RoamFile(BindableStorageFile file)
        {            
            //UI
            RoamedFiles.Add(file);
            file.IsRoamed = true;
            LocalFiles.Remove(file);

            //Backing values
            await FileUtilities.MoveFileToLocalAsync((StorageFile)file.BackingFile);            
        }

        protected async override Task CreateAsync()
        {
            if(_initialized)
            {
                return;
            }
            ApplicationData.Current.DataChanged += OnRoamingDataChanged;

            var files = await FileUtilities.GetFilesAsync();            
            List<BindableStorageFile> localFiles = new List<BindableStorageFile>();
            foreach(var file in files.LocalFiles)
            {
                BindableStorageFile localFile = await BindableStorageFile.Create(file);
                localFiles.Add(localFile);
            }

            List<BindableStorageFile> roamedFiles = new List<BindableStorageFile>();
            foreach (var file in files.RoamingFiles)
            {
                BindableStorageFile localFile = await BindableStorageFile.Create(file);
                localFile.IsRoamed = true;
                roamedFiles.Add(localFile);
            }
            
            _localFiles = new ObservableCollection<IBindableStorageFile>(localFiles);
            RaisePropertyChanged(nameof(LocalFiles));
            _roamedFiles = new ObservableCollection<IBindableStorageFile>(roamedFiles);
            RaisePropertyChanged(nameof(RoamedFiles));
            _initialized = true;
        }        

        /// <summary>
        /// Saves and encrypts the contents into a StorageFile with the given name, and the given password.
        /// </summary>
        /// <param name="contents">The contents to encrypt.</param>
        /// <param name="fileName">The filename with which to save the file.</param>
        /// <param name="password">The password that will be used to generate the encryption key.</param>
        /// <returns>If successful, returns the created StorageFile.</returns>
        public async Task<BindableStorageFile> SaveAndEncryptFileAsync(string contents, string fileName, string password)
        {
            if(!_initialized)
            {
                throw new ServiceNotInitializedException($"{nameof(FileService)} was not initialized before access was attempted.");
            }
            string savedFileName = await FileUtilities.SaveAndEncryptFileAsync(contents, fileName, password);
            StorageFile savedFile = await FileUtilities.GetEncryptedFileAsync(savedFileName);
            BindableStorageFile bsf = await BindableStorageFile.Create(savedFile);
            LocalFiles.Add(bsf);
            return bsf;
        }

        public IBindableStorageFile GetLoadedFile(string savedFileName)
        {
            if (!_initialized)
            {
                throw new ServiceNotInitializedException($"{nameof(FileService)} was not initialized before access was attempted.");
            }
            return LocalFiles.Where(x => x.Name == savedFileName).Single();
        }

        internal async Task<string> RetrieveFileContentsAsync(string name, string password, FileLocation location)
        {
            if (!_initialized)
            {
                throw new ServiceNotInitializedException($"{nameof(FileService)} was not initialized before access was attempted.");
            }

            var collectionToSearch = location == FileLocation.Local ? LocalFiles : RoamedFiles;
            string encryptedContents = await FileIO.ReadTextAsync(collectionToSearch.Single(x => x.Name == name).BackingFile);
            if(String.IsNullOrWhiteSpace(encryptedContents))
            {
                return null;
            }
            return EncryptionManager.Decrypt(encryptedContents, password);
        }

        internal async Task ClearFileAsync(string name, FileLocation location)
        {
            if (!_initialized)
            {
                throw new ServiceNotInitializedException($"{nameof(FileService)} was not initialized before access was attempted.");
            }
            var collectionToSearch = location == FileLocation.Local ? LocalFiles : RoamedFiles;
            var file = collectionToSearch.Single(x => x.Name == name);
            await FileIO.WriteTextAsync(file.BackingFile, string.Empty);
        }

        internal async Task DeleteFileAsync(StorageFile backingFile, FileLocation location)
        {
            if (!_initialized)
            {
                throw new ServiceNotInitializedException($"{nameof(FileService)} was not initialized before access was attempted.");
            }
            var collectionToSearch = location == FileLocation.Local ? LocalFiles : RoamedFiles;
            collectionToSearch.Remove(collectionToSearch.Single(x => x.BackingFile == backingFile)); 
            await FileUtilities.DeleteFileAsync(backingFile);
        }

        internal async Task RenameFileAsync(IBindableStorageFile file, string newName)
        {
            if(!_initialized)
            {
                throw new ServiceNotInitializedException($"{nameof(FileService)} was not initialized before access was attempted.");
            }                  
            await FileUtilities.RenameFileAsync((StorageFile)file.BackingFile, newName);
            file.NameChanged();
        }

        public async Task NukeFiles()
        {
            if(!_initialized)
            {
                throw new ServiceNotInitializedException($"{nameof(FileService)} was not initialized before access was attempted.");
            }
            for(int i = LocalFiles.Count - 1; i >= 0; i--)
            {
                await DeleteFileAsync((StorageFile)LocalFiles[i].BackingFile, FileLocation.Local);
            }
            for (int i = LocalFiles.Count - 1; i >= 0; i--)
            {
                await DeleteFileAsync((StorageFile)RoamedFiles[i].BackingFile, FileLocation.Roamed);
            }

        }

        private async void OnRoamingDataChanged(ApplicationData sender, object args)
        {
            foreach(var file in LocalFiles)
            {
                if(file.IsRoamed)
                {
                    LocalFiles.Remove(file);
                }
            }

            var roamingFiles = await sender.RoamingFolder.GetFilesAsync();
            foreach(var file in roamingFiles)
            {
                BindableStorageFile bsf = await BindableStorageFile.Create(file);                
                LocalFiles.Add(bsf);
            }
        }
        

        internal FileLocation GetFileLocation(BindableStorageFile file)
        {            
            return file.IsRoamed ? FileLocation.Roamed : FileLocation.Local;
        }
    }
}
