using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VetClinic.AppData;

namespace VetClinic.Pages
{
    public partial class CartServicesPage : Page
    {
        public CartServicesPage()
        {
            InitializeComponent();
            listCart.ItemsSource = AppConnect.CartServices;
            UpdateTotal();

            // Устанавливаем дату по умолчанию — завтра
            dpAppointmentDate.SelectedDate = DateTime.Now.AddDays(1);
        }

        private void UpdateTotal()
        {
            decimal total = AppConnect.CartServices.Sum(i => i.Sum);
            tbTotal.Text = $"Итого: {total:N2} ₽";
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is CartItemService item)
            {
                AppConnect.CartServices.Remove(item);
                listCart.Items.Refresh();
                UpdateTotal();
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.CartServices.Count == 0) return;
            if (MessageBox.Show("Очистить корзину?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;
            AppConnect.CartServices.Clear();
            listCart.Items.Refresh();
            UpdateTotal();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
            => AppFrame.mainFrame.Navigate(new ServicesPage());

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.CartServices.Count == 0)
            {
                MessageBox.Show("Корзина пуста.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем дату
            if (!dpAppointmentDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату приёма.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем время
            if (!TimeSpan.TryParse(txtTime.Text, out TimeSpan time))
            {
                MessageBox.Show("Введите корректное время (например, 10:00).", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем, что дата не в прошлом
            DateTime appointmentDateTime = dpAppointmentDate.SelectedDate.Value.Date + time;
            if (appointmentDateTime < DateTime.Now)
            {
                MessageBox.Show("Дата и время приёма не могут быть в прошлом.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Проверяем наличие питомцев
            var pets = AppConnect.model0db.Pets
                .Where(p => p.OwnerId == AppConnect.user.Id)
                .ToList();

            if (pets.Count == 0)
            {
                var result = MessageBox.Show(
                    "У вас нет зарегистрированных питомцев. Добавить питомца сейчас?",
                    "Нет питомцев",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    AppFrame.mainFrame.Navigate(new AddPetPage(autoCheckout: true));
                }
                return;
            }

            // Оформляем запись
            try
            {
                var firstService = AppConnect.CartServices.First();
                var vet = AppConnect.model0db.Veterinarians.FirstOrDefault();

                if (vet == null)
                {
                    MessageBox.Show("Нет доступных ветеринаров.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var appointment = new Appointments
                {
                    PetId = pets.First().Id,
                    VeterinarianId = vet.Id,
                    ServiceId = firstService.ServiceId,
                    AppointmentDate = appointmentDateTime,
                    StatusId = 1, // Запланирована
                    Description = $"Услуги: {string.Join(", ", AppConnect.CartServices.Select(s => s.ServiceName))}",
                    CreatedDate = DateTime.Now
                };

                AppConnect.model0db.Appointments.Add(appointment);
                AppConnect.model0db.SaveChanges();
                AppConnect.CartServices.Clear();

                MessageBox.Show($"Запись оформлена на {appointmentDateTime:dd.MM.yyyy HH:mm}!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                AppFrame.mainFrame.Navigate(new MyAppointmentsPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка оформления",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}