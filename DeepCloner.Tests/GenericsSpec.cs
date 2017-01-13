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
		}

		public class C1
		{
			public int X { get; set; }
		}

		public class C2 : C1
		{
			public int Y { get; set; }
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
