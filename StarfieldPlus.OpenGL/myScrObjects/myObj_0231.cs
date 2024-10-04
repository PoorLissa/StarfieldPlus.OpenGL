using GLFW;
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
        public static int Priority => 999910;
		public static System.Type Type => typeof(myObj_0231);

        private int cnt;
        private float x, y, dx, dy;
        private float mass, size, A, R, G, B, angle;

        private static int N = 0, shape = 0;
        private static bool doFillShapes = true, doUseRandomMass = false;
        private static float dimAlpha = 0.05f;

        private static float totalMass = 0;
        private static float weightedSumX = 0;
        private static float weightedSumY = 0;
        private static float centerX = 0;
        private static float centerY = 0;

        // ---------------------------------------------------------------------------------------------------------------

        public myObj_0231()
        {
            generateNew();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time global initialization
        protected override void initGlobal()
        {
            colorPicker = new myColorPicker(gl_Width, gl_Height);
            list = new List<myObject>();

            shape = rand.Next(5);

            initLocal();
        }

        // ---------------------------------------------------------------------------------------------------------------

        // One-time local initialization
        private void initLocal()
        {
            N = (N == 0) ? 100 + rand.Next(100) : N;
            N = 111111;

            doUseRandomMass = myUtils.randomBool(rand);
            doUseRandomMass = true;
            renderDelay = 3;
        }

        // ---------------------------------------------------------------------------------------------------------------

        protected override string CollectCurrentInfo(ref int width, ref int height)
        {
            string str = $"Obj = {Type}\n\n"                 	+
                            myUtils.strCountOf(list.Count, N)   +
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
            cnt = 33 + rand.Next(133);

            x = rand.Next(2 * gl_Width) - gl_Width/2;
            y = rand.Next(2 * gl_Width) - gl_Width/2 - (gl_Width - gl_Height)/2;

            dx = dy = 0;

            if (!true)
            {
                dx = myUtils.randomSign(rand) * (float)rand.NextDouble() * 7;
                dy = myUtils.randomSign(rand) * (float)rand.NextDouble() * 7;
            }

            if (true)
            {
                if (myUtils.randomChance(rand, 1, 333))
                {
                    dx = myUtils.randomSign(rand) * (float)rand.NextDouble() * 27;
                    dy = myUtils.randomSign(rand) * (float)rand.NextDouble() * 27;
                }
            }

            if (doUseRandomMass)
            {
                mass = rand.Next(9999) + 100;

                if (myUtils.randomChance(rand, 1, 123))
                    mass = rand.Next(933000) + 100;

                mass = 5000;
            }
            else
            {
                mass = 500;
            }

            if (id != uint.MaxValue)
                totalMass += mass;

            size = mass < 500 ? 1 : mass / 25000;

            if (size < 0)
                size = 2;

            angle = 0;

            A = 0.5f;
            R = (float)rand.NextDouble();
            G = (float)rand.NextDouble();
            B = (float)rand.NextDouble();

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

            factor = 0.000001f;
            factor = 0.0000000001f / N;

            DX = thisCenterX - x;
            DY = thisCenterY - y;
            d2 = DX * DX + DY * DY + 0.00001f;
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
                A = myUtils.randFloat(rand);
            }

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
            //Glfw.SwapInterval(0);

            while (list.Count < N)
            {
                list.Add(new myObj_0231());
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
                        //grad.Draw();
                    }
                }

                {
                    calculateTotalCenterOfMass();

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
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

        private void initShapes()
        {
            myPrimitive.init_ScrDimmer();
            base.initShapes(shape, N, 0);

            myPrimitive.init_Ellipse();

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
