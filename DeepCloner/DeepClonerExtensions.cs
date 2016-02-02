using Force.DeepCloner.Helpers;

namespace Force.DeepCloner
{
	public static class DeepClonerExtensions
	{
		/// <summary>
		/// Performs deep (full) copy of object and related graph
		/// </summary>
		public static T DeepClone<T>(this T obj)
		{
			return DeepClonerGenerator.CloneObject(obj);
		}

		/// <summary>
		/// Performs shallow (only new object returned, without cloning of dependencies) copy of object
		/// </summary>
		public static T ShallowClone<T>(this T obj)
		{
			return ShallowClonerGenerator.CloneObject(obj);
		}
	}
}
