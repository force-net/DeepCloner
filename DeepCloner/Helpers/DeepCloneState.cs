using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Force.DeepCloner.Helpers
{
	internal class DeepCloneState
	{
		private class CustomEqualityComparer : IEqualityComparer<object>
		{
			public bool Equals(object x, object y)
			{
				return ReferenceEquals(x, y);
			}

			public int GetHashCode(object obj)
			{
				return RuntimeHelpers.GetHashCode(obj);
			}
		}

		private static readonly CustomEqualityComparer Instance = new CustomEqualityComparer();

		private readonly Dictionary<object, object> _loops = new Dictionary<object, object>(Instance);

		public object GetKnownRef(object from)
		{
			object value;
			if (_loops.TryGetValue(from, out value)) return value;
			// null cannot bee a loop
			return null;
		}

		public void AddKnownRef(object from, object to)
		{
			_loops[from] = to;
		}
	}
}
