using System.Text;

namespace IppDecoder;

// Enum for delimiter tags (group tags)
enum IppGroupTag : byte
{
    OperationAttributes = 0x01,
    JobAttributes        = 0x02,
    EndOfAttributes      = 0x03,
    PrinterAttributes    = 0x04,
    UnsupportedAttributes= 0x05
}
// Enum for key IPP value tags (not exhaustive; others handled in parsing logic)
enum IppValueTag : byte
{
    // Out-of-band:
    Unsupported = 0x10,
    Unknown     = 0x12,
    NoValue     = 0x13,
    // Integer types:
    Integer     = 0x21,
    Boolean     = 0x22,
    Enum        = 0x23,
    // Octet-string types:
    OctetString = 0x30,
    DateTime    = 0x31,
    Resolution  = 0x32,
    RangeOfInteger = 0x33,
    BegCollection = 0x34,
    TextWithLanguage = 0x35,
    NameWithLanguage = 0x36,
    EndCollection   = 0x37,
    // Character/string types:
    TextWithoutLanguage = 0x41,
    NameWithoutLanguage = 0x42,
    Keyword     = 0x44,
    Uri         = 0x45,
    UriScheme   = 0x46,
    Charset     = 0x47,
    NaturalLanguage = 0x48,
    MimeMediaType   = 0x49,
    MemberAttrName  = 0x4A
    // (0x50-0x5F are reserved for future string types, 0x7F for extended tags)
}

class IppAttribute
{
    public string Name { get; }
    public IppValueTag ValueTag { get; }
    public List<object> Values { get; } = new List<object>();  // can hold primitive types or nested collections

    public IppAttribute(string name, IppValueTag valueTag)
    {
        Name = name;
        ValueTag = valueTag;
    }
}

class IppAttributeGroup
{
    public IppGroupTag GroupTag { get; }
    public List<IppAttribute> Attributes { get; } = new List<IppAttribute>();
    public IppAttributeGroup(IppGroupTag tag) => GroupTag = tag;
}

class IppMessage
{
    public int VersionMajor { get; set; }
    public int VersionMinor { get; set; }
    public ushort OperationOrStatusCode { get; set; }
    public int RequestId { get; set; }
    public List<IppAttributeGroup> Groups { get; } = new List<IppAttributeGroup>();

    // Look-up dictionaries for human-readable names
    private static readonly Dictionary<ushort, string> StatusCodeNames = new Dictionary<ushort, string> {
        { 0x0000, "successful-ok" },
        { 0x0001, "successful-ok-ignored-or-substituted-attributes" },
        { 0x0002, "successful-ok-conflicting-attributes" },
        { 0x0400, "client-error-bad-request" },
        { 0x0401, "client-error-forbidden" },
        { 0x0402, "client-error-not-authenticated" },
        { 0x0403, "client-error-not-authorized" },
        { 0x0404, "client-error-not-possible" },
        { 0x0405, "client-error-timeout" },
        { 0x0406, "client-error-not-found" },
        { 0x0407, "client-error-gone" },
        { 0x0408, "client-error-request-entity-too-large" },
        { 0x0409, "client-error-request-value-too-long" },
        { 0x040A, "client-error-document-format-not-supported" },
        { 0x040B, "client-error-attributes-or-values-not-supported" },
        { 0x040C, "client-error-uri-scheme-not-supported" },
        { 0x040D, "client-error-charset-not-supported" },
        { 0x040E, "client-error-conflicting-attributes" },
        { 0x0500, "server-error-internal-error" },
        { 0x0501, "server-error-operation-not-supported" },
        { 0x0502, "server-error-service-unavailable" },
        { 0x0503, "server-error-version-not-supported" },
        { 0x0504, "server-error-device-error" },
        { 0x0505, "server-error-temporary-error" },
        { 0x0506, "server-error-not-accepting-jobs" },
        { 0x0507, "server-error-busy" },
        { 0x0508, "server-error-job-canceled" }
    };
    private static readonly Dictionary<ushort, string> OperationNames = new Dictionary<ushort, string> {
        { 0x0002, "Print-Job" },
        { 0x0003, "Print-URI" },
        { 0x0004, "Validate-Job" },
        { 0x0005, "Create-Job" },
        { 0x0006, "Send-Document" },
        { 0x0007, "Send-URI" },
        { 0x0008, "Cancel-Job" },
        { 0x0009, "Get-Job-Attributes" },
        { 0x000A, "Get-Jobs" },
        { 0x000B, "Get-Printer-Attributes" },
        { 0x000C, "Hold-Job" },
        { 0x000D, "Release-Job" },
        { 0x000E, "Restart-Job" },
        { 0x0010, "Pause-Printer" },
        { 0x0011, "Resume-Printer" },
        { 0x0012, "Purge-Jobs" }
    };

