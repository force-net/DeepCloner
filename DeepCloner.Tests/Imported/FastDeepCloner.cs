#if !NETCORE
// copied from https://raw.githubusercontent.com/Alenah091/FastDeepCloner/master/FastDeepCloner.cs because I need .NET 4.0 for tests
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Force.DeepCloner.Tests.Imported
{
	/// <summary>
	/// Supports cloning, which creates a new instance of a class with the same value as an existing instance.
	/// Used to deep clone objects, whether they are serializable or not.
	/// </summary>
	public class FastDeepCloner
	{
		#region Private fields
		private const BindingFlags Binding = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy;
		private Type _primaryType;
		private object _desireObjectToBeCloned;
		private int _length;
		private bool _isArray;
		private bool _isDictionary;
		private bool _isList;
		private int _rank;
		private bool? _initPublicOnly;
		private Type _ignorePropertiesWithAttribute;
		private static IDictionary<Type, List<FieldInfo>> _cachedFields;
		private static IDictionary<Type, List<PropertyInfo>> _cachedPropertyInfo;
		private FieldType _fieldType;
		private IDictionary<string, bool> _alreadyCloned;
		#endregion

		#region Constructors
		public FastDeepCloner(object desireObjectToBeCloned, FieldType fieldType)
		{
			if (desireObjectToBeCloned == null)
			{
				throw new ArgumentNullException("desireObjectToBeCloned");
			}

			DataBind(desireObjectToBeCloned, fieldType, null, false);
		}

		public FastDeepCloner(object desireObjectToBeCloned, FieldType fieldType = FieldType.FieldInfo, Type ignorePropertiesWithAttribute = null, bool? initPublicOnly = null)
		{
			if (desireObjectToBeCloned == null)
			{
				throw new ArgumentNullException("desireObjectToBeCloned");
			}

			DataBind(desireObjectToBeCloned, fieldType, ignorePropertiesWithAttribute, initPublicOnly);
		}
		#endregion

		#region Public method clone
		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>A new object that is a copy of this instance.</returns>
		public object Clone()
		{
			return DeepClone();
		}

		/// <summary>
		/// Creates a new object that is a copy of the current instance.
		/// </summary>
		/// <returns>A new object that is a copy of this instance.</returns>
		public T Clone<T>()
		{
			return (T)DeepClone();
		}
		#endregion

		#region Private method deep clone
		private void DataBind(object desireObjectToBeCloned, FieldType fieldType = FieldType.FieldInfo, Type ignorePropertiesWithAttribute = null, bool? initPublicOnly = null, IDictionary<string, bool> alreadyCloned = null)
		{
			if (desireObjectToBeCloned == null)
				return;
			if (_cachedFields == null)
				_cachedFields = new Dictionary<Type, List<FieldInfo>>();
			if (_cachedPropertyInfo == null)
				_cachedPropertyInfo = new Dictionary<Type, List<PropertyInfo>>();

			_alreadyCloned = alreadyCloned ?? new Dictionary<string, bool>();
			_ignorePropertiesWithAttribute = ignorePropertiesWithAttribute;
			_primaryType = desireObjectToBeCloned.GetType();
			_desireObjectToBeCloned = desireObjectToBeCloned;
			_isArray = _primaryType.IsArray;
			_initPublicOnly = initPublicOnly;
			_fieldType = fieldType;

			if (_isArray)
			{
				var array = (Array)desireObjectToBeCloned;
				_length = array.Length;
				_rank = array.Rank;
			}
			else if ((desireObjectToBeCloned as IList) != null)
				_isList = true;
			else if (typeof(IDictionary).IsAssignableFrom(_primaryType))
				_isDictionary = true;
		}

		/// <summary>
		/// Clone the object properties and its children recursively.
		/// </summary>
		/// <returns></returns>
		private object DeepClone()
		{
			if (_desireObjectToBeCloned == null)
				return null;
			// If the item is array of type more than one dimension then use Array.Clone
			if (_isArray && _rank > 1)
				return ((Array)_desireObjectToBeCloned).Clone();

			object tObject;
			// Clone IList or Array
			if (_isArray || _isList)
			{
				tObject = _isArray ? Array.CreateInstance(_primaryType.GetElementType(), _length) : Activator.CreateInstance(typeof(List<>).MakeGenericType(_primaryType.GetProperties().Last().PropertyType));
				var i = 0;
				foreach (var item in (IList)_desireObjectToBeCloned)
				{
					object clonedIteam = null;
					if (item != null)
					{
						var underlyingSystemType = item.GetType().UnderlyingSystemType;
						clonedIteam = (item is string || !underlyingSystemType.IsClass || IsInternalType(underlyingSystemType))
							? item
							: new FastDeepCloner(item, _fieldType, _ignorePropertiesWithAttribute, _initPublicOnly, _alreadyCloned).DeepClone();
					}
					if (!_isArray)
						((IList)tObject).Add(clonedIteam);
					else
						((Array)tObject).SetValue(clonedIteam, i);

					i++;
				}
			}
			else if (_isDictionary) // Clone IDictionary
			{
				tObject = Activator.CreateInstance(_primaryType);
				var dictionary = (IDictionary)_desireObjectToBeCloned;
				foreach (var key in dictionary.Keys)
				{
					var item = dictionary[key];
					object clonedIteam = null;
					if (item != null)
					{
						var underlyingSystemType = item.GetType().UnderlyingSystemType;
						clonedIteam = (item is string || !underlyingSystemType.IsClass || IsInternalType(underlyingSystemType))
							? item
							: new FastDeepCloner(item, _fieldType, _ignorePropertiesWithAttribute, _initPublicOnly, _alreadyCloned).DeepClone();
					}
					((IDictionary)tObject).Add(key, clonedIteam);
				}
			}
			else
			{
				// Create an empty object and ignore its constructor.
				tObject = FormatterServices.GetUninitializedObject(_primaryType);
				var fullPath = _primaryType.Name;
				if (_fieldType == FieldType.PropertyInfo)
				{
					if (!_cachedPropertyInfo.ContainsKey(_primaryType))
					{
						var properties = new List<PropertyInfo>();
						if (_primaryType.BaseType != null && _primaryType.BaseType.Name != "Object")
						{
							properties.AddRange(_primaryType.BaseType.GetProperties(Binding));
							properties.AddRange(_primaryType.GetProperties(Binding | BindingFlags.DeclaredOnly));
						}
						else properties.AddRange(_primaryType.GetProperties(Binding));

						_cachedPropertyInfo.Add(_primaryType, properties);
						if (_ignorePropertiesWithAttribute != null)
							_cachedPropertyInfo[_primaryType].RemoveAll(
								x => x.GetCustomAttributes(_ignorePropertiesWithAttribute, false).FirstOrDefault() != null);
					}
				}
				else if (!_cachedFields.ContainsKey(_primaryType))
				{
					var properties = new List<FieldInfo>();
					if (_primaryType.BaseType != null && _primaryType.BaseType.Name != "Object")
					{
						properties.AddRange(_primaryType.BaseType.GetFields(Binding));
						properties.AddRange(_primaryType.GetFields(Binding | BindingFlags.DeclaredOnly));
					}
					else properties.AddRange(_primaryType.GetFields(Binding));

					_cachedFields.Add(_primaryType, properties);
					if (_ignorePropertiesWithAttribute != null)
						_cachedFields[_primaryType].RemoveAll(
							x => x.GetCustomAttributes(_ignorePropertiesWithAttribute, false).FirstOrDefault() != null);
				}

				if (_fieldType == FieldType.FieldInfo)
				{
					foreach (var property in _cachedFields[_primaryType])
					{
						// Validate if the property is a writable one.
						if (property.IsInitOnly || property.FieldType == typeof(System.IntPtr))
							continue;
						if (_initPublicOnly.HasValue && _initPublicOnly.Value && !property.IsPublic)
							continue;
						if (_alreadyCloned.ContainsKey(fullPath + property.Name))
							continue;
						var value = property.GetValue(_desireObjectToBeCloned);
						if (value == null)
							continue;

						if (!property.FieldType.IsClass || value is string)
							property.SetValue(tObject, value);
						else
						{
							_alreadyCloned.Add(fullPath + property.Name, true);
							property.SetValue(tObject,
								new FastDeepCloner(value, _fieldType, _ignorePropertiesWithAttribute, _initPublicOnly,
									_alreadyCloned).DeepClone());
						}
					}
				}
				else
				{
					foreach (var property in _cachedPropertyInfo[_primaryType])
					{
						// Validate if the property is a writable one.
						if (!property.CanWrite || !property.CanRead || property.PropertyType == typeof(System.IntPtr))
							continue;
						if (_alreadyCloned.ContainsKey(fullPath + property.Name))
							continue;
						var value = property.GetValue(_desireObjectToBeCloned, null);
						if (value == null)
							continue;

						if (!property.PropertyType.IsClass || value is string)
							property.SetValue(tObject, value, null);
						else
						{
							_alreadyCloned.Add(fullPath + property.Name, true);
							property.SetValue(tObject,
								new FastDeepCloner(value, _fieldType, _ignorePropertiesWithAttribute, _initPublicOnly,
									_alreadyCloned).DeepClone(), null);
						}
					}
				}
			}

			return tObject;
		}
		#endregion

		private FastDeepCloner(object desireObjectToBeCloned, FieldType fielType = FieldType.FieldInfo, Type ignorePropertiesWithAttribute = null, bool? initPublicOnly = null, IDictionary<string, bool> alreadyCloned = null)
		{
			DataBind(desireObjectToBeCloned, fielType, ignorePropertiesWithAttribute, initPublicOnly, alreadyCloned);
		}

		/// <summary>
		/// Determines if the specified type is an internal type.
		/// </summary>
		/// <param name="underlyingSystemType"></param>
		/// <returns><c>true</c> if type is internal, else <c>false</c>.</returns>
		private static bool IsInternalType(Type underlyingSystemType)
		{
			return underlyingSystemType == typeof(string) ||
				underlyingSystemType == typeof(decimal) ||
				underlyingSystemType == typeof(int) ||
				underlyingSystemType == typeof(double) ||
				underlyingSystemType == typeof(float) ||
				underlyingSystemType == typeof(bool) ||
				underlyingSystemType == typeof(long) ||
				underlyingSystemType == typeof(DateTime) ||
				underlyingSystemType == typeof(ushort) ||
				underlyingSystemType == typeof(short) ||
				underlyingSystemType == typeof(sbyte) ||
				underlyingSystemType == typeof(byte) ||
				underlyingSystemType == typeof(ulong) ||
				underlyingSystemType == typeof(uint) ||
				underlyingSystemType == typeof(char) ||
				underlyingSystemType == typeof(TimeSpan);
		}
	}

	public enum FieldType
	{
		FieldInfo,
		PropertyInfo
	}
}
#endif