using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ASM_GROUP3.Models;

namespace ASM_GROUP3.ViewModel
{
    public class SeafoodViewModel : INotifyPropertyChanged
    {
        private SeafoodManagementContext _context = new SeafoodManagementContext();

        public ObservableCollection<Seafood> Seafoods { get; set; }
        public ObservableCollection<string> Categories { get; set; }
        public ObservableCollection<string> Units { get; set; }

        private Seafood _selectedSeafood;
        public Seafood SelectedSeafood
        {
            get => _selectedSeafood;
            set
            {
                _selectedSeafood = value;
                OnPropertyChanged(nameof(SelectedSeafood));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); }
        }

        private string _selectedCategory;
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged(nameof(SelectedCategory));
                SearchSeafood();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand SearchCommand { get; }

        public SeafoodViewModel()
        {
            Seafoods = new ObservableCollection<Seafood>(_context.Seafoods.ToList());

            var distinctCategories = _context.Seafoods
                .Select(s => s.Category)
                .Distinct()
                .ToList();
            distinctCategories.Insert(0, "Tất cả");
            Categories = new ObservableCollection<string>(distinctCategories);

            Units = new ObservableCollection<string> { "kg", "con", "gram", "bao", "thùng" };

            AddCommand = new RelayCommand(AddSeafood);
            DeleteCommand = new RelayCommand(DeleteSeafood, () => SelectedSeafood != null);
            UpdateCommand = new RelayCommand(UpdateSeafood, () => SelectedSeafood != null);
            SearchCommand = new RelayCommand(SearchSeafood);
        }

        private void AddSeafood()
        {
            var newSeafood = new Seafood
            {
                Name = "Tên mới",
                Category = Categories.FirstOrDefault(c => c != "Tất cả") ?? "Chưa phân loại",
                Unit = Units.FirstOrDefault() ?? "kg",
                Quantity = 0m,
                UnitPrice = 0m,
                ImportDate = DateTime.Now,
                IsAvailable = true
            };

            _context.Seafoods.Add(newSeafood);
            _context.SaveChanges();

            Seafoods.Add(newSeafood);

            if (!Categories.Contains(newSeafood.Category))
            {
                Categories.Add(newSeafood.Category);
                if (!Categories.Contains("Tất cả"))
                    Categories.Insert(0, "Tất cả");
            }

            SelectedSeafood = newSeafood;
        }

        private void DeleteSeafood()
        {
            if (SelectedSeafood != null)
            {
                _context.Seafoods.Remove(SelectedSeafood);
                _context.SaveChanges();
                Seafoods.Remove(SelectedSeafood);
            }
        }

        private void UpdateSeafood()
        {
            if (SelectedSeafood == null) return;

            string error = ValidateSeafood(SelectedSeafood);
            if (error != null)
            {
                MessageBox.Show(error, "Lỗi dữ liệu", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var seafoodToUpdate = _context.Seafoods.FirstOrDefault(s => s.SeafoodId == SelectedSeafood.SeafoodId);
            if (seafoodToUpdate != null)
            {
                seafoodToUpdate.Name = SelectedSeafood.Name?.Trim();
                seafoodToUpdate.Category = SelectedSeafood.Category?.Trim();
                seafoodToUpdate.Quantity = SelectedSeafood.Quantity;
                seafoodToUpdate.UnitPrice = SelectedSeafood.UnitPrice;
                seafoodToUpdate.Unit = SelectedSeafood.Unit;
                seafoodToUpdate.ImportDate = SelectedSeafood.ImportDate;
                seafoodToUpdate.IsAvailable = SelectedSeafood.IsAvailable;

                _context.SaveChanges();

                MessageBox.Show("Cập nhật thành công.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                if (!Categories.Contains(seafoodToUpdate.Category))
                {
                    Categories.Add(seafoodToUpdate.Category);
                    if (!Categories.Contains("Tất cả"))
                        Categories.Insert(0, "Tất cả");
                }
            }
        }

        private void SearchSeafood()
        {
            var results = _context.Seafoods.AsQueryable();

            if (!string.IsNullOrEmpty(SearchText))
                results = results.Where(s => s.Name.Contains(SearchText) || s.Category.Contains(SearchText));

            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "Tất cả")
                results = results.Where(s => s.Category == SelectedCategory);

            Seafoods.Clear();
            foreach (var s in results.ToList())
                Seafoods.Add(s);
        }

        private string ValidateSeafood(Seafood item)
        {
            if (item == null) return "Không có dữ liệu.";

            if (string.IsNullOrWhiteSpace(item.Name))
                return "Vui lòng nhập tên hải sản.";

            if (string.IsNullOrWhiteSpace(item.Category))
                return "Vui lòng chọn loại.";

            if (item.UnitPrice == null || item.UnitPrice <= 0)
                return "Giá nhập phải lớn hơn 0.";

            if (item.IsAvailable == true && (item.Quantity == null || item.Quantity <= 0))
                return "Số lượng phải lớn hơn 0 khi còn hàng.";

            // kiểm tra trùng tên (không phân biệt hoa thường)
            bool isDuplicate = _context.Seafoods
                .Any(s => s.SeafoodId != item.SeafoodId &&
                          s.Name.Trim().ToLower() == item.Name.Trim().ToLower());

            if (isDuplicate)
                return "Tên hải sản đã tồn tại.";

            return null;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
