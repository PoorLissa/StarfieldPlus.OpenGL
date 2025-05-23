﻿using GLFW;
using static OpenGL.GL;
using System;


// Polygon with 4 sides, 4 angles and 4 vertices;
// To draw, requires 4 sets of cordinates


public class Polygon4 : myPrimitive
{
    // Vbo (Vertex Buffer Object) -- Manages memory buffer on the GPU
    // Ebo (Element Buffer Object) is a buffer that stores indices that are used to decide what vertices to draw (and in what order)

    private static uint vbo = 0, ebo_fill = 0, ebo_outline = 0, shaderProgram = 0;
    private static float[] vertices = null;
    private static float _angle;
    private static int myColor = 0, myAngle = 0, myCenter = 0, myGradient = 0;

    private static float invW = -1, invH = -1;

    private static int verticesLength = 12;
    private static int sizeofFloat_x_verticesLength = sizeof(float) * verticesLength;

    // 0 means opacity is constant
    // 1 means it is using vertical gradient
    private int _gradMode;

    // -------------------------------------------------------------------------------------------------------------------

    public Polygon4()
    {
        if (vertices == null)
        {
            invW = 2.0f / Width;
            invH = 2.0f / Height;

            vertices = new float[verticesLength];

            shaderProgram = CreateShader();
            glUseProgram(shaderProgram);

            // Uniforms
            myColor    = glGetUniformLocation(shaderProgram, "myColor");
            myAngle    = glGetUniformLocation(shaderProgram, "myAngle");
            myCenter   = glGetUniformLocation(shaderProgram, "myCenter");
            myGradient = glGetUniformLocation(shaderProgram, "myGradient");

            vbo         = glGenBuffer();
            ebo_fill    = glGenBuffer();
            ebo_outline = glGenBuffer();

            updateIndices();
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4, bool doFill = false)
    {
        Draw((int)x1, (int)y1, (int)x2, (int)y2, (int)x3, (int)y3, (int)x4, (int)y4, doFill);
    }

    // -------------------------------------------------------------------------------------------------------------------

    /*
        (x1, y1)----(x2, y2)
            |           |
            |           |
        (x3, y3)----(x4, y4)
    */
    public void Draw(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, bool doFill = false)
    {
        if (_angle == 0)
        {
            // Recalc screen coordinates into Normalized Device Coordinates (NDC)

            // x1, y1
            vertices[ 9] = -1.0f + x1 * invW;
            vertices[10] = +1.0f - y1 * invH;

            // x2, y2
            vertices[ 0] = -1.0f + x2 * invW;
            vertices[ 1] = +1.0f - y2 * invH;

            // x3, y3
            vertices[ 6] = -1.0f + x3 * invW;
            vertices[ 7] = +1.0f - y3 * invH;

            // x4, y4
            vertices[ 3] = -1.0f + x4 * invW;
            vertices[ 4] = +1.0f - y4 * invH;
        }
        else
        {
            // Leave coordinates as they are, and recalc them in the shader
/*
            float fx = x;
            float fy = y;

            vertices[06] = fx;
            vertices[09] = fx;
            vertices[01] = fy;
            vertices[10] = fy;

            fx = x + w;
            fy = y + h;

            vertices[0] = fx;
            vertices[3] = fx;
            vertices[4] = fy;
            vertices[7] = fy;
*/
        }


        updateVertices();

        glUseProgram(shaderProgram);

        glUniform4f(myColor, _r, _g, _b, _a);
        glUniform1f(myAngle, _angle);
        glUniform1i(myGradient, _gradMode);

        // Set the center of rotation
        if (_angle != 0.0f)
        {
            //glUniform2f(myCenter, x + w / 2, y + h / 2);
        }

        // Draw
        unsafe
        {
            if (doFill)
            {
                glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_fill);

                // Renders the triangles using an index buffer EBO
                glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL);
            }
            else
            {
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_outline);
                glDrawElements(GL_LINES, 8, GL_UNSIGNED_INT, NULL);
            }
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create a shader program
    private static uint CreateShader()
    {
        string vertHead =
            $@"layout (location = 0) in vec3 pos;
                uniform float myAngle; uniform vec2 myCenter;
                vec2 sc = vec2(sin(myAngle), cos(myAngle));
            ";

        string vertMain =
            $@" if (myAngle == 0)
                {{
                    gl_Position = vec4(pos, 1.0);
                }}
                else
                {{
                    vec2 pt = pos.xy - myCenter.xy;

                    gl_Position = vec4(pt.x * sc.y - pt.y * sc.x, pt.y * sc.y + pt.x * sc.x, pos.z, 1.0);
                    gl_Position.xy += myCenter.xy;

                    gl_Position.x = gl_Position.x * { 2.0 / Width  } - 1.0;
                    gl_Position.y = 1.0 - gl_Position.y * { 2.0 / Height };
                }}
            ";

        string fragHead =
            "out vec4 result; uniform vec4 myColor; uniform int myGradient;";

        string fragMain =
            $@"
                float mixValue = myColor.w;

                if (myGradient == 1)
                {{
                    vec2 st = vec2(0, gl_FragCoord.y * {1.0 / Height});
                    mixValue = 1.0 - distance(st, vec2(0, 1)) * 1.05;
                }}

                result = vec4(myColor.xyz, mixValue);
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
                glBufferData(GL_ARRAY_BUFFER, sizeofFloat_x_verticesLength, v, GL_DYNAMIC_DRAW);
        }

        glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeofFloat_x_3, NULL);
        glEnableVertexAttribArray(0);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Move indices data from CPU to GPU -- needs to be called only once, as we have 2 different EBOs, and they are not going to change;
    // The EBO must be activated prior to drawing the shape: glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, doFill ? ebo1 : ebo2);
    private static unsafe void updateIndices()
    {
        int usage = GL_STATIC_DRAW;

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_fill);
        {
            var indicesFill = new uint[]
            {
                0, 1, 3,   // first triangle
                1, 2, 3    // second triangle
            };

            fixed (uint* i = &indicesFill[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * 6, i, usage);

            // Unbind current buffer
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }

        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_outline);
        {
            var indicesOutline = new uint[]
            {
                0, 1,
                1, 2,
                2, 3,
                3, 0
            };

            fixed (uint* i = &indicesOutline[0])
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * 8, i, usage);

            // Unbind current buffer
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void SetAngle(float angle)
    {
        _angle = angle;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void SetGradient(int mode)
    {
        _gradMode = mode;
    }

    // -------------------------------------------------------------------------------------------------------------------
};
