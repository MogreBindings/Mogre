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

#include "OgreAxisAlignedBox.h"
#include "Prerequisites.h"
#include "Custom\MogreVector3.h"
#include "Custom\MogreMatrix4.h"
#include "MogreSphere.h"

namespace Mogre {

	/** A 3D box aligned with the x/y/z axes.
	@remarks
	This class represents a simple box which is aligned with the
	axes. Internally it only stores 2 points as the extremeties of
	the box, one which is the minima of all 3 axes, and the other
	which is the maxima of all 3 axes. This class is typically used
	for an axis-aligned bounding box (AABB) for collision and
	visibility determination.
	*/
	[Serializable]
	public ref class AxisAlignedBox
	{
	public:
		static operator AxisAlignedBox^ (const Ogre::AxisAlignedBox& box)
		{
			AxisAlignedBox^ clr_box = gcnew AxisAlignedBox(box.getMinimum(), box.getMaximum());
			if (box.isNull())
				clr_box->SetNull();
			else if (box.isInfinite())
				clr_box->SetInfinite();

			return clr_box;
		}

		static operator Ogre::AxisAlignedBox (AxisAlignedBox^ box)
		{
			Ogre::AxisAlignedBox o_box((Ogre::Vector3&)box->mMinimum, (Ogre::Vector3&)box->mMaximum);
			if (box->IsNull)
				o_box.setNull();
			else if (box->IsInfinite)
				o_box.setInfinite();

			return o_box;
		}

	protected:
		enum class Extent
		{
			EXTENT_NULL,
			EXTENT_FINITE,
			EXTENT_INFINITE
		};

		Vector3 mMinimum;
		Vector3 mMaximum;
		Extent mExtent;

		[NonSerialized]
		array<Vector3>^ mpCorners;

	public:
		/*
		1-----2
		/|    /|
		/ |   / |
		5-----4  |
		|  0--|--3
		| /   | /
		|/    |/
		6-----7
		*/
		enum class CornerEnum {
			FAR_LEFT_BOTTOM = Ogre::AxisAlignedBox::FAR_LEFT_BOTTOM,
			FAR_LEFT_TOP = Ogre::AxisAlignedBox::FAR_LEFT_TOP,
			FAR_RIGHT_TOP = Ogre::AxisAlignedBox::FAR_RIGHT_TOP,
			FAR_RIGHT_BOTTOM = Ogre::AxisAlignedBox::FAR_RIGHT_BOTTOM,
			NEAR_RIGHT_BOTTOM = Ogre::AxisAlignedBox::NEAR_RIGHT_BOTTOM,
			NEAR_LEFT_BOTTOM = Ogre::AxisAlignedBox::NEAR_LEFT_BOTTOM,
			NEAR_LEFT_TOP = Ogre::AxisAlignedBox::NEAR_LEFT_TOP,
			NEAR_RIGHT_TOP = Ogre::AxisAlignedBox::NEAR_RIGHT_TOP
		};
		AxisAlignedBox()
		{
			// Default to a null box 
			SetMinimum( -0.5, -0.5, -0.5 );
			SetMaximum( 0.5, 0.5, 0.5 );
			mExtent = Extent::EXTENT_NULL;
		}

		AxisAlignedBox(AxisAlignedBox^ rkBox)
		{
			if (rkBox->IsNull)
				SetNull();
			else if (rkBox->IsInfinite)
				SetInfinite();
			else
				SetExtents( rkBox->mMinimum, rkBox->mMaximum );
		}

		AxisAlignedBox( Vector3 min, Vector3 max )
		{
			SetExtents( min, max );
		}

		AxisAlignedBox(
			Real mx, Real my, Real mz,
			Real Mx, Real My, Real Mz )
		{
			SetExtents( mx, my, mz, Mx, My, Mz );
		}


		/** Gets the minimum corner of the box.
		*/
		property Vector3 Minimum
		{
			Vector3 get() { return mMinimum; }
		}

		/** Gets the maximum corner of the box.
		*/
		property Vector3 Maximum
		{ 
			Vector3 get() { return mMaximum; }
		}

		/** Sets the minimum corner of the box.
		*/
		inline void SetMinimum( Vector3 vec )
		{
			mExtent = Extent::EXTENT_FINITE;
			mMinimum = vec;
		}

		inline void SetMinimum( Real x, Real y, Real z )
		{
			mExtent = Extent::EXTENT_FINITE;
			mMinimum.x = x;
			mMinimum.y = y;
			mMinimum.z = z;
		}

		/** Changes one of the components of the minimum corner of the box
		used to resize only one dimension of the box
		*/
		inline void SetMinimumX(Real x)
		{
			mMinimum.x = x;
		}

		inline void SetMinimumY(Real y)
		{
			mMinimum.y = y;
		}

		inline void SetMinimumZ(Real z)
		{
			mMinimum.z = z;
		}

		/** Sets the maximum corner of the box.
		*/
		inline void SetMaximum( Vector3 vec )
		{
			mExtent = Extent::EXTENT_FINITE;
			mMaximum = vec;
		}

		inline void SetMaximum( Real x, Real y, Real z )
		{
			mExtent = Extent::EXTENT_FINITE;
			mMaximum.x = x;
			mMaximum.y = y;
			mMaximum.z = z;
		}

		/** Changes one of the components of the maximum corner of the box
		used to resize only one dimension of the box
		*/
		inline void SetMaximumX( Real x )
		{
			mMaximum.x = x;
		}

		inline void SetMaximumY( Real y )
		{
			mMaximum.y = y;
		}

		inline void SetMaximumZ( Real z )
		{
			mMaximum.z = z;
		}

		/** Sets both minimum and maximum extents at once.
		*/
		inline void SetExtents( Vector3 min, Vector3 max )
		{
			mExtent = Extent::EXTENT_FINITE;
			mMinimum = min;
			mMaximum = max;
		}

		inline void SetExtents(
			Real mx, Real my, Real mz,
			Real Mx, Real My, Real Mz )
		{
			mExtent = Extent::EXTENT_FINITE;

			mMinimum.x = mx;
			mMinimum.y = my;
			mMinimum.z = mz;

			mMaximum.x = Mx;
			mMaximum.y = My;
			mMaximum.z = Mz;

		}

		/** Returns a pointer to an array of 8 corner points, useful for
		collision vs. non-aligned objects.
		@remarks
		If the order of these corners is important, they are as
		follows: The 4 points of the minimum Z face (note that
		because Ogre uses right-handed coordinates, the minimum Z is
		at the 'back' of the box) starting with the minimum point of
		all, then anticlockwise around this face (if you are looking
		onto the face from outside the box). Then the 4 points of the
		maximum Z face, starting with maximum point of all, then
		anticlockwise around this face (looking onto the face from
		outside the box). Like this:
		<pre>
		1-----2
		/|    /|
		/ |   / |
		5-----4  |
		|  0--|--3
		| /   | /
		|/    |/
		6-----7
		</pre>
		@remarks as this implementation uses a static member, make sure to use your own copy !
		*/
		array<Vector3>^ GetAllCorners()
		{
			if (mExtent != Extent::EXTENT_FINITE)
				throw gcnew System::Exception("Can't get corners of a null or infinite AAB");

			// The order of these items is, using right-handed co-ordinates:
			// Minimum Z face, starting with Min(all), then anticlockwise
			//   around face (looking onto the face)
			// Maximum Z face, starting with Max(all), then anticlockwise
			//   around face (looking onto the face)
			// Only for optimization/compatibility.
			if (!mpCorners)
				mpCorners = gcnew array<Vector3>(8);

			mpCorners[0] = mMinimum;
			mpCorners[1].x = mMinimum.x; mpCorners[1].y = mMaximum.y; mpCorners[1].z = mMinimum.z;
			mpCorners[2].x = mMaximum.x; mpCorners[2].y = mMaximum.y; mpCorners[2].z = mMinimum.z;
			mpCorners[3].x = mMaximum.x; mpCorners[3].y = mMinimum.y; mpCorners[3].z = mMinimum.z;            

			mpCorners[4] = mMaximum;
			mpCorners[5].x = mMinimum.x; mpCorners[5].y = mMaximum.y; mpCorners[5].z = mMaximum.z;
			mpCorners[6].x = mMinimum.x; mpCorners[6].y = mMinimum.y; mpCorners[6].z = mMaximum.z;
			mpCorners[7].x = mMaximum.x; mpCorners[7].y = mMinimum.y; mpCorners[7].z = mMaximum.z;

			return mpCorners;
		}

		/** gets the position of one of the corners
		*/
		Vector3 GetCorner(CornerEnum cornerToGet)
		{
			switch(cornerToGet)
			{
			case CornerEnum::FAR_LEFT_BOTTOM:
				return mMinimum;
			case CornerEnum::FAR_LEFT_TOP:
				return Vector3(mMinimum.x, mMaximum.y, mMinimum.z);
			case CornerEnum::FAR_RIGHT_TOP:
				return Vector3(mMaximum.x, mMaximum.y, mMinimum.z);
			case CornerEnum::FAR_RIGHT_BOTTOM:
				return Vector3(mMaximum.x, mMinimum.y, mMinimum.z);
			case CornerEnum::NEAR_RIGHT_BOTTOM:
				return Vector3(mMaximum.x, mMinimum.y, mMaximum.z);
			case CornerEnum::NEAR_LEFT_BOTTOM:
				return Vector3(mMinimum.x, mMinimum.y, mMaximum.z);
			case CornerEnum::NEAR_LEFT_TOP:
				return Vector3(mMinimum.x, mMaximum.y, mMaximum.z);
			case CornerEnum::NEAR_RIGHT_TOP:
				return mMaximum;
			default:
				return Vector3();
			}
		}

		virtual String^ ToString() override
		{
			switch (mExtent)
			{
			case Extent::EXTENT_NULL:
				return "AxisAlignedBox(null)";

			case Extent::EXTENT_FINITE:
				return "AxisAlignedBox(min=" + mMinimum + ", max=" + mMaximum + ")";

			case Extent::EXTENT_INFINITE:
				return "AxisAlignedBox(infinite)";

			default: // shut up compiler
				throw gcnew Exception("Should never reach here" );
			}
		}

		/** Merges the passed in box into the current box. The result is the
		box which encompasses both.
		*/
		void Merge( AxisAlignedBox^ rhs )
		{
			// Do nothing if rhs null, or this is infinite
			if ((rhs->mExtent == Extent::EXTENT_NULL) || (mExtent == Extent::EXTENT_INFINITE))
			{
				return;
			}
			// Otherwise if rhs is infinite, make this infinite, too
			else if (rhs->mExtent == Extent::EXTENT_INFINITE)
			{
				mExtent = Extent::EXTENT_INFINITE;
			}
			// Otherwise if current null, just take rhs
			else if (mExtent == Extent::EXTENT_NULL)
			{
				SetExtents(rhs->mMinimum, rhs->mMaximum);
			}
			// Otherwise merge
			else
			{
				Vector3 min = mMinimum;
				Vector3 max = mMaximum;
				max.MakeCeil(rhs->mMaximum);
				min.MakeFloor(rhs->mMinimum);

				SetExtents(min, max);
			}

		}

		/** Extends the box to encompass the specified point (if needed).
		*/
		inline void Merge( Vector3 point )
		{
			switch (mExtent)
			{
			case Extent::EXTENT_NULL: // if null, use this point
				SetExtents(point, point);
				return;

			case Extent::EXTENT_FINITE:
				mMaximum.MakeCeil(point);
				mMinimum.MakeFloor(point);
				return;

			case Extent::EXTENT_INFINITE: // if infinite, makes no difference
				return;
			}

			throw gcnew Exception(" Should never reach here" );
		}

		/** Transforms the box according to the matrix supplied.
		@remarks
		By calling this method you get the axis-aligned box which
		surrounds the transformed version of this box. Therefore each
		corner of the box is transformed by the matrix, then the
		extents are mapped back onto the axes to produce another
		AABB. Useful when you have a local AABB for an object which
		is then transformed.
		*/
		inline void Transform( Matrix4^ matrix )
		{
			// Do nothing if current null or infinite
			if( mExtent != Extent::EXTENT_FINITE )
				return;

			Vector3 oldMin, oldMax, currentCorner;

			// Getting the old values so that we can use the existing merge method.
			oldMin = mMinimum;
			oldMax = mMaximum;

			// We sequentially compute the corners in the following order :
			// 0, 6, 5, 1, 2, 4 ,7 , 3
			// This sequence allows us to only change one member at a time to get at all corners.

			// For each one, we transform it using the matrix
			// Which gives the resulting point and merge the resulting point.

			// First corner 
			// min min min
			currentCorner = oldMin;
			Merge( matrix * currentCorner );

			// min,min,max
			currentCorner.z = oldMax.z;
			Merge( matrix * currentCorner );

			// min max max
			currentCorner.y = oldMax.y;
			Merge( matrix * currentCorner );

			// min max min
			currentCorner.z = oldMin.z;
			Merge( matrix * currentCorner );

			// max max min
			currentCorner.x = oldMax.x;
			Merge( matrix * currentCorner );

			// max max max
			currentCorner.z = oldMax.z;
			Merge( matrix * currentCorner );

			// max min max
			currentCorner.y = oldMin.y;
			Merge( matrix * currentCorner );

			// max min min
			currentCorner.z = oldMin.z;
			Merge( matrix * currentCorner ); 
		}

		/** Transforms the box according to the affine matrix supplied.
		@remarks
		By calling this method you get the axis-aligned box which
		surrounds the transformed version of this box. Therefore each
		corner of the box is transformed by the matrix, then the
		extents are mapped back onto the axes to produce another
		AABB. Useful when you have a local AABB for an object which
		is then transformed.
		@note
		The matrix must be an affine matrix. @see Matrix4::isAffine.
		*/
		void TransformAffine(Matrix4^ m)
		{
			if (!m->IsAffine)
				throw gcnew ArgumentException("Matrix should be Affine", "m");

			// Do nothing if current null or infinite
			if ( mExtent != Extent::EXTENT_FINITE )
				return;

			Vector3 centre = Center;
			Vector3 halfSize = HalfSize;

			Vector3 newCentre = m->TransformAffine(centre);
			Vector3 newHalfSize(
				Math::Abs(m->m00) * halfSize.x + Math::Abs(m->m01) * halfSize.y + Math::Abs(m->m02) * halfSize.z, 
				Math::Abs(m->m10) * halfSize.x + Math::Abs(m->m11) * halfSize.y + Math::Abs(m->m12) * halfSize.z,
				Math::Abs(m->m20) * halfSize.x + Math::Abs(m->m21) * halfSize.y + Math::Abs(m->m22) * halfSize.z);

			SetExtents(newCentre - newHalfSize, newCentre + newHalfSize);
		}

		/** Sets the box to a 'null' value i.e. not a box.
		*/
		inline void SetNull()
		{
			mExtent = Extent::EXTENT_NULL;
		}

		/** Returns true if the box is null i.e. empty.
		*/
		property bool IsNull
		{
			bool get() { return (mExtent == Extent::EXTENT_NULL); }
		}

		/** Returns true if the box is finite.
		*/
		property bool IsFinite
		{
			bool get() { return (mExtent == Extent::EXTENT_FINITE); }
		}

		/** Sets the box to 'infinite'
		*/
		inline void SetInfinite()
		{
			mExtent = Extent::EXTENT_INFINITE;
		}

		/** Returns true if the box is infinite.
		*/
		property bool IsInfinite
		{
			bool get() { return (mExtent == Extent::EXTENT_INFINITE); }
		}

		/** Returns whether or not this box intersects another. */
		inline bool Intersects(AxisAlignedBox^ b2)
		{
			// Early-fail for nulls
			if (this->IsNull || b2->IsNull)
				return false;

			// Early-success for infinites
			if (this->IsInfinite || b2->IsInfinite)
				return true;

			// Use up to 6 separating planes
			if (mMaximum.x < b2->mMinimum.x)
				return false;
			if (mMaximum.y < b2->mMinimum.y)
				return false;
			if (mMaximum.z < b2->mMinimum.z)
				return false;

			if (mMinimum.x > b2->mMaximum.x)
				return false;
			if (mMinimum.y > b2->mMaximum.y)
				return false;
			if (mMinimum.z > b2->mMaximum.z)
				return false;

			// otherwise, must be intersecting
			return true;

		}

		/// Calculate the area of intersection of this box and another
		inline AxisAlignedBox^ Intersection(AxisAlignedBox^ b2)
		{
            if (this->IsNull || b2->IsNull)
			{
				return gcnew AxisAlignedBox();
			}
			else if (this->IsInfinite)
			{
				return gcnew AxisAlignedBox(b2);
			}
			else if (b2->IsInfinite)
			{
				return gcnew AxisAlignedBox(this);
			}

			Vector3 intMin = mMinimum;
            Vector3 intMax = mMaximum;

            intMin.MakeCeil(b2->Minimum);
            intMax.MakeFloor(b2->Maximum);

            // Check intersection isn't null
            if (intMin.x < intMax.x &&
                intMin.y < intMax.y &&
                intMin.z < intMax.z)
            {
                return gcnew AxisAlignedBox(intMin, intMax);
            }

            return gcnew AxisAlignedBox();
		}

		/// Calculate the volume of this box
		Real Volume()
		{
			switch (mExtent)
			{
			case Extent::EXTENT_NULL:
				return 0.0f;

			case Extent::EXTENT_FINITE:
				{
					Vector3 diff = mMaximum - mMinimum;
					return diff.x * diff.y * diff.z;
				}

			case Extent::EXTENT_INFINITE:
				return Math::POS_INFINITY;

			default: // shut up compiler
				throw gcnew Exception("Should never reach here" );
			}
		}

		/** Scales the AABB by the vector given. */
		inline void Scale(Vector3 s)
		{
			// Do nothing if current null or infinite
			if (mExtent != Extent::EXTENT_FINITE)
				return;

			// NB assumes centered on origin
			Vector3 min = mMinimum * s;
			Vector3 max = mMaximum * s;
			SetExtents(min, max);
		}

		/** Tests whether this box intersects a sphere. */
		bool Intersects(Sphere s)
		{
			return Math::Intersects(s, this); 
		}
		/** Tests whether this box intersects a plane. */
		bool Intersects(Plane p)
		{
			return Math::Intersects(p, this);
		}
		/** Tests whether the vector point is within this box. */
		bool Intersects(Vector3 v)
		{
			switch (mExtent)
			{
			case Extent::EXTENT_NULL:
				return false;

			case Extent::EXTENT_FINITE:
				return(v.x >= mMinimum.x  &&  v.x <= mMaximum.x  && 
					v.y >= mMinimum.y  &&  v.y <= mMaximum.y  && 
					v.z >= mMinimum.z  &&  v.z <= mMaximum.z);

			case Extent::EXTENT_INFINITE:
				return true;

			default: // shut up compiler
				throw gcnew Exception("Should never reach here" );
			}
		}
		/// Gets the centre of the box
		property Vector3 Center
		{
			Vector3 get()
			{
				if (mExtent != Extent::EXTENT_FINITE)
					throw gcnew Exception("Can't get center of a null or infinite AAB");

				return Vector3(
					(mMaximum.x + mMinimum.x) * 0.5,
					(mMaximum.y + mMinimum.y) * 0.5,
					(mMaximum.z + mMinimum.z) * 0.5);
			}
		}
		/// Gets the size of the box
		property Vector3 Size
		{
			Vector3 get()
			{
			switch (mExtent)
				{
				case Extent::EXTENT_NULL:
					return Vector3::ZERO;

				case Extent::EXTENT_FINITE:
					return mMaximum - mMinimum;

				case Extent::EXTENT_INFINITE:
					return Vector3(
						Math::POS_INFINITY,
						Math::POS_INFINITY,
						Math::POS_INFINITY);

				default: // shut up compiler
					throw gcnew Exception("Should never reach here" );
				}
			}
		}
		/// Gets the half-size of the box
		property Vector3 HalfSize
		{
			Vector3 get()
			{
				switch (mExtent)
				{
				case Extent::EXTENT_NULL:
					return Vector3::ZERO;

				case Extent::EXTENT_FINITE:
					return (mMaximum - mMinimum) * 0.5;

				case Extent::EXTENT_INFINITE:
					return Vector3(
						Math::POS_INFINITY,
						Math::POS_INFINITY,
						Math::POS_INFINITY);

				default: // shut up compiler
					throw gcnew Exception("Should never reach here" );
				}
			}
		}

        /** Tests whether the given point contained by this box.
        */
        bool Contains(Vector3 v)
        {
            if (IsNull)
                return false;
            if (IsInfinite)
                return true;

            return mMinimum.x <= v.x && v.x <= mMaximum.x &&
                   mMinimum.y <= v.y && v.y <= mMaximum.y &&
                   mMinimum.z <= v.z && v.z <= mMaximum.z;
        }

        /** Tests whether another box contained by this box.
        */
        bool Contains(AxisAlignedBox^ other)
        {
            if (other->IsNull || this->IsInfinite)
                return true;

            if (this->IsNull || other->IsInfinite)
                return false;

            return this->mMinimum.x <= other->mMinimum.x &&
                   this->mMinimum.y <= other->mMinimum.y &&
                   this->mMinimum.z <= other->mMinimum.z &&
                   other->mMaximum.x <= this->mMaximum.x &&
                   other->mMaximum.y <= this->mMaximum.y &&
                   other->mMaximum.z <= this->mMaximum.z;
        }

        /** Tests 2 boxes for equality.
        */
		virtual bool Equals(Object^ obj) override
		{
			AxisAlignedBox^ box = dynamic_cast<AxisAlignedBox^>(obj);
			if (box)
				return this->Equals(box);
			else
				return false;
		}

		bool Equals(AxisAlignedBox^ rhs)
        {
            if (this->mExtent != rhs->mExtent)
                return false;

            if (!this->IsFinite)
                return true;

            return this->mMinimum == rhs->mMinimum &&
                   this->mMaximum == rhs->mMaximum;
        }
	};

} // namespace Mogre
