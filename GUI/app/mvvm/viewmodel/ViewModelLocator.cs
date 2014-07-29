using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using gui.app.controller;
using gui.app.controller.amd;
using gui.app.controller.nvidia;
using Microsoft.Practices.ServiceLocation;

namespace gui.app.mvvm.viewmodel
{
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<ShellViewModel>();
        }

        public ShellViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<ShellViewModel>();
            }
        }

        public static void Cleanup()
        {
        }
    }

    public class ShellViewModel : ViewModelBase
    {
        public ShellViewModel()
        {
            IGpuAdapter amdAdapter = new AmdAdapter();
            IGpuAdapter nvidiaAdapter = new NvidiaAdapter();

            if (amdAdapter.IsAvailable())
            {
                this.GpuViewModel = new AmdViewModel();
            }
            else if (nvidiaAdapter.IsAvailable())
            {
                this.GpuViewModel = new NvidiaViewModel();
            }
            else
            {
                this.GpuViewModel = new NotAvailableGpuViewModel();
            }
        }

        public IGpuViewModel GpuViewModel { get; private set; }
    }
}