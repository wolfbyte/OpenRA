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

using System.Collections.Generic;
using OpenRA.Traits;

namespace OpenRA.Mods.Common.Traits
{
	public class BuildableInfo : TraitInfo<Buildable>
	{
		[Desc("The prerequisite names that must be available before this can be built.",
			"This can be prefixed with ! to invert the prerequisite (disabling production if the prerequisite is available)",
			"and/or ~ to hide the actor from the production palette if the prerequisite is not available.",
			"Prerequisites are granted by actors with the ProvidesPrerequisite trait.")]
		public readonly string[] Prerequisites = { };

		[Desc("Production queue(s) that can produce this.")]
		public readonly HashSet<string> Queue = new HashSet<string>();

		[Desc("Override the production structure type (from the Production Produces list) that this unit should be built at.")]
		public readonly string BuildAtProductionType = null;

		[Desc("Disable production when there are more than this many of this actor on the battlefield. Set to 0 to disable.")]
		public readonly int BuildLimit = 0;

		[Desc("Build this many of the actor at once.")]
		public readonly int BuildAmount = 1;

		[Desc("Force a specific faction variant, overriding the faction of the producing actor.")]
		public readonly string ForceFaction = null;

		[Desc("Show a tooltip when hovered over my icon.")]
		public readonly bool ShowTooltip = true;

		[Desc("Sequence of the actor that contains the icon.")]
		[SequenceReference] public readonly string Icon = "icon";

		[Desc("Palette used for the production icon.")]
		[PaletteReference] public readonly string IconPalette = "chrome";

		[Desc("Base build time in frames (-1 indicates to use the unit's Value).")]
		public readonly int BuildDuration = -1;

		[Desc("Percentage modifier to apply to the build duration.")]
		public readonly int BuildDurationModifier = 60;

		// TODO: UI fluff; doesn't belong here
		public readonly int BuildPaletteOrder = 9999;

		// TODO: UI fluff; doesn't belong here
		public readonly bool ForceIconLocation = false;

		[Desc("Text shown in the production tooltip.")]
		[Translate] public readonly string Description = "";

		[Desc("Notification played when production is complete.",
			"The filename of the audio is defined per faction in notifications.yaml.",
			"Defaults to what is set for the Queue actor built from.")]
		public readonly string ReadyAudio = null;

		[Desc("Notification played when user clicks on the build palette icon.",
			"The filename of the audio is defined per faction in notifications.yaml.",
			"Defaults to what is set for the Queue actor built from.")]
		public readonly string QueuedAudio = null;

		[Desc("Notification played when player right-clicks on the build palette icon.",
			"The filename of the audio is defined per faction in notifications.yaml.",
			"Defaults to what is set for the Queue actor built from.")]
		public readonly string OnHoldAudio = null;

		[Desc("Notification played when player right-clicks on a build palette icon that is already on hold.",
			"The filename of the audio is defined per faction in notifications.yaml.",
			"Defaults to what is set for the Queue actor built from.")]
		public readonly string CancelledAudio = null;

		public static string GetInitialFaction(ActorInfo ai, string defaultFaction)
		{
			var bi = ai.TraitInfoOrDefault<BuildableInfo>();
			return bi != null ? bi.ForceFaction ?? defaultFaction : defaultFaction;
		}
	}

	public class Buildable { }
}
