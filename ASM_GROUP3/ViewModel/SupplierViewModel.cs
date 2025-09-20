using ASM_GROUP3.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace ASM_GROUP3.ViewModel
{
    public class SupplierViewModel : INotifyPropertyChanged
    {
        private readonly SeafoodManagementContext _context = new();

        public ObservableCollection<Supplier> Suppliers { get; set; }

        private Supplier? _selectedSupplier;
        public Supplier? SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                _selectedSupplier = value;
                OnPropertyChanged(nameof(SelectedSupplier));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand UpdateCommand { get; }

        public SupplierViewModel()
        {
            Suppliers = new ObservableCollection<Supplier>(_context.Suppliers.ToList());

            AddCommand = new RelayCommand(AddSupplier);
            DeleteCommand = new RelayCommand(DeleteSupplier, () => SelectedSupplier != null);
            UpdateCommand = new RelayCommand(UpdateSupplier, () => SelectedSupplier != null);
        }

        private bool ValidateSupplier(Supplier supplier, bool isNew = false)
        {
            // Kiểm tra mã nhà cung cấp
            if (string.IsNullOrWhiteSpace(supplier.SupplierCode))
            {
                ShowError("Mã nhà cung cấp không được để trống.");
                return false;
            }

            // Kiểm tra tên nhà cung cấp
            if (string.IsNullOrWhiteSpace(supplier.SupplierName))
            {
                ShowError("Tên nhà cung cấp không được để trống.");
                return false;
            }
            // Số điện thoại phải bắt đầu bằng 0 và có 10 chữ số
            if (!Regex.IsMatch(supplier.Phone, @"^0\d{9}$"))
            {
                ShowError("Số điện thoại phải bắt đầu bằng 0 và có đúng 10 chữ số.");
                return false;
            }
            // Chặn ký tự đặc biệt trong tên nhà cung cấp
            if (Regex.IsMatch(supplier.SupplierName, @"[^a-zA-Z0-9\s]"))
            {
                ShowError("Tên nhà cung cấp không được chứa ký tự đặc biệt.");
                return false;
            }

            // Kiểm tra người liên hệ
            if (string.IsNullOrWhiteSpace(supplier.ContactPerson))
            {
                ShowError("Người liên hệ không được để trống.");
                return false;
            }

            // Kiểm tra số điện thoại (8-15 chữ số, chỉ chứa số)
            if (string.IsNullOrWhiteSpace(supplier.Phone))
            {
                ShowError("Số điện thoại không được để trống.");
                return false;
            }

            if (!Regex.IsMatch(supplier.Phone, @"^\d{8,15}$"))
            {
                ShowError("Số điện thoại không hợp lệ. Chỉ được chứa số và từ 8-15 chữ số.");
                return false;
            }

            // Kiểm tra ngày hợp tác
            if (supplier.CooperationStartDate > DateTime.Now)
            {
                ShowError("Ngày hợp tác không được lớn hơn ngày hiện tại.");
                return false;
            }

            // Kiểm tra mã trùng
            bool isDuplicate = isNew
                ? _context.Suppliers.Any(s => s.SupplierCode == supplier.SupplierCode)
                : _context.Suppliers.Any(s => s.SupplierCode == supplier.SupplierCode && s.SupplierId != supplier.SupplierId);

            if (isDuplicate)
            {
                ShowError("Mã nhà cung cấp đã tồn tại.");
                return false;
            }

            return true;
        }


        private void AddSupplier()
        {
            var newSupplier = new Supplier
            {
                SupplierCode = "NCC-" + DateTime.Now.Ticks.ToString().Substring(10), // Tạo mã tạm
                SupplierName = "Nhà cung cấp mới",
                Address = "Địa chỉ",
                Phone = "0123456789",
                ContactPerson = "Người liên hệ",
                IsActive = true,
                CooperationStartDate = DateTime.Now
            };

            if (!ValidateSupplier(newSupplier, isNew: true)) return;

            _context.Suppliers.Add(newSupplier);
            _context.SaveChanges();
            Suppliers.Add(newSupplier);
        }

        private void DeleteSupplier()
        {
            if (SelectedSupplier != null)
            {
                var result = MessageBox.Show($"Bạn có chắc muốn xóa nhà cung cấp '{SelectedSupplier.SupplierName}' không?",
                    "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return;

                _context.Suppliers.Remove(SelectedSupplier);
                _context.SaveChanges();
                Suppliers.Remove(SelectedSupplier);
            }
        }

        private void UpdateSupplier()
        {
            if (SelectedSupplier == null || !ValidateSupplier(SelectedSupplier)) return;

            var duplicate = _context.Suppliers.FirstOrDefault(s =>
                s.SupplierCode == SelectedSupplier.SupplierCode && s.SupplierId != SelectedSupplier.SupplierId);

            if (duplicate != null)
            {
                MessageBox.Show("Mã nhà cung cấp đã tồn tại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var supplierToUpdate = _context.Suppliers.FirstOrDefault(s => s.SupplierId == SelectedSupplier.SupplierId);
            if (supplierToUpdate != null)
            {
                supplierToUpdate.SupplierName = SelectedSupplier.SupplierName;
                supplierToUpdate.SupplierCode = SelectedSupplier.SupplierCode;
                supplierToUpdate.Phone = SelectedSupplier.Phone;
                supplierToUpdate.Address = SelectedSupplier.Address;
                supplierToUpdate.ContactPerson = SelectedSupplier.ContactPerson;
                supplierToUpdate.IsActive = SelectedSupplier.IsActive;
                supplierToUpdate.CooperationStartDate = SelectedSupplier.CooperationStartDate;

                _context.SaveChanges();
                MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
