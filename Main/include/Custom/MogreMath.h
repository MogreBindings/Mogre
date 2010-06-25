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

#include "OgreMath.h"
#include "Prerequisites.h"
#include "MogrePair.h"

namespace Mogre
{
	ref class AxisAlignedBox;
	ref class Matrix4;
	value class Ray;
	value class Sphere;
	value class Plane;
	value class Vector2;
	value class Vector3;
	value class Vector4;

	value class Degree;

    /** Wrapper class which indicates a given angle value is in Radians.
    @remarks
        Radian values are interchangeable with Degree values, and conversions
        will be done automatically between them.
    */
	[Serializable]
	public value class Radian : IEquatable<Radian>, IComparable<Radian>
	{
		Real mRad;

	public:
		explicit Radian ( Real r ) : mRad(r) {}
		Radian ( Degree d );
		inline static operator Radian ( Real f ) { return Radian(f); }
		inline static operator Radian ( Degree d );

		property Real ValueDegrees { Real get(); }
		property Real ValueRadians { Real get() { return mRad; } }
		property Real ValueAngleUnits { Real get(); }

		inline static Radian operator + ( Radian l, Radian r ) { return Radian ( l.mRad + r.mRad ); }
		inline static Radian operator + ( Radian l, Degree d );
		inline static Radian operator - (Radian r ) { return Radian(-r.mRad); }
		inline static Radian operator - ( Radian l, Radian r ) { return Radian ( l.mRad - r.mRad ); }
		inline static Radian operator - ( Radian l, Degree d );
		inline static Radian operator * ( Radian l, Real f ) { return Radian ( l.mRad * f ); }
		inline static Radian operator * ( Real f, Radian r ) { return Radian ( r.mRad * f ); }
        inline static Radian operator * ( Radian l, Radian f ) { return Radian ( l.mRad * f.mRad ); }
		inline static Radian operator / ( Radian l, Real f ) { return Radian ( l.mRad / f ); }
		inline static Radian operator / ( Real f, Radian r ) { return Radian ( f / r.mRad ); }

		inline static bool operator <  ( Radian l, Radian r ) { return l.mRad <  r.mRad; }
		inline static bool operator <= ( Radian l, Radian r ) { return l.mRad <= r.mRad; }
		inline static bool operator == ( Radian l, Radian r ) { return l.mRad == r.mRad; }
		inline static bool operator != ( Radian l, Radian r ) { return l.mRad != r.mRad; }
		inline static bool operator >= ( Radian l, Radian r ) { return l.mRad >= r.mRad; }
		inline static bool operator >  ( Radian l, Radian r ) { return l.mRad >  r.mRad; }

		virtual bool Equals(Radian other) { return *this == other; }
		virtual int CompareTo(Radian other)
		{
			if (mRad < other.mRad) return -1;
			if (mRad > other.mRad) return 1;
			return 0;
		}

		DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( Radian )
	};

    /** Wrapper class which indicates a given angle value is in Degrees.
    @remarks
        Degree values are interchangeable with Radian values, and conversions
        will be done automatically between them.
    */
	[Serializable]
	public value class Degree : IEquatable<Degree>, IComparable<Degree>
	{
		Real mDeg;

	public:
		explicit Degree ( Real d ) : mDeg(d) {}
		Degree ( Radian r ) : mDeg(r.ValueDegrees) {}
		inline static operator Degree ( Real f ) { return Degree(f); }
		inline static operator Degree ( Radian r ) { return Degree(r.ValueDegrees); }

		property Real ValueDegrees { Real get() { return mDeg; } }
		property Real ValueRadians { Real get(); }
		property Real ValueAngleUnits { Real get(); }

		inline static Degree operator + ( Degree l, Degree d ) { return Degree ( l.mDeg + d.mDeg ); }
		inline static Degree operator + ( Degree l, Radian r ) { return Degree ( l.mDeg + r.ValueDegrees ); }
		inline static Degree operator - (Degree d) { return Degree(-d.mDeg); }
		inline static Degree operator - ( Degree l, Degree d ) { return Degree ( l.mDeg - d.mDeg ); }
		inline static Degree operator - ( Degree l, Radian r ) { return Degree ( l.mDeg - r.ValueDegrees ); }
		inline static Degree operator * ( Degree l, Real f ) { return Degree ( l.mDeg * f ); }
		inline static Degree operator * ( Real f, Degree r ) { return Degree ( r.mDeg * f ); }
        inline static Degree operator * ( Degree l, Degree f ) { return Degree ( l.mDeg * f.mDeg ); }
		inline static Degree operator / ( Degree l, Real f ) { return Degree ( l.mDeg / f ); }
		inline static Degree operator / ( Real f, Degree r ) { return Degree ( f / r.mDeg ); }

		inline static bool operator <  ( Degree l, Degree d ) { return l.mDeg <  d.mDeg; }
		inline static bool operator <= ( Degree l, Degree d ) { return l.mDeg <= d.mDeg; }
		inline static bool operator == ( Degree l, Degree d ) { return l.mDeg == d.mDeg; }
		inline static bool operator != ( Degree l, Degree d ) { return l.mDeg != d.mDeg; }
		inline static bool operator >= ( Degree l, Degree d ) { return l.mDeg >= d.mDeg; }
		inline static bool operator >  ( Degree l, Degree d ) { return l.mDeg >  d.mDeg; }

		virtual bool Equals(Degree other) { return *this == other; }
		virtual int CompareTo(Degree other)
		{
			if (mDeg < other.mDeg) return -1;
			if (mDeg > other.mDeg) return 1;
			return 0;
		}

		DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( Degree )
	};

