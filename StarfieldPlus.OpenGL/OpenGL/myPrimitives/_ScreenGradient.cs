﻿using System;
using System.Drawing;
using my;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static OpenGL.GL;

/*
    This class only serves one purpose: drawing a full screen color gradient
*/

public class myScreenGradient : myPrimitive
{
    private long tBegin;

    private static uint vbo = 0, ebo = 0, shaderProgram = 0;
    private static float[] vertices = null;

    private float _r1, _g1, _b1, _a1;
    private float _opacity;

    // Uniform ids:
    private static int loc_myColor1 = 0, loc_myColor2 = 0, loc_uTime = 0, loc_myOpacity = 0;

    private static int verticesLength = 12;
    private static int sizeofFloat_x_verticesLength = sizeof(float) * verticesLength;
    private static int _mode = 0;

    // -------------------------------------------------------------------------------------------------------------------

    public myScreenGradient(int mode = -1)
    {
        // Start the timer
        tBegin = DateTime.Now.Ticks;

        _mode = mode < 0
            ? new System.Random().Next(13)
            : mode;

        // Initial opacity is 1
        _opacity = 1.0f;

        if (vertices == null)
        {
            vertices = new float[verticesLength];

            shaderProgram = CreateShader();
            glUseProgram(shaderProgram);

            // Uniforms
            loc_uTime     = glGetUniformLocation(shaderProgram, "uTime");
            loc_myOpacity = glGetUniformLocation(shaderProgram, "myOpacity");
            loc_myColor1  = glGetUniformLocation(shaderProgram, "myColor1");
            loc_myColor2  = glGetUniformLocation(shaderProgram, "myColor2");

            vbo = glGenBuffer();
            ebo = glGenBuffer();

            updateIndices();

            // Need to do this only once, as it will always be drawn as (0, 0, Width, Height)
            {
                vertices[06] = -1.0f;
                vertices[09] = -1.0f;
                vertices[01] = +1.0f;
                vertices[10] = +1.0f;

                vertices[00] = +1.0f;
                vertices[03] = +1.0f;

                vertices[04] = -1.0f;
                vertices[07] = -1.0f;
            }
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void SetColor2(float r, float g, float b, float a)
    {
        _r1 = r;
        _g1 = g;
        _b1 = b;
        _a1 = a;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void SetOpacity(float a)
    {
        _opacity = a;
    }

    // -------------------------------------------------------------------------------------------------------------------

    public float GetOpacity()
    {
        return _opacity;
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Set random RGB gradient colors
    public void SetRandomColors(System.Random rand, float factor, int mode = -1)
    {
        // Set random color
        void rndColor(ref float R, ref float G, ref float B) {
            R = myUtils.randFloat(rand) * factor;
            G = myUtils.randFloat(rand) * factor;
            B = myUtils.randFloat(rand) * factor;
        }

        // Get a color and apply the same factor to all its components;
        // Should result is a different shade of the same initial color
        void applyConstFactor(float r, float g, float b, ref float R, ref float G, ref float B)
        {
            float constFactor = 0.5f + myUtils.randFloat(rand) * 0.35f;     // factor of [0.50 .. 0.85]

            if (myUtils.randomChance(rand, 1, 2))                           // factor of [1.15 .. 1.50]
                constFactor = 2.0f - constFactor;

            R = r * constFactor;
            G = g * constFactor;
            B = b * constFactor;
        }

        // -----------------------------------------------------------------------------

        // The default mode translates into 0 or 1
        mode = mode == -1 ? rand.Next(2) : mode;

        switch (mode)
        {
            // 2 random colors
            case 0:
                rndColor(ref _r, ref _g, ref _b);
                rndColor(ref _r1, ref _g1, ref _b1);
                break;

            // The 1st color is random; the 2nd color is a shade of the 1st one
            case 1:
                rndColor(ref _r, ref _g, ref _b);
                applyConstFactor(_r, _g, _b, ref _r1, ref _g1, ref _b1);
                break;

            // The 1st color is random;
            case 2:
                rndColor(ref _r, ref _g, ref _b);
                break;

            // The 2nd color is random;
            case 3:
                rndColor(ref _r1, ref _g1, ref _b1);
                break;

            // One of the colors is random
            case 4:
                {
                    if (myUtils.randomChance(rand, 1, 2))
                        rndColor(ref _r, ref _g, ref _b);
                    else
                        rndColor(ref _r1, ref _g1, ref _b1);
                }
                break;
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw()
    {
        glUseProgram(shaderProgram);

        // Update uniforms:
        {
            glUniform4f(loc_myColor1, _r, _g, _b, _a);
            glUniform4f(loc_myColor2, _r1, _g1, _b1, _a1);
            glUniform1f(loc_uTime, (float)(TimeSpan.FromTicks(DateTime.Now.Ticks - tBegin).TotalSeconds));
            glUniform1f(loc_myOpacity, _opacity);
        }

        // Move vertices data from CPU to GPU
        unsafe
        {
            // Bind a buffer;
            // From now on, all the operations on this type of buffer will be performed on the buffer we just bound;
            glBindBuffer(GL_ARRAY_BUFFER, vbo);
            {
                // Copy user-defined data into the currently bound buffer:
                fixed (float* v = &vertices[0])
                    glBufferData(GL_ARRAY_BUFFER, sizeofFloat_x_verticesLength, v, GL_STREAM_DRAW);
            }

            glVertexAttribPointer(0, 3, GL_FLOAT, false, sizeofFloat_x_3, null);
            glEnableVertexAttribArray(0);

            // Draw
            {
                glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
                glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
                glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, null);
            }
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create a shader program
    private static uint CreateShader()
    {
        string vertHead =
            "layout (location = 0) in vec3 pos;";

        string vertMain =
            "gl_Position = vec4(pos, 1.0);";

        string fragHead =
            $@" out vec4 result;
                uniform vec4 myColor1; uniform vec4 myColor2; uniform float uTime; uniform float myOpacity;
                {myShaderHelpers.Generic.noiseFunc12_v1}
                {myShaderHelpers.Generic.randFunc}
            ";

        string fragMain =
            getMainFunc();

        return CreateProgram(vertHead, vertMain, fragHead, fragMain);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Move indices data from CPU to GPU -- needs to be called only once
    // The EBO must be activated prior to drawing the shape: glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
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
        }
        glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Gradient appears to be banded (distinct color steps can be seen)
    // To avoid that, use dithering: https://shader-tutorial.dev/advanced/color-banding-dithering/
    private static string getMainFunc()
    {
        var rnd = new System.Random();

        string res = null;
        string addNoise = "mixValue *= 0.9 + noise12_v1(gl_FragCoord.xy * sin(uTime)) * 0.1;";

        switch (_mode)
        {
            // Simple vertical gradient
            case 00:
                {
                    res =
                        $@"vec2 st = vec2(0, gl_FragCoord.y * { 1.0 / Height });
                        float mixValue = distance(st, vec2(0, 1));
                        { addNoise }
                    ";
                }
                break;

            // Swaying vertical gradient
            case 01:
                {
                    res =
                        $@"vec2 st = vec2(0, gl_FragCoord.y * {1.0 / Height});
                        float val = 0.025 * (1 + sin(uTime + gl_FragCoord.x * {1.0 / (200 + rnd.Next(123))}) + cos(uTime/3) * 0.5);
                        float mixValue = distance(st, vec2(0, val));
                        {addNoise}
                    ";
                }
                break;

            // Simple horizontal gradient
            case 02:
                {
                    res =
                        $@"vec2 st = vec2(gl_FragCoord.x * { 1.0 / Width }, 0);
                        float mixValue = distance(st, vec2(1, 0));
                        { addNoise }
                    ";
                }
                break;

            // Swaying horizontal gradient
            case 03:
                {
                    res =
                        $@"vec2 st = vec2(gl_FragCoord.x * {1.0 / Width}, 0);
                        float val = 0.025 * (1 + sin(uTime + gl_FragCoord.y * {1.0 / (200 + rnd.Next(123))}) + cos(uTime/3) * 0.5);
                        float mixValue = distance(st, vec2(val, 0));
                        {addNoise}
                    ";
                }
                break;

            // Simple vertical gradient (central)
            case 04:
                {
                    res =
                        $@"vec2 st = vec2(0, gl_FragCoord.y * { 2.0 / Height });
                        float mixValue = distance(st, vec2(0.2, 0.95));
                        { addNoise }
                    ";
                }
                break;

            // Swaying vertical gradient (central)
            case 05:
                {
                    res =
                        $@"vec2 st = vec2(0, gl_FragCoord.y * {2.0 / Height});
                        float val = 0.025 * (1 + sin(uTime + gl_FragCoord.x * {1.0 / (200 + rnd.Next(123))}) + cos(uTime/3) * 0.5);
                        float mixValue = distance(st, vec2(0.1 + val, 0.95 - val));
                        {addNoise}
                    ";
                }
                break;

            // Simple horizontal gradient (central)
            case 06:
                {
                    res =
                        $@"vec2 st = vec2(gl_FragCoord.x * { 2.0 / Width }, 0);
                        float mixValue = distance(st, vec2(0.95, 0.2));
                        { addNoise }
                    ";
                }
                break;

            // Swaying horizontal gradient (central)
            case 07:
                {
                    res =
                        $@"vec2 st = vec2(gl_FragCoord.x * {2.0 / Width}, 0);
                        float val = 0.025 * (1 + sin(uTime + gl_FragCoord.y * {1.0 / (200 + rnd.Next(123))}) + cos(uTime/3) * 0.5);
                        float mixValue = distance(st, vec2(0.95 - val, 0.1 - val));
                        {addNoise}
                    ";
                }
                break;

            // Randomized gradient
            case 08:
            case 09:
            case 10:
                {
                    res =
                        $@"vec2 st = vec2(gl_FragCoord.x * { 1.0 / Width }, gl_FragCoord.y * { 1.0 / Height } );
                        float mixValue = distance(st, vec2({myUtils.randFloat(rnd)}, {myUtils.randFloat(rnd)}));
                        { addNoise }
                    ";
                }
                break;

            // Simple radial gradient 
            case 11:
                {
                    res =
                        $@"vec2 st = vec2(gl_FragCoord.x * { 2.0 / Width }, gl_FragCoord.y * { 2.0 / Height } );
                        float mixValue = distance(st, vec2(1, 1));
                        { addNoise }
                    ";
                }
                break;

            // Swaying radial gradient 
            case 12:
                {
                    res =
                        $@"vec2 st = vec2(gl_FragCoord.x * {2.0 / Width}, gl_FragCoord.y * {2.0 / Height} );
                        float val = 0.025 * (1 + sin(uTime + gl_FragCoord.y * {1.0 / (100 + rnd.Next(111))}));
                        float mixValue = distance(st, vec2(1 - val, 1 - val));
                        {addNoise}
                    ";
                }
                break;

            default:
                System.Diagnostics.Debug.Assert(false, "Unexpected scr.gradient mode.");
                break;
        }

        return res + @"vec3 color = mix(myColor1.xyz, myColor2.xyz, mixValue);
                       result = vec4(color, myOpacity);";
    }

    // -------------------------------------------------------------------------------------------------------------------
};
