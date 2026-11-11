using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VetClinic.AppData;

namespace VetClinic.Pages
{
    public partial class AddPetPage : Page
    {
        private bool _autoCheckout; // флаг: после сохранения сразу оформить запись

        public AddPetPage(bool autoCheckout = false)
        {
            InitializeComponent();
            _autoCheckout = autoCheckout;

            cmbAnimalType.ItemsSource = AppConnect.model0db.AnimalTypes.ToList();
            cmbGender.ItemsSource = AppConnect.model0db.Genders.ToList();
        }

        private void cmbAnimalType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbAnimalType.SelectedValue == null) return;

            int typeId = (int)cmbAnimalType.SelectedValue;
            cmbBreed.ItemsSource = AppConnect.model0db.Breeds
                .Where(b => b.AnimalTypeId == typeId)
                .ToList();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите имя питомца.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbAnimalType.SelectedValue == null)
            {
                MessageBox.Show("Выберите тип животного.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbBreed.SelectedValue == null)
            {
                MessageBox.Show("Выберите породу.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var pet = new Pets
                {
                    Name = txtName.Text.Trim(),
                    BreedId = (int)cmbBreed.SelectedValue,
                    OwnerId = AppConnect.user.Id,
                    BirthDate = dpBirth.SelectedDate,
                    GenderId = cmbGender.SelectedValue != null ? (int?)cmbGender.SelectedValue : null,
                    Weight = decimal.TryParse(txtWeight.Text, out decimal w) ? w : (decimal?)null,
                    Notes = string.IsNullOrWhiteSpace(txtNotes.Text) ? null : txtNotes.Text.Trim()
                };

                AppConnect.model0db.Pets.Add(pet);
                AppConnect.model0db.SaveChanges();

                MessageBox.Show($"Питомец «{pet.Name}» добавлен!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                if (_autoCheckout)
                {
                    // Возвращаемся в корзину для оформления записи
                    AppFrame.mainFrame.Navigate(new CartServicesPage());
                }
                else
                {
                    AppFrame.mainFrame.Navigate(new ServicesPage());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (_autoCheckout)
                AppFrame.mainFrame.Navigate(new CartServicesPage());
            else
                AppFrame.mainFrame.Navigate(new ServicesPage());
        }
    }
}