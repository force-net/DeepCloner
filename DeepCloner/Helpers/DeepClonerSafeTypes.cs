using System;
using System.Collections.Generic;
using System.Reflection;

namespace Force.DeepCloner.Helpers
{
	/// <summary>
	/// Safe types are types, which can be copied without real cloning. e.g. simple structs or strings (it is immutable)
	/// </summary>
	internal static class DeepClonerSafeTypes
	{
		internal static readonly HashSet<Type> SafeTypes = new HashSet<Type>(new[] { typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(char), typeof(string), typeof(bool), typeof(DateTime), typeof(IntPtr), typeof(UIntPtr) });

		internal static readonly HashSet<Type> UnSafeTypes = new HashSet<Type>();

		internal static bool IsTypeSafe(Type type, HashSet<Type> processingTypes)
		{
			if (SafeTypes.Contains(type)) return true;

			// enums are safe
			// pointers (e.g. int*) are unsafe, but we cannot do anything with it except blind copy
			if (type.IsEnum || type.IsPointer)
			{
				SafeTypes.Add(type);
				return true;
			}

			// non-value types should be copied always
			if (!type.IsValueType || UnSafeTypes.Contains(type)) return false;

			if (processingTypes == null)
				processingTypes = new HashSet<Type>();

			// structs cannot have a loops, but check it anyway
			processingTypes.Add(type);

			foreach (var fieldInfo in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
			{
				// type loop
				var fieldType = fieldInfo.FieldType;
				if (processingTypes.Contains(fieldType))
					continue;

				// not safe and not not safe. we need to go deeper
				if (!IsTypeSafe(fieldType, processingTypes))
				{
					UnSafeTypes.Add(type);
					return false;
				}
			}

			SafeTypes.Add(type);
			return true;
		}
	}
}
