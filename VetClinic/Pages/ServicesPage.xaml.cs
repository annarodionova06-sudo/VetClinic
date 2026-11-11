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
    public partial class ServicesPage : Page
    {
        private List<Services> _all = new List<Services>();

        public ServicesPage()
{
    InitializeComponent();

    var mw = Application.Current.MainWindow as MainWindow;
    mw.fio.Text = AppConnect.user?.FIO ?? "Гость";

    // Гость и клиент — только просмотр
    if (AppConnect.user == null || AppConnect.user.RoleId == 3)
        filterPanel.Visibility = Visibility.Collapsed;

    // Кнопка "Удалить" только для админа
    if (AppConnect.user?.RoleId == 1)
        btnDeleteService.Visibility = Visibility.Visible;

    Load();
}

        private void Load()
        {
            _all = AppConnect.model0db.Services.ToList();

            cmbSort.Items.Clear();
            cmbSort.Items.Add("Без сортировки");
            cmbSort.Items.Add("Цена ↑");
            cmbSort.Items.Add("Цена ↓");
            cmbSort.Items.Add("Длительность ↑");
            cmbSort.SelectedIndex = 0;

            Apply();
        }

        private void Apply()
        {
            var list = _all.AsEnumerable();

            string q = txtSearch.Text.Trim().ToLower();
            if (q.Length > 0)
                list = list.Where(s =>
                    s.ServiceName.ToLower().Contains(q) ||
                    (s.Description ?? "").ToLower().Contains(q));

            if (cmbSort.SelectedIndex == 1)
                list = list.OrderBy(s => s.Price);
            else if (cmbSort.SelectedIndex == 2)
                list = list.OrderByDescending(s => s.Price);
            else if (cmbSort.SelectedIndex == 3)
                list = list.OrderBy(s => s.DurationMinutes);

            listServices.ItemsSource = list.ToList();
        }

        private void Filters_Changed(object sender, System.EventArgs e) => Apply();

        private void listMedicines_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(listServices.SelectedItem is Medicines m)) return;

            // Режим выбора лекарства для добавления в заказ
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

            // Админ и Ветеринар — редактирование
            if (AppConnect.user != null && (AppConnect.user.RoleId == 1 || AppConnect.user.RoleId == 2))
            {
                AppFrame.mainFrame.Navigate(new MedicineDetailsPage(m));
                return;
            }

            // Клиент и гость — просмотр
            AppFrame.mainFrame.Navigate(new MedicineDetailsPage(m));
        }

        private void Medicines_Click(object sender, RoutedEventArgs e)
            => AppFrame.mainFrame.Navigate(new MedicinesPage());

        private void CartServices_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user == null)
            {
                MessageBox.Show("Авторизуйтесь.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            AppFrame.mainFrame.Navigate(new CartServicesPage());
        }

        private void CartMedicines_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user == null)
            {
                MessageBox.Show("Авторизуйтесь.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            AppFrame.mainFrame.Navigate(new CartMedicinesPage());
        }

        private void Appointments_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user == null)
            {
                MessageBox.Show("Авторизуйтесь.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            AppFrame.mainFrame.Navigate(new MyAppointmentsPage());
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user == null)
            {
                MessageBox.Show("Авторизуйтесь.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Клиент (роль 3) — свои заказы
            if (AppConnect.user.RoleId == 3)
            {
                AppFrame.mainFrame.Navigate(new MyOrdersPage());
                return;
            }

            // Менеджер и админ — все заказы
            AppFrame.mainFrame.Navigate(new MyOrdersPage());
        }


        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            AppConnect.user = null;
            AppConnect.CartServices.Clear();
            AppConnect.CartMedicines.Clear();
            AppFrame.mainFrame.Navigate(new AuthorizationPage());
        }

        private void AddServiceToCart_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user == null)
            {
                MessageBox.Show("Авторизуйтесь для добавления в корзину.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!(sender is Button btn) || !(btn.Tag is Services s)) return;

            var existing = AppConnect.CartServices.FirstOrDefault(c => c.ServiceId == s.Id);
            if (existing != null)
            {
                MessageBox.Show("Эта услуга уже в корзине.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            AppConnect.CartServices.Add(new CartItemService
            {
                ServiceId = s.Id,
                ServiceName = s.ServiceName,
                Price = s.Price,
                DurationMinutes = s.DurationMinutes
            });

            MessageBox.Show($"«{s.ServiceName}» добавлена в корзину!", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Del_Click(object sender, RoutedEventArgs e)
        {

        }

        private void listServices_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(listServices.SelectedItem is Medicines m)) return;

            // Режим выбора лекарства для добавления в заказ
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

            // Админ и Ветеринар — редактирование
            if (AppConnect.user != null && (AppConnect.user.RoleId == 1 || AppConnect.user.RoleId == 2))
            {
                AppFrame.mainFrame.Navigate(new MedicineDetailsPage(m));
                return;
            }

            // Клиент и гость — просмотр
            AppFrame.mainFrame.Navigate(new MedicineDetailsPage(m));
        }
    }
}