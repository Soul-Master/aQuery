using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using static aQuery.aQuery;

namespace aQuery.Amibroker
{
    public class AnalysisWindow
    {
        // ReSharper disable InconsistentNaming
        // :last it's mean newest created window.
        private const string analysisWindow = "pane[ClassName=MDIClient] > window[Name*=Analysis]:first";
        private const string toolbarPane = "'xtpBarTop' pane";
        private const string settingDialog = "'Backtester settings' Dialog";
        private const string settingToolbar = "'Main' tool bar > 'Settings' menu item";
        private const string backtestButton = "'Main' tool bar > 'Backtest' menu item";
        private const string optimizeButton = "'Main' tool bar > 'Optimize' menu item";
        private const string pickAFile = "'Formula' tool bar > 'Pick a file' button";
        private const string startDatePicker = "'Settings' tool bar[ClassName=XTPToolBar] > pane[ClassName=SysDateTimePick32][AutomationId=1]";
        private const string endDatePicker = "'Settings' tool bar[ClassName=XTPToolBar] > pane[ClassName=SysDateTimePick32][AutomationId=2]";
        private const string filterSetting = "'Settings' tool bar[ClassName=XTPToolBar] > edit[ClassName=RichEdit20A]:eq(0)";
        private const string rangeSetting = "'Settings' tool bar[ClassName=XTPToolBar] > edit[ClassName=RichEdit20A]:eq(1)";
        private const string progressPane = "'Progress' tool bar[ClassName=XTPToolBar]";
        //private const string progressCancelButton = "button[AccessKey=C]";
        private const string progressTextButton = "button[AccessKey=]";
        private const string outputPane = "pane[AutomationId=59648] > pane[AutomationId=1]";
        private const string resultListGrid = "data grid[AutomationId=1005]";
        private const string infoList = "list[AutomationId=1006]";
        private const string walkForwardGrid = "data grid[AutomationId=1007]";

        // values
        private const string dateRangeAllQuotes = "All quotes";
        private const string dateRangeFromToDates = "From-To dates";
        private const string dateRangeRecentBars = " recent bar(s)";
        private const string dateRangeRecentDays = " recent day(s)";
        // ReSharper restore InconsistentNaming

        public AmibrokerApp App { get; }
        public aQuery Analysis { get; }
        public WindowPattern Window { get; }
        public aQuery Toolbar { get; }
        public aQuery BacktesterSetting { get; private set; }
        public aQuery OutputPane { get; }

        public AnalysisWindow(AmibrokerApp app)
        {
            App = app;
            Analysis = a(analysisWindow, app.Window);
            Toolbar = a(toolbarPane, Analysis);
            Window = Analysis.Elements[0].GetPattern<WindowPattern>();
            OutputPane = a(outputPane, Analysis);

            if (Window.Current.WindowVisualState == WindowVisualState.Normal)
            {
                Window.SetWindowVisualState(WindowVisualState.Maximized);
            }
            else
            {
                // Workaround for set as selected tab
                Window.SetWindowVisualState(WindowVisualState.Normal);
                Window.SetWindowVisualState(WindowVisualState.Maximized);
            }
        }

        public void OpenSetting()
        {
            a(settingToolbar, Toolbar).Click();

            BacktesterSetting = App.Window.Find(settingDialog);
        }

        public void LoadSetting(string path)
        {
            if (BacktesterSetting == null) OpenSetting();

            // ReSharper disable once PossibleNullReferenceException
            BacktesterSetting.Find("'Load' button").Click();
            App.OpenFile(path, BacktesterSetting);
            a("'OK' button", BacktesterSetting).Click();

            BacktesterSetting = null;
        }

        public void SelectFormula(string path)
        {
            a(pickAFile, Toolbar).Click();
            App.OpenFile(path);
        }

        public async Task WaitUntilDone(IProgress<AnalysisProgress> progress = null)
        {
            var progressPanel = a(progressPane, Toolbar);
            if (!progressPanel.IsExists) return;

            await Task.Run(() =>
            {
                var lastProgress = -1;

                while (true)
                {
                    if (!progressPanel.IsVisible()) break;

                    // Directly call Find method to find only one-time
                    var progressText = progressPanel.Elements.Find(progressTextButton, 0);
                    if (progressText.Count == 0) break;

                    if (progress != null)
                    {
                        var data = new AnalysisProgress(a(progressText).Text());

                        if (data.Percent > lastProgress)
                        {
                            progress.Report(data);
                            lastProgress = data.Percent;
                        }
                    }

                    Thread.Sleep(50);
                }
            });
        }

        public async Task Backtest(IProgress<AnalysisProgress> progress = null)
        {
            a(backtestButton, Toolbar).Click();
            await WaitUntilDone(progress);
        }

        public async Task Backtest(string path, IProgress<AnalysisProgress> progress = null)
        {
            SelectFormula(path);

            await Backtest(progress);
        }

        public async Task Optimize(IProgress<AnalysisProgress> progress = null)
        {
            a(optimizeButton, Toolbar).Click();
            await WaitUntilDone(progress);
        }

        public async Task WalkForwardTest(string target, IProgress<AnalysisProgress> progress = null)
        {
            if (BacktesterSetting == null) OpenSetting();
            BacktesterSetting = App.Window.Find(settingDialog);
            var walkForwardTab = a("tab > 'Walk-Forward' tab item", BacktesterSetting);
            walkForwardTab.Select();
            a("combo box[AutomationId=1494]", walkForwardTab).Value(target);
            a("'OK' button", BacktesterSetting).Click();
            BacktesterSetting = null;

            a(optimizeButton, Toolbar).ShowButtonMenu();
            a("menu[ClassName=XTPPopupBar] > 'Walk-Forward' menu item", App.Window).Click();

            await WaitUntilDone(progress);
        }

