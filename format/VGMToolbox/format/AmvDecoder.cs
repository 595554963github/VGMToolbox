#nullable disable
using System;
using System.IO;
using System.Text;

namespace VGMToolbox.format
{
    public class AmvDecoder
    {
        public int DecodeAmvFile(string filePath, string outputPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string outDir = outputPath;

            using var amv = AmvFile.Open(filePath);
            var dec = new AmvFrameDecoder(amv.Header, amv.Mode, amv.Qtables);
            int count = 0;
            int totalFrames = (int)amv.Header.FrameCount;

            var it = amv.IterFrames();
            FramePacket pkt;
            while ((pkt = it.ReadNext()) != null)
            {
                var frame = dec.Decode(pkt);
                string ppmFileName = $"{fileName}_{frame.Index:D4}.ppm";
                string path = Path.Combine(outDir, ppmFileName);
                WritePpm(path, frame.Width, frame.Height, frame.Rgba);
                count++;

                if (count % 10 == 0)
                {
                    Console.WriteLine($"正在提取{fileName}: {count}/{totalFrames}帧");
                }
            }

            return count;
        }

        private static void WritePpm(string path, ushort w, ushort h, byte[] rgba)
        {
            using var fs = File.Create(path);

            var header = Encoding.ASCII.GetBytes($"P6\n{w} {h}\n255\n");
            fs.Write(header, 0, header.Length);

            byte[] rgbData = new byte[(rgba.Length / 4) * 3];
            for (int i = 0, j = 0; i < rgba.Length; i += 4, j += 3)
            {
                rgbData[j] = rgba[i];
                rgbData[j + 1] = rgba[i + 1];
                rgbData[j + 2] = rgba[i + 2];
            }

            fs.Write(rgbData, 0, rgbData.Length);
            fs.Flush();
        }

        private enum AmvMode { A, B }

        private class AmvHeader
        {
            public uint Magic, Unk04, Revision, HeaderSize, Unk10, FrameCount, FpsNum, FpsDen;
            public ushort Width, Height;
            public byte Attr;

            public AmvMode Mode()
            {
                if ((Attr & 1) != 0) return AmvMode.B;
                if ((Attr & 2) != 0) return AmvMode.A;
                throw new InvalidOperationException($"无效的属性:0x{Attr:x2}");
            }

            public int QTableSize() => Mode() == AmvMode.A ? 128 : 192;

            public void Validate()
            {
                if (Magic != 0x4D504A41) throw new InvalidOperationException($"错误的魔数:0x{Magic:x8}");
                if (Revision != 0) throw new InvalidOperationException($"不支持的版本:{Revision}");
                if (HeaderSize != 168 && HeaderSize != 232) throw new InvalidOperationException($"意外的头部大小:{HeaderSize}");
                if (FrameCount == 0) throw new InvalidOperationException("帧数为零");
                if (FpsNum == 0 || FpsDen == 0) throw new InvalidOperationException($"无效的帧率:{FpsNum}/{FpsDen}");
                if (Width == 0 || Height == 0) throw new InvalidOperationException($"无效的尺寸:{Width}x{Height}");
                if ((int)HeaderSize - 40 != QTableSize())
                    throw new InvalidOperationException("头部大小/量化表不匹配");
            }
        }

        private class QuantTables
        {
            public byte[] Q0, Q1;
            public byte[] Q2;

            public QuantTables(byte[] q0, byte[] q1, byte[] q2) { Q0 = q0; Q1 = q1; Q2 = q2; }

            public static QuantTables Parse(AmvMode mode, byte[] bytes)
            {
                static byte[] Take64(byte[] b, int s) { var r = new byte[64]; Array.Copy(b, s, r, 0, 64); return r; }
                if (mode == AmvMode.A)
                {
                    if (bytes.Length != 128) throw new InvalidOperationException($"模式A期望128个量化表字节,实际得到{bytes.Length}");
                    return new QuantTables(Take64(bytes, 0), Take64(bytes, 64), null);
                }
                if (bytes.Length != 192) throw new InvalidOperationException($"模式B期望192个量化表字节,实际得到{bytes.Length}");
                return new QuantTables(Take64(bytes, 0), Take64(bytes, 64), Take64(bytes, 128));
            }
        }

