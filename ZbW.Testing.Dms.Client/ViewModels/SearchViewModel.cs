using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ZbW.Testing.Dms.Client.Services.Interfaces;
using ZbW.Testing.Dms.Client.Views;

namespace ZbW.Testing.Dms.Client.ViewModels
{
    using System.Collections.Generic;

    using Prism.Commands;
    using Prism.Mvvm;

    using ZbW.Testing.Dms.Client.Model;
    using ZbW.Testing.Dms.Client.Repositories;

    internal class SearchViewModel : BindableBase
    {
        private List<Document> _filteredDocumentItems;

        private Document _selectedDocumentItem;

        private string _selectedTypItem;

        private string _suchbegriff;

        private List<string> _typItems;

        public SearchViewModel()
        {
            TypItems = ComboBoxItems.Typ;

            CmdSuchen = new DelegateCommand(OnCmdSuchen, OnCanCmdSuchen);
            CmdReset = new DelegateCommand(OnCmdReset);
            CmdOeffnen = new DelegateCommand(OnCmdOeffnen, OnCanCmdOeffnen);
        }

        public DelegateCommand CmdOeffnen { get; }

        public DelegateCommand CmdSuchen { get; }

        public DelegateCommand CmdReset { get; }

        public string Suchbegriff
        {
            get
            {
                return _suchbegriff;
            }

            set
            {
                if (SetProperty(ref _suchbegriff, value))
                {
                    CmdSuchen.RaiseCanExecuteChanged();
                }
            }
        }

        public List<string> TypItems
        {
            get
            {
                return _typItems;
            }

            set
            {
                SetProperty(ref _typItems, value);
            }
        }

        public string SelectedTypItem
        {
            get
            {
                return _selectedTypItem;
            }

            set
            {
                if (SetProperty(ref _selectedTypItem, value))
                {
                    CmdSuchen.RaiseCanExecuteChanged();
                }
            }
        }

        public List<Document> FilteredDocumentItems
        {
            get
            {
                return _filteredDocumentItems;
            }

            set
            {
                SetProperty(ref _filteredDocumentItems, value);
            }
        }

        public Document SelectedDocumentItem
        {
            get
            {
                return _selectedDocumentItem;
            }

            set
            {
                if (SetProperty(ref _selectedDocumentItem, value))
                {
                    CmdOeffnen.RaiseCanExecuteChanged();
                }
            }
        }

        private bool OnCanCmdOeffnen()
        {
            return SelectedDocumentItem != null;
        }

        private void OnCmdOeffnen()
        {
            if (!OnCanCmdOeffnen()) return;
            var storage = LoginView.ServiceProvider.GetService<IStorageService>();

            storage.OpenDocumentExternal(SelectedDocumentItem);
        }

        private bool OnCanCmdSuchen()
        {
            return !(string.IsNullOrEmpty(Suchbegriff) && string.IsNullOrEmpty(SelectedTypItem));
        }

        private async void OnCmdSuchen()
        {
            if(!OnCanCmdSuchen())
                return;

            try
            {
                var storage = LoginView.ServiceProvider.GetService<IStorageService>();

                FilteredDocumentItems = await storage.SearchDocument(Suchbegriff, SelectedTypItem);
            }
            catch (Exception e)
            {
                MessageBox.Show("Failed to Search!");
            }
            
        }

        private void OnCmdReset()
        {
            FilteredDocumentItems = new List<Document>();

            Suchbegriff = string.Empty;
            SelectedTypItem = string.Empty;
        }
    }
}