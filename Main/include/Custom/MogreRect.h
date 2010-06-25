#pragma once

#include "OgreCommon.h"

namespace Mogre
{

	[Serializable]
	public value class FloatRect
	{
	public:
		DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( FloatRect )

          float left, top, right, bottom;

          FloatRect( float l, float t, float r, float b )
            : left( l ), top( t ), right( r ), bottom( b )
          {
          }
		  property float Width
		  {
			  float get()
			  {
				return right - left;
			  }
		  }
		  property float Height
		  {
			  float get()
			  {
				return bottom - top;
			  }
		  }
	};

	[Serializable]
	public value class RealRect
	{
	public:
		DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( RealRect )

		Mogre::Real left, top, right, bottom;

		RealRect( float l, float t, float r, float b )
			: left( l ), top( t ), right( r ), bottom( b )
		{
		}
		property Mogre::Real Width
		{
			Mogre::Real get()
			{
				return right - left;
			}
		}
		property Mogre::Real Height
		{
			Mogre::Real get()
			{
				return bottom - top;
			}
		}
	};

	[Serializable]
	public value class Rect
	{
	public:
		DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( Rect )

          long left, top, right, bottom;

          Rect( long l, long t, long r, long b )
            : left( l ), top( t ), right( r ), bottom( b )
          {
          }
		  property long Width
		  {
			  long get()
			  {
				return right - left;
			  }
		  }
		  property long Height
		  {
			  long get()
			  {
				return bottom - top;
			  }
		  }
	};



}