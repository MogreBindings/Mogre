/*
-----------------------------------------------------------------------------
This source file is part of OGRE
    (Object-oriented Graphics Rendering Engine) ported to C++/CLI
For the latest info, see http://www.ogre3d.org/

Copyright (c) 2000-2005 The OGRE Team
Also see acknowledgements in Readme.html

This program is free software; you can redistribute it and/or modify it under
the terms of the GNU Lesser General Public License as published by the Free Software
Foundation; either version 2 of the License, or (at your option) any later
version.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License along with
this program; if not, write to the Free Software Foundation, Inc., 59 Temple
Place - Suite 330, Boston, MA 02111-1307, USA, or go to
http://www.gnu.org/copyleft/lesser.txt.
-----------------------------------------------------------------------------
*/
#pragma once

#include "OgreRay.h"
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