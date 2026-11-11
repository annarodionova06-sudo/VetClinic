using System.Windows;
using System.Windows.Threading;

namespace VetClinic
{
    public partial class App : Application
    {
        private void App_DispatcherUnhandledException(object sender,
            DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                $"Произошла ошибка: {e.Exception.Message}\n\n" +
                "Приносим извинения за неудобства.",
                "Ошибка приложения",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}