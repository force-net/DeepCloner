using System.Collections.Generic;

namespace Force.DeepCloner.Helpers
{
	internal class DeepCloneState
	{
		private readonly Dictionary<object, object> _loops = new Dictionary<object, object>();

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