    // Pretty-print the decoded IPP message
    public void PrintToConsole()
    {
        Console.WriteLine($"IPP Version: {VersionMajor}.{VersionMinor}");
        bool isResponse = (OperationOrStatusCode & 0xFF00) != 0x0000 || OperationOrStatusCode < 0x0002;
        // Heuristic: operations are typically 0x0002 - 0x003F (per RFC), status codes have high bits (0x01xx,0x04xx,...).
        if (isResponse)
        {
            // Status code
            string statusName = StatusCodeNames.ContainsKey(OperationOrStatusCode)
                                ? StatusCodeNames[OperationOrStatusCode]
                                : "(unknown status)";
            Console.WriteLine($"Status Code: 0x{OperationOrStatusCode:X4} ({statusName})");
        }
        else
        {
            string opName = OperationNames.ContainsKey(OperationOrStatusCode)
                            ? OperationNames[OperationOrStatusCode]
                            : "(unknown operation)";
            Console.WriteLine($"Operation: 0x{OperationOrStatusCode:X4} ({opName})");
        }
        Console.WriteLine($"Request ID: {RequestId}");
        Console.WriteLine();

        foreach (var group in Groups)
        {
            // Print group heading
            string groupName = group.GroupTag switch {
                IppGroupTag.OperationAttributes    => "Operation Attributes",
                IppGroupTag.JobAttributes          => "Job Attributes",
                IppGroupTag.PrinterAttributes      => "Printer Attributes",
                IppGroupTag.UnsupportedAttributes  => "Unsupported Attributes",
                _ => $"Unknown Group (0x{((byte)group.GroupTag):X2})"
            };
            Console.WriteLine($"{groupName}:");
            // Print each attribute in the group
            foreach (var attr in group.Attributes)
            {
                PrintAttribute(attr, indentLevel: 4);
            }
            Console.WriteLine();
        }
    }

    private void PrintAttribute(IppAttribute attr, int indentLevel)
    {
        string indent = new string(' ', indentLevel);
        // Print attribute name and type
        string typeName = attr.ValueTag switch {
            IppValueTag.Integer   => "integer",
            IppValueTag.Boolean   => "boolean",
            IppValueTag.Enum      => "enum",
            IppValueTag.TextWithoutLanguage => "textWithoutLanguage",
            IppValueTag.NameWithoutLanguage => "nameWithoutLanguage",
            IppValueTag.TextWithLanguage    => "textWithLanguage",
            IppValueTag.NameWithLanguage    => "nameWithLanguage",
            IppValueTag.Keyword   => "keyword",
            IppValueTag.Uri       => "uri",
            IppValueTag.UriScheme => "uriScheme",
            IppValueTag.Charset   => "charset",
            IppValueTag.NaturalLanguage => "naturalLanguage",
            IppValueTag.MimeMediaType   => "mimeMediaType",
            IppValueTag.DateTime  => "dateTime",
            IppValueTag.Resolution=> "resolution",
            IppValueTag.RangeOfInteger => "rangeOfInteger",
            IppValueTag.BegCollection   => "collection",
            IppValueTag.EndCollection   => "endCollection",
            IppValueTag.MemberAttrName  => "memberAttrName",
            IppValueTag.Unsupported => "unsupported",
            IppValueTag.Unknown     => "unknown",
            IppValueTag.NoValue     => "no-value",
            _ => attr.ValueTag.ToString()
        };
        // If this attribute is a collection, we'll print its values differently (structured).
        if (attr.ValueTag == IppValueTag.BegCollection)
        {
            Console.WriteLine($"{indent}{attr.Name} (collection):");
            // Each value in attr.Values for a collection is a List<IppAttribute> (sub-attributes)
            int subIndent = indentLevel + 4;
            foreach (var val in attr.Values)
            {
                if (val is List<IppAttribute> collection)
                {
                    Console.WriteLine($"{new string(' ', subIndent-2)}{{");  // open brace for collection value
                    foreach (var subAttr in collection)
                    {
                        PrintAttribute(subAttr, subIndent);
                    }
                    Console.WriteLine($"{new string(' ', subIndent-2)}}},");  // close brace for collection (comma if multiple)
                }
            }
        }
        else
        {
            // Non-collection attribute
            if (attr.Values.Count == 0)
            {
                Console.WriteLine($"{indent}{attr.Name} ({typeName}): (no value)");
                return;
            }
            if (attr.Values.Count == 1)
            {
                // Single-valued attribute
                Console.WriteLine($"{indent}{attr.Name} ({typeName}): {FormatValue(attr.Values[0], attr)}");
            }
            else
            {
                // Multi-valued attribute: list each value (comma-separated or on new lines)
                Console.WriteLine($"{indent}{attr.Name} ({typeName}):");
                for (int i = 0; i < attr.Values.Count; i++)
                {
                    object val = attr.Values[i];
                    string formatted = FormatValue(val, attr);
                    Console.WriteLine($"{indent}  - {formatted}");
                }
            }
        }
    }

