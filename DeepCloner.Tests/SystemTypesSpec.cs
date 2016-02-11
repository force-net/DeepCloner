using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

using NUnit.Framework;

namespace Force.DeepCloner.Tests
{
	[TestFixture(false)]
	[TestFixture(true)]
	public class SystemTypesSpec : BaseTest
	{
		public SystemTypesSpec(bool isSafeInit)
			: base(isSafeInit)
		{
		}

		[Test]
		public void StandardTypes_Should_Be_Cloned()
		{
			var b = new StringBuilder();
			b.Append("test1");
			var cloned = b.DeepClone();
			Assert.That(cloned.ToString(), Is.EqualTo("test1"));
			var arr = new[] { 1, 2, 3 };
			var enumerator = arr.GetEnumerator();
			enumerator.MoveNext();
			var enumCloned = enumerator.DeepClone();
			enumerator.MoveNext();
			Assert.That(enumCloned.Current, Is.EqualTo(1));
		}

		[Test(Description = "Just for fun, do not clone such object in real situation")]
		public void Type_With_Native_Resource_Should_Be_Cloned()
		{
			var fileName = Path.GetTempFileName();
			try
			{
				var writer = File.CreateText(fileName);
				writer.AutoFlush = true;
				writer.Write("1");
				var cloned = writer.DeepClone();
				writer.Write("2");
				cloned.Write(3);
				var f = typeof(FileStream).GetField("_handle", BindingFlags.NonPublic | BindingFlags.Instance);
				var f2 = typeof(SafeHandle).GetField("_state", BindingFlags.NonPublic | BindingFlags.Instance);
				Console.WriteLine(f2.GetValue(f.GetValue(writer.BaseStream)));
				Console.WriteLine(f2.GetValue(f.GetValue(cloned.BaseStream)));
				writer.Close();
				cloned.Close();
				// this was a bug, we should not throw there
				// Assert.Throws<ObjectDisposedException>(cloned.Close);
				var res = File.ReadAllText(fileName);
				Assert.That(res, Is.EqualTo("123"));
			}
			finally
			{
				if (File.Exists(fileName))
					File.Delete(fileName);
			}
		}

		[Test]
		public void Funcs_Should_Be_Cloned()
		{
			var closure = new[] { "123" };
			Func<int, string> f = x => closure[0] + x.ToString(CultureInfo.InvariantCulture);
			var df = f.DeepClone();
			var cf = f.ShallowClone();
			closure[0] = "xxx";
			Assert.That(f(3), Is.EqualTo("xxx3"));
			// we clone delegate together with a closure
			Assert.That(df(3), Is.EqualTo("1233"));
			Assert.That(cf(3), Is.EqualTo("xxx3"));
		}

		public class EventHandlerTest1
		{
			public event Action<int> Event;

			public int Call(int x)
			{
				if (Event != null)
				{
					Event(x);
					return Event.GetInvocationList().Length;
				}

				return 0;
			}
		}

		[Test(Description = "Some libraries notifies about problems with this types")]
		public void Events_Should_Be_Cloned()
		{
			var eht = new EventHandlerTest1();
			var summ = new int[1];
			Action<int> a1 = x => summ[0] += x;
			Action<int> a2 = x => summ[0] += x;
			eht.Event += a1;
			eht.Event += a2;
			eht.Call(1);
			Assert.That(summ[0], Is.EqualTo(2));
			var clone = eht.DeepClone();
			clone.Call(1);
			// do not call
			Assert.That(summ[0], Is.EqualTo(2));
			eht.Event -= a1;
			eht.Event -= a2;
			Assert.That(eht.Call(1), Is.EqualTo(0)); // 0
			Assert.That(summ[0], Is.EqualTo(2)); // nothing to increment
			Assert.That(clone.Call(1), Is.EqualTo(2));
		}
	}
}
