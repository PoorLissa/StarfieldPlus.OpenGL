using GLFW;
using static OpenGL.GL;
using System;


// todo:
//  - triangles are larger then other shapes with the same radius


public class myTriangleInst : myInstancedPrimitive
{
    private static float[] vertices = null;

    private static uint shaderProgram = 0, instVbo = 0, triVbo = 0;
    private static int locationColor = 0, locationRotateMode = 0;

    private int rotationMode;

    // -------------------------------------------------------------------------------------------------------------------

    public myTriangleInst(int maxInstCount)
    {
        // Number of elements in [instanceArray] that define one single instance:
        // - 3 floats for Coordinates (x, y, radius of an escribed circle) + 1 float for angle
        // - 4 floats for RGBA
        n = 8;

        if (vertices == null)
        {
            N = 0;

            vertices = new float[9];
            instanceArray = new float[maxInstCount * n];

            for (int i = 0; i < 9; i++)
                vertices[i] = 0.0f;

            // Coordinates of an equilateral triangle, inscribed in a circle of raduis 1.0
            vertices[0] = +0.0f;
            vertices[1] = +1.0f;
            vertices[3] = +(float)Math.Cos(Math.PI/6);
            vertices[4] = -0.5f;
            vertices[6] = -(float)Math.Cos(Math.PI/6);
            vertices[7] = -0.5f;

            shaderProgram = CreateShader();
            glUseProgram(shaderProgram);

            locationColor      = glGetUniformLocation(shaderProgram, "myColor");
            locationRotateMode = glGetUniformLocation(shaderProgram, "myRttMode");

            instVbo = glGenBuffer();
            triVbo  = glGenBuffer();
        }

        rotationMode = 0;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public override void Draw(bool doFill = false)
    {
        updateInstances();
        updateVertices();

        glUseProgram(shaderProgram);
        setColor(locationColor, _r, _g, _b, _a);
        glUniform1i(locationRotateMode, rotationMode);

        // Draw only outline or fill the whole polygon with color
        glPolygonMode(GL_FRONT_AND_BACK, doFill ? GL_FILL : GL_LINE);
        glDrawArraysInstanced(GL_TRIANGLES, 0, 3, N);
    }

    // -------------------------------------------------------------------------------------------------------------------

    private static uint CreateShader()
    {
        string vertHead =
            @"layout (location = 0) in vec3 pos;
              layout (location = 1) in mat2x4 mData;
                uniform int myRttMode;
                out vec4 rgbaColor;
            ";

        string vertMain =
            $@"  rgbaColor = mData[1];

                float realSizeY = { 2.0 / Height } * mData[0].z;
                float realSizeX = { 2.0 / Width  } * mData[0].z;
            
                if (mData[0].w == 0)
                {{
                    gl_Position = vec4(pos.x * realSizeX, pos.y * realSizeY, 1.0, 1.0);
                }}
                else
                {{
                    vec2 sc = vec2(sin(mData[0].w), cos(mData[0].w));

                    // Rotate
                    float x = pos.x * sc.y - pos.y * sc.x;
                    float y = pos.y * sc.y + pos.x * sc.x;

                    // Additional pseudo-3d rotation:
                    switch (myRttMode)
                    {{
                        case 1: x *= sc.x; break;
                        case 2: y *= sc.y; break;
                        case 3: x *= sc.x;
                                y *= sc.y; break;
                    }}

                    gl_Position = vec4(x * realSizeX, y * realSizeY, 1.0, 1.0);
                }}

                // Adjust for pixel density and move into final position
                gl_Position.x += { +2.0 / Width  } * mData[0].x - 1.0;
                gl_Position.y += { -2.0 / Height } * mData[0].y + 1.0;
            ";

        string fragHead =
            "in vec4 rgbaColor; out vec4 result; uniform vec4 myColor;";

        string fragMain =
            @"  result = rgbaColor;

                if (myColor.w < 0)
                    result.w *= -myColor.w;
            ";

        return CreateProgram(vertHead, vertMain, fragHead, fragMain);
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceCoords(float x, float y, float rad, float angle)
    {
        instanceArray[instArrayPosition + 0] = x;
        instanceArray[instArrayPosition + 1] = y;
        instanceArray[instArrayPosition + 2] = rad;
        instanceArray[instArrayPosition + 3] = angle;

        instArrayPosition += 4;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceColor(float r, float g, float b, float a)
    {
        instanceArray[instArrayPosition + 0] = r;
        instanceArray[instArrayPosition + 1] = g;
        instanceArray[instArrayPosition + 2] = b;
        instanceArray[instArrayPosition + 3] = a;

        instArrayPosition += 4;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceColor(double r, double g, double b, double a)
    {
        instanceArray[instArrayPosition + 0] = (float)r;
        instanceArray[instArrayPosition + 1] = (float)g;
        instanceArray[instArrayPosition + 2] = (float)b;
        instanceArray[instArrayPosition + 3] = (float)a;

        instArrayPosition += 4;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setRotationMode(int mode)
    {
        rotationMode = mode;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // todo: check later, if it is possible to do this only once
    private unsafe void updateVertices()
    {
        glBindBuffer(GL_ARRAY_BUFFER, triVbo);
        {
            fixed (float* v = &vertices[0])
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_DYNAMIC_DRAW);

            glVertexAttribPointer(0, 3, GL_FLOAT, false, 3 * sizeof(float), NULL);
            glEnableVertexAttribArray(0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create GPU buffer out of out instances from the array
    protected override unsafe void updateInstances()
    {
        if (instArrayPosition > 1)
        {
            N = instArrayPosition / n;

            // Copy data to GPU:
            glBindBuffer(GL_ARRAY_BUFFER, instVbo);
            {
                fixed (float* a = &instanceArray[0])
                    glBufferData(GL_ARRAY_BUFFER, sizeof(float) * instArrayPosition, a, GL_DYNAMIC_COPY);

                glEnableVertexAttribArray(1);
                glVertexAttribPointer(1, 4, GL_FLOAT, false, n * sizeof(float), NULL);

                glEnableVertexAttribArray(2);
                glVertexAttribPointer(2, 4, GL_FLOAT, false, n * sizeof(float), new IntPtr(1 * 4 * sizeof(float)));

                // Tell OpenGL this is an instanced vertex attribute
                glVertexAttribDivisor(1, 1);
                glVertexAttribDivisor(2, 1);

                glBindBuffer(GL_ARRAY_BUFFER, 0);
            }
        }

        return;
    }

    // -------------------------------------------------------------------------------------------------------------------
};
