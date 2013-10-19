using System;
using System.Collections.Generic;
using Rainmeter.Plugin;

namespace Rainmeter.API
{
    public static class Plugin
    {
        private static readonly Dictionary<uint, Measure> Measures = new Dictionary<uint, Measure>();

        [DllExport]
        public static unsafe void Initialize(void** data, void* rm)
        {
            var id = (uint) *data;
            Measures.Add(id, new Measure());
        }

        [DllExport]
        public static unsafe void Finalize(void* data)
        {
            var id = (uint) data;
            Measures.Remove(id);
        }

        [DllExport]
        public static unsafe void Reload(void* data, void* rm, double* maxValue)
        {
            var id = (uint) data;
            Measures[id].Reload(new RainmeterAPI((IntPtr) rm), ref *maxValue);
        }

        [DllExport]
        public static unsafe double Update(void* data)
        {
            var id = (uint) data;
            return Measures[id].Update();
        }

        [DllExport]
        public static unsafe char* GetString(void* data)
        {
            var id = (uint) data;
            fixed (char* s = Measures[id].GetString()) return s;
        }

        [DllExport]
        public static unsafe void ExecuteBang(void* data, char* args)
        {
            Measure.ExecuteBang(new string(args));
        }
    }
}