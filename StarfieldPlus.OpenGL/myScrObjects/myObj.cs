using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Windows.Forms;



namespace my
{
    public class myObject
    {
        public static int gl_Width, gl_Height;

        // -------------------------------------------------------------------------

        protected static Random rand = new Random((int)DateTime.Now.Ticks);
        protected static List<myObject> list = null;
        protected static myColorPicker colorPicker = null;

        // -------------------------------------------------------------------------

        protected float _a, _r, _g, _b;

        // -------------------------------------------------------------------------
#if false
        protected static Pen p = null;
        protected static SolidBrush br = null;
        protected static Graphics g = null;
        protected static Form form = null;
        protected static Font f = null;
        protected static bool isAlive = true;
#endif
        // -------------------------------------------------------------------------

        public myObject()
        {
        }

        // -------------------------------------------------------------------------

        protected virtual void generateNew()
        {
        }

        // -------------------------------------------------------------------------

        protected virtual void Move()
        {
        }

        // -------------------------------------------------------------------------

        protected virtual void Show()
        {
        }

        // -------------------------------------------------------------------------

        // Override it for every derived class to implement the logic
        protected virtual void Process(Window window)
        {
        }

        // -------------------------------------------------------------------------

        protected static void processInput(Window window)
        {
            if (Glfw.GetKey(window, GLFW.Keys.Escape) == GLFW.InputState.Press)
                Glfw.SetWindowShouldClose(window, true);
        }

        // -------------------------------------------------------------------------

        // Base Process method which calls for overriden classe's Process method
        public void Process(ScreenSaver scr)
        {
            try
            {
                Window openGL_Window;
                gl_Width  = 0;
                gl_Height = 0;

                // Set context creation hints
                myOGL.PrepareContext();

                // Create window
                {
#if RELEASE
                    gl_Width  = 0;
                    gl_Height = 0;
#else
                    if (false)
                    {
                        gl_Width  = 1920;
                        gl_Height = 1200;
                    }
#endif

                    openGL_Window = myOGL.CreateWindow(ref gl_Width, ref gl_Height, "scr.OpenGL", trueFullScreen: false);
                }

                // Set Blend mode
                if (true)
                {
                    glEnable(GL_BLEND);                                 // Enable blending
                    glBlendEquation(GL_FUNC_ADD);
                    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);  // Set blending function                    
                }

                // One time call to let all the primitives know the screen dimensions
                myPrimitive.init(gl_Width, gl_Height);

                Process(openGL_Window);

                Glfw.Terminate();
            }
            catch (System.Exception ex)
            {

                MessageBox.Show($"{this.ToString()} says:\n{ex.Message}", "Process Exception}", MessageBoxButtons.OK);
            }

            return;
        }

        // -------------------------------------------------------------------------

        protected void Log(string str)
        {
#if DEBUG
            try
            {
                using (System.IO.StreamWriter sw = System.IO.File.AppendText("zzz.log"))
                {
                    sw.WriteLine(str);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Log Exception", MessageBoxButtons.OK);
            }
#endif
        }

        // -------------------------------------------------------------------------
    };
};


#if false

                // Test
                main: @"
                        if (myAngle == 0)
                        {
                            float angle = 0.0;

                            rgbaColor = mData[1];

                            //gl_Position = vec4(pos.x * 1600 / 3840, pos.y, 1.0, 1.0);
                            gl_Position = vec4(pos.x, pos.y, 1.0, 1.0);
/*
                            gl_Position = vec4(pos.x * mData[0].z + mData[0].x, pos.y * mData[0].w + mData[0].y, 1.0, 1.0);
                            gl_Position.x -= (1.5 - mData[0].z);
                            gl_Position.y += (1.5 - mData[0].w);

                            gl_Position.x = 0;
                            gl_Position.y = 0;
*/
                            gl_Position = vec4(pos.x * mData[0].z + mData[0].x, pos.y * mData[0].w + mData[0].y, 1.0, 1.0);

                            gl_Position.x = pos.x;
                            gl_Position.y = pos.y;

                        }

                        if (myAngle == 1)
                        {
                            // backup
                            float angle = 0.25;
                            float zzz = 0.75;

                            float X = pos.x + zzz;
                            float Y = pos.y - zzz;

                            vec2 p = vec2(X * cos(angle) - Y * sin(angle), Y * cos(angle) + X * sin(angle));

                            //gl_Position = vec4(pos.x + zzz, pos.y - zzz, pos.z, 1.0);

                            gl_Position = vec4(p.x, p.y * 3840/1600, pos.z, 1.0);

                            gl_Position.x -= zzz;
                            gl_Position.y += zzz;

                            rgbaColor = mData[1];
                        }

                        if (myAngle == 2)
                        {
                            // something like rotation, but need to figure this out yet
                            float angle = 0.5;
                            float zzz = 0.75;
zzz = 0.0;
                            float X = pos.x + zzz;
                            float Y = pos.y - zzz;

                            vec2 p = vec2(X * cos(angle) - Y * sin(angle), Y * cos(angle) + X * sin(angle));

                            //gl_Position = vec4(pos.x + zzz, pos.y - zzz, pos.z, 1.0);

                            //gl_Position = vec4(p.x, p.y * 3840/1600, pos.z, 1.0);

                            //gl_Position = vec4(pos.x, pos.y, pos.z, 1.0);
                            gl_Position = vec4(pos.x * mData[0].z + mData[0].x, pos.y * mData[0].w + mData[0].y, 1.0, 1.0);
                            //gl_Position.x -= zzz;
                            //gl_Position.y += zzz;

                            gl_Position.x -= (1 - mData[0].z);
                            gl_Position.y += (1 - mData[0].w);

                            rgbaColor = mData[1];
                        }"


#endif




#if false


mostly works

                main: @"rgbaColor = mData[1];

                        if (myAngle == 123)
                        {
                            // no rotation, initialliy centered, works ok
                            float densityX = 2.0 / myScrSize.x;
                            float densityY = 2.0 / myScrSize.y;

                            float x = pos.x * mData[0].z;
                            float y = pos.y * mData[0].w;

                            // moving into position
                            x += +densityX * (mData[0].x + mData[0].z/2) - 1.0;
                            y += -densityY * (mData[0].y + mData[0].w/2) + 1.0;

                            gl_Position = vec4(x, y, 1.0, 1.0);
                        }
                        else
                        {
                            // rotation
                            float a = myAngle;

                            float densityX = +2.0 / myScrSize.x;
                            float densityY = -2.0 / myScrSize.y;

                            float zzz = 1.0 * myScrSize.x / myScrSize.y;

                            float x1 = pos.x;
                            float y1 = pos.y;

                            float x2 = x1 * cos(a) - y1 * sin(a);
                            float y2 = y1 * cos(a) + x1 * sin(a);

                            float x4 = x2 * mData[0].z;
                            float y4 = y2 * mData[0].w;

                            // moving into position
                            x4 += densityX * (mData[0].x + mData[0].z/2) - 1.0;
                            y4 += densityY * (mData[0].y + mData[0].w/2) + 1.0;

                            gl_Position = vec4(x4, y4, 1.0, 1.0);
                        }"


#endif