    private string FormatValue(object value, IppAttribute attrContext)
    {
        // Format a value object for printing, possibly mapping known enums etc.
        switch (value)
        {
            case string s:
                return s;  // already a string (for text, name, keyword, uri, etc.)
            case bool b:
                return b ? "true" : "false";
            case int i:
                // If this attribute is known to be an enum with specific meanings, map if possible.
                if (attrContext.Name == "printer-state")
                {
                    // IPP printer-state enum values: 3=idle, 4=processing, 5=stopped
                    return i switch {
                        3 => "3 (idle)",
                        4 => "4 (processing)",
                        5 => "5 (stopped)",
                        _ => i.ToString()
                    };
                }
                if (attrContext.Name == "job-state")
                {
                    // IPP job-state: 3=pending,4=held,5=processing,6=stopped,7=canceled,8=aborted,9=completed
                    return i switch {
                        3 => "3 (pending)",
                        4 => "4 (pending-held)",
                        5 => "5 (processing)",
                        6 => "6 (processing-stopped)",
                        7 => "7 (canceled)",
                        8 => "8 (aborted)",
                        9 => "9 (completed)",
                        _ => i.ToString()
                    };
                }
                return i.ToString();
            case DateTimeOffset dto:
                // Include offset in format, include a decimal second if fraction exists
                string basic = dto.ToString("yyyy-MM-ddTHH:mm:ss");
                if (dto.Millisecond != 0)
                {
                    // Millisecond here actually represents deciseconds * 100. (We only had one decimal digit precision)
                    int deci = dto.Millisecond / 100;
                    basic += "." + deci;
                }
                string tz = dto.ToString("K"); // includes offset like +02:00
                return basic + tz;
            case ResolutionValue res:
                return $"{res.X}x{res.Y} {(res.IsDotsPerInch ? "dpi" : "dpcm")}";
            case RangeValue range:
                return $"{range.Lower} to {range.Upper}";
            case byte[] octets:
                // Represent octetString as hex dump (shorten if long)
                const int maxBytes = 16;
                int len = octets.Length;
                StringBuilder hex = new StringBuilder();
                int displayLen = Math.Min(len, maxBytes);
                for (int j = 0; j < displayLen; j++)
                    hex.Append(octets[j].ToString("X2"));
                string hexStr = hex.ToString();
                if (len > maxBytes) hexStr += "...";
                return $"<octetString: 0x{hexStr} ({len} bytes)>";
            case IppStringWithLanguage langText:
                // Format as "text (language)"
                return $"\"{langText.Text}\" (language: {langText.Language})";
            default:
                return value?.ToString() ?? "";
        }
    }
}

// Helper struct for resolution values
struct ResolutionValue { public int X, Y; public bool IsDotsPerInch; }
// Helper struct for range of integer values
struct RangeValue { public int Lower, Upper; }
// Helper class for text/name with language
class IppStringWithLanguage {
    public string Text { get; }
    public string Language { get; }
    public IppStringWithLanguage(string lang, string text) { Language = lang; Text = text; }
}

