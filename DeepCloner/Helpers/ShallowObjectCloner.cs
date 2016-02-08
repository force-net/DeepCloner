using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Force.DeepCloner.Helpers
{
	/// <summary>
	/// Internal class but due implementation restriction should be public
	/// </summary>
	public abstract class ShallowObjectCloner
	{
		protected abstract object DoCloneObject(object obj);

		private static readonly ShallowObjectCloner _instance;

		/// <summary>
		/// Performs real shallow object clone
		/// </summary>
		public static object CloneObject(object obj)
		{
			if (obj == null) return null;
			if (obj is string) return obj;
			return _instance.DoCloneObject(obj);
		}

		private class ShallowSafeObjectCloner : ShallowObjectCloner
		{
			private readonly Func<object, object> _cloneFunc;

			public ShallowSafeObjectCloner()
			{
				var methodInfo = typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
				ParameterExpression parameterExpression1 = Expression.Parameter(typeof(object));
				var mce = Expression.Call(parameterExpression1, methodInfo);
				_cloneFunc = Expression.Lambda<Func<object, object>>(mce, parameterExpression1).Compile();
			}

			protected override object DoCloneObject(object obj)
			{
				return _cloneFunc(obj);
			}
		}

		static ShallowObjectCloner()
		{
			_instance = new ShallowSafeObjectCloner();
			return;

			var mb = TypeCreationHelper.GetModuleBuilder();

			var builder = mb.DefineType("ShallowSafeObjectClonerImpl", TypeAttributes.Public, typeof(ShallowObjectCloner));
			var ctorBuilder = builder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis | CallingConventions.HasThis, Type.EmptyTypes);

			var cil = ctorBuilder.GetILGenerator();
			cil.Emit(OpCodes.Ldarg_0);
// ReSharper disable AssignNullToNotNullAttribute
			cil.Emit(OpCodes.Call, typeof(ShallowObjectCloner).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0]);
// ReSharper restore AssignNullToNotNullAttribute
			cil.Emit(OpCodes.Ret);

			var methodBuilder = builder.DefineMethod(
				"DoCloneObject",
				MethodAttributes.Public | MethodAttributes.Virtual,
				CallingConventions.HasThis,
				typeof(object),
				new[] { typeof(object) });

			var il = methodBuilder.GetILGenerator();
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Call, typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic));
			il.Emit(OpCodes.Ret);
			var type = builder.CreateType();
			_instance = (ShallowObjectCloner)Activator.CreateInstance(type);
		}
	}
}
