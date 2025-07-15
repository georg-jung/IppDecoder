# IPP Decoder

This is a simple .NET tool to decode and print binary Internet Printing Protocol (IPP) messages to the console in a human-readable format. It's designed to parse the attribute-based structure of IPP messages and display them, including operation/status codes, request IDs, and attribute groups.

## Features

- Decodes IPP version, operation/status code, and request ID.
- Parses IPP attribute groups, including:
  - `OperationAttributes`
  - `JobAttributes`
  - `PrinterAttributes`
  - `UnsupportedAttributes`
- Supports a wide range of IPP value tags, from integers and booleans to strings, collections, and date-time objects.
- Handles complex data types like `resolution`, `rangeOfInteger`, and nested `collection` attributes.
- Provides human-readable names for common IPP operations and status codes.
- Formats special integer enum values like `printer-state` and `job-state` with their well-known meanings (e.g., `3 (idle)`).
- Gracefully handles unknown or vendor-specific tags.

## Building the Project

This is a standard .NET project. You can build it using the .NET CLI:

```sh
dotnet build --configuration Release
```

The executable will be located in the `bin/Release/net<version>` directory.

## Usage

Run the decoder from the command line, providing the path to a binary IPP file as an argument.

```sh
# On Windows
IppDecoder.exe <path-to-ipp-file.bin>

# On Linux/macOS
./IppDecoder <path-to-ipp-file.bin>
```

## Example Output

The output for a `Get-Printer-Attributes` response might look something like this:

