namespace InvenageAPI.Services.Extension
{
    public static class StringExtensions
    {
        public static bool IsNullOrEmpty(this string input)
            => string.IsNullOrEmpty(input);
    }
}
