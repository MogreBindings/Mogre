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

#include "OgreVector4.h"
#include "Custom\MogreVector3.h"

namespace Mogre
{
    /** 4-dimensional homogenous vector.
    */
	[Serializable]
	public value class Vector4 : IEquatable<Vector4>
    {
    public:
		DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( Vector4 )

		Real x, y, z, w;

        inline Vector4( Real fX, Real fY, Real fZ, Real fW )
            : x( fX ), y( fY ), z( fZ ), w( fW)
        {
        }

        inline explicit Vector4( array<Real>^ afCoordinate )
            : x( afCoordinate[0] ),
              y( afCoordinate[1] ),
              z( afCoordinate[2] ),
              w( afCoordinate[3] )
        {
        }

        inline explicit Vector4( array<int>^ afCoordinate )
        {
            x = (Real)afCoordinate[0];
            y = (Real)afCoordinate[1];
            z = (Real)afCoordinate[2];
            w = (Real)afCoordinate[3];
        }

        inline explicit Vector4( Real* const r )
            : x( r[0] ), y( r[1] ), z( r[2] ), w( r[3] )
        {
        }

        inline explicit Vector4( Real scaler )
            : x( scaler )
            , y( scaler )
            , z( scaler )
            , w( scaler )
        {
        }

		property Real default[int]
		{
			inline Real get(int i)
			{
				assert( i < 4 );

				return *(&x+i);
			}

			inline void set(int i, Real value)
			{
				assert( i < 4 );

				*(&x+i) = value;
			}
		}

        inline static bool operator == ( Vector4 lvec, Vector4 rkVector )
        {
            return ( lvec.x == rkVector.x &&
                lvec.y == rkVector.y &&
                lvec.z == rkVector.z &&
                lvec.w == rkVector.w );
        }

        inline static bool operator != ( Vector4 lvec, Vector4 rkVector )
        {
            return ( lvec.x != rkVector.x ||
                lvec.y != rkVector.y ||
                lvec.z != rkVector.z ||
                lvec.w != rkVector.w );
        }

		virtual bool Equals(Vector4 other) { return *this == other; }

        inline static operator Vector4 (Vector3 rhs)
        {
			Vector4 vec;

            vec.x = rhs.x;
            vec.y = rhs.y;
            vec.z = rhs.z;
            vec.w = 1.0f;
            return vec;
        }

        // arithmetic operations
        inline static Vector4 operator + ( Vector4 lvec, Vector4 rkVector )
        {
            Vector4 kSum;

            kSum.x = lvec.x + rkVector.x;
            kSum.y = lvec.y + rkVector.y;
            kSum.z = lvec.z + rkVector.z;
            kSum.w = lvec.w + rkVector.w;

            return kSum;
        }

        inline static Vector4 operator - ( Vector4 lvec, Vector4 rkVector )
        {
            Vector4 kDiff;

            kDiff.x = lvec.x - rkVector.x;
            kDiff.y = lvec.y - rkVector.y;
            kDiff.z = lvec.z - rkVector.z;
            kDiff.w = lvec.w - rkVector.w;

            return kDiff;
        }

        inline static Vector4 operator * ( Vector4 lvec, Real fScalar )
        {
            Vector4 kProd;

            kProd.x = fScalar*lvec.x;
            kProd.y = fScalar*lvec.y;
            kProd.z = fScalar*lvec.z;
            kProd.w = fScalar*lvec.w;

            return kProd;
        }

        inline static Vector4 operator * ( Real fScalar, Vector4 rvec )
        {
            Vector4 kProd;

            kProd.x = fScalar*rvec.x;
            kProd.y = fScalar*rvec.y;
            kProd.z = fScalar*rvec.z;
            kProd.w = fScalar*rvec.w;

            return kProd;
        }

        inline static Vector4 operator * ( Vector4 lvec, Vector4 rhs)
        {
            Vector4 kProd;

            kProd.x = lvec.x * rhs.x;
            kProd.y = lvec.y * rhs.y;
            kProd.z = lvec.z * rhs.z;
            kProd.w = lvec.w * rhs.w;

            return kProd;
        }

        inline static Vector4 operator / ( Vector4 lvec, Real fScalar )
        {
            assert( fScalar != 0.0 );

            Vector4 kDiv;

            Real fInv = 1.0 / fScalar;
            kDiv.x = lvec.x * fInv;
            kDiv.y = lvec.y * fInv;
            kDiv.z = lvec.z * fInv;
            kDiv.w = lvec.w * fInv;

            return kDiv;
        }

        inline static Vector4 operator / ( Vector4 lvec, Vector4 rhs)
        {
            Vector4 kDiv;

            kDiv.x = lvec.x / rhs.x;
            kDiv.y = lvec.y / rhs.y;
            kDiv.z = lvec.z / rhs.z;
            kDiv.w = lvec.w / rhs.w;

            return kDiv;
        }


        inline static Vector4 operator - (Vector4 vec)
        {
            Vector4 kNeg;

            kNeg.x = -vec.x;
            kNeg.y = -vec.y;
            kNeg.z = -vec.z;
            kNeg.w = -vec.w;

            return kNeg;
        }

        inline static Vector4 operator + (Vector4 lhs, Real rhs)
        {
            return Vector4(rhs) + lhs;
        }

        inline static Vector4 operator + (Real lhs, Vector4 rhs)
        {
            return Vector4(lhs) + rhs;
        }

        inline static Vector4 operator - (Vector4 lhs, Real rhs)
        {
            return lhs - Vector4(rhs);
        }

        inline static Vector4 operator - (Real lhs, Vector4 rhs)
        {
            return Vector4(lhs) - rhs;
        }

        /** Calculates the dot (scalar) product of this vector with another.
            @param
                vec Vector with which to calculate the dot product (together
                with this one).
            @returns
                A float representing the dot product value.
        */
        inline Real DotProduct(Vector4 vec)
        {
            return x * vec.x + y * vec.y + z * vec.z + w * vec.w;
        }
        /** Function for writing to a stream.
        */
		virtual System::String^ ToString() override
        {
			return System::String::Format("Vector4({0}, {1}, {2}, {3})", x, y, z, w);
        }
        // special
        static const Vector4 ZERO = Vector4(0, 0, 0, 0);
    };
}