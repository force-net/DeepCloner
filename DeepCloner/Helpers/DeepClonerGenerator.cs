using System;
using System.Linq;

namespace Force.DeepCloner.Helpers
{
	internal static class DeepClonerGenerator
	{
		public static T CloneObject<T>(T obj)
		{
			return obj is ValueType && typeof(T) == obj.GetType()
						? CloneStructInternal(obj, new DeepCloneState()) 
						: (T)CloneClassRoot(obj);
		}

		private static object CloneClassRoot(object obj)
		{
			if (obj == null) return null;

			// we can receive an poco objects which is faster to copy in shallow way if possible
			var type = obj.GetType();
			// 200ms
			if (DeepClonerSafeTypes.IsClassSafe(type)) 
				return ShallowObjectCloner.CloneObject(obj);
			// 350ms
			var cloner = (Func<object, DeepCloneState, object>)DeepClonerCache.GetOrAddClass(type, t => GenerateCloner(t, true));

			if (cloner == null) return obj;

			// 200ms
			return cloner(obj, new DeepCloneState());
		}

		public static T CloneStruct<T>(T obj) where T : struct 
		{
			return CloneStructInternal(obj, new DeepCloneState());
		}

		private static object CloneClassInternal(object obj, DeepCloneState state)
		{
			if (obj == null) return null;

			var cloner = (Func<object, DeepCloneState, object>)DeepClonerCache.GetOrAddClass(obj.GetType(), t => GenerateCloner(t, true));

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
			var cloner = GetClonerForValueType<T>();

			// safe ojbect
			if (cloner == null) return obj;

			return cloner(obj, state);
		}

		// when we can't use code generation, we can use these methods
		internal static T[] Clone1DimArraySafeInternal<T>(T[] obj, DeepCloneState state)
		{
			var l = obj.Length;
			var outArray = new T[l];
			state.AddKnownRef(obj, outArray);
			Array.Copy(obj, outArray, obj.Length);
			return outArray;
		}

		internal static T[] Clone1DimArrayStructInternal<T>(T[] obj, DeepCloneState state)
		{
			// not null from called method, but will check it anyway
			if (obj == null) return null;
			var l = obj.Length;
			var outArray = new T[l];
			state.AddKnownRef(obj, outArray);
			var cloner = GetClonerForValueType<T>();
			for (var i = 0; i < l; i++)
				outArray[i] = cloner(obj[i], state);

			return outArray;
		}

		internal static T[] Clone1DimArrayClassInternal<T>(T[] obj, DeepCloneState state)
		{
			// not null from called method, but will check it anyway
			if (obj == null) return null;
			var l = obj.Length;
			var outArray = new T[l];
			state.AddKnownRef(obj, outArray);
			for (var i = 0; i < l; i++)
				outArray[i] = (T)CloneClassInternal(obj[i], state);

			return outArray;
		}

		// relatively frequent case. specially handled
		internal static T[,] Clone2DimArrayInternal<T>(T[,] obj, DeepCloneState state)
		{
			// not null from called method, but will check it anyway
			if (obj == null) return null;
			var l1 = obj.GetLength(0);
			var l2 = obj.GetLength(1);
			var outArray = new T[l1, l2];
			state.AddKnownRef(obj, outArray);
			if (DeepClonerSafeTypes.IsTypeSafe(typeof(T), null))
			{
				Array.Copy(obj, outArray, obj.Length);
				return obj;
			}

			if (typeof(T).IsValueType)
			{
				var cloner = GetClonerForValueType<T>();
				for (var i = 0; i < l1; i++)
					for (var k = 0; k < l2; k++)
						outArray[i, k] = cloner(obj[i, k], state);
			}
			else
			{
				for (var i = 0; i < l1; i++)
					for (var k = 0; k < l2; k++)
						outArray[i, k] = (T)CloneClassInternal(obj[i, k], state);
			}

			return outArray;
		}

		// rare cases, very slow cloning. currently it's ok
		internal static Array CloneAbstractArrayInternal(Array obj, DeepCloneState state)
		{
			// not null from called method, but will check it anyway
			if (obj == null) return null;
			var rank = obj.Rank;
			var lowerBounds = Enumerable.Range(0, rank).Select(obj.GetLowerBound).ToArray();
			var lengths = Enumerable.Range(0, rank).Select(obj.GetLength).ToArray();
			var idxes = Enumerable.Range(0, rank).Select(obj.GetLowerBound).ToArray();

			var outArray = Array.CreateInstance(obj.GetType().GetElementType(), lengths, lowerBounds);
			state.AddKnownRef(obj, outArray);
			while (true)
			{
				outArray.SetValue(CloneClassInternal(obj.GetValue(idxes), state), idxes);
				var ofs = rank - 1;
				while (true)
				{
					idxes[ofs]++;
					if (idxes[ofs] >= lowerBounds[ofs] + lengths[ofs])
					{
						idxes[ofs] = lowerBounds[ofs];
						ofs--;
						if (ofs < 0) return outArray;
					}
					else
						break;
				}
			}
		}

		private static Func<T, DeepCloneState, T> GetClonerForValueType<T>()
		{
			return (Func<T, DeepCloneState, T>)DeepClonerCache.GetOrAddStructAsObject(typeof(T), t => GenerateCloner(t, false));
		}

		private static object GenerateCloner(Type t, bool asObject)
		{
			if (ShallowObjectCloner.IsSafeVariant()) return DeepClonerExprGenerator.GenerateClonerInternal(t, asObject);
			else return DeepClonerMsilGenerator.GenerateClonerInternal(t, asObject);
		}
	}
}
