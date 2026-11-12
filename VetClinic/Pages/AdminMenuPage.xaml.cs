using System.Windows;
using System.Windows.Controls;
using VetClinic;
using VetClinic.AppData;

namespace VetClinic.Pages
{
    public partial class AdminMenuPage : Page
    {
        public AdminMenuPage()
        {
            InitializeComponent();
        }

        private void BtnAppointments_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new TableViewPage("Appointments", "📅 Записи на приём"));

        private void BtnPets_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new TableViewPage("Pets", "🐾 Питомцы"));

        private void BtnServices_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new TableViewPage("Services", "💉 Услуги"));

        private void BtnVeterinarians_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new TableViewPage("Veterinarians", "🩺 Ветеринары"));

        private void BtnMedicines_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new TableViewPage("Medicines", "💊 Лекарства"));

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new TableViewPage("Orders", "📦 Заказы"));

        private void BtnOrderItems_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new TableViewPage("OrderItems", "🧾 Состав заказов"));

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем роль через AppConnect.user
            if (AppConnect.user != null && AppConnect.user.RoleId == 1)
            {
                // Только Администратор (idRole == 1)
                AppFrame.mainFrame.Navigate(new TableViewPage("Users", "👥 Пользователи"));
            }
            else
            {
                MessageBox.Show("Доступно только Администратору.", "Ошибка доступа",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnRoles_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user.RoleId == 1)
                NavigationService?.Navigate(new TableViewPage("Roles", "🔑 Роли"));
            else
                MessageBox.Show("Доступно только Администратору.", "Ошибка доступа",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BtnBreeds_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new TableViewPage("Breeds", "🐕 Породы"));

        private void BtnAnimalTypes_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new TableViewPage("AnimalTypes", "🐾 Виды животных"));

        private void BtnSpecializations_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new TableViewPage("Specializations", "🩺 Специализации"));

        private void BtnManufacturers_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new TableViewPage("Manufacturers", "🏭 Производители"));

        private void BtnMedicineCategories_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new TableViewPage("MedicineCategories", "📂 Категории лекарств"));

        private void BtnAppointmentStatuses_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new TableViewPage("AppointmentStatuses", "📋 Статусы записей"));

        private void BtnOrderStatuses_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new TableViewPage("OrderStatuses", "📦 Статусы заказов"));

        private void BtnGenders_Click(object sender, RoutedEventArgs e)
            => NavigationService?.Navigate(new TableViewPage("Genders", "♂♀ Полы"));

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new ServicesPage());
        }
    }
}