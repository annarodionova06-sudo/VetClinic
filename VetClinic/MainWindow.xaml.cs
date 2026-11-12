using System.Windows;
using VetClinic.AppData;
using VetClinic.Pages;

namespace VetClinic
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AppConnect.model0db = new VetClinicEntities();
            AppFrame.mainFrame = MainFrame;

            MainFrame.Navigate(new AuthorizationPage());

            // ВАЖНО: вызываем при старте, чтобы гость не видел кнопку
            UpdateUIForRole();
        }

        public void UpdateUIForRole()
        {
            // Гость (user == null) или пользователь с ролью 3 — кнопку не показываем
            if (AppConnect.user != null &&
                (AppConnect.user.RoleId == 1 || AppConnect.user.RoleId == 2))
            {
                btnAdminMenu.Visibility = Visibility.Visible;
            }
            else
            {
                btnAdminMenu.Visibility = Visibility.Collapsed;
            }

            // ФИО: если не авторизован — показываем "Гость"
            if (AppConnect.user != null)
            {
                fio.Text = AppConnect.user.FIO;
            }
            else
            {
                fio.Text = "Гость";
            }
        }

        private void btnAdminMenu_Click(object sender, RoutedEventArgs e)
        {
            // Дублирующая проверка прямо на клике
            if (AppConnect.user == null ||
                (AppConnect.user.RoleId != 1 && AppConnect.user.RoleId != 2))
            {
                MessageBox.Show("Доступ запрещён. Раздел доступен только менеджерам и администраторам.",
                    "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MainFrame.Navigate(new AdminMenuPage());
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

        public void Logout()
        {
            AppConnect.user = null;
            MainFrame.Navigate(new AuthorizationPage());
            UpdateUIForRole(); // кнопка снова скроется
        }
    }
}