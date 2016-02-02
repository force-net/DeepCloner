using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Force.DeepCloner.Helpers
{
	public static class DeepClonerGenerator
	{
		// TODO: do something with unsafe fields
		internal static readonly HashSet<Type> SafeTypes = new HashSet<Type>(new[] { typeof(byte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(char), typeof(string), typeof(bool), typeof(DateTime), typeof(IntPtr), typeof(UIntPtr) });

		internal static readonly HashSet<Type> UnSafeTypes = new HashSet<Type>();

		private static int _counter;

		private static object GenerateClonerInternal(Type realType)
		{
			var type = realType;

			if (CheckIsTypeSafe(type, null)) return null;

			var mb = TypeCreationHelper.GetModuleBuilder();
			// var dt = mb.DefineType("DeepObjectCloner_" + type.Name + Interlocked.Increment(ref _counter));
			var dt = new DynamicMethod(
				"DeepObjectCloner_" + type.Name + "_" + Interlocked.Increment(ref _counter), type, new[] { realType, typeof(DeepCloneState) }, mb, true);

			var il = dt.GetILGenerator();

			GenerateProcessMethod(il, type);

			var funcType = typeof(Func<,,>).MakeGenericType(realType, typeof(DeepCloneState), realType);

			return dt.CreateDelegate(funcType);
		}

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
				return ((Func<T, DeepCloneState, T>)DeepClonerCache.GetOrAddConvertor(from, to, GenerateConvertor))(obj, state);
				/*return (T)typeof(DeepClonerGenerator).GetMethod("CloneObjectInternal", BindingFlags.Static | BindingFlags.NonPublic)
											.MakeGenericMethod(from)
											.Invoke(null, new object[] { obj, state });*/
			}

			var cloner = (Func<T, DeepCloneState, T>)DeepClonerCache.GetOrAdd(from, GenerateClonerInternal);

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
			var from = obj.GetType();

			var cloner = (Func<T, DeepCloneState, T>)DeepClonerCache.GetOrAdd(from, GenerateClonerInternal);

			// safe ojbect
			if (cloner == null) return obj;

			return cloner(obj, state);
		}

		private static bool CheckIsTypeSafe(Type type, HashSet<Type> processingTypes)
		{
			if (SafeTypes.Contains(type)) return true;

			if (type.IsEnum)
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
				if (!CheckIsTypeSafe(fieldType, processingTypes))
				{
					UnSafeTypes.Add(type);
					return false;
				}
			}

			SafeTypes.Add(type);
			return true;
		}

		private static void GenerateProcessMethod(ILGenerator il, Type type)
		{
			if (type.IsArray)
			{
				GenerateProcessArrayMethod(il, type);
				return;
			}
			
			var typeLocal = il.DeclareLocal(type);
			if (!type.IsValueType)
			{
				// Formatter services is slightly faster variant, but cannot create ContextBoundObject realizations
				if (typeof(ContextBoundObject).IsAssignableFrom(type))
				{
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Call, typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic));
					il.Emit(OpCodes.Stloc, typeLocal);
				}
				else
				{
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Call, typeof(object).GetMethod("GetType"));
					il.Emit(OpCodes.Call, typeof(System.Runtime.Serialization.FormatterServices).GetMethod("GetUninitializedObject"));
					il.Emit(OpCodes.Stloc, typeLocal);
				}
			}
			else
			{
				il.Emit(OpCodes.Ldloca_S, typeLocal);
				il.Emit(OpCodes.Initobj, type);
			}

			// added from -> to binding to ensure reference loop handling
			// structs cannot loop here
			if (type.IsClass)
			{
				il.Emit(OpCodes.Ldarg_1);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldloc, typeLocal);
				il.Emit(OpCodes.Call, typeof(DeepCloneState).GetMethod("AddKnownRef"));
			}

			foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
			{
				if (CheckIsTypeSafe(fieldInfo.FieldType, null))
				{
					il.Emit(type.IsClass ? OpCodes.Ldloc : OpCodes.Ldloca_S, typeLocal);
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldfld, fieldInfo);
					il.Emit(OpCodes.Stfld, fieldInfo);
				}
				else
				{
					il.Emit(type.IsClass ? OpCodes.Ldloc : OpCodes.Ldloca_S, typeLocal);
					il.Emit(OpCodes.Ldarg_0);
					il.Emit(OpCodes.Ldfld, fieldInfo);
					il.Emit(OpCodes.Ldarg_1);

					var methodInfo = typeof(DeepClonerGenerator).GetMethod(fieldInfo.FieldType.IsValueType ? "CloneStructInternal" : "CloneClassInternal", BindingFlags.NonPublic | BindingFlags.Static);
					il.Emit(OpCodes.Call, methodInfo.MakeGenericMethod(fieldInfo.FieldType));
					il.Emit(OpCodes.Stfld, fieldInfo);
				}
			}

			il.Emit(OpCodes.Ldloc, typeLocal);
			il.Emit(OpCodes.Ret);
		}

		//TODO: optimizie + check array as struct as interfce
		private static void GenerateProcessArrayMethod(ILGenerator il, Type type)
		{
			// TODO: processing array of structs can be simplified
			var typeLocal = il.DeclareLocal(type);
			var lenLocal = il.DeclareLocal(typeof(int));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, type.GetProperty("Length").GetGetMethod());
			il.Emit(OpCodes.Dup);
			il.Emit(OpCodes.Stloc, lenLocal);
			il.Emit(OpCodes.Newarr, type.GetElementType());
			il.Emit(OpCodes.Stloc, typeLocal);

			if (CheckIsTypeSafe(type.GetElementType(), null))
			{
				// Array.Copy(from, to, from.Length);
				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldloc, typeLocal);
				il.Emit(OpCodes.Ldloc, lenLocal);
				il.Emit(
					OpCodes.Call,
					typeof(Array).GetMethod("Copy", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Array), typeof(Array), typeof(int) }, null));
			}
			else
			{
				var endLoopLabel = il.DefineLabel();
				var startLoopLabel = il.DefineLabel();
				// using for-loop
				var iLocal = il.DeclareLocal(typeof(int));
				il.Emit(OpCodes.Ldc_I4_0);
				il.Emit(OpCodes.Stloc, iLocal);

				il.MarkLabel(startLoopLabel);

				il.Emit(OpCodes.Ldloc, iLocal);
				il.Emit(OpCodes.Ldloc, lenLocal);
				il.Emit(OpCodes.Bge_S, endLoopLabel);

				// to[i] = Clone(from[i])
				il.Emit(OpCodes.Ldloc, typeLocal); // for save
				il.Emit(OpCodes.Ldloc, iLocal);

				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldloc, iLocal);
				il.Emit(OpCodes.Ldelem, type.GetElementType()); // get elem

				il.Emit(OpCodes.Ldarg_1);

				var methodInfo = typeof(DeepClonerGenerator).GetMethod(
					type.IsValueType ? "CloneStructInternal" : "CloneClassInternal", BindingFlags.NonPublic | BindingFlags.Static);
				il.Emit(OpCodes.Call, methodInfo.MakeGenericMethod(type.GetElementType()));
				il.Emit(OpCodes.Stelem, type.GetElementType());

				il.Emit(OpCodes.Ldloc, iLocal);
				il.Emit(OpCodes.Ldc_I4_1);
				il.Emit(OpCodes.Add);
				il.Emit(OpCodes.Stloc, iLocal);
				il.Emit(OpCodes.Br_S, startLoopLabel);

				il.MarkLabel(endLoopLabel);
			}

			il.Emit(OpCodes.Ldloc, typeLocal);
			il.Emit(OpCodes.Ret);
		}

		private static object GenerateConvertor(Type from, Type to)
		{
			var mb = TypeCreationHelper.GetModuleBuilder();

			var dt = new DynamicMethod(
				"DeepObjectConvertor_" + from.Name + "_" + to.Name + "_" + Interlocked.Increment(ref _counter), to, new[] { to, typeof(DeepCloneState) }, mb, true);
			var il = dt.GetILGenerator();
			il.Emit(OpCodes.Ldarg_0); // to
			var isStruct = from.IsValueType;
			if (isStruct)
				il.Emit(OpCodes.Unbox_Any, from);
			il.Emit(OpCodes.Ldarg_1); // state
			var realMethod =
				typeof(DeepClonerGenerator).GetMethod(isStruct ? "CloneStructInternal" : "CloneClassInternal", BindingFlags.NonPublic | BindingFlags.Static)
											.MakeGenericMethod(from);

			il.Emit(OpCodes.Call, realMethod);
			if (isStruct)
				il.Emit(OpCodes.Box, from);
			il.Emit(OpCodes.Ret);
			var funcType = typeof(Func<,,>).MakeGenericType(to, typeof(DeepCloneState), to);

			return dt.CreateDelegate(funcType);
		}
	}
}
