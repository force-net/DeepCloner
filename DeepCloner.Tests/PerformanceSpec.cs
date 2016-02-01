using System;
using System.Diagnostics;

using NUnit.Framework;

namespace Force.DeepCloner.Tests
{
	[TestFixture]
	public class PerformanceSpec
	{
		private class C1
		{
			public int V1 { get; set; }

			public string V2 { get; set; }
		}

		private class C2 : C1
		{
		}

		private C1 ManualClone(C1 x)
		{
			var y = new C1();
			y.V1 = x.V1;
			y.V2 = x.V2;
			return y;
		}

		[Test, Ignore("Manual")]
		public void Test_Construct_Variants()
		{
			var c1 = new C1();
			// warm up
			for (var i = 0; i < 1000; i++) ManualClone(c1);
			for (var i = 0; i < 1000; i++) c1.DeepClone();

			// test
			var sw = new Stopwatch();
			sw.Start();

			for (var i = 0; i < 100000; i++) ManualClone(c1);
			Console.WriteLine("Manual: " + sw.ElapsedMilliseconds);
			sw.Restart();

			for (var i = 0; i < 100000; i++) c1.DeepClone();
			Console.WriteLine("Deep: " + sw.ElapsedMilliseconds);
		}

		[Test, Ignore("Manual")]
		public void Test_Parent_Casting_Variants()
		{
			var c2 = new C2();
			var c1 = c2 as C1;
			// warm up
			for (var i = 0; i < 1000; i++) c1.DeepClone();
			for (var i = 0; i < 1000; i++) c2.DeepClone();

			// test
			var sw = new Stopwatch();
			sw.Start();

			for (var i = 0; i < 100000; i++) c2.DeepClone();
			Console.WriteLine("Direct: " + sw.ElapsedMilliseconds);
			sw.Restart();

			for (var i = 0; i < 100000; i++) c1.DeepClone();
			Console.WriteLine("Parent: " + sw.ElapsedMilliseconds);
		}
	}
}
