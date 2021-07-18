using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using STRSOhioAnnualReporting;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System;

namespace STRSOhioAnnualReportingTests
{
    [TestClass]
    public class CsvLineSplitterTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            CsvLine.Split("a,b,c,d").ToArray().Should().Equal(new string[] { "a", "b", "c", "d" });

            CsvLine.Split("a").ToArray().Should().Equal(new string[] { "a" });

            CsvLine.Split("").ToArray().Should().Equal(new string[] { });

            CsvLine.Split("a,\"\"\"b\"\"\",c,d").ToArray().Should().Equal(new string[] { "a", "\"b\"", "c", "d" });

            CsvLine.Split("a,\"\"\"b\"\"\",\",c,\",d").ToArray().Should().Equal(new string[] { "a", "\"b\"", ",c,", "d" });

            CsvLine.Split("\"b\"").ToArray().Should().Equal(new string[] { "b" });

            CsvLine.Split("\"\"\"b\"\"\"").ToArray().Should().Equal(new string[] { "\"b\"" });
        }

        private List<string[]> ReadCsvByLine(Func<Stream> source)
        {
            List<string[]> lines = new List<string[]>();

            using (var sample1 = source())
            {
                using (var sr = new StreamReader(sample1))
                {
                    string line = sr.ReadLine();

                    while (line != null)
                    {
                        lines.Add(CsvLine.Split(line).ToArray());
                        line = sr.ReadLine();
                    }
                }
            }

            return lines;
        }

        [TestMethod]
        public void TestMethod2()
        {
            List<string[]> lines = ReadCsvByLine(CsvSamples.GetSample1);

            //C000,123456.78,2021,067, ,3332244.44,Sample Joe A,876543.21,123 HOME ST,APT 999,,New York,NY,12345,4444,22,123456.78,joe@sample.com,5555551234,C
            //C001,123456.78,2021,044, ,3332244.44,Test Tony T,876543.21,123 HOUSE ST,APT 123,,Los Angeles,CA,54321,4444,22,123456.78,tony@test.com,5555551234,H

            //Expected
            List<string[]> expected = new List<string[]> {
                new string[] { "C000", "123456.78", "2021", "067", " ", "3332244.44", "Sample Joe A", "876543.21", "123 HOME ST", "APT 999", "", "New York", "NY", "12345", "4444", "22", "123456.78", "joe@sample.com", "5555551234", "C" },
                new string[] { "C001", "123456.78", "2021", "044", " ", "3332244.44", "Test Tony T", "876543.21", "123 HOUSE ST", "APT 123", "", "Los Angeles", "CA", "54321", "4444", "22", "123456.78", "tony@test.com", "5555551234", "H" }
            };

            for (int i = 0; i < expected.Count && i < lines.Count; i++)
            {
                lines[i].Should().Equal(expected[i]);
            }

            lines.Count.Should().Be(expected.Count);

        }

        [TestMethod]
        public void TestMethod3()
        {
            List<string[]> lines = ReadCsvByLine(CsvSamples.GetSample2);

            /*
             C000,3456.78,2021,067, ,334.00,Sample Joe A,876.2150,123 HOME ST,APT 999,,New York,NY,12345,4444,22,12.789,joe@sample.com,5555551234,C
             C001,56.78,2021,044, ,3344.00,Test Tony T,3.21,123 HOUSE ST,APT 123,,Los Angeles,CA,54321,4444,22,1456.78,tony@test.com,5555551234,H
             */

            //Expected
            List<string[]> expected = new List<string[]> {
                new string[] { "C000", "3456.78", "2021", "067", " ", "334.00", "Sample Joe A", "876.2150", "123 HOME ST", "APT 999", "", "New York", "NY", "12345", "4444", "22", "12.789", "joe@sample.com", "5555551234", "C" },
                new string[] { "C001", "56.78", "2021", "044", " ", "3344.00", "Test Tony T", "3.21", "123 HOUSE ST", "APT 123", "", "Los Angeles", "CA", "54321", "4444", "22", "1456.78", "tony@test.com", "5555551234", "H" }
            };

            for (int i = 0; i < expected.Count && i < lines.Count; i++)
            {
                lines[i].Should().Equal(expected[i]);
            }

            lines.Count.Should().Be(expected.Count);
        }
    }
}
