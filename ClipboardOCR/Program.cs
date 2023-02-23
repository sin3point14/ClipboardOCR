using Microsoft.Win32;
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
    private int hotkeyID;
    private string baseDir;
    private void ClipboardOCR(object? sender, EventArgs e)
    {
        if (Clipboard.ContainsImage())
        {
            var img = (Bitmap)Clipboard.GetImage();
            using (var engine = new TesseractEngine(baseDir + @"\tessdata", "eng", EngineMode.Default))
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

        HotKeyManager.UnregisterHotKey(hotkeyID);

        Application.Exit();
    }

    private static bool IsStartup()
    {
        RegistryKey? rk = Registry.CurrentUser.OpenSubKey
            (@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", false);

        return rk?.GetValue("ClipboardOCR") != null;
    }
    private static void ToggleStartupState(object? sender, EventArgs e)
    {
        RegistryKey? rk = Registry.CurrentUser.OpenSubKey
            (@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
        if (rk is RegistryKey valRk && sender is ToolStripMenuItem item)
        {
            if (!IsStartup())
                valRk.SetValue("ClipboardOCR", Application.ExecutablePath);
            else
                valRk.DeleteValue("ClipboardOCR");
            item.Checked = !item.Checked;
        }
    }

    public TrayDaemon()
    {
        string? baseDirNullable = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName);
        if (baseDirNullable is string baseDir)
            this.baseDir = baseDir;
        else
        {
            MessageBox.Show("Error", "Unable to get base directory", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }

        if (!System.IO.File.Exists(this.baseDir + @"\tessdata\eng.traineddata"))
        {
            MessageBox.Show("Error: tessdata/eng.traineddata not found", "OCR Model data not found, please check your files.", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Application.Exit();
        }
        var startupItem = new ToolStripMenuItem("Launch on startup", null, ToggleStartupState, "Launch on startup");
        startupItem.Checked = IsStartup();

        hotkeyID = HotKeyManager.RegisterHotKey(Keys.V, KeyModifiers.Alt | KeyModifiers.Control);
        HotKeyManager.HotKeyPressed += ClipboardOCR;

        trayIcon = new NotifyIcon()
        {
            Icon = new NotifyIcon().Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath),
            ContextMenuStrip = new ContextMenuStrip()
            {
                Items = {
                        startupItem,
                        new ToolStripMenuItem("Exit", null, Exit, "Exit")
                }
            },
            Visible = true,
            Text = "Clipboard OCR",
            BalloonTipText = "Click to OCR the latest image in clipboard, Right click for more options"
        };
        trayIcon.Click += ClipboardOCR;
    }
}
