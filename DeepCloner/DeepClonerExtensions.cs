using Force.DeepCloner.Helpers;

namespace Force.DeepCloner
{
	public static class DeepClonerExtensions
	{
		public static T DeepClone<T>(this T obj)
		{
			return DeepClonerGenerator.CloneObject(obj);
		}
	}
}
