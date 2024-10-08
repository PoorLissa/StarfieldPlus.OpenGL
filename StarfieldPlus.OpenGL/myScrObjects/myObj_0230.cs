﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;


/*
    for reference:
    https://www.youtube.com/watch?v=0Kx4Y9TVMGg&ab_channel=Brainxyz

    - Gravity, unfinished

Got it! To achieve this, you can first calculate the total center of mass for all particles. Then, for each particle, you can exclude it
from the center of mass calculation by adjusting the total mass and the weighted sums accordingly. This way, you only need to calculate
the center of mass once and then adjust it for each particle.

Here’s how you can do it:

Calculate the Total Center of Mass: Compute the center of mass for all particles.
Adjust for Each Particle: For each particle, adjust the total mass and weighted sums to exclude that particle and compute the new center
of mass.

Example in C++
#include <vector>
#include <iostream>

struct Particle {
    double x, y; // Position
    double mass; // Mass
};

std::pair<double, double> calculateTotalCenterOfMass(const std::vector<Particle>& particles) {
    double totalMass = 0.0;
    double weightedSumX = 0.0;
    double weightedSumY = 0.0;

    for (const auto& p : particles) {
        totalMass += p.mass;
        weightedSumX += p.mass * p.x;
        weightedSumY += p.mass * p.y;
    }

    double centerX = weightedSumX / totalMass;
    double centerY = weightedSumY / totalMass;

    return {centerX, centerY};
}

std::pair<double, double> calculateReducedCenterOfMass(const std::vector<Particle>& particles, size_t excludeIndex, double totalMass, double weightedSumX, double weightedSumY)
{
    double reducedMass = totalMass - particles[excludeIndex].mass;
    double reducedSumX = weightedSumX - particles[excludeIndex].mass * particles[excludeIndex].x;
    double reducedSumY = weightedSumY - particles[excludeIndex].mass * particles[excludeIndex].y;

    double centerX = reducedSumX / reducedMass;
    double centerY = reducedSumY / reducedMass;

    return {centerX, centerY};
}

int main() {
    std::vector<Particle> particles = {
        {0.0, 0.0, 1.0},
        {1.0, 0.0, 2.0},
        {0.0, 1.0, 3.0},
        {1.0, 1.0, 4.0}
    };

    // Calculate the total center of mass
    auto totalCenterOfMass = calculateTotalCenterOfMass(particles);
    double totalMass = 0.0;
    double weightedSumX = 0.0;
    double weightedSumY = 0.0;

    for (const auto& p : particles) {
        totalMass += p.mass;
        weightedSumX += p.mass * p.x;
        weightedSumY += p.mass * p.y;
    }

    std::cout << "Total Center of Mass: (" << totalCenterOfMass.first << ", " << totalCenterOfMass.second << ")\n";

    // Calculate the reduced center of mass for each particle
    for (size_t i = 0; i < particles.size(); ++i) {
        auto reducedCenterOfMass = calculateReducedCenterOfMass(particles, i, totalMass, weightedSumX, weightedSumY);
        std::cout << "Reduced Center of Mass excluding particle " << i << ": (" << reducedCenterOfMass.first << ", " << reducedCenterOfMass.second << ")\n";
    }

    return 0;
}

Explanation
Calculate Total Center of Mass: The calculateTotalCenterOfMass function computes the center of mass for all particles.
Adjust for Each Particle: The calculateReducedCenterOfMass function adjusts the total mass and weighted sums to exclude a specific particle and computes the new center of mass.
Main Function: The main function demonstrates how to use these functions to calculate the total center of mass and the reduced center of mass for each particle.
This approach ensures that you only calculate the total center of mass once and then efficiently adjust it for each particle.

Would you like more details on any specific part of this process or help with another aspect of your simulation?
*/


namespace my
{
    public class myObj_0230 : myObject
    {
        // Priority
        public static int Priority => 10;
		public static System.Type Type => typeof(myObj_0230);

        private enum ParticleType { One, Two, Three, Four };

        private float x, y, dx, dy;
        private float mass, size, A, R, G, B, angle;
        private ParticleType type;

        private static int N = 0, shape = 0;
        private static bool doFillShapes = true, doUseRandomMass = false;
        private static float dimAlpha = 0.05f;

