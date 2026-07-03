using System.Collections.Generic;

namespace dependencyInjection
{
	internal class UserRepository
	{
		private readonly List<User> users = new();

		public IReadOnlyList<User> Users => users;

		public void Add(User user)
		{
			users.Add(user);
		}
	}
}
