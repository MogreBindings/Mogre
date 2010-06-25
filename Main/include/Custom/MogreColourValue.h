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

#include "OgreColourValue.h"
#include "Prerequisites.h"

namespace Mogre
{
	typedef Ogre::RGBA RGBA;
    typedef Ogre::ARGB ARGB;
    typedef Ogre::ABGR ABGR;
	typedef Ogre::BGRA BGRA;

	[Serializable]
	[StructLayout(LayoutKind::Sequential)]
	public value class ColourValue : IEquatable<ColourValue>
	{
	public:
		DEFINE_MANAGED_NATIVE_CONVERSIONS_FOR_VALUECLASS( ColourValue )

		static const ColourValue ZERO = ColourValue(0.0,0.0,0.0,0.0);
        static const ColourValue Black = ColourValue(0.0,0.0,0.0);
        static const ColourValue White = ColourValue(1.0,1.0,1.0);
        static const ColourValue Red = ColourValue(1.0,0.0,0.0);
        static const ColourValue Green = ColourValue(0.0,1.0,0.0);
        static const ColourValue Blue = ColourValue(0.0,0.0,1.0);

	    explicit ColourValue( float red,
				    float green,
				    float blue,
				    float alpha ) : r(red), g(green), b(blue), a(alpha)
        { }
	    explicit ColourValue( float red,
				    float green,
				    float blue ) : r(red), g(green), b(blue), a(1.0f)
        { }

	    static bool operator==(ColourValue lhs, ColourValue rhs);
	    static bool operator!=(ColourValue lhs, ColourValue rhs);

		virtual bool Equals(ColourValue other) { return *this == other; }

        float r,g,b,a;

	    /** Retrieves colour as RGBA.
	    */
	    RGBA GetAsRGBA(void);

	    /** Retrieves colour as ARGB.
	    */
	    ARGB GetAsARGB(void);

		/** Retrieves colour as BGRA.
		*/
		BGRA GetAsBGRA(void);

		/** Retrieves colours as ABGR */
	    ABGR GetAsABGR(void);

	    /** Sets colour as RGBA.
	    */
        void SetAsRGBA(const RGBA val);

	    /** Sets colour as ARGB.
	    */
        void SetAsARGB(const ARGB val);

		/** Sets colour as BGRA.
		*/
		void SetAsBGRA(const BGRA val);

	    /** Sets colour as ABGR.
	    */
        void SetAsABGR(const ABGR val);

        /** Clamps colour value to the range [0, 1].
        */
        void Saturate(void)
        {
            if (r < 0)
                r = 0;
            else if (r > 1)
                r = 1;

            if (g < 0)
                g = 0;
            else if (g > 1)
                g = 1;

            if (b < 0)
                b = 0;
            else if (b > 1)
                b = 1;

            if (a < 0)
                a = 0;
            else if (a > 1)
                a = 1;
        }

        /** As saturate, except that this colour value is unaffected and
            the saturated colour value is returned as a copy. */
        ColourValue SaturateCopy(void)
        {
            ColourValue ret = *this;
            ret.Saturate();
            return ret;
        }

        // arithmetic operations
        inline static ColourValue operator + ( ColourValue lcol, ColourValue rkVector )
        {
            ColourValue kSum;

            kSum.r = lcol.r + rkVector.r;
            kSum.g = lcol.g + rkVector.g;
            kSum.b = lcol.b + rkVector.b;
            kSum.a = lcol.a + rkVector.a;

            return kSum;
        }

        inline static ColourValue operator - ( ColourValue lcol, ColourValue rkVector )
        {
            ColourValue kDiff;

            kDiff.r = lcol.r - rkVector.r;
            kDiff.g = lcol.g - rkVector.g;
            kDiff.b = lcol.b - rkVector.b;
            kDiff.a = lcol.a - rkVector.a;

            return kDiff;
        }

        inline static ColourValue operator * (ColourValue lcol, const float fScalar )
        {
            ColourValue kProd;

            kProd.r = fScalar*lcol.r;
            kProd.g = fScalar*lcol.g;
            kProd.b = fScalar*lcol.b;
            kProd.a = fScalar*lcol.a;

            return kProd;
        }

        inline static ColourValue operator * ( ColourValue lcol, ColourValue rhs)
        {
            ColourValue kProd;

            kProd.r = rhs.r * lcol.r;
            kProd.g = rhs.g * lcol.g;
            kProd.b = rhs.b * lcol.b;
            kProd.a = rhs.a * lcol.a;

            return kProd;
        }

        inline static ColourValue operator / ( ColourValue lcol, ColourValue rhs)
        {
            ColourValue kProd;

            kProd.r = rhs.r / lcol.r;
            kProd.g = rhs.g / lcol.g;
            kProd.b = rhs.b / lcol.b;
            kProd.a = rhs.a / lcol.a;

            return kProd;
        }

        inline static ColourValue operator / (ColourValue lcol, const float fScalar )
        {
            assert( fScalar != 0.0 );

            ColourValue kDiv;

            float fInv = 1.0 / fScalar;
            kDiv.r = lcol.r * fInv;
            kDiv.g = lcol.g * fInv;
            kDiv.b = lcol.b * fInv;
            kDiv.a = lcol.a * fInv;

            return kDiv;
        }

        inline static ColourValue operator * (const float fScalar, ColourValue rkVector )
        {
            ColourValue kProd;

            kProd.r = fScalar * rkVector.r;
            kProd.g = fScalar * rkVector.g;
            kProd.b = fScalar * rkVector.b;
            kProd.a = fScalar * rkVector.a;

            return kProd;
        }

		/** Set a colour value from Hue, Saturation and Brightness.
		@param hue Hue value, scaled to the [0,1] range as opposed to the 0-360
		@param saturation Saturation level, [0,1]
		@param brightness Brightness level, [0,1]
		*/
		void SetHSB(Real hue, Real saturation, Real brightness);


		/** Function for writing to a stream.
		*/
		virtual System::String^ ToString() override
        {
			return System::String::Format("ColourValue({0}, {1}, {2}, {3})", r, g, b, a);
        }
	};
}