using System;
using System.Diagnostics.CodeAnalysis;

using Force.DeepCloner.Tests.Objects;

using NUnit.Framework;

namespace Force.DeepCloner.Tests
{
	[TestFixture]
	public class SimpleObjectSpec
	{
		[Test]
		public void SimpleObject_Should_Be_Cloned()
		{
			var obj = new TestObject1 { Int = 42, Byte = 42, Short = 42, Long = 42, DateTime = new DateTime(2001, 01, 01), Char = 'X', Decimal = 1.2m, Double = 1.3, Float = 1.4f, String = "test1", UInt = 42, ULong = 42, UShort = 42, Bool = true, IntPtr = new IntPtr(42), UIntPtr = new UIntPtr(42), Enum = AttributeTargets.Delegate };

			var cloned = obj.DeepClone();
			Assert.That(cloned.Byte, Is.EqualTo(42));
			Assert.That(cloned.Short, Is.EqualTo(42));
			Assert.That(cloned.UShort, Is.EqualTo(42));
			Assert.That(cloned.Int, Is.EqualTo(42));
			Assert.That(cloned.UInt, Is.EqualTo(42));
			Assert.That(cloned.Long, Is.EqualTo(42));
			Assert.That(cloned.ULong, Is.EqualTo(42));
			Assert.That(cloned.Decimal, Is.EqualTo(1.2));
			Assert.That(cloned.Double, Is.EqualTo(1.3));
			Assert.That(cloned.Float, Is.EqualTo(1.4f));
			Assert.That(cloned.Char, Is.EqualTo('X'));
			Assert.That(cloned.String, Is.EqualTo("test1"));
			Assert.That(cloned.DateTime, Is.EqualTo(new DateTime(2001, 1, 1)));
			Assert.That(cloned.Bool, Is.EqualTo(true));
			Assert.That(cloned.IntPtr, Is.EqualTo(new IntPtr(42)));
			Assert.That(cloned.UIntPtr, Is.EqualTo(new UIntPtr(42)));
			Assert.That(cloned.Enum, Is.EqualTo(AttributeTargets.Delegate));
		}

		public struct S1
		{
			public int A;
		}

		public struct S2
		{
			public S3 S;
		}

		public struct S3
		{
			public bool B;
		}

		[Test(Description = "We have an special logic for simple structs, so, this test checks that this logic works correctly")]
		public void SimpleStruct_Should_Be_Cloned()
		{
			var s1 = new S1 { A = 1 };
			var cloned = s1.DeepClone();
			Assert.That(cloned.A, Is.EqualTo(1));
		}

		[Test(Description = "We have an special logic for simple structs, so, this test checks that this logic works correctly")]
		public void Simple_Struct_With_Child_Should_Be_Cloned()
		{
			var s1 = new S2 { S = new S3 { B = true } };
			var cloned = s1.DeepClone();
			Assert.That(cloned.S.B, Is.EqualTo(true));
		}

		public class ClassWithNullable
		{
			public int? A { get; set; }

			public long? B { get; set; }
		}

		[Test]
		public void Nullable_Shoild_Be_Cloned()
		{
			var c = new ClassWithNullable { B = 42 };
			var cloned = c.DeepClone();
			Assert.That(cloned.A, Is.Null);
			Assert.That(cloned.B, Is.EqualTo(42));
		}

		public class C1
		{
			public C2 C { get; set; }
		}

		public class C2
		{
		}

		[Test]
		public void Class_Should_Be_Cloned()
		{
			var c1 = new C1();
			c1.C = new C2();
			var cloned = c1.DeepClone();
			Assert.That(cloned.C, Is.Not.Null);
			Assert.That(cloned.C, Is.Not.EqualTo(c1.C));
		}

		public struct S4
		{
			public C2 C;

			public int F;
		}

		[Test]
		public void StructWithClass_Should_Be_Cloned()
		{
			var c1 = new S4();
			c1.F = 1;
			c1.C = new C2();
			var cloned = c1.DeepClone();
			c1.F = 2;
			Assert.That(cloned.C, Is.Not.Null);
			Assert.That(cloned.F, Is.EqualTo(1));
		}

		[Test]
		public void Privitive_Should_Be_Cloned()
		{
			Assert.That(3.DeepClone(), Is.EqualTo(3));
			Assert.That('x'.DeepClone(), Is.EqualTo('x'));
			Assert.That("x".DeepClone(), Is.EqualTo("x"));
			Assert.That(DateTime.MinValue.DeepClone(), Is.EqualTo(DateTime.MinValue));
			Assert.That(AttributeTargets.Delegate.DeepClone(), Is.EqualTo(AttributeTargets.Delegate));
			Assert.That(((object)null).DeepClone(), Is.Null);
			var obj = new object();
			Assert.That(obj.DeepClone(), Is.Not.Null);
			Assert.That(obj.DeepClone().GetType(), Is.EqualTo(typeof(object)));
			Assert.That(obj.DeepClone(), Is.Not.EqualTo(obj));
		}

		private class UnsafeObject
		{
			[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
			public unsafe void* Void;

			[SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Reviewed. Suppression is OK here.")]
			public unsafe int* Int;
		}

		[Test]
		public void Unsafe_Should_Be_Cloned()
		{
			var u = new UnsafeObject();
			var i = 1;
			var j = 2;
			unsafe
			{
				u.Int = &i;
				u.Void = &i;
			}
			
			var cloned = u.DeepClone();
			unsafe
			{
				u.Int = &j;
				Assert.That(cloned.Int == &i, Is.True);
				Assert.That(cloned.Void == &i, Is.True);
			}
		}
	}
}
