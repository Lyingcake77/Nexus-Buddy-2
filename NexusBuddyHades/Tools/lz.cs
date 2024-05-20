using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NexusBuddyHades.Tools
{
    //source https://github.com/Huss-9/LZ4-Compression-algorithm/blob/master/lz.py
    public class Lz
    {
        public static string SacaString(byte[] byteArr)
        {
            return BitConverter.ToString(byteArr).Replace("-", "").Substring(12, byteArr.Length - 14);
        }

        public static (int, int) SacaT1T2(int st)
        {
            return (st / 16, st % 16);
        }

        public static int ByteToInt(byte[] st)
        {
            return BitConverter.ToInt32(st.Reverse().ToArray(), 0);
        }

        public static (int, int) LISC(byte[] entr, int i)
        {
            int e = 0;
            int y = i;
            int val = entr[y];
            while (val == 255)
            {
                e += val;
                y += 1;
                val = entr[y];
            }
            e += val;
            return (e, y);
        }

        public static List<byte> NumToBytes(int t)
        {
            int count = t / 255;
            List<byte> l = Enumerable.Repeat((byte)255, count).ToList();
            l.Add((byte)(t % 255));
            return l;
        }

        public static int NumberOfBytes(int t)
        {
            int count = t / 255;
            if (count % 255 != 0)
            {
                count += 1;
            }
            return count;
        }

        public static void NumToListInts(int t, List<int> tVals)
        {
            int count = t / 255;
            tVals.AddRange(Enumerable.Repeat(255, count));
            tVals.Add(t % 255);
        }

        public static (byte[], byte[]) GetT1OffsetT2(int t1, int t2, int offset)
        {
            List<int> t1Vals = new List<int>();
            List<int> t2Vals = new List<int>();
            if (offset != -1)
            {
                t2Vals.Add(offset % 256);
                t2Vals.Add(offset / 256);
            }

            if (t1 >= 15)
            {
                t1Vals.Add(240);
                t1 -= 15;
                NumToListInts(t1, t1Vals);
            }
            else
            {
                t1Vals.Add(t1 * 16);
            }

            if (t2 >= 15)
            {
                t1Vals[0] += 15;
                t2 -= 15;
                NumToListInts(t2, t2Vals);
            }
            else
            {
                t1Vals[0] += t2;
            }
            return (t1Vals.Select(x => (byte)x).ToArray(), t2Vals.Select(x => (byte)x).ToArray());
        }

        public static bool IsThere(byte[] entr, int i, int iMatch, int offset)
        {
            int ara = i + offset;
            int ant = iMatch + offset;
            return entr.Skip(ara).Take(1).SequenceEqual(entr.Skip(ant).Take(1));
        }

        public static int MaxMatch(byte[] entr, int i, int pos, int offset)
        {
            int n = entr.Length;
            while (i + offset < n)
            {
                if (IsThere(entr, i, pos, offset))
                {
                    offset += 1;
                }
                else
                {
                    return offset;
                }
            }
            return offset;
        }

        public static (int, int) FindBestMatch(byte[] entr, int i, Dictionary<string, int> dic)
        {
            string key = SacaString(entr.Skip(i).Take(4).ToArray());
            int pos = dic[key];
            if (i - pos < Math.Pow(2, 16))
            {
                int iMatch = pos;
                int lenMatch = MaxMatch(entr, i, pos, 4);
                dic[key] = i;
                return (iMatch, lenMatch);
            }
            else
            {
                dic[key] = i;
                return (-1, 0);
            }
        }

        public static void NormalCompressor(byte[] entr, string nombre, bool test = false, string vers = "")
        {
            int n = entr.Length;
            List<byte> listFile = new List<byte>();
            if (n <= 12)
            {
                var (firstPart, _) = GetT1OffsetT2(n, 0, -1);
                listFile.AddRange(firstPart);
                listFile.AddRange(entr.Take(n));
            }
            else
            {
                Dictionary<string, int> dic = new Dictionary<string, int>();
                int iLit = 0;
                int i = 0;
                double maxBytes = n / 1.11;
                bool stopOut = false;
                while (i < n && !stopOut)
                {
                    string key = SacaString(entr.Skip(i).Take(4).ToArray());
                    int extra = NumberOfBytes(n - i) + 1;
                    if (listFile.Count + (n - i) + extra >= maxBytes)
                    {
                        if (!dic.ContainsKey(key))
                        {
                            dic[key] = i;
                            if (i >= n - 5)
                            {
                                var (firstPart, _) = GetT1OffsetT2(n - iLit, 0, -1);
                                listFile.AddRange(firstPart);
                                listFile.AddRange(entr.Skip(iLit));
                                i = n;
                            }
                            i += 1;
                        }
                        else
                        {
                            var (pos, lenMatch) = FindBestMatch(entr, i, dic);
                            if (pos == -1)
                            {
                                i += 1;
                            }
                            else
                            {
                                if (i + lenMatch < n - 5)
                                {
                                    int iFin = i + lenMatch;
                                    int iIni = i + 1;
                                    while (iIni < iFin)
                                    {
                                        key = SacaString(entr.Skip(iIni).Take(4).ToArray());
                                        dic[key] = iIni;
                                        iIni += 1;
                                    }
                                    int t1 = i - iLit;
                                    int t2 = lenMatch - 4;
                                    int offset = i - pos;
                                    var (firstPart, secondPart) = GetT1OffsetT2(t1, t2, offset);
                                    listFile.AddRange(firstPart);
                                    listFile.AddRange(entr.Skip(iLit).Take(i - iLit));
                                    listFile.AddRange(secondPart);
                                    i = i + lenMatch;
                                    iLit = i;
                                }
                                else
                                {
                                    var (firstPart, _) = GetT1OffsetT2(n - iLit, 0, -1);
                                    listFile.AddRange(firstPart);
                                    listFile.AddRange(entr.Skip(iLit));
                                    i = n;
                                }
                            }
                        }
                    }
                    else
                    {
                        var (firstPart, _) = GetT1OffsetT2(n - iLit, 0, -1);
                        listFile.AddRange(firstPart);
                        listFile.AddRange(entr.Skip(iLit));
                        stopOut = true;
                    }
                }
            }

            if (test)
            {
                nombre = "comprimidos/" + vers + "_" + nombre + ".lz4";
            }
            else
            {
                nombre = nombre + ".lz4";
            }
            if (File.Exists(nombre))
            {
                File.Delete(nombre);
            }
            File.WriteAllBytes(nombre, listFile.ToArray());
        }

        public static void Decompressor(byte[] entr, string nombre, bool test = false)
        {
            int n = entr.Length;
            List<byte> listaFile = new List<byte>();
            int i = 0;
            while (i < n)
            {
                byte b = entr[i];
                var (t1, t2) = SacaT1T2(b);
                i += 1;
                if (t1 == 15)
                {
                    var (e1, y) = LISC(entr, i);
                    t1 += e1;
                    i = y + 1;
                }
                listaFile.AddRange(entr.Skip(i).Take(t1));
                i += t1;
                if (i < n - 2)
                {
                    int offset = entr[i + 1] * (int)Math.Pow(2, 8) + entr[i];
                    i += 2;
                    if (t2 == 15)
                    {
                        var (e2, y) = LISC(entr, i);
                        t2 += e2;
                        i = y + 1;
                    }
                    t2 += 4;
                    int cIni = listaFile.Count - offset;
                    int cFin = Math.Min(listaFile.Count, cIni + t2);
                    listaFile.AddRange(listaFile.Skip(cIni).Take(cFin - cIni));

                    t2 -= offset;
                    while (t2 > offset)
                    {
                        listaFile.AddRange(listaFile.Skip(listaFile.Count - offset));
                        t2 -= offset;
                    }
                    if (t2 > 0)
                    {
                        cIni = listaFile.Count - offset;
                        cFin = Math.Min(listaFile.Count, cIni + t2);
                        listaFile.AddRange(listaFile.Skip(cIni).Take(cFin - cIni));
                    }
                }
            }

            if (test)
            {
                nombre = "descomprimidos/" + nombre.Substring(0, nombre.Length - 4);
            }
            else
            {
                nombre = nombre.Substring(0, nombre.Length - 4);
            }
            if (File.Exists(nombre))
            {
                File.Delete(nombre);
            }
            File.WriteAllBytes(nombre, listaFile.ToArray());
        }

        public static void Main(string filename, string compressionType)
        {
            byte[] entr;
            using (var reader = new BinaryReader(File.Open(filename, FileMode.Open)))
            {
                entr = reader.ReadBytes((int)reader.BaseStream.Length);
            }
            if (compressionType == "-c")
            {
                NormalCompressor(entr, filename);
            }
            else if (compressionType == "-d")
            {
                Decompressor(entr, filename);
            }
        }
    }
}
