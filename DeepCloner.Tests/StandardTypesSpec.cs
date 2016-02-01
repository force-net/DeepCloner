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
	}
}
