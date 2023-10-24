using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;


/*
    This class creates and maintains a custom texture with a set of symbols printed to it;
    Additionally, it stores a map of <index, character_field_info> to access every single character position;
    These symbols then can be viewed on the screen;
    This way, we can output text in OpenGL

    https://learnopengl.com/In-Practice/Text-Rendering

    Update:
    Now this class also can work with instanced testure as well.
    For this, we have a different constructor and different Draw methods.
    Also, don't forget to access _texInst to ResetBuffer and Draw the whole thing later on.
*/


class TexText
{
    // Contains position of a single symbol within the texture
    class field
    {
        public int offset;      // x offset of the field
        public int width;       // width of the field
    };

    private myTexRectangle _tex = null;
    private myTexRectangleInst _texInst = null;
    private Dictionary<int, field> _map = null;

    private static int _scrWidth = 0, _scrHeight = 0;

    private int _texWidth = 0, _texHeight = 0;
    private int _Length = 0;

    private bool doUseCustomColor;

    private string _fontFamily = null;

    // -------------------------------------------------------------------------------------------------------------------

    // Use a random set of characters
    public TexText(int size, bool customColor, int alphabetId = -1)
    {
        System.Diagnostics.Debug.Assert(_scrWidth > 0 && _scrHeight > 0, "Screen Dimensions are not set.");

        doUseCustomColor = customColor;

        string str = "";
        
        getFont(ref _fontFamily);
        getAlphabet(ref str, alphabetId);

        _Length = str.Length;

        getFontTexture(_fontFamily, size, ref _texWidth, ref _texHeight, str);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Use a random set of characters -- Instanced mode
    public TexText(int size, bool customColor, int instancesNo, int alphabetId = -1)
    {
        System.Diagnostics.Debug.Assert(_scrWidth > 0 && _scrHeight > 0, "Screen Dimensions are not set.");

        doUseCustomColor = customColor;

        string str = "";

        getFont(ref _fontFamily);
        getAlphabet(ref str, alphabetId);

        _Length = str.Length;

        getFontTexture(_fontFamily, size, ref _texWidth, ref _texHeight, str, instancesNo);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Use a user-defined set of characters
    public TexText(int size, string text, bool customColor)
    {
        System.Diagnostics.Debug.Assert(_scrWidth > 0 && _scrHeight > 0, "Screen Dimensions are not set.");

        doUseCustomColor = customColor;

        _Length = text.Length;

        getFont(ref _fontFamily);

        getFontTexture(_fontFamily, size, ref _texWidth, ref _texHeight, text);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Set scren dimensions
    public static void setScrDimensions(int width, int height)
    {
        _scrWidth = width;
        _scrHeight = height;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public int Lengh()
    {
        return _Length;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public string FontFamily()
    {
        return _fontFamily;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public int getFieldWidth(int charIndex)
    {
        return _map[charIndex].width;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public int getFieldHeight()
    {
        return _texHeight;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw(float x, float y, int charIndex, float opacity, float angle, float sizeFactor)
    {
        Draw((int)x, (int)y, charIndex, opacity, angle, sizeFactor);
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw(float x, float y, int charIndex, float opacity, float angle, float sizeFactor, float R, float G, float B)
    {
        _tex.setColor(R, G, B);

        Draw((int)x, (int)y, charIndex, opacity, angle, sizeFactor);
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw(int x, int y, int charIndex, float opacity, float angle, float sizeFactor, float R, float G, float B)
    {
        _tex.setColor(R, G, B);

        Draw((int)x, (int)y, charIndex, opacity, angle, sizeFactor);
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw(int x, int y, int charIndex, float opacity, float angle, float sizeFactor)
    {
        _tex.setOpacity(opacity);
        _tex.setAngle(angle);

        var fld = _map[charIndex];

        // tex.Draw((int)x, (int)y, size, texHeight, offset * gl_Width / texWidth, 0, size * gl_Width / texWidth, texHeight * gl_Height / texHeight);

        _tex.Draw(x, y, (int)(fld.width * sizeFactor), (int)(_texHeight * sizeFactor),

                                fld.offset * _scrWidth / _texWidth, 0, fld.width * _scrWidth / _texWidth, _scrHeight);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Instanced mode
    public void Draw(float x, float y, int charIndex, float sizeFactor)
    {
        Draw((int)x, (int)y, charIndex, sizeFactor);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Instanced mode
    public void Draw(float x, float y, int charIndex, float sizeFactor, float R, float G, float B, float A)
    {
        Draw((int)x, (int)y, charIndex, sizeFactor, R, G, B, A);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Instanced mode
    public void Draw(int x, int y, int charIndex, float sizeFactor, float R = 1, float G = 1, float B = 1, float A = 1)
    {
        var fld = _map[charIndex];

        _texInst.setInstanceCoords(x, y, (int)(fld.width * sizeFactor), (int)(_texHeight * sizeFactor),
                        fld.offset * _scrWidth / _texWidth, 0, fld.width * _scrWidth / _texWidth, _scrHeight);

        _texInst.setInstanceColor(R, G, B, A);
        _texInst.setInstanceAngle(0);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Return random set of characters made of a random sum of defined sets
    private void getAlphabet(ref string str, int id = -1)
    {
        Random rand = new Random((int)DateTime.Now.Ticks);

        string[] arr1 = new string[] {
                "Hello World",
                "To be or not to be",
                "I have a dream",
                "Just do it",
                "May the Force be with you",
                "Houston we have a problem",
                "I'll be back",
                "The truth is out there",
                "Hasta la vista baby"
            };

        const int N = 7;
        string[] arr2 = new string[N] {
                "абвгдеёжзийклмнопрстуфхцчшщъыьэюя",
                "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ",
                "abcdefghijklmnopqrstuvwxyz",
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                "0123456789",
                "01",
                "!@#$%^&*()[]{}<>-+=/?~;:'"
            };

        if (id < 0)
        {
            if (rand.Next(2) == 0)
            {
                str = arr1[rand.Next(arr1.Length)];
            }
            else
            {
                str = "";

                // Get random number from [1 .. (2^N)-1]
                uint n = (uint)Math.Pow(2, N) - 1;

                n = (uint)rand.Next((int)n) + 1;

                for (int i = 0; i < N; i++)
                {
                    if (((uint)(1 << i) & n) != 0)
                    {
                        str += arr2[i];
                    }
                }
            }
        }
        else
        {
            str = arr2[id];
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Return random font family name
    private void getFont(ref string font)
    {
        Random rand = new Random((int)DateTime.Now.Ticks);

        switch (rand.Next(7))
        {
            case 0: font = "Tahoma";   break;
            case 1: font = "Arial";    break;
            case 2: font = "Consolas"; break;
            case 3: font = "Calibri";  break;

            default:
                using (System.Drawing.Text.InstalledFontCollection col = new System.Drawing.Text.InstalledFontCollection())
                {
                    int n = rand.Next(col.Families.Length);
                    font = col.Families[n].Name;
                }
                break;
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create a texture with symbols on it
    private void getFontTexture(string fontName, int fontSize, ref int Width, ref int Height, string str, int instancesNo = 0)
    {
        int totalWidth = 0;
        int maxHeight  = 0;

        Bitmap bmp = new Bitmap(100, 100);

        using (var gr = Graphics.FromImage(bmp))
        using (var br = new SolidBrush(Color.FromArgb(255, 255, 255, 255)))
        using (var font = new Font(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel))
        {
            gr.SmoothingMode = SmoothingMode.AntiAlias;
            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;

            for (int i = 0; i < str.Length; i++)
            {
                var size = gr.MeasureString(str[i].ToString(), font, 300, StringFormat.GenericDefault);

                totalWidth += (int)(size.Width + 0.5f);

                if (maxHeight < size.Height)
                {
                    maxHeight = (int)(size.Height + 0.5f);
                }
            }
        }

        _texHeight = maxHeight;

        _map = new Dictionary<int, field>();

        Width = totalWidth;
        Height = maxHeight;

        try
        {
            bmp = new Bitmap(totalWidth, maxHeight);
            RectangleF rect = new RectangleF(0, 0, 1, maxHeight);

            using (var gr = Graphics.FromImage(bmp))
            {
                // In case doUseCustomColor is True, we paint out alphabet in white, then set the custom color for each particle in Draw()
                int r = 255, g = 255, b = 255;

                // Otherwise, select color now, and never change it later on
                if (doUseCustomColor == false)
                {
                    Random rand = new Random((int)DateTime.Now.Ticks);

                    r = rand.Next(255);
                    g = rand.Next(255);
                    b = rand.Next(255);
                }

                using (var br = new SolidBrush(Color.FromArgb(255, r, g, b)))
                {
                    using (var font = new Font(fontName, fontSize, FontStyle.Regular, GraphicsUnit.Pixel))
                    {
                        gr.SmoothingMode = SmoothingMode.AntiAlias;
                        gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        gr.PixelOffsetMode = PixelOffsetMode.HighQuality;

                        for (int i = 0; i < str.Length; i++)
                        {
                            var size = gr.MeasureString(str[i].ToString(), font, 300, StringFormat.GenericDefault);

                            rect.Width = (int)(size.Width + 0.5f);
                            gr.DrawString(str[i].ToString(), font, br, rect);

                            field fld = new field();
                            fld.offset = (int)rect.X;
                            fld.width  = (int)rect.Width;

                            _map.Add(i, fld);

                            rect.X += rect.Width;
                        }
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            throw ex;
        }

        if (instancesNo == 0)
        {
            _tex = new myTexRectangle(bmp);
        }
        else
        {
            _texInst = new myTexRectangleInst(bmp, instancesNo);
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public myTexRectangleInst getTexInst()
    {
        return _texInst;
    }

    // -------------------------------------------------------------------------------------------------------------------
};
