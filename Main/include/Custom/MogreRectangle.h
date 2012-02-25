#pragma once

#pragma managed(push, off)
#include "OgreRectangle.h"
#pragma managed(pop)

namespace Mogre
{
	[Serializable]
	public value class Rectangle
	{
	public:
        Real left;
        Real top;
        Real right;
        Real bottom;

        inline bool Inside(Real x, Real y) { return x >= left && x <= right && y >= top && y <= bottom; }

		DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( Rectangle )
	};
}