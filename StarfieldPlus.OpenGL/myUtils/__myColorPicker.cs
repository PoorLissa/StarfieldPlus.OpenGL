using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;



namespace my
{
    public class myColorPicker
    {
        private int      _mode = -1, _rndMode = -1, _rndVariator = -1;
        private Bitmap   _img = null;
        private Random   _rand = null;
        private Graphics _g = null;
        private string   _f = null;

        private static int _W = -1, _H = -1, gl_R = -1, gl_G = -1, gl_B = -1, gl_r = -1, gl_g = -1, gl_b = -1;
        private static bool isRlocked = false, isGlocked = false, isBlocked = false;

        private enum scaleParams { scaleToWidth, scaleToHeight };
        public enum  colorMode { SNAPSHOT, IMAGE, SINGLE_RANDOM, RANDOM, TEXTURE, GRAY, SNAPSHOT_OR_IMAGE };

        // -------------------------------------------------------------------------

        public myColorPicker(int Width, int Height, myColorPicker.colorMode mode) : this(Width, Height, (int)mode)
        {
        }

        public myColorPicker(int Width, int Height, int mode = -1)
        {
            _W = Width;
            _H = Height;

            _rand = new Random((int)DateTime.Now.Ticks);
            _mode = mode;                                   // color mode
            _rndMode = _rand.Next(6);                       // random color mode
            _rndVariator = 25 + _rand.Next(75);             // random color variator

            // Select random mode
            if (_mode < 0)
            {
                _mode = _rand.Next(6);
            }

            // Select random mode out of [0, 1]
            if (_mode == (int)colorMode.SNAPSHOT_OR_IMAGE)
            {
                _mode = _rand.Next(2);
            }

            switch (_mode)
            {
                // Use Desktop Snapshot
                case 0:
                    getSnapshot(Width, Height);
                    break;

                // Use External Picture
                case 1:
                    getCustomPicture(Width, Height);
                    break;

                // Use Custom Made Texture
                case 4:
                    buildTexture(Width, Height);
                    break;

                // Use Custom Color
                default:
                    break;
            }
        }

        // -------------------------------------------------------------------------

        ~myColorPicker()
        {
            if (_g != null)
            {
                _g.Dispose();
                _g = null;
            }

            if (_img != null)
            {
                _img.Dispose();
                _img = null;
            }
        }

        // -------------------------------------------------------------------------

        public void setMode(int mode)
        {
            _mode = mode;
        }

        // -------------------------------------------------------------------------

        public int getMode()
        {
            return _mode;
        }

        // -------------------------------------------------------------------------

        public Bitmap getImg()
        {
            return _img;
        }

        // -------------------------------------------------------------------------

        public Graphics GetGraphics()
        {
            return _g;
        }

        // -------------------------------------------------------------------------

        public string GetFileName()
        {
            return _f;
        }

        // -------------------------------------------------------------------------

        public void setPixel(int x, int y, int A = 255)
        {
            if (x > -1 && y > -1 && x < _img.Width && y < _img.Height)
                _img.SetPixel(x, y, Color.FromArgb(A, 0, 0, 0));
        }

        // -------------------------------------------------------------------------

        // Get color at a point, as a brush
        public void getColor(SolidBrush br, int x, int y, int A = 255)
        {
            getColor(x, y, ref gl_r, ref gl_g, ref gl_b);
            br.Color = Color.FromArgb(A, gl_r, gl_g, gl_b);
        }

        // -------------------------------------------------------------------------

        // Get color at a point, as a pen
        public void getColor(Pen p, int x, int y, int A = 255)
        {
            getColor(x, y, ref gl_r, ref gl_g, ref gl_b);
            p.Color = Color.FromArgb(A, gl_r, gl_g, gl_b);
        }

        // -------------------------------------------------------------------------

        // Get color at a random point, as float R-G-B ([0..1]-[0..1]-[0..1])
        public void getColorRand(ref float R, ref float G, ref float B)
        {
            getColor(_rand.Next(_W), _rand.Next(_H), ref gl_r, ref gl_g, ref gl_b);

            R = gl_r / 255.0f;
            G = gl_g / 255.0f;
            B = gl_b / 255.0f;
        }

