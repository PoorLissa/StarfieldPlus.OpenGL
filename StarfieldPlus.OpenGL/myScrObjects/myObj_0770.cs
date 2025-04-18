﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - several layers of mutually repellent particles simulating liquid;
    - 'air' bubbles are raising from the bottom, moving the particles apart, and eventually reaching the surface
    - #unfinished, looks like crap
*/


namespace my
{
    public class myObj_0770 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0770);

        //private int layer;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, n = 0, shape = 0;
        private static bool doFillShapes = false;
        private static float maxRepelDistSquared = 1000;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0770()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            // Global unmutable constants
            {
                n = 3;
                N = rand.Next(10) + 10;
                N = 2345;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;

            renderDelay = rand.Next(3) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"renderDelay = {renderDelay}\n"         +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            if (id < n)
            {
                size = 33 + rand.Next(66);

                x = rand.Next(gl_Width);
                y = gl_Height + size * 2;

                dx = 0;
                dy = (0.75f + myUtils.randFloat(rand) * 0.25f) * -1.0f;

                A = 0.1f;
                R = G = B = 1;
            }
            else
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                dx = myUtils.randFloatSigned(rand, 0.1f) * 0.1f;
                dy = myUtils.randFloatSigned(rand, 0.1f) * 0.1f;

                size = 3;

                A = 0.2f;
                colorPicker.getColor(x, y, ref R, ref G, ref B);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (id < n)
            {
                x += dx;
                y += dy;

                if (y < -size)
                    generateNew();

                return;
            }

            int Count = list.Count;

            float factor = 0.9f;

            // Repelling by borders
            {
                int offset = 10;
                float repelForce = factor;

                if (x < offset)
                    dx += repelForce;

                if (x > gl_Width - offset)
                    dx -= repelForce;

                if (y < offset)
                    dy += repelForce;

                if (y > gl_Height - offset)
                    dy -= repelForce;
            }

            // Repelling by bubbles
            for (int i = 0; i < n; i++)
            {
                var bubble = list[i] as myObj_0770;

                float DX = x - bubble.x;
                float DY = y - bubble.y;
                float d2 = DX * DX + DY * DY;

                if (d2 < bubble.size * bubble.size)
                {
                    float dist = (float)Math.Sqrt(d2) + 0.0001f;

                    float F = 5.0f / dist;

                    dx += F * DX;
                    dy += F * DY;
                }
            }

            maxRepelDistSquared = 3333;

            // Repelling by each other
            for (int i = n; i != Count; i++)
            {
                var other = list[i] as myObj_0770;

                if (id != other.id)
                {
                    float DX = x - other.x;
                    float DY = y - other.y;
                    float d2 = DX * DX + DY * DY;

                    if (d2 < maxRepelDistSquared)
                    {
                        float dist = (float)Math.Sqrt(d2) + 0.0001f;

                        float F = factor / dist;

                        dx += F * DX;
                        dy += F * DY;
                    }
                }
            }

            // Apply gravity force
            dy += 0.005f;

            // Apply resisting force of the medium
            dx *= 1.0f - 0.1f;
            dy *= 1.0f - 0.1f;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            // Apply the final movement
            x += dx;
            y += dy;

            float size2x = size * 2;

            switch (shape)
            {
                // Instanced squares
                case 0:
                    myPrimitive._RectangleInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                    myPrimitive._RectangleInst.setInstanceColor(R, G, B, A);
                    myPrimitive._RectangleInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, size, angle);
                    myPrimitive._TriangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    myPrimitive._EllipseInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._EllipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    myPrimitive._PentagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._PentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    myPrimitive._HexagonInst.setInstanceCoords(x, y, size2x, angle);
                    myPrimitive._HexagonInst.setInstanceColor(R, G, B, A);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            while (list.Count < N)
            {
                list.Add(new myObj_0770());
            }

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                    grad.Draw();
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        (list[i] as myObj_0770).Move();
                    }

                    for (int i = 0; i != Count; i++)
                    {
                        (list[i] as myObj_0770).Show();
                    }

                    if (doFillShapes)
                    {
                        // Tell the fragment shader to multiply existing instance opacity by 0.5:
                        inst.SetColorA(-0.5f);
                        inst.Draw(true);
                    }

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};





#if false

using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    - several layers of mutually repellent particles simulating liquid;
    - 'air' bubbles are raising from the bottom, moving the particles apart, and eventually reaching the surface
*/


