﻿using GLFW;
using static OpenGL.GL;
using System;


// todo:
//  - need to create vertices only once -- do I?
//  - hexagons don't draw when instancing is enabled. fix hexagons and other shapes that are broken by instancing


public class myEllipseInst : myInstancedPrimitive
{
    private static float[] vertices = null;

    private static uint ebo_fill = 0, shaderProgram = 0, instVbo = 0, quadVbo = 0;
    private static int locationColor = 0, locationScrSize = 0, locationRotateMode = 0;

    // -------------------------------------------------------------------------------------------------------------------

    public myEllipseInst(int maxInstCount)
    {
        // Number of elements in [instanceArray] that define one single instance:
        // - 4 floats for Coordinates
        // - 4 floats for RGBA
        // - 1 float for Angle
        n = 8;

        if (vertices == null)
        {
            N = 0;

            vertices = new float[12];
            instanceArray = new float[maxInstCount * n];

            CreateProgram();
            glUseProgram(shaderProgram);
            locationColor      = glGetUniformLocation(shaderProgram, "myColor");
            locationScrSize    = glGetUniformLocation(shaderProgram, "myScrSize");
            locationRotateMode = glGetUniformLocation(shaderProgram, "myRttMode");

            instVbo     = glGenBuffer();
            quadVbo     = glGenBuffer();
            ebo_fill    = glGenBuffer();

            updateIndices();
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public override void Draw(bool doFill = false)
    {
        // todo: make parent method unsafe and remove this call: see if this is faster
        unsafe void __draw(bool fill)
        {
            glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo_fill);
            glDrawElementsInstanced(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL, N);
        }

        // ---------------------------------------------------------------------------------------

        // Our initial square is located at the center of coordinates: [x = -pixel/2, y = pixel/2, w = 1*pixel, h = 1*pixel];
        // It will be scaled and moved into position by the shader

        vertices[00] = +pixelX;
        vertices[01] = +pixelY;
        vertices[03] = +pixelX;
        vertices[04] = -pixelY;
        vertices[06] = -pixelX;
        vertices[07] = -pixelY;
        vertices[09] = -pixelX;
        vertices[10] = +pixelY;

        // todo: check later, if it is possible to do this only once
        updateVertices();

        glUseProgram(shaderProgram);

        setColor(locationColor, _r, _g, _b, _a);
        updUniformScreenSize(locationScrSize, Width, Height);

        __draw(doFill);
    }

    // -------------------------------------------------------------------------------------------------------------------

    private static void CreateProgram()
    {
        // mat2x4 mData is a [2 x 4] matrix of floats, where:
        // - first  4 floats are [x, y, w, h];
        // - second 4 floats are [r, g, b, a]
        var vertex = myOGL.CreateShaderEx(GL_VERTEX_SHADER,
            @"layout (location = 0) in vec3 pos;
              layout (location = 1) in mat2x4 mData;
                uniform ivec2 myScrSize;
                out vec3 zzz;
                out vec4 rgbaColor;",

                main: @"rgbaColor = mData[1];

                        gl_Position = vec4(pos.x * mData[0].z, pos.y * mData[0].z, 1.0, 1.0);

                        zzz = vec3(gl_Position.x, gl_Position.y, 0.1);

                        // Adjust for pixel density and move into final position
                        gl_Position.x += +2.0 / myScrSize.x * (mData[0].x + mData[0].z/2) - 1.0;
                        gl_Position.y += -2.0 / myScrSize.y * (mData[0].y + mData[0].z/2) + 1.0;
"
        );

        // In case opacity in myColor vec is negative, we know that we should just multiply our instance's opacity by this value (with neg.sign)
        // This is done to be able to draw all the instances the second time with changed opacity
        // For example: we want to draw set of filled-in rectangles with lower opacity, and then we want to give them borders with higher opacity
        var fragment = myOGL.CreateShaderEx(GL_FRAGMENT_SHADER,
            "in vec3 zzz; in vec4 rgbaColor; out vec4 result;",

                main: @"

                        float xySqd = zzz.x * zzz.x + zzz.y * zzz.y;

                        if (xySqd <= zzz.z * zzz.z)
                        {
                            result = rgbaColor;
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
    // -- not anymore. need to call this once now, i think
    private unsafe void updateVertices()
    {
        glBindBuffer(GL_ARRAY_BUFFER, quadVbo);
        {
            fixed (float* v = &vertices[0])
                glBufferData(GL_ARRAY_BUFFER, sizeof(float) * vertices.Length, v, GL_DYNAMIC_DRAW);

            glVertexAttribPointer(0, 3, GL_FLOAT, false, 3 * sizeof(float), NULL);
            glEnableVertexAttribArray(0);
        }
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
                glBufferData(GL_ELEMENT_ARRAY_BUFFER, sizeof(uint) * indicesFill.Length, i, usage);

            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void setInstanceCoords(float x, float y, float rad, float unused)
    {
        instanceArray[instArrayPosition + 0] = x;
        instanceArray[instArrayPosition + 1] = y;
        instanceArray[instArrayPosition + 2] = rad;
        instanceArray[instArrayPosition + 3] = unused;

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

    // Create GPU buffer out of out instances from the array
    public override unsafe void updateInstances()
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