        void SwitchOutputTab(int offsetLeft, int offetBottom)
        {
            var rect = OutputPane.Elements[0].Current.BoundingRectangle;
            var resultListTabPoint = new Point(rect.Left + offsetLeft, rect.Bottom - offetBottom);
            resultListTabPoint.MouseClick((int)App.Broker.Handle);

            Thread.Sleep(500);
        }

        public void ExportResultList(string excelFile, string workSheetName = ClosedXmlHelpers.DefaultSheetName)
        {
            var attempt = 5;
            aQuery aGrid = null;

            while (aGrid == null || aGrid.Elements.Count == 0)
            {
                SwitchOutputTab(100, 9);
                aGrid = a(resultListGrid, OutputPane);

                if (attempt == 0) return;
                attempt--;
                Thread.Sleep(250);
            }

            aGrid.DataTable().SaveAsExcel(excelFile);
        }

        public string GetAnalysisInfo()
        {
            var attempt = 5;
            aQuery list = null;

            while (list == null || list.Elements.Count == 0)
            {
                SwitchOutputTab(150, 9);
                list = a(infoList, OutputPane);

                if (attempt == 0) return string.Empty;
                attempt--;
                Thread.Sleep(250);
            }

            var items = a("list item", list).Elements.Select(x => x.GetText()).ToList();
            var lastTaskIndex = -1;

            for (var i = items.Count - 2; i > 0; i--)
            {
                if (items[i].StartsWith("-----"))
                {
                    lastTaskIndex = i;
                    break;
                }
            }

            if (lastTaskIndex > 0)
            {
                items = items.Skip(lastTaskIndex + 1).ToList();
            }

            return String.Join(Environment.NewLine, items.Take(items.Count - 1));
        }

        public int ExportWalkForward(string excelFile, string workSheetName = ClosedXmlHelpers.DefaultSheetName)
        {
            var attempt = 5;
            aQuery aGrid = null;

            while (aGrid == null || aGrid.Elements.Count == 0)
            {
                SwitchOutputTab(240, 9);
                aGrid = a(walkForwardGrid, OutputPane);

                if (attempt == 0) return 0;
                attempt--;
                Thread.Sleep(250);
            }
            var dataTable = aGrid.DataTable();
            dataTable.SaveAsExcel(excelFile);

            return dataTable.Rows.Count;
        }

        /// <summary>
        /// Set RangeMode to all quotes
        /// </summary>
        public void SetRange()
        {
            a(rangeSetting, Toolbar).Value(dateRangeAllQuotes);
        }

        /// <summary>
        /// Set RangeMode to from-to date
        /// </summary>
        public void SetRange(DateTime startDate, DateTime endDate)
        {
            a(rangeSetting, Toolbar).Value(dateRangeFromToDates);
            a(startDatePicker, Toolbar).DateTime(startDate);
            a(endDatePicker, Toolbar).DateTime(endDate);
        }

        /// <summary>
        /// Set RangeMode to recentBars
        /// </summary>
        public void SetRangeBars(int recentBars)
        {
            a(rangeSetting, Toolbar).Value(recentBars + dateRangeRecentBars);
        }

        /// <summary>
        /// Set RangeMode to recentDays
        /// </summary>
        public void SetRangeDays(int recentDays)
        {
            a(rangeSetting, Toolbar).Value(recentDays + dateRangeRecentDays);
        }

        /// <summary>
        /// Set Filter to All symbols/Current
        /// </summary>
        public void SetFilter(bool isCurrent = false)
        {
            a(filterSetting, Toolbar).Value(isCurrent ? "*Current" : "*All symbols");
        }

        /// <summary>
        /// Set Filter to Current
        /// </summary>
        public void SetFilter(object filterModel)
        {
            // TODO: Support set filter model
            a(filterSetting, Toolbar).Value("*Filter");
        }

        /// <summary>
        /// The latest backtest report will be moved to destination directory
        /// </summary>
        public bool MoveBacktestReport(string reportName, string destinationDir)
        {
            var reports = App.ReportsDir.GetDirectories(reportName + "-*");
            if (reports.Length == 0) return false;

            var reportDir = reports.OrderByDescending(x => x.Name).Last();

            return Win32Helpers.MoveFolder(reportDir.FullName, destinationDir);
        }

        /// <summary>
        /// The latest backtest report will be moved to destination directory
        /// </summary>
        public bool MoveWalkForwardReport(string reportName, string destinationDir, int numberOfYears)
        {
            var outOfSampleDir = App.ReportsDir.GetDirectories(reportName + " - Out-of-Sample summary-*")
                .OrderByDescending(x => x.LastWriteTime)
                .LastOrDefault();

            if (outOfSampleDir == null) return false;

            Win32Helpers.MoveFolder(outOfSampleDir.FullName, destinationDir);

            var yearReports = App.ReportsDir.GetDirectories(reportName + "-*")
                .OrderByDescending(x => x.LastWriteTime)
                .Take(numberOfYears);

            foreach (var report in yearReports)
            {
                Win32Helpers.MoveFolder(report.FullName, destinationDir);
            }

            return true;
        }
    }
}