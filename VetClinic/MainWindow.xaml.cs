using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VetClinic.AppData;

namespace VetClinic
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Создаем контекст БД
            AppConnect.model0db = new VetClinicEntities();

            // Сохраняем фрейм для навигации
            AppFrame.mainFrame = MainFrame;

            // Переходим на страницу входа
            MainFrame.Navigate(new Pages.AuthorizationPage());
        }
    }
}
