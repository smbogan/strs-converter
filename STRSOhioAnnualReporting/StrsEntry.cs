using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace STRSOhioAnnualReporting
{
    public class StrsEntry
    {
        [StrsDefinition(1, 4, StrsType.Alphanumeric)]
        public string EmployeeID { get; set; }

        [StrsDefinition(5, 8, StrsType.FixedPointDecimal)]
        public string TaxedMemberContribution { get; set; }

        [StrsDefinition(13, 4, StrsType.Year)]
        public string ReportFiscalYear { get; set; }

        [StrsDefinition(17, 3, StrsType.FixedPointDecimal)]
        public string ServiceCredit { get; set; }

        [StrsDefinition(20, 1, StrsType.Alphanumeric, StrsTrimming.Fail, StrsPadding.Fail)]
        public string MembershipTypeCode { get; set; }

        [StrsDefinition(21, 9, StrsType.Integer, StrsTrimming.Fail, StrsPadding.Fail)]
        public string SocialSecurityNumber { get; set; }

        [StrsDefinition(30, 30, StrsType.Alphanumeric)]
        public string NameOfEmployee { get; set; }

        [StrsDefinition(60, 8, StrsType.FixedPointDecimal)]
        public string TaxDeferredMemberContribution { get; set; }

        [StrsDefinition(68, 40, StrsType.Alphanumeric)]
        public string DeliveryAddressLine1 { get; set; }
        
        [StrsDefinition(108, 40, StrsType.Alphanumeric)]
        public string DeliveryAddressLine2 { get; set; }

        [StrsDefinition(148, 40, StrsType.Alphanumeric)]
        public string DeliveryAddressLine3 { get; set; }

        [StrsDefinition(188, 20, StrsType.Alphanumeric)]
        public string CityName { get; set; }

        [StrsDefinition(208, 2, StrsType.Alphanumeric)]
        public string StateCode { get; set; }

        [StrsDefinition(210, 5, StrsType.Integer)]
        public string ZipCode { get; set; }

        [StrsDefinition(215, 4, StrsType.Integer)]
        public string ZipCodeSuffix { get; set; }

        [StrsDefinition(219, 2, StrsType.Integer)]
        public string ZipCodeDeliveryPoint { get; set; }

        [StrsDefinition(221, 8, StrsType.FixedPointDecimal)]
        public string AccruedContributionAmount { get; set; }

        [StrsDefinition(229, 50, StrsType.Alphanumeric)]
        public string EmailAddress { get; set; }

        [StrsDefinition(279, 10, StrsType.Integer, StrsTrimming.Fail, StrsPadding.Fail)]
        public string PhoneNumber { get; set; }

        [StrsDefinition(289, 1, StrsType.Alphanumeric, StrsTrimming.Fail, StrsPadding.Fail)]
        public string PhoneNumberType { get; set; }

        [StrsDefinition(290, 61, StrsType.Reserved)]
        public string Reserved { get; set; }

        private static (PropertyInfo Property, StrsDefinitionAttribute Attribute, Func<StrsEntry, string> Getter, Action<StrsEntry, string> Setter)[] Properties { get; }

        private static int DatEntryLength { get; }

        public StrsEntry()
        {
        }

        static StrsEntry()
        {
            Type classType = typeof(StrsEntry);
            var props = classType.GetProperties();

            var orderedProps = (from p in props
                                let accessors = p.GetAccessors()
                                let attribute = p.GetCustomAttributes(false)[0] as StrsDefinitionAttribute
                                select 
                                (
                                    Property: p,
                                    Attribute: attribute,
                                    Getter: (Func<StrsEntry, string>)((StrsEntry o) => accessors[0].Invoke(o, null) as string),
                                    Setter: StringPaddingAndTrimming.TrimAndPad((Action<StrsEntry, string>)((o, s) => accessors[1].Invoke(o, new object[] { s })), attribute)
                                )).ToArray();

            Array.Sort(orderedProps, (a, b) => { return a.Attribute.StartingPosition - b.Attribute.StartingPosition; });

            DatEntryLength = orderedProps.Sum((o) => o.Attribute.Length);

            Properties = orderedProps.ToArray();

            //Verify the properties positions and sizes are valid.
            int position = 1; //The positions are 1-based in the specification

            foreach (var p in Properties)
            {
                if (p.Attribute.StartingPosition != position)
                {
                    throw new Exception(Logger.Instance.Error($"Internal Error: The STRS entry definition is not valid. ({p.Attribute.StartingPosition} != { position })"));
                }

                position += p.Attribute.Length;
            }

            if (position - 1 != DatEntryLength)
            {
                throw new Exception(Logger.Instance.Error($"Internal Error: The STRS entry definition is not valid. (Entry Size: {DatEntryLength} != {position - 1})"));
            }
        }

        public static List<StrsEntry> ReadDat(Stream stream)
        {
            List<StrsEntry> results = new List<StrsEntry>();

            using var reader = new StreamReader(stream);

            while (!reader.EndOfStream)
            {
                StrsEntry entry = new StrsEntry();

                foreach (var prop in Properties)
                {
                    var attribute = prop.Attribute;
                    char[] chars = new char[attribute.Length];

                    reader.ReadBlock(chars, 0, chars.Length);

                    prop.Setter(entry, new string(chars));
                }

                results.Add(entry);
            }

            return results;
        }

        public static void WriteDat(Stream stream, IEnumerable<StrsEntry> entries)
        {
            var line = new byte[DatEntryLength];
            Span<byte> span = new Span<byte>(line);

            foreach (var entry in entries)
            {
                foreach (var prop in Properties)
                {
                    string value = prop.Getter(entry);
                    byte[] bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(value); //latin1

                    var subspan = span.Slice(prop.Attribute.StartingPosition - 1, prop.Attribute.Length);
                    bytes.CopyTo(subspan);
                }

                stream.Write(span);
            }
        }

        public static List<StrsEntry> ReadCsv(Stream stream)
        {
            List<StrsEntry> results = new List<StrsEntry>();

            using var reader = new StreamReader(stream, Encoding.GetEncoding("iso-8859-1")); //latin1

            //TODO: Actually take advantage of streams by not pre-loading all of the data.
            Queue<string> data = new Queue<string>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                string[] lineEntries = CsvLine.Split(line).ToArray();

                foreach (var e in lineEntries)
                {
                    data.Enqueue(e);
                }
            }

            while (data.Count > 0)
            {
                StrsEntry entry = new StrsEntry();

                foreach (var prop in Properties)
                {
                    var attribute = prop.Attribute;

                    switch (attribute.Type)
                    {
                        case StrsType.Alphanumeric:
                        case StrsType.Integer:
                        case StrsType.Year:
                            prop.Setter(entry, data.Dequeue());
                            break;
                        case StrsType.FixedPointDecimal:
                            string number = data.Dequeue();
                            string[] parts = number.Split('.');
                            string encodedNumber;

                            if (parts.Length == 0)
                            {
                                encodedNumber = new string('0', attribute.Length);
                            }
                            else if (parts.Length == 1)
                            {
                                encodedNumber = parts[0].PadLeft(attribute.Length - 2, '0') + "00";
                            }
                            else if (parts.Length == 2)
                            {
                                string decimalPart = parts[1].TrimEnd('0');

                                if (!decimal.TryParse("0." + decimalPart, out var decimalValue))
                                {
                                    throw new Exception(Logger.Instance.Error("Invalid value in fixed point number."));
                                }

                                string roundedDecimalPart = decimalValue.ToString("F2").Substring(2);

                                encodedNumber = parts[0].PadLeft(attribute.Length - 2, '0') + roundedDecimalPart;

                                if (decimalPart.Length > 2)
                                {
                                    Logger.Instance.Warn($"Value |{number}| was rounded to |{parts[0]}.{roundedDecimalPart}|.");
                                }
                            }
                            else
                            {
                                throw new Exception(Logger.Instance.Error("Fixed point decimal is not in the correct format."));
                            }

                            prop.Setter(entry, encodedNumber);

                            break;
                        case StrsType.Reserved:
                            prop.Setter(entry, new string(' ', prop.Attribute.Length));
                            break;
                        default:
                            throw new Exception(Logger.Instance.Error("Internal Error: Unhandled Type while reading CSV."));
                    }
                }

                results.Add(entry);
            }

            return results;
        }

        public static void WriteCsv(List<StrsEntry> entries, Stream stream)
        {
            using StreamWriter writer = new StreamWriter(stream, Encoding.GetEncoding("iso-8859-1"), -1, true); //latin1

            foreach (var entry in entries)
            {
                bool first = true;

                foreach (var prop in Properties)
                {
                    if (prop.Attribute.Type == StrsType.Reserved)
                    {
                        continue; //Don't write out reserved properties.
                    }

                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        writer.Write(',');
                    }

                    switch (prop.Attribute.Type)
                    {
                        case StrsType.Alphanumeric:
                        case StrsType.Year:
                        case StrsType.Integer:
                            writer.Write(StringPaddingAndTrimming.Unpad(prop.Getter(entry), prop.Attribute));
                            break;
                        case StrsType.FixedPointDecimal:
                            string unpaddedValue = StringPaddingAndTrimming.Unpad(prop.Getter(entry), prop.Attribute);

                            string wholePart = unpaddedValue.Substring(0, unpaddedValue.Length - 2);
                            string fractionPart = unpaddedValue.Substring(wholePart.Length);

                            writer.Write(wholePart);
                            writer.Write('.');
                            writer.Write(fractionPart);
                            break;
                        default:
                            throw new Exception(Logger.Instance.Error($"Internal Error: Unknown Type: {Enum.GetName(typeof(StrsType), prop.Attribute.Type)}"));
                    }
                }

                writer.Write("\r\n");
            }
        }


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is not StrsEntry entry) {
                return false;
            }

            //Compare each entry
            bool equals = true;

            foreach (var property in Properties)
            {
                equals = property.Getter(this) == property.Getter(entry);

                if (!equals)
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();

            foreach (var prop in Properties)
            {
                hash.Add(prop.Getter(this));
            }

            return hash.ToHashCode();
        }
    }
}
