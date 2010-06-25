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
	String^ Plane::ToString()
    {
		return String::Format("Plane(normal={0}, d={1})", normal, d);
    }
}