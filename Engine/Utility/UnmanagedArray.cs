using System;
using System.Runtime.InteropServices;

namespace SE.Utility
{
    public unsafe class UnmanagedArray<T> : IDisposable where T : unmanaged
    {
        public T* Data;

        public UnmanagedArray(int size)
        {
            Data = (T*)Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)) * size);
        }

        public T* this[int i]
            => &Data[i];

        ~UnmanagedArray()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (Data != null) {
                Marshal.FreeHGlobal((IntPtr)Data);
                Data = null;
            }
        }
    }
}
