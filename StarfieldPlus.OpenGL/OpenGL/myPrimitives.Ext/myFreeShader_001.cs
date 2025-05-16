using static OpenGL.GL;
using System;
using my;


/*
    Custom free shader class with additional uniform depth variable
*/


class myFreeShader_001 : myFreeShader
{
    private int myDepth = -123;

    public myFreeShader_001(string fHeader = "", string fMain = "") : base(fHeader, fMain)
    {
        myDepth = myDepth < 0
            ? glGetUniformLocation(shaderProgram, "myDepth")
            : myDepth;
    }

    // ---------------------------------------------------------------------------------------------------------------

    public void Draw(float x, float y, float w, float h, float depth, int extraOffset = 0)
    {
        glUseProgram(shaderProgram);
        glUniform1f(myDepth, depth);

        base.Draw(x, y, w, h, extraOffset);
    }

    // ---------------------------------------------------------------------------------------------------------------

    // Circular smooth spot
    public static void getShader_000(ref string header, ref string main)
    {
        var rand = new Random(DateTime.Now.Millisecond);

        // Smooth circle
        var circle1 = $@"
                uniform float myDepth;
                float circle(vec2 uv, float rad)
                {{
                    float len = rad - length(uv);
                    if (len > 0)
                        return smoothstep(0.0, myDepth, len);
                    return 0;
                }}
            ";

        // Smooth circle with added dynamic noise
        var circle2 = $@"
                uniform float myDepth;
                {myShaderHelpers.Generic.noiseFunc12_v1}
                float circle(vec2 uv, float rad)
                {{
                    float len = rad - length(uv);
                    if (len > 0)
                        return smoothstep(0.0, myDepth, len) * (0.8 + noise12_v1(gl_FragCoord.xy * sin(uTime * 0.0001)) * 0.2);
                    return 0;
                }}
            ";

        // Smooth circle with added static noise
        var circle3 = $@"
                uniform float myDepth;
                {myShaderHelpers.Generic.noiseFunc12_v1}
                float circle(vec2 uv, float rad)
                {{
                    float len = rad - length(uv);
                    if (len > 0)
                        return smoothstep(0.0, myDepth, len) * (0.8 + noise12_v1(gl_FragCoord.xy) * 0.2);
                    return 0;
                }}
            ";

        // Ring
        var circle4 = $@"
                uniform float myDepth;
                float circle(vec2 uv, float rad)
                {{
                    float len = rad - length(uv);
                    return 1.0 - smoothstep(0.0, myDepth, abs(len));
                }}
            ";

        switch (rand.Next(9))
        {
            case 0:
            case 1:
            case 2:
            case 3:
                header = circle1;
                break;

            case 4:
            case 5:
                header = circle2;
                break;

            case 6:
            case 7:
                header = circle3;
                break;

            case 8:
                header = circle4;
                break;
        }

        main = $@"
                vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                uv -= Pos.xy;
                uv *= aspect;

                float r = circle(uv, Pos.z);
                result = vec4(myColor.xyz, r * myColor.w);
            ";
    }

    // ---------------------------------------------------------------------------------------------------------------

    // Ink drop amoebas
    public static void getShader_001(ref string header, ref string main)
    {
        header = $@"
                uniform float myDepth;

                float hash(vec2 p) {{
                    return fract(sin(dot(p ,vec2(127.1, 311.7))) * 43758.5453);
                }}

                float noise(vec2 p) {{
                    vec2 i = floor(p);
                    vec2 f = fract(p);
    
                    // 4 corners interpolation
                    float a = hash(i);
                    float b = hash(i + vec2(1.0, 0.0));
                    float c = hash(i + vec2(0.0, 1.0));
                    float d = hash(i + vec2(1.0, 1.0));

                    vec2 u = f*f*(3.0-2.0*f); // smoothstep

                    return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
                }}

                float circle(vec2 uv, float rad)
                {{
                    float edgeSoftness = 0.02;    // Smoothing factor for the edge
                    float noiseAmount = 0.03;     // How much the edge is distorted

                    float len = length(uv);
    
                    // Sample noise based on direction, time, etc.
                    float n = noise(uv * 5.0 + uTime/2 + myDepth); // scale = detail, time = animation
    
                    float disturbedRadius = rad + (n - 0.5) * noiseAmount;

                    // Apply depth focus effect
                    edgeSoftness = myDepth;

                    // Smooth edge using smoothstep
                    return smoothstep(disturbedRadius, disturbedRadius - edgeSoftness, len);
                }}
            ";

        main = $@"
                vec2 uv = (gl_FragCoord.xy / iResolution.xy * 2.0 - 1.0);

                uv -= Pos.xy;
                uv *= aspect;

                float r = circle(uv, Pos.z);
                result = vec4(myColor.xyz, r * myColor.w);
            ";
    }

    // ---------------------------------------------------------------------------------------------------------------
}
