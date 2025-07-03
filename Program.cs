using System; 
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq; 


public record SalesSummary(double TotalSales, List<SalesDetail> Details);
public record SalesDetail(string FileName, double FileTotal);
public record SalesData (double? Total); 

public class Program
{
    public static void Main(string[] args)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var storesDirectory = Path.Combine(currentDirectory, "stores");

        var salesTotalDir = Path.Combine(currentDirectory, "salesTotalDir");
        Directory.CreateDirectory(salesTotalDir);

        var salesFiles = FindFiles(storesDirectory);

        var salesReport = CalculateSalesTotal(salesFiles);

        File.AppendAllText(Path.Combine(salesTotalDir, "totals.txt"), $"{salesReport.TotalSales}{Environment.NewLine}");

        GenerateSalesReport(Path.Combine(salesTotalDir, "salesReport.txt"), salesReport);

        Console.WriteLine("Sales processing complete. Check 'salesTotalDir' for outputs.");
    }

    static IEnumerable<string> FindFiles(string folderName)
    {
        List<string> salesFiles = new List<string>();

        var foundFiles = Directory.EnumerateFiles(folderName, "*", SearchOption.AllDirectories);

        foreach (var file in foundFiles)
        {
            var extension = Path.GetExtension(file);
            if (extension == ".json")
            {
                salesFiles.Add(file);
            }
        }

        return salesFiles;
    }

    static SalesSummary CalculateSalesTotal(IEnumerable<string> salesFiles)
    {
        double totalSales = 0;
        List<SalesDetail> salesDetails = new List<SalesDetail>();

        foreach (var file in salesFiles)
        {
            string salesJson = File.ReadAllText(file);

            SalesData? data = JsonConvert.DeserializeObject<SalesData?>(salesJson);

            if (data != null && data.Total.HasValue)
            {
                double fileTotal = data.Total.Value;
                totalSales += fileTotal;

                string fileName = Path.GetFileName(file);
                salesDetails.Add(new SalesDetail(fileName, fileTotal));
            }
        }

        return new SalesSummary(totalSales, salesDetails);
    }

    static void GenerateSalesReport(string filePath, SalesSummary report)
    {
        var reportContent = new List<string>();
        reportContent.Add("Sales Summary");
        reportContent.Add("----------------------------");
        reportContent.Add($" Total Sales: ${report.TotalSales:N2}");

        reportContent.Add(Environment.NewLine + " Details:");
        foreach (var detail in report.Details)
        {
            reportContent.Add($"  {detail.FileName}: ${detail.FileTotal:N2}");
        }

        File.WriteAllLines(filePath, reportContent);
    }
}