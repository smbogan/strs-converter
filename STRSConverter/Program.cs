using System;
using System.IO;
using System.Linq;
using STRSOhioAnnualReporting;

namespace STRSConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                foreach (var filepath in Directory.EnumerateFiles(Directory.GetCurrentDirectory()))
                {
                    string file = Path.GetFileName(filepath);


                }

                args = (from filepath in Directory.EnumerateFiles(Directory.GetCurrentDirectory())
                let file = Path.GetFileName(filepath)
                where (file.EndsWith(".csv") || file.EndsWith(".dat")) && !file.EndsWith(".out.csv") && !file.EndsWith(".out.dat")
                select file).ToArray();
            }

            int i = 1;

            Console.WriteLine("Inputs:");

            foreach (var arg in args)
            {
                string extension = Path.GetExtension(arg);

                if (extension == ".csv" || extension == ".dat")
                {
                    Console.WriteLine($" {i,4}: {arg}");
                }
                else
                {
                    Console.WriteLine($" (skipped) {i,4}: {arg}");
                }

                i++;
            }

            foreach (var arg in args)
            {
                if (Path.GetExtension(arg) == ".csv")
                {
                    Console.WriteLine(new string('*', 20));
                    Console.WriteLine($"Converting {arg} to DAT");
                    Console.WriteLine(new string('*', 20));

                    string output = Path.ChangeExtension(arg, ".out.dat");

                    using var infs = File.OpenRead(arg);

                    var entries = StrsEntry.ReadCsv(infs);

                    using var outfs = File.Create(output);

                    StrsEntry.WriteDat(outfs, entries);

                    outfs.Close();
                }
                else if (Path.GetExtension(arg) == ".dat")
                {
                    Console.WriteLine(new string('*', 20));
                    Console.WriteLine($"Converting {arg} to CSV");
                    Console.WriteLine(new string('*', 20));

                    string output = Path.ChangeExtension(arg, ".out.csv");

                    using var infs = File.OpenRead(arg);

                    var entries = StrsEntry.ReadDat(infs);

                    using var outfs = File.Create(output);

                    StrsEntry.WriteCsv(entries, outfs);

                    outfs.Close();

                }                
            }
        }
    }
}
