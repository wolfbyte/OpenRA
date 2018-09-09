#region Copyright & License Information
/*
 * Copyright 2007-2018 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

namespace OpenRA.Traits
{
	[Desc("Show an icon at the location of the actor on the map preview at lobby.",
		"`mpspawn` actor should have this for spawn points to be shown on map preview.")]
	public class LobbyMapIconInfo: TraitInfo<LobbyMapIcon>
	{
		[Desc("Image name at `chrome.yaml` that will be used for map icon.")]
		public readonly string Image = "map-preview-icons";

		[FieldLoader.Require]
		[Desc("Sequence name at `chrome.yaml` that will be used for map icon.")]
		public readonly string Sequence = null;

		[Desc("Show a tooltip when hovering over the icon.")]
		public readonly bool ShowTooltip = true;

		[Desc("Tooltip to show on lobby. Defaults to Tooltip trait.")]
		public readonly string Tooltip = null;

		[Desc("Does this actor shows spawnpoints.")]
		public readonly bool Spawnpoint = false;

		[Desc("If this actor shows spawnpoints, use this sequence when claimed.")]
		public readonly string ClaimedSequence = "spawn-claimed";
	}

	public class LobbyMapIcon { }
}
