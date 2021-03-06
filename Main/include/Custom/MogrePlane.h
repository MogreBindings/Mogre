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
// This file is based on material originally from:
// Geometric Tools, LLC
// Copyright (c) 1998-2010
// Distributed under the Boost Software License, Version 1.0.
// http://www.boost.org/LICENSE_1_0.txt
// http://www.geometrictools.com/License/Boost/LICENSE_1_0.txt

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
            NEGATIVE_SIDE = Ogre::Plane::NEGATIVE_SIDE,
            BOTH_SIDE = Ogre::Plane::BOTH_SIDE
        };

        Side GetSide (Vector3 rkPoint);

        /**
        Returns the side where the alignedBox is. The flag BOTH_SIDE indicates an intersecting box.
        One corner ON the plane is sufficient to consider the box and the plane intersecting.
        */
        Side GetSide (AxisAlignedBox^ rkBox);

        /** Returns which side of the plane that the given box lies on.
            The box is defined as centre/half-size pairs for effectively.
        @param centre The centre of the box.
        @param halfSize The half-size of the box.
        @returns
            POSITIVE_SIDE if the box complete lies on the "positive side" of the plane,
            NEGATIVE_SIDE if the box complete lies on the "negative side" of the plane,
            and BOTH_SIDE if the box intersects the plane.
        */
        Side GetSide (Vector3 centre, Vector3 halfSize);

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

		/** Redefine this plane based on a normal and a point. */
		void Redefine(Vector3 rkNormal, Vector3 rkPoint);

		/** Project a vector onto the plane. 
		@remarks This gives you the element of the input vector that is perpendicular 
			to the normal of the plane. You can get the element which is parallel
			to the normal of the plane by subtracting the result of this method
			from the original vector, since parallel + perpendicular = original.
		@param v The input vector
		*/
		Vector3 ProjectVector(Vector3 v);

        /** Normalises the plane.
            @remarks
                This method normalises the plane's normal and the length scale of d
                is as well.
            @note
                This function will not crash for zero-sized vectors, but there
                will be no changes made to their components.
            @returns The previous length of the plane's normal.
        */
        Real Normalise();

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