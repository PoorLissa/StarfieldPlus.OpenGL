using GLFW;
using static OpenGL.GL;
using System;


public class myPrimitive
{
    // ---------------------------------------------------------------------------------------

    public static Triangle      _T = null;
    public static myRectangle   _R = null;

    // ---------------------------------------------------------------------------------------

    protected static int Width = -1, Height = -1;
    protected float _r = 0, _g = 0, _b = 0, _a = 0;

    // ---------------------------------------------------------------------------------------

    // One time call to let primitives know the screen dimensions
    public static void init(int width, int height)
    {
        if (Width < 0 && Height < 0)
        {
            Width  = width;
            Height = height;
        }
    }

    // ---------------------------------------------------------------------------------------

    protected void updUniformScreenSize(int location, int w, int h)
    {
        glUniform2i(location, w, h);
    }

    // ---------------------------------------------------------------------------------------

    // Just remember the color value: no color will be set after this call
    public void SetColor(float r, float g, float b, float a)
    {
        _r = r;
        _g = g;
        _b = b;
        _a = a;
    }

    // ---------------------------------------------------------------------------------------

    // Update shader program with a color (the program must be activated via 'glUseProgram' prior to that)
    public void setColor(int location, float r, float g, float b, float a)
    {
        glUniform4f(location, r, g, b, a);
    }

    // ---------------------------------------------------------------------------------------
};
