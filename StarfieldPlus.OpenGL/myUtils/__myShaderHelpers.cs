using System;



namespace my
{
    public class myShaderHelpers
    {
        public class Generic
        {
            public static string rotationMatrix = "mat2 rot(float t) { float s = sin(t); float c = cos(t); return mat2(c, -s, s, c);}";

            public static string noiseFunc = "float noise(vec2 p) {return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453123);}";

            public static string randFunc = "float rand(float x) {return fract(sin(x) * 43758.5453);}";
        };

        // Signed Distance Functions
        public class SDF
        {
            // Sphere - exact
            public static string sphereSDF = $@"
                float sphereSDF(vec3 p, float s)
                {{
                    return length(p) - s;
                }}";

            // Box - exact 
            public static string boxSDF = $@"
                float boxSDF(vec3 p, vec3 b)
                {{
                    vec3 q = abs(p) - b;
                    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
                }}";

            // Round Box - exact
            public static string roundBoxSDF = $@"
                float roundBoxSDF(vec3 p, vec3 b, float r)
                {{
                    vec3 q = abs(p) - b;
                    return length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0) - r;
                }}";
        }
    };
};
