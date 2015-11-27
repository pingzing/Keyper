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
using GalaSoft.MvvmLight.Views;
using System.Threading.Tasks;

namespace CodeStore8UI.ViewModel
{
    public class SettingsViewModel : ViewModelBase, INavigable
    {
        private FileService _fileService;
        private NavigationService _navigationService;

        public bool AllowGoingBack { get; set; } = true;
        private static AsyncLock s_lock = new AsyncLock();

        private RelayCommand<BindableStorageFile> _syncFileCommand;
        public RelayCommand<BindableStorageFile> SyncFileCommand => 
            _syncFileCommand ?? (_syncFileCommand = new RelayCommand<BindableStorageFile>(SyncFile));

        private RelayCommand<BindableStorageFile> _removeFileFromSyncCommand;
        public RelayCommand<BindableStorageFile> RemoveFileFromSyncCommand => 
            _removeFileFromSyncCommand ?? (_removeFileFromSyncCommand = new RelayCommand<BindableStorageFile>(RemoveFileFromSync));

        private RelayCommand _goBackCommand;
        public RelayCommand GoBackCommand => _goBackCommand ?? (_goBackCommand = new RelayCommand(GoBack));        

        private double _roamingSpaceUsed = 0;
        public double RoamingSpaceUsed
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

        private ObservableCollection<FileCollection> _fileGroups = new ObservableCollection<FileCollection>();
        public ObservableCollection<FileCollection> FileGroups
        {
            get { return _fileGroups; }
            set
            {
                if (_fileGroups == value)
                {
                    return;
                }
                _fileGroups = value;
                RaisePropertyChanged();
            }
        }

        public SettingsViewModel(IService fileService, INavigationService navService)
        {
            _fileService = fileService as FileService;
            _navigationService = navService as NavigationService;
        }

        private async void SyncFile(BindableStorageFile file)
        {
            FileGroups.First(x => x.Location == FileService.FileLocation.Local).Files.Remove(file);
            FileGroups.First(x => x.Location == FileService.FileLocation.Roamed).Files.Add(file);            
            await UpdateRoamingSpaceUsed();
        }

        private async void RemoveFileFromSync(BindableStorageFile file)
        {
            FileGroups.First(x => x.Location == FileService.FileLocation.Roamed).Files.Remove(file);
            FileGroups.First(x => x.Location == FileService.FileLocation.Local).Files.Add(file);               
            await UpdateRoamingSpaceUsed();
        }

        private async Task UpdateRoamingSpaceUsed()
        {
            using (await s_lock.Acquire())
            {

                ulong space = 0;
                if (FileGroups.All(x => x.Location != FileService.FileLocation.Roamed))
                {
                    return;
                }
                var syncedFiles = FileGroups.First(x => x.Location == FileService.FileLocation.Roamed).Files;
                for (int i = syncedFiles.Count - 1; i >= 0; i--)
                {
                    space += await syncedFiles[i].GetFileSizeInBytes();
                    await Task.Delay(250);
                }
                space += await FileUtilities.GetIVFileSize();
                RoamingSpaceUsed = (double)space / 1024;
            }            
        }

        private void GoBack()
        {             
            _navigationService.GoBack();            
        }

        public async void Activate(object parameter, NavigationMode navigationMode)
        {            
            if (navigationMode == NavigationMode.New && FileGroups.Count == 0)
            {                
                FileGroups.Add(new FileCollection(Constants.ROAMED_FILES_TITLE, 
                    new ObservableCollection<IBindableStorageFile>(_fileService.GetRoamedFiles()), FileService.FileLocation.Roamed));
                FileGroups.Add(new FileCollection(Constants.LOCAL_FILES_TITLE, 
                    new ObservableCollection<IBindableStorageFile>(_fileService.GetLocalFiles()), FileService.FileLocation.Local));
            }
            await UpdateRoamingSpaceUsed();
        }

        public async void Deactivating(object parameter)
        {
            //This needs to happen _before_ Deactivate(), because MainPage's Activate() fires BEFORE Deactivate() does.
            foreach (var local in FileGroups.First(x => x.Location == FileService.FileLocation.Local).Files)
            {
                await _fileService.StopRoamingFile(local);
            }
            foreach (var roamed in FileGroups.First(x => x.Location == FileService.FileLocation.Roamed).Files)
            {
                await _fileService.RoamFile(roamed);
            }
        }

        public void Deactivated(object parameter)
        {
            
        }
    }
}
