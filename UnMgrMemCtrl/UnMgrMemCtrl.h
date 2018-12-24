#pragma once


#pragma unmanaged
#include <string.h>

#pragma managed
using namespace System::Windows;
using namespace System::Collections::Generic;

namespace UnmanagedMemory {
	public ref class MemoryCtrl
	{
	private:
		static int refCount;
	public:
		static property int RefCount {
			int get() { return refCount; }
		}

		MemoryCtrl()
		{
			refCount++;
		}

		MemoryCtrl(MemoryCtrl^ test)
		{
			refCount++;
		}

		// Finalizer. Definitely called before Garbage Collection,
		// but not automatically called on explicit Dispose().
		// May be called multiple times.
		!MemoryCtrl()
		{
			refCount--;
		}

		// Destructor. Called on explicit Dispose() only.
		~MemoryCtrl()
		{
			this->!MemoryCtrl();
		}

	public:
		static int MemoryCopy(System::IntPtr Dst, System::Int32 sizeInBytes, System::IntPtr Src)
		{
			errno_t iErr = memcpy_s((void *)Dst.ToInt32(), sizeInBytes, (void *)Src.ToInt32(),sizeInBytes);
			return iErr;
		}
	};
}