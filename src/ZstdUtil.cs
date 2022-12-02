using System;
using System.Text;
using ZstdNet;

namespace CCLua
{
    public static class ZstdUtil
    {
        public static byte[] CompressToBytes(string source)
        {
            return ZstdCompress(Encoding.UTF8.GetBytes(source)).ToArray();
        }

        public static string DecompressFromBytes(byte[] compressed)
        {
            return Encoding.UTF8.GetString(ZstdDecompress(compressed).ToArray());
        }

        private static ReadOnlySpan<byte> ZstdCompress(byte[] bytes)
        {
            var options = new CompressionOptions(5);
            using (var comp = new Compressor(options))
            {
                return comp.Wrap(bytes);
            }
        }

        private static ReadOnlySpan<byte> ZstdDecompress(byte[] bytes)
        {
            using (var decomp = new Decompressor())
            {
                return decomp.Unwrap(bytes);
            }
        }
    }
}
