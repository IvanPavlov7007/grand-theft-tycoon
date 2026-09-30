public class StreetsSingleton : GlobalSingletonBehaviour<StreetsSingleton>
{
	protected override StreetsSingleton GlobalInstance { get => G.StreetsSingleton; set => G.StreetsSingleton = value; }
}