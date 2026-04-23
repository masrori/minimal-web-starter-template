using System.Text;

namespace Orchestrate.Extensions
{
    public static class ByteArrayExtensions
    {
        public static string ToHexString(this byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (var b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }
        public static byte[] SubBytes(this byte[] input, int start, int length)
        {
            if (input.Length < start + length)
                throw new ArgumentException("Key length is insufficient for the selected encryption type.");

            byte[] result = new byte[length];
            Array.Copy(input, start, result, 0, length);
            return result;
        }
        public static void Copy(this byte[] data, byte[] newValue, int startIndex)
        {
            if (data is null) throw new ArgumentNullException(nameof(data));
            if (newValue is null) throw new ArgumentNullException(nameof(newValue));
            if (startIndex < 0 || startIndex + newValue.Length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            Buffer.BlockCopy(newValue, 0, data, startIndex, newValue.Length);
        }
        public static byte[][] Split(this byte[] input, byte separator)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var result = new List<byte[]>();
            int start = 0;

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == separator)
                {
                    int length = i - start;
                    byte[] segment = new byte[length];
                    Buffer.BlockCopy(input, start, segment, 0, length);
                    result.Add(segment);
                    start = i + 1;
                }
            }

            if (start <= input.Length)
            {
                int length = input.Length - start;
                byte[] segment = new byte[length];
                Buffer.BlockCopy(input, start, segment, 0, length);
                result.Add(segment);
            }

            return result.ToArray();
        }

    }
}
