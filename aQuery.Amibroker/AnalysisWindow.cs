using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Forms;
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
        private const string resultListGrid = "pane[ClassName=AfxFrameOrView80] > pane[ClassName=AfxWnd80] > data grid[ClassName=SysListView32]";
        
        // values
        private const string dateRangeAllQuotes= "All quotes";
        private const string dateRangeFromToDates = "From-To dates";
        private const string dateRangeRecentBars = " recent bar(s)";
        private const string dateRangeRecentDays = " recent day(s)";
        // ReSharper restore InconsistentNaming

        public AmibrokerApp App { get; }
        public aQuery Element { get; }
        public WindowPattern Window { get; }
        public aQuery Toolbar { get; }
        public aQuery BacktesterSetting { get; private set; }

        public AnalysisWindow(AmibrokerApp app)
        {
            App = app;
            Element = a(analysisWindow, app.Window);
            Toolbar = a(toolbarPane, Element);
            Window = Element.Elements[0].GetPattern<WindowPattern>();

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

        public async Task<bool> Backtest(IProgress<AnalysisProgress> progress = null)
        {
            a(backtestButton, Toolbar).Click();
            await WaitUntilDone(progress);

            return true;
        }

        public async Task<bool> Backtest(string path, IProgress<AnalysisProgress> progress = null)
        {
            SelectFormula(path);

            return await Backtest(progress);
        }

        public async Task<bool> Optimize(IProgress<AnalysisProgress> progress = null)
        {
            a(optimizeButton, Toolbar).Click();
            await WaitUntilDone(progress);

            return true;
        }

        public async Task<bool> Optimize(string path, IProgress<AnalysisProgress> progress = null)
        {
            SelectFormula(path);

            return await Optimize(progress);
        }

        public void ExportResultList(string excelFile, string workSheetName)
        {
            var aGrid = a(resultListGrid, Element);
            var items = a("item", aGrid);
            items.SelectItem();

            Clipboard.Clear();
            SendKeys.SendWait("^c");
            var text = Clipboard.GetText();
            ClosedXmlHelpers.ConvertTextToExcel(excelFile, workSheetName, text);
            Clipboard.Clear();
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
    }
}