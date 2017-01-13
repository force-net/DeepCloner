using System;
using System.Diagnostics.CodeAnalysis;

using NUnit.Framework;

namespace Force.DeepCloner.Tests
{
#if !NETCORE
	[TestFixture(false)]
#endif
	[TestFixture(true)]
	public class InheritanceSpec : BaseTest
	{
		public InheritanceSpec(bool isSafeInit)
			: base(isSafeInit)
		{
		}

		public class C1 : IDisposable
		{
			[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
			public int X;

			[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
			public int Y;

			[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
			public object O; // make it not safe

			public void Dispose()
			{
			}
		}

		public class C2 : C1
		{
			[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
			public new int X;

			[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
			public int Z;
		}

		public class C1P : IDisposable
		{
			public int X { get; set; }

			[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
			public int Y { get; set; }

			[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
			public object O; // make it not safe

			public void Dispose()
			{
			}
		}

		public class C2P : C1P
		{
			[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
			public new int X { get; set; }

			[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
			public int Z { get; set; }
		}

		public struct S1 : IDisposable
		{
			public C1 X { get; set; }

			public int F;

			public void Dispose()
			{
			}
		}

		public struct S2 : IDisposable
		{
			public IDisposable X { get; set; }

			public void Dispose()
			{
			}
		}

		public class C3
		{
			public C1 X { get; set; }
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
		public void Class_Should_Be_Cloned_With_Parents()
		{
			var c2 = new C2P();
			c2.X = 1;
			c2.Y = 2;
			c2.Z = 3;
			var c1 = c2 as C1P;
			c1.X = 4;
			var cloned = c2.DeepClone();
			c2.X = 100;
			c2.Y = 100;
			c2.Z = 100;
			c1.X = 100;
			Assert.That(cloned, Is.TypeOf<C2P>());
			Assert.That(((C1P)cloned).X, Is.EqualTo(4));
			Assert.That(cloned.Y, Is.EqualTo(2));
			Assert.That(cloned.Z, Is.EqualTo(3));
			Assert.That(cloned.X, Is.EqualTo(1));
		}

		public struct S3
		{
			public C1P X { get; set; }

			public C1P Y { get; set; }
		}

		[Test]
		public void Struct_Should_Be_Cloned_With_Class_With_Parents()
		{
			var c2 = new S3();
			c2.X = new C1P();
			c2.Y = new C2P();

			c2.X.X = 1;
			c2.X.Y = 2;
			c2.Y.X = 3;
			c2.Y.Y = 4;
			((C2P)c2.Y).X = 5;
			((C2P)c2.Y).Z = 6;
			var cloned = c2.DeepClone();
			c2.X.X = 100;
			c2.X.Y = 200;
			c2.Y.X = 300;
			c2.Y.Y = 400;
			((C2P)c2.Y).X = 500;
			((C2P)c2.Y).Z = 600;
			Assert.That(cloned, Is.TypeOf<S3>());
			Assert.That(cloned.X.X, Is.EqualTo(1));
			Assert.That(cloned.X.Y, Is.EqualTo(2));
			Assert.That(cloned.Y.X, Is.EqualTo(3));
			Assert.That(cloned.Y.Y, Is.EqualTo(4));
			Assert.That(((C2P)cloned.Y).X, Is.EqualTo(5));
			Assert.That(((C2P)cloned.Y).Z, Is.EqualTo(6));
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

		[Test]
		public void Struct_Casted_To_Interface_Should_Be_Cloned()
		{
			var s1 = new S1();
			s1.F = 1;
			var disp = s1 as IDisposable;
			var cloned = disp.DeepClone();
			s1.F = 2;
			Assert.That(cloned, Is.TypeOf<S1>());
			Assert.That(((S1)cloned).F, Is.EqualTo(1));
		}

		public IDisposable CCC(IDisposable xx)
		{
			var x = (S1)xx;
			return x;
		}

		[Test]
		public void Class_Casted_To_Object_Should_Be_Cloned()
		{
			var c3 = new C3();
			c3.X = new C1();
			var obj = c3 as object;
			var cloned = obj.DeepClone();
			Assert.That(cloned, Is.TypeOf<C3>());
			Assert.That(c3, Is.Not.EqualTo(cloned));
			Assert.That(((C3)cloned).X, Is.Not.Null);
			Assert.That(((C3)cloned).X, Is.Not.EqualTo(c3.X));
		}

		[Test]
		public void Class_Casted_To_Interface_Should_Be_Cloned()
		{
			var c1 = new C1();
			var disp = c1 as IDisposable;
			var cloned = disp.DeepClone();
			Assert.That(c1, Is.Not.EqualTo(cloned));
			Assert.That(cloned, Is.TypeOf<C1>());
		}

		[Test]
		public void Struct_Casted_To_Interface_With_Class_As_Interface_Should_Be_Cloned()
		{
			var s2 = new S2();
			s2.X = new C1();
			var disp = s2 as IDisposable;
			var cloned = disp.DeepClone();
			Assert.That(cloned, Is.TypeOf<S2>());
			Assert.That(((S2)cloned).X, Is.TypeOf<C1>());
			Assert.That(((S2)cloned).X, Is.Not.EqualTo(s2.X));
		}

		[Test]
		public void Array_Of_Struct_Casted_To_Interface_Should_Be_Cloned()
		{
			var s1 = new S1();
			var arr = new IDisposable[] { s1, s1 };
			var clonedArr = arr.DeepClone();
			Assert.That(clonedArr[0], Is.EqualTo(clonedArr[1]));
		}
	}
}
