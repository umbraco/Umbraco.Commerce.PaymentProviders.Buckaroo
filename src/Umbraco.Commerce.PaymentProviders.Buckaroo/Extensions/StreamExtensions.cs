using System.IO;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo.Extensions
{
    internal static class StreamExtensions
    {
        public static byte[] ToByteArray(this Stream stream)
        {
            if (stream is MemoryStream memoryStream)
            {
                return memoryStream.ToArray();
            }

            using (MemoryStream ms = new())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }
}