        private class FrameAHeader { public uint FrameId; public short X, Y; public ushort W, H; }
        private class FrameBHeader { public uint FrameId; public short P0, P1; public ushort W, H; }

        private abstract class FramePacketKind
        {
            public class A : FramePacketKind { public FrameAHeader Header; public uint Seg0Len, Seg1Len; }
            public class B : FramePacketKind { public FrameBHeader Header; public uint PayloadLen; }
        }

        private class FramePacket
        {
            public uint Index, Tag, ChunkSize;
            public byte[] RawPacket;
            public FramePacketKind Kind;
            public byte[] Seg0, Seg1;

            public FrameAHeader HeaderA() => (Kind as FramePacketKind.A)?.Header;
            public FrameBHeader HeaderB() => (Kind as FramePacketKind.B)?.Header;
            public byte[] PayloadB() => Kind is FramePacketKind.B && RawPacket.Length >= 20 ? RawPacket[20..] : null;
        }

        private class AmvFile : IDisposable
        {
            public AmvHeader Header;
            public AmvMode Mode;
            public QuantTables Qtables;
            private readonly Stream _stream;

            private AmvFile(Stream stream) { _stream = stream; }
            public void Dispose() { _stream.Dispose(); }

            public static AmvFile Open(string path) => FromStream(File.OpenRead(path));

            public static AmvFile FromStream(Stream stream)
            {
                var f = new AmvFile(stream);
                var buf = new byte[40];
                ReadExact(stream, buf);
                f.Header = ParseHeader(buf);
                f.Header.Validate();
                f.Mode = f.Header.Mode();
                var qbuf = new byte[f.Header.QTableSize()];
                ReadExact(stream, qbuf);
                f.Qtables = QuantTables.Parse(f.Mode, qbuf);
                stream.Seek(f.Header.HeaderSize, SeekOrigin.Begin);
                return f;
            }

            public FrameIterator IterFrames() => new(_stream, Mode);

            private static AmvHeader ParseHeader(byte[] b) => new AmvHeader
            {
                Magic = BitConverter.ToUInt32(b, 0), Unk04 = BitConverter.ToUInt32(b, 4),
                Revision = BitConverter.ToUInt32(b, 8), HeaderSize = BitConverter.ToUInt32(b, 12),
                Unk10 = BitConverter.ToUInt32(b, 16), FrameCount = BitConverter.ToUInt32(b, 20),
                FpsNum = BitConverter.ToUInt32(b, 24), FpsDen = BitConverter.ToUInt32(b, 28),
                Width = BitConverter.ToUInt16(b, 32), Height = BitConverter.ToUInt16(b, 34),
                Attr = b[36]
            };

            private static void ReadExact(Stream s, byte[] buf)
            {
                int off = 0;
                while (off < buf.Length)
                {
                    int n = s.Read(buf, off, buf.Length - off);
                    if (n == 0) throw new EndOfStreamException();
                    off += n;
                }
            }
        }

        private class FrameIterator
        {
            private readonly Stream _r;
            private readonly AmvMode _mode;
            private uint _idx;

            public FrameIterator(Stream r, AmvMode mode) { _r = r; _mode = mode; }

