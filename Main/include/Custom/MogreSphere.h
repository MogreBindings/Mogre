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

#pragma managed(push, off)
#include "OgreSphere.h"
#pragma managed(pop)
#include "Prerequisites.h"
#include "Custom\MogreMath.h"
#include "Custom\MogreVector3.h"
#include "Custom\MogrePlane.h"

namespace Mogre
{
    /** <summary>A sphere primitive, mostly used for bounds checking.</summary>
    <remarks>
    A sphere in math texts is normally represented by the function
    x^2 + y^2 + z^2 = r^2 (for sphere's centered on the origin). Ogre stores spheres
    simply as a center point and a radius.
    </remarks>
    */
    [Serializable]
    public value class Sphere
    {
    protected:
        Real mRadius;
        Vector3 mCenter;
    public:
        DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( Sphere )

        /** <summary>Constructor allowing arbitrary spheres.</summary>
        <param name="center">The center point of the sphere.</param>
        <param name="radius">The radius of the sphere.</param>
        */
        Sphere(Vector3 center, Real radius)
        : mRadius(radius), mCenter(center) {}

        /** <summary>Gets or Sets the radius of the sphere.</summary> */
        property Real Radius
        {
            Real get() { return mRadius; }
            void set(Real radius) { mRadius = radius; }
        }

        /** <summary>Gets or Sets the center point of the sphere.</summary> */
        property Vector3 Center
        {
            Vector3 get() { return mCenter; }
            void set(Vector3 center) { mCenter = center; }
        }

        /** <summary>Returns whether or not this sphere interects another sphere.</summary> */
        bool Intersects(Sphere s)
        {
            return (s.mCenter - mCenter).Length <=
                (s.mRadius + mRadius);
        }
        /** <summary>Returns whether or not this sphere interects a box.</summary> */
        bool Intersects(AxisAlignedBox^ box)
        {
            return Math::Intersects(*this, box);
        }
        /** <summary>Returns whether or not this sphere interects a plane.</summary> */
        bool Intersects(Plane plane)
        {
            return Math::Intersects(*this, plane);
        }
        /** <summary>Returns whether or not this sphere interects a point.</summary> */
        bool Intersects(Vector3 v)
        {
            return ((v - mCenter).Length <= mRadius);
        }


    };
}