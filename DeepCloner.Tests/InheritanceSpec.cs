using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

namespace Force.DeepCloner.Tests
{
	[TestFixture]
	public class InheritanceSpec
	{
		public class C1
		{
			[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
			public int X;

			[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
			public int Y;
		}

		public class C2 : C1
		{
			[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
			public new int X;

			[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
			public int Z;
		}

		[Test]
		public void Descendant_Should_Be_Cloned()
		{
			var c2 = new C2();
			c2.X = 1;
			c2.Y = 2;
			c2.Z = 3;
			var c1 = c2 as C1;
			c1.X = 4;
			var cloned = c1.DeepClone();
			Assert.That(cloned, Is.TypeOf<C2>());
			Assert.That(cloned.X, Is.EqualTo(4));
			Assert.That(cloned.Y, Is.EqualTo(2));
			Assert.That(((C2)cloned).Z, Is.EqualTo(3));
			Assert.That(((C2)cloned).X, Is.EqualTo(1));
		}

		[Test]
		public void Descendant_In_Array_Should_Be_Cloned()
		{
			var c1 = new C1();
			var c2 = new C2();
			var arr = new[] { c1, c2 };

			var cloned = arr.DeepClone();
			Assert.That(cloned[0], Is.TypeOf<C1>());
			Assert.That(cloned[1], Is.TypeOf<C2>());
		}
	}
}
