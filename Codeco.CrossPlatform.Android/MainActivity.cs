﻿using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using AndroidGraphics = Android.Graphics; //Avoid namespaace collision witih Codeco.CrossPlatform.Android
using Acr.UserDialogs;
using Rg.Plugins.Popup;
using Android.Util;
using Codeco.CrossPlatform.Droid.DependencyServices;
using Android.Support.V4.Content;
using Android;
using System.Threading.Tasks;
using Android.Support.V4.App;
using Codeco.CrossPlatform.Services.DependencyInterfaces;
using GalaSoft.MvvmLight.Ioc;

namespace Codeco.CrossPlatform.Droid
{
    [Activity(Label = "Codeco.CrossPlatform", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Popup.Init(this, bundle);
            UserDialogs.Init(this);            

            // Populate theme before we init the Xamarin runtime            
            PlatformColorService.CurrentTheme = Theme;

            global::Xamarin.Forms.Forms.Init(this, bundle);

            var crossPlatApp = new App();
            RegisterXamarinDependencies();
            LoadApplication(crossPlatApp);

            GetStoragePermissions();
        }

        private const int StorageRequestCode = 1;
        private void GetStoragePermissions()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != Permission.Granted
                || ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage }, StorageRequestCode);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == 1)
            {
                if (grantResults[0] != Permission.Granted)
                {
                    // Say angry things.
                }
            }
        }

        public override void OnBackPressed()
        {
            if (Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed))
            {
                System.Diagnostics.Debug.WriteLine("Get Popup.OnBackPressed");
            }
            else
            {                
                base.OnBackPressed();
            }
        }

        private void RegisterXamarinDependencies()
        {
            //Dependency services            
            SimpleIoc.Default.Register<IAppFolderService, AppFolderService>();
            SimpleIoc.Default.Register<INativeFileServiceFacade, NativeFileServiceFacade>();
            SimpleIoc.Default.Register<IPlatformColorService, PlatformColorService>();
            SimpleIoc.Default.Register<IFileSystemWatcherService, FileSystemWatcherService>();
            SimpleIoc.Default.Register<IConnectedDeviceService, ConnectedDeviceService>();
        }
    }
}

