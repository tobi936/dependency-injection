namespace dependencyInjection.Messaging
{
	[AttributeUsage(AttributeTargets.Class)]
	internal sealed class MessengerAttribute : Attribute
	{
		public string DisplayName { get; }
		public string Note { get; }

		public MessengerAttribute(string displayName, string note)
		{
			DisplayName = displayName;
			Note = note;
		}
	}
}
