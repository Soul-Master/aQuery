using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Windows.Automation;
using aQuery.Log;
using aQuery.Win32;
using static aQuery.Log.LogHelpers;

namespace aQuery
{
    // ReSharper disable once InconsistentNaming
    public class aQuery
    {
        public List<AutomationElement> Elements;
        public bool IsExists => Elements.Count > 0;

        #region Constructor

        internal aQuery()
        {
            Elements = new List<AutomationElement>();
        }

        public aQuery(params AutomationElement[] elements)
        {
            if (elements == null || elements.Length == 0) return;

            Elements = new List<AutomationElement>(elements);
        }

        public aQuery(List<AutomationElement> elements)
        {
            Elements = elements ?? new List<AutomationElement>();
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static aQuery a(string selector)
        {
            return Create(selector);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static aQuery a(string selector, aQuery context)
        {
            return context.Find(selector);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static aQuery a(string selector, AutomationElement context)
        {
            return a(context.Find(selector));
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static aQuery a(params AutomationElement[] elements)
        {
            return new aQuery(elements);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static aQuery a(List<AutomationElement> elements)
        {
            return new aQuery(elements);
        }

        public static aQuery Create(string selector)
        {
            using (PerformanceTester.Start(Log(nameof(Create), selector)))
            {
                var matchedElements = AutomationElement.RootElement.Find(selector);

                return new aQuery(matchedElements.ToArray());
            }
        }

        #endregion

        public static int MinLogTime = 250;
        static Action<TimeSpan?> Log(string name, string argument = null)
        {
            Action<TimeSpan?> action = timeSpan =>
            {
                if (timeSpan == null)
                {
                    // Begin
                    return;
                }

                if (timeSpan.Value.Milliseconds > MinLogTime)
                {
                    Info(name + (!string.IsNullOrEmpty(argument) ? ": " + argument : string.Empty) + " took " + timeSpan.Value.Milliseconds + " ms.");
                }
            };

            return action;
        }

        public aQuery Find(string selector)
        {
            using (PerformanceTester.Start(Log(nameof(Find), selector)))
            {
                var result = Elements.Find(selector);

                if (result.Count == 0)
                {
                    Warn($"Cannot find element with `{selector}` selector");
                }
                if (result.Count > 1)
                {
                    Info($"Found {result.Count} elements with `{selector}` selector");
                }

                return a(result);
            }
        }

        public string GetSelector()
        {
            if (Elements == null || Elements.Count == 0) return null;

            // TODO: Support combine selector
            // [selector1],[selector2],[selector3]
            return Elements[0].GetSelector();
        }

        public string Text()
        {
            using (PerformanceTester.Start(Log("get" + nameof(MediaTypeNames.Text))))
            {
                if (Elements == null || Elements.Count == 0) return null;

                return Elements?[0].GetText();
            }
        }

        public string Value()
        {
            using (PerformanceTester.Start(Log("get" + nameof(Value))))
            {
                if (Elements == null || Elements.Count == 0) return null;

                return Elements?[0].GetValue();
            }
        }

        public bool IsVisible()
        {
            using (PerformanceTester.Start(Log("get" + nameof(IsVisible))))
            {
                if (Elements == null || Elements.Count == 0) return false;

                return Elements.Any(x => x.IsVisible());
            }
        }

        public DataTable DataTable()
        {
            using (PerformanceTester.Start(Log("get" + nameof(DataTable))))
            {
                if (Elements == null || Elements.Count == 0) return null;

                return Elements?[0].GetDataTable();
            }
        }

        public System.Windows.Rect? BoundingRectangle()
        {
            using (PerformanceTester.Start(Log("get" + nameof(BoundingRectangle))))
            {
                if (Elements == null || Elements.Count == 0) return null;

                return Elements?[0].Current.BoundingRectangle;
            }
        }

        #region Action

        private static void TryToClick(AutomationElement element)
        {
            var result = false;
            var t = new Thread(() =>
            {
                result = element.Click();
            });

            t.Start();
            if (result || (t.Join(TimeSpan.FromMilliseconds(2000)) && result)) return;
            if (t.IsAlive) t.Abort();

            element.MouseClick();
        }

        public aQuery Click()
        {
            using (PerformanceTester.Start(Log(nameof(Click))))
            {
                if (Elements == null || Elements.Count == 0) return this;

                Elements.ForEach(TryToClick);

                return this;
            }
        }

        public aQuery Text(string value)
        {
            using (PerformanceTester.Start(Log("set" + nameof(MediaTypeNames.Text), $"\"{value}\"")))
            {
                if (Elements == null || Elements.Count == 0) return this;

                Elements.ForEach(x => x.SetText(value));

                return this;
            }
        }

        public aQuery DateTime(DateTime value)
        {
            using (PerformanceTester.Start(Log("set" + nameof(DateTime), $"\"{value.ToShortDateString()}\"")))
            {
                if (Elements == null || Elements.Count == 0) return this;

                Elements.ForEach(x => x.SetDateTime(value));

                return this;
            }
        }

        public aQuery Value(string value)
        {
            using (PerformanceTester.Start(Log("set" + nameof(Value))))
            {
                if (Elements == null || Elements.Count == 0) return this;

                Elements.ForEach(x => x.SetText(value));
                return this;
            }
        }

        public aQuery Select()
        {
            using (PerformanceTester.Start(Log(nameof(Select))))
            {
                if (Elements == null || Elements.Count == 0) return this;

                Elements.ForEach(x => x.Select());

                return this;
            }
        }

        public aQuery ShowButtonMenu()
        {
            using (PerformanceTester.Start(Log(nameof(ShowButtonMenu))))
            {
                if (Elements == null || Elements.Count == 0) return this;

                Elements.ForEach(x => x.ShowButtonMenu());

                return this;
            }
        }

        public aQuery ShowWindow()
        {
            using (PerformanceTester.Start(Log(nameof(ShowButtonMenu))))
            {
                if (Elements == null || Elements.Count == 0) return this;

                SafeNativeMethods.SetForegroundWindow(Elements[0].Current.NativeWindowHandle);

                return this;
            }
        }

        #endregion
    }
}
