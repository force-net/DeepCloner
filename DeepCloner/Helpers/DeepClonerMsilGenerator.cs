using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace Force.DeepCloner.Helpers
{
	internal static class DeepClonerMsilGenerator
	{
		private static int _methodCounter;

		internal static object GenerateClonerInternal(Type realType)
		{
			var type = realType;

			if (DeepClonerSafeTypes.IsTypeSafe(type, null)) return null;

			var mb = TypeCreationHelper.GetModuleBuilder();
			var dt = new DynamicMethod(
				"DeepObjectCloner_" + type.Name + "_" + Interlocked.Increment(ref _methodCounter), type, new[] { realType, typeof(DeepCloneState) }, mb, true);

			var il = dt.GetILGenerator();

			GenerateProcessMethod(il, type);

			var funcType = typeof(Func<,,>).MakeGenericType(realType, typeof(DeepCloneState), realType);

			return dt.CreateDelegate(funcType);
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
				if (DeepClonerSafeTypes.IsTypeSafe(fieldInfo.FieldType, null))
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

		private static void GenerateProcessArrayMethod(ILGenerator il, Type type)
		{
			// TODO: processing array of structs can be simplified
			var typeLocal = il.DeclareLocal(type);
			var lenLocal = il.DeclareLocal(typeof(int));
			il.Emit(OpCodes.Ldarg_0);
			il.Emit(OpCodes.Call, type.GetProperty("Length").GetGetMethod());
			il.Emit(OpCodes.Dup);
			il.Emit(OpCodes.Stloc, lenLocal);
			var elementType = type.GetElementType();
			il.Emit(OpCodes.Newarr, elementType);
			il.Emit(OpCodes.Stloc, typeLocal);

			if (DeepClonerSafeTypes.IsTypeSafe(elementType, null))
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
				var methodInfo = typeof(DeepClonerGenerator).GetMethod(elementType.IsValueType ? "CloneStructInternal" : "CloneClassInternal", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(elementType);
				LocalBuilder clonerLocal = null;

				if (type.IsValueType)
				{
					// unsafe struct, no inheritance, so, we can use fixed cloner
					var funcType = typeof(Func<,,>).MakeGenericType(elementType, typeof(DeepCloneState), elementType);
					methodInfo = funcType.GetMethod("Invoke");
					clonerLocal = il.DeclareLocal(funcType);
					il.Emit(OpCodes.Call, typeof(DeepClonerGenerator).GetMethod("GetCloner", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(elementType));
					il.Emit(OpCodes.Stloc, clonerLocal);
				}

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

				if (clonerLocal != null)
					il.Emit(OpCodes.Ldloc, clonerLocal);

				il.Emit(OpCodes.Ldarg_0);
				il.Emit(OpCodes.Ldloc, iLocal);
				il.Emit(OpCodes.Ldelem, elementType); // get elem

				il.Emit(OpCodes.Ldarg_1);

				il.Emit(OpCodes.Call, methodInfo);
				il.Emit(OpCodes.Stelem, elementType);

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

		internal static object GenerateConvertor(Type from, Type to)
		{
			var mb = TypeCreationHelper.GetModuleBuilder();

			var dt = new DynamicMethod(
				"DeepObjectConvertor_" + from.Name + "_" + to.Name + "_" + Interlocked.Increment(ref _methodCounter), to, new[] { to, typeof(DeepCloneState) }, mb, true);
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
