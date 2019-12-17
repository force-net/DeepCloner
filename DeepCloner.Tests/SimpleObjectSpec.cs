using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using Force.DeepCloner.Tests.Objects;

using NUnit.Framework;

namespace Force.DeepCloner.Tests
{
	[TestFixture(true)]
	[TestFixture(false)]
	public class SimpleObjectSpec : BaseTest
	{
		public SimpleObjectSpec(bool isSafeInit)
			: base(isSafeInit)
		{
		}

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

		public class C3
		{
			public string X { get; set; }
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
			Assert.That("xxxxxxxxxx yyyyyyyyyyyyyy".DeepClone(), Is.EqualTo("xxxxxxxxxx yyyyyyyyyyyyyy"));
			Assert.That(string.Empty.DeepClone(), Is.EqualTo(string.Empty));
			Assert.True(ReferenceEquals("y".DeepClone(), "y"));
			Assert.That(DateTime.MinValue.DeepClone(), Is.EqualTo(DateTime.MinValue));
			Assert.That(AttributeTargets.Delegate.DeepClone(), Is.EqualTo(AttributeTargets.Delegate));
			Assert.That(((object)null).DeepClone(), Is.Null);
			var obj = new object();
			Assert.That(obj.DeepClone(), Is.Not.Null);
			Assert.That(true.DeepClone(), Is.True);
			Assert.That(((object)true).DeepClone(), Is.True);
			Assert.That(obj.DeepClone().GetType(), Is.EqualTo(typeof(object)));
			Assert.That(obj.DeepClone(), Is.Not.EqualTo(obj));
		}

		[Test]
		public void Guid_Should_Be_Cloned()
		{
			var g = Guid.NewGuid();
			Assert.That(g.DeepClone(), Is.EqualTo(g));
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

		[Test]
		public void String_In_Class_Should_Not_Be_Cloned()
		{
			var c = new C3 { X = "aaa" };
			var cloned = c.DeepClone();
			Assert.That(cloned.X, Is.EqualTo(c.X));
			Assert.True(ReferenceEquals(cloned.X, c.X));
		}

		public sealed class C6
		{
			public readonly int X = 1;

			private readonly object y = new object();

			// it is struct - and it can't be null, but it's readonly and should be copied
			// also it private to ensure it copied correctly
#pragma warning disable 169
			private readonly StructWithObject z;
#pragma warning restore 169

			public object GetY()
			{
				return y;
			}
		}

		public struct StructWithObject
		{
			public readonly object Z;
		}

		[Test]
		public void Object_With_Readonly_Fields_Should_Be_Cloned()
		{
			var c = new C6();
			var clone = c.DeepClone();
			Assert.That(clone, Is.Not.EqualTo(c));
			Assert.That(clone.X, Is.EqualTo(1));
			Assert.That(clone.GetY(), Is.Not.Null);
			Assert.That(clone.GetY(), Is.Not.EqualTo(c.GetY()));
			Assert.That(clone.GetY(), Is.Not.EqualTo(c.GetY()));
		}

		public class VirtualClass1
		{
			public virtual int A { get; set; }

			public virtual int B { get; set; }

			// not safe
			public object X { get; set; }
		}

		public class VirtualClass2 : VirtualClass1
		{
			public override int B { get; set; }
		}

		[Test(Description = "Nothings special, just for checking")]
		public void Class_With_Virtual_Methods_Should_Be_Cloned()
		{
			var v2 = new VirtualClass2();
			v2.A = 1;
			v2.B = 2;
			var v1 = v2 as VirtualClass1;
			v1.A = 3;
			var clone = v1.DeepClone() as VirtualClass2;
			v2.B = 0;
			v2.A = 0;
			Assert.That(clone.B, Is.EqualTo(2));
			Assert.That(clone.A, Is.EqualTo(3));
		}

		[Test(Description = "DBNull is compared by value, so, we don't need to clone it")]
		public void DbNull_Should_Not_Be_Cloned()
		{
			var v = DBNull.Value;
			Assert.That(v == v.DeepClone(), Is.True);
			Assert.That(v == v.ShallowClone(), Is.True);
		}
		
		public class EmptyClass {}
		
		[Test(Description = "Empty class does not have any mutable properties, so, it safe to use same class in cloning"),
		 Ignore("Think about logic, which is better to clone or not to clone, I do not know, but it changes current logic seriously")]
		public void Empty_Should_Not_Be_Cloned()
		{
			var v = new EmptyClass();
			Assert.That(ReferenceEquals(v, v.DeepClone()), Is.True);
			Assert.That(ReferenceEquals(v, v.ShallowClone()), Is.True);
		}
		
		[Test(Description = "Reflection classes should not be cloned")]
		public void MethodInfo_Should_Not_Be_Cloned()
		{
#if NETCORE13
			var v = GetType().GetTypeInfo().GetMethod("MethodInfo_Should_Not_Be_Cloned");
#else
			var v = GetType().GetMethod("MethodInfo_Should_Not_Be_Cloned");
#endif
			Assert.That(ReferenceEquals(v, v.DeepClone()), Is.True);
			Assert.That(ReferenceEquals(v, v.ShallowClone()), Is.True);
		}

		public class Readonly1
		{
			public readonly object X;
			
			public object Z = new object();

			public Readonly1(string x)
			{
				X = x;
			}
		}

		[Test]
		public void Readonly_Field_Should_Remain_ReadOnly()
		{
			var c = new Readonly1("Z").DeepClone();
			Assert.That(c.X, Is.EqualTo("Z"));
			Assert.That(typeof(Readonly1).GetField("X").IsInitOnly, Is.True);
		}
	}
}
