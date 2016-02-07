using System.Security;

using Force.DeepCloner.Helpers;

namespace Force.DeepCloner
{
	/// <summary>
	/// Extensions for object cloning
	/// </summary>
	public static class DeepClonerExtensions
	{
		/// <summary>
		/// Performs deep (full) copy of object and related graph
		/// </summary>
		// [PermissionSetAttribute(SecurityAction.Demand, Name = "FullTrust")] // <-- this attribute slowers execution
		public static T DeepClone<T>(this T obj)
		{
			return DeepClonerGenerator.CloneObject(obj);
		}

		/// <summary>
		/// Performs shallow (only new object returned, without cloning of dependencies) copy of object
		/// </summary>
		// [PermissionSet(SecurityAction.Demand, Name = "FullTrust")] // <-- this attribute slowers execution
		public static T ShallowClone<T>(this T obj)
		{
			return ShallowClonerGenerator.CloneObject(obj);
		}

		static DeepClonerExtensions()
		{
			if (!PermissionCheck())
			{
				throw new SecurityException("DeepCloner should have enough permissions to run. FullTrust set is enough to run.");
			}
		}

		private static bool PermissionCheck()
		{
			// best way to check required permission: execute something and receive exception
			// .net security policy is weird for normal usage
			try
			{
				new object().ShallowClone();
			}
			catch (VerificationException)
			{
				return false;
			}
			
			return true;
		}
	}
}
