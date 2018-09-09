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
using System.Linq;

namespace OpenRA.Mods.Common.UpdateRules.Rules
{
	public class AddLobbyMapIconToMPSpawn : UpdateRule
	{
		public override string Name { get { return "'mpspawn' actor now needs 'LobbyMapIcon' trait."; } }
		public override string Description
		{
			get
			{
				return "To show spawn points on map previews 'mpspawn' actor now requisres 'LobbyMapIcon' trait.";
			}
		}

		public override IEnumerable<string> UpdateActorNode(ModData modData, MiniYamlNode actorNode)
		{
			if (actorNode.Key == "mpspawn")
				AddLobbyMapIconNode(actorNode);

			yield break;
		}

		void AddLobbyMapIconNode(MiniYamlNode actorNode)
		{
			var lmi = new MiniYamlNode("LobbyMapIcon", "");
			lmi.AddNode("Image", "lobby-bits");
			lmi.AddNode("Sequence", "spawn-unclaimed");
			lmi.AddNode("Spawnpoint", "true");
			actorNode.AddNode(lmi);
		}
	}
}
