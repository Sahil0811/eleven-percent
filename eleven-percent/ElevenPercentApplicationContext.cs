﻿﻿﻿﻿namespace eleven_percent;

using Timer = System.Windows.Forms.Timer;
public class ElevenPercentApplicationContext : ApplicationContext
{
    private NotifyIcon _trayIcon;
    private Timer _timer;
    public ElevenPercentApplicationContext()
    {
        ContextMenuStrip contextMenuStrip = new ContextMenuStrip();

        ToolStripMenuItem startupMenuItem = new ToolStripMenuItem("Run at startup")
        {
            Checked = StartupShortcutExists,
            CheckOnClick = true
        };

        startupMenuItem.CheckStateChanged += ToggleRunAtStartup;
        contextMenuStrip.Items.Add(startupMenuItem);
        
        
        contextMenuStrip.Items.Add(new ToolStripMenuItem("Exit", null, Exit));
        int percent = GetBatteryPercent();
        _trayIcon = new NotifyIcon()
        {
            Icon = IconFromText(percent.ToString()),
            ContextMenuStrip = contextMenuStrip,
            Visible = true,
        };
        this._timer = new Timer()
        {
            Interval = 5000,
        };
        
        _timer.Tick += UpdateBatteryPercent;

        _timer.Start();
    }

    void CreateStartupShortcut()
    {
        string shortcutPath = StartupShortcutPath;

        using (StreamWriter writer = new StreamWriter(shortcutPath))
        {
            string app = Path.Combine(System.AppContext.BaseDirectory, "eleven-percent.exe");
            writer.WriteLine("[InternetShortcut]");
            writer.WriteLine("URL=file:///" + app);
            writer.WriteLine("IconIndex=0");
            string icon = app.Replace('\\', '/');
            writer.WriteLine("IconFile=" + icon);
        }
    }

    private static string StartupShortcutPath
    {
        get
        {
            string startupDir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            string shortcutPath = Path.Combine(startupDir, "ElevenPercent.url");
            return shortcutPath;
        }
    }

    private static bool StartupShortcutExists => File.Exists(StartupShortcutPath);

    private void ToggleRunAtStartup(object? sender, EventArgs e)
    {
        if(!(sender is ToolStripMenuItem menuItem))
        {
            throw new ArgumentException(nameof(sender));
        }
        
        if (StartupShortcutExists)
        {
            File.Delete(StartupShortcutPath);
        }
        else
        {
            CreateStartupShortcut();
        }
    }

    private void UpdateBatteryPercent(object? sender, EventArgs args)
    {
        Icon? oldIcon = _trayIcon.Icon;
        int percent = GetBatteryPercent();
        _trayIcon.Icon = IconFromText(percent.ToString());
        oldIcon?.Dispose();
    }

    private static int GetBatteryPercent()
    {
        PowerStatus powerStatus = SystemInformation.PowerStatus;
        int percent = (int)Math.Ceiling(powerStatus.BatteryLifePercent * 100d);
        return percent;
    }

    private static Icon IconFromText(string str)
    {
        Font fontToUse = new Font("Microsoft Sans Serif", 20, FontStyle.Regular, GraphicsUnit.Pixel);
        Brush brushToUse = new SolidBrush(Color.White);
        Bitmap bitmapText = new Bitmap(24, 24);
        Graphics g = Graphics.FromImage(bitmapText);

        nint hIcon;

        g.Clear(Color.Transparent);
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
        float xOffset;
        if (str.Length == 3)
        {
            xOffset = -4;
        }
        else if (str.Length == 2)
        {
            xOffset = 0;
        }
        else
        {
            xOffset = 4;
        }
        
        g.DrawString(str, fontToUse, brushToUse, xOffset, 0);
        hIcon = (bitmapText.GetHicon());
        Icon fromHandle = Icon.FromHandle(hIcon);
        return fromHandle;
        //DestroyIcon(hIcon.ToInt32);
    }

    void Exit(object? sender, EventArgs e)
    {
        _trayIcon.Visible = false;
        this.Dispose();
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        this._timer.Dispose();
        this._trayIcon.Dispose();
    }
}
