using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Force.DeepCloner.Helpers
{
	/// <summary>
	/// Internal class but due implementation restriction should be public
	/// </summary>
	public abstract class ShallowSafeObjectCloner
	{
		public object DoClone()
		{
			// yes, this is correct
			if (this == null) return null;
			return MemberwiseClone();
		}

		public abstract object DoCloneObject(object obj);

		private static readonly ShallowSafeObjectCloner _instance;

		public static ShallowSafeObjectCloner GetInstance()
		{
			return _instance;
		}

		static ShallowSafeObjectCloner()
		{
			var mb = TypeCreationHelper.GetModuleBuilder();
			var builder = mb.DefineType("ShallowSafeObjectClonerImpl", TypeAttributes.Public, typeof(ShallowSafeObjectCloner));
			var ctorBuilder = builder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { });
			ctorBuilder.GetILGenerator().Emit(OpCodes.Ret);

			var methodBuilder = builder.DefineMethod(
				"DoCloneObject",
				MethodAttributes.Public | MethodAttributes.Virtual,
				CallingConventions.HasThis,
				typeof(object),
				new[] { typeof(object) });
			var il = methodBuilder.GetILGenerator();
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Call, typeof(ShallowSafeObjectCloner).GetMethod("DoClone"));
			il.Emit(OpCodes.Ret);
			var type = builder.CreateType();
			_instance = (ShallowSafeObjectCloner)Activator.CreateInstance(type);
		}
	}
}