namespace my
{
    public class myObj_0770 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0770);

        private int layer;
        private float x, y, dx, dy;
        private float size, A, R, G, B, angle = 0;

        private static int N = 0, n = 0, shape = 0;
        private static bool doFillShapes = false;
        private static float maxRepelDistSquared = 1000;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0770()
        {
            if (id != uint.MaxValue)
                generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            // Global unmutable constants
            {
                n = 33;
                N = rand.Next(10) + 10;
                N = 3333;

                shape = rand.Next(5);
            }

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            doClearBuffer = true;

            renderDelay = rand.Next(3) + 3;

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            height = 600;

            string nStr(int   n) { return n.ToString("N0");    }
            //string fStr(float f) { return f.ToString("0.000"); }

            string str = $"Obj = {Type}\n\n"                         +
                            $"N = {nStr(list.Count)} of {nStr(N)}\n" +
                            $"renderDelay = {renderDelay}\n"         +
                            $"file: {colorPicker.GetFileName()}"
                ;
            return str;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // 
        protected override void setNextMode()
        {
            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void generateNew()
        {
            if (id < n)
            {
                size = 33 + rand.Next(66);

                x = rand.Next(gl_Width);
                y = gl_Height + size * 2;

                dx = 0;
                dy = (1 + myUtils.randFloat(rand) * 1.5f) * -1.0f;

                A = 0.01f;
                R = G = B = 1;
            }
            else
            {
                x = rand.Next(gl_Width);
                y = rand.Next(gl_Height);

                dx = myUtils.randFloatSigned(rand, 0.1f) * 0.1f;
                dy = myUtils.randFloatSigned(rand, 0.1f) * 0.1f;

                size = 3;

                A = 0.5f + myUtils.randFloat(rand) * 0.35f;
                colorPicker.getColor(x, y, ref R, ref G, ref B);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            if (id < n)
            {
                x += dx;
                y += dy;

                dy += 0.0001f;

                if (y < -size)
                    generateNew();

                return;
            }

            int Count = list.Count;

            float factor = 0.1f;

            // Repelling by borders
            {
                int offset = 10;
                float repelForce = factor;

                if (x < offset)
                    dx += repelForce;

                if (x > gl_Width - offset)
                    dx -= repelForce;

                if (y < offset)
                    dy += repelForce;

                if (y > gl_Height - offset)
                    dy -= repelForce;
            }

            // Repelling by bubbles
            for (int i = 0; i < n; i++)
            {
                var bubble = list[i] as myObj_0770;

                float DX = x - bubble.x;
                float DY = y - bubble.y;
                float d2 = DX * DX + DY * DY;

                if (d2 < bubble.size * bubble.size)
                {
                    float dist = (float)Math.Sqrt(d2) + 0.0001f;

                    float F = 10.0f / dist;

                    dx += F * DX * 0.1f;
                    dy += F * DY * 0.1f;

                    x += F * DX;
                    y += F * DY;
                }
            }

            maxRepelDistSquared = 3333;

            // Repelling by each other
            for (int i = n; i != Count; i++)
            {
                var other = list[i] as myObj_0770;

                if (id != other.id)
                {
                    float DX = x - other.x;
                    float DY = y - other.y;
                    float d2 = DX * DX + DY * DY;

                    if (d2 < maxRepelDistSquared)
                    {
                        float dist = (float)Math.Sqrt(d2) + 0.0001f;

                        float F = factor / dist;

                        dx += F * DX;
                        dy += F * DY;
                    }
                }
            }

            // Apply gravity force
            if (y < gl_y0/2)
                dy += 0.75f;

            // Apply resisting force of the medium
            if (true)
            {
                //dx *= 1.0f - 0.25f;
                //dy *= 1.0f - 0.25f;

                dx *= 1.0f - 0.01f;
                dy *= 1.0f - 0.01f;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            // Apply the final movement
            x += dx;
            y += dy;

            float size2x = size * 2;

            switch (shape)
            {
                // Instanced squares
                case 0:
                    rectInst.setInstanceCoords(x - size, y - size, size2x, size2x);
                    rectInst.setInstanceColor(R, G, B, A);
                    rectInst.setInstanceAngle(angle);
                    break;

                // Instanced triangles
                case 1:
                    var triangleInst = inst as myTriangleInst;

                    triangleInst.setInstanceCoords(x, y, size, angle);
                    triangleInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced circles
                case 2:
                    var ellipseInst = inst as myEllipseInst;

                    ellipseInst.setInstanceCoords(x, y, size2x, angle);
                    ellipseInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced pentagons
                case 3:
                    var pentagonInst = inst as myPentagonInst;

                    pentagonInst.setInstanceCoords(x, y, size2x, angle);
                    pentagonInst.setInstanceColor(R, G, B, A);
                    break;

                // Instanced hexagons
                case 4:
                    var hexagonInst = inst as myHexagonInst;

                    hexagonInst.setInstanceCoords(x, y, size2x, angle);
                    hexagonInst.setInstanceColor(R, G, B, A);
                    break;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Process(Window window)
        {
            uint cnt = 0;
            initShapes();

            clearScreenSetup(doClearBuffer, 0.1f);

            while (list.Count < N)
            {
                list.Add(new myObj_0770());
            }

            while (!Glfw.WindowShouldClose(window))
            {
                int Count = list.Count;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                // Dim screen
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                    grad.Draw();
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        (list[i] as myObj_0770).Move();
                    }

                    for (int i = 0; i != Count; i++)
                    {
                        (list[i] as myObj_0770).Show();
                    }

                    if (doFillShapes)
                    {
                        // Tell the fragment shader to multiply existing instance opacity by 0.5:
                        inst.SetColorA(-0.5f);
                        inst.Draw(true);
                    }

                    // Tell the fragment shader to do nothing with the existing instance opacity:
                    inst.SetColorA(0);
                    inst.Draw(false);
                }

                cnt++;
                System.Threading.Thread.Sleep(renderDelay);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N, 0);

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------
    }
};


#endif
