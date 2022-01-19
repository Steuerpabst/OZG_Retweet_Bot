using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace OZG_Retweet_Bot
{
  [SupportedOSPlatform("windows")]
  internal class Quote2Image
  {
    #region Attributes

    [DllImport("gdi32.dll")]
    private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);

    private readonly PrivateFontCollection _fonts;

    private readonly float _maxWidthText = 910f;
    private readonly float _maxHeightText = 380f;
    private readonly float _maxWidthAuthor = 530f;
    private readonly float _maxHeightAuthor = 115f;

    private string _text;
    private string _author;

    private int _fontSize;

    private readonly Tuple<SolidBrush, SolidBrush>[] _brushes =
    {
      // Sunday
      new Tuple<SolidBrush, SolidBrush>(new SolidBrush(Color.FromArgb(95, 95, 95)), new SolidBrush(Color.FromArgb(126, 126, 126))),
      // Monday
      new Tuple<SolidBrush, SolidBrush>(new SolidBrush(Color.FromArgb(255, 196, 19)), new SolidBrush(Color.FromArgb(255, 154, 19))),
      // Tuesday
      new Tuple<SolidBrush, SolidBrush>(new SolidBrush(Color.FromArgb(11, 117, 132)), new SolidBrush(Color.FromArgb(2, 129, 24))),
      // Wednesday
      new Tuple<SolidBrush, SolidBrush>(new SolidBrush(Color.FromArgb(158, 40, 167)), new SolidBrush(Color.FromArgb(101, 52, 173))),
      // Thursday
      new Tuple<SolidBrush, SolidBrush>(new SolidBrush(Color.FromArgb(147, 234, 48)), new SolidBrush(Color.FromArgb(226, 250, 51))),
      // Friday
      new Tuple<SolidBrush, SolidBrush>(new SolidBrush(Color.FromArgb(255, 198, 149)), new SolidBrush(Color.FromArgb(222, 100, 97))),
      // Saturday
      new Tuple<SolidBrush, SolidBrush>(new SolidBrush(Color.FromArgb(255, 170, 0)), new SolidBrush(Color.FromArgb(255, 116, 0)))
    };

    private Point _innerTemplate = new(60, 60);

    //private static readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

    #endregion

    #region Constructor

    public Quote2Image(string text, string author)
    {
      _fonts = new PrivateFontCollection();
      _text = text;
      _author = author;
      _fontSize = 44;
    }

    #endregion

    #region Properties

    public string Text
    {
      get { return _text; }
      set { _text = value; }
    }

    public string Author
    {
      get { return _author; }
      set { _author = value; }
    }

    #endregion

    #region Methods

    // Thanks to: knighter (https://stackoverflow.com/a/23519499)
    private FontFamily TextFontFamily
    {
      get
      {
        byte[] fontData = Properties.Resources.Caveat_SemiBold;

        IntPtr fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
        Marshal.Copy(fontData, 0, fontPtr, fontData.Length);

        uint dummy = 0;

        _fonts.AddMemoryFont(fontPtr, Properties.Resources.Caveat_SemiBold.Length);
        AddFontMemResourceEx(fontPtr, (uint)Properties.Resources.Caveat_SemiBold.Length, IntPtr.Zero, ref dummy);

        Marshal.FreeCoTaskMem(fontPtr);

        return _fonts.Families[0];
      }
    }

    // Thanks to: Gary Kindel (https://stackoverflow.com/a/15449620)
    private static Bitmap Template(string dayOfWeek)
    {
      Assembly assembly = Assembly.GetExecutingAssembly();

      string resourceName = assembly.GetName().Name + ".Properties.Resources";

      ResourceManager resManager = new(resourceName, assembly);

      return (Bitmap)resManager.GetObject(dayOfWeek)!;
    }

    private static StringFormat Format(string value)
    {
      StringFormat strFormat = new()
      {
        Trimming = StringTrimming.Word
      };

      if (value == "text")
      {
        strFormat.Alignment = StringAlignment.Center;
      }
      else
      {
        strFormat.Alignment = StringAlignment.Near;
      }

      return strFormat;
    }

    // Thanks for inspiration to: VR Speedruns (https://github.com/VRSpeedruns/Twitter/blob/master/Twitter/Entities/Run.cs)
    public byte[]? DrawToImage(DayOfWeek dayOfWeek)
    {
      int dayNumber = (int)dayOfWeek;

      Image dummyImage = new Bitmap(1, 1);

      Graphics textGraphics = Graphics.FromImage(dummyImage);

      SizeF textSize = textGraphics.MeasureString(Text, new Font(TextFontFamily, _fontSize, FontStyle.Regular), (int)_maxWidthText);

      while (textSize.Height > _maxHeightText)
      {
        textSize = textGraphics.MeasureString(Text, new Font(TextFontFamily, --_fontSize, FontStyle.Regular), (int)_maxWidthText);
      }

      dummyImage.Dispose();
      textGraphics.Dispose();

      float textXCorner = (_maxWidthText - textSize.Width) / 2 + _innerTemplate.X;
      float textYCorner = (_maxHeightText - textSize.Height) / 2 + _innerTemplate.Y;
      float authorYCorner = textYCorner + textSize.Height + 75;

      Bitmap template = Template(dayOfWeek.ToString());

      using(Graphics graphics = Graphics.FromImage(template))
      {
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        graphics.DrawString(Text, new Font(TextFontFamily, _fontSize, FontStyle.Regular), _brushes[dayNumber].Item1, new RectangleF(textXCorner, textYCorner, textSize.Width, textSize.Height), Format("text"));
        graphics.DrawString(Author, new Font("Brush Script MT", 36, FontStyle.Italic), _brushes[dayNumber].Item2, new RectangleF(575, authorYCorner, _maxWidthAuthor, _maxHeightAuthor), Format("author"));

        graphics.Save();

        graphics.Dispose();
      }

      MemoryStream mStream = new();

      try
      {
        template.Save(mStream, ImageFormat.Png);
        //template.Save(_baseDirectory + "\\" + value + "_tweet.png", ImageFormat.Png);

        template.Dispose();

        return mStream.ToArray();
      }
      catch (Exception ex)
      {
        LogWriter.WriteToLog(ex, Log.LogLevel.ERROR);
        return null;
      }
    }

    #endregion
  }
}
