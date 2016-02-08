using System.Reflection;

using Force.DeepCloner.Helpers;

namespace Force.DeepCloner.Tests
{
	public class BaseTest
	{
		public BaseTest(bool isSafeInit)
		{
			SwitchTo(isSafeInit);
		}

		public static void SwitchTo(bool isSafeInit)
		{
			typeof(ShallowObjectCloner).GetMethod("SwitchTo", BindingFlags.NonPublic | BindingFlags.Static)
									.Invoke(null, new object[] { isSafeInit });
		}
	}
}
