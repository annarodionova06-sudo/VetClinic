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
using System.Data.Entity;
using VetClinic.AppData;

namespace VetClinic.Pages
{
    public partial class MyAppointmentsPage : Page
    {
        public MyAppointmentsPage()
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            var pets = AppConnect.model0db.Pets
                .Where(p => p.OwnerId == AppConnect.user.Id)
                .Select(p => p.Id)
                .ToList();

            listAppointments.ItemsSource = AppConnect.model0db.Appointments
                .Where(a => pets.Contains(a.PetId))
                .Include("Pets")
                .Include("Pets.Breeds")
                .Include("Veterinarians")
                .Include("Services")
                .Include("AppointmentStatuses")
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (!(listAppointments.SelectedItem is Appointments a))
            {
                MessageBox.Show("Выберите запись.", "Уведомление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show($"Отменить запись на {a.AppointmentDate:dd.MM.yyyy HH:mm}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question)
                != MessageBoxResult.Yes)
                return;

            try
            {
                a.StatusId = 5; // Отменена
                AppConnect.model0db.SaveChanges();
                Load();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
            => AppFrame.mainFrame.Navigate(new ServicesPage());
    }
}