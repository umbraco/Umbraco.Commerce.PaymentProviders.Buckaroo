using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Umbraco.Commerce.PaymentProviders.Buckaroo.Extensions
{
    internal static class StreamExtensions
    {
        public static async Task<byte[]> ToByteArrayAsync(this Stream stream, CancellationToken cancellationToken = default)
        {
            if (stream is MemoryStream memoryStream)
            {
                return memoryStream.ToArray();
            }

            using (MemoryStream ms = new())
            {
                await stream.CopyToAsync(ms, cancellationToken).ConfigureAwait(false);
                return ms.ToArray();
            }
        }
    }
}