```txt
IPP Version: 1.1
Status Code: 0x0000 (successful-ok)
Request ID: 1

Operation Attributes:
    attributes-charset (charset): utf-8
    attributes-natural-language (naturalLanguage): en

Printer Attributes:
    queued-job-count (integer): 0
    uri-authentication-supported (keyword): none
    uri-security-supported (keyword): tls
    copies-default (integer): 1
    document-format-supported (mimeMediaType):
      - application/octet-stream
      - image/jpeg
      - image/urf
      - image/pwg-raster
    document-format-default (mimeMediaType): application/octet-stream
    orientation-requested-supported (enum): 3
    orientation-requested-default (enum): 3
    jpeg-k-octets-supported (rangeOfInteger): 0 to 12288
    jpeg-x-dimension-supported (rangeOfInteger): 16 to 9600
    jpeg-y-dimension-supported (rangeOfInteger): 16 to 9600
    color-supported (boolean): true
    finishings-supported (enum): 3
    finishings-default (enum): 3
    output-bin-supported (keyword): face-up
    output-bin-default (keyword): face-up
    print-color-mode-supported (keyword):
      - color
      - monochrome
      - auto
      - auto-monochrome
    output-mode-supported (keyword):
      - color
      - monochrome
      - auto
      - auto-monochrome
    print-color-mode-default (keyword): color
    output-mode-default (keyword): color
    pages-per-minute (integer): 6
    pages-per-minute-color (integer): 2
    pdf-versions-supported (keyword): none
    printer-resolution-supported (resolution): 600x600 dpi
    printer-resolution-default (resolution): 600x600 dpi
    print-quality-supported (enum):
      - 4
      - 5
    print-quality-default (enum): 4
    sides-supported (keyword): one-sided
    sides-default (keyword): one-sided
    landscape-orientation-requested-preferred (enum): 5
    printer-uuid (uri): urn:uuid:00000000-0000-1000-1000-123456789123
    charset-configured (charset): us-ascii
    charset-supported (charset):
      - us-ascii
      - utf-8
    compression-supported (keyword): none
    copies-supported (rangeOfInteger): 1 to 99
    generated-natural-language-supported (naturalLanguage): en-us
    ipp-versions-supported (keyword):
      - 1.1
      - 2.0
    job-creation-attributes-supported (keyword):
      - copies
      - finishings
      - sides
      - orientation-requested
      - media
      - print-quality
      - printer-resolution
      - output-bin
      - media-col
      - print-color-mode
      - ipp-attribute-fidelity
      - job-name
    media-col-supported (keyword):
      - media-bottom-margin
      - media-left-margin
      - media-right-margin
      - media-size
      - media-source
      - media-top-margin
      - media-type
    multiple-document-jobs-supported (boolean): false
    multiple-operation-time-out (integer): 60
    natural-language-configured (naturalLanguage): en-us
    operations-supported (enum):
      - 2
      - 4
      - 5
      - 6
      - 8
      - 9
      - 10
      - 11
      - 60
    pdl-override-supported (keyword): attempted
    printer-firmware-name (nameWithoutLanguage): IPP
    printer-firmware-string-version (textWithoutLanguage): 2.0
    printer-firmware-version (OctetString): <octetString: 0x0200 (2 bytes)>
    urf-supported (keyword):
      - V1.4
      - CP1
      - PQ4-5
      - RS600
      - SRGB24
      - W8
      - OB9
      - OFU0
      - IS1
    printer-kind (keyword):
      - document
      - envelope
      - photo
    ipp-features-supported (keyword): airprint-1.4
    identify-actions-supported (keyword):
      - flash
      - sound
    identify-actions-default (keyword): flash
    print-content-optimize-supported (keyword): auto
    print-content-optimize-default (keyword): auto
    print-scaling-supported (keyword):
      - none
      - fill
      - fit
      - auto-fit
      - auto
    print-scaling-default (keyword): auto
    pwg-raster-document-resolution-supported (resolution): 600x600 dpi
    pwg-raster-document-sheet-back (keyword): rotated
    pwg-raster-document-type-supported (keyword):
      - srgb_8
      - sgray_8
    media-supported (keyword):
      - na_index-4x6_4x6in
      - na_number-10_4.125x9.5in
      - iso_dl_110x220mm
      - na_5x7_5x7in
      - iso_a5_148x210mm
      - jis_b5_182x257mm
      - na_govt-letter_8x10in
      - iso_a4_210x297mm
      - na_letter_8.5x11in
      - na_legal_8.5x14in
      - custom_min_101.6x152.4mm
      - custom_max_215.9x676mm
    media-type-supported (keyword):
      - photographic
      - stationery
      - envelope
    media-source-supported (keyword):
      - auto
      - main
    media-top-margin-supported (integer):
      - 0
      - 500
      - 800
    media-left-margin-supported (integer):
      - 0
      - 340
      - 560
      - 640
    media-right-margin-supported (integer):
      - 0
      - 340
      - 560
      - 630
    media-bottom-margin-supported (integer):
      - 0
      - 500
      - 2900
    printer-input-tray (OctetString):
      - <octetString: 0x74123456789123456789123456789123... (81 bytes)>
      - <octetString: 0x74123456789123456789123456789123... (102 bytes)>
    printer-output-tray (OctetString): <octetString: 0x74123456789123456789123456789123... (116 bytes)>
    media-default (keyword): iso_a4_210x297mm
    printer-is-accepting-jobs (boolean): true
    printer-location (textWithoutLanguage): Arbeitszimmer
    printer-geo-location (uri): geo:0.00000,0.00000,0
    printer-make-and-model (textWithoutLanguage): Canon MX490 series
    printer-info (textWithoutLanguage): Canon MX490 series
    printer-dns-sd-name (nameWithoutLanguage): Canon MX490 series
    printer-name (nameWithoutLanguage): MX490 series
    media-ready (keyword): iso_a4_210x297mm
    printer-state-reasons (keyword): none
    marker-names (nameWithoutLanguage):
      - Color
      - Black
    marker-colors (nameWithoutLanguage):
      - #39D2E7#D945DD#DFD31D
      - #101010
    marker-types (keyword):
      - inkCartridge
      - inkCartridge
    marker-high-levels (integer):
      - 100
      - 100
    marker-low-levels (integer):
      - 15
      - 15
    marker-levels (integer):
      - 100
      - 90
    printer-state (enum): 3 (idle)
    page-ranges-supported (boolean): false
    printer-device-id (textWithoutLanguage): MFG:Canon;<Anonymized>;
    printer-up-time (integer): 17519002
    printer-uri-supported (uri): ipp://192.168.12.34/ipp/print
    printer-icons (uri):
      - http://192.168.12.34/icon/printer_icon.png
      - http://192.168.12.34/icon/printer_icon_large.png
    printer-more-info (uri): http://192.168.12.34/index.html?PAGE_AAP
    printer-supply-info-uri (uri): http://192.168.12.34/index.html?PAGE_INK
```

## License

This project is licensed under the terms of the MIT license, see [LICENSE.txt](./LICENSE.txt).
