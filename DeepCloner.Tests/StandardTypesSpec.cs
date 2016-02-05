using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

using NUnit.Framework;

namespace Force.DeepCloner.Tests
{
	[TestFixture]
	public class StandardTypesSpec
	{
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
	}
}
