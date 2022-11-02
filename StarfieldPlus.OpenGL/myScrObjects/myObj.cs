using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Windows.Forms;


#pragma warning disable CS0162                      // Unreachable code warnings


namespace my
{
    public class myObject
    {
        public static int gl_Width, gl_Height, gl_x0, gl_y0, renderDelay = 25;

        // -------------------------------------------------------------------------

        protected static Random rand = new Random((int)DateTime.Now.Ticks);
        protected static List<myObject> list = null;
        protected static myColorPicker colorPicker = null;
        protected static myInstancedPrimitive inst = null;

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

        // Override it for every derived class to implement the logic
        protected virtual string CollectCurrentInfo(ref int width, ref int height)
        {
            return string.Empty;
        }

        // -------------------------------------------------------------------------

        // Override it for every derived class to implement the logic
        protected virtual void setNextMode()
        {
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
                int width = 500;
                int height = 500;

                var form = new Form();
                var rich = new RichTextBox();

                form.Width = width;
                form.Height = height;
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
                rich.AppendText(CollectCurrentInfo(ref width, ref height));
                rich.AppendText("\n");
                rich.SelectAll();
                rich.SelectionAlignment = HorizontalAlignment.Center;
                rich.DeselectAll();
                rich.Select(rich.TextLength, 0);
                rich.ReadOnly = true;
                form.Controls.Add(rich);

                if (form.Width != width)
                    form.Width = width;

                if (form.Height != height)
                    form.Height = height;

                rich.PreviewKeyDown += (object sender, PreviewKeyDownEventArgs e) => {
                    if (e.KeyCode == System.Windows.Forms.Keys.Escape || e.KeyCode == System.Windows.Forms.Keys.Tab)
                        form.Close();
                };

                form.PreviewKeyDown += (object sender, PreviewKeyDownEventArgs e) => {
                    if (e.KeyCode == System.Windows.Forms.Keys.Escape || e.KeyCode == System.Windows.Forms.Keys.Tab)
                        form.Close();
                };

                // Display modal form
                form.ShowDialog();
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

            // Next mode
            if (Glfw.GetKey(window, GLFW.Keys.Space) == GLFW.InputState.Press)
            {
                setNextMode();
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

                    gl_x0 = gl_Width  / 2;
                    gl_y0 = gl_Height / 2;
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

        // Instantiate instanced primitive
        protected void initShapes(int shape, int cnt, int rotationSubMode)
        {
            switch (shape)
            {
                case 0:
                    myPrimitive.init_RectangleInst(cnt);
                    myPrimitive._RectangleInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._RectangleInst;
                    break;

                case 1:
                    myPrimitive.init_TriangleInst(cnt);
                    myPrimitive._TriangleInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._TriangleInst;
                    break;

                case 2:
                    myPrimitive.init_EllipseInst(cnt);
                    myPrimitive._EllipseInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._EllipseInst;
                    break;

                case 3:
                    myPrimitive.init_PentagonInst(cnt);
                    myPrimitive._PentagonInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._PentagonInst;
                    break;

                case 4:
                    myPrimitive.init_HexagonInst(cnt);
                    myPrimitive._HexagonInst.setRotationMode(rotationSubMode);
                    inst = myPrimitive._HexagonInst;
                    break;
            }

            return;
        }

        // -------------------------------------------------------------------------
    };
};

#pragma warning restore CS0162
