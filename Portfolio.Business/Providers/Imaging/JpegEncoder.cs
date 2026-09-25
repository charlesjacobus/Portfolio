using System;
using System.IO;
using System.Numerics;

namespace Portfolio.Business.Providers.Imaging
{
    /// <summary>
    /// Encodes images as baseline JPEG (JFIF), using YCbCr without chroma subsampling and the standard tables
    /// </summary>
    /// <remarks>
    /// JPEG has no alpha channel, so each pixel's alpha is ignored.
    /// </remarks>
    public static class JpegEncoder
    {
        public const int DefaultQuality = 75;

        private const int BlockSize = 8;
        private const int BlockLength = BlockSize * BlockSize;

        // Natural (row-major) index of each coefficient, in the order they're written
        private static readonly byte[] ZigZag =
        [
             0,  1,  8, 16,  9,  2,  3, 10, 17, 24, 32, 25, 18, 11,  4,  5,
            12, 19, 26, 33, 40, 48, 41, 34, 27, 20, 13,  6,  7, 14, 21, 28,
            35, 42, 49, 56, 57, 50, 43, 36, 29, 22, 15, 23, 30, 37, 44, 51,
            58, 59, 52, 45, 38, 31, 39, 46, 53, 60, 61, 54, 47, 55, 62, 63
        ];

        // Quantization and Huffman tables are the example tables from the JPEG specification (ITU T.81, Annex K)
        private static readonly byte[] LuminanceQuantization =
        [
            16, 11, 10, 16,  24,  40,  51,  61,
            12, 12, 14, 19,  26,  58,  60,  55,
            14, 13, 16, 24,  40,  57,  69,  56,
            14, 17, 22, 29,  51,  87,  80,  62,
            18, 22, 37, 56,  68, 109, 103,  77,
            24, 35, 55, 64,  81, 104, 113,  92,
            49, 64, 78, 87, 103, 121, 120, 101,
            72, 92, 95, 98, 112, 100, 103,  99
        ];

        private static readonly byte[] ChrominanceQuantization =
        [
            17, 18, 24, 47, 99, 99, 99, 99,
            18, 21, 26, 66, 99, 99, 99, 99,
            24, 26, 56, 99, 99, 99, 99, 99,
            47, 66, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99,
            99, 99, 99, 99, 99, 99, 99, 99
        ];

