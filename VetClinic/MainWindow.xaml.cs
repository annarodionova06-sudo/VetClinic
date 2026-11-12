using System;
using System.Windows;
using System.Windows.Controls;
using VetClinic.AppData;
using VetClinic.Pages;

namespace VetClinic
{
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
            MainFrame.Navigate(new AuthorizationPage());
        }

        // Метод для обновления интерфейса после авторизации
        public void UpdateUIForRole()
        {
            // Проверяем, что пользователь авторизован
            if (AppConnect.user != null)
            {
                // Показываем ФИО пользователя
                fio.Text = AppConnect.user.FIO;

                // Проверяем роль (idRole == 1 - Админ, idRole == 2 - Менеджер)
                if (AppConnect.user.RoleId == 1 || AppConnect.user.RoleId == 2)
                {
                    btnAdminMenu.Visibility = Visibility.Visible;
                }
                else
                {
                    btnAdminMenu.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                // Пользователь не авторизован
                fio.Text = "";
                btnAdminMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user != null)
            {
                MainFrame.Navigate(new ProfilePage());
            }
            else
            {
                MessageBox.Show("Авторизуйтесь для доступа к личному кабинету.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnAdminMenu_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AdminMenuPage());
        }

        // Метод для выхода из системы
        public void Logout()
        {
            AppConnect.user = null;
            MainFrame.Navigate(new AuthorizationPage());
            UpdateUIForRole();
        }
    }
}