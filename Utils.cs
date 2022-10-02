namespace Place
{
    public class Utils
    {
        internal static bool IsValidDate(uint year, uint month, uint day)
        {
            return year >= 2000 && year <= 9999
                                && month >= 1 && month <= 12
                                && day >= 1 && day <= 31;
        }

        internal static bool IsValidName(string name)
        {
            return true;
        }
    }
}