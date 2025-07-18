﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using StarfieldPlus.OpenGL;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing.Text;
using StarfieldPlus.OpenGL.myUtils;
using System.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using System.Security.AccessControl;


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

        // Timeouts for Monitor Off, System Sleep:
        private static uint _monitorOffTime = 10 * 60, _systemSleepTime = 30 * 60, _sysStateCnt = 0;
        private static DateTime _startTime;

        // ---------------------------------------------------------------------------------------------------------------

        public static int gl_Width, gl_Height, gl_x0, gl_y0, renderDelay = 25, stepsPerFrame = 1;
        private static uint s_id = uint.MaxValue;
        private static string filePath = "zzz.log";

#if !DEBUG
        private static double cursorx = 0, cursory = 0;
#endif

        // ---------------------------------------------------------------------------------------------------------------

        protected static Random rand = new Random((int)DateTime.Now.Ticks);
        protected static List<myObject> list = null;
        protected static myColorPicker colorPicker = null;
        protected static myInstancedPrimitive inst = null;
        protected static myStopwatch stopwatch = null;

        protected static BgrDrawMode bgrDrawMode = BgrDrawMode.NEVER;
        protected static float bgrOpacity = 0.01f;
        protected static float bgrR = 0.0f, bgrG = 0.0f, bgrB = 0.0f;

        protected static bool doClearBuffer = true;

        // ---------------------------------------------------------------------------------------------------------------

        public myObject()
        {
            // Assign unique id (the objects that are actually used have their ids starting at 0)
            id = s_id++;

            if (colorPicker == null)
            {
                {
                    try
                    {
                        filePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\" + filePath;

                        // Create log file and immediately close it
                        if (!System.IO.File.Exists(filePath))
                            System.IO.File.Create(filePath).Close();

                        // Get access privileges to the file
                        FileSecurity security = System.IO.File.GetAccessControl(filePath);
                        security.AddAccessRule(new FileSystemAccessRule("Everyone", FileSystemRights.Write, AccessControlType.Allow));
                        System.IO.File.SetAccessControl(filePath, security);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Log Exception", MessageBoxButtons.OK);
                    }
                }

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
        protected virtual void Process(GLFW.Window window)
        {
        }

        // ---------------------------------------------------------------------------------------------------------------

        // This function is called each frame. It is used to:
        // - process user input
        // - track system state
        protected void processInput(GLFW.Window window)
        {
            // Exit via mouse move (only in Release mode)
            {
#if DEBUG
                ;
#else
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
#endif
            }

            // Watch the Monitor Off timeout (Windows 10 related issue)
            if (Program.gl_State == Program.STATE.MANAGED_MAIN)
            {
                if (++_sysStateCnt == 100)
                {
                    _sysStateCnt = 0;

                    // This task is not time critical;
                    // Run it when the counter expires
                    if ((DateTime.Now - _startTime).TotalSeconds > _monitorOffTime)
                    {
                        Program.gl_State = Program.STATE.MANAGED_MONITOR_OFF;
                        Glfw.SetWindowShouldClose(window, true);
                        return;
                    }
                }
            }

            // Exit via Esc Key press
            if (Glfw.GetKey(window, GLFW.Keys.Escape) == GLFW.InputState.Press)
            {
                Glfw.SetWindowShouldClose(window, true);
                return;
            }

            // Show some info (Tab)
            if (Glfw.GetKey(window, GLFW.Keys.Tab) == GLFW.InputState.Press)
            {
                displayInfo();
                return;
            }

            // Decrease speed
            if (Glfw.GetKey(window, GLFW.Keys.Up) == GLFW.InputState.Press)
            {
                renderDelay++;
                stopwatch?.MakeSlower();
                return;
            }

            // Increase speed
            if (Glfw.GetKey(window, GLFW.Keys.Down) == GLFW.InputState.Press)
            {
                renderDelay -= (renderDelay > 0) ? 1 : 0;
                stopwatch?.MakeFaster();
                return;
            }

            // Next mode
            if (Glfw.GetKey(window, GLFW.Keys.Space) == GLFW.InputState.Press)
            {
                setNextMode();
                return;
            }

            // Print screen
            if (Glfw.GetKey(window, GLFW.Keys.PrintScreen) == GLFW.InputState.Press)
            {
                return;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Base Process method which calls for overriden classe's Process method
        public void Process(ScreenSaver scr)
        {
            try
            {
                Log($" ------------- {this.GetType().Name} ------------- ");

                // Get Monitor Off and Sleep timeout values
                getWindowsTimeouts();

                // Set context creation hints
                myOGL.PrepareContextHints();

                // Create window
                GLFW.Window openGL_Window = myOGL.CreateWindow(ref gl_Width, ref gl_Height, "scr.OpenGL", scr.GetMode());

                // Set Blend mode
                {
                    glEnable(GL_BLEND);                                 // Enable blending
                    glBlendEquation(GL_FUNC_ADD);
                    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);  // Set blending function                    
                }

                // One time call to let all the primitives know the screen dimensions
                myPrimitive.init(gl_Width, gl_Height);

                // Set culture to avoid incorrect float conversion in shader strings
                // Tags: locale, culture, en-US
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");

                // Hide cursor
                Glfw.SetInputMode(openGL_Window, InputMode.Cursor, (int)GLFW.CursorMode.Hidden);

                // Make the window topmost (in the case of task scheduler)
                makeTopmost(openGL_Window);

                // Main Procedure
                {
                    Log($"Scr: Process, gl_State = {Program.gl_State}");
                    Process(openGL_Window);
                    Log($"Scr: PostProcess");
                    PostProcess(openGL_Window);
                }

                // Show cursor
                Glfw.SetInputMode(openGL_Window, InputMode.Cursor, (int)GLFW.CursorMode.Normal);
                Glfw.Terminate();

                // Finally, in case we've reached all the timeouts, put the PC to sleep before exiting
                if (Program.gl_State == Program.STATE.MANAGED_SLEEP)
                {
                    Log("Scr: SetSuspendState");
                    Application.SetSuspendState(PowerState.Suspend, true, true);
                }

                Log("Scr: Graceful Exit");
            }
            catch (System.Exception ex)
            {
                Log($"Scr: System.Exception in Process() : {ex.Message}\n\n{ex.StackTrace}");

#if false
                var player = new SoundPlayer(@"c:\Windows\Media\Windows Hardware Fail.wav");
                player.Play();
#endif

                var time = DateTime.Now.ToString();
                MessageBox.Show($"{this.ToString()} says:\n{ex.Message}\n\n{ex.StackTrace}", $"Process Exception {time}", MessageBoxButtons.OK);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        public static void Log(string str)
        {
            try
            {
                using (System.IO.StreamWriter sw = System.IO.File.AppendText(filePath))
                {
                    var dt = DateTime.Now.ToString();
                    sw.WriteLine($"{dt} : " + str);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message, "Log Exception", MessageBoxButtons.OK);
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Get and set up the timers information needed to handle monitor off and computer sleep functions;
        private void getWindowsTimeouts()
        {
            _startTime = DateTime.Now;

            _monitorOffTime  = ((uint)my.myDll.monitorOffTimeout());
            _systemSleepTime = ((uint)my.myDll.systemSleepTimeout());
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void makeTopmost(GLFW.Window window)
        {
            // Make the process window topmost, as the TaskScheduler might run it in a background
            if (Program.gl_State == Program.STATE.TASK_SCHEDULER || Program.gl_State == Program.STATE.MANAGED_MAIN)
            {
#pragma warning disable
                if (window != IntPtr.Zero)
                {
                    int SWP_ASYNCWINDOWPOS = 0x4000;    // If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
                    int SWP_DEFERERASE     = 0x2000;    // Prevents generation of the WM_SYNCPAINT message.
                    int SWP_DRAWFRAME      = 0x0020;    // Draws a frame(defined in the window's class description) around the window.
                    int SWP_FRAMECHANGED   = 0x0020;    // Applies new frame styles set using the SetWindowLong function.Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
                    int SWP_HIDEWINDOW     = 0x0080;    // Hides the window.
                    int SWP_NOACTIVATE     = 0x0010;    // Does not activate the window.If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
                    int SWP_NOCOPYBITS     = 0x0100;    // Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
                    int SWP_NOMOVE         = 0x0002;    // Retains the current position(ignores X and Y parameters).
                    int SWP_NOOWNERZORDER  = 0x0200;    // Does not change the owner window's position in the Z order.
                    int SWP_NOREDRAW       = 0x0008;    // Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area(including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
                    int SWP_NOREPOSITION   = 0x0200;    // Same as the SWP_NOOWNERZORDER flag
                    int SWP_NOSENDCHANGING = 0x0400;    // Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
                    int SWP_NOSIZE         = 0x0001;    // Retains the current size(ignores the cx and cy parameters)
                    int SWP_NOZORDER       = 0x0004;    // Retains the current Z order(ignores the hWndInsertAfter parameter)
                    int SWP_SHOWWINDOW     = 0x0040;    // Displays the window

                    int flags = SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE;
                    int HWND_TOPMOST = -1;

                    var handle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                    my.myWinAPI.SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, flags);
                }
#pragma warning restore
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Some post procesing
        private void PostProcess(GLFW.Window window)
        {
            switch (Program.gl_State)
            {
                // In this case, screensaver reached the first timeout;
                // By this time, Windows should be turning the monitor off;
                // We're going to keep the scr running to prevent it from starting again;
                // This state should be active until we've received the user's input or the second timeout expires (whichever comes first)
                case Program.STATE.MANAGED_MONITOR_OFF:
                    {
                        Glfw.SetWindowShouldClose(window, false);

                        // Turn the screen off
                        // Actually, no need for that, as the system is able to count down monitor timeout and turn the screen off itself
                        //var handle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
                        //my.myWinApiExt.MonitorTurnOff(handle);

                        int sleepCnt = 0;

                        Stopwatch sw = new Stopwatch();
                        sw.Start();

                        // Recalculate sleep timer target value:
                        // If the monitor timeout was set to 20 min, and the system sleep was set to 60 min,
                        // then out current _systemSleepTime must be set to 60 - 20 = 40 min (the first 20 min are already gone).
                        _systemSleepTime -= _systemSleepTime > _monitorOffTime ? _monitorOffTime : 0;

                        while (!Glfw.WindowShouldClose(window))
                        {
                            processInput(window);
                            Glfw.PollEvents();
                            System.Threading.Thread.Sleep(66);

                            // Periodically check the time elapsed 
                            if (++sleepCnt == 50)
                            {
                                // Set the next state and break out of this loop
                                if (sw.ElapsedMilliseconds > _systemSleepTime * 1000)
                                {
                                    Program.gl_State = Program.STATE.MANAGED_SLEEP;
                                    Glfw.SetWindowShouldClose(window, true);
                                }

                                sleepCnt = 0;
                            }
                        }
                    }
                    break;
            }

            return;
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

                // Default implementation with minimal impact;
                // In case we use the shapes other than [0-4], we still want our 'inst' object to be non-null;
                // Then inst.ResetBuffer() will still work without additional checks
                default:
                    myPrimitive.init_TriangleInst(0);
                    myPrimitive._TriangleInst.setRotationMode(0);
                    inst = myPrimitive._TriangleInst;
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

            // 
            void func(object sender, PreviewKeyDownEventArgs e)
            {
                if (e.KeyCode == System.Windows.Forms.Keys.Escape || e.KeyCode == System.Windows.Forms.Keys.Tab)
                    form.Close();
            }

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

            rich.PreviewKeyDown += func;
            form.PreviewKeyDown += func;

            // Display the modal form
            {
                form.Focus();
                form.ShowDialog();
            }

            rich.PreviewKeyDown -= func;
            form.PreviewKeyDown -= func;

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

        // Get the RGB colors that are currently used in dimScreen() function
        protected void dimScreenRGB_Get(ref float r, ref float g, ref float b)
        {
            r = bgrR;
            g = bgrG;
            b = bgrB;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Set the RGB colors that are used by dimScreen() function
        protected void dimScreenRGB_SetRandom(float factor, bool ligtmMode = false)
        {
            if (ligtmMode)
            {
                bgrR = 1.0f - myUtils.randFloat(rand) * factor;
                bgrG = 1.0f - myUtils.randFloat(rand) * factor;
                bgrB = 1.0f - myUtils.randFloat(rand) * factor;
            }
            else
            {
                bgrR = myUtils.randFloat(rand) * factor;
                bgrG = myUtils.randFloat(rand) * factor;
                bgrB = myUtils.randFloat(rand) * factor;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Change the RGB colors that are used by dimScreen() function
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

            if (doShiftColor)
            {
                // Shift background color just a bit, to hide long lasting traces of shapes
                myPrimitive._Scr.SetColor(rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, rand.Next(5) * 0.01f, dimAlpha * dimFactor);
            }
            else
            {
                myPrimitive._Scr.SetColor(bgrR, bgrG, bgrB, dimAlpha * dimFactor);
            }

            myPrimitive._Scr.Draw();

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected virtual void clearScreenSetup(bool doClearBuffer, float rndFactor, bool front_and_back = false)
        {
            dimScreenRGB_SetRandom(rndFactor);

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(myObject.bgrR, myObject.bgrG, myObject.bgrB, 1);
            }
            else
            {
                glDrawBuffer(front_and_back ? GL_FRONT_AND_BACK : GL_BACK);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    };
};

#pragma warning restore CS0162
