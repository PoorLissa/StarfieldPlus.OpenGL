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
                {
                    glEnable(GL_BLEND);                                 // Enable blending
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
