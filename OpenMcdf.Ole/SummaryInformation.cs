using System.Text;

namespace OpenMcdf.Ole;

public sealed class SummaryInformation
{
    /// <summary>
    ///  PIDSI_CODEPAGE
    /// </summary>
    public short CodePage { get; set; }

    /// <summary>
    /// PIDSI_TITLE
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// PIDSI_SUBJECT
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// PIDSI_AUTHOR
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// PIDSI_KEYWORDS
    /// </summary>
    public string? Keywords { get; set; }

    /// <summary>
    /// PIDSI_COMMENTS
    /// </summary>
    public string? Comments { get; set; }

    /// <summary>
    /// PIDSI_TEMPLATE
    /// </summary>
    public string? Template { get; set; }

    /// <summary>
    /// PIDSI_LASTAUTHOR
    /// </summary>
    public string? LastAuthor { get; set; }

    /// <summary>
    /// PIDSI_REVNUMBER
    /// </summary>
    public string? RevisionNumber { get; set; }

    /// <summary>
    /// PIDSI_EDITTIME
    /// </summary>
    public DateTimeOffset? EditTIme { get; set; }

    /// <summary>
    /// PIDSI_LASTPRINTED
    /// </summary>
    public DateTimeOffset? LastPrinted { get; set; }

    /// <summary>
    /// PIDSI_CREATE_DTM
    /// </summary>
    public DateTimeOffset? CreateTime { get; set; }

    /// <summary>
    /// PIDSI_LASTSAVE_DTM
    /// </summary>
    public DateTimeOffset? LastSavedTime { get; set; }

    /// <summary>
    /// PIDSI_PAGECOUNT
    /// </summary>
    public int? PageCount { get; set; }

    /// <summary>
    /// PIDSI_WORDCOUNT
    /// </summary>
    public int? WordCount { get; set; }

    /// <summary>
    /// PIDSI_CHARCOUNT
    /// </summary>
    public int? CharacterCount { get; set; }

    /// <summary>
    /// PIDSI_APPNAME
    /// </summary>
    public string? ApplicationName { get; set; }

    /// <summary>
    /// PIDSI_DOC_SECURITY
    /// </summary>
    public int? DocumentSecurity { get; set; }
}

public static class OleExtensions
{
    public static SummaryInformation? GetSummaryInformation(this CfbStream summaryInformationStream)
    {
        PropertySetStream pStream = new();
        using BinaryReader reader = new(summaryInformationStream, Encoding.Unicode, true);
        pStream.Read(reader);

        if (pStream.FMTID0 != FormatIdentifiers.SummaryInformation)
        {
            throw new Exception("wrong stream type"); // todo: exception type?
        }

        SummaryInformation information = new();

        // load props
        // todo: constants for all the identifiers
        IEnumerable<(uint PropertyIdentifier, IProperty prop)> loadedProperties =
            pStream.PropertySet0.Properties.Zip(pStream.PropertySet0.PropertyIdentifierAndOffsets, (prop, offset) => (offset.PropertyIdentifier, prop));

        foreach (var value in loadedProperties)
        {
            switch (value)
            {
                case (0x00000001, ITypedPropertyValue { VTType: VTPropertyType.VT_I2, Value: short propValue }):
                    information.CodePage = propValue;
                    break;

                case (0x00000002, ITypedPropertyValue { VTType: VTPropertyType.VT_LPSTR or VTPropertyType.VT_LPWSTR, Value: string propValue }):
                    information.Title = propValue.Trim('\0');
                    break;

                case (0x00000003, ITypedPropertyValue { VTType: VTPropertyType.VT_LPSTR or VTPropertyType.VT_LPWSTR, Value: string propValue }):
                    information.Subject = propValue.Trim('\0');
                    break;

                case (0x00000004, ITypedPropertyValue { VTType: VTPropertyType.VT_LPSTR or VTPropertyType.VT_LPWSTR, Value: string propValue }):
                    information.Author = propValue.Trim('\0');
                    break;

                case (0x00000005, ITypedPropertyValue { VTType: VTPropertyType.VT_LPSTR or VTPropertyType.VT_LPWSTR, Value: string propValue }):
                    information.Keywords = propValue.Trim('\0');
                    break;

                case (0x00000006, ITypedPropertyValue { VTType: VTPropertyType.VT_LPSTR or VTPropertyType.VT_LPWSTR, Value: string propValue }):
                    information.Comments = propValue.Trim('\0');
                    break;

                case (0x00000007, ITypedPropertyValue { VTType: VTPropertyType.VT_LPSTR or VTPropertyType.VT_LPWSTR, Value: string propValue }):
                    information.Template = propValue.Trim('\0');
                    break;

                case (0x00000008, ITypedPropertyValue { VTType: VTPropertyType.VT_LPSTR or VTPropertyType.VT_LPWSTR, Value: string propValue }):
                    information.LastAuthor = propValue.Trim('\0');
                    break;

                case (0x00000009, ITypedPropertyValue { VTType: VTPropertyType.VT_LPSTR or VTPropertyType.VT_LPWSTR, Value: string propValue }):
                    information.RevisionNumber = propValue.Trim('\0');
                    break;

                case (0x0000000A, ITypedPropertyValue { VTType: VTPropertyType.VT_FILETIME, Value: DateTime propValue }):
                    information.EditTIme = propValue.ToUniversalTime();
                    break;

                case (0x0000000B, ITypedPropertyValue { VTType: VTPropertyType.VT_FILETIME, Value: DateTime propValue }):
                    information.LastPrinted = propValue.ToUniversalTime();
                    break;

                case (0x0000000C, ITypedPropertyValue { VTType: VTPropertyType.VT_FILETIME, Value: DateTime propValue }):
                    information.CreateTime = propValue.ToUniversalTime();
                    break;

                case (0x0000000D, ITypedPropertyValue { VTType: VTPropertyType.VT_FILETIME, Value: DateTime propValue }):
                    information.LastSavedTime = propValue.ToUniversalTime();
                    break;

                case (0x0000000E, ITypedPropertyValue { VTType: VTPropertyType.VT_I4, Value: int propValue }):
                    information.PageCount = propValue;
                    break;

                case (0x0000000F, ITypedPropertyValue { VTType: VTPropertyType.VT_I4, Value: int propValue }):
                    information.WordCount = propValue;
                    break;

                case (0x00000010, ITypedPropertyValue { VTType: VTPropertyType.VT_I4, Value: int propValue }):
                    information.CharacterCount = propValue;
                    break;

                case (0x00000012, ITypedPropertyValue { VTType: VTPropertyType.VT_LPSTR or VTPropertyType.VT_LPWSTR, Value: string propValue }):
                    information.ApplicationName = propValue.Trim('\0');
                    break;

                case (0x00000013, ITypedPropertyValue { VTType: VTPropertyType.VT_I4, Value: int propValue }):
                    information.DocumentSecurity = propValue;
                    break;
            }
        }

        return information;
    }

    // todo: TryGetSummaryInformation with an out parameter etc?
    public static SummaryInformation? GetSummaryInformation(this RootStorage rootStorage)
    {
        if (rootStorage.TryOpenStream(PropertySetNames.SummaryInformation, out CfbStream? summaryInformationStream))
        {
            using (summaryInformationStream)
                return summaryInformationStream.GetSummaryInformation();
        }

        return null;
    }


}
