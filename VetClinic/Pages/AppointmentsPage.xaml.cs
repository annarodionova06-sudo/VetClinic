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
    public partial class AppointmentsPage : Page
    {
        private List<Appointments> _all = new List<Appointments>();

        public AppointmentsPage()
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            _all = AppConnect.model0db.Appointments
                .Include("Pets")
                .Include("Pets.Breeds")
                .Include("Veterinarians")
                .Include("Services")
                .Include("AppointmentStatuses")
                .ToList();

            cmbSort.Items.Clear();
            cmbSort.Items.Add("Без сортировки");
            cmbSort.Items.Add("Дата ↑");
            cmbSort.Items.Add("Дата ↓");
            cmbSort.SelectedIndex = 0;

            cmbFilter.Items.Clear();
            cmbFilter.Items.Add("Все статусы");
            foreach (var s in AppConnect.model0db.AppointmentStatuses.ToList())
                cmbFilter.Items.Add(s.StatusName);
            cmbFilter.SelectedIndex = 0;

            Apply();
        }

        private void Apply()
        {
            var list = _all.AsEnumerable();

            string q = txtSearch.Text.Trim().ToLower();
            if (q.Length > 0)
            {
                list = list.Where(a =>
                    (a.Pets?.Name ?? "").ToLower().Contains(q) ||
                    (a.Services?.ServiceName ?? "").ToLower().Contains(q) ||
                    (a.Veterinarians?.Surname ?? "").ToLower().Contains(q));
            }

            if (cmbFilter.SelectedIndex > 0)
            {
                string status = cmbFilter.SelectedItem.ToString();
                list = list.Where(a => a.AppointmentStatuses?.StatusName == status);
            }

            if (cmbSort.SelectedIndex == 1)
                list = list.OrderBy(a => a.AppointmentDate);
            else if (cmbSort.SelectedIndex == 2)
                list = list.OrderByDescending(a => a.AppointmentDate);

            listAppointments.ItemsSource = list.ToList();
        }

        private void Filters_Changed(object sender, EventArgs e) => Apply();

        private void listAppointments_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => Edit_Click(sender, e);

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user?.RoleId != 1 && AppConnect.user?.RoleId != 2)
            {
                MessageBox.Show("Доступ ограничен.", "Уведомление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            AppFrame.mainFrame.Navigate(new AddAppointmentPage(null));
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user?.RoleId != 1)
            {
                MessageBox.Show("Редактирование доступно только администратору.",
                    "Доступ ограничен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!(listAppointments.SelectedItem is Appointments a))
            {
                MessageBox.Show("Выберите запись.", "Уведомление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            AppFrame.mainFrame.Navigate(new AddAppointmentPage(a));
        }

        private void Del_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user?.RoleId != 1)
            {
                MessageBox.Show("Удаление доступно только администратору.",
                    "Доступ ограничен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!(listAppointments.SelectedItem is Appointments a))
            {
                MessageBox.Show("Выберите запись.", "Уведомление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (MessageBox.Show($"Удалить запись от {a.AppointmentDate:dd.MM.yyyy HH:mm}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question)
                != MessageBoxResult.Yes)
                return;

            try
            {
                AppConnect.model0db.Appointments.Remove(a);
                AppConnect.model0db.SaveChanges();
                Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Невозможно удалить запись, так как она используется в связанных таблицах.\n\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
            => AppFrame.mainFrame.Navigate(new ServicesPage());
    }
}