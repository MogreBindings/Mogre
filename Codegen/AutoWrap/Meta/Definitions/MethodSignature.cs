namespace AutoWrap.Meta
{
    public class MethodSignature
    {
        private string _signature;

        public MethodSignature(MemberMethodDefinition methodDef)
        {
            _signature = methodDef.IsVirtual.ToString() + methodDef.ProtectionLevel + methodDef.Name;
            foreach (ParamDefinition param in methodDef.Parameters)
            {
                _signature += "|" + param.TypeName + "#" + param.PassedByType + "#" + param.Container + "#" + param.Array;
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MethodSignature);
        }

        public bool Equals(MethodSignature obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            if (obj == null)
                return false;

            return _signature == obj._signature;
        }

        public override int GetHashCode()
        {
            return _signature.GetHashCode();
        }

        public static bool operator ==(MethodSignature a, MethodSignature b)
        {
            // If both are null, or both are same instance, return true.
            if (ReferenceEquals(a, b))
                return true;

            // If one is null, but not both, return false.
            if (a == null || b == null)
                return false;

            // Return true if the fields match:
            return a._signature == b._signature;
        }

        public static bool operator !=(MethodSignature a, MethodSignature b)
        {
            return !(a == b);
        }
    }
}