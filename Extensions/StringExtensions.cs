namespace Orchestrate.Extensions
{
    public static class StringExtensions
    {
        public static byte[] ToInet(this string value)
        {
            var arr = value.Split('.');
            var bytes = new byte[4];
            if (arr.Length != 4) return bytes;

            for (int i = 0; i < 4; i++)
            {
                byte.TryParse(arr[i], out bytes[i]);
            }
            return bytes;
        }
        public static void Log(this string value)
        {
            string dir = System.IO.Path.Combine(AppContext.BaseDirectory, "error.txt");
            try
            {
                System.IO.File.AppendAllText(dir, "INFO:\n" + DateTime.Now.ToString() + "\t\t\t" + value);
            } catch(Exception) { }
        }
    }
}
