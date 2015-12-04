/*
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"  
*/

using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using Microsoft.Practices.ServiceLocation;
using Codeco.Services;
using System.Threading.Tasks;
using Codeco.ViewModel.DesignViewModels;
using Codeco.Services.Mocks;
#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
#endif

namespace Codeco.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            if (ViewModelBase.IsInDesignModeStatic)
            {
                // Create design time view services and models
                //SimpleIoc.Default.Register<IDataService, DesignDataService>();
                //SimpleIoc.Default.Register<IService, FileService>();                                 
                SimpleIoc.Default.Register<IService, MockFileService>();
                SimpleIoc.Default.Register<INavigationServiceEx, NavigationServiceEx>();
                SimpleIoc.Default.Register<SettingsViewModelDesign>();
                SimpleIoc.Default.Register<MainViewModelDesign>();
            }
            else
            {
                // Create run time view services and models
                //SimpleIoc.Default.Register<IDataService, DataService>();                
                NavigationServiceEx navService = InitializeNavigationService();
                SimpleIoc.Default.Register<INavigationServiceEx>(() => navService);

                IService fileService = InitializeFileService().Result;
                SimpleIoc.Default.Register(() => fileService);

                SimpleIoc.Default.Register<MainViewModel>();
                SimpleIoc.Default.Register<SettingsViewModel>();
            }            
        }

        private async Task<IService> InitializeFileService()
        {
            FileService service = new FileService();
            return await service.InitializeAsync();
        }

        private NavigationServiceEx InitializeNavigationService()
        {
            NavigationServiceEx navService = new NavigationServiceEx();
            navService.Configure(nameof(MainPage), typeof(MainPage));
            navService.Configure(nameof(SettingsPage), typeof(SettingsPage));
#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed += (s, e) =>
            {
                if (navService.BackStackDepth == 0 || navService.BackStack[navService.BackStackDepth - 1] == null)
                {
                    e.Handled = false; //let the system do what it will
                }
                else
                {
                    navService.OnBackButtonPressed(s, new Common.UniversalBackPressedEventArgs(e.Handled));
                    e.Handled = true;
                }
            };
#endif
            return navService;
        }

        public MainViewModel Main => ViewModelBase.IsInDesignModeStatic
            ? ServiceLocator.Current.GetInstance<MainViewModelDesign>()
            : ServiceLocator.Current.GetInstance<MainViewModel>();

        public SettingsViewModel Settings => ViewModelBase.IsInDesignModeStatic 
            ? ServiceLocator.Current.GetInstance<SettingsViewModelDesign>() 
            : ServiceLocator.Current.GetInstance<SettingsViewModel>();
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}