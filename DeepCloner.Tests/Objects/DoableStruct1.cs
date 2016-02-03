namespace Force.DeepCloner.Tests.Objects
{
	public struct DoableStruct1 : IDoable
	{
		public int X;

		public int Do()
		{
			return ++X;
		}
	}
}
