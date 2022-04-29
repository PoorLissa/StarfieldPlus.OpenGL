using GLFW;
using static OpenGL.GL;
using System;


/*
    Base class for all my drawing OpenGL primitives
*/


public class myPrimitive
{
    // ---------------------------------------------------------------------------------------

    // Predefined Static Primitives
    public static myLine        _Line      = null;
    public static myTriangle    _Triangle  = null;
    public static myRectangle   _Rectangle = null;
    public static myPentagon    _Pentagon  = null;
    public static myHexagon     _Hexagon   = null;
    public static myEllipse     _Ellipse   = null;

    // Quick Initialization of the Predefined Static Primitives
    public static void init_Line()      { if (_Line      == null) _Line      = new myLine();        }
    public static void init_Triangle()  { if (_Triangle  == null) _Triangle  = new myTriangle();    }
    public static void init_Rectangle() { if (_Rectangle == null) _Rectangle = new myRectangle();   }
    public static void init_Pentagon()  { if (_Pentagon  == null) _Pentagon  = new myPentagon();    }
    public static void init_Hexagon()   { if (_Hexagon   == null) _Hexagon   = new myHexagon();     }
    public static void init_Ellipse()   { if (_Ellipse   == null) _Ellipse   = new myEllipse();     }

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
    protected void setColor(int location, float r, float g, float b, float a)
    {
        glUniform4f(location, r, g, b, a);
    }

    // ---------------------------------------------------------------------------------------
};
