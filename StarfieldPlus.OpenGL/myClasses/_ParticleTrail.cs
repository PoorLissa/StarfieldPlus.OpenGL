using GLFW;
using static OpenGL.GL;
using System;
using System.Collections.Generic;
using System.Drawing;



namespace my
{
    public class myParticleTrail
    {
        private int _index;
        private int _N;
        private float[] _arr = null;

        // ---------------------------------------------------------------------------------------------------------------

        public myParticleTrail(int n)
        {
            _N = n;
            _arr = new float[_N * 2];
            _index = 0;
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Initialize all the trail array with the same values
        public myParticleTrail(int n, float x, float y)
        {
            _N = n;
            _arr = new float[_N * 2];
            _index = 0;

            for (int i = 0; i < _N * 2; i += 2)
            {
                _arr[i + 0] = x;
                _arr[i + 1] = y;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        public void update(float x, float y)
        {
            _arr[_index++] = x;
            _arr[_index++] = y;

            if (_index == _N * 2)
            {
                _index = 0;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        public void reset(float x, float y)
        {
            _index = 0;

            for (int i = 0; i < _N * 2; i += 2)
            {
                _arr[i + 0] = x;
                _arr[i + 1] = y;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        public void getXY(int i, ref float x, ref float y)
        {
            int idx = _index - 2 - 2 * i;
            
            if (idx < 0)
                idx += 2 * _N;

            x = _arr[idx + 0];
            y = _arr[idx + 1];
        }

        // ---------------------------------------------------------------------------------------------------------------

        public int getIndex()
        {
            return _index;
        }

        // ---------------------------------------------------------------------------------------------------------------

        unsafe float* getArray()
        {
            fixed (float *ptr = &_arr[0])
            {
                return ptr;
            }
        }

        // ---------------------------------------------------------------------------------------------------------------

        // Draw the whole trail;
        // Relies on myPrimitive._LineInst, which must be initialized to the proper size
        public void Show(float R, float G, float B, float A, float da)
        {
            float x1 = 0, y1 = 0, x2 = 0, y2 = 0;
            int i = 0;

            // Get the first pair of coordinates
            getXY(i++, ref x1, ref y1);

            for (; i < _N; i++)
            {
                // Get the second pair of coordinates
                getXY(i, ref x2, ref y2);

                myPrimitive._LineInst.setInstanceCoords(x1, y1, x2, y2);
                myPrimitive._LineInst.setInstanceColor(R, G, B, A);

                // Shift the first pair 1 position towards the end
                x1 = x2;
                y1 = y2;

                A -= da;
            }

            return;
        }

        // ---------------------------------------------------------------------------------------------------------------

    };
};