            public FramePacket ReadNext()
            {
                long startOff;
                try { startOff = _r.Seek(0, SeekOrigin.Current); } catch { startOff = _r.Position; }

                int hdrSize = _mode == AmvMode.A ? 24 : 20;
                var hdr = new byte[hdrSize];
                if (Read(_r, hdr) < hdrSize) return null;

                if (_mode == AmvMode.A)
                {
                    uint tag = BitConverter.ToUInt32(hdr, 0);
                    uint chunkSize = BitConverter.ToUInt32(hdr, 4);
                    var fh = new FrameAHeader
                    {
                        FrameId = BitConverter.ToUInt32(hdr, 8),
                        X = BitConverter.ToInt16(hdr, 12), Y = BitConverter.ToInt16(hdr, 14),
                        W = BitConverter.ToUInt16(hdr, 16), H = BitConverter.ToUInt16(hdr, 18)
                    };
                    uint seg0Len = BitConverter.ToUInt32(hdr, 20);
                    if (chunkSize < 16 || seg0Len > chunkSize - 16)
                        throw new InvalidOperationException($"帧{_idx}:无效的大小");
                    Validate16(fh.W, fh.H);
                    uint seg1Len = (chunkSize - 16) - seg0Len;
                    var seg0 = new byte[seg0Len]; ReadExact2(_r, seg0);
                    var seg1 = new byte[seg1Len]; ReadExact2(_r, seg1);
                    var raw = new byte[hdrSize + seg0.Length + seg1.Length];
                    Array.Copy(hdr, 0, raw, 0, hdrSize);
                    Array.Copy(seg0, 0, raw, hdrSize, seg0.Length);
                    Array.Copy(seg1, 0, raw, hdrSize + seg0.Length, seg1.Length);
                    return new FramePacket
                    {
                        Index = _idx++, Tag = tag, ChunkSize = chunkSize, RawPacket = raw,
                        Kind = new FramePacketKind.A { Header = fh, Seg0Len = seg0Len, Seg1Len = seg1Len },
                        Seg0 = seg0, Seg1 = seg1
                    };
                }
                else
                {
                    uint tag = BitConverter.ToUInt32(hdr, 0);
                    uint chunkSize = BitConverter.ToUInt32(hdr, 4);
                    var fh = new FrameBHeader
                    {
                        FrameId = BitConverter.ToUInt32(hdr, 8),
                        P0 = BitConverter.ToInt16(hdr, 12), P1 = BitConverter.ToInt16(hdr, 14),
                        W = BitConverter.ToUInt16(hdr, 16), H = BitConverter.ToUInt16(hdr, 18)
                    };
                    Validate16(fh.W, fh.H);
                    if (chunkSize < 12) throw new InvalidOperationException($"帧{_idx}:无效的块大小");
                    int payloadLen = (int)(chunkSize - 12);
                    var payload = new byte[payloadLen]; ReadExact2(_r, payload);
                    var raw = new byte[hdrSize + payload.Length];
                    Array.Copy(hdr, 0, raw, 0, hdrSize);
                    Array.Copy(payload, 0, raw, hdrSize, payload.Length);
                    return new FramePacket
                    {
                        Index = _idx++, Tag = tag, ChunkSize = chunkSize, RawPacket = raw,
                        Kind = new FramePacketKind.B { Header = fh, PayloadLen = (uint)payloadLen },
                        Seg0 = null, Seg1 = null
                    };
                }
            }

            private static void Validate16(ushort w, ushort h)
            {
                if ((w & 0xF) != 0 || (h & 0xF) != 0)
                    throw new InvalidOperationException($"宽/高不是16的倍数:{w}x{h}");
            }

            private static int Read(Stream s, byte[] buf)
            {
                int off = 0;
                while (off < buf.Length)
                {
                    int n = s.Read(buf, off, buf.Length - off);
                    if (n == 0) return off;
                    off += n;
                }
                return off;
            }

            private static void ReadExact2(Stream s, byte[] buf)
            {
                int off = 0;
                while (off < buf.Length)
                {
                    int n = s.Read(buf, off, buf.Length - off);
                    if (n == 0) throw new EndOfStreamException();
                    off += n;
                }
            }
        }

        private class DecodedFrame
        {
            public uint Index;
            public ushort Width, Height;
            public byte[] Rgba;
        }

        private class AmvFrameDecoder
        {
            private readonly AmvHeader _h;
            private readonly AmvMode _mode;
            private readonly QuantTables _q;
            private readonly HuffmanTable _dcL, _acL, _dcC, _acC;
            private byte[] _prev;

