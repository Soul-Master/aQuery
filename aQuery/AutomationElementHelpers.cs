using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Forms;
using aQuery.Win32;

namespace aQuery
{
    public static class AutomationElementHelpers
    {
        public static IEnumerable<AutomationElement> GetChildren(this AutomationElement element)
        {
            // Conditions for the basic views of the subtree (content, control, and raw) 
            // are available as fields of TreeWalker, and one of these is used in the 
            // following code.
            var elementNode = TreeWalker.ControlViewWalker.GetFirstChild(element);

            while (elementNode != null)
            {
                yield return elementNode;

                elementNode = TreeWalker.ControlViewWalker.GetNextSibling(elementNode);
            }
        }

        public static T GetPattern<T>(this AutomationElement element)
            where T : BasePattern
        {
            object objPattern;
            var patternType = typeof(T);
            var pattern = patternType.GetField("Pattern");
            var patternValue = (AutomationPattern)pattern.GetValue(null);

            if (!element.TryGetCurrentPattern(patternValue, out objPattern)) return null;

            return (T)objPattern;
        }

        private static string GetElementSelector(this AutomationElement element)
        {
            var selector = string.Empty;
            var el = element.Current;
            selector += "'" + (!string.IsNullOrEmpty(el.Name) ? el.Name : string.Empty) + "' ";
            selector += !string.IsNullOrEmpty(el.LocalizedControlType) ? el.LocalizedControlType : "";
            selector += !string.IsNullOrEmpty(el.ClassName) ? $"[ClassName={el.ClassName}]" : "[ClassName=]";

            return selector;
        }

        public static List<AutomationElement> GetParentsAndSelf(this AutomationElement element)
        {
            var result = new List<AutomationElement>
            {
                element
            };
            var walker = TreeWalker.ControlViewWalker;
            var parentElement = element;

            do
            {
                parentElement = walker.GetParent(parentElement);

                if (parentElement == null || parentElement == AutomationElement.RootElement) break;
                result.Add(parentElement);
            } while (true);

            return result;
        }

