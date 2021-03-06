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
#include "MogreStableHeaders.h"

#include "Custom\MogrePlane.h"
#include "Custom\MogreMatrix3.h"

namespace Mogre
{
    //-----------------------------------------------------------------------
    Plane::Plane (Vector3 rkNormal, Real fConstant)
    {
        normal = rkNormal;
        d = -fConstant;
    }
    //-----------------------------------------------------------------------
    Plane::Plane (Vector3 rkNormal, Vector3 rkPoint)
    {
        normal = rkNormal;
        d = -rkNormal.DotProduct(rkPoint);
    }
    //-----------------------------------------------------------------------
    Plane::Plane (Vector3 rkPoint0, Vector3 rkPoint1,
        Vector3 rkPoint2)
    {
        Redefine(rkPoint0, rkPoint1, rkPoint2);
    }
    //-----------------------------------------------------------------------
    Real Plane::GetDistance (Vector3 rkPoint)
    {
        return normal.DotProduct(rkPoint) + d;
    }
    //-----------------------------------------------------------------------
    Plane::Side Plane::GetSide (Vector3 rkPoint)
    {
        Real fDistance = GetDistance(rkPoint);

        if ( fDistance < 0.0 )
			return Plane::Side::NEGATIVE_SIDE;

        if ( fDistance > 0.0 )
			return Plane::Side::POSITIVE_SIDE;

		return Plane::Side::NO_SIDE;
    }
	//-----------------------------------------------------------------------
	Plane::Side Plane::GetSide (AxisAlignedBox^ box)
	{
		if (box->IsNull) 
			return Plane::Side::NO_SIDE;
		if (box->IsInfinite)
			return Plane::Side::BOTH_SIDE;

        return GetSide(box->Center, box->HalfSize);
	}
    //-----------------------------------------------------------------------
    Plane::Side Plane::GetSide (Vector3 centre, Vector3 halfSize)
    {
        // Calculate the distance between box centre and the plane
        Real dist = GetDistance(centre);

        // Calculate the maximise allows absolute distance for
        // the distance between box centre and plane
        Real maxAbsDist = normal.AbsDotProduct(halfSize);

        if (dist < -maxAbsDist)
            return Plane::Side::NEGATIVE_SIDE;

        if (dist > +maxAbsDist)
            return Plane::Side::POSITIVE_SIDE;

        return Plane::Side::BOTH_SIDE;
    }
    //-----------------------------------------------------------------------
    void Plane::Redefine(Vector3 rkPoint0, Vector3 rkPoint1,
        Vector3 rkPoint2)
    {
        Vector3 kEdge1 = rkPoint1 - rkPoint0;
        Vector3 kEdge2 = rkPoint2 - rkPoint0;
        normal = kEdge1.CrossProduct(kEdge2);
        normal.Normalise();
        d = -normal.DotProduct(rkPoint0);
    }
	//-----------------------------------------------------------------------
	void Plane::Redefine(Vector3 rkNormal, Vector3 rkPoint)
	{
		normal = rkNormal;
		d = -rkNormal.DotProduct(rkPoint);
	}
	//-----------------------------------------------------------------------
	Vector3 Plane::ProjectVector(Vector3 p)
	{
		// We know plane normal is unit length, so use simple method
		Matrix3^ xform = gcnew Matrix3;
		xform->m00 = normal.x * normal.x - 1.0f;
		xform->m01 = normal.x * normal.y;
		xform->m02 = normal.x * normal.z;
		xform->m10 = normal.y * normal.x;
		xform->m11 = normal.y * normal.y - 1.0f;
		xform->m12 = normal.y * normal.z;
		xform->m20 = normal.z * normal.x;
		xform->m21 = normal.z * normal.y;
		xform->m22 = normal.z * normal.z - 1.0f;
		return xform * p;

	}
	//-----------------------------------------------------------------------
    Real Plane::Normalise()
    {
        Real fLength = normal.Length;

        // Will also work for zero-sized vectors, but will change nothing
        if (fLength > 1e-08f)
        {
            Real fInvLength = 1.0f / fLength;
            normal *= fInvLength;
            d *= fInvLength;
        }

        return fLength;
    }
    //-----------------------------------------------------------------------
	String^ Plane::ToString()
    {
		return String::Format("Plane(normal={0}, d={1})", normal, d);
    }
}