﻿using CodeStore8UI.Common;
using CodeStore8UI.Model;
using CodeStore8UI.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CodeStore8UI
{    
    public sealed partial class MainPage : BindablePage
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }      

        private void Border_Tapped(object sender, TappedRoutedEventArgs e)
        {
            AppBar.IsOpen = true;
        }

        private void SavedFilesControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                StorageFile file = (e.AddedItems[0] as BindableStorageFile).BackingFile;
                (this.DataContext as MainViewModel).ChangeActiveFileCommand.Execute(file);
            }
        }
    }
}