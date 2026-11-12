using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VetClinic.AppData;

namespace VetClinic.Pages
{
    public partial class AuthorizationPage : Page
    {
        public AuthorizationPage()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите email и пароль.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Ищем пользователя по Email и паролю
            var u = AppConnect.model0db.Users
                .FirstOrDefault(x => x.Email == login && x.PasswordHash == password);

            if (u == null)
            {
                MessageBox.Show("Неверный email или пароль.", "Ошибка входа",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // 1. Сохраняем пользователя в глобальную переменную
            AppConnect.user = u;

            // 2. Получаем ссылку на главное окно (один раз)
            var mainWindow = Application.Current.MainWindow as MainWindow;

            if (mainWindow != null)
            {
                // 3. Обновляем ФИО и видимость кнопок (включая кнопку "Управление")
                mainWindow.fio.Text = u.FIO;
                mainWindow.UpdateUIForRole(); // <-- ДА, ВЫЗЫВАЕМ ЗДЕСЬ!

                // 4. Переходим на нужную страницу в зависимости от роли
                // ⚠️ ВАЖНО: проверь в своей модели (.edmx), как точно называется поле: 
                // если оно называется idRole, замени u.RoleId на u.idRole
                if (u.RoleId == 1 || u.RoleId == 2)
                {
                    AppFrame.mainFrame.Navigate(new AppointmentsPage());
                }
                else
                {
                    AppFrame.mainFrame.Navigate(new ServicesPage());
                }
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new RegistrationPage());
        }

        private void Guest_Click(object sender, RoutedEventArgs e)
        {
            AppConnect.user = null;
            ((MainWindow)Application.Current.MainWindow).fio.Text = "Гость";
            AppFrame.mainFrame.Navigate(new ServicesPage());
        }
    }
}