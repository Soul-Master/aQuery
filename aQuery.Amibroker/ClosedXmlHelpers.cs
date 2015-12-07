using ClosedXML.Excel;

namespace aQuery.Amibroker
{
    internal static class ClosedXmlHelpers
    {
        public static void ConvertTextToExcel(string excelFileName, string worksheetName, string text)
        {
            using (var workbook = new XLWorkbook())
            {
                using (var worksheet = workbook.Worksheets.Add(worksheetName ?? "Sheet1"))
                {
                    var rowCount = 1;
                    var csvLines = text.Split('\n');

                    foreach (var line in csvLines)
                    {
                        var colCount = 1;
                    
                        foreach (var col in line.Split('\t'))
                        {
                            worksheet.Cell(rowCount, colCount).Value = col;
                            colCount++;
                        }
                        rowCount++;
                    }

                }

                workbook.SaveAs(excelFileName);
            }
        }
    }
}