#if !NETSTANDARD
using System;
using System.Collections.Generic;

using NUnit.Framework;

using ServiceStack.FluentValidation;

namespace Force.DeepCloner.Tests
{
	[TestFixture]
	public class ClrExpectionSpec
	{
		public sealed class ItemToBeCloned2 : BaseClassForTest2
		{
			public SomeCollection2 SomeCollectionProperty { get; set; }

			public ItemToBeCloned2()
			{
				SomeCollectionProperty = new SomeCollection2();
			}

			protected override IValidator GetValidator()
			{
				return new FVInventoryValidator();
			}

			private class FVInventoryValidator : AbstractValidator<ItemToBeCloned2>
			{
				public FVInventoryValidator()
				{
					RuleFor(x => x.SomeCollectionProperty).SetValidator(new SomeCollection2.SomeCollectionValidator2());
				}
			}

			public class SomeCollection2 : List<string>
			{
				internal SomeCollection2()
				{
				}

				#region Validation

				internal sealed class SomeCollectionValidator2 : AbstractValidator<SomeCollection2>
				{
					public SomeCollectionValidator2()
					{
						/*
						 * case 1: if commented out - no crash
						 * case 2: if left as is then works only with BaseClassForTest2.Validate #region #1
						 */
						RuleFor(x => 1 == 1);
					}
				}

				#endregion
			}
		}

		public abstract class BaseClassForTest2
		{
			private IValidator _validator;

			protected virtual IValidator GetValidator()
			{
				return null;
			}

			public void Validate()
			{
				#region #1 This works

				//var validator = GetValidator();
				//Console.WriteLine( validator );

				#endregion

				#region #2 This crashes

				_validator = GetValidator();

				#endregion
			}
		}

		[Test]
		[Repeat(1000)]
		[Ignore("For time being")]
		public void TestMethod2()
		{
			// typeof(ShallowObjectCloner).GetMethod("SwitchTo", BindingFlags.NonPublic | BindingFlags.Static)
			 //                          .Invoke(null, new object[] { true });

			// ServiceStack.FluentValidation.AbstractValidator
			var toBeCloned = new ItemToBeCloned2();
			toBeCloned.Validate();

			var cloned = DeepClonerExtensions.DeepClone(toBeCloned);

			Console.WriteLine(cloned);
		}
	}
}
#endif
