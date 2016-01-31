using System;
using System.Collections.Concurrent;

namespace Force.DeepCloner.Helpers
{
	internal static class DeepClonerCache
	{
		private static readonly ConcurrentDictionary<Type, object> _typeCache = new ConcurrentDictionary<Type, object>();

		public static T GetOrAdd<T>(Type type, Func<Type, T> adder)
		{
			return (T)_typeCache.GetOrAdd(type, x => adder(x));
		}
	}
}
