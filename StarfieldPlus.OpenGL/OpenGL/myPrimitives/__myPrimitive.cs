﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Diagnostics;
using System.Windows.Forms;


/*
    Base classes for all drawing OpenGL primitives
*/


public class myPrimitive
{
    // ---------------------------------------------------------------------------------------

    // Predefined Static Primitives: Standard
    public static myScrDimmer    _Scr      = null;
    public static myLine        _Line      = null;
    public static myTriangle    _Triangle  = null;
    public static myRectangle   _Rectangle = null;
    public static myPentagon    _Pentagon  = null;
    public static myHexagon     _Hexagon   = null;
    public static myEllipse     _Ellipse   = null;

    // ---------------------------------------------------------------------------------------

    // Predefined Static Primitives: Instanced
    public static myTriangleInst  _TriangleInst  = null;
    public static myRectangleInst _RectangleInst = null;
    public static myPentagonInst  _PentagonInst  = null;
    public static myHexagonInst   _HexagonInst   = null;
    public static myEllipseInst   _EllipseInst   = null;
    public static myLineInst      _LineInst      = null;

    // ---------------------------------------------------------------------------------------

    // Quick Initialization of the Predefined Static Standard Primitives
    public static void init_ScrDimmer() { if (_Scr       == null) _Scr       = new myScrDimmer();   }
    public static void init_Line()      { if (_Line      == null) _Line      = new myLine();        }
    public static void init_Triangle()  { if (_Triangle  == null) _Triangle  = new myTriangle();    }
    public static void init_Rectangle() { if (_Rectangle == null) _Rectangle = new myRectangle();   }
    public static void init_Pentagon()  { if (_Pentagon  == null) _Pentagon  = new myPentagon();    }
    public static void init_Hexagon()   { if (_Hexagon   == null) _Hexagon   = new myHexagon();     }
    public static void init_Ellipse()   { if (_Ellipse   == null) _Ellipse   = new myEllipse();     }

    // ---------------------------------------------------------------------------------------

    // Quick Initialization of the Predefined Static Instanced Primitives
    public static void init_TriangleInst (int n)    { if (_TriangleInst  == null) _TriangleInst  = new myTriangleInst(n);   }
    public static void init_RectangleInst(int n)    { if (_RectangleInst == null) _RectangleInst = new myRectangleInst(n);  }
    public static void init_PentagonInst (int n)    { if (_PentagonInst  == null) _PentagonInst  = new myPentagonInst(n);   }
    public static void init_HexagonInst  (int n)    { if (_HexagonInst   == null) _HexagonInst    = new myHexagonInst(n);   }
    public static void init_EllipseInst  (int n)    { if (_EllipseInst   == null) _EllipseInst   = new myEllipseInst(n);    }
    public static void init_LineInst     (int n)    { if (_LineInst      == null) _LineInst      = new myLineInst(n);       }

    // ---------------------------------------------------------------------------------------

    private static uint static_vao = 0;
    protected static int Width = -1, Height = -1;
    protected float _r = 0, _g = 0, _b = 0, _a = 0;

    // ---------------------------------------------------------------------------------------

    protected static int sizeofFloat_x_3 = sizeof(float) * 3;

    // ---------------------------------------------------------------------------------------

    public myPrimitive()
    {
    }

    // ---------------------------------------------------------------------------------------

    // One time call to let primitives know the screen dimensions
    public static void init(int width, int height)
    {
        if (Width < 0 && Height < 0)
        {
            Width  = width;
            Height = height;

            // https://stackoverflow.com/questions/30057286/how-to-use-vbos-without-vaos-with-opengl-core-profile
            // Need at least one VAO to be able to render;
            // But as I don't really have a use for VAOs, it is possible to just create and bind one and forget about it:
            static_vao = glGenVertexArray();
            glBindVertexArray(static_vao);
        }
    }

    // ---------------------------------------------------------------------------------------

    // Common method to create shader programs
    protected static uint CreateProgram(string vertexShaderHeader, string vertexShaderMain, string fragmentShaderHeader, string fragmentShaderMain)
    {
        var vertex = myOGL.CreateShaderEx(GL_VERTEX_SHADER, vertexShaderHeader, vertexShaderMain);

        // Check for Vertex Shader Errors
        if (glGetShaderiv(vertex, GL_COMPILE_STATUS, 1)[0] == 0)
            MessageBox.Show(glGetShaderInfoLog(vertex), "", MessageBoxButtons.OK);

        var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER, fragmentShaderHeader, fragmentShaderMain);

        // Check for Fragment Shader Errors
        if (glGetShaderiv(fragment, GL_COMPILE_STATUS, 1)[0] == 0)
            MessageBox.Show(glGetShaderInfoLog(fragment), "", MessageBoxButtons.OK);

        uint program = glCreateProgram();

        glAttachShader(program, vertex);
        glAttachShader(program, fragment);

        glLinkProgram(program);

        glDetachShader(program, vertex);
        glDetachShader(program, fragment);

        glDeleteShader(vertex);
        glDeleteShader(fragment);

        return program;
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
