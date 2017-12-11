#if !NETCORE
using System;
using System.Reflection;

namespace Force.DeepCloner.Helpers
{
	internal static class DeepClonerMsilHelper
	{
		public static bool IsConstructorDoNothing(Type type, ConstructorInfo constructor)
		{
			if (constructor == null) return false;
			try
			{
				// will not try to determine body for this types
				if (type.IsGenericType || type.IsContextful || type.IsCOMObject || type.Assembly.IsDynamic) return false;

				var methodBody = constructor.GetMethodBody();

				// this situation can be for com
				if (methodBody == null) return false;

				var ilAsByteArray = methodBody.GetILAsByteArray();
				if (ilAsByteArray.Length == 7
					&& ilAsByteArray[0] == 0x02 // Ldarg_0
					&& ilAsByteArray[1] == 0x28 // newobj
					&& ilAsByteArray[6] == 0x2a // ret
					&& type.Module.ResolveMethod(BitConverter.ToInt32(ilAsByteArray, 2)) == typeof(object).GetConstructor(Type.EmptyTypes)) // call object
				{
					return true;
				}
				else if (ilAsByteArray.Length == 1 && ilAsByteArray[0] == 0x2a) // ret
				{
					return true;
				}

				return false;
			}
			catch (Exception)
			{
				// no permissions or something similar
				return false;
			}
		}
	}
}
#endif