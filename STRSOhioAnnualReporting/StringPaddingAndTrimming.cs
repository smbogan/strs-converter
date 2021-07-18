using System;
using System.Linq;

namespace STRSOhioAnnualReporting
{
    public static class StringPaddingAndTrimming
    {
        public static Action<T, string> Pad<T>(Action<T, string> setter, StrsPadding paddingType, char paddingCharacter, int width)
        {
            return (t, s) => {
                string p;

                switch (paddingType)
                {
                    case StrsPadding.Left:
                        p = s.PadLeft(width, paddingCharacter);
                        break;
                    case StrsPadding.Right:
                        p = s.PadRight(width, paddingCharacter);
                        break;
                    case StrsPadding.Fail:
                        if (s.Length != width)
                        {
                            throw new Exception(Logger.Instance.Error($"The input data was not the expected length ({ width }): {s}"));
                        }
                        p = s;
                        break;
                    case StrsPadding.Default:
                        throw new Exception(Logger.Instance.Error("Internal Error: Padding default was not defined."));
                    default:
                        throw new Exception(Logger.Instance.Error($"Internal Error: Unknown padding type: { Enum.GetName(typeof(StrsPadding), paddingType) }"));
                }

                setter(t, p);
            };
        }

        public static Action<T, string> Trim<T>(Action<T, string> setter, StrsTrimming trimmingType, char paddingCharacter, int width)
        {
            return (t, s) => {
                string p;

                switch (trimmingType)
                {
                    case StrsTrimming.Left:
                        if (s.Length > width)
                        {
                            p = s.Substring(s.Length - width, width);
                            Logger.Instance.Warn($"Value trimmed on the left from |{ s }| to |{ p }|.");
                        }
                        else
                        {
                            p = s;
                        }
                        break;
                    case StrsTrimming.Right:
                        if (s.Length > width)
                        {
                            p = s.Substring(0, width);
                            Logger.Instance.Warn($"Value trimmed on the right from |{ s }| to |{ p }|.");
                        }
                        else
                        {
                            p = s;
                        }
                        break;
                    case StrsTrimming.LeftOnlyPadding:
                        if (s.Length > width)
                        {
                            string trim = s.Substring(0, s.Length - width);

                            if (trim.Sum((c) => c == paddingCharacter ? 0 : 1) > 0)
                            {
                                throw new Exception(Logger.Instance.Error($"Attempted to truncate |{s}| to a width of { width }, but the leftmost { s.Length - width } characters were not all '{ paddingCharacter }'."));
                            }

                            p = s.Substring(s.Length - width, width);
                            Logger.Instance.Warn($"Value trimmed on the left from |{ s }| to |{ p }|.");
                        }
                        else
                        {
                            p = s;
                        }
                        break;
                    case StrsTrimming.RightOnlyPadding:
                        if (s.Length > width)
                        {
                            string trim = s.Substring(width, s.Length - width);

                            if (trim.Sum((c) => c == paddingCharacter ? 0 : 1) > 0)
                            {
                                throw new Exception(Logger.Instance.Error($"Attempted to truncate |{s}| to a width of { width }, but the rightmost { s.Length - width } characters were not all '{ paddingCharacter }'."));
                            }

                            p = s.Substring(0, width);
                            Logger.Instance.Warn($"Value trimmed on the right from |{ s }| to |{ p }|.");
                        }
                        else
                        {
                            p = s;
                        }
                        break;
                    case StrsTrimming.Default:
                        throw new Exception(Logger.Instance.Error("Internal Error: Trimming default was not defined."));
                    case StrsTrimming.Fail:
                        if (s.Length != width)
                        {
                            throw new Exception(Logger.Instance.Error($"The input data was not the expected length ({ width }): {s}"));
                        }
                        p = s;
                        break;
                    default:
                        throw new Exception(Logger.Instance.Error($"Internal Error: Unknown trimming type: { Enum.GetName(typeof(StrsTrimming), trimmingType) }"));
                }

                setter(t, p);
            };
        }

        public static Action<T, string> TrimAndPad<T>(Action<T, string> setter, StrsDefinitionAttribute definition)
        {
            return Trim(Pad(setter, definition.Padding, definition.PaddingCharacter, definition.Length), definition.Trimming, definition.PaddingCharacter, definition.Length);
        }

        public static string Unpad(string value, StrsDefinitionAttribute definition)
        {
            //Used for outputting to CSV (i.e. trims off the Dat extra info)

            switch (definition.Padding)
            {
                case StrsPadding.Left:
                    return value.TrimStart(definition.PaddingCharacter);
                case StrsPadding.Right:
                    return value.TrimEnd(definition.PaddingCharacter);
                case StrsPadding.Default:
                    throw new Exception(Logger.Instance.Error("Internal Error: Padding default was not defined."));
                case StrsPadding.Fail:
                    return value;
                default:
                    throw new Exception(Logger.Instance.Error($"Internal Error: Unknown padding type: { Enum.GetName(typeof(StrsPadding), definition.Padding) }"));
            }
        }
    }
}
