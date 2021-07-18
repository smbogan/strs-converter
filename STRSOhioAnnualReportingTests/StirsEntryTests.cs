using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using STRSOhioAnnualReporting;
using System.Linq;
using System.IO;
using System.Reflection;

namespace STRSOhioAnnualReportingTests
{
    [TestClass]
    public class StirsEntryTests
    {
        [TestMethod]
        public void ReadCsv()
        {
            using var sample1 = CsvSamples.GetSample1();

            var entries = StrsEntry.ReadCsv(sample1);

            entries.Count.Should().Be(2);
        }

        [TestMethod]
        public void WriteCsv()
        {
            using var sample1 = CsvSamples.GetSample1();

            var entries = StrsEntry.ReadCsv(sample1);

            using var stream = new MemoryStream();

            StrsEntry.WriteCsv(entries, stream);

            stream.Position = 0;
        }

        [TestMethod]
        public void ReadDat()
        {
            var sample1 = DatSamples.GetSample2();

            var entries = StrsEntry.ReadDat(sample1);

            entries.Count.Should().Be(2);
        }

        [TestMethod]
        public void WriteDat()
        {
            using var sample1 = CsvSamples.GetSample2();

            var entries = StrsEntry.ReadCsv(sample1);

            using var stream = new MemoryStream();

            StrsEntry.WriteDat(stream, entries);
        }
    }
}
