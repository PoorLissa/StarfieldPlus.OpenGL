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

    public static myTriangleInst  _TriangleInst  = null;
    public static myRectangleInst _RectangleInst = null;
    public static myEllipseInst   _EllipseInst   = null;

    // Quick Initialization of the Predefined Static Standard Primitives
    public static void init_Line()      { if (_Line      == null) _Line      = new myLine();        }
    public static void init_Triangle()  { if (_Triangle  == null) _Triangle  = new myTriangle();    }
    public static void init_Rectangle() { if (_Rectangle == null) _Rectangle = new myRectangle();   }
    public static void init_Pentagon()  { if (_Pentagon  == null) _Pentagon  = new myPentagon();    }
    public static void init_Hexagon()   { if (_Hexagon   == null) _Hexagon   = new myHexagon();     }
    public static void init_Ellipse()   { if (_Ellipse   == null) _Ellipse   = new myEllipse();     }

    // Quick Initialization of the Predefined Static Instanced Primitives
    public static void init_TriangleInst( int n)    { if (_TriangleInst  == null) _TriangleInst  = new myTriangleInst(n);   }
    public static void init_RectangleInst(int n)    { if (_RectangleInst == null) _RectangleInst = new myRectangleInst(n);  }
    public static void init_EllipseInst(  int n)    { if (_EllipseInst   == null) _EllipseInst   = new myEllipseInst(n);    }

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

    // Just remember the color value: no color will be set after this call
    public void SetColorA(float a)
    {
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


// =======================================================================================================================
// =======================================================================================================================
// =======================================================================================================================


/*
    Base class for all my drawing OpenGL instanced primitives
*/


// https://learnopengl.com/code_viewer_gh.php?code=src/4.advanced_opengl/10.1.instancing_quads/instancing_quads.cpp


public class myInstancedPrimitive : myPrimitive
{
    protected static float pixelX = 0, pixelY = 0;

    protected float[] instanceArray = null;

    protected int instArrayPosition = 0, N = 0, n = 0;

    public myInstancedPrimitive()
    {
        instArrayPosition = 0;
        N = 0;
        n = 0;

        pixelX = 1.0f / Width;
        pixelY = 1.0f / Height;
    }

    // ---------------------------------------------------------------------------------------

    public virtual unsafe void updateInstances() { }

    // ---------------------------------------------------------------------------------------

    public virtual void Draw(bool doFill = false) { }

    // ---------------------------------------------------------------------------------------

    // Reset the position in the buffer;
    // From now on, the buffer will be filled starting from zero again
    public void ResetBuffer()
    {
        instArrayPosition = 0;
    }

    // ---------------------------------------------------------------------------------------

    // Reallocate inner instances array, if its size is less than the new size
    public void Resize(int Size)
    {
        if (instanceArray.Length < Size * n)
        {
            instanceArray = new float[Size * n];
        }
    }

    // ---------------------------------------------------------------------------------------
};
