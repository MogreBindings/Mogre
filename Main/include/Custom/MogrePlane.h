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
// Original free version by:
// Magic Software, Inc.
// http://www.geometrictools.com/
// Copyright (c) 2000, All Rights Reserved

#pragma once

#pragma managed(push, off)
#include "OgrePlane.h"
#pragma managed(pop)
#include "Custom\MogreVector3.h"

namespace Mogre
{
    /** Defines a plane in 3D space.
        @remarks
            A plane is defined in 3D space by the equation
            Ax + By + Cz + D = 0
        @par
            This equates to a vector (the normal of the plane, whose x, y
            and z components equate to the coefficients A, B and C
            respectively), and a constant (D) which is the distance along
            the normal you have to go to move the plane back to the origin.
     */
	[Serializable]
	public value class Plane : IEquatable<Plane>
    {
	public:
		DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( Plane )

        /** Construct a plane through a normal, and a distance to move the plane along the normal.*/
        Plane (Vector3 rkNormal, Real fConstant);
        Plane (Vector3 rkNormal, Vector3 rkPoint);
        Plane (Vector3 rkPoint0, Vector3 rkPoint1,
            Vector3 rkPoint2);

        /** The "positive side" of the plane is the half space to which the
            plane normal points. The "negative side" is the other half
            space. The flag "no side" indicates the plane itself.
        */
        enum class Side
        {
			NO_SIDE = Ogre::Plane::NO_SIDE,
            POSITIVE_SIDE = Ogre::Plane::POSITIVE_SIDE,
            NEGATIVE_SIDE = Ogre::Plane::NEGATIVE_SIDE
        };

        Side GetSide (Vector3 rkPoint);

        /** This is a pseudodistance. The sign of the return value is
            positive if the point is on the positive side of the plane,
            negative if the point is on the negative side, and zero if the
            point is on the plane.
            @par
            The absolute value of the return value is the true distance only
            when the plane normal is a unit length vector.
        */
        Real GetDistance (Vector3 rkPoint);

        /** Redefine this plane based on 3 points. */
        void Redefine(Vector3 rkPoint0, Vector3 rkPoint1,
            Vector3 rkPoint2);

		/** Project a vector onto the plane. 
		@remarks This gives you the element of the input vector that is perpendicular 
			to the normal of the plane. You can get the element which is parallel
			to the normal of the plane by subtracting the result of this method
			from the original vector, since parallel + perpendicular = original.
		@param v The input vector
		*/
		Vector3 ProjectVector(Vector3 v);

		Vector3 normal;
        Real d;
        /// Comparison operator
        static bool operator==(Plane lhs, Plane rhs)
        {
            return (rhs.d == lhs.d && rhs.normal == lhs.normal);
        }

        static bool operator!=(Plane lhs, Plane rhs)
        {
            return (rhs.d != lhs.d || rhs.normal != lhs.normal);
        }

		virtual bool Equals(Plane other) { return *this == other; }

        virtual String^ ToString() override;
    };
}