using System;
using System.Collections.Concurrent;

namespace Force.DeepCloner.Helpers
{
	internal static class DeepClonerCache
	{
		private static readonly ConcurrentDictionary<Type, object> _typeCache = new ConcurrentDictionary<Type, object>();

		private static readonly ConcurrentDictionary<Tuple<Type, Type>, object> _typeConvertCache = new ConcurrentDictionary<Tuple<Type, Type>, object>();

		public static T GetOrAdd<T>(Type type, Func<Type, T> adder)
		{
			return (T)_typeCache.GetOrAdd(type, x => adder(x));
		}

		public static T GetOrAddConvertor<T>(Type from, Type to, Func<Type, Type, T> adder)
		{
			return (T)_typeConvertCache.GetOrAdd(new Tuple<Type, Type>(from, to), (tuple) => adder(tuple.Item1, tuple.Item2));
		}
	}
}
