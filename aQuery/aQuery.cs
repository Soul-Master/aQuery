using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Automation;
using aQuery.Log;

namespace aQuery
{
    // ReSharper disable once InconsistentNaming
    public class aQuery
    {
        public AutomationElement Element;
        public bool IsExists => Element != null;

        #region Constructor

        internal aQuery() { }

        public aQuery(AutomationElement element)
        {
            Element = element;
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
        public static aQuery a(AutomationElement element)
        {
            return new aQuery(element);
        }

        public static aQuery Create(string selector)
        {
            using (PerformanceTester.Start(Log(nameof(Create), selector)))
            {
                var result = AutomationElement.RootElement.Find(selector);

                return result;
            }
        }

        public static aQuery Create(AutomationElement element)
        {
            return new aQuery(element);
        }

        #endregion
        
        static Action<TimeSpan?> Log(string name, string argument = null)
        {
            Action<TimeSpan?> action = timeSpan =>
            {
                if (timeSpan == null)
                {
                    // Begin
                    Console.WriteLine(name + (!string.IsNullOrEmpty(argument) ? ": " + argument : string.Empty));
                    return;
                }

                Console.WriteLine(timeSpan.Value.Milliseconds + " ms.");
            };

            return action;
        }

        public aQuery Find(string selector)
        {
            using (PerformanceTester.Start(Log(nameof(Find), selector)))
            {
                if (Element == null)
                {
                    Console.WriteLine("Current element is null");
                    return new aQuery();
                }

                return Element.Find(selector);
            }
        }

        public string GetSelector()
        {
            return Element.GetSelector();
        }

        #region Action

        public aQuery Click()
        {
            using (PerformanceTester.Start(Log(nameof(Click))))
            {
                if (Element == null) return this;

                var result = false;
                var t = new Thread(() =>
                {
                    try
                    {
                        result = Element.Click();
                    }
                    catch (ThreadAbortException)
                    {
                        Thread.ResetAbort();
                    }
                });

                t.Start();
                if (!t.Join(TimeSpan.FromSeconds(2)) || !result)
                {
                    if (t.IsAlive)
                    {
                        t.Abort();
                    }

                    Console.WriteLine("Try click via SendMessage");
                    Element.ClickViaSendMessage();
                }

                return this;
            }
        }

        public aQuery Text(string value)
        {
            using (PerformanceTester.Start(Log("(Set)" + nameof(Text), $"\"{value}\"")))
            {
                if (Element == null) return this;

                Element.SetText(value);

                return this;
            }
        }

        public aQuery DateTime(DateTime value)
        {
            using (PerformanceTester.Start(Log("(Set)" + nameof(DateTime), $"\"{value.ToShortDateString()}\"")))
            {
                if (Element == null) return this;

                Element.SetDateTime(value);

                return this;
            }
        }

        #endregion
    }
}
