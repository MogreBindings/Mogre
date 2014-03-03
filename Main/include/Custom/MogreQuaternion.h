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

#pragma warning(push, 0)
#pragma managed(push, off)
#include "OgreQuaternion.h"
#pragma managed(pop)
#pragma warning(pop)
#include "Marshalling.h"
#include "Prerequisites.h"
#include "Custom\MogreMath.h"
#include "Custom\MogreVector3.h"

namespace Mogre
{
    ref class Matrix3;

    /** <summary>Implementation of a Quaternion, i.e. a rotation around an axis.</summary>
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
        /// <summary>Construct a quaternion from a rotation matrix</summary>
        inline Quaternion(Matrix3^ rot)
        {
            this->FromRotationMatrix(rot);
        }
        /// <summary>Construct a quaternion from an angle/axis</summary>
        inline Quaternion(Radian rfAngle, Vector3 rkAxis)
        {
            this->FromAngleAxis(rfAngle, rkAxis);
        }
        /// <summary>Construct a quaternion from 3 orthonormal local axes</summary>
        inline Quaternion(Vector3 xaxis, Vector3 yaxis, Vector3 zaxis)
        {
            this->FromAxes(xaxis, yaxis, zaxis);
        }
        /// <summary>Construct a quaternion from 3 orthonormal local axes</summary>
        inline Quaternion(array<Vector3>^ akAxis)
        {
            this->FromAxes(akAxis);
        }
        /// <summary>Construct a quaternion from 4 manual w/x/y/z values</summary>
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
        /// <summary>Get the local x-axis</summary>
        property Vector3 XAxis
        {
            Vector3 get();
        }
        /// <summary>Get the local y-axis</summary>
        property Vector3 YAxis
        {
            Vector3 get();
        }
        /// <summary>Get the local z-axis</summary>
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
        /// <summary>Normalises this quaternion, and returns the previous length</summary>
        Real Normalise(); 
        Quaternion Inverse();  // apply to non-zero quaternion
        Quaternion UnitInverse();  // apply to unit-length quaternion
        Quaternion Exp();
        Quaternion Log();

        // rotation of a vector by a quaternion
        static Vector3 operator* (Quaternion lquat, Vector3 rkVector);

        /// <summary>Calculate the local roll element of this quaternion</summary>
        property Radian Roll
        {
            Radian get();
        }
        /// <summary>Calculate the local pitch element of this quaternion</summary>
        property Radian Pitch
        {
            Radian get();
        }
        /// <summary>Calculate the local yaw element of this quaternion</summary>
        property Radian Yaw
        {
            Radian get();
        }

        /** <summary>Calculate the local roll element of this quaternion.</summary>
        <param name="reprojectAxis">By default the method returns the 'intuitive' result
        that is, if you projected the local Y of the quaternion onto the X and
        Y axes, the angle between them is returned. If set to false though, the
        result is the actual yaw that will be used to implement the quaternion,
        which is the shortest possible path to get to the same orientation and 
        may involve less axial rotation.</param>
        */
        Radian GetRoll(bool reprojectAxis);
        /** <summary>Calculate the local pitch element of this quaternion</summary>
        <param name="reprojectAxis">By default the method returns the 'intuitive' result
        that is, if you projected the local Z of the quaternion onto the X and
        Y axes, the angle between them is returned. If set to true though, the
        result is the actual yaw that will be used to implement the quaternion,
        which is the shortest possible path to get to the same orientation and 
        may involve less axial rotation.</param>
        */
        Radian GetPitch(bool reprojectAxis);
        /** <summary>Calculate the local yaw element of this quaternion</summary>
        <param name="reprojectAxis">By default the method returns the 'intuitive' result
        that is, if you projected the local Z of the quaternion onto the X and
        Z axes, the angle between them is returned. If set to true though, the
        result is the actual yaw that will be used to implement the quaternion,
        which is the shortest possible path to get to the same orientation and 
        may involve less axial rotation.</param>
        */
        Radian GetYaw(bool reprojectAxis);

        /// <summary>Equality with tolerance (tolerance is max angle difference)</summary>
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
        static initonly Real ms_fEpsilon = 1e-03;

        // special values
        static initonly Quaternion ZERO = Quaternion(0.0,0.0,0.0,0.0);
        static initonly Quaternion IDENTITY = Quaternion(1.0,0.0,0.0,0.0);

        Real w, x, y, z;

        /// <summary>Check whether this quaternion contains valid values</summary>
        property bool IsNaN
        {
            inline bool get()
            {
                return Real::IsNaN(x) || Real::IsNaN(y) || Real::IsNaN(z) || Real::IsNaN(w);
            }
        }

        virtual System::String^ ToString() override
        {
            return System::String::Format("Quaternion({0}, {1}, {2}, {3})", w, x, y, z);
        }
    };
}