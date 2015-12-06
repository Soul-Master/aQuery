using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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
            var a = new aQuery(element);
            a.Element.GetParentsAndSelf().ForEach(x =>
            {
                temp.Add(x.GetElementSelector());
            });

            return temp.Count > 1 ? temp.Aggregate((i, j) => j + SelectorItem.ChildrenSeparator + i) : temp[0];
        }

        public static aQuery Find(this AutomationElement element, List<SelectorItem> selectorItems)
        {
            var index = 0;
            var result = new aQuery();

            while (index < selectorItems.Count)
            {
                var selectorItem = selectorItems[index];
                var matchedElement = element.FindFirst(selectorItem.Scope, selectorItem.GetCondition());

                if (matchedElement == null) break;

                if (index == selectorItems.Count - 1)
                {
                    result.Element = matchedElement;
                }
                else
                {
                    element = matchedElement;
                }
                index++;
            }

            return result;
        }
        
        public static aQuery Find(this AutomationElement element, string selector, int maxRetry = 19)
        {
            aQuery result = null;
            var selectorItems = SelectorItem.SplitSelector(selector);
            var counter = maxRetry;

            while (counter >= 0)
            {
                result = element.Find(selectorItems);

                if (result.IsExists) return result;
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
            object patternObj;
            if (element.TryGetCurrentPattern(ValuePattern.Pattern, out patternObj))
            {
                var valuePattern = (ValuePattern)patternObj;
                return valuePattern.Current.Value;
            }

            return null;
        }

        public static string GetText(this AutomationElement element)
        {
            object patternObj;

            if (element.TryGetCurrentPattern(TextPattern.Pattern, out patternObj))
            {
                var textPattern = (TextPattern)patternObj;
                return textPattern.DocumentRange.GetText(-1).TrimEnd('\r'); // often there is an extra '\r' hanging off the end.
            }

            var result = element.GetValue();

            return !string.IsNullOrEmpty(result) ? result : element.Current.Name;
        }

        public static bool IsVisible(this AutomationElement element)
        {
            return !element.Current.IsOffscreen;
        }

        #region Action

        public static bool Click(this AutomationElement element)
        {
            object objPattern;

            if (!element.TryGetCurrentPattern(InvokePattern.Pattern, out objPattern))
            {
                return false;
            }

            var invPattern = (InvokePattern)objPattern;
            invPattern?.Invoke();

            return true;
        }

        public static bool ClickViaSendMessage(this AutomationElement element)
        {
            try
            {
                var p = element.GetClickablePoint();
                Win32Helpers.Click(p);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot Click via SendMessage");
                Console.WriteLine(ex.Message);
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


            object valuePattern;
            if (!element.TryGetCurrentPattern(
                ValuePattern.Pattern, out valuePattern))
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
            else
            {
                // Control supports the ValuePattern pattern so we can 
                // use the SetValue method to insert content.
                // Set focus for input functionality and begin.
                element.SetFocus();

                ((ValuePattern)valuePattern).SetValue(value);
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

        #endregion
    }
}