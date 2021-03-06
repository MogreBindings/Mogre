/*
-----------------------------------------------------------------------------
This source file is part of OGRE
    (Object-oriented Graphics Rendering Engine) ported to C++/CLI
For the latest info, see http://www.ogre3d.org/

Copyright (c) 2000-2011 Torus Knot Software Ltd

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
-----------------------------------------------------------------------------
*/
#pragma once

#pragma managed(push, off)
#include "OgreRay.h"
#pragma managed(pop)
#include "Prerequisites.h"
#include "Custom\MogreMath.h"
#include "Custom\MogreVector3.h"
#include "Custom\MogrePlane.h"
#include "MogreSphere.h"
#include "MogrePlaneBoundedVolume.h"

namespace Mogre
{
    /** Representation of a ray in space, ie a line with an origin and direction. */
	[Serializable]
    public value class Ray
    {
	protected:
        Vector3 mOrigin;
        Vector3 mDirection;
    public:
		DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( Ray )

        Ray(Vector3 origin, Vector3 direction)
            :mOrigin(origin), mDirection(direction) {}

		property Vector3 Origin
		{
			/** Sets the origin of the ray. */
			void set(Vector3 origin) {mOrigin = origin;} 
			/** Gets the origin of the ray. */
			Vector3 get() {return mOrigin;} 
		}

		property Vector3 Direction
		{
			/** Sets the direction of the ray. */
			void set(Vector3 dir) {mDirection = dir;} 
			/** Gets the direction of the ray. */
			Vector3 get() {return mDirection;} 
		}

		/** Gets the position of a point t units along the ray. */
		Vector3 GetPoint(Real t) { 
			return Vector3(mOrigin + (mDirection * t));
		}
		
		/** Gets the position of a point t units along the ray. */
		static Vector3 operator*(Ray r, Real t) { 
			return r.GetPoint(t);
		};

		/** Tests whether this ray Intersects the given plane. 
		@returns A pair structure where the first element indicates whether
			an intersection occurs, and if true, the second element will
			indicate the distance along the ray at which it Intersects. 
			This can be converted to a point in space by calling getPoint().
		*/
		Pair<bool, Real> Intersects(Plane p)
		{
			return Math::Intersects(*this, p);
		}
        /** Tests whether this ray Intersects the given plane bounded volume. 
        @returns A pair structure where the first element indicates whether
        an intersection occurs, and if true, the second element will
        indicate the distance along the ray at which it Intersects. 
        This can be converted to a point in space by calling getPoint().
        */
        Pair<bool, Real> Intersects(PlaneBoundedVolume^ p)
        {
			return Math::Intersects(*this, p->planes, p->outside == Plane::Side::POSITIVE_SIDE);
        }
		/** Tests whether this ray Intersects the given sphere. 
		@returns A pair structure where the first element indicates whether
			an intersection occurs, and if true, the second element will
			indicate the distance along the ray at which it Intersects. 
			This can be converted to a point in space by calling getPoint().
		*/
		Pair<bool, Real> Intersects(Sphere s)
		{
			return Math::Intersects(*this, s);
		}
		/** Tests whether this ray Intersects the given box. 
		@returns A pair structure where the first element indicates whether
			an intersection occurs, and if true, the second element will
			indicate the distance along the ray at which it Intersects. 
			This can be converted to a point in space by calling getPoint().
		*/
		Pair<bool, Real> Intersects(AxisAlignedBox^ box)
		{
			return Math::Intersects(*this, box);
		}

    };
}