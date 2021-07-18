using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace STRSOhioAnnualReporting
{
    public static class CsvLine
    {
        private class Pointer
        {
            public string Text { get; set; }
            public int Value { get; set; } = 0;

            public char? Current
            {
                get
                {
                    if (Value >= Text.Length)
                        return null;

                    return Text[Value];
                }
            }

            public Pointer(string text)
            {
                Text = text;
            }
        }

        delegate State State(Pointer position, StringBuilder entry, Action<string> emit);

        private static State Start(Pointer position, StringBuilder entry, Action<string> emit)
        {
            if (position.Current == null)
            {
                return null; //Empty line
            }

            return PreEntry;
        }

        private static State PreEntry(Pointer position, StringBuilder entry, Action<string> emit)
        {
            entry.Clear();

            char? c = position.Current;

            switch (c)
            {
                case null:
                    emit(""); //We've reached an entry with nothing in it at the end of a line
                    return null;
                case '"':
                    position.Value++;
                    return InQuotedEntry;
                case ',':
                    return PostEntry;
                default:
                    position.Value++;
                    entry.Append(c.Value);
                    return InEntry;
            }
        }

        

        private static State InQuotedEntry(Pointer position, StringBuilder entry, Action<string> emit)
        {
            switch (position.Current)
            {
                case null:
                    throw new Exception(Logger.Instance.Error("Invalid CSV. Unexpected end of line while inside of a quoted entry."));
                case '"':
                    position.Value++;
                    return InQuotedEntryAtQuote;
                default:
                    entry.Append(position.Current);
                    position.Value++;
                    return InQuotedEntry;
            }
        }

        private static State InQuotedEntryAtQuote(Pointer position, StringBuilder entry, Action<string> emit)
        {
            switch (position.Current)
            {
                case null:
                    emit(entry.ToString()); //End of the line
                    return null;
                case '"':
                    //We have an escaped double quote
                    entry.Append('"');
                    position.Value++;
                    return InQuotedEntry;
                case ',':
                    return PostEntry;
                default:
                    throw new Exception(Logger.Instance.Error("Invalid CSV. Unexpected character following a double quote inside of a quoted entry. Expecting another double quote, comma, or the end of the line."));
            }
        }

        private static State InEntry(Pointer position, StringBuilder entry, Action<string> emit)
        {
            char? c = position.Current;

            switch (c)
            {
                case null:
                    emit(entry.ToString()); //We've reached the end of a line
                    return null;
                case ',':
                    return PostEntry;
                default:
                    position.Value++;
                    entry.Append(c.Value);
                    return InEntry;
            }
        }

        private static State PostEntry(Pointer position, StringBuilder entry, Action<string> emit)
        {
            //We can only get here on the arrival of a comma
            Debug.Assert(position.Current == ',');

            position.Value++; //Consume the comma

            //Emit the previous entry
            emit(entry.ToString());

            switch(position.Current)
            {
                case null:
                    return null;
                default:
                    return PreEntry;
            }
        }

        public static IEnumerable<string> Split(string line)
        {
            Queue<string> outputs = new Queue<string>();
            StringBuilder currentEntry = new StringBuilder();
            Pointer pointer = new Pointer(line);

            State currentState = Start;

            Action<string> emitter = (s) => {
                outputs.Enqueue(s);
            };

            while (currentState != null)
            {
                currentState = currentState(pointer, currentEntry, emitter);

                //Yield any results that were obtained
                while (outputs.Count > 0)
                {
                    yield return outputs.Dequeue();
                }
            }
        }

        private static string Escape(string s)
        {
            StringBuilder sb = new StringBuilder();

            bool foundComma = false;
            bool foundQuote = false;

            foreach (var c in s)
            {
                switch (c)
                {
                    case ',':
                        foundComma = true;
                        break;
                    case '"':
                        foundQuote = true;
                        sb.Append('"'); //Escape the double quote.
                        break;
                    default:
                        break;
                }

                sb.Append(c);
            }

            if (foundComma || foundQuote)
            {
                sb.Insert(0, '"');
                sb.Append('"');
            }

            return sb.ToString();
        }

        public static string Join(IEnumerable<string> data)
        {
            return string.Join(',',
                from d in data
                select Escape(d)
                );
        }
    }
}
