using System;

namespace Force.DeepCloner.Helpers
{
	public static class DeepClonerGenerator
	{
		public static T CloneObject<T>(T obj)
		{
			return obj is ValueType && typeof(T).IsValueType 
						? CloneStructInternal(obj, new DeepCloneState()) 
						: (T)CloneClassInternal(obj, new DeepCloneState());
		}

		private static object CloneClassInternal(object obj, DeepCloneState state)
		{
			if (obj == null) return null;

			var cloner = (Func<object, DeepCloneState, object>)DeepClonerCache.GetOrAdd(obj.GetType(), t => DeepClonerMsilGenerator.GenerateClonerInternal(t, true));

			// safe ojbect
			if (cloner == null) return obj;

			// loop
			var knownRef = state.GetKnownRef(obj);
			if (knownRef != null) return knownRef;

			return cloner(obj, state);
		}

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
			return (Func<T, DeepCloneState, T>)DeepClonerCache.GetOrAdd(typeof(T), t => DeepClonerMsilGenerator.GenerateClonerInternal(t, false));
		}
	}
}
