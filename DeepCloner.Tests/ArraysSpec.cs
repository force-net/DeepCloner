using System.Collections.Generic;

using NUnit.Framework;

namespace Force.DeepCloner.Tests
{
	[TestFixture]
	public class ArraysSpec
	{
		[Test]
		public void IntArray_Should_Be_Cloned()
		{
			var arr = new[] { 1, 2, 3 };
			var cloned = arr.DeepClone();
			Assert.That(cloned.Length, Is.EqualTo(3));
			CollectionAssert.AreEqual(arr, cloned);
		}

		[Test]
		public void StringArray_Should_Be_Cloned()
		{
			var arr = new[] { "1", "2", "3" };
			var cloned = arr.DeepClone();
			Assert.That(cloned.Length, Is.EqualTo(3));
			CollectionAssert.AreEqual(arr, cloned);
		}

		public class C1
		{
			public C1(int x)
			{
				X = x;
			}

			public int X { get; set; }
		}

		[Test]
		public void ClassArray_Should_Be_Cloned()
		{
			var arr = new[] { new C1(1), new C1(2) };
			var cloned = arr.DeepClone();
			Assert.That(cloned.Length, Is.EqualTo(2));
			Assert.That(cloned[0].X, Is.EqualTo(1));
			Assert.That(cloned[1].X, Is.EqualTo(2));
		}

		public struct S1
		{
			public S1(int x)
			{
				X = x;
			}

			public int X;
		}

		public struct S2
		{
			 public C1 C;
		}

		[Test]
		public void StructArray_Should_Be_Cloned()
		{
			var arr = new[] { new S1(1), new S1(2) };
			var cloned = arr.DeepClone();
			Assert.That(cloned.Length, Is.EqualTo(2));
			Assert.That(cloned[0].X, Is.EqualTo(1));
			Assert.That(cloned[1].X, Is.EqualTo(2));
		}

		[Test]
		public void StructArray_With_Class_Should_Be_Cloned()
		{
			var arr = new[] { new S2 { C = new C1(1) }, new S2 { C = new C1(2) } };
			var cloned = arr.DeepClone();
			Assert.That(cloned.Length, Is.EqualTo(2));
			Assert.That(cloned[0].C.X, Is.EqualTo(1));
			Assert.That(cloned[1].C.X, Is.EqualTo(2));
		}

		[Test]
		public void NullArray_With_Should_Be_Cloned()
		{
			var arr = new C1[] { null, null };
			var cloned = arr.DeepClone();
			Assert.That(cloned.Length, Is.EqualTo(2));
			Assert.That(cloned[0], Is.Null);
			Assert.That(cloned[1], Is.Null);
		}

		[Test]
		public void IntList_Should_Be_Cloned()
		{
			// TODO: better performance for this type
			var arr = new List<int> { 1, 2, 3 };
			var cloned = arr.DeepClone();
			Assert.That(cloned.Count, Is.EqualTo(3));
			Assert.That(cloned[0], Is.EqualTo(1));
			Assert.That(cloned[1], Is.EqualTo(2));
			Assert.That(cloned[2], Is.EqualTo(3));
		}

		[Test]
		public void Dictionary_Should_Be_Cloned()
		{
			// TODO: better performance for this type
			var d = new Dictionary<string, decimal>();
			d["a"] = 1;
			d["b"] = 2;
			var cloned = d.DeepClone();
			Assert.That(cloned.Count, Is.EqualTo(2));
			Assert.That(cloned["a"], Is.EqualTo(1));
			Assert.That(cloned["b"], Is.EqualTo(2));
		}
	}
}
