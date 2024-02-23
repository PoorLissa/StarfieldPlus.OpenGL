using GLFW;
using static OpenGL.GL;
using System;


/*
    This class lets the user to create custom non-full-screen shaders.

    Example:

            string fragHeader = @"
                float circle(vec2 uv, float rad) {{ return smoothstep(rad, rad - 0.005, length(uv)); }}
                float Circle(vec2 uv, float rad) {{ return 1.0 - smoothstep(0.0, 0.005, abs(rad-length(uv))); }}
            ";

            string fragShader = $@"
                vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                uv -= Pos.xy;           // Move the quad into our position
                uv *= aspect;           // Adjust quad's aspect ratio

                // Make an ellipse
                if (Pos.w != Pos.z)
                    uv *= vec2(1.0, Pos.z / Pos.w);

                float circ = circle(uv, Pos.z);

                result = vec4(myColor.xyz * circ, 0.75 * circ);     // ellipse
                //result = vec4(myColor.xyz, 0.75);                 // rectangle
            ";

            ...

            myFreeShader shader = new myFreeShader(fHeader: fragHeader, fMain: fragShader);

            ...

            shader.Draw(333, 333, 555, 333, extraOffset: 3);
*/


public class myFreeShader : myPrimitive
{
    private long tBegin;
    private uint vbo = 0, ebo = 0, shaderProgram = 0;
    private float[] vertices = null;

    // Uniform ids:
    private int u_Time, myPos, myColor;

    private static int verticesLength = 12;
    private static int sizeofFloat_x_verticesLength = sizeof(float) * verticesLength;

    // -------------------------------------------------------------------------------------------------------------------

    public myFreeShader(string fHeader = "", string fMain = "")
    {
        if (vertices == null)
        {
            vertices = new float[verticesLength];

            shaderProgram = CreateShader(fHeader, fMain);
            glUseProgram(shaderProgram);

            // Uniforms
            u_Time  = glGetUniformLocation(shaderProgram, "uTime");
            myPos   = glGetUniformLocation(shaderProgram, "myPos");
            myColor = glGetUniformLocation(shaderProgram, "myColor");

            vbo = glGenBuffer();
            ebo = glGenBuffer();

            updateIndices();

            // Start the timer
            tBegin = DateTime.Now.Ticks;
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Draw;
    // [x, y]: central point of the quad to draw;
    // [w, h]: half-width and half-height of the quad;
    // [extraOffset]: smooth images can sometimes put some pixels outside of the quad, creating visible edges.
    //                to evade that, use additional offset to increase the quad some more;
    public void Draw(int x, int y, int w, int h, int extraOffset = 0)
    {
        // Calculate in-screen coordinates of the quad;
        // Leave them in an in-screen space
        {
            vertices[06] = vertices[09] = x - w - extraOffset;
            vertices[01] = vertices[10] = y - h - extraOffset;
            vertices[00] = vertices[03] = x + w + extraOffset;
            vertices[04] = vertices[07] = y + h + extraOffset;
        }

        updateVertices();

        glUseProgram(shaderProgram);

        // Update uniforms:
        glUniform4f(myColor, _r, _g, _b, _a);
        glUniform1f(u_Time, (float)(TimeSpan.FromTicks(DateTime.Now.Ticks - tBegin).TotalSeconds));
        glUniform4f(myPos, x, y, w, h);

        // Draw
        unsafe
        {
            glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);
            glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
            glDrawElements(GL_TRIANGLES, 6, GL_UNSIGNED_INT, NULL);
        }
    }

    // -------------------------------------------------------------------------------------------------------------------

    public void Draw(float x, float y, float w, float h, int extraOffset = 0)
    {
        Draw((int)x, (int)y, (int)w, (int)h, extraOffset);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Create a shader program:
    // - Vertex shader uses the default implementation
    // - Fragment shader code must be supplied by the user
    private uint CreateShader(string fHeader, string fMain)
    {
        string vHeader, vMain;

        // Vertex Shader Program
        {
            vHeader = "layout (location=0) in vec3 pos; uniform vec4 myPos; out vec4 Pos;";

            // Recalc gl_Position coordinates in the shader;
            // Also, recalc screen coordinates of a position vector into Normalized Device Coordinates (NDC)
            vMain = $@"

                float wInv = {2.0 / Width };
                float hInv = {2.0 / Height};

                gl_Position.x = -1.0 + pos.x * wInv;
                gl_Position.y = +1.0 - pos.y * hInv;

                Pos.x = -1.0 + myPos.x * wInv;
                Pos.y = +1.0 - myPos.y * hInv;
                Pos.zw = myPos.zw * wInv;
            ";
        }

        // Fragment Shader Program
        {
            if (string.IsNullOrEmpty(fHeader))
            {
                // Default implementation
                fHeader = $@"
                          out vec4 result;
                          in vec4 Pos;
                          uniform float uTime; uniform vec4 myColor;
                          vec2 iResolution = vec2({Width}, {Height}); vec2 aspect = vec2(1.0, {1.0 * Height / Width});
                ";
            }
            else
            {
                // Extend the header with some pre-defined variables:
                fHeader = $@"
                    out vec4 result;
                    in vec4 Pos;
                    uniform float uTime; uniform vec4 myColor;
                    vec2 iResolution = vec2({Width}, {Height}); vec2 aspect = vec2(1.0, {1.0 * Height / Width});
                    {fHeader}
                ";
            }

            if (string.IsNullOrEmpty(fMain))
            {
                // Default implementation
                fMain = $@"
                    vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                    uv -= Pos.xy;
                    uv *= aspect;

                    float mask = smoothstep(0.5, 0.0, length(uv) * 1);      // change 1st param here to change size
                    mask *= 1.0 - (uv.y + 0.5);

                    float f = 10.0;
                    float newTime = uTime * 3.0;
                    float d = (uv.y + 0.5) + 1.0;

                    float final = 0.05 * sin(dot(uv, vec2(sin(newTime * 0.2), cos(newTime * 0.15))) * 10.5 * d + newTime);
                    final += 0.15 * sin(dot(uv, vec2(sin(newTime * +0.20 + 1.42), cos(newTime * +0.15 + 1.46))) * 06.5 * d * d + newTime);
                    final += 0.15 * sin(dot(uv, vec2(sin(newTime * -0.20 + 2.42), cos(newTime * -0.20 + 2.42))) * 20.5 + newTime);
                    final += 0.09 * sin(dot(uv, vec2(sin(newTime * +0.26 + 2.42), cos(newTime * +0.26 + 2.42))) * 16.5 + newTime);

                    final = final * 0.5 + 0.5;
                    final *= mask;
                    final += mask * 0.7;

                    final += smoothstep(0.5, 0.6, final);

                    result = vec4(vec3(myColor.xyz * final), final);
                ";
            }
            else
            {
                fMain = $@"
                    {fMain}
                ";
            }
        }

        return CreateProgram(vHeader, vMain, fHeader, fMain);
    }

    // -------------------------------------------------------------------------------------------------------------------

    // Move vertices data from CPU to GPU -- needs to be called each time we change the Rectangle's coordinates
    private unsafe void updateVertices()
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

    // Move indices data from CPU to GPU -- needs to be called only once
    // The EBO must be activated prior to drawing the shape: glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, ebo);
    private unsafe void updateIndices()
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
