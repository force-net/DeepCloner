using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;

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

			permissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess | ReflectionPermissionFlag.MemberAccess));
			// permissions.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess));

			var test = AppDomain.CreateDomain("sandbox", null, setup, permissions);

			var instance = (Executor)test.CreateInstanceFromAndUnwrap(this.GetType().Assembly.Location, typeof(Executor).FullName);
			instance.DoShallowClone();
			instance.DoDeepClone();
		}

		public class Test
		{
			public int X { get; set; }
		}

		public class Executor : MarshalByRefObject
		{
			 public void DoDeepClone()
			 {
				 new List<int> { 1, 2, 3 }.DeepClone();
			 }

			 public void DoShallowClone()
			 {
				new List<int> { 1, 2, 3 }.ShallowClone();
			 }
		}
	}
}