            private static readonly int[] Zigzag =
            {
                0,1,8,16,9,2,3,10,17,24,32,25,18,11,4,5,
                12,19,26,33,40,48,41,34,27,20,13,6,7,14,21,28,
                35,42,49,56,57,50,43,36,29,22,15,23,30,37,44,51,
                58,59,52,45,38,31,39,46,53,60,61,54,47,55,62,63
            };

            public AmvFrameDecoder(AmvHeader h, AmvMode mode, QuantTables q)
            {
                _h = h; _mode = mode; _q = q;
                _dcL = HuffmanTable.DcLuma(); _acL = HuffmanTable.AcLuma();
                _dcC = HuffmanTable.DcChroma(); _acC = HuffmanTable.AcChroma();
            }

            public DecodedFrame Decode(FramePacket pkt)
            {
                int w = _h.Width, h = _h.Height;
                byte[] cur = _prev != null ? (byte[])_prev.Clone() : new byte[w * h * 4];

                if (pkt.Kind is FramePacketKind.A)
                    DecodeA(pkt, cur);
                else
                    DecodeB(pkt, cur);

                _prev = (byte[])cur.Clone();
                return new DecodedFrame { Index = pkt.Index, Width = _h.Width, Height = _h.Height, Rgba = cur };
            }

            private void DecodeA(FramePacket pkt, byte[] rgba)
            {
                var ah = pkt.HeaderA() ?? throw new InvalidOperationException("模式A头部缺失");
                byte[] payload = pkt.Seg1 ?? throw new InvalidOperationException("模式A seg1缺失");
                DecodeRect(payload, ah.X, ah.Y, ah.W, ah.H, false, _q.Q1, _q.Q0, null, rgba);
            }

            private void DecodeB(FramePacket pkt, byte[] rgba)
            {
                var bh = pkt.HeaderB() ?? throw new InvalidOperationException("模式B头部缺失");
                byte[] payload = pkt.PayloadB() ?? throw new InvalidOperationException("模式B负载缺失");
                DecodeRect(payload, bh.P0, bh.P1, bh.W, bh.H, true, _q.Q1, _q.Q0, _q.Q2, rgba);
            }

            private void DecodeRect(byte[] payload, int x0, int y0, int rw, int rh,
                bool hasAlpha, byte[] qC, byte[] qL, byte[] qA, byte[] rgba)
            {
                if (rw <= 0 || rh <= 0) return;
                int fw = _h.Width, fh = _h.Height, stride = fw * 4;
                int mbW = rw >> 4, mbH = rh >> 4;
                var br = new BitReader(payload);
                int predC = 0, predL = 0;

                for (int my = 0; my < mbH; my++)
                {
                    for (int mx = 0; mx < mbW; mx++)
                    {
                        var cb = DecodeBlock(br, _dcC, _acC, ref predC, qC);
                        var cr = DecodeBlock(br, _dcC, _acC, ref predC, qC);
                        var y0b = DecodeBlock(br, _dcL, _acL, ref predL, qL);
                        var y1b = DecodeBlock(br, _dcL, _acL, ref predL, qL);
                        var y2b = DecodeBlock(br, _dcL, _acL, ref predL, qL);
                        var y3b = DecodeBlock(br, _dcL, _acL, ref predL, qL);

                        DecodedBlock a0b = null, a1b = null, a2b = null, a3b = null;
                        if (hasAlpha)
                        {
                            var qa = qA;
                            a0b = DecodeBlock(br, _dcL, _acL, ref predL, qa);
                            a1b = DecodeBlock(br, _dcL, _acL, ref predL, qa);
                            a2b = DecodeBlock(br, _dcL, _acL, ref predL, qa);
                            a3b = DecodeBlock(br, _dcL, _acL, ref predL, qa);
                        }

                        bool dcOnly = cb.AcEmpty && cr.AcEmpty && y0b.AcEmpty && y1b.AcEmpty && y2b.AcEmpty && y3b.AcEmpty
                            && (!hasAlpha || (a0b.AcEmpty && a1b.AcEmpty && a2b.AcEmpty && a3b.AcEmpty));

                        int bx = x0 + (mx << 4), by = y0 + (my << 4);
                        if (bx >= fw || by >= fh || bx + 16 <= 0 || by + 16 <= 0) continue;

                        byte[] cbPx = dcOnly ? DcPx(cb) : cb.Pixels;
                        byte[] crPx = dcOnly ? DcPx(cr) : cr.Pixels;
                        var yB = new[] { y0b, y1b, y2b, y3b };
                        byte[][] yPx = new byte[4][];
                        for (int i = 0; i < 4; i++) yPx[i] = dcOnly ? DcPx(yB[i]) : yB[i].Pixels;

                        byte[][] aPx = null;
                        if (hasAlpha)
                        {
                            var aB = new[] { a0b, a1b, a2b, a3b };
                            aPx = new byte[4][];
                            for (int i = 0; i < 4; i++) aPx[i] = dcOnly ? DcPx(aB[i]) : aB[i].Pixels;
                        }

                        for (int dy = 0; dy < 16; dy++)
                        {
                            int py = by + dy;
                            if (py < 0 || py >= fh) continue;
                            int rowOff = py * stride;
                            for (int dx = 0; dx < 16; dx++)
                            {
                                int px = bx + dx;
                                if (px < 0 || px >= fw) continue;
                                int yb = (dx >= 8 ? 1 : 0) + (dy >= 8 ? 2 : 0);
                                int yv = yPx[yb][(dy & 7) * 8 + (dx & 7)];
                                int cbv = cbPx[(dy >> 1) * 8 + (dx >> 1)];
                                int crv = crPx[(dy >> 1) * 8 + (dx >> 1)];
                                var rgb = Yuv2Rgb(yv, cbv, crv);
                                int a = aPx != null ? aPx[yb][(dy & 7) * 8 + (dx & 7)] : 255;
                                int off = rowOff + px * 4;
                                rgba[off] = rgb.r; rgba[off + 1] = rgb.g; rgba[off + 2] = rgb.b; rgba[off + 3] = (byte)a;
                            }
                        }
                    }
                }
            }

