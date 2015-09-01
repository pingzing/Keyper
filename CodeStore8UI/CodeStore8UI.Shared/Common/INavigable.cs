﻿using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Navigation;

namespace CodeStore8UI.Common
{
    public interface INavigable
    {
        void Activate(object parameter, NavigationMode navigationMode);
        void Deactivate(object parameter);
        bool AllowGoingBack { get; set; }
    }
}