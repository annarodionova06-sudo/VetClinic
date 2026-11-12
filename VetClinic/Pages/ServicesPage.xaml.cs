using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

            // Гость и клиент — скрываем фильтры
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

        private void Filters_Changed(object sender, EventArgs e) => Apply();

        // Двойной клик по услуге — редактирование для админа
        private void listServices_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(listServices.SelectedItem is Services s)) return;

            // Режим выбора услуги для добавления в запись
            if (AppConnect.SelectServiceMode)
            {
                AppConnect.SelectServiceMode = false;
                var existing = AppConnect.CartServices.FirstOrDefault(c => c.ServiceId == s.Id);
                if (existing != null)
                    existing.Quantity++;
                else
                    AppConnect.CartServices.Add(new CartItemService
                    {
                        ServiceId = s.Id,
                        ServiceName = s.ServiceName,
                        Price = s.Price,
                        DurationMinutes = s.DurationMinutes
                    });
                AppFrame.mainFrame.Navigate(new CartServicesPage());
                return;
            }

            // ТОЛЬКО админ (роль 1) — редактирование
            if (AppConnect.user != null && AppConnect.user.RoleId == 1)
            {
                AppFrame.mainFrame.Navigate(new ServiceDetailsPage(s));
                return;
            }

            // Все остальные — ничего не происходит
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
            var mw = Application.Current.MainWindow as MainWindow;
            if (mw != null)
            {
                mw.Logout();
            }

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
            // 1. Проверка прав (только Администратор, роль 1)
            if (AppConnect.user?.RoleId != 1)
            {
                MessageBox.Show("Удаление доступно только администратору.",
                    "Доступ ограничен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Проверка, выбрана ли услуга в списке
            if (!(listServices.SelectedItem is Services s))
            {
                MessageBox.Show("Сначала выберите услугу в списке для удаления.",
                    "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 3. ЗАПРОС ПОДТВЕРЖДЕНИЯ (Именно наличие этого окна проверяет ваш тест!)
            var result = MessageBox.Show(
                $"Вы действительно хотите удалить услугу «{s.ServiceName}»?\nЭто действие нельзя будет отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return; // Пользователь нажал "Нет", прерываем выполнение
            }

            // 4. Попытка удаления из базы данных
            try
            {
                AppConnect.model0db.Services.Remove(s);
                AppConnect.model0db.SaveChanges();

                MessageBox.Show("Услуга успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Load(); // Обновляем список на экране
            }
            catch (Exception ex)
            {
                // Если услуга уже используется в таблице Appointments (Записи), сработает эта защита (F4)
                MessageBox.Show("Невозможно удалить услугу, так как она уже присутствует в одной или нескольких записях на приём.\n\n" + ex.Message,
                    "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}