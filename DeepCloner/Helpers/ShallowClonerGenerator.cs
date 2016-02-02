using System;

namespace Force.DeepCloner.Helpers
{
	internal static class ShallowClonerGenerator
	{
		public static T CloneObject<T>(T obj)
		{
			return typeof(T).IsValueType
						? CloneStructInternal(obj)
						: CloneClassInternal(obj);
		}

		private static T CloneStructInternal<T>(T obj) // where T : struct
		{
			return obj; // will be cloned by struct behaviour
		}

		private static Func<object, object> _cloner = DeepClonerMsilGenerator.GenerateMemberwiseCloner();

		private static T CloneClassInternal<T>(T obj) // where T : class
		{
// ReSharper disable CompareNonConstrainedGenericWithNull
			if (obj == null) return default(T);
// ReSharper restore CompareNonConstrainedGenericWithNull

			return (T)_cloner(obj);
		}
	}
}
