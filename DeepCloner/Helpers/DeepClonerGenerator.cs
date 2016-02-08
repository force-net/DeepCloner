using System;

namespace Force.DeepCloner.Helpers
{
	internal static class DeepClonerGenerator
	{
		public static T CloneObject<T>(T obj)
		{
			return obj is ValueType && typeof(T) == obj.GetType()
						? CloneStructInternal(obj, new DeepCloneState()) 
						: CloneClassRoot<T>(obj);
		}

		private static T CloneClassRoot<T>(object obj)
		{
			if (obj == null) return default(T);

			// we can receive an poco objects which is faster to copy in shallow way if possible
			if (DeepClonerSafeTypes.IsClassSafe(obj.GetType())) 
				return (T)ShallowObjectCloner.CloneObject(obj);

			return (T)CloneClassInternal(obj, new DeepCloneState());
		}

		public static T CloneStruct<T>(T obj) where T : struct 
		{
			return CloneStructInternal(obj, new DeepCloneState());
		}

		private static object CloneClassInternal(object obj, DeepCloneState state)
		{
			if (obj == null) return null;

			// var cloner = (Func<object, DeepCloneState, object>)DeepClonerCache.GetOrAddClass(obj.GetType(), t => DeepClonerMsilGenerator.GenerateClonerInternal(t, true));

			var cloner = (Func<object, DeepCloneState, object>)DeepClonerCache.GetOrAddClass(obj.GetType(), t => DeepClonerExprGenerator.GenerateClonerInternal(t, true));

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
			return (Func<T, DeepCloneState, T>)DeepClonerCache.GetOrAddStructAsObject(typeof(T), t => DeepClonerExprGenerator.GenerateClonerInternal(t, false));
		}
	}
}