class IppParser
{
    public static IppMessage Parse(byte[] data)
    {
        IppMessage message = new IppMessage();
        int index = 0;
        if (data.Length < 8) throw new Exception("Data too short for IPP header.");
        // Parse header
        message.VersionMajor = data[index++];
        message.VersionMinor = data[index++];
        message.OperationOrStatusCode = ReadUInt16(data, ref index);
        message.RequestId = ReadInt32(data, ref index);

        // Parse attribute groups until end-of-attributes tag
        bool endOfAttrs = false;
        while (!endOfAttrs && index < data.Length)
        {
            byte tag = data[index];
            if (tag == (byte)IppGroupTag.EndOfAttributes)
            {
                // End-of-attributes reached
                index++;
                endOfAttrs = true;
            }
            else if (IsDelimiterTag(tag))
            {
                // Begin a new attribute group
                IppGroupTag groupTag = Enum.IsDefined(typeof(IppGroupTag), tag)
                                        ? (IppGroupTag)tag
                                        : IppGroupTag.OperationAttributes; // treat unknown as generic
                index++;
                IppAttributeGroup group = new IppAttributeGroup(groupTag);
                // Parse attributes in this group until a new delimiter or end-of-attrs
                while (index < data.Length)
                {
                    byte attrTag = data[index];
                    if (attrTag <= 0x0F)
                    {
                        // We've hit the next delimiter (group tag or end-of-attributes) – break to outer loop
                        break;
                    }
                    // Parse one attribute (could be multi-valued)
                    IppAttribute attribute = ParseAttribute(data, ref index);
                    group.Attributes.Add(attribute);
                    // After ParseAttribute returns, it will have consumed all values of that attribute (including any repeated 0-name values).
                }
                message.Groups.Add(group);
            }
            else
            {
                // If we encounter a value tag without a preceding group delimiter, the IPP message is malformed.
                throw new Exception($"Unexpected tag 0x{tag:X2} at position {index} (expected a group delimiter).");
            }
        }
        return message;
    }

    private static bool IsDelimiterTag(byte tag) => tag >= 0x00 && tag <= 0x0F;

    private static IppAttribute ParseAttribute(byte[] data, ref int index)
    {
        // First byte is value tag
        byte tagByte = data[index++];
        if (IsDelimiterTag(tagByte))
            throw new Exception("Delimiter tag encountered where value tag expected.");
        IppValueTag valueTag = Enum.IsDefined(typeof(IppValueTag), tagByte)
                                ? (IppValueTag)tagByte
                                : (IppValueTag)tagByte; // use the byte as-is if unknown (to handle vendor extensions gracefully)
        ushort nameLen = ReadUInt16(data, ref index);
        string name = nameLen > 0 ? Encoding.UTF8.GetString(data, index, nameLen) : string.Empty;
        index += nameLen;
        ushort valueLen = ReadUInt16(data, ref index);
        // Create attribute (if nameLen is 0, this is actually a continuation, which should be handled in context of previous attr, not here)
        if (name.Length == 0)
        {
            throw new Exception("Attribute name length 0 in new attribute context (malformed IPP message).");
        }
        IppAttribute attr = new IppAttribute(name, valueTag);
        // Parse first value
        ParseAttributeValue(data, ref index, valueTag, valueLen, attr);
        // Check for additional values for this attribute (name-length will be 0 for continuations)
        while (index < data.Length)
        {
            byte nextTag = data[index];
            if (nextTag <= 0x0F)
            {
                // Next is a delimiter (start of new group or end), so no more values for this attribute
                break;
            }
            // Peek the name-length of the next attribute/value
            ushort nextNameLen = (ushort)((data[index+1] << 8) | data[index+2]);
            if (nextNameLen != 0)
            {
                // Next attribute has a non-zero name, so this attribute's values are done.
                break;
            }
            // If we reach here, we have an additional value for the same attribute (name-length = 0).
            // Consume the next tag and zero name-length, then parse the value.
            byte nextValueTagByte = data[index++];
            IppValueTag nextValueTag = Enum.IsDefined(typeof(IppValueTag), nextValueTagByte)
                                        ? (IppValueTag)nextValueTagByte
                                        : (IppValueTag)nextValueTagByte;
            // Read and skip the name-length (should be 0)
            ushort zero = ReadUInt16(data, ref index);
            if (zero != 0)
            {
                throw new Exception("Expected zero-length attribute name for additional value.");
            }
            ushort nextValueLen = ReadUInt16(data, ref index);
            ParseAttributeValue(data, ref index, nextValueTag, nextValueLen, attr);
        }
        return attr;
    }

