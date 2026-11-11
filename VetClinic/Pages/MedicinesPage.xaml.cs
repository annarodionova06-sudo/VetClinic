using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VetClinic.AppData;

namespace VetClinic.Pages
{
    public partial class MedicinesPage : Page
    {
        private List<Medicines> _all = new List<Medicines>();

        public MedicinesPage()
        {
            InitializeComponent();

            var mw = Application.Current.MainWindow as MainWindow;
            mw.fio.Text = AppConnect.user?.FIO ?? "Гость";

            // Гость и клиент (роль 3) — скрываем фильтры
            if (AppConnect.user == null || AppConnect.user.RoleId == 3)
            {
                filterPanel.Visibility = Visibility.Collapsed;
            }

            // Кнопка "Удалить" видна ТОЛЬКО администратору (роль 1)
            if (AppConnect.user != null && AppConnect.user.RoleId == 1)
            {
                btnDelete.Visibility = Visibility.Visible;
            }

            Load();
        }

        private void Load()
        {
            _all = AppConnect.model0db.Medicines
                .Include("Manufacturers")
                .Include("MedicineCategories")
                .ToList();

            cmbSort.Items.Clear();
            cmbSort.Items.Add("Без сортировки");
            cmbSort.Items.Add("Цена ↑");
            cmbSort.Items.Add("Цена ↓");
            cmbSort.SelectedIndex = 0;

            cmbFilter.Items.Clear();
            cmbFilter.Items.Add("Все категории");
            foreach (var c in AppConnect.model0db.MedicineCategories.ToList())
                cmbFilter.Items.Add(c.CategoryName);
            cmbFilter.SelectedIndex = 0;

            Apply();
        }

        private void Apply()
        {
            var list = _all.AsEnumerable();

            string q = txtSearch.Text.Trim().ToLower();
            if (q.Length > 0)
                list = list.Where(m => m.Name.ToLower().Contains(q));

            if (cmbFilter.SelectedIndex > 0)
            {
                string cat = cmbFilter.SelectedItem.ToString();
                list = list.Where(m => m.MedicineCategories.CategoryName == cat);
            }

            if (cmbSort.SelectedIndex == 1)
                list = list.OrderBy(m => m.FinalPrice);
            else if (cmbSort.SelectedIndex == 2)
                list = list.OrderByDescending(m => m.FinalPrice);

            listMedicines.ItemsSource = list.ToList();
        }

        private void Filters_Changed(object sender, EventArgs e) => Apply();

        private void listMedicines_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(listMedicines.SelectedItem is Medicines m)) return;

            // 1. Режим выбора для добавления в существующий заказ (из AddOrderPage)
            if (AppConnect.SelectMedicineMode)
            {
                AppConnect.SelectMedicineMode = false;
                var existing = AppConnect.CartMedicines.FirstOrDefault(c => c.MedicineId == m.Id);
                if (existing != null)
                    existing.Quantity++;
                else
                    AppConnect.CartMedicines.Add(new CartItemMedicine
                    {
                        MedicineId = m.Id,
                        Name = m.Name,
                        ManufacturerName = m.Manufacturers?.ManufacturerName ?? "",
                        PricePerUnit = m.FinalPrice,
                        Quantity = 1
                    });
                AppFrame.mainFrame.Navigate(new CartMedicinesPage());
                return;
            }

            // 2. ТОЛЬКО АДМИН (Роль 1) может редактировать по двойному клику
            if (AppConnect.user != null && AppConnect.user.RoleId == 1)
            {
                AppFrame.mainFrame.Navigate(new MedicineDetailsPage(m));
                return;
            }

            // 3. ВСЕ ОСТАЛЬНЫЕ (Клиент, Ветеринар, Гость) — НИЧЕГО НЕ ДЕЛАЕМ при двойном клике.
            // Они добавляют товары в корзину только через кнопку "В корзину" в карточке.
        }

        private void Del_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user?.RoleId != 1)
            {
                MessageBox.Show("Удаление доступно только администратору.",
                    "Доступ ограничен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!(listMedicines.SelectedItem is Medicines m)) return;

            if (MessageBox.Show($"Удалить «{m.Name}»?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                AppConnect.model0db.Medicines.Remove(m);
                AppConnect.model0db.SaveChanges();
                Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Невозможно удалить лекарство, так как оно присутствует в заказах.\n\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cart_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user == null)
            {
                MessageBox.Show("Авторизуйтесь.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            AppFrame.mainFrame.Navigate(new CartMedicinesPage());
        }

        private void AddMedicineToCart_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user == null)
            {
                MessageBox.Show("Авторизуйтесь для добавления в корзину.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!(sender is Button btn) || !(btn.Tag is Medicines m)) return;

            if (m.StockQuantity <= 0)
            {
                MessageBox.Show("Товара нет в наличии.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existing = AppConnect.CartMedicines.FirstOrDefault(c => c.MedicineId == m.Id);
            if (existing != null)
            {
                if (existing.Quantity + 1 > m.StockQuantity)
                {
                    MessageBox.Show($"Доступно только {m.StockQuantity} шт.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                existing.Quantity++;
            }
            else
            {
                AppConnect.CartMedicines.Add(new CartItemMedicine
                {
                    MedicineId = m.Id,
                    Name = m.Name,
                    ManufacturerName = m.Manufacturers?.ManufacturerName ?? "",
                    PricePerUnit = m.FinalPrice,
                    Quantity = 1
                });
            }

            MessageBox.Show($"«{m.Name}» добавлен в корзину!", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
            => AppFrame.mainFrame.Navigate(new ServicesPage());
    }
}