        private static int nTaskCount = 1, activeThreads = 1, proc = 0;
        private static bool threadsAreRunning = true;

/*
        private static int border = 3;
        private static float reverseFactor = 0.99999f;
        private static float resistFactor = 0.99999f;
*/
        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0230()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            nTaskCount = Environment.ProcessorCount - 1;

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            N = (N == 0) ? 100 + rand.Next(100) : N;
            N = 2345;
            //N = 3333;

            // As it is a very processor-consuming task, reduce the number of particles in most of the cases:
            switch (rand.Next(5))
            {
                case 0:
                    N = rand.Next(666) + 666;
                    break;

                case 1:
                    N = rand.Next(777) + 777;
                    break;

                case 2:
                    N = rand.Next(1111) + 1111;
                    break;

                case 3:
                    N = 2345;
                    break;

                case 4:
                    N = 3333;
                    break;
            }

            N = 666;

            doUseRandomMass = myUtils.randomBool(rand);

            //doUseRandomMass = false;
            proc = 1;

            // Determine the number of threads we need
            {
                if (nTaskCount > 0)
                {
                    int n = (N / 200) + 1;
                    nTaskCount = n < nTaskCount ? n : nTaskCount;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = {Type}\n\n"                 	+
                            myUtils.strCountOf(list.Count, N)   +
                            $"nTaskCount = {nTaskCount}\n"      +
                            $"proc = {proc}\n"                  +
                            $"renderDelay = {renderDelay}\n"    +
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
            x = rand.Next(gl_Width);
            y = rand.Next(gl_Height);

            dx = dy = 0;

            dx = myUtils.randomSign(rand) * (float)rand.NextDouble() * 3;
            dy = myUtils.randomSign(rand) * (float)rand.NextDouble() * 3;

            if (doUseRandomMass)
            {
                mass = rand.Next(3333) + 10;
                //mass *= rand.Next(100) == 0 ? 10 : 1;
            }
            else
            {
                mass = 500;
            }

            size = mass < 500 ? 1 : mass / 500;
            angle = 0;

            A = 0.5f;
            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

            type = ParticleType.Two;
            /*
                        if (rand.Next(666) == 0)
                            type = ParticleType.One;
            */
            type = ParticleType.Three;
            type = (ParticleType)rand.Next(4);

#if false
            if (list.Count == 11 /*|| list.Count == 12 || list.Count == 13*/)
            {
                type = ParticleType.Four;
                mass = 5000000;
                size = 7;
                dx = 0;
                dy = 0;
            }
#endif

            if (type == ParticleType.One)
            {
                R = 0.50f + (float)(rand.NextDouble() * 0.50);
                G = 0.20f + (float)(rand.NextDouble() * 0.10);
                B = 0.10f + (float)(rand.NextDouble() * 0.10);
            }

            if (type == ParticleType.Two)
            {
                R = 0.10f + (float)(rand.NextDouble() * 0.10);
                G = 0.50f + (float)(rand.NextDouble() * 0.25);
                B = 0.10f + (float)(rand.NextDouble() * 0.10);
            }

            if (type == ParticleType.Three)
            {
                R = 0.10f + (float)(rand.NextDouble() * 0.10);
                G = 0.45f + (float)(rand.NextDouble() * 0.10);
                B = 0.50f + (float)(rand.NextDouble() * 0.33);
            }

            if (type == ParticleType.Four)
            {
                R = 1.0f / 250 * 245 + (float)(rand.NextDouble() * 0.10);
                G = 1.0f / 250 * 180 + (float)(rand.NextDouble() * 0.10);
                B = 1.0f / 250 * 040 + (float)(rand.NextDouble() * 0.10);
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            myObj_0230 obj;
            float DX = 0, DY = 0, dist = 0, F = 0, factor = 0, d2 = 0;
            float anotherResistFactor = 1.0f - 0.00001f;

            int Count = list.Count;

            for (int i = 0; i != Count; i++)
            {
                obj = (myObj_0230)(list[i]);

                {
                    factor = 0.000001f;

                    DX = x - obj.x;
                    DY = y - obj.y;
                    d2 = DX * DX + DY * DY;

                    if (d2 > 0)
                    {
                        dist = (float)Math.Sqrt(d2);

                        if (type == obj.type)
                        {
                            if (dist < 20)
                            {
                                factor *= dist < 10 ? -1000 : -500;

                                if (!doUseRandomMass)
                                {
                                    factor *= 10;
                                }
                            }
                            else
                            {
                                switch (type)
                                {
                                    case ParticleType.One:

                                        if (dist >= 222)
                                            factor *= 0.1f;

                                        if (dist > 333)
                                            factor *= 0.05f;

                                        factor *= 5;
                                        break;

                                    case ParticleType.Two:

                                        if (dist >= 222)
                                            factor *= 0.1f;

                                        if (dist > 333)
                                            factor *= 0.03f;

                                        factor *= 3;
                                        break;

                                    case ParticleType.Three:

                                        if (dist >= 222)
                                            factor *= 0.1f;

                                        if (dist > 333)
                                            factor *= 0.02f;

                                        factor *= 2;
                                        break;

                                    case ParticleType.Four:

                                        if (dist >= 222)
                                            factor *= 0.1f;

                                        if (dist > 333)
                                            factor *= 0.01f;

                                        factor *= 1;
                                        break;
                                }
                            }
                        }
                        else
                        {
                            if (dist >= 100)
                            {
                                factor = 0;
                            }
                            else
                            {
                                //factor *= -1000;
                                //factor *= aaa;

                                switch (type)
                                {
                                    case ParticleType.One:
                                        factor *= 3;
                                        break;

                                    case ParticleType.Two:
                                        factor *= 3;
                                        break;

                                    case ParticleType.Three:
                                        factor *= 3;
                                        break;

                                    case ParticleType.Four:
                                        factor *= 3;
                                        break;
                                }
                            }
                        }

                        if (factor != 0)
                        {
                            F = factor * obj.mass / dist;

                            dx -= F * DX;
                            dy -= F * DY;
                        }

                        // Another resisting force ---- WTF is this here and not outside the loop?
                        dx *= anotherResistFactor;
                        dy *= anotherResistFactor;

#if false
                        // Optional resisting force
                        if (dist < 10)
                        {
                            dx *= resistFactor;
                            dy *= resistFactor;
                        }
                        else
                        {
                            dx *= resistFactor * 1.00000001f;
                            dy *= resistFactor * 1.00000001f;
                        }
#endif
                    }

#if false
                    if (x < border && dx < 0)
                    {
                        //dx *= -1;
                        dx *= reverseFactor;
                    }

                    if (x > gl_Width - border && dx > 0)
                    {
                        //dx *= -1;
                        dx *= reverseFactor;
                    }

                    if (y < border && dy < 0)
                    {
                        //dy *= -1;
                        dy *= reverseFactor;
                    }

                    if (y > gl_Height - border && dy > 0)
                    {
                        //dy *= -1;
                        dy *= reverseFactor;
                    }
#endif
                }   // end of 'if (obj != this)'
            }       // end of 'for' loop

            // For 2 points, the center of masses MC lies somewhere on the line between them.
            // The distance from pt1 to MC, d = DIST / (m1/m2 + 1);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
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
                    myPrimitive._TriangleInst.setInstanceCoords(x, y, size2x, angle);
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
            initShapes();

            // Disable VSYNC if needed
            Glfw.SwapInterval(0);

            // Threading
            {
                if (nTaskCount == 0)
                {
                    proc = 0;
                }

                switch (proc)
                {
                    case 0:
                        process0(window);
                        break;

                    case 1:
                        process1(window);   // old threads -- seems to be faster
                        break;

                    case 2:
                        process2(window);   // new threads
                        break;
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N, 0);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void process0(Window window)
        {
            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void process1(Window window)
        {
            uint cnt = 0;

            if (doClearBuffer)
            {
                glDrawBuffer(GL_FRONT_AND_BACK | GL_DEPTH_BUFFER_BIT);
                glClearColor(0, 0, 0, 1);
            }

            while (list.Count < N)
            {
                list.Add(new myObj_0230());
            }

            var taskList = new System.Threading.Tasks.Task[nTaskCount];

            // Define a delegate that prints and returns the system tick count
            Func<object, int> action = (object obj) =>
            {
                int k = (int)obj;

                int beg = (k + 0) * list.Count / nTaskCount;
                int end = (k + 1) * list.Count / nTaskCount;

                for (int i = beg; i < end; i++)
                    (list[i] as myObj_0230).Move();

                return 0;
            };

            for (int k = 0; k < nTaskCount; k++)
            {
                var task = System.Threading.Tasks.Task<int>.Factory.StartNew(action, k);
                taskList[k] = task;
            }

            var t1 = System.DateTime.Now.Ticks;

            while (!Glfw.WindowShouldClose(window))
            {
                cnt++;

                processInput(window);

                // Swap fore/back framebuffers, and poll for operating system events.
                Glfw.SwapBuffers(window);
                Glfw.PollEvents();

                if (doClearBuffer)
                {
                    glClear(GL_COLOR_BUFFER_BIT);
                }
                else
                {
                    dimScreen(dimAlpha, false);
                }

                // Render Frame
                {
                    inst.ResetBuffer();

                    // Wait until all the tasks have finished
                    System.Threading.Tasks.Task.WaitAll(taskList);

                    // Draw everything to the Inst
                    for (int i = 0; i < list.Count; i++)
                    {
                        (list[i] as myObj_0230).Show();
                    }

                    // Restart all the tasks
                    for (int k = 0; k < nTaskCount; k++)
                    {
                        var task = System.Threading.Tasks.Task<int>.Factory.StartNew(action, k);
                        taskList[k] = task;
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
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void process2(Window window)
        {
            uint cnt = 0;
            object thLock = new object();

            var thList = new System.Threading.Thread[nTaskCount];
            var pauseEvents = new System.Threading.ManualResetEvent[nTaskCount];

            {
                glDrawBuffer(GL_FRONT | GL_DEPTH_BUFFER_BIT);

                float r = (float)rand.NextDouble() / 11;
                float g = (float)rand.NextDouble() / 11;
                float b = (float)rand.NextDouble() / 11;

                glClearColor(r, g, b, 1);
            }

            while (list.Count < N)
            {
                list.Add(new myObj_0230());
            }

            // Initialize and start all the threads
            {
                activeThreads = nTaskCount;

                for (int k = 0; k != nTaskCount; k++)
                {
                    thList[k] = new System.Threading.Thread(
                        new System.Threading.ParameterizedThreadStart(thFunc))
                        {
                            Name = $"th_{k.ToString("000")}",
                            Priority = System.Threading.ThreadPriority.Normal
                        };
                    pauseEvents[k] = new System.Threading.ManualResetEvent(true);   // Threads are initially NOT blocked
                    thList[k].Start(k);
                }

                // Thread function
                void thFunc(object obj)
                {
                    int threadId = (int)obj;

                    int beg = (threadId + 0) * list.Count / nTaskCount;
                    int end = (threadId + 1) * list.Count / nTaskCount;

                    var pauseEvent = pauseEvents[threadId];

                    while (threadsAreRunning)
                    {
                        // Wait while the thread is blocked
                        pauseEvent.WaitOne(System.Threading.Timeout.Infinite);

                        if (threadsAreRunning == false)
                            return;

                        for (int i = beg; i != end; i++)
                        {
                            (list[i] as myObj_0230).Move();
                        }

                        lock (thLock)
                        {
                            pauseEvent.Reset();         // Block the thread
                            activeThreads--;
                        }
                    }
                }
            }

            while (!Glfw.WindowShouldClose(window))
            {
                while (System.Threading.WaitHandle.WaitAll(pauseEvents, 0, true))
                    ;

                processInput(window);

                if (activeThreads == 0)
                {
                    cnt++;

                    // Swap fore/back framebuffers, and poll for operating system events.
                    Glfw.SwapBuffers(window);
                    Glfw.PollEvents();

                    glClear(GL_COLOR_BUFFER_BIT);

                    // Render Frame
                    {
                        inst.ResetBuffer();

                        // Draw everything to the Inst
                        int Count = list.Count;
                        for (int i = 0; i != Count; i++)
                        {
                            (list[i] as myObj_0230).Show();
                        }

                        // Restart all the tasks
                        lock (thLock)
                        {
                            activeThreads = nTaskCount;
                            for (int k = 0; k != nTaskCount; k++)
                                pauseEvents[k].Set();
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
                }
            }

            // Stop all the threads
            {
                threadsAreRunning = false;

                // Unblock all the threads first, to let them stop gracefully
                foreach (System.Threading.ManualResetEvent e in pauseEvents)
                    e.Set();

                foreach (System.Threading.Thread th in thList)
                    th.Join();
            }

            return;
        }
    }
};
