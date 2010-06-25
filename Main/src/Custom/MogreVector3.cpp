#include "MogreStableHeaders.h"

#include "Custom\MogreVector3.h"
#include "Custom\MogreQuaternion.h"

namespace Mogre
{
	Vector3 Vector3::RandomDeviant(
        Radian angle,
        Vector3 up )
    {
        Vector3 newUp;

        if (up == Vector3::ZERO)
        {
            // Generate an up vector
            newUp = this->Perpendicular;
        }
        else
        {
            newUp = up;
        }

        // Rotate up vector by random amount around this
        Quaternion q;
        q.FromAngleAxis( Radian(Math::UnitRandom() * Math::TWO_PI), *this );
        newUp = q * newUp;

        // Finally rotate this by given angle around randomised up
        q.FromAngleAxis( angle, newUp );
        return q * (*this);
    }

	Quaternion Vector3::GetRotationTo(Vector3 dest, Vector3 fallbackAxis)
    {
        // Based on Stan Melax's article in Game Programming Gems
        Quaternion q;
        // Copy, since cannot modify local
        Vector3 v0 = *this;
        Vector3 v1 = dest;
        v0.Normalise();
        v1.Normalise();

        Real d = v0.DotProduct(v1);
        // If dot == 1, vectors are the same
        if (d >= 1.0f)
        {
            return Quaternion::IDENTITY;
        }
		
		// sometimes the dot product yields -1.0000001
		// floating point math does that to you
		if (d < -1.0f)
			d = -1.0f;

        Real s = Math::Sqrt( (1+d)*2 );
		if (s < 1e-6f)
		{
			if (fallbackAxis != Vector3::ZERO)
			{
				// rotate 180 degrees about the fallback axis
				q.FromAngleAxis(Radian(Math::PI), fallbackAxis);
			}
			else
			{
				// Generate an axis
				Vector3 axis = Vector3::UNIT_X.CrossProduct(*this);
				if (axis.IsZeroLength) // pick another if colinear
					axis = Vector3::UNIT_Y.CrossProduct(*this);
				axis.Normalise();
				q.FromAngleAxis(Radian(Math::PI), axis);
			}
		}
		else
		{
            Real invs = 1 / s;

			Vector3 c = v0.CrossProduct(v1);

	        q.x = c.x * invs;
    	    q.y = c.y * invs;
        	q.z = c.z * invs;
        	q.w = s * 0.5;
			q.Normalise();
		}
        return q;
    }

	Quaternion Vector3::GetRotationTo(Vector3 dest)
	{
		return GetRotationTo(dest, Vector3::ZERO);
	}
}