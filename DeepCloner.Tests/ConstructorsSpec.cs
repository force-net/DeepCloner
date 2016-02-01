using System;

using NUnit.Framework;

namespace Force.DeepCloner.Tests
{
	[TestFixture]
	public class ConstructorsSpec
	{
		public class T1
		{
			private T1()
			{
			}

			public static T1 Create()
			{
				return new T1();
			}

			public int X { get; set; }
		}

		public class T2
		{
			public T2(int arg1, int arg2)
			{
			}

			public int X { get; set; }
		}

		[Test]
		public void Object_With_Private_Constructor_Should_Be_Cloned()
		{
			var t1 = T1.Create();
			t1.X = 42;
			var cloned = t1.DeepClone();
			t1.X = 0;
			Assert.That(cloned.X, Is.EqualTo(42));
		}

		[Test]
		public void Object_With_Complex_Constructor_Should_Be_Cloned()
		{
			var t2 = new T2(1, 2);
			t2.X = 42;
			var cloned = t2.DeepClone();
			t2.X = 0;
			Assert.That(cloned.X, Is.EqualTo(42));
		}

		[Test]
		public void Anonymous_Object_Should_Be_Cloned()
		{
			var t2 = new { A = 1, B = "x" };
			var cloned = t2.DeepClone();
			Assert.That(cloned.A, Is.EqualTo(1));
			Assert.That(cloned.B, Is.EqualTo("x"));
		}

		private class C3 : ContextBoundObject
		{
		}

		[Test]
		public void ContextBound_Object_Should_Be_Cloned()
		{
			// FormatterServices.CreateUninitializedObject cannot use context-bound objects
			var c = new C3();
			var cloned = c.DeepClone();
			Assert.That(cloned, Is.Not.Null);
		}
	}
}
