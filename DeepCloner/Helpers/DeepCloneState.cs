using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Force.DeepCloner.Helpers
{
	internal class DeepCloneState
	{
		private class CustomEqualityComparer : IEqualityComparer<object>, IEqualityComparer
		{
			bool IEqualityComparer<object>.Equals(object x, object y)
			{
				return ReferenceEquals(x, y);
			}

			bool IEqualityComparer.Equals(object x, object y)
			{
				return ReferenceEquals(x, y);
			}

			public int GetHashCode(object obj)
			{
				return RuntimeHelpers.GetHashCode(obj);
			}
		}

		private static readonly CustomEqualityComparer Instance = new CustomEqualityComparer();

		private Dictionary<object, object> _loops;

		private readonly object[] _baseFrom = new object[3];
		private readonly object[] _baseTo = new object[3];

		private int _idx;

		public object GetKnownRef(object from)
		{
			// this is faster than call Diectionary from begin
			// also, small poco objects does not have a lot of references
			if (ReferenceEquals(from, _baseFrom[0])) return _baseTo[0];
			if (ReferenceEquals(from, _baseFrom[1])) return _baseTo[1];
			if (ReferenceEquals(from, _baseFrom[2])) return _baseTo[2];
			if (_loops == null) return null;
			object value;
			if (_loops.TryGetValue(from, out value)) return value;
			// null cannot bee a loop
			return null;
		}

		public void AddKnownRef(object from, object to)
		{
			if (_idx < 3)
			{
				_baseFrom[_idx] = from;
				_baseTo[_idx] = to;
				_idx++;
				return;
			}
			if (_loops == null) _loops = new Dictionary<object, object>(Instance);
			_loops[from] = to;
		}
	}
}
