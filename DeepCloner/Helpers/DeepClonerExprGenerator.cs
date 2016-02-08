using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

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
			var toLocal = Expression.Variable(type, "toLocal");
			var to = toLocal;
			var state = Expression.Parameter(typeof(DeepCloneState));

			if (!type.IsValueType)
			{
				var methodInfo = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
				
				var mce = Expression.Call(from, methodInfo);
				var ass = Expression.Assign(toLocal, Expression.Convert(mce, type));
				expressionList.Add(ass);
				
				fromLocal = Expression.Variable(type);
				expressionList.Add(Expression.Assign(fromLocal, Expression.Convert(from, type)));

				// added from -> to binding to ensure reference loop handling
				// structs cannot loop here

				expressionList.Add(Expression.Call(state, typeof(DeepCloneState).GetMethod("AddKnownRef"), from, to));
			}
			else
			{
				if (unboxStruct)
				{
					to = Expression.Variable(methodType, "to");
					expressionList.Add(Expression.Assign(toLocal, Expression.Unbox(from, type)));
					fromLocal = Expression.Variable(type);
					expressionList.Add(Expression.Assign(fromLocal, toLocal));
				}
				else
				{
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

			if (unboxStruct)
			{
				expressionList.Add(Expression.Assign(to, Expression.Convert(toLocal, typeof(object))));
			}
			else
			{
				 expressionList.Add(Expression.Assign(to, to));
			}

			var funcType = typeof(Func<,,>).MakeGenericType(methodType, typeof(DeepCloneState), methodType);

			var blockParams = new List<ParameterExpression>();
			if (from != fromLocal) blockParams.Add(fromLocal);
			blockParams.Add(to);
			if (to != toLocal) blockParams.Add(toLocal);

			return Expression.Lambda(funcType, Expression.Block(blockParams, expressionList), from, state).Compile();
		}

		private static object GenerateProcessArrayMethod(Type type)
		{
			var elementType = type.GetElementType();
			ParameterExpression from = Expression.Parameter(typeof(object));
			var fromLocal = Expression.Variable(type, "fromLocal");
			var toLocal = Expression.Variable(type, "toLocal");
			var state = Expression.Parameter(typeof(DeepCloneState));

			var expressionList = new List<Expression>();

			var arr = new int[333];
			var obj = arr as object;
			var arr2 = (int[])obj;
			Console.WriteLine(arr2);

			expressionList.Add(Expression.Assign(fromLocal, Expression.TypeAs(from, type)));

			// expressionList.Add(Expression.Assign(toLocal, Expression.NewArrayBounds(elementType, Expression.ArrayLength(fromLocal))));
			expressionList.Add(Expression.Assign(toLocal, Expression.NewArrayBounds(elementType, Expression.Constant(3))));
			expressionList.Add(Expression.Assign(fromLocal, Expression.TypeAs(from, type)));
			if (DeepClonerSafeTypes.IsTypeSafe(elementType, null))
			{
				var copyMethod = typeof(Array).GetMethod("Copy", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(Array), typeof(Array), typeof(int) }, null);
				expressionList.Add(Expression.Call(copyMethod, Expression.TypeAs(from, typeof(int[])), toLocal, Expression.Constant(3)));
			}
			else
			{
				var methodInfo = elementType.IsValueType
						? typeof(DeepClonerGenerator).GetMethod("CloneStructInternal", BindingFlags.NonPublic | BindingFlags.Static).MakeGenericMethod(elementType)
						: typeof(DeepClonerGenerator).GetMethod("CloneClassInternal", BindingFlags.NonPublic | BindingFlags.Static);
			}

			expressionList.Add(Expression.Assign(toLocal, toLocal));

			return Expression.Lambda(typeof(Func<object, DeepCloneState, object>), Expression.Block(new[] { from, fromLocal, toLocal }, expressionList), from, state).Compile();
		}
	}
}