        private static readonly HuffmanTable LuminanceDc = new(
            [0, 1, 5, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0],
            [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);

        private static readonly HuffmanTable ChrominanceDc = new(
            [0, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0],
            [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11]);

        private static readonly HuffmanTable LuminanceAc = new(
            [0, 2, 1, 3, 3, 2, 4, 3, 5, 5, 4, 4, 0, 0, 1, 0x7d],
            [
                0x01, 0x02, 0x03, 0x00, 0x04, 0x11, 0x05, 0x12, 0x21, 0x31, 0x41, 0x06, 0x13, 0x51, 0x61, 0x07,
                0x22, 0x71, 0x14, 0x32, 0x81, 0x91, 0xa1, 0x08, 0x23, 0x42, 0xb1, 0xc1, 0x15, 0x52, 0xd1, 0xf0,
                0x24, 0x33, 0x62, 0x72, 0x82, 0x09, 0x0a, 0x16, 0x17, 0x18, 0x19, 0x1a, 0x25, 0x26, 0x27, 0x28,
                0x29, 0x2a, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49,
                0x4a, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69,
                0x6a, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7a, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89,
                0x8a, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9a, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6, 0xa7,
                0xa8, 0xa9, 0xaa, 0xb2, 0xb3, 0xb4, 0xb5, 0xb6, 0xb7, 0xb8, 0xb9, 0xba, 0xc2, 0xc3, 0xc4, 0xc5,
                0xc6, 0xc7, 0xc8, 0xc9, 0xca, 0xd2, 0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd8, 0xd9, 0xda, 0xe1, 0xe2,
                0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9, 0xea, 0xf1, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8,
                0xf9, 0xfa
            ]);

        private static readonly HuffmanTable ChrominanceAc = new(
            [0, 2, 1, 2, 4, 4, 3, 4, 7, 5, 4, 4, 0, 1, 2, 0x77],
            [
                0x00, 0x01, 0x02, 0x03, 0x11, 0x04, 0x05, 0x21, 0x31, 0x06, 0x12, 0x41, 0x51, 0x07, 0x61, 0x71,
                0x13, 0x22, 0x32, 0x81, 0x08, 0x14, 0x42, 0x91, 0xa1, 0xb1, 0xc1, 0x09, 0x23, 0x33, 0x52, 0xf0,
                0x15, 0x62, 0x72, 0xd1, 0x0a, 0x16, 0x24, 0x34, 0xe1, 0x25, 0xf1, 0x17, 0x18, 0x19, 0x1a, 0x26,
                0x27, 0x28, 0x29, 0x2a, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
                0x49, 0x4a, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59, 0x5a, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68,
                0x69, 0x6a, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79, 0x7a, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87,
                0x88, 0x89, 0x8a, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98, 0x99, 0x9a, 0xa2, 0xa3, 0xa4, 0xa5,
                0xa6, 0xa7, 0xa8, 0xa9, 0xaa, 0xb2, 0xb3, 0xb4, 0xb5, 0xb6, 0xb7, 0xb8, 0xb9, 0xba, 0xc2, 0xc3,
                0xc4, 0xc5, 0xc6, 0xc7, 0xc8, 0xc9, 0xca, 0xd2, 0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd8, 0xd9, 0xda,
                0xe2, 0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9, 0xea, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8,
                0xf9, 0xfa
            ]);

        // Cosines[x, u] = C(u) / 2 * cos((2x + 1)uπ / 16), so a separable pass yields the standard 8x8 DCT
        private static readonly float[,] Cosines = CreateCosines();

        public static void Encode(RasterImage image, Stream stream, int quality = DefaultQuality)
        {
            ArgumentNullException.ThrowIfNull(image);
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentOutOfRangeException.ThrowIfLessThan(quality, 1);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(quality, 100);

            var luminanceQuantization = ScaleQuantization(LuminanceQuantization, quality);
            var chrominanceQuantization = ScaleQuantization(ChrominanceQuantization, quality);

            WriteHeaders(stream, image, luminanceQuantization, chrominanceQuantization);

            var writer = new BitWriter(stream);

            var y = new float[BlockLength];
            var cb = new float[BlockLength];
            var cr = new float[BlockLength];
            var coefficients = new int[BlockLength];

            int previousY = 0, previousCb = 0, previousCr = 0;

            for (int top = 0; top < image.Height; top += BlockSize)
            {
                for (int left = 0; left < image.Width; left += BlockSize)
                {
                    LoadBlock(image, left, top, y, cb, cr);

                    previousY = EncodeBlock(writer, y, luminanceQuantization, previousY, LuminanceDc, LuminanceAc, coefficients);
                    previousCb = EncodeBlock(writer, cb, chrominanceQuantization, previousCb, ChrominanceDc, ChrominanceAc, coefficients);
                    previousCr = EncodeBlock(writer, cr, chrominanceQuantization, previousCr, ChrominanceDc, ChrominanceAc, coefficients);
                }
            }

            writer.Flush();

            WriteMarker(stream, 0xD9); // EOI
        }

        private static float[,] CreateCosines()
        {
            var cosines = new float[BlockSize, BlockSize];

            for (int x = 0; x < BlockSize; x++)
            {
                for (int u = 0; u < BlockSize; u++)
                {
                    var scale = u == 0 ? 1 / Math.Sqrt(2) : 1;
                    cosines[x, u] = (float)(scale / 2 * Math.Cos((2 * x + 1) * u * Math.PI / 16));
                }
            }

            return cosines;
        }

        private static byte[] ScaleQuantization(byte[] table, int quality)
        {
            // The IJG (libjpeg) quality scaling
            var scale = quality < 50 ? 5000 / quality : 200 - quality * 2;

            var result = new byte[BlockLength];
            for (int i = 0; i < BlockLength; i++)
            {
                result[i] = (byte)Math.Clamp((table[i] * scale + 50) / 100, 1, 255);
            }

            return result;
        }

        private static void LoadBlock(RasterImage image, int left, int top, float[] y, float[] cb, float[] cr)
        {
            var pixels = image.Pixels;

            for (int row = 0; row < BlockSize; row++)
            {
                // Blocks past the right or bottom edge repeat the edge pixels
                var sourceY = Math.Min(top + row, image.Height - 1);

                for (int column = 0; column < BlockSize; column++)
                {
                    var sourceX = Math.Min(left + column, image.Width - 1);
                    var i = (sourceY * image.Width + sourceX) * RasterImage.Channels;

                    float r = pixels[i], g = pixels[i + 1], b = pixels[i + 2];

                    // JFIF YCbCr, level-shifted to center on zero
                    var n = row * BlockSize + column;
                    y[n] = 0.299f * r + 0.587f * g + 0.114f * b - 128;
                    cb[n] = -0.168736f * r - 0.331264f * g + 0.5f * b;
                    cr[n] = 0.5f * r - 0.418688f * g - 0.081312f * b;
                }
            }
        }

        private static int EncodeBlock(BitWriter writer, float[] block, byte[] quantization, int previousDc,
            HuffmanTable dcTable, HuffmanTable acTable, int[] coefficients)
        {
            Transform(block, quantization, coefficients);

            WriteValue(writer, dcTable, 0, coefficients[0] - previousDc);

            var run = 0;
            for (int k = 1; k < BlockLength; k++)
            {
                if (coefficients[k] == 0)
                {
                    run++;
                    continue;
                }

                while (run > 15)
                {
                    writer.Write(acTable, 0xF0); // ZRL (sixteen zeros)
                    run -= 16;
                }

                WriteValue(writer, acTable, run << 4, coefficients[k]);
                run = 0;
            }

            if (run > 0)
            {
                writer.Write(acTable, 0x00); // EOB
            }

            return coefficients[0];
        }

        // Applies the forward DCT, then quantizes the coefficients into zigzag order
        private static void Transform(float[] block, byte[] quantization, int[] coefficients)
        {
            Array.Clear(coefficients);

            // Solid blocks (most of a leet) have only a DC coefficient: 8 × the sample value
            if (Array.TrueForAll(block, sample => sample == block[0]))
            {
                coefficients[0] = (int)Math.Round(8 * block[0] / quantization[0]);
                return;
            }

            Span<float> rows = stackalloc float[BlockLength];
            for (int y = 0; y < BlockSize; y++)
            {
                for (int u = 0; u < BlockSize; u++)
                {
                    var sum = 0f;
                    for (int x = 0; x < BlockSize; x++)
                    {
                        sum += Cosines[x, u] * block[y * BlockSize + x];
                    }
                    rows[y * BlockSize + u] = sum;
                }
            }

            for (int k = 0; k < BlockLength; k++)
            {
                var index = ZigZag[k];
                var v = index / BlockSize;
                var u = index % BlockSize;

                var sum = 0f;
                for (int y = 0; y < BlockSize; y++)
                {
                    sum += Cosines[y, v] * rows[y * BlockSize + u];
                }

                coefficients[k] = (int)Math.Round(sum / quantization[index]);
            }
        }

        private static void WriteValue(BitWriter writer, HuffmanTable table, int run, int value)
        {
            var category = 32 - BitOperations.LeadingZeroCount((uint)Math.Abs(value));

            writer.Write(table, run | category);

            if (category > 0)
            {
                // Negative values are written as their one's complement
                writer.Write(value < 0 ? value + (1 << category) - 1 : value, category);
            }
        }

        private static void WriteHeaders(Stream stream, RasterImage image, byte[] luminanceQuantization, byte[] chrominanceQuantization)
        {
            WriteMarker(stream, 0xD8); // SOI

            WriteSegment(stream, 0xE0, // APP0 (JFIF 1.01, square pixels)
            [
                (byte)'J', (byte)'F', (byte)'I', (byte)'F', 0,
                1, 1,
                0,
                0, 1, 0, 1,
                0, 0
            ]);

            var quantization = new byte[2 * (1 + BlockLength)];
            WriteQuantization(quantization, 0, 0, luminanceQuantization);
            WriteQuantization(quantization, 1 + BlockLength, 1, chrominanceQuantization);
            WriteSegment(stream, 0xDB, quantization); // DQT

            WriteSegment(stream, 0xC0, // SOF0 (8-bit, three components, no subsampling)
            [
                8,
                (byte)(image.Height >> 8), (byte)image.Height,
                (byte)(image.Width >> 8), (byte)image.Width,
                3,
                1, 0x11, 0,
                2, 0x11, 1,
                3, 0x11, 1
            ]);

            using (var huffman = new MemoryStream())
            {
                WriteHuffman(huffman, 0x00, LuminanceDc);
                WriteHuffman(huffman, 0x10, LuminanceAc);
                WriteHuffman(huffman, 0x01, ChrominanceDc);
                WriteHuffman(huffman, 0x11, ChrominanceAc);
                WriteSegment(stream, 0xC4, huffman.ToArray()); // DHT
            }

            WriteSegment(stream, 0xDA, // SOS
            [
                3,
                1, 0x00,
                2, 0x11,
                3, 0x11,
                0, 63, 0
            ]);
        }

        private static void WriteQuantization(byte[] buffer, int offset, int identifier, byte[] table)
        {
            buffer[offset] = (byte)identifier;

            for (int k = 0; k < BlockLength; k++)
            {
                buffer[offset + 1 + k] = table[ZigZag[k]];
            }
        }

        private static void WriteHuffman(Stream stream, byte identifier, HuffmanTable table)
        {
            stream.WriteByte(identifier);
            stream.Write(table.Bits);
            stream.Write(table.Values);
        }

        private static void WriteMarker(Stream stream, byte marker)
        {
            stream.WriteByte(0xFF);
            stream.WriteByte(marker);
        }

        private static void WriteSegment(Stream stream, byte marker, byte[] data)
        {
            WriteMarker(stream, marker);

            var length = data.Length + 2;
            stream.WriteByte((byte)(length >> 8));
            stream.WriteByte((byte)length);
            stream.Write(data);
        }

        private sealed class HuffmanTable
        {
            public HuffmanTable(byte[] bits, byte[] values)
            {
                Bits = bits;
                Values = values;

                // Canonical codes: consecutive within each length, then shifted left for the next length
                var code = 0;
                var k = 0;
                for (int length = 1; length <= bits.Length; length++)
                {
                    for (int i = 0; i < bits[length - 1]; i++)
                    {
                        Codes[values[k]] = (ushort)code;
                        Lengths[values[k]] = (byte)length;

                        code++;
                        k++;
                    }

                    code <<= 1;
                }
            }

            public byte[] Bits { get; }

            public byte[] Values { get; }

            public ushort[] Codes { get; } = new ushort[256];

            public byte[] Lengths { get; } = new byte[256];
        }

        private sealed class BitWriter(Stream stream)
        {
            private readonly Stream _stream = stream;

            private uint _buffer;
            private int _count;

            public void Write(HuffmanTable table, int symbol)
            {
                Write(table.Codes[symbol], table.Lengths[symbol]);
            }

            public void Write(int bits, int length)
            {
                _buffer = (_buffer << length) | ((uint)bits & ((1u << length) - 1));
                _count += length;

                while (_count >= 8)
                {
                    var value = (byte)(_buffer >> (_count - 8));
                    _stream.WriteByte(value);

                    // A 0xFF byte in the scan data is followed by a zero, so it isn't read as a marker
                    if (value == 0xFF)
                    {
                        _stream.WriteByte(0);
                    }

                    _count -= 8;
                }
            }

            public void Flush()
            {
                // Pad the final byte with ones
                if (_count > 0)
                {
                    var padding = 8 - _count;
                    Write((1 << padding) - 1, padding);
                }
            }
        }
    }
}
