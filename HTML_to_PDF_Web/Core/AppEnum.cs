namespace HTML_to_PDF_Web.Core
{
    /// <summary>
    /// Document Type is used for define sections of a full document.
    /// </summary>
    public enum PdfDocumentTypeEnum
    {
        GeneralPage = 0,
        HeaderLogo = 1,
        FooterBrand = 2,
        Forwardingletter = 3,
        FooterPageNumber = 4,
        BlankPage = 5,
        IndexPage = 6,
    }
}
