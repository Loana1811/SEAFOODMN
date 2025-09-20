using ASM_GROUP3.Models;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ASM_GROUP3.ViewModel
{
    public class ImportOrderViewModel : INotifyPropertyChanged
    {
        private readonly SeafoodManagementContext _context = new();

        public ObservableCollection<ImportOrder> ImportOrders { get; set; }
        public ObservableCollection<ImportOrderDetail> OrderDetails { get; set; }
        public ObservableCollection<Seafood> Seafoods { get; set; }
        public ObservableCollection<Supplier> Suppliers { get; set; }

        private ImportOrder? _selectedOrder;
        public ImportOrder? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged(nameof(SelectedOrder));
                LoadOrderDetails();
                OnPropertyChanged(nameof(SelectedSupplier));
            }
        }

        public Supplier? SelectedSupplier
        {
            get => SelectedOrder?.Supplier;
            set
            {
                if (SelectedOrder != null)
                {
                    SelectedOrder.Supplier = value;
                    SelectedOrder.SupplierId = value?.SupplierId;
                    OnPropertyChanged(nameof(SelectedSupplier));
                }
            }
        }

        private ImportOrderDetail? _selectedDetail;
        public ImportOrderDetail? SelectedDetail
        {
            get => _selectedDetail;
            set
            {
                _selectedDetail = value;
                OnPropertyChanged(nameof(SelectedDetail));
            }
        }

        private Seafood? _selectedSeafood;
        public Seafood? SelectedSeafood
        {
            get => _selectedSeafood;
            set
            {
                _selectedSeafood = value;
                OnPropertyChanged(nameof(SelectedSeafood));
                CalculateSubtotal();
            }
        }

        private decimal _quantity = 1;
        public decimal Quantity
        {
            get => _quantity;
            set
            {
                if (value >= 0)
                {
                    _quantity = value;
                    OnPropertyChanged(nameof(Quantity));
                    CalculateSubtotal();
                }
            }
        }

        private decimal _unitPrice = 0;
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (value >= 0)
                {
                    _unitPrice = value;
                    OnPropertyChanged(nameof(UnitPrice));
                    CalculateSubtotal();
                }
            }
        }

        private decimal _subtotal;
        public decimal Subtotal
        {
            get => _subtotal;
            set
            {
                _subtotal = value;
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public ICommand AddOrderCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand SaveOrderCommand { get; }
        public ICommand AddDetailCommand { get; }
        public ICommand DeleteDetailCommand { get; }
        public ICommand ExportCsvCommand => new RelayCommand(ExportToCsv, () => SelectedOrder != null);

        public ImportOrderViewModel()
        {
            ImportOrders = new ObservableCollection<ImportOrder>(_context.ImportOrders.ToList());
            Seafoods = new ObservableCollection<Seafood>(_context.Seafoods.ToList());
            Suppliers = new ObservableCollection<Supplier>(_context.Suppliers.Where(s => s.IsActive == true).ToList());


            OrderDetails = new ObservableCollection<ImportOrderDetail>();

            AddOrderCommand = new RelayCommand(AddOrder);
            DeleteOrderCommand = new RelayCommand(DeleteOrder, () => SelectedOrder != null);
            SaveOrderCommand = new RelayCommand(SaveImportOrder, () => SelectedOrder != null);
            AddDetailCommand = new RelayCommand(AddDetail, () => SelectedOrder != null && SelectedSeafood != null);
            DeleteDetailCommand = new RelayCommand(DeleteDetail, () => SelectedDetail != null);
        }

        private void AddOrder()
        {
            var newOrder = new ImportOrder
            {
                OrderDate = DateTime.Now,
                SupplierId = Suppliers.FirstOrDefault()?.SupplierId,
                TotalAmount = 0
            };
            _context.ImportOrders.Add(newOrder);
            _context.SaveChanges();
            ImportOrders.Add(newOrder);
            SelectedOrder = newOrder;
        }

        private void DeleteOrder()
        {
            if (SelectedOrder == null) return;

            var details = _context.ImportOrderDetails.Where(d => d.OrderId == SelectedOrder.OrderId).ToList();
            foreach (var detail in details)
                _context.ImportOrderDetails.Remove(detail);

            _context.ImportOrders.Remove(SelectedOrder);
            _context.SaveChanges();

            ImportOrders.Remove(SelectedOrder);
            OrderDetails.Clear();
            SelectedOrder = null;
        }

        private void AddDetail()
        {
            if (SelectedOrder == null || SelectedSeafood == null)
            {
                MessageBox.Show("Vui lòng chọn hải sản.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Quantity <= 0 || UnitPrice <= 0)
            {
                MessageBox.Show("Số lượng và đơn giá phải lớn hơn 0.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool exists = OrderDetails.Any(d => d.SeafoodId == SelectedSeafood.SeafoodId);
            if (exists)
            {
                MessageBox.Show("Hải sản này đã có trong chi tiết. Vui lòng chỉnh sửa thay vì thêm mới.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var detail = new ImportOrderDetail
            {
                OrderId = SelectedOrder.OrderId,
                SeafoodId = SelectedSeafood.SeafoodId,
                Quantity = Quantity,
                UnitPrice = UnitPrice
            };

            _context.ImportOrderDetails.Add(detail);
            _context.SaveChanges();
            OrderDetails.Add(detail);
            UpdateTotalAmount();

            Quantity = 1;
            UnitPrice = 0;
            SelectedSeafood = null;
        }

        private void DeleteDetail()
        {
            if (SelectedDetail == null) return;

            _context.ImportOrderDetails.Remove(SelectedDetail);
            _context.SaveChanges();
            OrderDetails.Remove(SelectedDetail);
            UpdateTotalAmount();
        }

        private void SaveImportOrder()
        {
            if (SelectedOrder == null || SelectedOrder.SupplierId == null || !OrderDetails.Any())
            {
                MessageBox.Show("Vui lòng chọn nhà cung cấp và thêm ít nhất một mặt hàng.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedOrder.TotalAmount = OrderDetails.Sum(d => d.Quantity * d.UnitPrice);
            _context.SaveChanges();

            ImportOrders.Clear();
            foreach (var order in _context.ImportOrders.ToList())
                ImportOrders.Add(order);

            MessageBox.Show("Lưu phiếu nhập thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportToCsv()
        {
            if (SelectedOrder == null || !OrderDetails.Any())
            {
                MessageBox.Show("Không có dữ liệu để xuất.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv",
                FileName = $"ImportOrder_{SelectedOrder.OrderId}.csv"
            };

            if (dialog.ShowDialog() == true)
            {
                using var writer = new StreamWriter(dialog.FileName, false, System.Text.Encoding.UTF8);
                writer.WriteLine("Mã phiếu," + SelectedOrder.OrderId);
                writer.WriteLine("Ngày," + SelectedOrder.OrderDate?.ToString("dd/MM/yyyy"));
                writer.WriteLine("Nhà cung cấp," + SelectedOrder.Supplier?.SupplierName);
                writer.WriteLine();
                writer.WriteLine("Tên hải sản,Số lượng,Đơn giá,Thành tiền");
                foreach (var d in OrderDetails)
                {
                    var name = d.Seafood?.Name ?? "";
                    writer.WriteLine($"{name},{d.Quantity},{d.UnitPrice},{d.Quantity * d.UnitPrice}");
                }
                writer.WriteLine();
                writer.WriteLine("Tổng cộng," + SelectedOrder.TotalAmount);
                MessageBox.Show("Xuất CSV thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadOrderDetails()
        {
            if (SelectedOrder == null) return;

            OrderDetails.Clear();
            var details = _context.ImportOrderDetails
                .Where(d => d.OrderId == SelectedOrder.OrderId)  // Đảm bảo 'OrderId' là tên đúng của cột trong DB
                .ToList();

            foreach (var detail in details)
            {
                OrderDetails.Add(detail);
            }

            UpdateTotalAmount();
        }




        private void UpdateTotalAmount()
        {
            if (SelectedOrder != null)
            {
                SelectedOrder.TotalAmount = OrderDetails.Sum(d => d.Quantity * d.UnitPrice);
                _context.SaveChanges();
                OnPropertyChanged(nameof(SelectedOrder));
            }
        }

        private void CalculateSubtotal()
        {
            Subtotal = Quantity * UnitPrice;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
