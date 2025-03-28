﻿using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Windows.Forms;


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
    public class myObj_0231 : myObject
    {
        // Priority
        public static int Priority => 30;
		public static System.Type Type => typeof(myObj_0231);

        private int cnt;
        private float x, y, dx, dy;
        private float mass, size, A, R, G, B, angle;

        private static int N = 0, shape = 0, moveMode = 0;
        private static int localCenterX = 0, localCenterY = 0, localMode = 0, chanceMin = 1, chanceMax = 1, tmpX = 0, tmpY = 0, tmpMax = 0;
        private static int nTrails = 0, nTrailsQty = 0, trailRatio = 1, nTrailLengthMin = 0, nTrailLengthMax = 0;
        private static bool doFillShapes = true, doUseRandomMass = false, doUseCenters = false, doUseSingleLargeMass = false;
        private static bool doUseColorPicker = false, doUseTrails = false;
        private static float localR = 0, localG = 0, localB = 9, maxOpacity = 1;
        private static float constSpd = 1.0f;

        private static float totalMass = 0;
        private static float weightedSumX = 0;
        private static float weightedSumY = 0;
        private static float centerX = 0;
        private static float centerY = 0;

        private myParticleTrail trail = null;

        private static myScreenGradient grad = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0231()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height, mode: myColorPicker.colorMode.SNAPSHOT_OR_IMAGE);
            list = new List<myObject>();

            shape = rand.Next(5);

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            N = 111111 + rand.Next(123456);

            trailRatio = 66;                    // Ratio of particles with a trail to the total number of particles
            nTrailsQty = N / trailRatio;        // Exact number of particles with a trail

            nTrailLengthMin = 33;               // Trail min length
            nTrailLengthMax = 133;              // Trail length = nTrailLengthMin + rand(nTrailLengthMax);

            if (myUtils.randomChance(rand, 1, 3))
            {
                nTrailLengthMax = 133 + rand.Next(99);
            }

            doUseRandomMass = myUtils.randomBool(rand);
            doUseCenters = myUtils.randomChance(rand, 2, 3);
            doClearBuffer = myUtils.randomBool(rand);
            doUseSingleLargeMass = myUtils.randomChance(rand, 1, 5);
            doUseColorPicker = myUtils.randomChance(rand, 1, 3);
            doUseTrails = myUtils.randomChance(rand, 1, 2);

            // doUseCenters chance
            {
                chanceMax = 1 + rand.Next(33);
                do { chanceMin = 1 + rand.Next(11);
                } while (chanceMax < chanceMin);

                tmpMax = 1 + rand.Next(23);
            }

            renderDelay = 3;
            moveMode = rand.Next(8);
            localMode = rand.Next(5);

            localCenterX = rand.Next(2 * gl_Width) - gl_Width / 2;
            localCenterY = rand.Next(2 * gl_Width) - gl_Width / 2 - (gl_Width - gl_Height) / 2;

            localR = myUtils.randFloat(rand);
            localG = myUtils.randFloat(rand);
            localB = myUtils.randFloat(rand);

            constSpd = 0.0001f + myUtils.randFloat(rand);
            maxOpacity = 0.25f + myUtils.randFloat(rand) * 0.75f;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = {Type}\n\n"                 	               +
                            myUtils.strCountOf(list.Count, N)                  +
                            $"moveMode = {moveMode}\n"                         +
                            $"localMode = {localMode}\n"                       +
                            $"doUseCenters = {doUseCenters}\n"                 +
                            $"doUseRandomMass = {doUseRandomMass}\n"           +
                            $"doUseSingleLargeMass = {doUseSingleLargeMass}\n" +
                            $"doClearBuffer = {doClearBuffer}\n"               +
                            $"doUseTrails = {doUseTrails}\n"                   +
                            $"maxOpacity = {myUtils.fStr(maxOpacity)}\n"       +
                            $"chance = {chanceMin} of {chanceMax}\n"           +
                            $"renderDelay = {renderDelay}\n"                   +
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
            cnt = 33 + rand.Next(133);

            x = rand.Next(2 * gl_Width) - gl_Width/2;
            y = rand.Next(2 * gl_Width) - gl_Width/2 - (gl_Width - gl_Height)/2;

            A = 0.5f;
            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

            if (doUseCenters && myUtils.randomChance(rand, chanceMin, chanceMax))
            {
                // Get new local center
                if (myUtils.randomChance(rand, 1, 1111))
                {
                    localCenterX = rand.Next(2 * gl_Width) - gl_Width / 2;
                    localCenterY = rand.Next(2 * gl_Width) - gl_Width / 2 - (gl_Width - gl_Height) / 2;

                    localR = myUtils.randFloat(rand);
                    localG = myUtils.randFloat(rand);
                    localB = myUtils.randFloat(rand);
                }

                // Get localized coordinates:

                switch (localMode)
                {
                    case 0:
                        {
                            x = localCenterX + myUtils.randomSign(rand) * rand.Next(111);
                            y = localCenterY + myUtils.randomSign(rand) * rand.Next(111);
                        }
                        break;

                    case 1:
                        {
                            int rad = 10 + rand.Next(123);
                            x = localCenterX + myUtils.randomSign(rand) * rand.Next(rad);
                            y = localCenterY + myUtils.randomSign(rand) * rand.Next(rad);
                        }
                        break;

                    case 2:
                        {
                            int rnd = rand.Next(222) * myUtils.randomSign(rand);
                            x = localCenterX + rnd;
                            y = localCenterY + rnd;
                        }
                        break;

                    case 3:
                        {
                            int rad = 80 + rand.Next(33);
                            int rnd1 = rand.Next(rad) * myUtils.randomSign(rand);
                            int rnd2 = rand.Next(rad) * myUtils.randomSign(rand);

                            x = localCenterX + rnd1 + rand.Next(75) * myUtils.randomSign(rand) + rand.Next(50) * myUtils.randomSign(rand) + rand.Next(23) * myUtils.randomSign(rand);
                            y = localCenterY + rnd2 + rand.Next(75) * myUtils.randomSign(rand) + rand.Next(50) * myUtils.randomSign(rand) + rand.Next(23) * myUtils.randomSign(rand);
                        }
                        break;

                    case 4:
                        {
                            int rad = rand.Next(tmpMax);

                            x = localCenterX + tmpX;
                            y = localCenterY + tmpY;

                            tmpX += rand.Next(rad) * myUtils.randomSign(rand);
                            tmpY += rand.Next(rad) * myUtils.randomSign(rand);
                        }
                        break;
                }

                float f = 0.1f;

                R = localR + (float)rand.NextDouble() * f;
                G = localG + (float)rand.NextDouble() * f;
                B = localB + (float)rand.NextDouble() * f;
            }

            dx = dy = 0;

            switch (moveMode)
            {
                case 0:
                    break;

                case 1:
                    dx = myUtils.randomSign(rand) * (float)rand.NextDouble() * 3;
                    dy = myUtils.randomSign(rand) * (float)rand.NextDouble() * 3;
                    break;

                case 2:
                    if (myUtils.randomChance(rand, 1, 333))
                    {
                        dx = myUtils.randomSign(rand) * (float)rand.NextDouble() * 27;
                        dy = myUtils.randomSign(rand) * (float)rand.NextDouble() * 27;
                    }
                    break;

                case 3:
                    dy = constSpd * (x > gl_x0 ? 1 : -1);
                    break;

                case 4:
                    dy = constSpd * (x > gl_x0 ? 1 : -1);
                    dy *= Math.Abs((x - gl_x0) / gl_x0);
                    break;

                case 5:
                    dx = myUtils.randomSign(rand) * (float)rand.NextDouble() * 3;
                    break;

                case 6:
                    dy = myUtils.randomSign(rand) * (float)rand.NextDouble() * 3;
                    break;

                case 7:
                    if (myUtils.randomChance(rand, 1, 2))
                    {
                        dx = myUtils.randomSign(rand) * (float)rand.NextDouble() * 3;
                    }
                    else
                    {
                        dy = myUtils.randomSign(rand) * (float)rand.NextDouble() * 3;
                    }
                    break;

                case 99:
                    if (myUtils.randomChance(rand, 1, 2))
                    {
                        dx = myUtils.randomSign(rand) * (float)rand.NextDouble() * 3;
                    }
                    else
                    {
                        dy = myUtils.randomSign(rand) * (float)rand.NextDouble() * 3;
                    }
                    break;
            }

            if (doUseRandomMass)
            {
                mass = rand.Next(9999) + 100;

                if (myUtils.randomChance(rand, 1, 12345))
                    mass = rand.Next(933000) + 100;
            }
            else
            {
                mass = 5000;
            }

            if (doUseSingleLargeMass)
            {
                if (id == 0)
                {
                    //mass = 99999999.0f * 100;
                    mass = 99999999.0f * 1;
                }
            }

            //size = mass < 500 ? 1 : mass / 25000;

            size = (int)Math.Log(mass)/5;
            size = size < 1 ? 1 : size;

            angle = 0;

            if (id != uint.MaxValue)
                totalMass += mass;

            if (doUseColorPicker)
            {
                colorPicker.getColor(x, y, ref R, ref G, ref B);
            }

            // Trails
            if (doUseTrails && nTrails < nTrailsQty && myUtils.randomChance(rand, 1, trailRatio))
            {
                nTrails++;
                int nTrail = nTrailLengthMin + rand.Next(nTrailLengthMax);

                // Initialize Trail
                if (trail == null)
                {
                    trail = new myParticleTrail(nTrail, x, y);
                }
                else
                {
                    trail.reset(x, y);
                }

                if (trail != null)
                {
                    trail.updateDa(A);
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Move()
        {
            float DX = 0, DY = 0, dist = 0, F = 0, factor = 0, d2 = 0;

            // Get reduced mass and reduced center of masses:
            float reducedMass = totalMass - mass;
            float reducedSumX = weightedSumX - mass * x;
            float reducedSumY = weightedSumY - mass * y;

            float thisCenterX = reducedSumX / reducedMass;
            float thisCenterY = reducedSumY / reducedMass;

            //factor = 0.000001f;
            factor = 0.0000000001f / N;

            DX = thisCenterX - x;
            DY = thisCenterY - y;
            //d2 = DX * DX + DY * DY + 0.00001f;
            d2 = DX * DX + DY * DY;

            if (d2 > 0)
            {
                dist = (float)Math.Sqrt(d2);

                if (factor != 0)
                {
                    F = factor * mass * reducedMass / dist;
                    //F = factor * mass * reducedMass / d2;

                    dx += F * DX;
                    dy += F * DY;
                }
            }
            else
            {
                ;
            }

            if (--cnt == 0)
            {
                cnt = 33 + rand.Next(133);
                A = myUtils.randFloat(rand) * maxOpacity;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override void Show()
        {
            if (doUseTrails && trail != null)
                trail.update(x, y);

            x += dx;
            y += dy;

            if (doUseTrails && trail != null)
                trail.Show(R, G, B, 0.33f);

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
            //Glfw.SwapInterval(0);

            if (true)
            {
                while (list.Count < N)
                {
                    list.Add(new myObj_0231());
                }
            }

            // 
            if (false)
            {
                var obj0 = list[0] as myObj_0231;

                totalMass -= obj0.mass;

                //calculateTotalCenterOfMass();

                obj0.dx = 0;
                obj0.dy = 0;

                //obj0.x = centerX; obj0.y = centerY;

                obj0.x = gl_x0; obj0.y = gl_y0;

                obj0.mass = totalMass * 0.1f;
                obj0.size = 15;

                totalMass += obj0.mass;
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
                    if (doClearBuffer)
                    {
                        glClear(GL_COLOR_BUFFER_BIT);
                    }
                    else
                    {
                        grad.Draw();
                    }
                }

                {
                    calculateTotalCenterOfMass();

                    if (doUseTrails)
                        myPrimitive._LineInst.ResetBuffer();

                    inst.ResetBuffer();

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0231;
                        obj.Move();
                    }

                    for (int i = 0; i != Count; i++)
                    {
                        var obj = list[i] as myObj_0231;
                        obj.Show();
                    }

                    if (doUseTrails)
                        myPrimitive._LineInst.Draw();

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

                myPrimitive._Ellipse.SetColor(1, 1, 1, 1);
                myPrimitive._Ellipse.Draw(centerX, centerY, 13, 13, true);

                System.Threading.Thread.Sleep(renderDelay);

                continue;

                if (list.Count < N)
                {
                    int a = rand.Next(111);

                    while(list.Count < N && --a > 0)
                        list.Add(new myObj_0231());
                }
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N, 0);

            myPrimitive.init_Ellipse();

            if (doUseTrails)
                myPrimitive.init_LineInst(nTrailsQty * (nTrailLengthMin + nTrailLengthMax));

            grad = new myScreenGradient();
            grad.SetRandomColors(rand, 0.2f);

            if (doClearBuffer == false)
                grad.SetOpacity(0.2f);

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void calculateTotalCenterOfMass()
        {
            weightedSumX = 0;
            weightedSumY = 0;

            foreach (myObj_0231 obj in list)
            {
                weightedSumX += obj.mass * obj.x;
                weightedSumY += obj.mass * obj.y;
            }

            centerX = weightedSumX / totalMass;
            centerY = weightedSumY / totalMass;
        }

        // ---------------------------------------------------------------------------------------------------------------

    }
};
