using System;
using System.IO;
using System.Windows;

namespace Launch__
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Launch++");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }
}
