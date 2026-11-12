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

            // Вызываем при старте, чтобы настроить шапку для гостя
            UpdateHeader();
        }

        /// <summary>
        /// Единый метод для обновления шапки (ФИО и видимость кнопок)
        /// </summary>
        public void UpdateHeader()
        {
            if (AppConnect.user == null)
            {
                // ГОСТЬ
                fio.Text = "Гость";
                btnAdminMenu.Visibility = Visibility.Collapsed;
                btnProfile.Visibility = Visibility.Collapsed;
            }
            else if (AppConnect.user.RoleId == 1)
            {
                // АДМИНИСТРАТОР
                fio.Text = AppConnect.user.FIO;
                btnAdminMenu.Visibility = Visibility.Visible;
                btnProfile.Visibility = Visibility.Visible;
            }
            else if (AppConnect.user.RoleId == 2)
            {
                // ВЕТЕРИНАР / МЕНЕДЖЕР
                fio.Text = AppConnect.user.FIO;
                btnAdminMenu.Visibility = Visibility.Visible; // Менеджер тоже имеет доступ к управлению
                btnProfile.Visibility = Visibility.Visible;
            }
            else
            {
                // КЛИЕНТ (Роль 3)
                fio.Text = AppConnect.user.FIO;
                btnAdminMenu.Visibility = Visibility.Collapsed;
                btnProfile.Visibility = Visibility.Visible;
            }
        }

        private void btnAdminMenu_Click(object sender, RoutedEventArgs e)
        {
            // Дублирующая проверка на всякий случай
            if (AppConnect.user == null || (AppConnect.user.RoleId != 1 && AppConnect.user.RoleId != 2))
            {
                MessageBox.Show("Доступ запрещён.", "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            MainFrame.Navigate(new AdminMenuPage()); // Или ManagementPage, как у тебя названа страница
        }

        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user != null)
            {
                MainFrame.Navigate(new ProfilePage());
            }
        }

        /// <summary>
        /// Метод для корректного выхода из системы (вызывать из других страниц)
        /// </summary>
        public void Logout()
        {
            AppConnect.user = null;
            AppConnect.CartServices.Clear();
            AppConnect.CartMedicines.Clear();

            UpdateHeader(); // <-- ВОТ ЭТО обновит шапку на "Гость" и скроет кнопки!

            MainFrame.Navigate(new AuthorizationPage());
        }
    }
}