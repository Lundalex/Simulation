using UnityEngine;
using Unity.Mathematics;
using System;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Resources
{
#region Utilities
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
#endregion
#region Functions
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

        /// <summary>Calculates the uv coord for a point projected onto a triangle, with respect to the scale</summary>
        public static Vector2 TriUV(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
        {
            // Calculate barycentric coordinates of point p with respect to triangle ABC
            Vector3 v0 = b - a;
            Vector3 v1 = c - a;
            Vector3 v2 = p - a;

            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);

            float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
            float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            float v = (dot00 * dot12 - dot01 * dot02) * invDenom;
            float w = 1 - u - v;

            Vector2 uvA = new Vector2(0, 0); // UV coordinates for vertex a
            Vector2 uvB = new Vector2(1, 0); // UV coordinates for vertex b
            Vector2 uvC = new Vector2(0, 1); // UV coordinates for vertex c

            Vector2 uv = (u * uvA + v * uvB + w * uvC) % 1.0f;

            return uv;
        }
    }
#endregion
}