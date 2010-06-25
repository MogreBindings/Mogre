#pragma once

#include "OgreCommon.h"


namespace Mogre
{
	value class Box;
	value class PixelBox;
}

#include "MogrePixelFormat.h"

namespace Mogre
{
    /** Structure used to define a box in a 3-D integer space.
     	Note that the left, top, and front edges are included but the right, 
     	bottom and top ones are not.
     */
	[Serializable]
    public value class Box
    {
	public:
		DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( Box )

		size_t left, top, right, bottom, front, back;
        /** Define a box from left, top, right and bottom coordinates
        	This box will have depth one (front=0 and back=1).
        	@param	l	x value of left edge
        	@param	t	y value of top edge
        	@param	r	x value of right edge
        	@param	b	y value of bottom edge
        	@note Note that the left, top, and front edges are included 
	           	but the right, bottom and top ones are not.
        */
        Box( size_t l, size_t t, size_t r, size_t b ):
            left(l),
            top(t),   
            right(r),
            bottom(b),
            front(0),
            back(1)
        {
      		assert(right >= left && bottom >= top && back >= front);
        }
        /** Define a box from left, top, front, right, bottom and back
        	coordinates.
        	@param	l	x value of left edge
        	@param	t	y value of top edge
        	@param  ff  z value of front edge
        	@param	r	x value of right edge
        	@param	b	y value of bottom edge
        	@param  bb  z value of back edge
        	@note Note that the left, top, and front edges are included 
	           	but the right, bottom and top ones are not.
        */
        Box( size_t l, size_t t, size_t ff, size_t r, size_t b, size_t bb ):
            left(l),
            top(t),   
            right(r),
            bottom(b),
            front(ff),
            back(bb)
        {
      		assert(right >= left && bottom >= top && back >= front);
        }
        
        /// Return true if the other box is a part of this one
        bool Contains(Box def)
        {
        	return (def.left >= left && def.top >= top && def.front >= front &&
				def.right <= right && def.bottom <= bottom && def.back <= back);
        }
        
        /// Get the width of this box
		property size_t Width
		{
			size_t get() { return right-left; }
		}

        /// Get the height of this box
		property size_t Height
		{
			size_t get() { return bottom-top; }
		}

        /// Get the depth of this box
		property size_t Depth
		{
			size_t get() { return back-front; }
		}
    };


	/** A primitive describing a volume (3D), image (2D) or line (1D) of pixels in memory.
     	In case of a rectangle, depth must be 1. 
     	Pixels are stored as a succession of "depth" slices, each containing "height" rows of 
     	"width" pixels.
    */
    public value class PixelBox
	{
    public:
		DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( PixelBox )
		
		Box box;

		/** Constructor providing extents in the form of a Box object. This constructor
    		assumes the pixel data is laid out consecutively in memory. (this
    		means row after row, slice after slice, with no space in between)
    		@param extents	    Extents of the region defined by data
    		@param pixelFormat	Format of this buffer
    		@param pixelData	Pointer to the actual data
    	*/
		PixelBox(Box extents, PixelFormat pixelFormat, IntPtr pixelData):
			box(extents), data(pixelData), format(pixelFormat)
		{
			setConsecutive();
		}

		PixelBox(Box extents, PixelFormat pixelFormat):
			box(extents), data(0), format(pixelFormat)
		{
			setConsecutive();
		}
    	/** Constructor providing width, height and depth. This constructor
    		assumes the pixel data is laid out consecutively in memory. (this
    		means row after row, slice after slice, with no space in between)
    		@param width	    Width of the region
    		@param height	    Height of the region
    		@param depth	    Depth of the region
    		@param pixelFormat	Format of this buffer
    		@param pixelData    Pointer to the actual data
    	*/
    	PixelBox(size_t width, size_t height, size_t depth, PixelFormat pixelFormat, IntPtr pixelData):
    		box( Box(0, 0, 0, width, height, depth) ),
    		data(pixelData), format(pixelFormat)
    	{
    		setConsecutive();
    	}
    	
    	PixelBox(size_t width, size_t height, size_t depth, PixelFormat pixelFormat):
    		box( Box(0, 0, 0, width, height, depth) ),
    		data(0), format(pixelFormat)
    	{
    		setConsecutive();
    	}

		/// The data pointer 
        IntPtr data;
        /// The pixel format 
        PixelFormat format;
        /** Number of elements between the leftmost pixel of one row and the left
         	pixel of the next. This value must always be equal to getWidth() (consecutive) 
			for compressed formats.
        */
        size_t rowPitch;
        /** Number of elements between the top left pixel of one (depth) slice and 
         	the top left pixel of the next. This can be a negative value. Must be a multiple of
         	rowPitch. This value must always be equal to getWidth()*getHeight() (consecutive) 
			for compressed formats.
        */
        size_t slicePitch;
        
        /** Set the rowPitch and slicePitch so that the buffer is laid out consecutive 
         	in memory.
        */        
        void setConsecutive()
        {
            rowPitch = box.Width;
            slicePitch = box.Width*box.Height;
        }
        /**	Get the number of elements between one past the rightmost pixel of 
         	one row and the leftmost pixel of the next row. (IE this is zero if rows
         	are consecutive).
        */
		property size_t RowSkip
		{
			size_t get() { return rowPitch - box.Width; }
		}

        /** Get the number of elements between one past the right bottom pixel of
         	one slice and the left top pixel of the next slice. (IE this is zero if slices
         	are consecutive).
        */
		property size_t SliceSkip
		{
			size_t get() { return slicePitch - (box.Height * rowPitch); }
		}

        /** Return whether this buffer is laid out consecutive in memory (ie the pitches
         	are equal to the dimensions)
        */
		property bool IsConsecutive
		{
			bool get()
			{ 
				return rowPitch == box.Width && slicePitch == box.Width*box.Height; 
			}
		}
        /** Return the size (in bytes) this image would take if it was
        	laid out consecutive in memory
      	*/
      	size_t GetConsecutiveSize();

		/** Return a subvolume of this PixelBox.
      		@param def	Defines the bounds of the subregion to return
      		@returns	A pixel box describing the region and the data in it
      		@remarks	This function does not copy any data, it just returns
      			a PixelBox object with a data pointer pointing somewhere inside 
      			the data of object.
      		@throws	Exception(ERR_INVALIDPARAMS) if def is not fully contained
      	*/
      	PixelBox GetSubVolume(Box def);
    };
}