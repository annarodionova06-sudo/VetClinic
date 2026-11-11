using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VetClinic.AppData;

namespace VetClinic.Pages
{
    public partial class MyOrdersPage : Page
    {
        public MyOrdersPage()
        {
            InitializeComponent();
            Load();
        }

        private void Load()
        {
            listOrders.ItemsSource = AppConnect.model0db.Orders
                .Where(o => o.UserId == AppConnect.user.Id)
                .Include("OrderItems")
                .Include("OrderItems.Medicines")
                .Include("OrderStatuses")
                .OrderByDescending(o => o.OrderDate)
                .ToList();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (!(listOrders.SelectedItem is Orders order))
            {
                MessageBox.Show("Выберите заказ.", "Уведомление",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Проверяем статус — можно отменить только "Новый" (1) или "В сборке" (2)
            if (order.StatusId != 1 && order.StatusId != 2)
            {
                MessageBox.Show("Можно отменить только заказы со статусом 'Новый' или 'В сборке'.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Отменить заказ №{order.OrderNumber}?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question)
                != MessageBoxResult.Yes)
                return;

            try
            {
                // Возвращаем остатки лекарств на склад
                foreach (var item in order.OrderItems.ToList())
                {
                    var medicine = AppConnect.model0db.Medicines.Find(item.MedicineId);
                    if (medicine != null)
                    {
                        medicine.StockQuantity += item.Quantity;
                    }
                }

                // Меняем статус на "Отменён" (Id = 5)
                order.StatusId = 5;
                AppConnect.model0db.SaveChanges();

                MessageBox.Show($"Заказ №{order.OrderNumber} отменён.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отмене заказа: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
            => AppFrame.mainFrame.Navigate(new ServicesPage());
    }
}