            private static DecodedBlock DecodeBlock(BitReader br, HuffmanTable dcT, HuffmanTable acT, ref int pred, byte[] q)
            {
                int s = dcT.Decode(br);
                pred += br.ReadJpegSigned(s);
                int[] coeff = new int[64];
                coeff[0] = pred;

                int k = 1;
                bool sawAc = false;
                while (k < 64)
                {
                    byte sym = acT.Decode(br);
                    if (sym == 0x00) break;
                    if (sym == 0xF0) { k += 16; continue; }
                    k += sym >> 4;
                    if (k >= 64) break;
                    int v = br.ReadJpegSigned(sym & 0x0F);
                    coeff[Zigzag[k]] = v;
                    sawAc = true;
                    k++;
                }

                int[] deq = new int[64];
                for (int i = 0; i < 64; i++) deq[i] = coeff[i] * q[i] * 4;

                short[] spatial = Idct.Idct8x8(deq);
                byte[] px = new byte[64];
                for (int i = 0; i < 64; i++) px[i] = (byte)Math.Clamp(spatial[i] + 128, 0, 255);

                return new DecodedBlock { Pixels = px, Dc = deq[0], AcEmpty = !sawAc };
            }

            private static byte[] DcPx(DecodedBlock b)
            {
                int v = Math.Clamp(Idct.DcOnly(b.Dc) + 128, 0, 255);
                var px = new byte[64];
                for (int i = 0; i < 64; i++) px[i] = (byte)v;
                return px;
            }

            private static (byte r, byte g, byte b) Yuv2Rgb(int y, int cb, int cr)
            {
                double cbf = cb - 128.0, crf = cr - 128.0, yf = y;
                return (
                    (byte)Math.Clamp(Math.Round(yf + 1.402 * crf), 0, 255),
                    (byte)Math.Clamp(Math.Round(yf - 0.344136 * cbf - 0.714136 * crf), 0, 255),
                    (byte)Math.Clamp(Math.Round(yf + 1.772 * cbf), 0, 255));
            }
        }

