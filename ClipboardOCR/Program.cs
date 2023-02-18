using Tesseract;

internal class Program
{    
    [STAThreadAttribute]
    private static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new TrayDaemon());
    }
}

public class TrayDaemon : ApplicationContext
{
    private NotifyIcon trayIcon;
    private static void ClipboardOCR(object? sender, EventArgs e)
    {
        if (Clipboard.ContainsImage())
        {
            var img = (Bitmap)Clipboard.GetImage();
            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {
                using (var page = engine.Process(img))
                {
                    var text = page.GetText();
                    Clipboard.SetText(text);
                }
            }
        }
    }

    void Exit(object? sender, EventArgs e)
    {
        // Hide tray icon, otherwise it will remain shown until user mouses over it
        trayIcon.Visible = false;

        Application.Exit();
    }

    public TrayDaemon()
    {
        trayIcon = new NotifyIcon()
        {
            Icon = new NotifyIcon().Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath),
            ContextMenuStrip = new ContextMenuStrip()
            {
                Items =
                {
                    new ToolStripMenuItem("Get Text", null, ClipboardOCR, "Get Text"),
                    new ToolStripMenuItem("Exit", null, Exit, "Exit"),
                }
            },
            Visible = true
        };
    }
}
