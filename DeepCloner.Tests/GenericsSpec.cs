using System;

using NUnit.Framework;

namespace Force.DeepCloner.Tests
{
#if !NETCORE
	[TestFixture(false)]
#endif
	[TestFixture(true)]
	public class GenericsSpec : BaseTest
	{
		public GenericsSpec(bool isSafeInit)
			: base(isSafeInit)
		{
		}

		[Test]
		public void Tuple_Should_Be_Cloned()
		{
			var c = new Tuple<int, int>(1, 2).DeepClone();
			Assert.That(c.Item1, Is.EqualTo(1));
			Assert.That(c.Item2, Is.EqualTo(2));

			c = new Tuple<int, int>(1, 2).ShallowClone();
			Assert.That(c.Item1, Is.EqualTo(1));
			Assert.That(c.Item2, Is.EqualTo(2));
			
			var cc = new Tuple<int, int, int, int, int, int, int>(1, 2, 3, 4, 5, 6, 7).DeepClone();
			Assert.That(cc.Item7, Is.EqualTo(7));

			var tuple = new Tuple<int, Generic<object>>(1, new Generic<object>());
			tuple.Item2.Value = tuple;
			var ccc = tuple.DeepClone();
			Assert.That(ccc, Is.EqualTo(ccc.Item2.Value));
		}

		[Test]
		public void Generic_Should_Be_Cloned()
		{
			var c = new Generic<int>();
			c.Value = 12;
			Assert.That(c.DeepClone().Value, Is.EqualTo(12));

			var c2 = new Generic<object>();
			c2.Value = 12;
			Assert.That(c2.DeepClone().Value, Is.EqualTo(12));
		}

		public class C1
		{
			public int X { get; set; }
		}

		public class C2 : C1
		{
			public int Y { get; set; }
		}

		public class Generic<T>
		{
			public T Value { get; set; }
		}

		[Test]
		public void Tuple_Should_Be_Cloned_With_Inheritance_And_Same_Object()
		{
			var c2 = new C2 { X = 1, Y = 2 };
			var c = new Tuple<C1, C2>(c2, c2).DeepClone();
			var cs = new Tuple<C1, C2>(c2, c2).ShallowClone();
			c2.X = 42;
			c2.Y = 42;
			Assert.That(c.Item1.X, Is.EqualTo(1));
			Assert.That(c.Item2.Y, Is.EqualTo(2));
			Assert.That(c.Item2, Is.EqualTo(c.Item1));

			Assert.That(cs.Item1.X, Is.EqualTo(42));
			Assert.That(cs.Item2.Y, Is.EqualTo(42));
			Assert.That(cs.Item2, Is.EqualTo(cs.Item1));
		}
	}
}