        private class DecodedBlock
        {
            public byte[] Pixels = new byte[0];
            public int Dc;
            public bool AcEmpty;
        }

        private class BitReader
        {
            private readonly byte[] _data;
            private int _bytePos;
            private int _bitPos;

            public BitReader(byte[] data)
            {
                _data = data;
                _bytePos = 0;
                _bitPos = 0;
            }

            private uint ReadBit()
            {
                if (_bytePos >= _data.Length)
                    throw new InvalidOperationException("比特流不足");
                int b = _data[_bytePos];
                int bit = (b >> (7 - _bitPos)) & 1;
                _bitPos++;
                if (_bitPos == 8) { _bitPos = 0; _bytePos++; }
                return (uint)bit;
            }

            public uint ReadBits(int n)
            {
                if (n == 0) return 0;
                uint v = 0;
                for (int i = 0; i < n; i++)
                    v = (v << 1) | ReadBit();
                return v;
            }

            public int ReadJpegSigned(int n)
            {
                if (n == 0) return 0;
                int raw = (int)ReadBits(n);
                int half = 1 << (n - 1);
                return raw < half ? raw - ((1 << n) - 1) : raw;
            }
        }

        private class HuffmanTable
        {
            private readonly ushort[] _firstCode = new ushort[17];
            private readonly ushort[] _count = new ushort[17];
            private readonly ushort[] _firstSymbol = new ushort[17];
            private readonly byte[] _symbols;

            private HuffmanTable(byte[] symbols) { _symbols = symbols; }

            public static HuffmanTable Create(byte[] bits, byte[] values)
            {
                var t = new HuffmanTable(values);
                int total = 0;
                for (int i = 0; i < 16; i++) { t._count[i + 1] = bits[i]; total += bits[i]; }
                if (total != values.Length) throw new InvalidOperationException("霍夫曼编码比特/值不匹配");
                ushort code = 0, si = 0;
                for (int len = 1; len <= 16; len++)
                {
                    t._firstCode[len] = code;
                    t._firstSymbol[len] = si;
                    ushort cnt = t._count[len];
                    si += cnt; code += cnt; code <<= 1;
                }
                return t;
            }

            public byte Decode(BitReader br)
            {
                ushort code = 0;
                for (int len = 1; len <= 16; len++)
                {
                    code = (ushort)((code << 1) | (ushort)br.ReadBits(1));
                    ushort first = _firstCode[len], cnt = _count[len];
                    if (cnt == 0) continue;
                    if (code >= first && code < first + cnt)
                        return _symbols[_firstSymbol[len] + (code - first)];
                }
                throw new InvalidOperationException("无效的霍夫曼编码");
            }

            public static HuffmanTable DcLuma() => Create(
                new byte[] { 0, 1, 5, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
                new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });

            public static HuffmanTable AcLuma() => Create(
                new byte[] { 0, 2, 1, 3, 3, 2, 4, 3, 5, 5, 4, 4, 0, 0, 1, 0x7D },
                new byte[] { 0x01,0x02,0x03,0x00,0x04,0x11,0x05,0x12,0x21,0x31,0x41,0x06,0x13,0x51,
                 0x61,0x07,0x22,0x71,0x14,0x32,0x81,0x91,0xa1,0x08,0x23,0x42,0xb1,0xc1,
                 0x15,0x52,0xd1,0xf0,0x24,0x33,0x62,0x72,0x82,0x09,0x0a,0x16,0x17,0x18,
                 0x19,0x1a,0x25,0x26,0x27,0x28,0x29,0x2a,0x34,0x35,0x36,0x37,0x38,0x39,
                 0x3a,0x43,0x44,0x45,0x46,0x47,0x48,0x49,0x4a,0x53,0x54,0x55,0x56,0x57,
                 0x58,0x59,0x5a,0x63,0x64,0x65,0x66,0x67,0x68,0x69,0x6a,0x73,0x74,0x75,
                 0x76,0x77,0x78,0x79,0x7a,0x83,0x84,0x85,0x86,0x87,0x88,0x89,0x8a,0x92,
                 0x93,0x94,0x95,0x96,0x97,0x98,0x99,0x9a,0xa2,0xa3,0xa4,0xa5,0xa6,0xa7,
                 0xa8,0xa9,0xaa,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb8,0xb9,0xba,0xc2,0xc3,
                 0xc4,0xc5,0xc6,0xc7,0xc8,0xc9,0xca,0xd2,0xd3,0xd4,0xd5,0xd6,0xd7,0xd8,
                 0xd9,0xda,0xe1,0xe2,0xe3,0xe4,0xe5,0xe6,0xe7,0xe8,0xe9,0xea,0xf1,0xf2,
                 0xf3,0xf4,0xf5,0xf6,0xf7,0xf8,0xf9,0xfa});

