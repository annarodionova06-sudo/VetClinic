using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VetClinic.AppData;

namespace VetClinic.Pages
{
    public partial class ServiceDetailsPage : Page
    {
        private Services _service;
        private bool _isNew;

        public ServiceDetailsPage(Services service)
        {
            InitializeComponent();
            _isNew = service == null;
            _service = service ?? new Services();

            if (!_isNew)
            {
                txtName.Text = _service.ServiceName;
                txtPrice.Text = _service.Price.ToString();
                txtDuration.Text = _service.DurationMinutes.ToString();
                txtDescription.Text = _service.Description;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название услуги.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Введите корректную цену.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtDuration.Text, out int duration) || duration < 10 || duration > 240)
            {
                MessageBox.Show("Длительность должна быть от 10 до 240 минут.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _service.ServiceName = txtName.Text.Trim();
                _service.Price = price;
                _service.DurationMinutes = duration;
                _service.Description = string.IsNullOrWhiteSpace(txtDescription.Text)
                    ? null
                    : txtDescription.Text.Trim();

                if (_isNew)
                {
                    AppConnect.model0db.Services.Add(_service);
                }

                AppConnect.model0db.SaveChanges();

                MessageBox.Show(_isNew ? "Услуга добавлена!" : "Изменения сохранены!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                AppFrame.mainFrame.Navigate(new ServicesPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new ServicesPage());
        }
    }
}