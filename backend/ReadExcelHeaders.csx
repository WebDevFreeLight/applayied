#r "nuget: MiniExcel, 1.31.2"
using MiniExcelLibs;
using System.Collections.Generic;
using System.Linq;

var filePath = @"Chart_of_Accounts_with_Flags (1).xlsx";
var rows = MiniExcel.Query(filePath, useHeaderRow: true).Cast<IDictionary<string, object>>().FirstOrDefault();
if (rows != null)
{
    int i = 1;
    foreach (var key in rows.Keys)
    {
        Console.WriteLine($"{i}. {key}");
        i++;
    }
}
