using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;

namespace Force.DeepCloner.Tests
{
	[TestFixture]
	public class PerformanceSpec
	{
		[Serializable]
		public class C1
		{
			public int V1 { get; set; }

			public string V2 { get; set; }

			public object O { get; set; }

			public C1 Clone()
			{
				return (C1)MemberwiseClone();
			}
		}

		private class C2 : C1
		{
		}

		private C1 ManualClone(C1 x)
		{
			var y = new C1();
			y.V1 = x.V1;
			y.V2 = x.V2;
			y.O = x.O;
			return y;
		}

		private T CloneViaFormatter<T>(T obj)
		{
			var bf = new BinaryFormatter();
			var ms = new MemoryStream();
			bf.Serialize(ms, obj);
			ms.Seek(0, SeekOrigin.Begin);
			return (T)bf.Deserialize(ms);
		}

		[Test, Ignore("Manual")]
		public void Test_Construct_Variants()
		{
			var c1 = new C1 { V1 = 1 };
			// warm up
			for (var i = 0; i < 1000; i++) ManualClone(c1);
			for (var i = 0; i < 1000; i++) c1.DeepClone();
			for (var i = 0; i < 1000; i++) CloneViaFormatter(c1);

			// test
			var sw = new Stopwatch();
			sw.Start();

			for (var i = 0; i < 1000000; i++) ManualClone(c1);
			Console.WriteLine("Manual: " + sw.ElapsedMilliseconds);
			sw.Restart();

			for (var i = 0; i < 1000000; i++) c1.DeepClone();
			Console.WriteLine("Deep: " + sw.ElapsedMilliseconds);
			sw.Restart();

			// inaccurate variant, but test should complete in reasonable time
			for (var i = 0; i < 100000; i++) CloneViaFormatter(c1);
			Console.WriteLine("Binary Formatter: " + (sw.ElapsedMilliseconds * 10));
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

		public struct S1
		{
			public C1 C;
		}

		[Test, Ignore("Manual")]
		public void Test_Array_Of_Structs_With_Class()
		{
			var c1 = new S1[100000];
			// warm up
			for (var i = 0; i < 2; i++) c1.DeepClone();

			// test
			var sw = new Stopwatch();
			sw.Start();

			for (var i = 0; i < 100; i++) c1.DeepClone();
			Console.WriteLine("Deep: " + sw.ElapsedMilliseconds);
		}

		[Test, Ignore("Manual")]
		public void Test_Shallow_Variants()
		{
			var c1 = new C1();
			// warm up
			for (var i = 0; i < 1000; i++) ManualClone(c1);
			for (var i = 0; i < 1000; i++) c1.Clone();
			for (var i = 0; i < 1000; i++) c1.ShallowClone();

			// test
			var sw = new Stopwatch();
			sw.Start();

			for (var i = 0; i < 1000000; i++) ManualClone(c1);
			Console.WriteLine("Manual External: " + sw.ElapsedMilliseconds);
			sw.Restart();

			for (var i = 0; i < 1000000; i++) c1.Clone();
			Console.WriteLine("Auto Internal: " + sw.ElapsedMilliseconds);
			sw.Restart();

			for (var i = 0; i < 1000000; i++) c1.ShallowClone();
			Console.WriteLine("Shallow: " + sw.ElapsedMilliseconds);
		}
	}
}
