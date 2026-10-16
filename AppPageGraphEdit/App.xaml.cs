using OPLAPI.OIEL.CORE.Browser;
using System.Configuration;
using System.Data;
using System.Windows;

namespace AppPageGraphEdit
{
    /// <summary>
    /// Главный класс приложения
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Точка входа в программу
        /// </summary>
        /// <param name="e">Объект события начала работы прораммы</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            PageBrowser SourcePage = new MainPage();
            MainWindow = new Window()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Content = SourcePage,
                Width = SourcePage.Width + 50,
                Height = SourcePage.Height + 50,
            };
            MainWindow.Show();
        }
    }

}
