using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VetClinic.AppData;

namespace VetClinic.Pages
{
    public partial class MedicineDetailsPage : Page
    {
        private Medicines _medicine;
        private bool _isNew;

        public MedicineDetailsPage(Medicines medicine)
        {
            InitializeComponent();
            _isNew = medicine == null;
            _medicine = medicine ?? new Medicines();

            cmbCategory.ItemsSource = AppConnect.model0db.MedicineCategories.ToList();
            cmbManufacturer.ItemsSource = AppConnect.model0db.Manufacturers.ToList();

            if (!_isNew)
            {
                txtArticle.Text = _medicine.Article;
                txtName.Text = _medicine.Name;
                cmbCategory.SelectedValue = _medicine.CategoryId;
                cmbManufacturer.SelectedValue = _medicine.ManufacturerId;
                txtReleaseForm.Text = _medicine.ReleaseForm;
                txtPrice.Text = _medicine.Price.ToString();
                txtDiscount.Text = _medicine.DiscountPercent?.ToString() ?? "0";
                txtStock.Text = _medicine.StockQuantity.ToString();
                txtDescription.Text = _medicine.Description;
                chkPrescription.IsChecked = _medicine.RequiresPrescription;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtArticle.Text))
            {
                MessageBox.Show("Введите артикул.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbCategory.SelectedValue == null)
            {
                MessageBox.Show("Выберите категорию.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbManufacturer.SelectedValue == null)
            {
                MessageBox.Show("Выберите производителя.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtReleaseForm.Text))
            {
                MessageBox.Show("Введите форму выпуска.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Введите корректную цену.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtDiscount.Text, out int discount) || discount < 0 || discount > 100)
            {
                MessageBox.Show("Скидка должна быть от 0 до 100%.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Введите корректное количество.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _medicine.Article = txtArticle.Text.Trim();
                _medicine.Name = txtName.Text.Trim();
                _medicine.CategoryId = (int)cmbCategory.SelectedValue;
                _medicine.ManufacturerId = (int)cmbManufacturer.SelectedValue;
                _medicine.ReleaseForm = txtReleaseForm.Text.Trim();
                _medicine.Price = price;
                _medicine.DiscountPercent = discount;
                _medicine.StockQuantity = stock;
                _medicine.Description = string.IsNullOrWhiteSpace(txtDescription.Text)
                    ? null
                    : txtDescription.Text.Trim();
                _medicine.RequiresPrescription = chkPrescription.IsChecked ?? false;

                if (_isNew)
                {
                    AppConnect.model0db.Medicines.Add(_medicine);
                }
                AppConnect.model0db.SaveChanges();

                MessageBox.Show(_isNew ? "Лекарство добавлено!" : "Изменения сохранены!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                AppFrame.mainFrame.Navigate(new MedicinesPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new MedicinesPage());
        }
    }
}