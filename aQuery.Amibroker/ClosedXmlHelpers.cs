using System.Data;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace aQuery.Amibroker
{
    internal static class ClosedXmlHelpers
    {
        public const string DefaultSheetName = "Sheet1";

        /// <summary>
        ///  Select all elements and export selected text to Excel
        /// </summary>
        public static void ConvertToExcel(this aQuery selectableElement, string excelFileName, string worksheetName = DefaultSheetName)
        {
            selectableElement.Select();
            Clipboard.Clear();
            SendKeys.SendWait("^c");

            var text = Clipboard.GetText();

            using (var workbook = new XLWorkbook())
            {
                using (var worksheet = workbook.Worksheets.Add(worksheetName ?? DefaultSheetName))
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

            Clipboard.Clear();
        }

        /// <summary>
        ///  Select all elements and export selected text to Excel
        /// </summary>
        public static void SaveAsExcel(this DataTable table, string excelFileName)
        {
            if (table == null) return;

            using (var workbook = new XLWorkbook())
            {
                if (string.IsNullOrEmpty(table.TableName))
                {
                    table.TableName = DefaultSheetName;
                }

                workbook.AddWorksheet(table);
                workbook.SaveAs(excelFileName);
            }
        }
    }
}