using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VetClinic.AppData
{
    internal class AppConnect
    {
        public static VetClinicEntities model0db;
        public static Users user;

        // Корзины
        public static List<CartItemMedicine> CartMedicines { get; } = new List<CartItemMedicine>();
        public static List<CartItemService> CartServices { get; } = new List<CartItemService>();

        // Режим выбора для добавления в заказ
        public static bool SelectServiceMode;
        public static bool SelectMedicineMode;
        public static Appointments EditingAppointment;
        public static Orders EditingOrder;
    }

    // Расширения для EF-классов
    public partial class Users
    {
        public string FIO => $"{Surname} {FirstName} {Patronymic}".Trim();
    }
    public partial class Veterinarians
    {
        public string FIO => $"{Surname} {FirstName} {Patronymic}".Trim();
    }

    public partial class Services
    {
        // Выделение услуг дороже 1000 рублей
        public Brush RowColor => Price > 1000
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8DEF5"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));

        public string PriceHighlight => Price > 1000 ? "Bold" : "Normal";
    }

    public partial class Medicines
    {
        public string QuantityText => StockQuantity > 5 ? "много" : "мало";

        public Brush RowColor => StockQuantity <= 3
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFB3B3"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8DEF5"));

        public string ImagePathDisplay => string.IsNullOrEmpty(ImagePath)
            ? "/Images/picture.png"
            : $"/Images/{ImagePath}";

        // Расчёт старой цены, если есть скидка
        public decimal? OldPrice => DiscountPercent.HasValue && DiscountPercent.Value > 0
            ? Math.Round(Price / (1 - DiscountPercent.Value / 100m), 2)
            : (decimal?)null;

        public decimal FinalPrice => DiscountPercent.HasValue && DiscountPercent.Value > 0
            ? Math.Round(Price * (1 - DiscountPercent.Value / 100m), 2)
            : Price;
    }

    public partial class OrderItems
    {
        public decimal Sum => (decimal)(Quantity * PricePerUnit);
    }

    public partial class Orders
    {
        public string ClientFIO => Users?.FIO ?? "";
        public decimal TotalSum => OrderItems != null ? OrderItems.Sum(i => i.Sum) : 0;
    }

    public partial class Appointments
    {
        public string PetInfo => Pets != null ? $"{Pets.Name} ({Pets.Breeds?.BreedName})" : "";
        public string VetName => Veterinarians != null ? Veterinarians.FIO : "";
        public string ServiceName => Services?.ServiceName ?? "";
        public string StatusName => AppointmentStatuses?.StatusName ?? "";
    }

    // Элементы корзин
    public class CartItemMedicine
    {
        public int MedicineId { get; set; }
        public string Name { get; set; }
        public string ManufacturerName { get; set; }
        public int Quantity { get; set; }
        public decimal PricePerUnit { get; set; }
        public decimal Sum => Quantity * PricePerUnit;
    }

    public class CartItemService
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal Sum => Quantity * Price;
    }
}
