using UnityEngine;
using Unity.Mathematics;
using System;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using System.Numerics;

namespace Resources
{
    public static class Utils
    {
        public static int GetThreadGroupsNum(int threadsNum, int threadSize)
        {
            int threadGroupsNum = (int)Math.Ceiling((float)threadsNum / threadSize);
            return threadGroupsNum;
        }
        public static int2 GetThreadGroupsNum(int2 threadsNum, int threadSize)
        {
            int threadGroupsNumX = GetThreadGroupsNum(threadsNum.x, threadSize);
            int threadGroupsNumY = GetThreadGroupsNum(threadsNum.y, threadSize);
            return new(threadGroupsNumX, threadGroupsNumY);
        }
        public static int3 GetThreadGroupsNum(int3 threadsNum, int threadSize)
        {
            int threadGroupsNumX = GetThreadGroupsNum(threadsNum.x, threadSize);
            int threadGroupsNumY = GetThreadGroupsNum(threadsNum.y, threadSize);
            int threadGroupsNumZ = GetThreadGroupsNum(threadsNum.z, threadSize);
            return new(threadGroupsNumX, threadGroupsNumY, threadGroupsNumZ);
        }

        public static bool2 GetMousePressed()
        {
            bool LMousePressed = Input.GetMouseButton(0);
            bool RMousePressed = Input.GetMouseButton(1);

            bool2 MousePressed = new bool2(LMousePressed, RMousePressed);

            return MousePressed;
        }

        public static Vector2 GetMousePosNormalised()
        {
            Vector3 mousePos = Input.mousePosition;
            Vector2 mouseWorldPos = new Vector2(mousePos.x / 3840, mousePos.y / 2160);

            return mouseWorldPos;
        }

        public static float CelciusToKelvin(float celciusTemp)
        {
            return 273.15f + celciusTemp;
        }
        
        public static float3 GetParticleSpawnPosition(int pIndex, int maxIndex, int width, int height, int depth)
        {
            float posX = Func.RandFloat((float)5+width/4, (float)3*width/4-5);
            float posY = Func.RandFloat((float)5, (float)height-5);
            float posZ = Func.RandFloat((float)5, (float)depth-5);

            return new float3(posX, posY, posZ);
        }
    }

    public static class Func
    {
        public static void Log2(ref int a, bool doCeil = false)
        {
            double logValue = Math.Log(a, 2);
            a = doCeil ? (int)Math.Ceiling(logValue) : (int)logValue;
        }
        
        public static int Log2(int a, bool doCeil = false)
        {
            double logValue = Math.Log(a, 2);
            return doCeil ? (int)Math.Ceiling(logValue) : (int)logValue;
        }

        public static int Pow2(int a)
        {
            double powValue = Mathf.Pow(2, a);
            return (int)powValue;
        }

        /// <returns>returns a random integer between a min value (INCLUSIVE) and a max value (INCLUSIVE)</returns>
        public static int RandInt(int min, int max)
        {
            return UnityEngine.Random.Range(min, max+1);
        }

        /// <returns>returns a random float between a min value (INCLUSIVE) and a max value (INCLUSIVE)</returns>
        public static float RandFloat(float min, float max)
        {
            return UnityEngine.Random.Range(min, max+1);
        }

        public static int NextPow2(int a)
        {
            int nextPow2 = 1;
            while (nextPow2 < a)
            {
                nextPow2 *= 2;
            }
            return nextPow2;
        }

        public static void NextPow2(ref int a)
        {
            int nextPow2 = 1;
            while (nextPow2 < a)
            {
                nextPow2 *= 2;
            }
            a = nextPow2;
        }

        /// <summary>Calculates the logarithm (base 2) of the next power of 2</summary>
        public static int NextLog2(int a)
        {
            return Log2(NextPow2(a));
        }

        /// <summary>Calculates the logarithm (base 2) of the next power of 2</summary>
        public static void NextLog2(ref int a)
        {
            a = Log2(NextPow2(a));
        }

        /// <summary>Calculates the next integer divisible by a divisor</summary>
        public static void NextDivisible(ref int a, int divisor)
        {
            a = Mathf.CeilToInt(a / divisor) * divisor;
        }
        /// <summary>Calculates the next integer divisible by a divisor</summary>
        public static int NextDivisible(int a, int divisor)
        {
            return Mathf.CeilToInt(a / divisor) * divisor;
        }
    }
}