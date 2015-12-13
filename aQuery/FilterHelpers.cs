namespace aQuery
{
    public static class FilterHelpers
    {
        public static ICustomFilter CreateFilter(string filterName, string filterValue)
        {
            switch (filterName)
            {
                case "eq": return new ElementAtFilter(filterValue);
                case "last": return new LastFilter();
                case "first": return new FirstFilter();
                case "visible": return new VisibleOrder();
                case "orderByColumnIndex": return new ColumnIndexOrder();
                case "orderByAutomationNo": return new AutomationNoOrder();
                case "orderByHPositionOrder": return new HorizontalPositionOrder();
            }

            return null;
        }
    }
}