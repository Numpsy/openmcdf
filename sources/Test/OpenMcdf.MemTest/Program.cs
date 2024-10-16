﻿using System;
using System.Diagnostics;
using System.IO;
//This project is used for profiling memory and performances of OpenMCDF .

namespace OpenMcdf.MemTest
{
    static class Program
    {
        static void Main(string[] args)
        {
            //TestMultipleStreamCommit();
            TestCode();
            //StressMemory();
            //DummyFile();
            //Console.WriteLine("CLOSED");
            //Console.ReadKey();
        }

        private static void TestCode()
        {
            const int N_FACTOR = 1000;

            byte[] bA = GetBuffer(20 * 1024 * N_FACTOR, 0x0A);
            byte[] bB = GetBuffer(5 * 1024, 0x0B);
            byte[] bC = GetBuffer(5 * 1024, 0x0C);
            byte[] bD = GetBuffer(5 * 1024, 0x0D);
            byte[] bE = GetBuffer(8 * 1024 * N_FACTOR + 1, 0x1A);
            byte[] bF = GetBuffer(16 * 1024 * N_FACTOR, 0x1B);
            byte[] bG = GetBuffer(14 * 1024 * N_FACTOR, 0x1C);
            byte[] bH = GetBuffer(12 * 1024 * N_FACTOR, 0x1D);
            byte[] bE2 = GetBuffer(8 * 1024 * N_FACTOR, 0x2A);
            byte[] bMini = GetBuffer(1027, 0xEE);

            Stopwatch sw = new Stopwatch();
            sw.Start();

            var cf = new CompoundFile(CFSVersion.Ver_3, CFSConfiguration.SectorRecycle);
            cf.RootStorage.AddStream("A").SetData(bA);
            cf.SaveAs("OneStream.cfs");

            cf.Close();

            cf = new CompoundFile("OneStream.cfs", CFSUpdateMode.ReadOnly, CFSConfiguration.SectorRecycle);

            cf.RootStorage.AddStream("B").SetData(bB);
            cf.RootStorage.AddStream("C").SetData(bC);
            cf.RootStorage.AddStream("D").SetData(bD);
            cf.RootStorage.AddStream("E").SetData(bE);
            cf.RootStorage.AddStream("F").SetData(bF);
            cf.RootStorage.AddStream("G").SetData(bG);
            cf.RootStorage.AddStream("H").SetData(bH);

            cf.SaveAs("8_Streams.cfs");

            cf.Close();

            File.Copy("8_Streams.cfs", "6_Streams.cfs", true);

            cf = new CompoundFile("6_Streams.cfs", CFSUpdateMode.Update, CFSConfiguration.SectorRecycle | CFSConfiguration.EraseFreeSectors);
            cf.RootStorage.Delete("D");
            cf.RootStorage.Delete("G");
            cf.Commit();

            cf.Close();

            File.Copy("6_Streams.cfs", "6_Streams_Shrinked.cfs", true);

            cf = new CompoundFile("6_Streams_Shrinked.cfs", CFSUpdateMode.Update, CFSConfiguration.SectorRecycle);
            cf.RootStorage.AddStream("ZZZ").SetData(bF);
            cf.RootStorage.GetStream("E").Append(bE2);
            cf.Commit();
            cf.Close();

            cf = new CompoundFile("6_Streams_Shrinked.cfs", CFSUpdateMode.Update, CFSConfiguration.SectorRecycle);
            cf.RootStorage.CLSID = new Guid("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            cf.Commit();
            cf.Close();

            cf = new CompoundFile("6_Streams_Shrinked.cfs", CFSUpdateMode.Update, CFSConfiguration.SectorRecycle);
            cf.RootStorage.AddStorage("MyStorage").AddStream("ANS").Append(bE);
            cf.Commit();
            cf.Close();

            cf = new CompoundFile("6_Streams_Shrinked.cfs", CFSUpdateMode.Update, CFSConfiguration.SectorRecycle);
            cf.RootStorage.AddStorage("AnotherStorage").AddStream("ANS").Append(bE);
            cf.RootStorage.Delete("MyStorage");
            cf.Commit();
            cf.Close();

            CompoundFile.ShrinkCompoundFile("6_Streams_Shrinked.cfs");

            cf = new CompoundFile("6_Streams_Shrinked.cfs", CFSUpdateMode.Update, CFSConfiguration.SectorRecycle);
            cf.RootStorage.AddStorage("MiniStorage").AddStream("miniSt").Append(bMini);
            cf.RootStorage.GetStorage("MiniStorage").AddStream("miniSt2").Append(bMini);
            cf.Commit();
            cf.Close();

            cf = new CompoundFile("6_Streams_Shrinked.cfs", CFSUpdateMode.Update, CFSConfiguration.SectorRecycle);
            cf.RootStorage.GetStorage("MiniStorage").Delete("miniSt");

            cf.RootStorage.GetStorage("MiniStorage").GetStream("miniSt2").Append(bE);
            cf.Commit();
            cf.Close();

            cf = new CompoundFile("6_Streams_Shrinked.cfs", CFSUpdateMode.ReadOnly, CFSConfiguration.SectorRecycle);

            var myStream = cf.RootStorage.GetStream("C");
            var data = myStream.GetData();
            Console.WriteLine(data[0] + " : " + data[data.Length - 1]);

            myStream = cf.RootStorage.GetStream("B");
            data = myStream.GetData();
            Console.WriteLine(data[0] + " : " + data[data.Length - 1]);

            cf.Close();

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);

            Console.ReadKey();
        }

