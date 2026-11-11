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
    public partial class CartMedicinesPage : Page
    {
        public CartMedicinesPage()
        {
            InitializeComponent();
            listCart.ItemsSource = AppConnect.CartMedicines;
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal total = AppConnect.CartMedicines.Sum(i => i.Sum);
            tbTotal.Text = $"Итого: {total:N2} ₽";
        }

        private void Qty_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.DataContext is CartItemMedicine item)
            {
                if (!int.TryParse(tb.Text, out int q) || q <= 0)
                {
                    MessageBox.Show("Количество должно быть целым числом > 0.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tb.Text = item.Quantity.ToString();
                    return;
                }

                var med = AppConnect.model0db.Medicines.Find(item.MedicineId);
                if (med != null && q > med.StockQuantity)
                {
                    MessageBox.Show($"Доступно только {med.StockQuantity} шт.",
                        "Недостаточно товара", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tb.Text = item.Quantity.ToString();
                    return;
                }

                item.Quantity = q;
                listCart.Items.Refresh();
                UpdateTotal();
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.Tag is CartItemMedicine item)
            {
                AppConnect.CartMedicines.Remove(item);
                listCart.Items.Refresh();
                UpdateTotal();
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.CartMedicines.Count == 0) return;
            if (MessageBox.Show("Очистить корзину?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;
            AppConnect.CartMedicines.Clear();
            listCart.Items.Refresh();
            UpdateTotal();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
            => AppFrame.mainFrame.Navigate(new MedicinesPage());

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.CartMedicines.Count == 0)
            {
                MessageBox.Show("Корзина пуста.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                int maxNum = AppConnect.model0db.Orders.Any()
                ? AppConnect.model0db.Orders.Max(o => o.OrderNumber)
                : 0;

                var order = new Orders
                {
                    OrderNumber = maxNum + 1,
                    OrderDate = DateTime.Now,
                    UserId = AppConnect.user.Id,
                    StatusId = 1
                };
                AppConnect.model0db.Orders.Add(order);
                AppConnect.model0db.SaveChanges();

                foreach (var ci in AppConnect.CartMedicines)
                {
                    var med = AppConnect.model0db.Medicines.Find(ci.MedicineId);
                    if (med == null || med.StockQuantity < ci.Quantity)
                    {
                        MessageBox.Show($"Недостаточно «{ci.Name}».",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    AppConnect.model0db.OrderItems.Add(new OrderItems
                    {
                        OrderId = order.Id,
                        MedicineId = ci.MedicineId,
                        Quantity = ci.Quantity,
                        PricePerUnit = ci.PricePerUnit
                    });

                    med.StockQuantity -= ci.Quantity;
                }

                AppConnect.model0db.SaveChanges();
                AppConnect.CartMedicines.Clear();

                MessageBox.Show($"Заказ №{order.OrderNumber} оформлен!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                AppFrame.mainFrame.Navigate(new ServicesPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка оформления заказа",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
