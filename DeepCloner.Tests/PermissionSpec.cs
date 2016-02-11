using System;
using System.Security;
using System.Security.Permissions;

using CloneExtensions;

using NUnit.Framework;

namespace Force.DeepCloner.Tests
{
	[TestFixture]
	public class PermissionSpec
	{
		[Test, Ignore("Just manual check")]
		public void EnsurePermission()
		{
			var setup = new AppDomainSetup
			{
				ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
				ApplicationName = "sandbox",
			};

			var permissions = new PermissionSet(PermissionState.None);
			// assembly load
			permissions.AddPermission(new FileIOPermission(PermissionState.Unrestricted));
			// assembly execute
			permissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.Execution));

			// permissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess | ReflectionPermissionFlag.MemberAccess));
			permissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.MemberAccess));

			var test = AppDomain.CreateDomain("sandbox", null, setup, permissions);

			var instance = (Executor)test.CreateInstanceFromAndUnwrap(this.GetType().Assembly.Location, typeof(Executor).FullName);
			instance.CloneExtensionsClone();
			instance.DoShallowClone();
			instance.DoDeepClone();
		}

		public class Test
		{
			public int X { get; set; }

			private readonly object y = new object();

			private readonly UnsafeStructTest z;

			public object GetY()
			{
				return y;
			}
		}

		public struct UnsafeStructTest
		{
			public object Y { get; set; }
		}

		public class Executor : MarshalByRefObject
		{
			 public void DoDeepClone()
			 {
				 var test = new Test();
				 var clone = test.DeepClone();
				 if (clone.GetY() == test.GetY())
					 throw new Exception("Deep Clone fail");
			 }

			public void DoShallowClone()
			 {
				 new Test().ShallowClone();
			 }

			public void CloneExtensionsClone()
			{
				new Test().GetClone();
			}
		}
	}
}