        private static void StressMemory()
        {
            const int N_LOOP = 20;
            const int MB_SIZE = 10;

            byte[] b = GetBuffer(1024 * 1024 * MB_SIZE); //2GB buffer
            byte[] cmp = new byte[] { 0x0, 0x1, 0x2, 0x3, 0x4, 0x5, 0x6, 0x7 };

            CompoundFile cf = new CompoundFile(CFSVersion.Ver_4, CFSConfiguration.Default);
            _ = cf.RootStorage.AddStream("MySuperLargeStream");
            cf.SaveAs("LARGE.cfs");
            cf.Close();

            //Console.WriteLine("Closed save");
            //Console.ReadKey();

            cf = new CompoundFile("LARGE.cfs", CFSUpdateMode.Update, CFSConfiguration.Default);
            CFStream cfst = cf.RootStorage.GetStream("MySuperLargeStream");

            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < N_LOOP; i++)
            {
                cfst.Append(b);
                cf.Commit(true);

                Console.WriteLine("     Updated " + i.ToString());
                //Console.ReadKey();
            }

            cfst.Append(cmp);
            cf.Commit(true);
            sw.Stop();

            cf.Close();

            Console.WriteLine(sw.Elapsed.TotalMilliseconds);
            sw.Reset();

            //Console.WriteLine(sw.Elapsed.TotalMilliseconds);

            //Console.WriteLine("Closed Transacted");
            //Console.ReadKey();

            cf = new CompoundFile("LARGE.cfs");
            int count = 8;
            sw.Reset();
            sw.Start();
            byte[] data = new byte[count];
            count = cf.RootStorage.GetStream("MySuperLargeStream").Read(data, b.Length * (long)N_LOOP, count);
            sw.Stop();
            Console.Write(count);
            cf.Close();

            Console.WriteLine("Closed Final " + sw.ElapsedMilliseconds);
            Console.ReadKey();
        }

        private static void DummyFile()
        {
            Console.WriteLine("Start");
            FileStream fs = new FileStream("myDummyFile", FileMode.Create);
            fs.Close();

            Stopwatch sw = new Stopwatch();

            byte[] b = GetBuffer(1024 * 1024 * 50); //2GB buffer

            fs = new FileStream("myDummyFile", FileMode.Open);
            sw.Start();
            for (int i = 0; i < 42; i++)
            {
                fs.Seek(b.Length * i, SeekOrigin.Begin);
                fs.Write(b, 0, b.Length);
            }

            fs.Close();
            sw.Stop();
            Console.WriteLine("Stop - " + sw.ElapsedMilliseconds);
            sw.Reset();

            Console.ReadKey();
        }

        private static void AddNodes(string depth, CFStorage cfs)
        {
            void va(CFItem target)
            {
                string temp = target.Name + (target is CFStorage ? "" : " (" + target.Size + " bytes )");

                //Stream

                Console.WriteLine(depth + temp);

                if (target is CFStorage)
                { //Storage
                    string newDepth = depth + "    ";

                    //Recursion into the storage
                    AddNodes(newDepth, (CFStorage)target);
                }
            }

            //Visit NON-recursively (first level only)
            cfs.VisitEntries(va, false);
        }

        public static void TestMultipleStreamCommit()
        {
            string srcFilename = Directory.GetCurrentDirectory() + @"\testfile\report.xls";
            string dstFilename = Directory.GetCurrentDirectory() + @"\testfile\reportOverwriteMultiple.xls";
            //Console.WriteLine(Directory.GetCurrentDirectory());
            //Console.ReadKey();
            File.Copy(srcFilename, dstFilename, true);

            CompoundFile cf = new CompoundFile(dstFilename, CFSUpdateMode.Update, CFSConfiguration.SectorRecycle);

            Random r = new Random();

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
            {
                byte[] buffer = GetBuffer(r.Next(100, 3500), 0x0A);

                if (i > 0)
                {
                    if (r.Next(0, 100) > 50)
                    {
                        cf.RootStorage.Delete("MyNewStream" + (i - 1).ToString());
                    }
                }

                CFStream addedStream = cf.RootStorage.AddStream("MyNewStream" + i.ToString());

                addedStream.SetData(buffer);

                // Random commit, not on single addition
                if (r.Next(0, 100) > 50)
                    cf.Commit();
            }

            cf.Close();

            Console.WriteLine($"Elapsed: {stopwatch.Elapsed}");
        }

        private static byte[] GetBuffer(int count)
        {
            Random r = new Random();
            byte[] b = new byte[count];
            r.NextBytes(b);
            return b;
        }

        private static byte[] GetBuffer(int count, byte c)
        {
            byte[] b = new byte[count];
            for (int i = 0; i < b.Length; i++)
            {
                b[i] = c;
            }

            return b;
        }
    }
}
