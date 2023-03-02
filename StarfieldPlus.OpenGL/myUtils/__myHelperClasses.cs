// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace my
{
    // ===================================================================================================================
    // ===================================================================================================================

    /*
        This class wraps around an array of pointers;
        Each pointer in the array references either:
            - default value
            - single external variable

        How to Use (in pseudocode):

            float t = 0;

            unsafe
            {
                fixed (float *ptr_t = &t)
                {
                    arrPtr = new ptrArray(ptr_t, 2, 1.0f);
                }
            }

            // Later:

            arrPtr.Set(0, random.Bool);
            arrPtr.Set(1, random.Bool);

            while(1)
            {
                float func = X * arrPtr.Get(0) * Sin(Y * arrPtr.Get(1))
                t += 0.01f;
            }

            Here, depending on the values in the array, the func will resolve into one of the following:
                a) func = X * Sin(Y)
                b) func = X * Sin(Y * t)
                c) func = X * t * Sin(Y)
                d) func = X * t * Sin(Y * t)
    */

    public unsafe class ptrArray
    {
        private int      _N = 0;                // number of elements in the array
        private float*   _ptrMain = null;       // pointer to the main referenced value
        private float*[] _data = null;          // array of pointers
        private float    _default = 1.0f;       // default value

        public ptrArray(float* externalValue, int numElements, float defaultValue = 1.0f)
        {
            _N = numElements;

            // Set pointer to the external variable
            _ptrMain = externalValue;

            // Create array
            _data = new float*[_N];

            // Set default value
            _default = defaultValue;

            Reset();
        }

        // Set the value in the array: make it reference the external variable OR the default value
        public void Set(int i, bool value)
        {
            if (value)
            {
                _data[i] = _ptrMain;
            }
            else
            {
                fixed (float* ptr = &_default)
                {
                    _data[i] = ptr;
                }
            }
        }

        // Get value from the array
        public float Get(int i)
        {
            return *(_data[i]);
        }

        // Make every pointer in the array reference the default value
        public void Reset()
        {
            fixed (float* ptrDefault = &_default)
            {
                for (int i = 0; i < _N; i++)
                    _data[i] = ptrDefault;
            }
        }
    };

    // ===================================================================================================================
    // ===================================================================================================================

};
