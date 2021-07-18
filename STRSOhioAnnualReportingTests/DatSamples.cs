using System;
using System.IO;
using System.Reflection;

namespace STRSOhioAnnualReportingTests
{
    public static class DatSamples
    {
        public static Stream GetSample1()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("STRSOhioAnnualReportingTests.sample1.dat");
        }

        public static Stream GetSample2()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("STRSOhioAnnualReportingTests.sample2.dat");
        }
    }
}
