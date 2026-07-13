namespace dependencyInjection.Advanced
{
	// Marker-Interface fuer Ressourcen, deren Lebenszyklus wir sichtbar machen wollen.
	// Wird im "Disposal"-Demo verwendet, um .ExternallyOwned() vs. normale Disposal sichtbar zu machen.
	internal interface ITrackedResource
	{
		string Name { get; }
	}
}
