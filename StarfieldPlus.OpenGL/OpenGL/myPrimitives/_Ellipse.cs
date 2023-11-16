using GLFW;
using static OpenGL.GL;
using System;

/*
    - Draws an ellipse
    - For now, actually, only circle. To be able to draw an ellipse, needs some adjustments
*/

public class myEllipse : myPrimitive
{
    private static uint ebo = 0, vbo = 0, shaderProgram = 0;
    private static float[] vertices = null;
    private static int locationColor = 0, locationCenter = 0, locationScrSize = 0, locationRadSq = 0, locationSize;
    private static float ptWidth = 0, ptHeight = 0, ptWH = 0;

    private float lineThickness = 0;

    // -------------------------------------------------------------------------------------------------------------------

    public myEllipse()
    {
        if (vertices == null)
        {
            ptWidth  = 1.0f / Width;
            ptHeight = 1.0f / Height;
            ptWH = (float)Height / (float)Width;

            vertices = new float[12];

            for (int i = 0; i < 12; i++)
                vertices[i] = 0;

            shaderProgram = CreateShader();
            glUseProgram(shaderProgram);

            locationColor   = glGetUniformLocation(shaderProgram, "myColor");
            locationCenter  = glGetUniformLocation(shaderProgram, "myCenter");
            locationScrSize = glGetUniformLocation(shaderProgram, "myScrSize");
            locationRadSq   = glGetUniformLocation(shaderProgram, "RadSq");
            locationSize    = glGetUniformLocation(shaderProgram, "mySize");

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
        // Leave coordinates as they are, and recalc them in the shader
        float fx = x;
        float fy = y;
        float radx = ptWidth * w;
        float radSquared = radx * radx;

        vertices[06] = fx;
        vertices[09] = fx;
        vertices[01] = fy;
        vertices[10] = fy;

        // Use w for both width and height (for a circle they're equal, anyway)

        fx = x + w;
        vertices[00] = fx;
        vertices[03] = fx;

        fy = y + w;
        vertices[04] = fy;
        vertices[07] = fy;

        updateVertices();

        glUseProgram(shaderProgram);

        glUniform4f(locationColor, _r, _g, _b, _a);
        glUniform2f(locationCenter, x + w/2, y + w/2);
        glUniform3f(locationScrSize, ptWidth, ptHeight, ptWH);
        glUniform2i(locationSize, w, h);

        if (doFill)
        {
            glUniform3f(locationRadSq, radSquared, 0, radx);
        }
        else
        {
            // todo: [lineThickness] needs to be tested on different resolutions. Probably need some additional adjustments.
            glUniform3f(locationRadSq, radSquared, radSquared - radx * lineThickness, radx);
        }

        // Draw a rectangle quad, but use the shader to hide everything except for the ellipse shape
        unsafe
        {
            glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
            glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create a shader program
    private static uint CreateShader()
    {
        string vertHead =
            @"layout (location = 0) in vec3 pos;
                uniform vec2 myCenter; uniform vec3 myScrSize; uniform ivec2 mySize;
                out vec2 zzz;"
            ;

        // Multiply either part of zzz in this shader by [float > 1.0f] to get an ellipse, not circle
        string vertMain =
            @"  gl_Position.x = 2.0f * pos.x * myScrSize.x - 1.0f;
                gl_Position.y = 1.0f - 2.0f * pos.y * myScrSize.y;

                zzz = vec2((gl_Position.x - (2.0f * myCenter.x * myScrSize.x - 1.0f)),
                            ((gl_Position.y - (1.0f - 2.0f * myCenter.y * myScrSize.y)) * myScrSize.z));
            ";

        string fragHead =
            @"in vec2 zzz; out vec4 result;
                uniform vec4 myColor; uniform vec3 RadSq;
                float xySqd = zzz.x * zzz.x + zzz.y * zzz.y;
            ";

        string fragMain =
            @"  if (xySqd <= RadSq.x)
                {
                    if (RadSq.y == 0.0) {
                        result = myColor;
                    }
                    else {
                        if (xySqd > RadSq.y)
                            result = myColor;
                    }
                }
                else {
                    result = vec4(0, 0, 0, 0);
                }
            ";

        return CreateProgram(vertHead, vertMain, fragHead, fragMain);
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
