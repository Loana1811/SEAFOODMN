using ASM_GROUP3.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ASM_GROUP3.ViewModel
{
    public class OrderViewModel : INotifyPropertyChanged
    {
        private readonly SeafoodManagementContext _context = new();

        public ObservableCollection<Order> Orders { get; set; } = new();
        public ObservableCollection<Seafood> Seafoods { get; set; } = new();
        public ObservableCollection<OrderDetail> OrderDetails { get; set; } = new();

        private Order? _selectedOrder;
        public Order? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged(nameof(SelectedOrder));

                if (_selectedOrder != null)
                {
                    OrderDetails = new ObservableCollection<OrderDetail>(
                        _context.OrderDetails
                        .Where(d => d.OrderId == _selectedOrder.OrderId)
                        .ToList()
                    );
                    OnPropertyChanged(nameof(OrderDetails));
                }
            }
        }

        public OrderDetail? SelectedOrderDetail { get; set; }

        private Seafood? _selectedSeafood;
        public Seafood? SelectedSeafood
        {
            get => _selectedSeafood;
            set
            {
                _selectedSeafood = value;
                OnPropertyChanged(nameof(SelectedSeafood));

                // Gán đơn giá từ giá hải sản
                if (_selectedSeafood?.UnitPrice != null)
                {
                    InputUnitPrice = _selectedSeafood.UnitPrice.Value;
                    OnPropertyChanged(nameof(InputUnitPrice));
                    OnPropertyChanged(nameof(InputSubtotal));
                }
            }
        }

        private decimal _inputQuantity;
        public decimal InputQuantity
        {
            get => _inputQuantity;
            set
            {
                if (_inputQuantity != value)
                {
                    _inputQuantity = value;
                    OnPropertyChanged(nameof(InputQuantity));
                    OnPropertyChanged(nameof(InputSubtotal));
                }
            }
        }

        private decimal _inputUnitPrice;
        public decimal InputUnitPrice
        {
            get => _inputUnitPrice;
            set
            {
                if (_inputUnitPrice != value)
                {
                    _inputUnitPrice = value;
                    OnPropertyChanged(nameof(InputUnitPrice));
                    OnPropertyChanged(nameof(InputSubtotal));
                }
            }
        }

        public decimal InputSubtotal => InputQuantity * InputUnitPrice;

        public ICommand AddOrderCommand { get; set; }
        public ICommand DeleteOrderCommand { get; set; }
        public ICommand SaveOrderCommand { get; set; }
        public ICommand PrintOrderCommand { get; set; }
        public ICommand AddOrderDetailCommand { get; set; }
        public ICommand DeleteOrderDetailCommand { get; set; }

        public OrderViewModel()
        {
            Orders = new ObservableCollection<Order>(_context.Orders.ToList());
            Seafoods = new ObservableCollection<Seafood>(_context.Seafoods.ToList());

            AddOrderCommand = new RelayCommand(AddOrder);
            DeleteOrderCommand = new RelayCommand(DeleteOrder);
            SaveOrderCommand = new RelayCommand(SaveOrder);
            PrintOrderCommand = new RelayCommand(PrintOrder);
            AddOrderDetailCommand = new RelayCommand(AddOrderDetail);
            DeleteOrderDetailCommand = new RelayCommand(DeleteOrderDetail);
        }

        private void AddOrder()
        {
            SelectedOrder = new Order { CreatedDate = DateTime.Now };
            OrderDetails = new ObservableCollection<OrderDetail>();
            OnPropertyChanged(nameof(SelectedOrder));
            OnPropertyChanged(nameof(OrderDetails));
        }

        private void DeleteOrder()
        {
            if (SelectedOrder == null) return;

            if (MessageBox.Show("Bạn có chắc muốn xóa đơn hàng này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _context.Orders.Remove(SelectedOrder);
                _context.SaveChanges();
                Orders.Remove(SelectedOrder);
                SelectedOrder = null;
                OrderDetails.Clear();
                OnPropertyChanged(nameof(SelectedOrder));
                OnPropertyChanged(nameof(OrderDetails));
            }
        }

        private void SaveOrder()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Không có đơn hàng để lưu.");
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedOrder.Customer) ||
                string.IsNullOrWhiteSpace(SelectedOrder.Phone) ||
                string.IsNullOrWhiteSpace(SelectedOrder.Address))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin khách hàng.");
                return;
            }

            if (!SelectedOrder.Phone.StartsWith("0") || SelectedOrder.Phone.Length < 10 || SelectedOrder.Phone.Length > 11)
            {
                MessageBox.Show("Số điện thoại không hợp lệ. Bắt đầu bằng 0 và có 10-11 chữ số.");
                return;
            }

            if (OrderDetails.Count == 0)
            {
                MessageBox.Show("Đơn hàng chưa có chi tiết.");
                return;
            }

            SelectedOrder.TotalAmount = OrderDetails.Sum(d => d.Subtotal);

            // Thêm mới đơn hàng nếu OrderId = 0
            if (SelectedOrder.OrderId == 0)
            {
                _context.Orders.Add(SelectedOrder);
                _context.SaveChanges(); // để EF tạo OrderId
            }
            else
            {
                _context.Orders.Update(SelectedOrder);

                // Xóa dữ liệu chi tiết cũ trong DB
                var oldDetails = _context.OrderDetails.Where(d => d.OrderId == SelectedOrder.OrderId).ToList();
                _context.OrderDetails.RemoveRange(oldDetails);
                _context.SaveChanges();
            }

            // Lưu lại chi tiết đơn hàng
            foreach (var detail in OrderDetails)
            {
                detail.OrderDetailId = 0; // để EF tự tạo lại
                detail.OrderId = SelectedOrder.OrderId;
                detail.Order = SelectedOrder;
                _context.OrderDetails.Add(detail);
            }

            _context.SaveChanges();

            // 🔄 Load lại OrderDetails từ DB để tránh lặp hiển thị
            OrderDetails = new ObservableCollection<OrderDetail>(
                _context.OrderDetails.Where(d => d.OrderId == SelectedOrder.OrderId).ToList()
            );
            OnPropertyChanged(nameof(OrderDetails));

            if (!Orders.Contains(SelectedOrder))
                Orders.Add(SelectedOrder);

            MessageBox.Show("Lưu đơn hàng thành công.");
            OnPropertyChanged(nameof(Orders));
        }



        private void AddOrderDetail()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Vui lòng tạo hoặc chọn đơn hàng.");
                return;
            }

            if (SelectedSeafood == null)
            {
                MessageBox.Show("Vui lòng chọn hải sản.");
                return;
            }

            if (InputQuantity <= 0)
            {
                MessageBox.Show("Số lượng phải lớn hơn 0.");
                return;
            }

            if (InputUnitPrice <= 0)
            {
                MessageBox.Show("Đơn giá phải lớn hơn 0.");
                return;
            }

            if (OrderDetails.Any(d => d.SeafoodId == SelectedSeafood.SeafoodId))
            {
                MessageBox.Show("Hải sản đã tồn tại trong đơn hàng.");
                return;
            }

            var detail = new OrderDetail
            {
                SeafoodId = SelectedSeafood.SeafoodId,
                Seafood = SelectedSeafood,
                Quantity = InputQuantity,
                UnitPrice = InputUnitPrice,
                Subtotal = InputSubtotal
            };

            OrderDetails.Add(detail);
            OnPropertyChanged(nameof(OrderDetails));
        }

        private void DeleteOrderDetail()
        {
            if (SelectedOrderDetail == null) return;
            OrderDetails.Remove(SelectedOrderDetail);
            OnPropertyChanged(nameof(OrderDetails));
        }

        private void PrintOrder()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Vui lòng chọn đơn hàng để in.");
                return;
            }

            // Cho người dùng chọn nơi lưu file
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Lưu hóa đơn dưới dạng CSV",
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"HoaDon_{SelectedOrder.OrderId}_{DateTime.Now:yyyyMMddHHmmss}.csv",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };

            if (dialog.ShowDialog() == true)
            {
                string filePath = dialog.FileName;

                try
                {
                    using (var writer = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                    {
                        // Ghi thông tin đơn hàng
                        writer.WriteLine("Mã đơn hàng,Ngày tạo,Khách hàng,Số điện thoại,Địa chỉ,Ghi chú,Tổng tiền");
                        writer.WriteLine($"{SelectedOrder.OrderId},{SelectedOrder.CreatedDate},{SelectedOrder.Customer},{SelectedOrder.Phone},{SelectedOrder.Address},{SelectedOrder.Note},{SelectedOrder.TotalAmount:N0}");

                        writer.WriteLine(); // dòng trống
                        writer.WriteLine("STT,Tên hải sản,Số lượng,Đơn giá,Thành tiền");

                        int stt = 1;
                        foreach (var detail in OrderDetails)
                        {
                            writer.WriteLine($"{stt},{detail.Seafood?.Name},{detail.Quantity},{detail.UnitPrice:N0},{detail.Subtotal:N0}");
                            stt++;
                        }
                    }

                    // Cập nhật trạng thái đã in
                    SelectedOrder.IsPrinted = true;
                    _context.Orders.Update(SelectedOrder);
                    _context.SaveChanges();

                    MessageBox.Show("Hóa đơn đã được lưu thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi lưu file: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                OnPropertyChanged(nameof(Orders));
            }
        }



        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
