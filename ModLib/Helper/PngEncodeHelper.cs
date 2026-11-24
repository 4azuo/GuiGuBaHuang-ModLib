using ModLib.Attributes;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ModLib.Helper
{
    /// <summary>
    /// Minimal PNG encoder (RGBA, no compression) used to export textures without Unity image modules.
    /// </summary>
    [ActionCatIgn]
    public static class PngEncodeHelper
    {
        /// <summary>
        /// Encodes raw RGBA bytes into a PNG file.
        /// </summary>
        public static byte[] EncodeRGBA(byte[] rgba, uint width, uint height)
        {
            using (var ms = new MemoryStream())
            {
                // signature as bytes: 0x89 is not ASCII, Encoding.ASCII wrote '?' (BUG-07)
                ms.Write(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, 0, 8);

                WriteChunk(ms, "IHDR", CreateIHDR(width, height));
                WriteChunk(ms, "IDAT", CreateIDAT(rgba, width, height));
                WriteChunk(ms, "IEND", new byte[0]);

                return ms.ToArray();
            }
        }

        /// <summary>
        /// Builds the IHDR chunk.
        /// </summary>
        static byte[] CreateIHDR(uint w, uint h)
        {
            using (var ms = new MemoryStream())
            {
                WriteInt(ms, w);
                WriteInt(ms, h);
                ms.WriteByte(8); // bit depth
                ms.WriteByte(6); // RGBA
                ms.WriteByte(0);
                ms.WriteByte(0);
                ms.WriteByte(0);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Builds the IDAT chunk (stored deflate blocks).
        /// </summary>
        static byte[] CreateIDAT(byte[] rgba, uint w, uint h)
        {
            using (var raw = new MemoryStream())
            {
                int stride = (int)w * 4;
                for (var y = 0; y < h; y++)
                {
                    raw.WriteByte(0); // filter
                    raw.Write(rgba, y * stride, stride);
                }

                using (var comp = new MemoryStream())
                {
                    using (var ds = new DeflateStream(comp, CompressionLevel.Fastest, true))
                    {
                        raw.Position = 0; 
                        raw.CopyTo(ds);
                    }

                    return comp.ToArray();
                }
            }
        }

        /// <summary>
        /// Writes one PNG chunk with its CRC.
        /// </summary>
        static void WriteChunk(Stream s, string type, byte[] data)
        {
            WriteInt(s, (uint)data.Length);
            Write(s, type);
            s.Write(data, 0, data.Length);
            WriteInt(s, CRC(type, data));
        }

        /// <summary>
        /// Writes bytes to the stream.
        /// </summary>
        static void Write(Stream s, string str)
            => s.Write(Encoding.ASCII.GetBytes(str), 0, str.Length);

        /// <summary>
        /// Writes a big-endian int.
        /// </summary>
        static void WriteInt(Stream s, uint v)
        {
            s.WriteByte((byte)(v >> 24));
            s.WriteByte((byte)(v >> 16));
            s.WriteByte((byte)(v >> 8));
            s.WriteByte((byte)v);
        }

        /// <summary>
        /// Computes the CRC32 of a buffer.
        /// </summary>
        static uint CRC(string type, byte[] data)
        {
            uint crc = 0xffffffff;
            foreach (byte b in Encoding.ASCII.GetBytes(type))
                crc = UpdateCRC(crc, b);
            foreach (byte b in data)
                crc = UpdateCRC(crc, b);
            return crc ^ 0xffffffff;
        }

        /// <summary>
        /// Updates a running CRC32.
        /// </summary>
        static uint UpdateCRC(uint c, byte b)
        {
            c ^= b;
            for (int k = 0; k < 8; k++)
                c = (c & 1) != 0 ? 0xedb88320 ^ (c >> 1) : c >> 1;
            return c;
        }
    }
}
