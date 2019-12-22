using System;
using System.Security;

namespace Force.DeepCloner.Helpers
{
	internal static class PermissionChecker
	{
		public static void ThrowIfNoPermission()
		{
			if (!CheckPermission())
			{
				throw new SecurityException("DeepCloner should have enough permissions to run. Grant FullTrust or Reflection permission.");
			}
		}

		public static bool CheckPermission()
		{
			// best way to check required permission: execute something and receive exception
			// .net security policy is weird for normal usage
			try
			{
				DeepClonerGenerator.CloneObject(new object());
			}
			catch (VerificationException)
			{
				return false;
			}
			catch (MemberAccessException)
			{
				return false;
			}

			return true;
		}
	}
}