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

#include "OgreSphere.h"
#include "Prerequisites.h"
#include "Custom\MogreMath.h"
#include "Custom\MogreVector3.h"
#include "Custom\MogrePlane.h"

namespace Mogre
{
    /** A sphere primitive, mostly used for bounds checking. 
    @remarks
        A sphere in math texts is normally represented by the function
        x^2 + y^2 + z^2 = r^2 (for sphere's centered on the origin). Ogre stores spheres
        simply as a center point and a radius.
    */
	[Serializable]
    public value class Sphere
    {
    protected:
        Real mRadius;
        Vector3 mCenter;
    public:
		DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( Sphere )

        /** Constructor allowing arbitrary spheres. 
            @param center The center point of the sphere.
            @param radius The radius of the sphere.
        */
        Sphere(Vector3 center, Real radius)
            : mRadius(radius), mCenter(center) {}

        /** Returns the radius of the sphere. */
		property Real Radius
		{
			Real get() { return mRadius; }
			void set(Real radius) { mRadius = radius; }
		}

		property Vector3 Center
		{
			/** Returns the center point of the sphere. */
			Vector3 get() { return mCenter; }

			/** Sets the center point of the sphere. */
			void set(Vector3 center) { mCenter = center; }
		}

		/** Returns whether or not this sphere interects another sphere. */
		bool Intersects(Sphere s)
		{
			return (s.mCenter - mCenter).Length <=
				(s.mRadius + mRadius);
		}
		/** Returns whether or not this sphere interects a box. */
		bool Intersects(AxisAlignedBox^ box)
		{
			return Math::Intersects(*this, box);
		}
		/** Returns whether or not this sphere interects a plane. */
		bool Intersects(Plane plane)
		{
			return Math::Intersects(*this, plane);
		}
		/** Returns whether or not this sphere interects a point. */
		bool Intersects(Vector3 v)
		{
			return ((v - mCenter).Length <= mRadius);
		}
        

    };
}