    /** Wrapper class which identifies a value as the currently default angle 
        type, as defined by Math::setAngleUnit.
    @remarks
        Angle values will be automatically converted between radians and degrees,
        as appropriate.
    */
	[Serializable]
	public value class Angle : IEquatable<Angle>, IComparable<Angle>
	{
		Real mAngle;
	public:
		explicit Angle ( Real angle ) : mAngle(angle) {}
		static operator Radian( Angle a );
		static operator Degree( Angle a );

		virtual bool Equals(Angle other) { return mAngle == other.mAngle; }
		virtual int CompareTo(Angle other)
		{
			if (mAngle < other.mAngle) return -1;
			if (mAngle > other.mAngle) return 1;
			return 0;
		}
	};

	// these functions could not be defined within the class definition of class
	// Radian because they required class Degree to be defined
	inline Radian::Radian ( Degree d ) : mRad(d.ValueRadians) {
	}
	inline Radian::operator Radian ( Degree d ) {
		return Radian(d.ValueRadians);
	}
	inline Radian Radian::operator + ( Radian l, Degree d ) {
		return Radian ( l.mRad + d.ValueRadians );
	}
	inline Radian Radian::operator - ( Radian l, Degree d ) {
		return Radian ( l.mRad - d.ValueRadians );
	}

