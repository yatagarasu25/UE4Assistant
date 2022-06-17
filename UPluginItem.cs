namespace UE4Assistant;

using System.Collections.Generic;
using SystemEx;

public class UPluginItem
{
	public string Name;
	public bool Enabled;
	public string MarketplaceURL;
	public List<string> BlacklistPlatforms = new List<string>();

	public bool ShouldSerializeMarketplaceURL() => !MarketplaceURL.IsNullOrWhiteSpace();
	public bool ShouldSerializeBlacklistPlatforms() => BlacklistPlatforms != null && BlacklistPlatforms.Count > 0;
}
