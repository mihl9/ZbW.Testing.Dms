﻿using Microsoft.Extensions.DependencyInjection;
using ZbW.Testing.Dms.Client.Services;
using ZbW.Testing.Dms.Client.Services.Interfaces;
using ZbW.Testing.Dms.Client.Views;

namespace ZbW.Testing.Dms.Client
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            _serviceProvider = services.BuildServiceProvider();
        }


        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMetadataService, MetadataService>();
            services.AddSingleton<IStorageService, FilesystemStorageHandler>();

            services.AddSingleton<LoginView>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var main = _serviceProvider.GetService<LoginView>();
            main.Show();
        }
    }
}