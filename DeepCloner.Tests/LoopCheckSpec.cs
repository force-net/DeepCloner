using NUnit.Framework;

namespace Force.DeepCloner.Tests
{
	[TestFixture(false)]
	[TestFixture(true)]
	public class LoopCheckSpec : BaseTest
	{
		public LoopCheckSpec(bool isSafeInit)
			: base(isSafeInit)
		{
		}

		public class C1
		{
			public int F { get; set; }

			public C1 A { get; set; }
		}

		[Test]
		public void SimpleLoop_Should_Be_Handled()
		{
			var c1 = new C1();
			var c2 = new C1();
			c1.F = 1;
			c2.F = 2;
			c1.A = c2;
			c1.A.A = c1;
			var cloned = c1.DeepClone();

			Assert.That(cloned.A, Is.Not.Null);
			Assert.That(cloned.A.A.F, Is.EqualTo(cloned.F));
			Assert.That(cloned.A.A, Is.EqualTo(cloned));
		}

		[Test]
		public void Object_Own_Loop_Should_Be_Handled()
		{
			var c1 = new C1();
			c1.F = 1;
			c1.A = c1;
			var cloned = c1.DeepClone();

			Assert.That(cloned.A, Is.Not.Null);
			Assert.That(cloned.A.F, Is.EqualTo(cloned.F));
			Assert.That(cloned.A, Is.EqualTo(cloned));
		}

		[Test]
		public void Array_Of_Same_Objects_Should_Be_Cloned()
		{
			var c1 = new C1();
			var arr = new[] { c1, c1, c1 };
			c1.F = 1;
			var cloned = arr.DeepClone();

			Assert.That(cloned.Length, Is.EqualTo(3));
			Assert.That(cloned[0], Is.EqualTo(cloned[1]));
			Assert.That(cloned[1], Is.EqualTo(cloned[2]));
		}
	}
}
