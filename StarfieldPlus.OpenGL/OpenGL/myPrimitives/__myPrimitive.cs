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
















public class myEllipse_2 : myPrimitive
{
    private static uint ebo = 0, vbo = 0, shaderProgram = 0;
    private static float[] vertices = null;
    private static int locationColor = 0, locationCenter = 0, locationScrSize = 0, locationRadSq = 0;

    private float lineThickness = 0;

    // -------------------------------------------------------------------------------------------------------------------

    public myEllipse_2()
    {
        if (vertices == null)
        {
            vertices = new float[12];

            for (int i = 0; i < 12; i++)
                vertices[i] = 0;

            CreateProgram();
            glUseProgram(shaderProgram);
            locationColor   = glGetUniformLocation(shaderProgram, "myColor");
            locationCenter  = glGetUniformLocation(shaderProgram, "myCenter");
            locationScrSize = glGetUniformLocation(shaderProgram, "myScrSize");
            locationRadSq   = glGetUniformLocation(shaderProgram, "RadSq");

            vbo = glGenBuffer();
            ebo = glGenBuffer();

            updateIndices();
        }

        // Set default line thickness
        resetLineThickness();
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Change the thickness of the line (used only in non-filling mode)
    public void setLineThickness(float val)
    {
        lineThickness = 2.0f * (val + 1) / Width;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Get current line thickness
    public float getLineThickness()
    {
        return lineThickness;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Reset the thickness of the line to its default value
    public void resetLineThickness()
    {
        setLineThickness(1.0f);
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw(float x, float y, float w, float h, bool doFill = false)
    {
        Draw((int)x, (int)y, (int)w, (int)h, doFill);
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw(int x, int y, int w, int h, bool doFill = false)
    {
        // Draw a rectangle but use shader to hide everything except for the ellipse
        unsafe void __draw()
        {
            glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
            glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL);
        }

        // ---------------------------------------------------------------------------------------

        // Leave coordinates as they are, and recalc them in the shader
        float fx = x;
        float fy = y;
        float radx = (float)w / (float)Width;

        vertices[06] = fx;
        vertices[09] = fx;
        vertices[01] = fy;
        vertices[10] = fy;

        fx = x + w;
        vertices[0] = fx;
        vertices[3] = fx;

        fy = y + h;
        vertices[4] = fy;
        vertices[7] = fy;

        updateVertices();

        glUseProgram(shaderProgram);

        setColor(locationColor, _r, _g, _b, _a);
        glUniform2f(locationCenter, x + w / 2, y + h / 2);
        updUniformScreenSize(locationScrSize, Width, Height);

        float radSquared = radx * radx;

        if (doFill)
        {
            glUniform2f(locationRadSq, radSquared, 0);
        }
        else
        {
            // [lineThickness] needs to be tested on different resolutions. Probably need some additional adjustments.
            glUniform2f(locationRadSq, radSquared, radSquared - radx * lineThickness);
        }

        __draw();
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create a shader program
    private static void CreateProgram()
    {
        // Multiply either part of zzz in this shader by [float > 1.0f] to get an ellipse, not circle
        var vertex = myOGL.CreateShaderEx(GL_VERTEX_SHADER,
            @"layout (location = 0) in vec3 pos;
                uniform vec2 myCenter; uniform ivec2 myScrSize;
                out vec2 zzz;",

                main: @"gl_Position.x = 2.0f * pos.x / myScrSize.x - 1.0f;
                        gl_Position.y = 1.0f - 2.0f * pos.y / myScrSize.y;

                        zzz = vec2((gl_Position.x - (2.0f * myCenter.x / myScrSize.x - 1.0f)), ((gl_Position.y - (1.0f - 2.0f * myCenter.y / myScrSize.y)) * myScrSize.y / myScrSize.x));"
        );

        var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
            "in vec2 zzz; out vec4 result; uniform vec4 myColor; uniform vec2 RadSq;",

                main: @"float xySqd = zzz.x * zzz.x + zzz.y * zzz.y;
                        if (xySqd <= RadSq.x)
                        {
                            if (RadSq.y == 0.0)
                            {
                                result = myColor;
                            }
                            else
                            {
                                if (xySqd > RadSq.y)
                                {
                                    result = myColor;
                                }
                            }
                        }
                        else
                        {
                            result = vec4(0, 0, 0, 0);
                        }"
        );

        shaderProgram = glCreateProgram();

        glAttachShader(shaderProgram, vertex);
        glAttachShader(shaderProgram, fragment);

        glLinkProgram(shaderProgram);

        glDeleteShader(vertex);
        glDeleteShader(fragment);

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Move vertices data from CPU to GPU -- needs to be called each time we change the Rectangle's coordinates
    private static unsafe void updateVertices()
    {
        // Bind a buffer;
        // From now on, all the operations on this type of buffer will be performed on the buffer we just bound;
        glBindBuffer(GL_ARRAY_BUFFER, vbo);
        {
            // Copy user-defined data into the currently bound buffer:
            fixed (float* v = &vertices[0])
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_DYNAMIC_DRAW);
        }

        glVertexAttribPointer(0, 3, GL_FLOAT, false, 3 * sizeof(float), NULL);
        glEnableVertexAttribArray(0);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Move indices data from CPU to GPU -- needs to be called only once, as we have 2 different EBOs, and they are not going to change;
    // The EBO must be activated prior to drawing the shape: glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, doFill ? ebo1 : ebo2);
    private static unsafe void updateIndices()
    {
        int usage = GL_STATIC_DRAW;

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
        {
            var indicesFill = new uint[]
            {
                0, 1, 3,   // first triangle
                1, 2, 3    // second triangle
            };

            fixed (uint* i = &indicesFill[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indicesFill.Length, i, usage);

            // Unbind current buffer
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------
};
