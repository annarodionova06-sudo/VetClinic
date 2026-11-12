using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using VetClinic.AppData;

namespace VetClinic.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            if (AppConnect.user == null) return;

            // Роль
            var role = AppConnect.model0db.Roles.FirstOrDefault(r => r.Id == AppConnect.user.RoleId);
            tbRole.Text = $"Ваша роль: {role?.RoleName ?? "Неизвестно"}";
            txtRole.Text = role?.RoleName ?? "";

            // Дата регистрации
            txtRegDate.Text = AppConnect.user.RegistrationDate.ToString("dd.MM.yyyy");

            // Данные пользователя
            txtSurname.Text = AppConnect.user.Surname;
            txtFirstName.Text = AppConnect.user.FirstName;
            txtPatronymic.Text = AppConnect.user.Patronymic ?? "";
            txtEmail.Text = AppConnect.user.Email;
            txtPhone.Text = AppConnect.user.Phone;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Валидация обязательных полей
            if (string.IsNullOrWhiteSpace(txtSurname.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Заполните все обязательные поля (отмеченные *).",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация email
            if (!Regex.IsMatch(txtEmail.Text.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Введите корректный email.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка уникальности email (если изменился)
            string newEmail = txtEmail.Text.Trim();
            if (newEmail != AppConnect.user.Email &&
                AppConnect.model0db.Users.Any(u => u.Email == newEmail))
            {
                MessageBox.Show("Пользователь с таким email уже существует.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка уникальности телефона (если изменился)
            string newPhone = txtPhone.Text.Trim();
            if (newPhone != AppConnect.user.Phone &&
                AppConnect.model0db.Users.Any(u => u.Phone == newPhone))
            {
                MessageBox.Show("Пользователь с таким телефоном уже существует.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверка смены пароля
            string newPassword = txtNewPassword.Password;
            string confirmPassword = txtConfirmPassword.Password;

            if (!string.IsNullOrEmpty(newPassword) || !string.IsNullOrEmpty(confirmPassword))
            {
                if (newPassword != confirmPassword)
                {
                    MessageBox.Show("Пароли не совпадают.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (newPassword.Length < 6)
                {
                    MessageBox.Show("Пароль должен содержать минимум 6 символов.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            try
            {
                // Сохранение данных
                AppConnect.user.Surname = txtSurname.Text.Trim();
                AppConnect.user.FirstName = txtFirstName.Text.Trim();
                AppConnect.user.Patronymic = string.IsNullOrWhiteSpace(txtPatronymic.Text)
                    ? null
                    : txtPatronymic.Text.Trim();
                AppConnect.user.Email = newEmail;
                AppConnect.user.Phone = newPhone;

                // Смена пароля (если введён)
                if (!string.IsNullOrEmpty(newPassword))
                {
                    AppConnect.user.PasswordHash = newPassword;
                }

                AppConnect.model0db.SaveChanges();

                // Обновляем ФИО в шапке
                var mw = Application.Current.MainWindow as MainWindow;
                if (mw != null)
                {
                    mw.fio.Text = AppConnect.user.FIO;
                }

                MessageBox.Show("Данные успешно сохранены!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Очищаем поля пароля
                txtNewPassword.Clear();
                txtConfirmPassword.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Возврат на предыдущую страницу в зависимости от роли
            if (AppConnect.user != null && (AppConnect.user.RoleId == 1 || AppConnect.user.RoleId == 2))
            {
                AppFrame.mainFrame.Navigate(new AppointmentsPage());
            }
            else
            {
                AppFrame.mainFrame.Navigate(new ServicesPage());
            }
        }
    }
}