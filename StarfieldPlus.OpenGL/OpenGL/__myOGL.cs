using GLFW;
using static OpenGL.GL;
using System.Drawing;
using System.Drawing.Imaging;



class myOGL
{
    // -------------------------------------------------------------------------------------------------------------------

    // Set some common hints for the OpenGL profile creation
    public static void PrepareContextHints()
    {
        Glfw.WindowHint(Hint.ClientApi, ClientApi.OpenGL);
        Glfw.WindowHint(Hint.ContextVersionMajor, 3);
        Glfw.WindowHint(Hint.ContextVersionMinor, 3);
        Glfw.WindowHint(Hint.OpenglProfile, Profile.Core);
        Glfw.WindowHint(Hint.Doublebuffer, true);
        Glfw.WindowHint(Hint.Decorated, false);

        // https://www.glfw.org/docs/3.3/window_guide.html
        // This should take care of the situation when the window opens in background;
        // This happens when the screensaver is started by a TaskScheduler in Win10
        // Glfw.WindowHint(Hint.Floating, true);

        //Glfw.WindowHint(Hint.Samples, 0);
        //Glfw.WindowHint(Hint.ScaleToMonitor, true);
        //Glfw.WindowHint(Hint.Floating, false);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Obtain current desktop resolution
    public static void getDesktopResolution(ref int width, ref int height)
    {
        var mode = Glfw.GetVideoMode(Glfw.PrimaryMonitor);

        width  = mode.Width;
        height = mode.Height;

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Creates and returns a handle to a GLFW window with a current OpenGL context.
    /// </summary>
    /// <param name="width">The width of the client area, in pixels.</param>
    /// <param name="height">The height of the client area, in pixels.</param>
    /// <returns>A handle to the created window.</returns>
    public static Window CreateWindow(ref int width, ref int height, string Title, byte windowMode)
    {
        // Create window, make the OpenGL context current on the thread, and import graphics functions

        Window window;

        var mode = Glfw.GetVideoMode(Glfw.PrimaryMonitor);

        switch (windowMode)
        {
            // True full screen mode
            // Sets monitor to real full screen mode (which might take a long time in some cases -- maybe, dual monitor problem?..)
            case 0:
                width  = mode.Width;
                height = mode.Height;
                window = Glfw.CreateWindow(width, height, Title, Glfw.PrimaryMonitor, Window.None);
                break;

            // Windowed full screen mode
            // Create window that fully covers the entire monitor at its current resolution
            case 1:
                width  = mode.Width;
                height = mode.Height;
                window = Glfw.CreateWindow(width, height, Title, Monitor.None, Window.None);
                break;

            // Windowed mode
            // Create window with user-defined size
            case 2:
                window = Glfw.CreateWindow(width, height, Title, Monitor.None, Window.None);

                // Center window
                var screen = Glfw.PrimaryMonitor.WorkArea;
                var x = (screen.Width  - width ) / 2;
                var y = (screen.Height - height) / 2;

                Glfw.SetWindowPosition(window, x, y);
                break;

            default:
                throw new System.Exception("Unsupported window mode");
                break;
        }

        Glfw.MakeContextCurrent(window);
        Import(Glfw.GetProcAddress);

        return window;
    }

    // -------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Creates a shader of the specified type from the given source string.
    /// </summary>
    /// <param name="type">An OpenGL enum for the shader type.</param>
    /// <param name="source">The source code of the shader.</param>
    /// <returns>The created shader. No error checking is performed for this basic example.</returns>
    public static uint CreateShader(int type, string source)
    {
        var shader = glCreateShader(type);
        glShaderSource(shader, source);
        glCompileShader(shader);
        return shader;
    }

    // -------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Creates a shader of the specified type from the given source string.
    /// </summary>
    /// <param name="type">An OpenGL enum for the shader type.</param>
    /// <param name="source">The source code of the shader.</param>
    /// <returns>The created shader. No error checking is performed for this basic example.</returns>
    public static uint CreateShaderEx(int type, string header, string main)
    {
        string src = "#version 330 core\n";

        src += header;
        src += "void main(){";
        src += main;
        src += "}";

        var shader = glCreateShader(type);
        glShaderSource(shader, src);
        glCompileShader(shader);

        return shader;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Load texture from an image file
    public static void loadTexture(uint tex, string path)
    {
        loadTexture(tex, new Bitmap(path));

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create texture from the supplied bmp image
    public static void loadTexture(uint tex, Bitmap bmp)
    {
        // All upcoming GL_TEXTURE_2D operations now have effect on this texture object
        glBindTexture(GL_TEXTURE_2D, tex);

        // Turn off MipMaps
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_BASE_LEVEL, 0);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAX_LEVEL, 0);

        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

        var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

        //glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, data.Width, data.Height, 0, GL_BGRA, GL_UNSIGNED_BYTE, data.Scan0);
        //glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, data.Width, data.Height, 0, GL_RGB, GL_UNSIGNED_BYTE, data.Scan0);

        glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA8, data.Width, data.Height, 0, GL_BGRA, GL_UNSIGNED_BYTE, data.Scan0);

        // todo: Do we need mipmaps anyway?
        //glGenerateMipmap(GL_TEXTURE_2D);

        bmp.UnlockBits(data);

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Copy rectangle area from the screen to a bmp
    // usage: var bmp = myOGL.copyScreenBuffer(x, (y + gl_Height - size), size, size);
    public static Bitmap copyScreenBuffer(int x, int y, int width, int height)
    {
        Bitmap bmp = new Bitmap(width, height);

        var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

        glReadBuffer(GL_FRONT_AND_BACK);
        glReadPixels(x, y, width, height, GL_BGRA, GL_UNSIGNED_BYTE, data.Scan0);

        bmp.UnlockBits(data);

        // Image is upside down, need to flip it:
        bmp.RotateFlip(rotateFlipType: RotateFlipType.Rotate180FlipX);

        return bmp;
    }

    // -------------------------------------------------------------------------------------------------------------------
}