        // -------------------------------------------------------------------------

        // Get color at a random point, as float R-G-B ([0..1]-[0..1]-[0..1][0..1])
        public void getColorRand(ref float R, ref float G, ref float B, ref float A)
        {
            getColor(_rand.Next(_W), _rand.Next(_H), ref gl_r, ref gl_g, ref gl_b);

            R = gl_r / 255.0f;
            G = gl_g / 255.0f;
            B = gl_b / 255.0f;
            A = (float)_rand.NextDouble();
        }

        // -------------------------------------------------------------------------

        // Get color at a point, as float R-G-B ([0..1]-[0..1]-[0..1])
        public void getColor(float x, float y, ref float R, ref float G, ref float B)
        {
            getColor((int)x, (int)y, ref gl_r, ref gl_g, ref gl_b);

            R = gl_r / 255.0f;
            G = gl_g / 255.0f;
            B = gl_b / 255.0f;
        }

        // -------------------------------------------------------------------------

        // Get average color at a rectangle, as float R-G-B ([0..1]-[0..1]-[0..1])
        public void getColorAverage(float x, float y, int width, int height, ref float R, ref float G, ref float B)
        {
            R = G = B = 0;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    getColor((int)x + i, (int)y + j, ref gl_r, ref gl_g, ref gl_b);
                    R += gl_r / 255.0f;
                    G += gl_g / 255.0f;
                    B += gl_b / 255.0f;
                }
            }

