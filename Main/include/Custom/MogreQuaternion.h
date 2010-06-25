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
// NOTE THAT THIS FILE IS BASED ON MATERIAL FROM:

// Magic Software, Inc.
// http://www.geometrictools.com/
// Copyright (c) 2000, All Rights Reserved
//
// Source code from Magic Software is supplied under the terms of a license
// agreement and may not be copied or disclosed except in accordance with the
// terms of that agreement.  The various license agreements may be found at
// the Magic Software web site.  This file is subject to the license
//
// FREE SOURCE CODE
// http://www.geometrictools.com/License/WildMagic3License.pdf
#pragma once

#include "OgreQuaternion.h"
#include "Marshalling.h"
#include "Prerequisites.h"
#include "Custom\MogreMath.h"
#include "Custom\MogreVector3.h"

namespace Mogre
{
	ref class Matrix3;

    /** Implementation of a Quaternion, i.e. a rotation around an axis.
    */
	[Serializable]
	public value class Quaternion : IEquatable<Quaternion>
    {
    public:
		DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( Quaternion )

        inline Quaternion (
            Real fW,
            Real fX, Real fY, Real fZ)
		{
			w = fW;
			x = fX;
			y = fY;
			z = fZ;
		}
        inline Quaternion ( Real fW )
		{
			w = fW;
			x = 0;
			y = 0;
			z = 0;
		}
        /// Construct a quaternion from a rotation matrix
        inline Quaternion(Matrix3^ rot)
        {
            this->FromRotationMatrix(rot);
        }
        /// Construct a quaternion from an angle/axis
        inline Quaternion(Radian rfAngle, Vector3 rkAxis)
        {
            this->FromAngleAxis(rfAngle, rkAxis);
        }
        /// Construct a quaternion from 3 orthonormal local axes
        inline Quaternion(Vector3 xaxis, Vector3 yaxis, Vector3 zaxis)
        {
            this->FromAxes(xaxis, yaxis, zaxis);
        }
        /// Construct a quaternion from 3 orthonormal local axes
        inline Quaternion(array<Vector3>^ akAxis)
        {
            this->FromAxes(akAxis);
        }
		/// Construct a quaternion from 4 manual w/x/y/z values
		inline Quaternion(array<Real>^ valptr)
		{
			w = valptr[0];
			x = valptr[1];
			y = valptr[2];
			z = valptr[3];
		}

        void FromRotationMatrix (Matrix3^ kRot);
        Matrix3^ ToRotationMatrix ();
        void FromAngleAxis (Radian rfAngle, Vector3 rkAxis);
        void ToAngleAxis ([Out] Radian% rfAngle, [Out] Vector3% rkAxis);
        inline void ToAngleAxis ([Out] Degree% dAngle, [Out] Vector3% rkAxis) {
            Radian rAngle;
            ToAngleAxis ( rAngle, rkAxis );
            dAngle = rAngle;
        }
        void FromAxes (array<Vector3>^ akAxis);
        void FromAxes (Vector3 xAxis, Vector3 yAxis, Vector3 zAxis);
        void ToAxes ([Out] array<Vector3>^% akAxis);
        void ToAxes ([Out] Vector3% xAxis, [Out] Vector3% yAxis, [Out] Vector3% zAxis);
        /// Get the local x-axis
		property Vector3 XAxis
		{
			Vector3 get();
		}
        /// Get the local y-axis
		property Vector3 YAxis
		{
			Vector3 get();
		}
        /// Get the local z-axis
		property Vector3 ZAxis
		{
			Vector3 get();
		}

		static Quaternion operator+ (Quaternion lkQ, Quaternion rkQ);
        static Quaternion operator- (Quaternion lkQ, Quaternion rkQ);
        static Quaternion operator* (Quaternion lkQ, Quaternion rkQ);
        static Quaternion operator* (Quaternion lkQ, Real fScalar);
        static Quaternion operator* (Real fScalar, Quaternion rkQ);
        static Quaternion operator- (Quaternion rkQ);
        inline static bool operator== (Quaternion lhs, Quaternion rhs)
		{
			return (rhs.x == lhs.x) && (rhs.y == lhs.y) &&
				(rhs.z == lhs.z) && (rhs.w == lhs.w);
		}
        inline static bool operator!= (Quaternion lhs, Quaternion rhs)
		{
			return !(lhs == rhs);
		}

		virtual bool Equals(Quaternion other) { return *this == other; }

		// functions of a quaternion
        Real Dot (Quaternion rkQ);  // dot product
		property Real Norm  // squared-length
		{
			Real get();
		}
        /// Normalises this quaternion, and returns the previous length
        Real Normalise(); 
		Quaternion Inverse();  // apply to non-zero quaternion
		Quaternion UnitInverse();  // apply to unit-length quaternion
		Quaternion Exp();
		Quaternion Log();

        // rotation of a vector by a quaternion
        static Vector3 operator* (Quaternion lquat, Vector3 rkVector);

   		/// Calculate the local roll element of this quaternion
		property Radian Roll
		{
			Radian get();
		}
   		/// Calculate the local pitch element of this quaternion
		property Radian Pitch
		{
			Radian get();
		}
   		/// Calculate the local yaw element of this quaternion
		property Radian Yaw
		{
			Radian get();
		}
		/// Equality with tolerance (tolerance is max angle difference)
		bool Equals(Quaternion rhs, Radian tolerance);
		
	    // spherical linear interpolation
        static Quaternion Slerp (Real fT, Quaternion rkP,
            Quaternion rkQ, bool shortestPath);
        static Quaternion Slerp (Real fT, Quaternion rkP,
            Quaternion rkQ)
		{
			return Slerp(fT, rkP, rkQ, false);
		}

        static Quaternion SlerpExtraSpins (Real fT,
            Quaternion rkP, Quaternion rkQ,
            int iExtraSpins);

        // setup for spherical quadratic interpolation
        static void Intermediate (Quaternion rkQ0,
            Quaternion rkQ1, Quaternion rkQ2,
            Quaternion& rka, Quaternion& rkB);

        // spherical quadratic interpolation
        static Quaternion Squad (Real fT, Quaternion rkP,
            Quaternion rkA, Quaternion rkB,
            Quaternion rkQ, bool shortestPath);
        static Quaternion Squad (Real fT, Quaternion rkP,
            Quaternion rkA, Quaternion rkB,
            Quaternion rkQ)
		{
			return Squad(fT, rkP, rkA, rkB, rkQ, false);
		}

        // normalised linear interpolation - faster but less accurate (non-constant rotation velocity)
        static Quaternion Nlerp(Real fT, Quaternion rkP, 
            Quaternion rkQ, bool shortestPath);
        static Quaternion Nlerp(Real fT, Quaternion rkP, 
            Quaternion rkQ)
		{
			return Nlerp(fT, rkP, rkQ, false);
		}

        // cutoff for sine near zero
        static const Real ms_fEpsilon = 1e-03;

        // special values
        static const Quaternion ZERO = Quaternion(0.0,0.0,0.0,0.0);
        static const Quaternion IDENTITY = Quaternion(1.0,0.0,0.0,0.0);

		Real w, x, y, z;

        /** Function for writing to a stream. Outputs "Quaternion(w, x, y, z)" with w,x,y,z
            being the member values of the quaternion.
        */
		virtual System::String^ ToString() override
        {
			return System::String::Format("Quaternion({0}, {1}, {2}, {3})", w, x, y, z);
        }
    };
}