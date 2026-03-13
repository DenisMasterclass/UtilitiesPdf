using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;

namespace Pdf
{
    public static class Reader
    {
        public static StringBuilder PdfToTxt(this StringBuilder sb, string filePath)
        {
            sb = new StringBuilder();
            if (filePath != null)
            {
                using (var reader = new PdfReader(filePath))
                {
                    PdfDocument document = new PdfDocument(reader);
                    for (int pagenumber = 1; pagenumber <= document.GetNumberOfPages(); pagenumber++)
                    {
                        PdfPage pdfPage = document.GetPage(pagenumber);
                        ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                        string pageText = PdfTextExtractor.GetTextFromPage(pdfPage);
                        sb.AppendLine(pageText);

                    }
                    document.Close();
                }
                return sb;
            }
            return sb;
        }
    }
}