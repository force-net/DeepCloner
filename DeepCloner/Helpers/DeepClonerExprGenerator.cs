using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Force.DeepCloner.Helpers
{
	internal static class DeepClonerExprGenerator
	{
		internal static object GenerateClonerInternal(Type realType, bool asObject)
		{
			return GenerateProcessMethod(realType, asObject && realType.IsValueType);
		}

		private static object GenerateProcessMethod(Type type, bool unboxStruct)
		{
			if (type.IsArray)
			{
				return	GenerateProcessArrayMethod(type);
			}

			var methodType = unboxStruct || type.IsClass ? typeof(object) : type;

			var expressionList = new List<Expression>();

			ParameterExpression from = Expression.Parameter(methodType);
			var fromLocal = from;
			var toLocal = Expression.Variable(type);
			var state = Expression.Parameter(typeof(DeepCloneState));

			if (!type.IsValueType)
			{
				var methodInfo = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
				
				// to = (T)from.MemberwiseClone()
				expressionList.Add(Expression.Assign(toLocal, Expression.Convert(Expression.Call(from, methodInfo), type)));
				
				fromLocal = Expression.Variable(type);
				// fromLocal = (T)from
				expressionList.Add(Expression.Assign(fromLocal, Expression.Convert(from, type)));

				// added from -> to binding to ensure reference loop handling
				// structs cannot loop here
				// state.AddKnownRef(from, to)
				expressionList.Add(Expression.Call(state, typeof(DeepCloneState).GetMethod("AddKnownRef"), from, toLocal));
			}
			else
			{
				if (unboxStruct)
				{
					// toLocal = (T)from;
					expressionList.Add(Expression.Assign(toLocal, Expression.Unbox(from, type)));
					fromLocal = Expression.Variable(type);
					// fromLocal = toLocal; // structs, it is ok to copy
					expressionList.Add(Expression.Assign(fromLocal, toLocal));
				}
				else
				{
					// toLocal = from
					expressionList.Add(Expression.Assign(toLocal, from));
				}
			}

			List<FieldInfo> fi = new List<FieldInfo>();
			var tp = type;
			do
			{
				// don't do anything with this dark magic!
				if (tp == typeof(ContextBoundObject)) break;
				fi.AddRange(tp.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
				tp = tp.BaseType;
			}
			while (tp != null);

			foreach (var fieldInfo in fi)
			{
				if (!DeepClonerSafeTypes.IsTypeSafe(fieldInfo.FieldType, null))
				{
					var methodInfo = fieldInfo.FieldType.IsValueType
										? typeof(DeepClonerGenerator).GetMethod("CloneStructInternal", BindingFlags.NonPublic | BindingFlags.Static)
																	.MakeGenericMethod(fieldInfo.FieldType)
										: typeof(DeepClonerGenerator).GetMethod("CloneClassInternal", BindingFlags.NonPublic | BindingFlags.Static);

					var get = Expression.Field(fromLocal, fieldInfo);

					// toLocal.Field = Clone...Internal(fromLocal.Field)
					var call = (Expression)Expression.Call(methodInfo, get, state);
					if (!fieldInfo.FieldType.IsValueType)
						call = Expression.Convert(call, fieldInfo.FieldType);

					// should handle specially
					if (fieldInfo.IsInitOnly)
					{
						var setMethod = fieldInfo.GetType().GetMethod("SetValue", new[] { typeof(object), typeof(object) });
						expressionList.Add(Expression.Call(Expression.Constant(fieldInfo), setMethod, toLocal, call));
					}
					else
					{
						expressionList.Add(Expression.Assign(Expression.Field(toLocal, fieldInfo), call));
					}
				}
			}

			expressionList.Add(Expression.Convert(toLocal, methodType));

			var funcType = typeof(Func<,,>).MakeGenericType(methodType, typeof(DeepCloneState), methodType);

			var blockParams = new List<ParameterExpression>();
			if (from != fromLocal) blockParams.Add(fromLocal);
			blockParams.Add(toLocal);

			return Expression.Lambda(funcType, Expression.Block(blockParams, expressionList), from, state).Compile();
		}

		private static object GenerateProcessArrayMethod(Type type)
		{
			var elementType = type.GetElementType();
			var rank = type.GetArrayRank();

			MethodInfo methodInfo;

			// multidim or not zero-based arrays
			if (rank != 1 || type != elementType.MakeArrayType())
			{
				if (rank == 2)
				{
					// small optimization for 2 dim arrays
					methodInfo = typeof(DeepClonerGenerator).GetMethod("Clone2DimArrayInternal", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(elementType);
				}
				else
				{
					methodInfo = typeof(DeepClonerGenerator).GetMethod("CloneAbstractArrayInternal", BindingFlags.NonPublic | BindingFlags.Static);
				}
			}
			else
			{
				var methodName = "Clone1DimArrayClassInternal";
				if (DeepClonerSafeTypes.IsTypeSafe(elementType, null)) methodName = "Clone1DimArraySafeInternal";
				else if (elementType.IsValueType) methodName = "Clone1DimArrayStructInternal";
				methodInfo = typeof(DeepClonerGenerator).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(elementType);
			}

			ParameterExpression from = Expression.Parameter(typeof(object));
			var state = Expression.Parameter(typeof(DeepCloneState));
			var call = Expression.Call(methodInfo, Expression.Convert(from, type), state);

			var funcType = typeof(Func<,,>).MakeGenericType(typeof(object), typeof(DeepCloneState), typeof(object));

			return Expression.Lambda(funcType, call, from, state).Compile();
		}
	}
}