    /** Class to provide access to common mathematical functions.
        @remarks
            Most of the maths functions are aliased versions of the C runtime
            library functions. They are aliased here to provide future
            optimisation opportunities, either from faster RTLs or custom
            math approximations.
        @note
            <br>This is based on MgcMath.h from
            <a href="http://www.geometrictools.com/">Wild Magic</a>.
    */
	public ref class Math
	{
	public:

       enum class AngleUnit
       {
           AU_DEGREE,
           AU_RADIAN
       };

    protected:
       // angle units used by the api
       static AngleUnit msAngleUnit;

        /// Size of the trig tables as determined by constructor.
        static int mTrigTableSize;

        /// Radian -> index factor value ( mTrigTableSize / 2 * PI )
        static Real mTrigTableFactor;
        static array<Real>^ mSinTable;
        static array<Real>^ mTanTable;

        /** Private function to build trig tables.
        */
        void buildTrigTables();

		static Real SinTable (Real fValue);
		static Real TanTable (Real fValue);

		static System::Random^ mRandomizer = gcnew System::Random;

    public:
        /** Default constructor.
            @param
                trigTableSize Optional parameter to set the size of the
                tables used to implement Sin, Cos, Tan
        */
        Math(unsigned int trigTableSize);
        Math();

		static inline int IAbs (int iValue) { return ( iValue >= 0 ? iValue : -iValue ); }
		static inline int ICeil (float fValue) { return int(ceil(fValue)); }
		static inline int IFloor (float fValue) { return int(floor(fValue)); }
        static int ISign (int iValue);

		static inline Real Abs (Real fValue) { return Real(System::Math::Abs(fValue)); }
		static inline Degree Abs (Degree dValue) { return Degree(System::Math::Abs(dValue.ValueDegrees)); }
		static inline Radian Abs (Radian rValue) { return Radian(System::Math::Abs(rValue.ValueRadians)); }
		static Radian ACos (Real fValue);
		static Radian ASin (Real fValue);
		static inline Radian ATan (Real fValue) { return Radian(System::Math::Atan(fValue)); }
		static inline Radian ATan2 (Real fY, Real fX) { return Radian(System::Math::Atan2(fY,fX)); }
		static inline Real Ceil (Real fValue) { return Real(System::Math::Ceiling(fValue)); }

        /** Cosine function.
            @param
                fValue Angle in radians
            @param
                useTables If true, uses lookup tables rather than
                calculation - faster but less accurate.
        */
        static inline Real Cos (Radian fValue, bool useTables) {
			return (!useTables) ? Real(System::Math::Cos(fValue.ValueRadians)) : SinTable(fValue.ValueRadians + HALF_PI);
		}
        static inline Real Cos (Radian fValue) {
			return Real(System::Math::Cos(fValue.ValueRadians));
		}
        /** Cosine function.
            @param
                fValue Angle in radians
            @param
                useTables If true, uses lookup tables rather than
                calculation - faster but less accurate.
        */
        static inline Real Cos (Real fValue, bool useTables) {
			return (!useTables) ? Real(System::Math::Cos(fValue)) : SinTable(fValue + HALF_PI);
		}
        static inline Real Cos (Real fValue) {
			return Real(System::Math::Cos(fValue));
		}

        static Real Sign (Real fValue);
		static inline Radian Sign ( Radian rValue )
		{
			return Radian(Sign(rValue.ValueRadians));
		}
		static inline Degree Sign ( Degree dValue )
		{
			return Degree(Sign(dValue.ValueDegrees));
		}

        /** Sine function.
            @param
                fValue Angle in radians
            @param
                useTables If true, uses lookup tables rather than
                calculation - faster but less accurate.
        */
        static inline Real Sin (Radian fValue, bool useTables) {
			return (!useTables) ? Real(System::Math::Sin(fValue.ValueRadians)) : SinTable(fValue.ValueRadians);
		}
        static inline Real Sin (Radian fValue) {
			return Real(System::Math::Sin(fValue.ValueRadians));
		}
        /** Sine function.
            @param
                fValue Angle in radians
            @param
                useTables If true, uses lookup tables rather than
                calculation - faster but less accurate.
        */
        static inline Real Sin (Real fValue, bool useTables) {
			return (!useTables) ? Real(System::Math::Sin(fValue)) : SinTable(fValue);
		}
        static inline Real Sin (Real fValue) {
			return Real(System::Math::Sin(fValue));
		}

		static inline Real Sqr (Real fValue) { return fValue*fValue; }

		static inline Real Sqrt (Real fValue) { return Real(System::Math::Sqrt(fValue)); }

        static inline Radian Sqrt (Radian fValue) { return Radian(System::Math::Sqrt(fValue.ValueRadians)); }

        static inline Degree Sqrt (Degree fValue) { return Degree(System::Math::Sqrt(fValue.ValueDegrees)); }

		/** Inverse square root i.e. 1 / Sqrt(x), good for vector
            normalisation.
        */
		static Real InvSqrt(Real fValue);

		static Real UnitRandom ();  // in [0,1]

        static Real RangeRandom (Real fLow, Real fHigh);  // in [fLow,fHigh]

        static Real SymmetricRandom ();  // in [-1,1]

		static inline Real DegreesToRadians(Real degrees) { return degrees * fDeg2Rad; }
        static inline Real RadiansToDegrees(Real radians) { return radians * fRad2Deg; }

       /** These functions used to set the assumed angle units (radians or degrees) 
            expected when using the Angle type.
       @par
            You can set this directly after creating a new Root, and also before/after resource creation,
            depending on whether you want the change to affect resource files.
       */
       static void SetAngleUnit(AngleUnit unit);
       /** Get the unit being used for angles. */
       static AngleUnit GetAngleUnit(void);

       /** Convert from the current AngleUnit to radians. */
       static Real AngleUnitsToRadians(Real units);
       /** Convert from radians to the current AngleUnit . */
       static Real RadiansToAngleUnits(Real radians);
       /** Convert from the current AngleUnit to degrees. */
       static Real AngleUnitsToDegrees(Real units);
       /** Convert from degrees to the current AngleUnit. */
       static Real DegreesToAngleUnits(Real degrees);

       /** Checks whether a given point is inside a triangle, in a
            2-dimensional (Cartesian) space.
            @remarks
                The vertices of the triangle must be given in either
                trigonometrical (anticlockwise) or inverse trigonometrical
                (clockwise) order.
            @param
                p The point.
            @param
                a The triangle's first vertex.
            @param
                b The triangle's second vertex.
            @param
                c The triangle's third vertex.
            @returns
                If the point resides in the triangle, <b>true</b> is
                returned.
            @par
                If the point is outside the triangle, <b>false</b> is
                returned.
        */
        static bool PointInTri2D(Vector2 p, Vector2 a, 
			Vector2 b, Vector2 c);

       /** Checks whether a given 3D point is inside a triangle.
       @remarks
            The vertices of the triangle must be given in either
            trigonometrical (anticlockwise) or inverse trigonometrical
            (clockwise) order, and the point must be guaranteed to be in the
			same plane as the triangle
        @param
            p The point.
        @param
            a The triangle's first vertex.
        @param
            b The triangle's second vertex.
        @param
            c The triangle's third vertex.
		@param 
			normal The triangle plane's normal (passed in rather than calculated
				on demand since the callermay already have it)
        @returns
            If the point resides in the triangle, <b>true</b> is
            returned.
        @par
            If the point is outside the triangle, <b>false</b> is
            returned.
        */
        static bool PointInTri3D(Vector3 p, Vector3 a, 
			Vector3 b, Vector3 c, Vector3 normal);
        /** Ray / plane intersection, returns boolean result and distance. */
        static Pair<bool, Real> Intersects(Ray ray, Plane plane);

        /** Ray / sphere intersection, returns boolean result and distance. */
        static Pair<bool, Real> Intersects(Ray ray, Sphere sphere, 
            bool discardInside);
        static Pair<bool, Real> Intersects(Ray ray, Sphere sphere);
        
        /** Ray / box intersection, returns boolean result and distance. */
        static Pair<bool, Real> Intersects(Ray ray, AxisAlignedBox^ box);

        /** Ray / box intersection, returns boolean result and two intersection distance.
        @param
            ray The ray.
        @param
            box The box.
        @param
            d1 A real pointer to retrieve the near intersection distance
                from the ray origin, maybe <b>null</b> which means don't care
                about the near intersection distance.
        @param
            d2 A real pointer to retrieve the far intersection distance
                from the ray origin, maybe <b>null</b> which means don't care
                about the far intersection distance.
        @returns
            If the ray is Intersects the box, <b>true</b> is returned, and
            the near intersection distance is return by <i>d1</i>, the
            far intersection distance is return by <i>d2</i>. Guarantee
            <b>0</b> <= <i>d1</i> <= <i>d2</i>.
        @par
            If the ray isn't Intersects the box, <b>false</b> is returned, and
            <i>d1</i> and <i>d2</i> is unmodified.
        */
        static bool Intersects(Ray ray, AxisAlignedBox^ box,
            [Out] Real% d1, [Out] Real% d2);

        /** Ray / triangle intersection, returns boolean result and distance.
        @param
            ray The ray.
        @param
            a The triangle's first vertex.
        @param
            b The triangle's second vertex.
        @param
            c The triangle's third vertex.
		@param 
			normal The triangle plane's normal (passed in rather than calculated
				on demand since the callermay already have it), doesn't need
                normalised since we don't care.
        @param
            positiveSide Intersect with "positive side" of the triangle
        @param
            negativeSide Intersect with "negative side" of the triangle
        @returns
            If the ray is Intersects the triangle, a pair of <b>true</b> and the
            distance between intersection point and ray origin returned.
        @par
            If the ray isn't Intersects the triangle, a pair of <b>false</b> and
            <b>0</b> returned.
        */
        static Pair<bool, Real> Intersects(Ray ray, Vector3 a,
            Vector3 b, Vector3 c, Vector3 normal,
            bool positiveSide, bool negativeSide);

        static Pair<bool, Real> Intersects(Ray ray, Vector3 a,
            Vector3 b, Vector3 c, Vector3 normal,
            bool positiveSide);

        static Pair<bool, Real> Intersects(Ray ray, Vector3 a,
            Vector3 b, Vector3 c, Vector3 normal );

        /** Ray / triangle intersection, returns boolean result and distance.
        @param
            ray The ray.
        @param
            a The triangle's first vertex.
        @param
            b The triangle's second vertex.
        @param
            c The triangle's third vertex.
        @param
            positiveSide Intersect with "positive side" of the triangle
        @param
            negativeSide Intersect with "negative side" of the triangle
        @returns
            If the ray is Intersects the triangle, a pair of <b>true</b> and the
            distance between intersection point and ray origin returned.
        @par
            If the ray isn't Intersects the triangle, a pair of <b>false</b> and
            <b>0</b> returned.
        */
        static Pair<bool, Real> Intersects(Ray ray, Vector3 a,
            Vector3 b, Vector3 c,
            bool positiveSide, bool negativeSide);

        static Pair<bool, Real> Intersects(Ray ray, Vector3 a,
            Vector3 b, Vector3 c,
            bool positiveSide);

        static Pair<bool, Real> Intersects(Ray ray, Vector3 a,
            Vector3 b, Vector3 c );

        /** Sphere / box intersection test. */
        static bool Intersects(Sphere sphere, AxisAlignedBox^ box);

        /** Plane / box intersection test. */
        static bool Intersects(Plane plane, AxisAlignedBox^ box);

        /** Ray / convex plane list intersection test. 
        @param ray The ray to test with
        @param plaeList List of planes which form a convex volume
        @param normalIsOutside Does the normal point outside the volume
        */
        static Pair<bool, Real> Intersects(
			Ray ray, Collections::Generic::List<Plane>^ planeList, 
            bool normalIsOutside);
        /** Ray / convex plane list intersection test. 
        @param ray The ray to test with
        @param plaeList List of planes which form a convex volume
        @param normalIsOutside Does the normal point outside the volume
        */
        static Pair<bool, Real> Intersects(
            Ray ray, Collections::Generic::IEnumerable<Plane>^ planeList, 
            bool normalIsOutside);

        /** Sphere / plane intersection test. 
        @remarks NB just do a plane.getDistance(sphere.getCenter()) for more detail!
        */
        static bool Intersects(Sphere sphere, Plane plane);

        /** Compare 2 reals, using tolerance for inaccuracies.
        */
        static bool RealEqual(Real a, Real b, Real tolerance);
        static bool RealEqual(Real a, Real b )
		{
			return RealEqual(a, b, Real::Epsilon);
		}

        /** Calculates the tangent space vector for a given set of positions / texture coords. */
        static Vector3 CalculateTangentSpaceVector(
            Vector3 position1, Vector3 position2, Vector3 position3,
            Real u1, Real v1, Real u2, Real v2, Real u3, Real v3);

        /** Build a reflection matrix for the passed in plane. */
        static Matrix4^ BuildReflectionMatrix(Plane p);
        /** Calculate a face normal, including the w component which is the offset from the origin. */
        static Vector4 CalculateFaceNormal(Vector3 v1, Vector3 v2, Vector3 v3);
        /** Calculate a face normal, no w-information. */
        static Vector3 CalculateBasicFaceNormal(Vector3 v1, Vector3 v2, Vector3 v3);
        /** Calculate a face normal without normalize, including the w component which is the offset from the origin. */
        static Vector4 CalculateFaceNormalWithoutNormalize(Vector3 v1, Vector3 v2, Vector3 v3);
        /** Calculate a face normal without normalize, no w-information. */
        static Vector3 CalculateBasicFaceNormalWithoutNormalize(Vector3 v1, Vector3 v2, Vector3 v3);

		/** Generates a value based on the gaussian (normal) distribution function
			with the given offset and scale parameters.
		*/
		static Real GaussianDistribution(Real x, Real offset, Real scale);
		static Real GaussianDistribution(Real x, Real offset)
		{
			return GaussianDistribution(x, offset, 1.0f);
		}
		static Real GaussianDistribution(Real x)
		{
			return GaussianDistribution(x, 0.0f, 1.0f);
		}

		literal Real POS_INFINITY = Real::PositiveInfinity;
		literal Real NEG_INFINITY = Real::NegativeInfinity;
		literal Real PI = Real( System::Math::PI );
        literal Real TWO_PI = Real( 2.0 * PI );
        literal Real HALF_PI = Real( 0.5 * PI );
		literal Real fDeg2Rad = PI / Real(180.0);
		literal Real fRad2Deg = Real(180.0) / PI;
	};

	// these functions must be defined down here, because they rely on the
	// angle unit conversion functions in class Math:

	inline Real Radian::ValueDegrees::get()
	{
		return Math::RadiansToDegrees ( mRad );
	}

	inline Real Radian::ValueAngleUnits::get()
	{
		return Math::RadiansToAngleUnits ( mRad );
	}

	inline Real Degree::ValueRadians::get()
	{
		return Math::DegreesToRadians ( mDeg );
	}

	inline Real Degree::ValueAngleUnits::get()
	{
		return Math::DegreesToAngleUnits ( mDeg );
	}

	inline Angle::operator Radian( Angle a )
	{
		return Radian(Math::AngleUnitsToRadians(a.mAngle));
	}

	inline Angle::operator Degree( Angle a )
	{
		return Degree(Math::AngleUnitsToDegrees(a.mAngle));
	}
}