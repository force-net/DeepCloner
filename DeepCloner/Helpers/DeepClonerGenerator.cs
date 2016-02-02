using System;

namespace Force.DeepCloner.Helpers
{
	public static class DeepClonerGenerator
	{
		// TODO: do something with unsafe fields

		public static T CloneObject<T>(T obj)
		{
			return typeof(T).IsValueType 
						? CloneStructInternal(obj, new DeepCloneState()) 
						: CloneClassInternal(obj, new DeepCloneState());
		}

		private static T CloneClassInternal<T>(T obj, DeepCloneState state) // where T : class
		{
			// null
			var to = typeof(T);
// ReSharper disable CompareNonConstrainedGenericWithNull
			if (obj == null) return default(T);
// ReSharper restore CompareNonConstrainedGenericWithNull

			// todo: think about optimization
			var from = obj.GetType();
			if (from != to)
			{
				return ((Func<T, DeepCloneState, T>)DeepClonerCache.GetOrAddConvertor(from, to, DeepClonerMsilGenerator.GenerateConvertor))(obj, state);
				/*return (T)typeof(DeepClonerGenerator).GetMethod("CloneObjectInternal", BindingFlags.Static | BindingFlags.NonPublic)
											.MakeGenericMethod(from)
											.Invoke(null, new object[] { obj, state });*/
			}

			var cloner = GetCloner<T>();

			// safe ojbect
			if (cloner == null) return obj;

			// loop
			var knownRef = state.GetKnownRef(obj);
			if (knownRef != null) return (T)knownRef;

			return cloner(obj, state);
		}

		// TODO: check IsClass/IsInterface usage
		private static T CloneStructInternal<T>(T obj, DeepCloneState state) // where T : struct
		{
			// no loops, no nulls, no inheritance
			var cloner = GetCloner<T>();

			// safe ojbect
			if (cloner == null) return obj;

			return cloner(obj, state);
		}

		private static Func<T, DeepCloneState, T> GetCloner<T>()
		{
			return (Func<T, DeepCloneState, T>)DeepClonerCache.GetOrAdd(typeof(T), DeepClonerMsilGenerator.GenerateClonerInternal);
		}
	}
}
