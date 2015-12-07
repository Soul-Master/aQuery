namespace aQuery
{
    public static class FilterHelpers
    {
        public static ICustomFilter CreateFilter(string filterName, string filterValue)
        {
            switch (filterName)
            {
                case "eq": return new ElementAtFilter(filterValue);
            }

            return null;
        }
    }
}