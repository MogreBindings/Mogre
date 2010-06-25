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

#include "OgreVector2.h"
#include "Custom\MogreMath.h"

namespace Mogre
{
    /** Standard 2-dimensional vector.
        @remarks
            A direction in 2D space represented as distances along the 2
            orthoganal axes (x, y). Note that positions, directions and
            scaling factors can be represented by a vector, depending on how
            you interpret the values.
    */
	[Serializable]
	public value class Vector2 : IEquatable<Vector2>
    {
    public:
		DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( Vector2 )

		Real x, y;

    public:
        inline Vector2(Real fX, Real fY )
            : x( fX ), y( fY )
        {
        }

        inline explicit Vector2( Real scaler )
            : x( scaler), y( scaler )
        {
        }

        inline explicit Vector2( array<Real>^ afCoordinate )
            : x( afCoordinate[0] ),
              y( afCoordinate[1] )
        {
        }

        inline explicit Vector2( array<int>^ afCoordinate )
        {
            x = (Real)afCoordinate[0];
            y = (Real)afCoordinate[1];
        }

        inline explicit Vector2( Real* const r )
            : x( r[0] ), y( r[1] )
        {
        }

		property Real default[int]
		{
			inline Real get(int i)
			{
				assert( i < 2 );

				return *(&x+i);
			}

			inline void set(int i, Real value)
			{
				assert( i < 2 );

				*(&x+i) = value;
			}
		}

        inline static bool operator == ( Vector2 lvec, Vector2 rkVector )
        {
            return ( lvec.x == rkVector.x && lvec.y == rkVector.y );
        }

        inline static bool operator != ( Vector2 lvec, Vector2 rkVector )
        {
            return ( lvec.x != rkVector.x || lvec.y != rkVector.y  );
        }

		virtual bool Equals(Vector2 other) { return *this == other; }

		// arithmetic operations
        inline static Vector2 operator + ( Vector2 lvec, Vector2 rkVector )
        {
            Vector2 kSum;

            kSum.x = lvec.x + rkVector.x;
            kSum.y = lvec.y + rkVector.y;

            return kSum;
        }

        inline static Vector2 operator - ( Vector2 lvec, Vector2 rkVector )
        {
            Vector2 kDiff;

            kDiff.x = lvec.x - rkVector.x;
            kDiff.y = lvec.y - rkVector.y;

            return kDiff;
        }

        inline static Vector2 operator * ( Vector2 lvec, Real fScalar )
        {
            Vector2 kProd;

            kProd.x = fScalar*lvec.x;
            kProd.y = fScalar*lvec.y;

            return kProd;
        }

        inline static Vector2 operator * ( Real fScalar, Vector2 rvec )
        {
            Vector2 kProd;

            kProd.x = fScalar*rvec.x;
            kProd.y = fScalar*rvec.y;

            return kProd;
        }

        inline static Vector2 operator * ( Vector2 lhs, Vector2 rhs)
        {
            Vector2 kProd;

            kProd.x = lhs.x * rhs.x;
            kProd.y = lhs.y * rhs.y;

            return kProd;
        }

        inline static Vector2 operator / ( Vector2 lvec, Real fScalar )
        {
            assert( fScalar != 0.0 );

            Vector2 kDiv;

            Real fInv = 1.0 / fScalar;
            kDiv.x = lvec.x * fInv;
            kDiv.y = lvec.y * fInv;

            return kDiv;
        }

        inline static Vector2 operator - (Vector2 vec)
        {
            Vector2 kNeg;

            kNeg.x = -vec.x;
            kNeg.y = -vec.y;

            return kNeg;
        }

        inline static Vector2 operator + (Vector2 lhs, Real rhs)
        {
            Vector2 ret(rhs);
            return ret += lhs;
        }

        inline static Vector2 operator + (Real lhs, Vector2 rhs)
        {
            Vector2 ret(lhs);
            return ret += rhs;
        }

        inline static Vector2 operator - (Vector2 lhs, Real rhs)
        {
            return lhs - Vector2(rhs);
        }

        inline static Vector2 operator - (Real lhs, Vector2 rhs)
        {
            Vector2 ret(lhs);
            return ret -= rhs;
        }

        /** Returns the length (magnitude) of the vector.
            @warning
                This operation requires a square root and is expensive in
                terms of CPU operations. If you don't need to know the exact
                length (e.g. for just comparing lengths) use squaredLength()
                instead.
        */
		property Real Length
		{
			inline Real get()
			{
				return System::Math::Sqrt( x * x + y * y );
			}
		}

        /** Returns the square of the length(magnitude) of the vector.
            @remarks
                This  method is for efficiency - calculating the actual
                length of a vector requires a square root, which is expensive
                in terms of the operations required. This method returns the
                square of the length of the vector, i.e. the same as the
                length but before the square root is taken. Use this if you
                want to find the longest / shortest vector without incurring
                the square root.
        */
		property Real SquaredLength
		{
			inline Real get()
			{
				return x * x + y * y;
			}
		}

        /** Calculates the dot (scalar) product of this vector with another.
            @remarks
                The dot product can be used to calculate the angle between 2
                vectors. If both are unit vectors, the dot product is the
                cosine of the angle; otherwise the dot product must be
                divided by the product of the lengths of both vectors to get
                the cosine of the angle. This result can further be used to
                calculate the distance of a point from a plane.
            @param
                vec Vector with which to calculate the dot product (together
                with this one).
            @returns
                A float representing the dot product value.
        */
        inline Real DotProduct(Vector2 vec)
        {
            return x * vec.x + y * vec.y;
        }

        /** Normalises the vector.
            @remarks
                This method normalises the vector such that it's
                length / magnitude is 1. The result is called a unit vector.
            @note
                This function will not crash for zero-sized vectors, but there
                will be no changes made to their components.
            @returns The previous length of the vector.
        */
        inline Real Normalise()
        {
            Real fLength = System::Math::Sqrt( x * x + y * y);

            // Will also work for zero-sized vectors, but will change nothing
            if ( fLength > 1e-08 )
            {
                Real fInvLength = 1.0 / fLength;
                x *= fInvLength;
                y *= fInvLength;
            }

            return fLength;
        }



        /** Returns a vector at a point half way between this and the passed
            in vector.
        */
        inline Vector2 MidPoint( Vector2 vec )
        {
            return Vector2(
                ( x + vec.x ) * 0.5,
                ( y + vec.y ) * 0.5 );
        }

        /** Returns true if the vector's scalar components are all greater
            that the ones of the vector it is compared against.
        */
        inline static bool operator < ( Vector2 lhs, Vector2 rhs )
        {
            if( lhs.x < rhs.x && lhs.y < rhs.y )
                return true;
            return false;
        }

        /** Returns true if the vector's scalar components are all smaller
            that the ones of the vector it is compared against.
        */
        inline static bool operator > ( Vector2 lhs, Vector2 rhs )
        {
            if( lhs.x > rhs.x && lhs.y > rhs.y )
                return true;
            return false;
        }

        /** Sets this vector's components to the minimum of its own and the
            ones of the passed in vector.
            @remarks
                'Minimum' in this case means the combination of the lowest
                value of x, y and z from both vectors. Lowest is taken just
                numerically, not magnitude, so -1 < 0.
        */
        inline void MakeFloor( Vector2 cmp )
        {
            if( cmp.x < x ) x = cmp.x;
            if( cmp.y < y ) y = cmp.y;
        }

        /** Sets this vector's components to the maximum of its own and the
            ones of the passed in vector.
            @remarks
                'Maximum' in this case means the combination of the highest
                value of x, y and z from both vectors. Highest is taken just
                numerically, not magnitude, so 1 > -3.
        */
        inline void MakeCeil( Vector2 cmp )
        {
            if( cmp.x > x ) x = cmp.x;
            if( cmp.y > y ) y = cmp.y;
        }

        /** Generates a vector perpendicular to this vector (eg an 'up' vector).
            @remarks
                This method will return a vector which is perpendicular to this
                vector. There are an infinite number of possibilities but this
                method will guarantee to generate one of them. If you need more
                control you should use the Quaternion class.
        */
		property Vector2 Perpendicular
		{
			inline Vector2 get()
			{
				return Vector2 (-y, x);
			}
		}
        /** Calculates the 2 dimensional cross-product of 2 vectors, which results
			in a single floating point value which is 2 times the area of the triangle.
        */
        inline Real CrossProduct( Vector2 rkVector )
        {
            return x * rkVector.y - y * rkVector.x;
        }
        /** Generates a new random vector which deviates from this vector by a
            given angle in a random direction.
            @remarks
                This method assumes that the random number generator has already
                been seeded appropriately.
            @param
                angle The angle at which to deviate in radians
            @param
                up Any vector perpendicular to this one (which could generated
                by cross-product of this vector and any other non-colinear
                vector). If you choose not to provide this the function will
                derive one on it's own, however if you provide one yourself the
                function will be faster (this allows you to reuse up vectors if
                you call this method more than once)
            @returns
                A random vector which deviates from this vector by angle. This
                vector will not be normalised, normalise it if you wish
                afterwards.
        */
        inline Vector2 RandomDeviant(
            Real angle)
        {

            angle *=  Math::UnitRandom() * Math::TWO_PI;
			Real cosa = System::Math::Cos(angle);
			Real sina = System::Math::Sin(angle);
            return  Vector2(cosa * x - sina * y,
                            sina * x + cosa * y);
        }

        /** Returns true if this vector is zero length. */
		property bool IsZeroLength
		{
			inline bool get()
			{
				Real sqlen = (x * x) + (y * y);
				return (sqlen < (1e-06 * 1e-06));

			}
		}

        /** As normalise, except that this vector is unaffected and the
            normalised vector is returned as a copy. */
		property Vector2 NormalisedCopy
		{
			inline Vector2 get()
			{
				Vector2 ret = *this;
				ret.Normalise();
				return ret;
			}
		}

        /** Calculates a reflection vector to the plane with the given normal .
        @remarks NB assumes 'this' is pointing AWAY FROM the plane, invert if it is not.
        */
        inline Vector2 Reflect(Vector2 normal)
        {
            return Vector2( *this - ( 2 * this->DotProduct(normal) * normal ) );
        }

        // special points
        static const Vector2 ZERO = Vector2( 0, 0);
        static const Vector2 UNIT_X = Vector2( 1, 0);
        static const Vector2 UNIT_Y = Vector2( 0, 1);
        static const Vector2 NEGATIVE_UNIT_X = Vector2( -1,  0);
        static const Vector2 NEGATIVE_UNIT_Y = Vector2(  0, -1);
        static const Vector2 UNIT_SCALE = Vector2(1, 1);

        /** Function for writing to a stream.
        */
		virtual System::String^ ToString() override
        {
			return System::String::Format("Vector2({0}, {1})", x, y);
        }
    };
}