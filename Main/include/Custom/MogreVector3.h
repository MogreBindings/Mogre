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

#pragma warning(push, 0)
#pragma managed(push, off)
#include "OgreVector3.h"
#pragma managed(pop)
#pragma warning(pop)
#include "Prerequisites.h"
#include "Custom\MogreMath.h"

namespace Mogre
{
    value class Quaternion;

    /** <summary>Standard 3-dimensional vector.</summary>
    <remarks>
    A direction in 3D space represented as distances along the 3
    orthoganal axes (x, y, z). Note that positions, directions and
    scaling factors can be represented by a vector, depending on how
    you interpret the values.
    </remarks>
    */
    [Serializable]
    public value class Vector3 : IEquatable<Vector3>
    {
    public:
        DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( Vector3 )

        Real x, y, z;

        inline Vector3( Real fX, Real fY, Real fZ )
            : x( fX ), y( fY ), z( fZ )
        {
        }

        inline explicit Vector3( array<Real>^ afCoordinate )
            : x( afCoordinate[0] ),
              y( afCoordinate[1] ),
              z( afCoordinate[2] )
        {
        }

        inline explicit Vector3( array<int>^ afCoordinate )
        {
            x = (Real)afCoordinate[0];
            y = (Real)afCoordinate[1];
            z = (Real)afCoordinate[2];
        }

        inline explicit Vector3( Real* const r )
            : x( r[0] ), y( r[1] ), z( r[2] )
        {
        }

        inline explicit Vector3( Real scaler )
            : x( scaler )
            , y( scaler )
            , z( scaler )
        {
        }

        property Real default[int]
        {
            inline Real get(int i)
            {
                assert( i < 3 );

                return *(&x+i);
            }

            inline void set(int i, Real value)
            {
                assert( i < 3 );

                *(&x+i) = value;
            }
        }

        inline static bool operator == ( Vector3 lvec, Vector3 rvec )
        {
            return ( lvec.x == rvec.x && lvec.y == rvec.y && lvec.z == rvec.z );
        }

        inline static bool operator != ( Vector3 lvec, Vector3 rvec )
        {
            return ( lvec.x != rvec.x || lvec.y != rvec.y || lvec.z != rvec.z );
        }

        virtual bool Equals(Vector3 other) { return *this == other; }

        // arithmetic operations
        inline static Vector3 operator + ( Vector3 lvec, Vector3 rvec)
        {
            Vector3 kSum;

            kSum.x = lvec.x + rvec.x;
            kSum.y = lvec.y + rvec.y;
            kSum.z = lvec.z + rvec.z;

            return kSum;
        }

        inline static Vector3 operator - ( Vector3 lvec, Vector3 rvec)
        {
            Vector3 kDiff;

            kDiff.x = lvec.x - rvec.x;
            kDiff.y = lvec.y - rvec.y;
            kDiff.z = lvec.z - rvec.z;

            return kDiff;
        }

        inline static Vector3 operator * ( Vector3 lvec, Real fScalar)
        {
            Vector3 kProd;

            kProd.x = fScalar*lvec.x;
            kProd.y = fScalar*lvec.y;
            kProd.z = fScalar*lvec.z;

            return kProd;
        }

        inline static Vector3 operator * ( Real fScalar, Vector3 rvec)
        {
            Vector3 kProd;

            kProd.x = fScalar*rvec.x;
            kProd.y = fScalar*rvec.y;
            kProd.z = fScalar*rvec.z;

            return kProd;
        }

        inline static Vector3 operator * ( Vector3 lvec, Vector3 rvec)
        {
            Vector3 kProd;

            kProd.x = lvec.x * rvec.x;
            kProd.y = lvec.y * rvec.y;
            kProd.z = lvec.z * rvec.z;

            return kProd;
        }

        inline static Vector3 operator / ( Vector3 lvec, Real fScalar)
        {
            assert( fScalar != 0.0 );

            Vector3 kDiv;

            Real fInv = 1.0 / fScalar;
            kDiv.x = lvec.x * fInv;
            kDiv.y = lvec.y * fInv;
            kDiv.z = lvec.z * fInv;

            return kDiv;
        }

        inline static Vector3 operator / ( Vector3 lvec, Vector3 rvec )
        {
            Vector3 kDiv;

            kDiv.x = lvec.x / rvec.x;
            kDiv.y = lvec.y / rvec.y;
            kDiv.z = lvec.z / rvec.z;

            return kDiv;
        }

        inline static Vector3 operator - ( Vector3 vec )
        {
            Vector3 kNeg;

            kNeg.x = -vec.x;
            kNeg.y = -vec.y;
            kNeg.z = -vec.z;

            return kNeg;
        }

        inline static Vector3 operator + ( Vector3 lvec, Real rhs )
        {
            Vector3 ret(rhs);
            return ret += lvec;
        }

        inline static Vector3 operator + ( Real lhs, Vector3 rvec )
        {
            Vector3 ret(lhs);
            return ret += rvec;
        }

        inline static Vector3 operator - ( Vector3 lvec, Real rhs )
        {
            return lvec - Vector3(rhs);
        }

        inline static Vector3 operator - ( Real lhs, Vector3 rvec )
        {
            Vector3 ret(lhs);
            return ret -= rvec;
        }

        /** <summary>Returns the length (magnitude) of the vector.</summary>
        <remarks>
        <note type="warning">
        This operation requires a square root and is expensive in
        terms of CPU operations. If you don't need to know the exact
        length (e.g. for just comparing lengths) use squaredLength()
        instead.
        </note>
        </remarks>
        */
        property Real Length
        {
            inline Real get()
            {
                return System::Math::Sqrt( x * x + y * y + z * z );
            }
        }

        /** <summary>Returns the square of the length(magnitude) of the vector.</summary>
        <remarks>
        This  method is for efficiency - calculating the actual
        length of a vector requires a square root, which is expensive
        in terms of the operations required. This method returns the
        square of the length of the vector, i.e. the same as the
        length but before the square root is taken. Use this if you
        want to find the longest / shortest vector without incurring
        the square root.
        </remarks>
        */
        property Real SquaredLength
        {
            inline Real get()
            {
                return x * x + y * y + z * z;
            }
        }

        /** <summary>Returns the distance to another vector.</summary>
        <remarks>
        <note type="warning">
        This operation requires a square root and is expensive in
        terms of CPU operations. If you don't need to know the exact
        distance (e.g. for just comparing distances) use squaredDistance()
        instead.
        </note>
        </remarks>
        */
        inline Real Distance(Vector3 rhs)
        {
            return (*this - rhs).Length;
        }

        /** <summary>Returns the square of the distance to another vector.</summary>
        <remarks>
        This method is for efficiency - calculating the actual
        distance to another vector requires a square root, which is
        expensive in terms of the operations required. This method
        returns the square of the distance to another vector, i.e.
        the same as the distance but before the square root is taken.
        Use this if you want to find the longest / shortest distance
        without incurring the square root.
        </remarks>
        */
        inline Real SquaredDistance(Vector3 rhs)
        {
            return (*this - rhs).SquaredLength;
        }

        /** <summary>Calculates the dot (scalar) product of this vector with another.</summary>
        <remarks>
        The dot product can be used to calculate the angle between 2
        vectors. If both are unit vectors, the dot product is the
        cosine of the angle; otherwise the dot product must be
        divided by the product of the lengths of both vectors to get
        the cosine of the angle. This result can further be used to
        calculate the distance of a point from a plane.
        </remarks>
        <param name="vec">Vector with which to calculate the dot product (together with this one).</param>
        <returns>A float representing the dot product value.</returns>
        */
        inline Real DotProduct(Vector3 vec)
        {
            return x * vec.x + y * vec.y + z * vec.z;
        }

        /** <summary>Calculates the absolute dot (scalar) product of this vector with another.</summary>
        <remarks>
        This function work similar dotProduct, except it use absolute value
        of each component of the vector to computing.
        </remarks>
        <param name="vec">Vector with which to calculate the absolute dot product (together with this one).</param>
        <returns>A Real representing the absolute dot product value.</returns>
        */
        inline Real AbsDotProduct(Vector3 vec)
        {
            return System::Math::Abs(x * vec.x) + System::Math::Abs(y * vec.y) + System::Math::Abs(z * vec.z);
        }

        /** <summary>Normalises the vector.</summary>
        <remarks>
        This method normalises the vector such that it's
        length / magnitude is 1. The result is called a unit vector.
        <note>
        This function will not crash for zero-sized vectors, but there
        will be no changes made to their components.
        </note>
        </remarks>
        <returns>The previous length of the vector.</returns>
        */
        inline Real Normalise()
        {
            Real fLength = System::Math::Sqrt( x * x + y * y + z * z );

            // Will also work for zero-sized vectors, but will change nothing
            if ( fLength > 1e-08 )
            {
                Real fInvLength = 1.0 / fLength;
                x *= fInvLength;
                y *= fInvLength;
                z *= fInvLength;
            }

            return fLength;
        }

        /** <summary>Calculates the cross-product of 2 vectors, i.e. the vector that
        lies perpendicular to them both.</summary>
        <remarks>
        The cross-product is normally used to calculate the normal
        vector of a plane, by calculating the cross-product of 2
        non-equivalent vectors which lie on the plane (e.g. 2 edges
        of a triangle).
        </remarks>
        <param name="rkVector">Vector which, together with this one, will be used to calculate the cross-product.</param>
        <returns>
        <para>
        A vector which is the result of the cross-product. This
        vector will <b>NOT</b> be normalised, to maximise efficiency
        - call Vector3::normalise on the result if you wish this to
        be done. As for which side the resultant vector will be on, the
        returned vector will be on the side from which the arc from 'this'
        to rkVector is anticlockwise, e.g. UNIT_Y.CrossProduct(UNIT_Z)
        = UNIT_X, whilst UNIT_Z.CrossProduct(UNIT_Y) = -UNIT_X.
        This is because OGRE uses a right-handed coordinate system.
        </para>
        <para>
        For a clearer explanation, look a the left and the bottom edges
        of your monitor's screen. Assume that the first vector is the
        left edge and the second vector is the bottom edge, both of
        them starting from the lower-left corner of the screen. The
        resulting vector is going to be perpendicular to both of them
        and will go <i>inside</i> the screen, towards the cathode tube
        (assuming you're using a CRT monitor, of course).
        </para>
        </returns>
        */
        inline Vector3 CrossProduct( Vector3 rkVector )
        {
            Vector3 kCross;

            kCross.x = y * rkVector.z - z * rkVector.y;
            kCross.y = z * rkVector.x - x * rkVector.z;
            kCross.z = x * rkVector.y - y * rkVector.x;

            return kCross;
        }

        /** <summary>Returns a vector at a point half way between this and the passed
        in vector.</summary>
        */
        inline Vector3 MidPoint( Vector3 vec )
        {
            return Vector3(
                ( x + vec.x ) * 0.5,
                ( y + vec.y ) * 0.5,
                ( z + vec.z ) * 0.5 );
        }

        /** <summary>Returns true if the vector's scalar components are all greater
        that the ones of the vector it is compared against.</summary>
        */
        inline static bool operator < ( Vector3 lvec, Vector3 rvec )
        {
            if( lvec.x < rvec.x && lvec.y < rvec.y && lvec.z < rvec.z )
                return true;
            return false;
        }

        /** <summary>Returns true if the vector's scalar components are all smaller
        that the ones of the vector it is compared against.</summary>
        */
        inline static bool operator > ( Vector3 lvec, Vector3 rhs )
        {
            if( lvec.x > rhs.x && lvec.y > rhs.y && lvec.z > rhs.z )
                return true;
            return false;
        }

        /** <summary>Sets this vector's components to the minimum of its own and the
        ones of the passed in vector.</summary>
        <remarks>
        'Minimum' in this case means the combination of the lowest
        value of x, y and z from both vectors. Lowest is taken just
        numerically, not magnitude, so -1 &lt; 0.
        </remarks>
        */
        inline void MakeFloor( Vector3 cmp )
        {
            if( cmp.x < x ) x = cmp.x;
            if( cmp.y < y ) y = cmp.y;
            if( cmp.z < z ) z = cmp.z;
        }

        /** <summary>Sets this vector's components to the maximum of its own and the
        ones of the passed in vector.</summary>
        <remarks>
        'Maximum' in this case means the combination of the highest
        value of x, y and z from both vectors. Highest is taken just
        numerically, not magnitude, so 1 > -3.
        </remarks>
        */
        inline void MakeCeil( Vector3 cmp )
        {
            if( cmp.x > x ) x = cmp.x;
            if( cmp.y > y ) y = cmp.y;
            if( cmp.z > z ) z = cmp.z;
        }

        /** <summary>Generates a vector perpendicular to this vector (eg an 'up' vector).</summary>
        <remarks>
        This method will return a vector which is perpendicular to this
        vector. There are an infinite number of possibilities but this
        method will guarantee to generate one of them. If you need more
        control you should use the Quaternion class.
        </remarks>
        */
        property Vector3 Perpendicular
        {
            inline Vector3 get()
            {
                static const Real fSquareZero = 1e-06 * 1e-06;

                Vector3 perp = this->CrossProduct( Vector3::UNIT_X );

                // Check length
                if( perp.SquaredLength < fSquareZero )
                {
                    /* This vector is the Y axis multiplied by a scalar, so we have
                    to use another axis.
                    */
                    perp = this->CrossProduct( Vector3::UNIT_Y );
                }

                return perp;
            }
        }
        /** <summary>Generates a new random vector which deviates from this vector by a
        given angle in a random direction.</summary>
        <remarks>
        This method assumes that the random number generator has already
        been seeded appropriately.
        </remarks>
        <param name="angle">The angle at which to deviate</param>
        <returns>
        A random vector which deviates from this vector by angle. This
        vector will not be normalised, normalise it if you wish
        afterwards.
        </returns>
        */
        inline Vector3 RandomDeviant( Radian angle )
        {
            return RandomDeviant(angle, Vector3::ZERO);
        }

        /** <summary>Generates a new random vector which deviates from this vector by a
        given angle in a random direction.</summary>
        <remarks>
        This method assumes that the random number generator has already
        been seeded appropriately.
        </remarks>
        <param name="angle">The angle at which to deviate</param>
        <param name="up">Any vector perpendicular to this one (which could generated
        by cross-product of this vector and any other non-colinear
        vector). If you choose not to provide this the function will
        derive one on it's own, however if you provide one yourself the
        function will be faster (this allows you to reuse up vectors if
        you call this method more than once)</param>
        <returns>
        A random vector which deviates from this vector by angle. This
        vector will not be normalised, normalise it if you wish
        afterwards.
        </returns>
        */
        Vector3 RandomDeviant(
            Radian angle,
            Vector3 up );

        /** <summary>Gets the angle between 2 vectors.</summary>
        <remarks>
        Vectors do not have to be unit-length but must represent directions.
        </remarks>
        */
        inline Radian AngleBetween(Vector3 dest)
        {
            Real lenProduct = Length * dest.Length;

            // Divide by zero check
            if(lenProduct < 1e-6f)
                lenProduct = 1e-6f;

            Real f = DotProduct(dest) / lenProduct;

            f = Math::Clamp(f, (Real)-1.0, (Real)1.0);
            return Math::ACos(f);

        }

        /** <summary>Gets the shortest arc quaternion to rotate this vector to the destination
        vector.</summary>
        <remarks>
        If you call this with a dest vector that is close to the inverse
        of this vector, we will rotate 180 degrees around the 'fallbackAxis'
        (if specified, or a generated axis if not) since in this case
        ANY axis of rotation is valid.
        </remarks>
        */
        Quaternion GetRotationTo(Vector3 dest,
            Vector3 fallbackAxis);

        /** <summary>Gets the shortest arc quaternion to rotate this vector to the destination
        vector.</summary>
        <remarks>
        If you call this with a dest vector that is close to the inverse
        of this vector, we will rotate 180 degrees around the 'fallbackAxis'
        (if specified, or a generated axis if not) since in this case
        ANY axis of rotation is valid.
        </remarks>
        */
        Quaternion GetRotationTo(Vector3 dest);

        /** <summary>Returns true if this vector is zero length.</summary> */
        property bool IsZeroLength
        {
            inline bool get()
            {
                Real sqlen = (x * x) + (y * y) + (z * z);
                return (sqlen < (1e-06 * 1e-06));

            }
        }

        /** <summary>As normalise, except that this vector is unaffected and the
        normalised vector is returned as a copy.</summary>
        */
        property Vector3 NormalisedCopy
        {
            inline Vector3 get()
            {
                Vector3 ret = *this;
                ret.Normalise();
                return ret;
            }
        }

        /** <summary>Calculates a reflection vector to the plane with the given normal.</summary>
        <remarks> NB assumes 'this' is pointing AWAY FROM the plane, invert if it is not.</remarks>
        */
        inline Vector3 Reflect(Vector3 normal)
        {
            return Vector3( *this - ( 2 * this->DotProduct(normal) * normal ) );
        }

        /** <summary>Returns whether this vector is within a positional tolerance
        of another vector.</summary>
        <param name="rhs">The vector to compare with</param>
        <param name="tolerance">The amount that each element of the vector may vary by
        and still be considered equal</param>
        */
        inline bool PositionEquals(Vector3 rhs, Real tolerance)
        {
            return Math::RealEqual(x, rhs.x, tolerance) &&
                Math::RealEqual(y, rhs.y, tolerance) &&
                Math::RealEqual(z, rhs.z, tolerance);

        }
        /** <summary>Returns whether this vector is within a positional tolerance
        of another vector.</summary>
        <param name="rhs">The vector to compare with</param>
        */
        inline bool PositionEquals(Vector3 rhs)
        {
            return PositionEquals(rhs, 1e-03);
        }

        /** <summary>Returns whether this vector is within a positional tolerance
        of another vector, also take scale of the vectors into account.</summary>
        <param name="rhs">The vector to compare with</param>
        <param name="tolerance">The amount (related to the scale of vectors) that distance
        of the vector may vary by and still be considered close</param>
        */
        inline bool PositionCloses(Vector3 rhs, Real tolerance)
        {
            return SquaredDistance(rhs) <=
                (SquaredLength + rhs.SquaredLength) * tolerance;
        }
        /** <summary>Returns whether this vector is within a positional tolerance
        of another vector, also take scale of the vectors into account.</summary>
        <param name="rhs">The vector to compare with</param>
        */
        inline bool PositionCloses(Vector3 rhs)
        {
            return PositionCloses(rhs, 1e-03f);
        }

        /** <summary>Returns whether this vector is within a directional tolerance
        of another vector.</summary>
        <param name="rhs">The vector to compare with</param>
        <param name="tolerance">The maximum angle by which the vectors may vary and
        still be considered equal</param>
        */
        inline bool DirectionEquals(Vector3 rhs, Radian tolerance)
        {
            Real dot = DotProduct(rhs);
            Radian angle = Mogre::Math::ACos(dot);

            return Math::Abs(angle.ValueRadians) <= tolerance.ValueRadians;
        }

        /// <summary>Check whether this vector contains valid values</summary>
        property bool IsNaN
        {
            inline bool get()
            {
                return Real::IsNaN(x) || Real::IsNaN(y) || Real::IsNaN(z);
            }
        }

        // special points
        static initonly Vector3 ZERO = Vector3( 0, 0, 0 );
        static initonly Vector3 UNIT_X = Vector3( 1, 0, 0 );
        static initonly Vector3 UNIT_Y = Vector3( 0, 1, 0 );
        static initonly Vector3 UNIT_Z = Vector3( 0, 0, 1 );
        static initonly Vector3 NEGATIVE_UNIT_X = Vector3( -1,  0,  0 );
        static initonly Vector3 NEGATIVE_UNIT_Y = Vector3(  0, -1,  0 );
        static initonly Vector3 NEGATIVE_UNIT_Z = Vector3(  0,  0, -1 );
        static initonly Vector3 UNIT_SCALE = Vector3(1, 1, 1);

        /// <inheritdoc />
        virtual System::String^ ToString() override
        {
            return System::String::Format("Vector3({0}, {1}, {2})", x, y, z);
        }
    };
}