    private static void ParseAttributeValue(byte[] data, ref int index, IppValueTag tag, ushort valueLen, IppAttribute attr)
    {
        switch (tag)
        {
            case IppValueTag.TextWithLanguage:
            case IppValueTag.NameWithLanguage:
                // Composite value: two shorts and two strings (language and text)
                if (valueLen < 4)
                {
                    attr.Values.Add(string.Empty);
                    index += valueLen; // safety: skip if abnormal
                }
                else
                {
                    ushort langLen = (ushort)((data[index] << 8) | data[index+1]);
                    index += 2;
                    string language = langLen > 0 ? Encoding.UTF8.GetString(data, index, langLen) : string.Empty;
                    index += langLen;
                    ushort textLen = (ushort)((data[index] << 8) | data[index+1]);
                    index += 2;
                    string text = textLen > 0 ? Encoding.UTF8.GetString(data, index, textLen) : string.Empty;
                    index += textLen;
                    attr.Values.Add(new IppStringWithLanguage(language, text));
                }
                break;
            case IppValueTag.BegCollection:
                // Start of a collection value.
                // According to RFC, valueLen for begCollection is typically 0:contentReference[oaicite:26]{index=26} (no immediate value bytes), but we will skip any bytes if valueLen > 0 just in case.
                index += valueLen;
                // Parse collection members until EndCollection
                List<IppAttribute> collectionAttrs = new List<IppAttribute>();
                while (index < data.Length)
                {
                    byte memberTag = data[index];
                    if (memberTag == (byte)IppValueTag.EndCollection)
                    {
                        // Consume EndCollection (tag + 2-byte nameLen + 2-byte valueLen)
                        index++;
                        ushort endNameLen = ReadUInt16(data, ref index);
                        ushort endValLen = ReadUInt16(data, ref index);
                        // endNameLen and endValLen should be 0 per spec:contentReference[oaicite:27]{index=27}; we can ignore them.
                        break;
                    }
                    if (memberTag == (byte)IppValueTag.MemberAttrName)
                    {
                        // Parse member attribute name
                        index++;
                        ushort memberNameLen = ReadUInt16(data, ref index);
                        string memberName = memberNameLen > 0 ? Encoding.UTF8.GetString(data, index, memberNameLen) : string.Empty;
                        index += memberNameLen;
                        ushort memberValueNameLen = ReadUInt16(data, ref index); // length of the name string value
                        string subAttrName = memberValueNameLen > 0 ? Encoding.UTF8.GetString(data, index, memberValueNameLen) : string.Empty;
                        index += memberValueNameLen;
                        // Now we have a sub-attribute name (subAttrName). Next, its value follows.
                        // Determine next tag to see the type of the sub-attribute's value.
                        byte subAttrTagByte = data[index];
                        index++;
                        IppValueTag subAttrTag = Enum.IsDefined(typeof(IppValueTag), subAttrTagByte)
                                                   ? (IppValueTag)subAttrTagByte
                                                   : (IppValueTag)subAttrTagByte;
                        // Read name-length (should be 0 for the value of this sub-attribute, since the name was provided via MemberAttrName)
                        ushort subAttrNameLen = ReadUInt16(data, ref index);
                        if (subAttrNameLen != 0)
                            throw new Exception("Expected zero name-length for member attribute value.");
                        ushort subAttrValueLen = ReadUInt16(data, ref index);
                        // Create sub-attribute and parse its value(s)
                        IppAttribute subAttr = new IppAttribute(subAttrName, subAttrTag);
                        ParseAttributeValue(data, ref index, subAttrTag, subAttrValueLen, subAttr);
                        // Check for multi-valued sub-attribute (additional values with name-length 0)
                        while (index < data.Length)
                        {
                            byte nextSubTag = data[index];
                            if (nextSubTag == (byte)IppValueTag.EndCollection || nextSubTag == (byte)IppValueTag.MemberAttrName)
                            {
                                // Next attribute of collection or end of collection, so break multi-value loop
                                break;
                            }
                            // Additional value for the current sub-attribute:
                            // Consume tag and zero name-length
                            byte addTagByte = data[index++];
                            IppValueTag addTag = Enum.IsDefined(typeof(IppValueTag), addTagByte)
                                                    ? (IppValueTag)addTagByte
                                                    : (IppValueTag)addTagByte;
                            ushort addNameLen = ReadUInt16(data, ref index);
                            if (addNameLen != 0)
                                break; // if it's not 0, it's not an additional value (shouldn't happen here).
                            ushort addValLen = ReadUInt16(data, ref index);
                            ParseAttributeValue(data, ref index, addTag, addValLen, subAttr);
                        }
                        collectionAttrs.Add(subAttr);
                    }
                    else
                    {
                        // We encountered something other than MemberAttrName or EndCollection inside a collection – this is unexpected.
                        throw new Exception($"Unexpected tag 0x{data[index]:X2} inside collection (position {index})");
                    }
                }
                // Add the parsed collection (as a list of attributes) to the parent attribute's values
                attr.Values.Add(collectionAttrs);
                break;
            case IppValueTag.Integer:
            case IppValueTag.Enum:
                // 4-byte signed integer
                if (valueLen != 4)
                {
                    // If length is not 4, handle gracefully by reading min(length,4) bytes
                    int intBytes = Math.Min(4, (int)valueLen);
                    int val = 0;
                    for (int i = 0; i < intBytes; i++)
                    {
                        val = (val << 8) | data[index + i];
                    }
                    // Sign-extend if needed
                    if (intBytes == 4)
                    {
                        // interpret as signed 32-bit
                        // (In C#, shifting in above loop already gave us a 32-bit int. It's correctly signed if MSB was 1.)
                    }
                    attr.Values.Add(val);
                    index += valueLen;
                }
                else
                {
                    int iVal = (data[index] << 24) | (data[index+1] << 16) | (data[index+2] << 8) | data[index+3];
                    // Interpret as signed
                    attr.Values.Add(iVal);
                    index += 4;
                }
                break;
            case IppValueTag.Boolean:
                // 1-byte boolean
                if (valueLen >= 1)
                {
                    byte bVal = data[index];
                    attr.Values.Add(bVal != 0);
                }
                else
                {
                    attr.Values.Add(false);
                }
                index += valueLen;
                break;
            case IppValueTag.DateTime:
                if (valueLen == 11)
                {
                    // 11 bytes as per RFC2579 DateAndTime
                    ushort year = (ushort)((data[index] << 8) | data[index+1]);
                    byte month = data[index+2];
                    byte day = data[index+3];
                    byte hour = data[index+4];
                    byte minute = data[index+5];
                    byte second = data[index+6];
                    byte deciSeconds = data[index+7];
                    sbyte tzSign = (sbyte)data[index+8];  // This might be signed 8-bit representing direction (+/-)
                    byte tzHours = data[index+9];
                    byte tzMinutes = data[index+10];
                    // Calculate offset:
                    int offsetMinutes = tzHours * 60 + tzMinutes;
                    if (tzSign == '-') offsetMinutes = -offsetMinutes;
                    var offset = TimeSpan.FromMinutes(offsetMinutes);
                    try
                    {
                        var dto = new DateTimeOffset(year, month, day, hour, minute, second, offset);
                        // deciSeconds is tenths of a second
                        if (deciSeconds >= 0 && deciSeconds < 10)
                        {
                            dto = dto.AddMilliseconds(deciSeconds * 100);
                        }
                        attr.Values.Add(dto);
                    }
                    catch
                    {
                        // Fallback: store raw components if DateTimeOffset constructor fails (e.g., out-of-range date)
                        attr.Values.Add($"{year:D4}-{month:D2}-{day:D2} {hour:D2}:{minute:D2}:{second:D2}.{deciSeconds} TZ={tzSign}{tzHours:D2}:{tzMinutes:D2}");
                    }
                }
                else
                {
                    // If length is not 11, just store raw bytes
                    attr.Values.Add(GetOctetString(data, index, valueLen));
                }
                index += valueLen;
                break;
            case IppValueTag.Resolution:
                if (valueLen == 9)
                {
                    int xres = (data[index] << 24) | (data[index+1] << 16) | (data[index+2] << 8) | data[index+3];
                    int yres = (data[index+4] << 24) | (data[index+5] << 16) | (data[index+6] << 8) | data[index+7];
                    byte unit = data[index+8];
                    attr.Values.Add(new ResolutionValue { X = xres, Y = yres, IsDotsPerInch = (unit == 3) });
                }
                else
                {
                    // handle unexpected length by reading what we can
                    if (valueLen >= 9)
                    {
                        int xres = (data[index] << 24) | (data[index+1] << 16) | (data[index+2] << 8) | data[index+3];
                        int yres = (data[index+4] << 24) | (data[index+5] << 16) | (data[index+6] << 8) | data[index+7];
                        byte unit = data[index+8];
                        attr.Values.Add(new ResolutionValue { X = xres, Y = yres, IsDotsPerInch = (unit == 3) });
                    }
                    else
                    {
                        attr.Values.Add($"(invalid resolution data, length={valueLen})");
                    }
                }
                index += valueLen;
                break;
            case IppValueTag.RangeOfInteger:
                if (valueLen == 8)
                {
                    int lower = (data[index] << 24) | (data[index+1] << 16) | (data[index+2] << 8) | data[index+3];
                    int upper = (data[index+4] << 24) | (data[index+5] << 16) | (data[index+6] << 8) | data[index+7];
                    attr.Values.Add(new RangeValue { Lower = lower, Upper = upper });
                }
                else if (valueLen >= 8)
                {
                    int lower = (data[index] << 24) | (data[index+1] << 16) | (data[index+2] << 8) | data[index+3];
                    int upper = (data[index+4] << 24) | (data[index+5] << 16) | (data[index+6] << 8) | data[index+7];
                    attr.Values.Add(new RangeValue { Lower = lower, Upper = upper });
                }
                else
                {
                    attr.Values.Add($"(invalid rangeOfInteger data, length={valueLen})");
                }
                index += valueLen;
                break;
            case IppValueTag.Unsupported:
            case IppValueTag.Unknown:
            case IppValueTag.NoValue:
                // Out-of-band: no value bytes (valueLen should be 0 as per spec:contentReference[oaicite:28]{index=28})
                attr.Values.Add(tag == IppValueTag.Unsupported ? "unsupported"
                               : tag == IppValueTag.Unknown   ? "unknown"
                               : "no-value");
                index += valueLen;
                break;
            default:
                // All other types (octetString, text, name, keyword, uri, etc.)
                if (valueLen > 0)
                {
                    if (tag == IppValueTag.OctetString)
                    {
                        // Arbitrary binary data
                        attr.Values.Add(GetOctetString(data, index, valueLen));
                    }
                    else
                    {
                        // Treat as string
                        string text = Encoding.UTF8.GetString(data, index, valueLen);
                        attr.Values.Add(text);
                    }
                }
                else
                {
                    // zero-length value (empty string or no data)
                    attr.Values.Add(string.Empty);
                }
                index += valueLen;
                break;
        }
    }

    private static byte[] GetOctetString(byte[] data, int offset, int length)
    {
        byte[] bytes = new byte[length];
        Array.Copy(data, offset, bytes, 0, length);
        return bytes;
    }

    private static ushort ReadUInt16(byte[] data, ref int index)
    {
        if (index + 1 >= data.Length) throw new IndexOutOfRangeException();
        ushort value = (ushort)((data[index] << 8) | data[index+1]);
        index += 2;
        return value;
    }

    private static int ReadInt32(byte[] data, ref int index)
    {
        if (index + 3 >= data.Length) throw new IndexOutOfRangeException();
        int value = (data[index] << 24) | (data[index+1] << 16) | (data[index+2] << 8) | data[index+3];
        index += 4;
        return value;
    }
}

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: IppDecoder <ipp-binary-file>");
            return;
        }
        string filePath = args[0];
        if (!File.Exists(filePath))
        {
            Console.Error.WriteLine($"File not found: {filePath}");
            return;
        }
        byte[] data = File.ReadAllBytes(filePath);
        try
        {
            IppMessage msg = IppParser.Parse(data);
            msg.PrintToConsole();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error parsing IPP message: " + ex.Message);
        }
    }
}