            public static HuffmanTable DcChroma() => Create(
                new byte[] { 0, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0 },
                new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 });

            public static HuffmanTable AcChroma() => Create(
                new byte[] { 0, 2, 1, 2, 4, 4, 3, 4, 7, 5, 4, 4, 0, 1, 2, 0x77 },
                new byte[] { 0x00,0x01,0x02,0x03,0x11,0x04,0x05,0x21,0x31,0x06,0x12,0x41,0x51,0x07,
                 0x61,0x71,0x13,0x22,0x32,0x81,0x08,0x14,0x42,0x91,0xa1,0xb1,0xc1,0x09,
                 0x23,0x33,0x52,0xf0,0x15,0x62,0x72,0xd1,0x0a,0x16,0x24,0x34,0xe1,0x25,
                 0xf1,0x17,0x18,0x19,0x1a,0x26,0x27,0x28,0x29,0x2a,0x35,0x36,0x37,0x38,
                 0x39,0x3a,0x43,0x44,0x45,0x46,0x47,0x48,0x49,0x4a,0x53,0x54,0x55,0x56,
                 0x57,0x58,0x59,0x5a,0x63,0x64,0x65,0x66,0x67,0x68,0x69,0x6a,0x73,0x74,
                 0x75,0x76,0x77,0x78,0x79,0x7a,0x82,0x83,0x84,0x85,0x86,0x87,0x88,0x89,
                 0x8a,0x92,0x93,0x94,0x95,0x96,0x97,0x98,0x99,0x9a,0xa2,0xa3,0xa4,0xa5,
                 0xa6,0xa7,0xa8,0xa9,0xaa,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb8,0xb9,0xba,
                 0xc2,0xc3,0xc4,0xc5,0xc6,0xc7,0xc8,0xc9,0xca,0xd2,0xd3,0xd4,0xd5,0xd6,
                 0xd7,0xd8,0xd9,0xda,0xe2,0xe3,0xe4,0xe5,0xe6,0xe7,0xe8,0xe9,0xea,0xf2,
                 0xf3,0xf4,0xf5,0xf6,0xf7,0xf8,0xf9,0xfa});
        }

        private static class Idct
        {
            private static readonly double Sqrt2Inv = 1.0 / Math.Sqrt(2.0);

            public static short[] Idct8x8(int[] coeff)
            {
                var output = new short[64];
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        double sum = 0.0;
                        for (int v = 0; v < 8; v++)
                        {
                            for (int u = 0; u < 8; u++)
                            {
                                double cu = u == 0 ? Sqrt2Inv : 1.0;
                                double cv = v == 0 ? Sqrt2Inv : 1.0;
                                double a = ((2 * x + 1) * u * Math.PI) / 16.0;
                                double b = ((2 * y + 1) * v * Math.PI) / 16.0;
                                sum += cu * cv * coeff[v * 8 + u] * Math.Cos(a) * Math.Cos(b);
                            }
                        }
                        output[y * 8 + x] = (short)Math.Clamp(Math.Round(sum / 4.0), short.MinValue, short.MaxValue);
                    }
                }
                return output;
            }

            public static short DcOnly(int dc)
            {
                int v = dc >= 0 ? (dc + 4) >> 3 : -((-dc + 4) >> 3);
                return (short)Math.Clamp(v, short.MinValue, short.MaxValue);
            }
        }
    }
}
