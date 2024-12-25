namespace DocumentDBClient
{
    internal class MyConvert
    {
        /// <summary>
        /// Convert any object to string with handle null and DBNull, if any invalid value then return string.Empty value.
        /// </summary>
        /// <param name="Input">object input</param>
        /// <returns>converted string value</returns>
        public static string ToString(object Input)
        {
            if (Input == null || Input == DBNull.Value)
                return string.Empty;
            else
            {
                try
                {
                    return Convert.ToString(Input);
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
    }
}
