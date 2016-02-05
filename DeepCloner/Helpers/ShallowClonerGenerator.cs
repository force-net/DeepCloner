using System;

namespace Force.DeepCloner.Helpers
{
	public static class ShallowClonerGenerator
	{
		internal static T CloneObject<T>(T obj)
		{
			// this is faster than typeof(T).IsValueType
			if (obj is ValueType)
				if (typeof(T) == obj.GetType()) return obj;

			return (T)ShallowSafeObjectCloner.CloneObject(obj);
		}

		/*private static T CloneStructInternal<T>(T obj) // where T : struct
		{
			return obj; // will be cloned by struct behaviour
		}*/

		/*private static Func<object, object> _cloner = DeepClonerMsilGenerator.GenerateMemberwiseCloner();

		private static T CloneClassInternal<T>(T obj) // where T : class
		{
// ReSharper disable CompareNonConstrainedGenericWithNull
			if (obj == null) return default(T);
// ReSharper restore CompareNonConstrainedGenericWithNull

			return (T)_cloner(obj);
		}*/
	}
}
