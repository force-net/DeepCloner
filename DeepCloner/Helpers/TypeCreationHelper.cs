#if !NETCORE
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Force.DeepCloner.Helpers
{
	internal static class TypeCreationHelper
	{
		private static ModuleBuilder _moduleBuilder;

		internal static ModuleBuilder GetModuleBuilder()
		{
			// todo: think about multithread
			if (_moduleBuilder == null)
			{
				AssemblyName aName = new AssemblyName("DeepClonerCode");
				var ab = AppDomain.CurrentDomain.DefineDynamicAssembly(aName, AssemblyBuilderAccess.Run);
				var mb = ab.DefineDynamicModule(aName.Name);
				_moduleBuilder = mb;
			}

			return _moduleBuilder;
		}
	}
}
#endif