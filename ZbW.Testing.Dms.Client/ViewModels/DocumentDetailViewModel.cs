using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using ZbW.Testing.Dms.Client.Model;
using ZbW.Testing.Dms.Client.Services.Interfaces;
using ZbW.Testing.Dms.Client.Views;

namespace ZbW.Testing.Dms.Client.ViewModels
{
    using System;
    using System.Collections.Generic;

    using Microsoft.Win32;

    using Prism.Commands;
    using Prism.Mvvm;

    using ZbW.Testing.Dms.Client.Repositories;

    internal class DocumentDetailViewModel : BindableBase
    {
        private readonly Action _navigateBack;

        private string _benutzer;

        private string _bezeichnung;

        private DateTime _erfassungsdatum;

        private string _filePath;

        private bool _isRemoveFileEnabled;

        private string _selectedTypItem;

        private string _stichwoerter;

        private List<string> _typItems;

        private DateTime? _valutaDatum;

        public DocumentDetailViewModel(string benutzer, Action navigateBack)
        {
            _navigateBack = navigateBack;
            Benutzer = benutzer;
            Erfassungsdatum = DateTime.Now;
            TypItems = ComboBoxItems.Typ;

            CmdDurchsuchen = new DelegateCommand(OnCmdDurchsuchen);
            CmdSpeichern = new DelegateCommand(OnCmdSpeichern);
        }

        public string Stichwoerter
        {
            get
            {
                return _stichwoerter;
            }

            set
            {
                SetProperty(ref _stichwoerter, value);
            }
        }

        public string Bezeichnung
        {
            get
            {
                return _bezeichnung;
            }

            set
            {
                SetProperty(ref _bezeichnung, value);
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
                SetProperty(ref _selectedTypItem, value);
            }
        }

        public DateTime Erfassungsdatum
        {
            get
            {
                return _erfassungsdatum;
            }

            set
            {
                SetProperty(ref _erfassungsdatum, value);
            }
        }

        public string Benutzer
        {
            get
            {
                return _benutzer;
            }

            set
            {
                SetProperty(ref _benutzer, value);
            }
        }

        public DelegateCommand CmdDurchsuchen { get; }

        public DelegateCommand CmdSpeichern { get; }

        public DateTime? ValutaDatum
        {
            get
            {
                return _valutaDatum;
            }

            set
            {
                SetProperty(ref _valutaDatum, value);
            }
        }

        public bool IsRemoveFileEnabled
        {
            get
            {
                return _isRemoveFileEnabled;
            }

            set
            {
                SetProperty(ref _isRemoveFileEnabled, value);
            }
        }

        private void OnCmdDurchsuchen()
        {
            var openFileDialog = new OpenFileDialog();
            var result = openFileDialog.ShowDialog();

            if (result.GetValueOrDefault())
            {
                _filePath = openFileDialog.FileName;
            }
        }

        private async void OnCmdSpeichern()
        {
            if (string.IsNullOrEmpty(Bezeichnung) || string.IsNullOrEmpty(SelectedTypItem) ||
                string.IsNullOrEmpty(Stichwoerter) || ValutaDatum == null)
            {
                MessageBox.Show("Es müssen alle Pflichtfelder ausgefüllt werden!", "Achtung!", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            
            var storage = LoginView.ServiceProvider.GetService<IStorageService>();
            try
            {
                using (var stream = File.OpenRead(_filePath))
                {
                    var doc = new Document()
                    {
                        Id = Guid.NewGuid(),
                        File = stream,
                        Metadata = new MetadataItem()
                        {
                            CreationTime = Erfassungsdatum,
                            FileEnding = Path.GetExtension(_filePath),
                            FileName = Bezeichnung,
                            Keywords = Stichwoerter,
                            Typ = SelectedTypItem,
                            Username = Benutzer,
                            Valuta = ValutaDatum.GetValueOrDefault()
                        }
                    };

                    await storage.SaveDocument(doc);
                }

                if (IsRemoveFileEnabled)
                {
                    File.Delete(_filePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Failed to Save!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            _navigateBack();
        }
    }
}