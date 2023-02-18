using System.Windows.Forms;
using Tesseract;

internal class Program
{
    [STAThreadAttribute]
    private static void Main(string[] args)
    {
        if (Clipboard.ContainsImage())
        {
            var img = (Bitmap)Clipboard.GetImage();
            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {
                using (var page = engine.Process(img))
                {
                    var text = page.GetText();
                    Console.WriteLine("Text found: {0}", text);
                }
            }
        } 
    }
}