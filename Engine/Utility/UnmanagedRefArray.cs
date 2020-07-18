using System;
using System.Runtime.InteropServices;

namespace SE.Utility
{
    public unsafe ref struct UnmanagedRefArray<T> where T : unmanaged
    {
        public T* Data;

        public UnmanagedRefArray(int size)
        {
            Data = (T*) Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)) * size);
        }

        public T* this[int i] 
            => &Data[i];

        public void Dispose()
        {
            if (Data != null) {
                Marshal.FreeHGlobal((IntPtr) Data);
                Data = null;
            }
        }
    }
}