        public static string GetSelector(this AutomationElement element)
        {
            var temp = new List<string>();
            element.GetParentsAndSelf().ForEach(x =>
            {
                temp.Add(x.GetElementSelector());
            });

            return temp.Count > 1 ? temp.Aggregate((i, j) => j + SelectorItem.ChildrenSeparator + i) : temp[0];
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static List<AutomationElement> Find(this AutomationElement element, List<SelectorItem> selectorItems, int index)
        {
            var result = new List<AutomationElement>();
            var selectorItem = selectorItems[index];
            var condition = selectorItem.SelectorCondition;
            var matchedElements = element.FindAll(selectorItem.Scope, condition.NativeCondition).Cast<AutomationElement>();

            if (!matchedElements.Any()) return result;
            if (condition.CustomConditions.Count > 0)
            {
                matchedElements = matchedElements.Where(x => condition.CustomConditions.All(y => y.IsMatch(x)));
            }
            if (condition.CustomFilters.Count > 0)
            {
                matchedElements = condition.CustomFilters.Aggregate(matchedElements, (current, filter) => filter.Filter(current));
            }

            if (index == selectorItems.Count - 1)
            {
                return matchedElements.ToList();
            }

            foreach (var el in matchedElements)
            {
                var subResult = el.Find(selectorItems, index + 1);

                if (subResult != null && subResult.Count > 0)
                {
                    result.AddRange(subResult);
                }
            }

            return result;
        }

        public static List<AutomationElement> Find(this List<AutomationElement> elements, string selector, int maxRetry = 20)
        {
            List<AutomationElement> result = new List<AutomationElement>();
            var selectorItems = SelectorItem.SplitSelector(selector);
            var counter = maxRetry;

            while (counter >= 0)
            {
                foreach (AutomationElement el in elements)
                {
                    var subResult = el.Find(selectorItems, 0);

                    if (subResult != null && subResult.Count > 0)
                    {
                        result.AddRange(subResult);
                    }
                }

                if (result.Count > 0) return result;
                counter--;

                if (counter >= 0)
                {
                    Thread.Sleep(50);
                }
            }

            return result;
        }

        public static List<AutomationElement> Find(this AutomationElement element, string selector, int maxRetry = 19)
        {
            List<AutomationElement> result = null;
            var selectorItems = SelectorItem.SplitSelector(selector);
            var counter = maxRetry;

            while (counter >= 0)
            {
                result = element.Find(selectorItems, 0);

                if (result.Count > 0) return result;
                counter--;

                if (counter >= 0)
                {
                    Thread.Sleep(50);
                }
            }

            return result;
        }

        public static string GetValue(this AutomationElement element)
        {
            var patternObj = element.GetPattern<ValuePattern>();

            return patternObj?.Current.Value;
        }

        public static string GetText(this AutomationElement element)
        {
            var patternObj = element.GetPattern<TextPattern>();

            if (patternObj != null)
            {
                return patternObj.DocumentRange.GetText(-1).TrimEnd('\r'); // often there is an extra '\r' hanging off the end.
            }

            var result = element.GetValue();

            return !string.IsNullOrEmpty(result) ? result : element.Current.Name;
        }

        public static bool IsVisible(this AutomationElement element)
        {
            return !element.Current.IsOffscreen;
        }

        public static DataTable GetDataTable(this AutomationElement element)
        {
            var dt = new DataTable();
            element.Find("header > header item").ForEach(x =>
            {
                dt.Columns.Add(x.GetText(), typeof(string));
            });
            element.Find("item").ForEach(x =>
            {
                var row = dt.NewRow();
                var texts = x.Find("text");

                for (var i = 0; i < texts.Count; i++)
                {
                    row[i] = texts[i].GetText();
                }
                dt.Rows.Add(row);
            });

            return dt;
        }

        #region Action

        public static bool Click(this AutomationElement element)
        {
            var patternObj = element.GetPattern<InvokePattern>();

            if (patternObj == null) return false;

            patternObj.Invoke();
            return true;
        }

        /// <summary>
        /// Trigger click event by moving mouse to clickable point and clicking
        /// </summary>
        public static bool MouseClick(this AutomationElement element)
        {
            try
            {
                var p = element.GetClickablePoint();
                Win32Helpers.Click(element.Current.NativeWindowHandle, p);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Trigger click event by moving mouse to clickable point and clicking
        /// </summary>
        public static bool MouseClick(this Point point, int windowHandle)
        {
            try
            {
                Win32Helpers.Click(windowHandle, point);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void SetText(this AutomationElement element, string value)
        {
            // Validate arguments / initial setup
            if (value == null)
                throw new ArgumentNullException(nameof(value), "String parameter must not be null.");

            if (!element.Current.IsEnabled)
            {
                throw new InvalidOperationException(
                    "The control with an AutomationID of "
                    + element.Current.AutomationId
                    + " is not enabled.\n\n");
            }

            if (!element.Current.IsKeyboardFocusable)
            {
                throw new InvalidOperationException(
                    "The control with an AutomationID of "
                    + element.Current.AutomationId
                    + "is read-only.\n\n");
            }

            var valuePattern = element.GetPattern<ValuePattern>();
            if (valuePattern != null)
            {
                // Control supports the ValuePattern pattern so we can 
                // use the SetValue method to insert content.
                // Set focus for input functionality and begin.
                element.SetFocus();
                valuePattern.SetValue(value);
            }
            else
            {
                // Set focus for input functionality and begin.
                element.SetFocus();

                // Pause before sending keyboard input.
                Thread.Sleep(100);

                // Delete existing content in the control and insert new content.
                SendKeys.SendWait("^{HOME}");   // Move to start of control
                SendKeys.SendWait("^+{END}");   // Select everything
                SendKeys.SendWait("{DEL}");     // Delete selection
                SendKeys.SendWait(value);
            }
        }

        public static void SetDateTime(this AutomationElement element, DateTime value)
        {
            var sysTime = new SYSTEMTIME(value);
            var structMemLen = Marshal.SizeOf(typeof(SYSTEMTIME));
            var buffer = new byte[structMemLen];

            //Assign the values as you prefer
            var dataPtr = Marshal.AllocHGlobal(structMemLen);
            Marshal.StructureToPtr(sysTime, dataPtr, true);
            Marshal.Copy(dataPtr, buffer, 0, structMemLen);
            Marshal.FreeHGlobal(dataPtr);

            var hndProc = IntPtr.Zero;
            var lpAddress = IntPtr.Zero;
            try
            {
                Win32Helpers.InjectMemory(element.Current.ProcessId, buffer, out hndProc, out lpAddress);
                SafeNativeMethods.SendMessage((IntPtr)element.Current.NativeWindowHandle, NativeMethods.DTM_SETSYSTEMTIME, (IntPtr)NativeMethods.GDT_VALID, lpAddress);
            }
            finally
            {
                // release memory and close handle
                if (lpAddress != (IntPtr)0 || lpAddress != IntPtr.Zero)
                {
                    // we don't really care about the result because if release fails there is nothing we can do about it
                    SafeNativeMethods.VirtualFreeEx(hndProc, lpAddress, 0, FreeType.Release);
                }

                if (hndProc != (IntPtr)0 || hndProc != IntPtr.Zero)
                {
                    SafeNativeMethods.CloseHandle(hndProc);
                }
            }
        }

        public static bool Select(this AutomationElement element)
        {
            var patternObj = element.GetPattern<SelectionItemPattern>();

            if (patternObj == null) return false;

            patternObj.Select();
            return true;
        }

        public static void ShowButtonMenu(this AutomationElement element)
        {
            Point point;
            var bound = element.Current.BoundingRectangle;
            var handle = element.Current.NativeWindowHandle;

            element.TryGetClickablePoint(out point);
            point.X = bound.TopRight.X - 5;
            point.MouseClick(handle);
        }

        #endregion
    }
}