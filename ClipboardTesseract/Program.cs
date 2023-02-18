using System.Windows.Forms;

internal class Program
{
    [STAThreadAttribute]
    private static void Main(string[] args)
    {
        Clipboard.SetText("hello");
    }
}