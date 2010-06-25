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

#include "Custom\MogreMatrix4.h"

namespace Mogre
{
    //-----------------------------------------------------------------------
    inline static Real
        MINOR(Matrix4^ m, const size_t r0, const size_t r1, const size_t r2, 
								const size_t c0, const size_t c1, const size_t c2)
    {
        return m[r0, c0] * (m[r1, c1] * m[r2, c2] - m[r2, c1] * m[r1, c2]) -
            m[r0, c1] * (m[r1, c0] * m[r2, c2] - m[r2, c0] * m[r1, c2]) +
            m[r0, c2] * (m[r1, c0] * m[r2, c1] - m[r2, c0] * m[r1, c1]);
    }
    //-----------------------------------------------------------------------
    Matrix4^ Matrix4::Adjoint()
    {
        return gcnew Matrix4( MINOR(this, 1, 2, 3, 1, 2, 3),
            -MINOR(this, 0, 2, 3, 1, 2, 3),
            MINOR(this, 0, 1, 3, 1, 2, 3),
            -MINOR(this, 0, 1, 2, 1, 2, 3),

            -MINOR(this, 1, 2, 3, 0, 2, 3),
            MINOR(this, 0, 2, 3, 0, 2, 3),
            -MINOR(this, 0, 1, 3, 0, 2, 3),
            MINOR(this, 0, 1, 2, 0, 2, 3),

            MINOR(this, 1, 2, 3, 0, 1, 3),
            -MINOR(this, 0, 2, 3, 0, 1, 3),
            MINOR(this, 0, 1, 3, 0, 1, 3),
            -MINOR(this, 0, 1, 2, 0, 1, 3),

            -MINOR(this, 1, 2, 3, 0, 1, 2),
            MINOR(this, 0, 2, 3, 0, 1, 2),
            -MINOR(this, 0, 1, 3, 0, 1, 2),
            MINOR(this, 0, 1, 2, 0, 1, 2));
    }
    //-----------------------------------------------------------------------
	Real Matrix4::Determinant::get()
    {
        return m00 * MINOR(this, 1, 2, 3, 1, 2, 3) -
            m01 * MINOR(this, 1, 2, 3, 0, 2, 3) +
            m02 * MINOR(this, 1, 2, 3, 0, 1, 3) -
            m03 * MINOR(this, 1, 2, 3, 0, 1, 2);
    }
    //-----------------------------------------------------------------------
    Matrix4^ Matrix4::Inverse()
    {
        return Adjoint() * (1.0f / Determinant);
    }
    //-----------------------------------------------------------------------
    void Matrix4::MakeTransform(Vector3 position, Vector3 scale, Quaternion orientation)
    {
        // Ordering:
        //    1. Scale
        //    2. Rotate
        //    3. Translate

        Matrix3^ rot3x3, ^scale3x3;
        rot3x3 = orientation.ToRotationMatrix();
        scale3x3 = Matrix3::ZERO;
        scale3x3->m00 = scale.x;
        scale3x3->m11 = scale.y;
        scale3x3->m22 = scale.z;

        // Set up final matrix with scale, rotation and translation
        *this = rot3x3 * scale3x3;
        this->SetTrans(position);

        // No projection term
        m30 = 0; m31 = 0; m32 = 0; m33 = 1;
    }
    //-----------------------------------------------------------------------
    void Matrix4::MakeInverseTransform(Vector3 position, Vector3 scale, Quaternion orientation)
    {
        // Invert the parameters
        Vector3 invTranslate = -position;
        Vector3 invScale(1 / scale.x, 1 / scale.y, 1 / scale.z);
        Quaternion invRot = orientation.Inverse();

        // Because we're inverting, order is translation, rotation, scale
        // So make translation relative to scale & rotation
        invTranslate *= invScale; // scale
        invTranslate = invRot * invTranslate; // rotate

        // Next, make a 3x3 rotation matrix and apply inverse scale
        Matrix3^ rot3x3, ^scale3x3;
        rot3x3 = invRot.ToRotationMatrix();
        scale3x3 = Matrix3::ZERO;
        scale3x3->m00 = invScale.x;
        scale3x3->m11 = invScale.y;
        scale3x3->m22 = invScale.z;

        // Set up final matrix with scale, rotation and translation
        *this = scale3x3 * rot3x3;
        this->SetTrans(invTranslate);

        // No projection term
        m30 = 0; m31 = 0; m32 = 0; m33 = 1;
    }
    //-----------------------------------------------------------------------
    Matrix4^ Matrix4::InverseAffine()
    {
		if (!IsAffine)
			throw gcnew Exception("This matrix must be affine");

        Real m10 = this->m10, m11 = this->m11, m12 = this->m12;
        Real m20 = this->m20, m21 = this->m21, m22 = this->m22;

        Real t00 = m22 * m11 - m21 * m12;
        Real t10 = m20 * m12 - m22 * m10;
        Real t20 = m21 * m10 - m20 * m11;

        Real m00 = this->m00, m01 = this->m01, m02 = this->m02;

        Real invDet = 1 / (m00 * t00 + m01 * t10 + m02 * t20);

        t00 *= invDet; t10 *= invDet; t20 *= invDet;

        m00 *= invDet; m01 *= invDet; m02 *= invDet;

        Real r00 = t00;
        Real r01 = m02 * m21 - m01 * m22;
        Real r02 = m01 * m12 - m02 * m11;

        Real r10 = t10;
        Real r11 = m00 * m22 - m02 * m20;
        Real r12 = m02 * m10 - m00 * m12;

        Real r20 = t20;
        Real r21 = m01 * m20 - m00 * m21;
        Real r22 = m00 * m11 - m01 * m10;

        Real m03 = this->m03, m13 = this->m13, m23 = this->m23;

        Real r03 = - (r00 * m03 + r01 * m13 + r02 * m23);
        Real r13 = - (r10 * m03 + r11 * m13 + r12 * m23);
        Real r23 = - (r20 * m03 + r21 * m13 + r22 * m23);

        return gcnew Matrix4(
            r00, r01, r02, r03,
            r10, r11, r12, r13,
            r20, r21, r22, r23,
              0,   0,   0,   1);
    }
}