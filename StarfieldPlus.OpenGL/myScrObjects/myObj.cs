using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Windows.Forms;


#pragma warning disable CS0162


namespace my
{
    public class myObject
    {
        public static int gl_Width, gl_Height, renderDelay = 25;

        // -------------------------------------------------------------------------

        protected static Random rand = new Random((int)DateTime.Now.Ticks);
        protected static List<myObject> list = null;
        protected static myColorPicker colorPicker = null;

        // -------------------------------------------------------------------------

        protected float _a, _r, _g, _b;

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

        protected virtual string CollectCurrentInfo()
        {
            return string.Empty;
        }

        // -------------------------------------------------------------------------

        // Override it for every derived class to implement the logic
        protected virtual void Process(Window window)
        {
        }

        // -------------------------------------------------------------------------

        protected void processInput(Window window)
        {
            // Exit
            if (Glfw.GetKey(window, GLFW.Keys.Escape) == GLFW.InputState.Press)
            {
                Glfw.SetWindowShouldClose(window, true);
            }

            // Show some info
            if (Glfw.GetKey(window, GLFW.Keys.Tab) == GLFW.InputState.Press)
            {
                var form = new Form();
                var rich = new RichTextBox();

                form.Width = 500;
                form.Height = 500;
                form.StartPosition = FormStartPosition.CenterScreen;
                form.TopMost = true;
                form.Opacity = 50;
                form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                form.BackColor = System.Drawing.Color.Black;

                rich.BackColor = System.Drawing.Color.Black;
                rich.ForeColor = System.Drawing.Color.Red;

                rich.Font = new System.Drawing.Font("Helvetica Condensed", 11, System.Drawing.FontStyle.Regular, rich.Font.Unit, rich.Font.GdiCharSet);

                rich.Dock = DockStyle.Fill;
                rich.AppendText("\n");
                rich.AppendText(CollectCurrentInfo());
                rich.AppendText("\n");
                rich.SelectAll();
                rich.SelectionAlignment = HorizontalAlignment.Center;
                rich.DeselectAll();
                rich.Select(rich.TextLength, 0);
                rich.ReadOnly = true;
                form.Controls.Add(rich);

                rich.PreviewKeyDown += (object sender, PreviewKeyDownEventArgs e) => {
                    if (e.KeyCode == System.Windows.Forms.Keys.Escape || e.KeyCode == System.Windows.Forms.Keys.Tab)
                        form.Close();
                };

                form.PreviewKeyDown += (object sender, PreviewKeyDownEventArgs e) => {
                    if (e.KeyCode == System.Windows.Forms.Keys.Escape || e.KeyCode == System.Windows.Forms.Keys.Tab)
                        form.Close();
                };

                form.ShowDialog();
                //MessageBox.Show(CollectCurrentInfo(), "Current info", MessageBoxButtons.OK);
            }

            // Decrease speed
            if (Glfw.GetKey(window, GLFW.Keys.Up) == GLFW.InputState.Press)
            {
                renderDelay++;
            }

            // Increase speed
            if (Glfw.GetKey(window, GLFW.Keys.Down) == GLFW.InputState.Press)
            {
                renderDelay -= (renderDelay > 0) ? 1 : 0;
            }
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

                // Hide cursor
                Glfw.SetInputMode(openGL_Window, InputMode.Cursor, (int)GLFW.CursorMode.Hidden);

                // One time call to let all the primitives know the screen dimensions
                myPrimitive.init(gl_Width, gl_Height);

                Process(openGL_Window);

                Glfw.SetInputMode(openGL_Window, InputMode.Cursor, (int)GLFW.CursorMode.Normal);
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

#pragma warning restore CS0162
