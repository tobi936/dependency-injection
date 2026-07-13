namespace dependencyInjection.Model
{
	internal static class UserSeeder
	{
		public static void Seed(UserRepository users)
		{
			if (users.Users.Count > 0)
			{
				return;
			}

			users.Add(new User { Id = 1, Name = "John" });
			users.Add(new User { Id = 2, Name = "Jane" });
			users.Add(new User { Id = 3, Name = "Bob" });
			users.Add(new User { Id = 4, Name = "Alice" });
		}
	}
}
