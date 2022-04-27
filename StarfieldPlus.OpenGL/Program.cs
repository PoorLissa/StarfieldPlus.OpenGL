using System;
using System.Collections.Generic;
using System.Windows.Forms;



namespace StarfieldPlus.OpenGL
{
    class Program
    {
        // -------------------------------------------------------------------------------------------------------------------

        static void Main(string[] args)
        {
            const string appName = "starField.Plus.OpenGL";
            bool createdNew;
            var mutex = new System.Threading.Mutex(true, appName, out createdNew);

            // Allow only one single instance of the app
            if (createdNew)
            {
                if (args.Length > 0)
                {
                    string arg1 = args[0].ToLower().Trim(), arg2 = null;

                    // Handle cases where arguments are separated by colon (Example: /c:1234567 or /P:1234567)
                    if (arg1.Length > 2)
                    {
                        arg2 = arg1.Substring(3).Trim();
                        arg1 = arg1.Substring(0, 2);
                    }
                    else
                    {
                        if (args.Length > 1)
                            arg2 = args[1];
                    }

                    switch (arg1)
                    {
                        // Configuration mode
                        case "/c":
                            configProc();
                            break;

                        // Preview mode
                        case "/p":
                            previewProc();
                            break;

                        // Full-screen mode
                        case "/s":
                            mainProc();
                            break;

                        default:
                            MessageBox.Show($"Command line argument {arg1} is not valid", "ScreenSaver", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            break;
                    }
                }
                else
                {
                    // No arguments: treat like /c
                    // todo: remove this later and really treat it like /c
#if false
                    configProc();
#else
                    mainProc();
#endif
                }
            }

            return;
        }

        // -------------------------------------------------------------------------------------------------------------------

        // Display config window
        // todo: make it read/write config from a file (check if it is even possible due to a screensaver security restrictions)
        private static void configProc()
        {
            var f = new Form();
            f.Show();
            Application.Run(f);
        }

        // -------------------------------------------------------------------------------------------------------------------

        // Preview
        private static void previewProc()
        {
            ;
        }

        // -------------------------------------------------------------------------------------------------------------------

        private static void mainProc()
        {
            try
            {
                int w = 0, h = 0;
                myOGL.getDesktopResolution(ref w, ref h);

                ScreenSaver scr = new ScreenSaver(w, h);

                scr.selectObject();
                scr.Show();
            }
            catch (System.Exception)
            {
                ;
            }

            return;
        }

        // -------------------------------------------------------------------------------------------------------------------
    }
}
