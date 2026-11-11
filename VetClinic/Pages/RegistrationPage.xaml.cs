using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using VetClinic.AppData;

namespace VetClinic.Pages
{
    public partial class RegistrationPage : Page
    {
        public RegistrationPage()
        {
            InitializeComponent();
        }

       
        private bool ValidateEmail(string email)
        {
            try
            {
                return Regex.IsMatch(email.Trim(),
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch
            {
                return false;
            }
        }

        private bool ValidatePhone(string phone)
        {
            string cleaned = Regex.Replace(phone, @"[^\d+]", "");
            return cleaned.Length >= 10 && cleaned.Length <= 20;
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSurname.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtPhone.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Password))
            {
                MessageBox.Show("Заполните все обязательные поля (отмеченные *).",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidateEmail(txtEmail.Text))
            {
                MessageBox.Show("Введите корректный email адрес.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (AppConnect.model0db.Users.Any(u => u.Email == txtEmail.Text.Trim()))
            {
                MessageBox.Show("Пользователь с таким email уже существует.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!ValidatePhone(txtPhone.Text))
            {
                MessageBox.Show("Введите корректный номер телефона (минимум 10 цифр).",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (AppConnect.model0db.Users.Any(u => u.Phone == txtPhone.Text.Trim()))
            {
                MessageBox.Show("Пользователь с таким телефоном уже существует.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (txtPassword.Password.Length < 6)
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (txtPassword.Password != txtPasswordConfirm.Password)
            {
                MessageBox.Show("Пароли не совпадают.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var newUser = new Users
                {
                    Surname = txtSurname.Text.Trim(),
                    FirstName = txtFirstName.Text.Trim(),
                    Patronymic = string.IsNullOrWhiteSpace(txtPatronymic.Text)
                    ? null
                    : txtPatronymic.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Phone = txtPhone.Text.Trim(),
                    PasswordHash = txtPassword.Password,  // ← было HashPassword(...), стало plain text
                    RegistrationDate = DateTime.Today,
                    RoleId = 3
                };

                AppConnect.model0db.Users.Add(newUser);
                AppConnect.model0db.SaveChanges();

                MessageBox.Show("Регистрация успешна! Теперь вы можете войти.",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                AppFrame.mainFrame.Navigate(new AuthorizationPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new AuthorizationPage());
        }
    }
}