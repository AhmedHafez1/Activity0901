using EF_Activity001;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

namespace Activity0201_ExistingDbCore
{
    internal class Program
    {
        static IConfigurationRoot _configuration;

        static DbContextOptionsBuilder<AdventureWorksContext> _optionsBuilder;
        static void Main(string[] args)
        {
            BuildConfiguration();
            BuildOptions();

            var input = string.Empty;
            Console.WriteLine("Would you like to view the sales report?");
            input = Console.ReadLine();

            if (input.StartsWith("y", StringComparison.OrdinalIgnoreCase))
            {
                GenerateSalesReport();
            }
        }

        private static void GenerateSalesReport()
        {
            Console.WriteLine("What is the minimum amount of sales?");
            var input = Console.ReadLine();
            decimal filter = 0.0m;
            if (!decimal.TryParse(input, out filter))
            {
                Console.WriteLine("Bad input");
                return;
            }
            using (AdventureWorksContext db = new AdventureWorksContext(_optionsBuilder.Options))
            {
                var salesReport = db.SalesPeople.Select(sp => new
                {
                    beid = sp.BusinessEntityId,
                    sp.BusinessEntity.BusinessEntity.FirstName,
                    sp.BusinessEntity.BusinessEntity.LastName,
                    sp.SalesYtd,
                    Territories = sp.SalesTerritoryHistories
                        .Select(t => t.Territory.Name),
                    OrderCount = sp.SalesOrderHeaders.Count(),
                    TotalProductsSold = sp.SalesOrderHeaders.SelectMany(y => y.SalesOrderDetails).Sum(z => z.OrderQty)
                })
                .Where(x => x.SalesYtd > filter)
                .OrderBy(srds => srds.LastName)
                .ThenBy(srds => srds.FirstName)
                .ThenByDescending(srds => srds.SalesYtd)
                .Take(20).ToList();

                foreach (var srd in salesReport)
                {
                    Console.WriteLine($"{srd.beid}| {srd.LastName}, {srd.FirstName}" +
                        $"| YTD Sales: {srd.SalesYtd}" +
                        $"| {string.Join(',', srd.Territories)}" +
                        $"| Order Count: {srd.OrderCount}" +
                        $"Products Sold: {srd.TotalProductsSold}");
                }
            }


        }

        static void BuildConfiguration()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true,
            reloadOnChange: true);
            _configuration = builder.Build();
        }

        static void BuildOptions()
        {
            _optionsBuilder = new DbContextOptionsBuilder<AdventureWorksContext>();
            _optionsBuilder.UseSqlServer(_configuration.GetConnectionString("AdventureWorks"));
        }


        private static void ShowAllSalesPeopleUsingProjection()
        {
            using (var db = new AdventureWorksContext(_optionsBuilder.Options))
            {
                var salesPeople = db.SalesPeople
                    .Select(x => new
                    {
                        x.BusinessEntityId,
                        x.BusinessEntity.BusinessEntity.FirstName,
                        x.BusinessEntity.BusinessEntity.LastName,
                        x.SalesQuota,
                        x.SalesYtd,
                        x.SalesLastYear
                    }).ToList();

                foreach (var sp in salesPeople)
                {
                    Console.WriteLine($"BID: {sp.BusinessEntityId} | Name: {sp.LastName}"
                        + $", {sp.FirstName} | Quota: {sp.SalesQuota} | "
                        + $"YTD Sales: {sp.SalesYtd} | SalesLastYear {sp.SalesLastYear}");
                }
            }
        }
    }
}
