﻿using CodeStore8UI.Common;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml.Navigation;
using System.Linq;
using System.Collections.ObjectModel;
using CodeStore8UI.Services;
using CodeStore8UI.Model;
using GalaSoft.MvvmLight.Command;

namespace CodeStore8UI.ViewModel
{
    public class SettingsViewModel : ViewModelBase, INavigable
    {
        private FileService _fileService;

        public bool AllowGoingBack { get; set; }

        private RelayCommand<BindableStorageFile> _syncFileCommand;
        public RelayCommand<BindableStorageFile> SyncFileCommand => 
            _syncFileCommand ?? (_syncFileCommand = new RelayCommand<BindableStorageFile>(SyncFile));

        private RelayCommand<BindableStorageFile> _removeFileFromSyncCommand;
        public RelayCommand<BindableStorageFile> RemoveFileFromSyncCommand => 
            _removeFileFromSyncCommand ?? (_removeFileFromSyncCommand = new RelayCommand<BindableStorageFile>(RemoveFileFromSync));        

        private ulong _roamingSpaceUsed = 0;
        public ulong RoamingSpaceUsed
        {
            get { return _roamingSpaceUsed; }
            set
            {
                if(value == _roamingSpaceUsed)
                {
                    return;
                }
                _roamingSpaceUsed = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<BindableStorageFile> _syncingFiles = new ObservableCollection<BindableStorageFile>();
        public ObservableCollection<BindableStorageFile> SyncingFiles
        {
            get { return _syncingFiles; }
            set
            {
                if(value == _syncingFiles)
                {
                    return;
                }
                _syncingFiles = value;
                RaisePropertyChanged();
            }
        }        

        public SettingsViewModel(IService fileService)
        {
            _fileService = fileService as FileService;
        }

        private void SyncFile(BindableStorageFile file)
        {
            //_fileService.RoamFile(file);
        }

        private void RemoveFileFromSync(BindableStorageFile file)
        {
            //_fileService.StopRoamingFile(file);
        }

        public async void Activate(object parameter, NavigationMode navigationMode)
        {
            await _fileService.InitializeAsync();
            SyncingFiles.Clear();
            foreach(var file in _fileService.LoadedFiles)
            {
                if(file.IsRoamed)
                {
                    SyncingFiles.Add(file);
                }
            }            


        }

        public void Deactivate(object parameter)
        {
            
        }
    }
}
