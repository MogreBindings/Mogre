#include "MogreStableHeaders.h"

namespace Mogre
{
    size_t PixelBox::GetConsecutiveSize()
    { 
        return PixelUtil::GetMemorySize(box.Width, box.Height, box.Depth, format); 
    }

    Mogre::PixelBox PixelBox::GetSubVolume(Box def)
    {
        if(PixelUtil::IsCompressed(format))
        {
            if(def.left == box.left && def.top == box.top && def.front == box.front &&
                def.right == box.right && def.bottom == box.bottom && def.back == box.back)
            {
                // Entire buffer is being queried
                return *this;
            }
            throw gcnew ArgumentException("Cannot return subvolume of compressed PixelBuffer", "def");
        }
        if(!box.Contains(def))
            throw gcnew ArgumentException("Bounds out of range", "def");

        const size_t elemSize = PixelUtil::GetNumElemBytes(format);
        // Calculate new data origin
        PixelBox rval(def, format, (IntPtr)(void*) ( ((uint8*)(void*)data) 
            + ((def.left-box.left)*elemSize)
            + ((def.top-box.top)*rowPitch*elemSize)
            + ((def.front-box.front)*slicePitch*elemSize) )
            );		

        rval.rowPitch = rowPitch;
        rval.slicePitch = slicePitch;
        rval.format = format;

        return rval;
    }
}