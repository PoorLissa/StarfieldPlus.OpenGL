using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;



namespace StarfieldPlus.OpenGL
{
    class Program
    {

        // Used to prevent the screensaver from starting another instance
        private static System.Threading.Mutex singletonMutex = null;

        public static byte gl_WinVer = 0;

        // -------------------------------------------------------------------------------------------------------------------

        static void Main(string[] args)
        {
/*
            uint originalTimeout = 0;
            uint SPI_GETSCREENSAVETIMEOUT = 0x000E;
            uint SPI_SETSCREENSAVETIMEOUT = 0x000F;

            my.myWinAPI.SystemParametersInfo(SPI_GETSCREENSAVETIMEOUT,       0, ref originalTimeout, 0);
            my.myWinAPI.SystemParametersInfo(SPI_SETSCREENSAVETIMEOUT, 60 * 60, ref originalTimeout, 0);
*/
            const string appName = "starField.Plus.OpenGL";

            bool initialOwnershipGranted = false;
            singletonMutex = new System.Threading.Mutex(true, appName, out initialOwnershipGranted);

            // Allow only one single instance of the app
            if (initialOwnershipGranted)
            {
                ini_file_base _ini = new ini_file_base();
                _ini.read();

                if (_ini.getDic() == null)
                {
                    _ini["Settings.ImgPath"] = "";
                    _ini.save();
                }

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

                        // Full-screen mode
                        // '/t' means, we're in Windows 10, and the screensaver is started from TaskScheduler
                        case "/t":
                            gl_WinVer = 10;
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

                //my.myWinAPI.SystemParametersInfo(SPI_SETSCREENSAVETIMEOUT, originalTimeout, ref originalTimeout, 0);
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
            // !!! Microsoft are jerks, after all
            // https://www.tenforums.com/customization/25427-screen-saver-question-2.html
            // https://www.reddit.com/r/Windows10/comments/8pkrwx/desktop_disappears_when_using_bubbles_screensaver/
            // https://answers.microsoft.com/en-us/windows/forum/windows_8-desktop/bubbles-screensaver-has-black-background/e0807324-5ca6-4abe-b6ba-716848b41ff5?page=4

            // also,
            // https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-setthreadexecutionstate
            // https://stackoverflow.com/questions/3665332/how-do-i-prevent-screen-savers-and-sleeps-during-my-program-execution

            // scheduler solution: will probably need to set screensaver start timeout to high value, so it does not run again while the screensaver is active
            // https://superuser.com/questions/538146/run-a-batch-cmd-upon-screensaver

            // another scheduler solution (no dummy, just idle time - try it)
            // https://virtualcustoms.net/showthread.php/69386-Re-Enable-Screensaver-Transparency-%28float-on-desktop%29-Function-in-8-1

            /*
                Possible solutions to fix disgusting Windows 10 behaviour:
                1. Don't use desktop screenshot at all
                    - Bad idea
                2. Write my own scr launcher
                    - No flash upon the start
                    - I know how to check for idle time (mouse + keyboard)
                    - I don't know how to adjust it for active applications (youtube, video players, etc)
                3. Write an app that will take the desktop screenshot just before the screensaver launches
                    - Need to figure out the exact timing
                4. Use TaskScheduler to run the screensaver in response to Event ID 4802
                    - Need to use a dummy screensaver (or use this one, but with custom arguments)
                    - The dummy causes quick screen flash
                    - Don't need to think about active apps (the screensaver start time will be managed by Windows)
                    - Need to make sure the dummy does not start again while the scr is active (as the dummy will exit immediately -- its purpose is only to trigger the event)
                    - Need to make the screensaver the topmost window, as I already saw it starting in a background
            */

            winSpecificAction();

            try
            {
                ScreenSaver scr = new ScreenSaver();

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

        // Perform some additional actions, specific to different Windows versions
        private static void winSpecificAction()
        {
            switch (gl_WinVer)
            {
                // For Windows 10:
                // Prevent Windows from running the screensaver again;
                // todo: This needs some more thought, because now the PC does not go to sleep at all!
                case 10:
                    {
                        uint ES_CONTINUOUS = 0x80000000;
                        uint ES_DISPLAY_REQUIRED = 0x00000002;
                        //uint ES_SYSTEM_REQUIRED = 0x00000001;

                        my.myWinAPI.SetThreadExecutionState(ES_CONTINUOUS | ES_DISPLAY_REQUIRED);
                        //my.myWinAPI.SetThreadExecutionState((uint)(0x80000000L | 0x00000002L | 0x00000001L));
                    }
                    break;
            }
        }

        // -------------------------------------------------------------------------------------------------------------------
    }
}
