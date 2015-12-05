using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms;

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
            selector += !string.IsNullOrEmpty(el.LocalizedControlType) ? el.LocalizedControlType : "(null)";

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

        private const int MaxTryAmount = 40;
        public static aQuery Find(this AutomationElement element, string selector)
        {
            aQuery result = null;
            var selectorItems = SelectorItem.SplitSelector(selector);
            var counter = MaxTryAmount;

            while (counter > 0)
            {
                var attempt = MaxTryAmount - counter + 1;
                if (attempt > 1)
                {
                    Console.WriteLine("attempt " + attempt);
                }

                result = element.Find(selectorItems);

                if (result.IsExists) return result;
                counter--;

                if (attempt != MaxTryAmount)
                {
                    Thread.Sleep(50);
                }
            }

            Console.WriteLine("Cannot find element");
            return result;
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

        #endregion
    }
}