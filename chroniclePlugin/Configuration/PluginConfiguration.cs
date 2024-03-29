using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace chroniclePlugin.Configuration;
[Serializable]
public class PluginConfiguration : IPluginConfiguration
{
	public bool showTextNotify { get; set; } = true;

	private static readonly int VersionLatest = 0;
	public int Version { get; set; } = VersionLatest;

	public bool gameInteractionContextMenu { get; set; } = true;

	public float dialogueDrawSpeed { get; set; } = 0.05f;

	// the below exist just to make saving less cumbersome
	[NonSerialized]
	private DalamudPluginInterface? pluginInterface;

	public void Initialize(DalamudPluginInterface pluginInterface)
	{
		this.pluginInterface = pluginInterface;

		var needsResave = Version != VersionLatest;
		if (needsResave) { Save(); }
	}

	public void Save()
	{
		pluginInterface!.SavePluginConfig(this);
		//IPluginLog.Debug("Saved Configuration");
	}
}
