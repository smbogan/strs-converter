using System;
namespace STRSOhioAnnualReporting
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class StrsDefinitionAttribute : Attribute
    {
        public int StartingPosition { get; }
        public int Length { get; }
        public StrsType Type { get; }
        public StrsTrimming Trimming { get; }
        public StrsPadding Padding { get; }
        public char PaddingCharacter { get; }

        public StrsDefinitionAttribute(int startingPosition, int length, StrsType strsType, StrsTrimming trimming = StrsTrimming.Default, StrsPadding padding = StrsPadding.Default, char paddingCharacter = '\0')
        {
            StartingPosition = startingPosition;
            Length = length;
            Type = strsType;

            //Determine the trimming side (left/right)
            if (trimming == StrsTrimming.Default)
            {
                switch (strsType)
                {
                    case StrsType.Alphanumeric:
                    case StrsType.Reserved: //Doesn't matter
                        Trimming = StrsTrimming.Right;
                        break;
                    case StrsType.Year:
                        Trimming = StrsTrimming.Fail;
                        break;
                    case StrsType.Integer:
                    case StrsType.FixedPointDecimal:
                        Trimming = StrsTrimming.LeftOnlyPadding;
                        break;
                }
            }
            else
            {
                Trimming = trimming;
            }

            //Determine padding side (left/right)
            if (padding == StrsPadding.Default)
            {
                switch (strsType)
                {
                    case StrsType.Reserved: //Doesn't really matter for reserved - since the entire string is filled
                    case StrsType.Alphanumeric:
                        Padding = StrsPadding.Right;
                        break;
                    case StrsType.FixedPointDecimal:
                    case StrsType.Integer:
                        Padding = StrsPadding.Left;
                        break;
                    case StrsType.Year:
                        Padding = StrsPadding.Fail; //Never pad years - they have to be in the correct format.
                        break;
                    default:
                        throw new Exception($"No default padding type for the type: { Enum.GetName(typeof(StrsType), strsType) }");
                }
            }
            else
            {
                Padding = padding;
            }

            //Determine the default character if not provided
            if (paddingCharacter == '\0')
            {
                switch (strsType)
                {
                    case StrsType.Alphanumeric:
                    case StrsType.Reserved:
                        PaddingCharacter = ' ';
                        break;
                    case StrsType.Year:
                    case StrsType.Integer:
                    case StrsType.FixedPointDecimal:
                        PaddingCharacter = '0';
                        break;
                    default:
                        throw new Exception($"No default padding character for the type: { Enum.GetName(typeof(StrsType), strsType) }");
                }
            }
            else
            {
                PaddingCharacter = paddingCharacter;
            }
        }
    }
}
