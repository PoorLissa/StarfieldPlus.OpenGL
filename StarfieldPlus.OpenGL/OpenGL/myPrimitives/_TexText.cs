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
    private Dictionary<int, field> _map = null;

    private static int _scrWidth = 0, _scrHeight = 0;

    private int _texWidth = 0, _texHeight = 0;
    private int _Length = 0;

    private bool doUseCustomColor;

    private string _fontFamily = null;

    // -------------------------------------------------------------------------------------------------------------------

    // Use a random set of characters
    public TexText(int size, bool customColor)
    {
        System.Diagnostics.Debug.Assert(_scrWidth > 0 && _scrHeight > 0, "Screen Dimensions are not set.");

        doUseCustomColor = customColor;

        string str = "";
        
        getFont(ref _fontFamily);
        getAlphabet(ref str);

        _Length = str.Length;

        getFontTexture(_fontFamily, size, ref _texWidth, ref _texHeight, str);
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

    // Return random set of characters made of a random sum of defined sets
    private void getAlphabet(ref string str)
    {
        Random rand = new Random((int)DateTime.Now.Ticks);

        if (rand.Next(2) == 0)
        {
            string[] arr = new string[] {
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

            str = arr[rand.Next(arr.Length)];
        }
        else
        {
            const int N = 6;
            str = "";

            string[] arr = new string[N] {
                "абвгдеёжзийклмнопрстуфхцчшщъыьэюя",
                "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ",
                "abcdefghijklmnopqrstuvwxyz",
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                "0123456789",
                "!@#$%^&*()[]{}<>-+=/?~;:'"
            };

            // Get random number from [1 .. (2^N)-1]
            uint n = (uint)rand.Next((int)Math.Pow(2, N) - 1) + 1;

            for (int i = 0; i < N; i++)
            {
                if (((uint)(1 << i) & n) != 0)
                {
                    str += arr[i];
                }
            }
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
    private void getFontTexture(string fontName, int fontSize, ref int Width, ref int Height, string str)
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

        _tex = new myTexRectangle(bmp);

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------
};
