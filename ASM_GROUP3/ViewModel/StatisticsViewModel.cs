using ASM_GROUP3.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace ASM_GROUP3.ViewModel
{
    public class StatisticsViewModel : INotifyPropertyChanged
    {
        private readonly SeafoodManagementContext _context = new();

        public ObservableCollection<MonthlyRevenue> MonthlyRevenues { get; set; } = new();
        public ObservableCollection<TopSeafood> BestSellingSeafoods { get; set; } = new();
        public ObservableCollection<ImportSummary> ImportSummaries { get; set; } = new();

        public StatisticsViewModel()
        {
            LoadRevenueStats();
            LoadBestSellingSeafoods();
            LoadImportStats();
        }

        private void LoadRevenueStats()
        {
            var revenues = _context.Orders
                .Where(o => o.CreatedDate.HasValue)
                .GroupBy(o => new
                {
                    o.CreatedDate!.Value.Year,
                    o.CreatedDate!.Value.Month
                })
                .Select(g => new MonthlyRevenue
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(o => (decimal)(o.TotalAmount ?? 0)),
                    OrderCount = g.Count(),
                    TotalQuantity = g.SelectMany(o => o.OrderDetails).Sum(d => d.Quantity ?? 0)
                })
                .OrderByDescending(r => r.Year)
                .ThenByDescending(r => r.Month)
                .ToList();

            MonthlyRevenues = new ObservableCollection<MonthlyRevenue>(revenues);
            OnPropertyChanged(nameof(MonthlyRevenues));
        }

        private void LoadBestSellingSeafoods()
        {
            var bestSellers = _context.OrderDetails
                .Where(d => d.Seafood != null)
                .GroupBy(d => d.SeafoodId)
                .Select(g => new TopSeafood
                {
                    SeafoodName = g.First().Seafood!.Name,
                    TotalSold = g.Sum(x => x.Quantity ?? 0),
                    TotalRevenue = g.Sum(x => (x.Quantity ?? 0) * (x.UnitPrice ?? 0))
                })
                .OrderByDescending(s => s.TotalSold)
                .Take(5)
                .ToList();

            BestSellingSeafoods = new ObservableCollection<TopSeafood>(bestSellers);
            OnPropertyChanged(nameof(BestSellingSeafoods));
        }

        private void LoadImportStats()
        {
            var imports = _context.ImportOrders
                .Where(i => i.OrderDate.HasValue)
                .GroupBy(i => new
                {
                    i.OrderDate!.Value.Year,
                    i.OrderDate!.Value.Month
                })
                .Select(g => new ImportSummary
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalImport = g.Sum(i => (decimal)(i.TotalAmount ?? 0)),
                    ImportOrderCount = g.Count(),
                    TotalItems = g.SelectMany(i => i.ImportOrderDetails).Count()
                })
                .OrderByDescending(i => i.Year)
                .ThenByDescending(i => i.Month)
                .ToList();

            ImportSummaries = new ObservableCollection<ImportSummary>(imports);
            OnPropertyChanged(nameof(ImportSummaries));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }

    public class MonthlyRevenue
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalQuantity { get; set; }
    }

    public class TopSeafood
    {
        public string SeafoodName { get; set; } = string.Empty;
        public decimal TotalSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class ImportSummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalImport { get; set; }
        public int ImportOrderCount { get; set; }
        public int TotalItems { get; set; }
    }
}
