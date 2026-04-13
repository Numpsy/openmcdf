namespace OpenMcdf.Ole;

internal class PropertySet
{
    public PropertyContext PropertyContext { get; set; } = new();

    public uint Size { get; set; }

    public List<PropertyIdentifierAndOffset> PropertyIdentifierAndOffsets { get; } = new();

    public List<IProperty> Properties { get; } = new();

    public virtual void LoadContext(int propertySetOffset, BinaryReader br)
    {
        // The CodePage should always be present - threat it being missing as an error
        if (!PropertyIdentifierAndOffsets.Any(pio => pio.PropertyIdentifier == SpecialPropertyIdentifiers.CodePage))
        {
            throw new FileFormatException("Required CodePage property not present");
        }

        ReadContextProperties(propertySetOffset, br);
    }

    private protected void ReadContextProperties(int propertySetOffset, BinaryReader br)
    {
        long currPos = br.BaseStream.Position;

        PropertyIdentifierAndOffset? codePageProperty = PropertyIdentifierAndOffsets.FirstOrDefault(pio => pio.PropertyIdentifier == SpecialPropertyIdentifiers.CodePage);
        if (codePageProperty is not null)
        {
            long codePageOffset = propertySetOffset + codePageProperty.Offset;
            br.BaseStream.Seek(codePageOffset, SeekOrigin.Begin);

            var vType = (VTPropertyType)br.ReadUInt16();
            br.ReadUInt16(); // Ushort Padding
            PropertyContext.CodePage = (ushort)br.ReadInt16();
        }

        PropertyIdentifierAndOffset? localeProperty = PropertyIdentifierAndOffsets.FirstOrDefault(pio => pio.PropertyIdentifier == SpecialPropertyIdentifiers.Locale);
        if (localeProperty is not null)
        {
            long localeOffset = propertySetOffset + localeProperty.Offset;
            br.BaseStream.Seek(localeOffset, SeekOrigin.Begin);

            var vType = (VTPropertyType)br.ReadUInt16();
            br.ReadUInt16(); // Ushort Padding
            PropertyContext.Locale = br.ReadUInt32();
        }

        br.BaseStream.Position = currPos;
    }

    public void Add(IDictionary<uint, string> propertyNames)
    {
        DictionaryProperty dictionaryProperty = new(PropertyContext.CodePage)
        {
            Value = propertyNames,
        };
        Properties.Add(dictionaryProperty);
        PropertyIdentifierAndOffsets.Add(new PropertyIdentifierAndOffset() { PropertyIdentifier = SpecialPropertyIdentifiers.Dictionary, Offset = 0 });
    }
}

internal sealed class HwpSummaryPropertySet : PropertySet
{
    // For HWP Summary information streams, default the codepage to UTF-8
    public HwpSummaryPropertySet()
    {
        PropertyContext.CodePage = 65001;
    }

    public override void LoadContext(int propertySetOffset, BinaryReader br) => ReadContextProperties(propertySetOffset, br);
}
