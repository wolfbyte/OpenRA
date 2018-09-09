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

using System;
using System.Linq;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;
using OpenRA.Widgets;

namespace OpenRA.Mods.Common.Widgets.Logic
{
	public class SpawnSelectorTooltipLogic : ChromeLogic
	{
		[ObjectCreator.UseCtor]
		public SpawnSelectorTooltipLogic(Widget widget, TooltipContainerWidget tooltipContainer, MapPreviewWidget preview, bool showUnoccupiedSpawnpoints)
		{
			bool showTooltip = true;
			widget.IsVisible = () => preview.TooltipIconIndex != -1 && showTooltip;
			var label = widget.Get<LabelWidget>("LABEL");
			var flag = widget.Get<ImageWidget>("FLAG");
			var team = widget.Get<LabelWidget>("TEAM");
			var singleHeight = widget.Get("SINGLE_HEIGHT").Bounds.Height;
			var doubleHeight = widget.Get("DOUBLE_HEIGHT").Bounds.Height;
			var ownerFont = Game.Renderer.Fonts[label.Font];
			var teamFont = Game.Renderer.Fonts[team.Font];

			// Width specified in YAML is used as the margin between flag / label and label / border
			var labelMargin = widget.Bounds.Width;

			var cachedWidth = 0;
			var labelText = "";
			string playerFaction = null;
			var playerTeam = -1;

			tooltipContainer.BeforeRender = () =>
			{
				showTooltip = true;
				var occupant = preview.SpawnOccupants().Values.FirstOrDefault(c => c.SpawnPoint == preview.TooltipIconIndex);

				var teamWidth = 0;
				var actor = preview.HoveredIconActor;
				if (actor != null && !actor.TraitInfo<LobbyMapIconInfo>().Spawnpoint)
				{
					var lmitooltip = actor.TraitInfo<LobbyMapIconInfo>().Tooltip;
					var tooltips = actor.TraitInfos<TooltipInfo>().Where(t => t.EnabledByDefault);
					var tooltip = tooltips.Any() ? tooltips.First() : null;
					var name = tooltip != null ? tooltip.Name : null;
					labelText = lmitooltip ?? tooltip.Name;

					if (labelText == null)
						label.Text = actor.Name;

					widget.Bounds.Height = singleHeight;
				}
				else
				{
					if (occupant == null)
					{
						if (!showUnoccupiedSpawnpoints)
						{
							showTooltip = false;
							return;
						}

						labelText = "Available spawn";
						playerFaction = null;
						playerTeam = 0;
						widget.Bounds.Height = singleHeight;
					}
					else
					{
						labelText = occupant.PlayerName;
						playerFaction = occupant.Faction;
						playerTeam = occupant.Team;
						widget.Bounds.Height = playerTeam > 0 ? doubleHeight : singleHeight;
						teamWidth = teamFont.Measure(team.GetText()).X;
					}
				}

				label.Bounds.X = playerFaction != null ? flag.Bounds.Right + labelMargin : labelMargin;

				var textWidth = ownerFont.Measure(labelText).X;
				if (textWidth != cachedWidth)
				{
					label.Bounds.Width = textWidth;
					widget.Bounds.Width = 2 * label.Bounds.X + textWidth;
				}

				widget.Bounds.Width = Math.Max(teamWidth + 2 * labelMargin, label.Bounds.Right + labelMargin);
				team.Bounds.Width = widget.Bounds.Width;
			};

			label.GetText = () => labelText;
			flag.IsVisible = () => playerFaction != null;
			flag.GetImageCollection = () => "flags";
			flag.GetImageName = () => playerFaction;
			team.GetText = () => "Team {0}".F(playerTeam);
			team.IsVisible = () => playerTeam > 0;
		}
	}
}