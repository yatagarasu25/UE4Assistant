namespace UE4Assistant;

public class UPluginItem
{
	public string Name = string.Empty;
	public bool Enabled = false;
	public string MarketplaceURL = string.Empty;
	public List<string> BlacklistPlatforms = new();
	public List<string> PlatformDenyList = new();

	public bool ShouldSerializeMarketplaceURL() => !MarketplaceURL.IsNullOrWhiteSpace();
	public bool ShouldSerializeBlacklistPlatforms() => BlacklistPlatforms != null && BlacklistPlatforms.Count > 0;
	public bool ShouldSerializePlatformDenyList() => PlatformDenyList != null && PlatformDenyList.Count > 0;
}
