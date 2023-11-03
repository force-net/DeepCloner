using Force.DeepCloner.Helpers;

namespace Force.DeepCloner
{
	/// <summary>
	/// Interface for classes capable to perform copy of object
	/// </summary>
	public interface ICloner
	{
		/// <summary>
		/// Performs deep (full) copy of object and related graph
		/// </summary>
		T DeepClone<T>(T obj);

		/// <summary>
		/// Performs deep (full) copy of object and related graph to existing object
		/// </summary>
		/// <returns>existing filled object</returns>
		/// <remarks>Method is valid only for classes, classes should be descendants in reality, not in declaration</remarks>
		TTo DeepCloneTo<TFrom, TTo>(TFrom objFrom, TTo objTo) where TTo : class, TFrom;

		/// <summary>
		/// Performs shallow copy of object to existing object
		/// </summary>
		/// <returns>existing filled object</returns>
		/// <remarks>Method is valid only for classes, classes should be descendants in reality, not in declaration</remarks>
		TTo ShallowCloneTo<TFrom, TTo>(TFrom objFrom, TTo objTo) where TTo : class, TFrom;
	}

	/// <summary>
	/// Performs copy of object
	/// </summary>
	public class Cloner : ICloner
	{
		public Cloner()
		{
			PermissionChecker.ThrowIfNoPermission();
		}

		/// <summary>
		/// Performs deep (full) copy of object and related graph
		/// </summary>
		public T DeepClone<T>(T obj)
		{
			return DeepClonerGenerator.CloneObject(obj);
		}

		/// <summary>
		/// Performs deep (full) copy of object and related graph to existing object
		/// </summary>
		/// <returns>existing filled object</returns>
		/// <remarks>Method is valid only for classes, classes should be descendants in reality, not in declaration</remarks>
		public TTo DeepCloneTo<TFrom, TTo>(TFrom objFrom, TTo objTo) where TTo : class, TFrom
		{
			return (TTo)DeepClonerGenerator.CloneObjectTo(objFrom, objTo, true);
		}

		/// <summary>
		/// Performs shallow copy of object to existing object
		/// </summary>
		/// <returns>existing filled object</returns>
		/// <remarks>Method is valid only for classes, classes should be descendants in reality, not in declaration</remarks>
		public TTo ShallowCloneTo<TFrom, TTo>(TFrom objFrom, TTo objTo) where TTo : class, TFrom
		{
			return (TTo)DeepClonerGenerator.CloneObjectTo(objFrom, objTo, false);
		}

		/// <summary>
		/// Performs shallow (only new object returned, without cloning of dependencies) copy of object
		/// </summary>
		public static T ShallowClone<T>(T obj)
		{
			return ShallowClonerGenerator.CloneObject(obj);
		}
	}
}