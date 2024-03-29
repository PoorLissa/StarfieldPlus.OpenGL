﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.ExceptionServices;
using System.Text;
using my;


/*
    This class creates and maintains a custom texture with a set of symbols printed to it;
    Additionally, it stores a map of <index, character_field_info> to access every single character position;
    These symbols then can be viewed on the screen;
    This way, we can output text in OpenGL

    https://learnopengl.com/In-Practice/Text-Rendering

    Update:
    Now this class also can work with instanced texture as well.
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
    private Dictionary<int, field> _map_index = null;
    private Dictionary<char, field> _map_char = null;

    private static int _scrWidth = 0, _scrHeight = 0;
    private static float _scrToTexRatio = 0;

    private int _texWidth = 0, _texHeight = 0;
    private int _Length = 0;

    private bool doUseCustomColor;

    private string _fontFamily = null;

    // -------------------------------------------------------------------------------------------------------------------

    // Use a random set of characters
    public TexText(int size, bool customColor, int fontStyle, int alphabetId = -1)
    {
        System.Diagnostics.Debug.Assert(_scrWidth > 0 && _scrHeight > 0, "Screen Dimensions are not set.");

        doUseCustomColor = customColor;

        string str = "";
        
        getFont(ref _fontFamily);
        getAlphabet(ref str, alphabetId);

        _Length = str.Length;

        getFontTexture(_fontFamily, size, ref _texWidth, ref _texHeight, fontStyle, str);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Use a random set of characters -- Instanced mode
    public TexText(int size, bool customColor, int instancesNo, int fontStyle, int alphabetId = -1, string fontFamily = "")
    {
        System.Diagnostics.Debug.Assert(_scrWidth > 0 && _scrHeight > 0, "Screen Dimensions are not set.");

        doUseCustomColor = customColor;

        string alphabet = "";

        if (fontFamily.Length > 0)
        {
            _fontFamily = fontFamily;
        }
        else
        {
            getFont(ref _fontFamily);
        }

        getAlphabet(ref alphabet , alphabetId);

        _Length = alphabet.Length;

        getFontTexture(_fontFamily, size, ref _texWidth, ref _texHeight, fontStyle, alphabet, instancesNo);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Use a user-defined set of characters
    public TexText(int size, int fontStyle, string customAlphabet, bool customColor)
    {
        System.Diagnostics.Debug.Assert(_scrWidth > 0 && _scrHeight > 0, "Screen Dimensions are not set.");

        doUseCustomColor = customColor;

        _Length = customAlphabet.Length;

        getFont(ref _fontFamily);

        getFontTexture(_fontFamily, size, ref _texWidth, ref _texHeight, fontStyle, customAlphabet);
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
        return _map_index[charIndex].width;
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

    // Simple non-instanced mode
    public void Draw(int x, int y, int charIndex, float opacity, float angle, float sizeFactor, float R, float G, float B)
    {
        _tex.setColor(R, G, B);

        Draw((int)x, (int)y, charIndex, opacity, angle, sizeFactor);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Simple non-instanced mode
    public void Draw(int x, int y, int charIndex, float opacity, float angle, float sizeFactor)
    {
        _tex.setOpacity(opacity);
        _tex.setAngle(angle);

        var fld = _map_index[charIndex];

        // tex.Draw((int)x, (int)y, size, texHeight, offset * gl_Width / texWidth, 0, size * gl_Width / texWidth, texHeight * gl_Height / texHeight);

        _tex.Draw(x, y, (int)(fld.width * sizeFactor), (int)(_texHeight * sizeFactor),

                                fld.offset * _scrWidth / _texWidth, 0, fld.width * _scrWidth / _texWidth, _scrHeight);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Instanced mode, index-based
    public void Draw(float x, float y, int charIndex, float sizeFactor)
    {
        Draw((int)x, (int)y, charIndex, sizeFactor);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Instanced mode, char-based
    public void Draw(float x, float y, char ch, float sizeFactor)
    {
        Draw((int)x, (int)y, ch, sizeFactor);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Instanced mode, index-based
    public void Draw(float x, float y, int charIndex, float sizeFactor, float R, float G, float B, float A)
    {
        Draw((int)x, (int)y, charIndex, sizeFactor, R, G, B, A);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Instanced mode, char-based
    public void Draw(float x, float y, char ch, float sizeFactor, float R, float G, float B, float A)
    {
        Draw((int)x, (int)y, ch, sizeFactor, R, G, B, A);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Instanced mode, index-based
    public void Draw(int x, int y, int charIndex, float sizeFactor, float R = 1, float G = 1, float B = 1, float A = 1)
    {
        var fld = _map_index[charIndex];
/*
        _texInst.setInstanceCoords(x, y, (int)(fld.width * sizeFactor), (int)(_texHeight * sizeFactor),
            fld.offset * _scrWidth / _texWidth, 0, fld.width * _scrWidth / _texWidth, _scrHeight);*/

        // Slightly optimized call with less calculations
        _texInst.setInstanceCoords(x, y, (int)(fld.width * sizeFactor), (int)(_texHeight * sizeFactor),
                                                fld.offset * _scrToTexRatio, 0, fld.width * _scrToTexRatio, _scrHeight);

        _texInst.setInstanceColor(R, G, B, A);
        _texInst.setInstanceAngle(0);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Instanced mode, char-based
    public void Draw(int x, int y, char ch, float sizeFactor, float R = 1, float G = 1, float B = 1, float A = 1)
    {
        if (_map_char.ContainsKey(ch))
        {
            var fld = _map_char[ch];

            // Slightly optimized call with less calculations
            _texInst.setInstanceCoords(x, y, (int)(fld.width * sizeFactor), (int)(_texHeight * sizeFactor),
                                                    fld.offset * _scrToTexRatio, 0, fld.width * _scrToTexRatio, _scrHeight);

            _texInst.setInstanceColor(R, G, B, A);
            _texInst.setInstanceAngle(0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Get a string consisting of random elements of the array
    private void getRandomArrayStrings(Random rand, int N, ref string str, string[] arr)
    {
        var bld = new StringBuilder();

        // Get the total number of permutations (the range of [1 .. (2^N)-1])
        uint nTotal = (uint)Math.Pow(2, N) - 1;

        // Get a random permutation
        uint n = (uint)rand.Next((int)nTotal) + 1;

        // Get array elements corresponding to the selected permutation
        for (int i = 0; i < N; i++)
        {
            if (((uint)(1 << i) & n) != 0)
            {
                bld.Append(arr[i]);
            }
        }

        str = bld.ToString();
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Get a random length string consisting of random Unicode characters
    private void getRandomUnicodeString(Random rand, ref string str)
    {
        var bld = new StringBuilder();
        int len = 33 + rand.Next(22);

        for (int i = 0; i < len; i++)
            bld.Append((char)rand.Next(0xFFFF));

        str = bld.ToString();
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Return random set of characters made of a random sum of defined sets
    private void getAlphabet(ref string str, int id = -1)
    {
        Random rand = new Random((int)DateTime.Now.Ticks);

        string[] arr1 = new string[] {
                "HelloWorld",
                "Tobeornottobe",
                "Ihaveadream",
                "Justdoit",
                "MaytheForcebewithyou",
                "Houstonwehaveaproblem",
                "I'llbeback",
                "Thetruthisoutthere",
                "Hastalavistababy"
            };

        const int N = 8;
        string[] arr2 = new string[N] {
                "абвгдеёжзийклмнопрстуфхцчшщъыьэюя",
                "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ",
                "abcdefghijklmnopqrstuvwxyz",
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                "0123456789",
                "01",
                "!@#$%^&*()[]{}<>-+=/?~;:'",
                "極珠輸雄熊清兵強日大年中会人本月長国出上十生子分東三行同今高金時手見市力米自前円合立内二事社者地京間田体学下目五後新明方部八心四民対主正代言九小思七山実入回場野開万全定家北六問話文動度県水安氏和政保表道相意発不党"
            };

        if (id >= 0)
        {
            // One of the predefined alphabets (user selected)
            str = arr2[id];
        }
        else
        {
            switch (rand.Next(6))
            {
                // One of the predefined phrases
                case 0:
                    str = arr1[rand.Next(arr1.Length)];
                    break;

                // One of the predefined alphabets
                case 1:
                    str = arr2[rand.Next(arr2.Length)];
                    break;

                // Japanese Kanji
                case 2:
                    str = arr2[7];
                    break;

                // One or more random alphabets from arr2 (excluding Kanji)
                case 3:
                    getRandomArrayStrings(rand, N - 1, ref str, arr2);
                    break;

                // One or more random alphabets from arr2 (including Kanji)
                case 4:
                    getRandomArrayStrings(rand, N, ref str, arr2);
                    break;

                // Random set of Unicode characters
                case 5:
                    getRandomUnicodeString(rand, ref str);
                    break;
            }
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Return random font family name
    private void getFont(ref string font)
    {
        Random rand = new Random((int)DateTime.Now.Ticks);

        switch (rand.Next(8))
        {
            case 0: font = "Tahoma";    break;
            case 1: font = "Arial";     break;
            case 2: font = "Calibri";   break;
            case 3: font = "Calibri";   break;
            case 4: font = "Agency FB"; break;

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

    // Create a texture with all the characters printed upon it
    private void getFontTexture(string fontName, int fontSize, ref int Width, ref int Height, int fontStyle, string str, int instancesNo = 0)
    {
        int totalWidth = 0;
        int maxHeight  = 0;

        Bitmap bmp = new Bitmap(100, 100);

        var thisFontStyle = FontStyle.Regular;

        switch (fontStyle)
        {
            case 1:
                thisFontStyle = FontStyle.Bold;
                break;

            case 2:
                thisFontStyle = FontStyle.Italic;
                break;
        }

        using (var gr = Graphics.FromImage(bmp))
        using (var br = new SolidBrush(Color.FromArgb(255, 255, 255, 255)))
        using (var font = new Font(fontName, fontSize, thisFontStyle, GraphicsUnit.Pixel))
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

        _map_index = new Dictionary<int, field>();
        _map_char  = new Dictionary<char, field>();

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

                // todo: How do I antialize the fonts when rotated?
                if (false)
                {
                    var rand = new Random((int)DateTime.Now.Ticks);
                    var br = new SolidBrush(Color.FromArgb(1, r, g, b));

                    for (int i = 0; i < 1000; i++)
                    {
                        gr.FillRectangle(br, rand.Next(totalWidth), rand.Next(maxHeight), 2, 2);
                    }
                }

                using (var br = new SolidBrush(Color.FromArgb(255, r, g, b)))
                {
                    using (var font = new Font(fontName, fontSize, thisFontStyle, GraphicsUnit.Pixel))
                    {
                        myUtils.SetAntializingMode(true);

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

                            // Store fld index-based
                            _map_index.Add(i, fld);

                            // Store the same fld char-based
                            if (!_map_char.ContainsKey(str[i]))
                                _map_char.Add(str[i], fld);

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

        _scrToTexRatio = 1.0f * _scrWidth / _texWidth;

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public myTexRectangleInst getTexInst()
    {
        return _texInst;
    }

    // -------------------------------------------------------------------------------------------------------------------
};