            R /= (width * height);
            G /= (width * height);
            B /= (width * height);
        }

        // -------------------------------------------------------------------------

        // Get color at a point, as R-G-B
        public void getColor(int x, int y, ref int R, ref int G, ref int B)
        {
            switch (_mode)
            {
                // Get Color from Snapshot
                case 0:
                // Get Color from custom image
                case 1:

                    if (_img != null)
                    {
                        fixCoordinates(ref x, ref y);
                        var pixel = _img.GetPixel(x, y);

                        R = pixel.R;
                        G = pixel.G;
                        B = pixel.B;
                    }
                    break;

                // Single Random Color -- to get different shades of this color, use Alpha channel
                case 2:

                    // Run once per session
                    if (gl_R < 0 && gl_G < 0 && gl_B < 0)
                    {
                        while (gl_R + gl_G + gl_B < 500)
                        {
                            gl_R = _rand.Next(256);
                            gl_G = _rand.Next(256);
                            gl_B = _rand.Next(256);
                        }
                    }

                    R = gl_R;
                    G = gl_G;
                    B = gl_B;
                    break;

                // Random Color (of 6 different types)
                // todo: See if someting like that could be implemented: https://color.adobe.com/create/color-wheel
                case 3:
                    getRandomColor(ref R, ref G, ref B);
                    break;

                // Custom texture
                case 4:
                    if (_img != null)
                    {
                        fixCoordinates(ref x, ref y);
                        var pixel = _img.GetPixel(x, y);

                        R = pixel.R;
                        G = pixel.G;
                        B = pixel.B;
                    }
                    break;

                // Shades of Gray
                default:
                    R = _rand.Next(256);
                    G = R;
                    B = G;
                    break;
            }

            return;
        }

        // -------------------------------------------------------------------------

        // Select a random image file from a list of directories
        private string getRandomFile(System.Collections.Generic.List<string> list)
        {
            string res = "";

            int len = list.Count;
            Random r = new Random();

            foreach (int i in Enumerable.Range(0, len).OrderBy(x => r.Next()))
            {
                res = getRandomFile(list[i]);

                if (res.Length > 0)
                    break;
            }

            return res;
        }

        // -------------------------------------------------------------------------

        // Select a random image file from a certain directory
        private string getRandomFile(string dir)
        {
            string res = "";

            if (System.IO.Directory.Exists(dir))
            {
                try
                {
                    string[] files = System.IO.Directory.GetFiles(dir);

                    if (files != null)
                    {
                        string[] Extensions = { ".bmp", ".gif", ".png", ".jpg", ".jpeg" };

                        var list = new System.Collections.Generic.List<string>();

                        for (int i = 0; i < files.Length; i++)
                        {
                            var file = files[i];

                            foreach (var ext in Extensions)
                            {
                                if (file.EndsWith(ext))
                                {
                                    list.Add(file);
                                    break;
                                }
                            }
                        }

                        if (list.Count > 0)
                        {
                            int rnd = new Random((int)DateTime.Now.Ticks).Next(list.Count);
                            res = list[rnd];
                        }
                    }
                }
                catch (Exception)
                {
                    res = "";
                }
            }

            return res;
        }

        // -------------------------------------------------------------------------

        // Take a snapshot of a current desktop
        private void getSnapshot(int Width, int Height)
        {
            try
            {
                if (_img == null)
                {
                    _img = new Bitmap(Width, Height);

                    if (_g != null)
                    {
                        _g.Dispose();
                        _g = null;
                    }

                    _g = Graphics.FromImage(_img);
                    _g.CopyFromScreen(Point.Empty, Point.Empty, new Size(Width, Height));
                    _f = "[ Desktop Snapshot ]";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}", "myColorPicker", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            return;
        }

        // -------------------------------------------------------------------------

        private void getCustomPicture(int Width, int Height)
        {
            try
            {
                var list = new System.Collections.Generic.List<string>();

                list.Add(@"C:\_maxx\pix");
                list.Add(@"E:\iNet\pix");
                list.Add(@"E:\iNet\pix\wallpapers_3840x1600");

                //string image = getRandomFile(StarfieldPlus.Program._imgPath);
                string image = getRandomFile(list);

                if (image != null && image != string.Empty)
                {
                    _img = new Bitmap(image);

                    //if (_img.Width <= Width || _img.Height <= Height)
                    {
                        // Stretch the image, if its size is less than the desktop size
                        // todo: see why some of my 3840x1600 images are displayed incorrectly if not resized here

                        scaleParams param = scaleParams.scaleToWidth;

                        if (Width <= Height)
                        {
                            param = scaleParams.scaleToHeight;
                        }

                        _img = resizeImage(_img, Width, Height, param);
                    }

                    if (_g != null)
                    {
                        _g.Dispose();
                        _g = null;
                    }

                    _g = Graphics.FromImage(_img);
                    _f = image.Substring(image.LastIndexOf('\\') + 1);
                }
                else
                {
                    throw new Exception("");
                }
            }
            catch (Exception)
            {
                _img = null;
                _g = null;
                _mode = 0;
                getSnapshot(Width, Height);
            }

            return;
        }

        // -------------------------------------------------------------------------

        // High quality resize: https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp
        private Bitmap resizeImage(Image src, int width, int height, scaleParams scaleParam)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            //destImage.SetResolution(src.HorizontalResolution, src.VerticalResolution);

            float desktopRatio = (float)width / (float)height;
            float srcRatio = (float)src.Width / (float)src.Height;
            float ratioDiff = Math.Abs(desktopRatio - srcRatio);

            // todo: need to find out what max ratio value do we need here
            if (ratioDiff > 0.05f)
            {
                switch (scaleParam)
                {
                    case scaleParams.scaleToHeight:
                        destRect.Width = src.Width * height / src.Height;

                        if (destRect.Width < width)
                        {
                            destRect.X = (width - destRect.Width) / 2;
                        }
                        break;

                    case scaleParams.scaleToWidth:
                        destRect.Height = src.Height * width / src.Width;

                        if (destRect.Height > height)
                        {
                            int yOffset = destRect.Height - height;
                            yOffset = _rand.Next(yOffset);
                            destRect.Y = -yOffset;
                        }
                        break;
                }
            }

            using (var gr = Graphics.FromImage(destImage))
            {
                gr.CompositingMode    = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                gr.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                gr.InterpolationMode  = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                gr.SmoothingMode      = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                gr.PixelOffsetMode    = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new System.Drawing.Imaging.ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    gr.DrawImage(src, destRect, 0, 0, src.Width, src.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        // -------------------------------------------------------------------------

        // Get new random color and update the brush with this color instantly
        public void getNewBrush(SolidBrush br)
        {
            int alpha = 0, R = 0, G = 0, B = 0, max = 256;

            while (alpha + R + G + B < 100)
            {
                R = _rand.Next(max);
                G = _rand.Next(max);
                B = _rand.Next(max);
            }

            br.Color = Color.FromArgb(alpha, R, G, B);
        }

        // -------------------------------------------------------------------------

        // Get new random color and then gradually get closer to it with each iteration, until the color value is matched
        // Update the brush with the current color on each iteration
        public bool getNewBrush(SolidBrush br, bool doGenerate, int minVal = 100)
        {
            if (doGenerate)
            {
                gl_R = gl_G = gl_B = 0;

                while (gl_R + gl_G + gl_B < minVal)
                {
                    gl_R = _rand.Next(256);
                    gl_G = _rand.Next(256);
                    gl_B = _rand.Next(256);
                }
            }

            int r = br.Color.R;
            int g = br.Color.G;
            int b = br.Color.B;

            r += r == gl_R ? 0 : r > gl_R ? -1 : 1;
            g += g == gl_G ? 0 : g > gl_G ? -1 : 1;
            b += b == gl_B ? 0 : b > gl_B ? -1 : 1;

            br.Color = Color.FromArgb(255, r, g, b);

            return r == gl_R && g == gl_G && b == gl_B;
        }

        // -------------------------------------------------------------------------

        // Randomly set 1 bool to true, 2 to false
        private void lock1of3(ref bool val1, ref bool val2, ref bool val3)
        {
            val1 = val2 = val3 = true;

            switch (_rand.Next(3))
            {
                case 0: val2 = val3 = false; break;
                case 1: val1 = val3 = false; break;
                case 2: val1 = val2 = false; break;
            }
        }

        // -------------------------------------------------------------------------

        // Randomly set 2 bools to true, 1 to false
        private void lock2of3(ref bool val1, ref bool val2, ref bool val3)
        {
            val1 = val2 = val3 = false;

            switch (_rand.Next(3))
            {
                case 0: val2 = val3 = true; break;
                case 1: val1 = val3 = true; break;
                case 2: val1 = val2 = true; break;
            }
        }

        // -------------------------------------------------------------------------

        private void getRandomColor(ref int R, ref int G, ref int B)
        {
            switch (_rndMode)
            {
                // Family of colors: one R-G-B component is locked, two are fully random
                case 0:

                    // Run once per session
                    if (gl_R < 0 && gl_G < 0 && gl_B < 0)
                    {
                        while (gl_R + gl_G + gl_B < 150)
                        {
                            gl_R = _rand.Next(256);
                            gl_G = _rand.Next(256);
                            gl_B = _rand.Next(256);
                        }

                        lock1of3(ref isRlocked, ref isGlocked, ref isBlocked);
                    }

                    R = isRlocked ? gl_R : _rand.Next(256);
                    G = isGlocked ? gl_G : _rand.Next(256);
                    B = isBlocked ? gl_B : _rand.Next(256);
                    break;

                // Family of colors: two R-G-B components are locked, one is fully random
                case 1:

                    // Run once per session
                    if (gl_R < 0 && gl_G < 0 && gl_B < 0)
                    {
                        while (gl_R + gl_G + gl_B < 150)
                        {
                            gl_R = _rand.Next(256);
                            gl_G = _rand.Next(256);
                            gl_B = _rand.Next(256);
                        }

                        lock2of3(ref isRlocked, ref isGlocked, ref isBlocked);
                    }

                    R = isRlocked ? gl_R : _rand.Next(256);
                    G = isGlocked ? gl_G : _rand.Next(256);
                    B = isBlocked ? gl_B : _rand.Next(256);
                    break;

                // Family of colors: one R-G-B component is locked, two are narrowly random
                case 2:

                    // Run once per session
                    if (gl_R < 0 && gl_G < 0 && gl_B < 0)
                    {
                        while (gl_R + gl_G + gl_B < 150)
                        {
                            gl_R = _rand.Next(256 - _rndVariator);
                            gl_G = _rand.Next(256 - _rndVariator);
                            gl_B = _rand.Next(256 - _rndVariator);
                        }

                        lock1of3(ref isRlocked, ref isGlocked, ref isBlocked);
                    }

                    R = isRlocked ? gl_R + _rndVariator/2 : gl_R + _rand.Next(_rndVariator);
                    G = isGlocked ? gl_G + _rndVariator/2 : gl_G + _rand.Next(_rndVariator);
                    B = isBlocked ? gl_B + _rndVariator/2 : gl_B + _rand.Next(_rndVariator);
                    break;

                // Family of colors: two R-G-B components are locked, one is narrowly random
                case 3:

                    // Run once per session
                    if (gl_R < 0 && gl_G < 0 && gl_B < 0)
                    {
                        while (gl_R + gl_G + gl_B < 150)
                        {
                            gl_R = _rand.Next(256 - _rndVariator);
                            gl_G = _rand.Next(256 - _rndVariator);
                            gl_B = _rand.Next(256 - _rndVariator);
                        }

                        lock2of3(ref isRlocked, ref isGlocked, ref isBlocked);
                    }

                    R = isRlocked ? gl_R + _rndVariator/2 : gl_R + _rand.Next(_rndVariator);
                    G = isGlocked ? gl_G + _rndVariator/2 : gl_G + _rand.Next(_rndVariator);
                    B = isBlocked ? gl_B + _rndVariator/2 : gl_B + _rand.Next(_rndVariator);
                    break;

                // Narrowed Random Color:
                case 4:

                    // Run once per session
                    if (gl_R < 0 && gl_G < 0 && gl_B < 0)
                    {
                        while (gl_R + gl_G + gl_B < 150)
                        {
                            gl_R = _rand.Next(256 - _rndVariator);
                            gl_G = _rand.Next(256 - _rndVariator);
                            gl_B = _rand.Next(256 - _rndVariator);
                        }
                    }

                    R = gl_R + _rand.Next(_rndVariator);
                    G = gl_G + _rand.Next(_rndVariator);
                    B = gl_B + _rand.Next(_rndVariator);
                    break;

                // Fully Random Color
                case 5:
                    R = _rand.Next(256);
                    G = _rand.Next(256);
                    B = _rand.Next(256);
                    break;
            }
        }

        // -------------------------------------------------------------------------

        private void buildTexture(int Width, int Height)
        {
            SolidBrush br = new SolidBrush(Color.Black);

            int a, x, y, w, h, r = 0, g = 0, b = 0;

            _img = new Bitmap(Width, Height);

            if (_g != null)
            {
                _g.Dispose();
                _g = null;
            }

            _g = Graphics.FromImage(_img);

            // Black background
            _g.FillRectangle(br, 0, 0, Width, Height);

            // Add random colored rectangles
            for (int i = 0; i < Height; i += 10)
            {
                x = _rand.Next( Width) - Height / 3;
                y = _rand.Next(Height) - Height / 3;

                w = i;
                h = i;

                getRandomColor(ref r, ref g, ref b);

                a = _rand.Next(13) + 5;
                br.Color = Color.FromArgb(a, r, g, b);
                _g.FillRectangle(br, x, y, w, h);
            }
        }

        // -------------------------------------------------------------------------

        private void fixCoordinates(ref int x, ref int y)
        {
            if (x < 0)
                x = 0;

            if (y < 0)
                y = 0;

            if (x >= _img.Width)
                x = _img.Width - 1;

            if (y >= _img.Height)
                y = _img.Height - 1;

            return;
        }

        // -------------------------------------------------------------------------
    }
};
