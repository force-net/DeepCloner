using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Force.DeepCloner.Helpers
{
	/// <summary>
	/// Safe types are types, which can be copied without real cloning. e.g. simple structs or strings (it is immutable)
	/// </summary>
	internal static class DeepClonerSafeTypes
	{
		internal static readonly ConcurrentDictionary<Type, bool> KnownTypes = new ConcurrentDictionary<Type, bool>();

		internal static readonly ConcurrentDictionary<Type, bool> KnownClasses = new ConcurrentDictionary<Type, bool>();

		static DeepClonerSafeTypes()
		{
			foreach (
				var x in
					new[]
						{
							typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong),
							typeof(float), typeof(double), typeof(decimal), typeof(char), typeof(string), typeof(bool), typeof(DateTime),
							typeof(IntPtr), typeof(UIntPtr)
						}) KnownTypes.TryAdd(x, true);
		}

		internal static bool IsTypeSafe(Type type, HashSet<Type> processingTypes)
		{
			bool isSafe;
			if (KnownTypes.TryGetValue(type, out isSafe)) return isSafe;

			// enums are safe
			// pointers (e.g. int*) are unsafe, but we cannot do anything with it except blind copy
			if (type.IsEnum || type.IsPointer)
			{
				KnownTypes.TryAdd(type, true);
				return true;
			}

			// classes are always unsafe (we should copy it fully to count references)
			if (!type.IsValueType)
			{
				KnownTypes.TryAdd(type, false);
				return false;
			}

			if (processingTypes == null)
				processingTypes = new HashSet<Type>();

			// structs cannot have a loops, but check it anyway
			processingTypes.Add(type);

			List<FieldInfo> fi = new List<FieldInfo>();
			var tp = type;
			do
			{
				fi.AddRange(tp.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
				tp = tp.BaseType;
			}
			while (tp != null);

			foreach (var fieldInfo in fi)
			{
				// type loop
				var fieldType = fieldInfo.FieldType;
				if (processingTypes.Contains(fieldType))
					continue;

				// not safe and not not safe. we need to go deeper
				if (!IsTypeSafe(fieldType, processingTypes))
				{
					KnownTypes.TryAdd(type, false);
					return false;
				}
			}

			KnownTypes.TryAdd(type, true);
			return true;
		}

		/// <summary>
		/// Classes with only safe fields are safe for ShallowClone (if they root objects for copying)
		/// </summary>
		internal static bool IsClassSafe(Type type)
		{
			bool isSafe;
			if (KnownClasses.TryGetValue(type, out isSafe)) return isSafe;

			// enums are safe
			// pointers (e.g. int*) are unsafe, but we cannot do anything with it except blind copy
			if (!type.IsClass || type.IsArray)
			{
				KnownClasses.TryAdd(type, false);
				return false;
			}

			List<FieldInfo> fi = new List<FieldInfo>();
			var tp = type;
			do
			{
				fi.AddRange(tp.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
				tp = tp.BaseType;
			}
			while (tp != null);

			if (fi.Any(fieldInfo => !IsTypeSafe(fieldInfo.FieldType, null)))
			{
				KnownClasses.TryAdd(type, false);
				return false;
			}

			KnownClasses.TryAdd(type, true);
			return true;
		}
	}
}
