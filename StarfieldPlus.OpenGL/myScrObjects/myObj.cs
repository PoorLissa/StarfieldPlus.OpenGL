﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Windows.Forms;


#pragma warning disable CS0162                      // Unreachable code warnings


namespace my
{
    // Additional parameters could be stored in this object
    public class myObjectParams
    {
        public myObjectParams()
        {
        }
        ~myObjectParams()
        {
        }
    }

    public class myObject
    {
        protected enum BgrDrawMode : byte { NEVER, ONCE, ALWAYS };

        protected uint id { get; private set; } = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public static int gl_Width, gl_Height, gl_x0, gl_y0, renderDelay = 25;
        private static uint s_id = uint.MaxValue;
        private static double cursorx = 0, cursory = 0;

        // ---------------------------------------------------------------------------------------------------------------

        protected static Random rand = new Random((int)DateTime.Now.Ticks);
        protected static List<myObject> list = null;
        protected static myColorPicker colorPicker = null;
        protected static myInstancedPrimitive inst = null;

        protected static BgrDrawMode bgrDrawMode = BgrDrawMode.NEVER;
        protected static float       bgrOpacity = 0.01f;
        protected static float       bgrR = 0.0f, bgrG = 0.0f, bgrB = 0.0f;

        protected static bool doClearBuffer = true;

        // ---------------------------------------------------------------------------------------------------------------

        public myObject()
        {
            // Assign unique id (the objects that are actually used have their ids starting at 0)
            id = s_id++;

            if (colorPicker == null)
            {
                gl_x0 = gl_Width  / 2;
                gl_y0 = gl_Height / 2;

                initGlobal();
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Override this function to perform one-time initialization upon creating the first object of a derived class
        protected virtual void initGlobal()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected virtual void generateNew()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected virtual void Move()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected virtual void Show()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Override it for every derived class to implement the logic
        protected virtual string CollectCurrentInfo(ref int width, ref int height)
        {
            return string.Empty;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Override it for every derived class to implement the logic
        protected virtual void setNextMode()
        {
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Override it for every derived class to implement the logic
        protected virtual void Process(Window window)
        {
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected void processInput(Window window)
        {
            // Exit via mouse move
            if (false)
            {
                double xpos, ypos;
                Glfw.GetCursorPosition(window, out xpos, out ypos);

                if (cursorx != 0 && cursory != 0)
                {
                    if (xpos != cursorx || ypos != cursory)
                    {
                        Glfw.SetWindowShouldClose(window, true);
                    }
                }

                cursorx = xpos;
                cursory = ypos;
            }

            // Exit via Esc Key press
            if (Glfw.GetKey(window, GLFW.Keys.Escape) == GLFW.InputState.Press)
            {
                Glfw.SetWindowShouldClose(window, true);
            }

            // Show some info (Tab)
            if (Glfw.GetKey(window, GLFW.Keys.Tab) == GLFW.InputState.Press)
            {
                displayInfo();
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

        // ---------------------------------------------------------------------------------------------------------------

        // Base Process method which calls for overriden classe's Process method
        public void Process(ScreenSaver scr)
        {
            try
            {
                // Set context creation hints
                myOGL.PrepareContext();

                // Create window
                Window openGL_Window = myOGL.CreateWindow(ref gl_Width, ref gl_Height, "scr.OpenGL", scr.GetMode());

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

                gl_x0 = gl_Width  / 2;
                gl_y0 = gl_Height / 2;

                Process(openGL_Window);

                Glfw.SetInputMode(openGL_Window, InputMode.Cursor, (int)GLFW.CursorMode.Normal);
                Glfw.Terminate();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"{this.ToString()} says:\n{ex.Message}\n\n{ex.StackTrace}", "Process Exception", MessageBoxButtons.OK);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

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

        // ---------------------------------------------------------------------------------------------------------------

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

        // ---------------------------------------------------------------------------------------------------------------

        // Get info string from the concrete object and display this info in a separate window
        private void displayInfo()
        {
            int width  = 600;
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

            rich.Dispose();
            form.Dispose();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Set RGB colors to use in dimScreen() function
        protected void dimScreenRGB_Set(float r, float g, float b)
        {
            bgrR = r;
            bgrG = g;
            bgrB = b;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Set RGB colors to use in dimScreen() function
        protected void dimScreenRGB_Adjust(float factor)
        {
            bgrR += myUtils.randFloat(rand) * myUtils.randomSign(rand) * factor;
            bgrG += myUtils.randFloat(rand) * myUtils.randomSign(rand) * factor;
            bgrB += myUtils.randFloat(rand) * myUtils.randomSign(rand) * factor;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Dim the screen constantly
        protected virtual void dimScreen(float dimAlpha, bool doShiftColor = false, bool useStrongerDimFactor = false)
        {
            int dimFactor = 1;

            if (useStrongerDimFactor)
            {
                int rnd = rand.Next(101);

                if (rnd < 11)
                {
                    dimFactor = (rnd == 0) ? 5 : 2;
                }
            }

            myPrimitive._Rectangle.SetAngle(0);

            if (doShiftColor)
            {
                // Shift background color just a bit, to hide long lasting traces of shapes
                myPrimitive._Rectangle.SetColor(rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, dimAlpha * dimFactor);
            }
            else
            {
                myPrimitive._Rectangle.SetColor(bgrR, bgrG, bgrB, dimAlpha * dimFactor);
            }

            myPrimitive._Rectangle.Draw(0, 0, gl_Width, gl_Height, true);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    };
};

#pragma warning restore CS0162
