using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VetClinic.AppData;

namespace VetClinic.Pages
{
    public partial class AddAppointmentPage : Page
    {
        private Appointments _appointment;
        private bool _isNew;

        public AddAppointmentPage(Appointments appointment)
        {
            InitializeComponent();

            _isNew = appointment == null;
            _appointment = appointment ?? new Appointments();

            // Загрузка справочников
            cbClient.ItemsSource = AppConnect.model0db.Users
                .Where(u => u.RoleId == 3) // только клиенты
                .ToList();

            cbVet.ItemsSource = AppConnect.model0db.Veterinarians.ToList();

            cbService.ItemsSource = AppConnect.model0db.Services.ToList();

            cbStatus.ItemsSource = AppConnect.model0db.AppointmentStatuses.ToList();

            if (_isNew)
            {
                tbHeader.Text = "Новая запись на приём";
                dpDate.SelectedDate = DateTime.Now.AddDays(1);
                cbStatus.SelectedIndex = 0; // Запланирована
            }
            else
            {
                tbHeader.Text = $"Редактирование записи #{_appointment.Id}";
                dpDate.SelectedDate = _appointment.AppointmentDate.Date;
                txtTime.Text = _appointment.AppointmentDate.ToString("HH:mm");

                // Заполняем поля
                int? ownerId = _appointment.Pets?.OwnerId;
                cbClient.SelectedValue = ownerId;

                if (ownerId.HasValue)
                {
                    cbPet.ItemsSource = AppConnect.model0db.Pets
                        .Where(p => p.OwnerId == ownerId.Value)
                        .ToList();
                }
                cbPet.SelectedValue = _appointment.PetId;

                cbVet.SelectedValue = _appointment.VeterinarianId;
                cbService.SelectedValue = _appointment.ServiceId;
                cbStatus.SelectedValue = _appointment.StatusId;
                txtDescription.Text = _appointment.Description;

                cbClient.IsEnabled = false; // клиента не меняем
            }
        }

        private void cbClient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbClient.SelectedValue == null) return;

            int clientId = (int)cbClient.SelectedValue;
            cbPet.ItemsSource = AppConnect.model0db.Pets
                .Where(p => p.OwnerId == clientId)
                .ToList();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (cbClient.SelectedValue == null)
            {
                MessageBox.Show("Выберите клиента.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cbPet.SelectedValue == null)
            {
                MessageBox.Show("Выберите питомца.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cbVet.SelectedValue == null)
            {
                MessageBox.Show("Выберите ветеринара.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cbService.SelectedValue == null)
            {
                MessageBox.Show("Выберите услугу.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cbStatus.SelectedValue == null)
            {
                MessageBox.Show("Выберите статус.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!dpDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TimeSpan.TryParse(txtTime.Text, out TimeSpan time))
            {
                MessageBox.Show("Введите корректное время (например, 10:00).", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _appointment.PetId = (int)cbPet.SelectedValue;
                _appointment.VeterinarianId = (int)cbVet.SelectedValue;
                _appointment.ServiceId = (int)cbService.SelectedValue;
                _appointment.AppointmentDate = dpDate.SelectedDate.Value.Date + time;
                _appointment.StatusId = (int)cbStatus.SelectedValue;
                _appointment.Description = string.IsNullOrWhiteSpace(txtDescription.Text)
                    ? null
                    : txtDescription.Text.Trim();

                if (_isNew)
                {
                    _appointment.CreatedDate = DateTime.Now;
                    AppConnect.model0db.Appointments.Add(_appointment);
                }

                AppConnect.model0db.SaveChanges();

                MessageBox.Show(_isNew ? "Запись создана!" : "Изменения сохранены!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                AppFrame.mainFrame.Navigate(new AppointmentsPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new AppointmentsPage());
        }
    }
}