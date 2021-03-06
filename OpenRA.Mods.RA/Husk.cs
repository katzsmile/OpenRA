﻿#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made 
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Collections.Generic;
using OpenRA.Traits;
using OpenRA.FileFormats;

namespace OpenRA.Mods.RA
{
	class HuskInfo : ITraitInfo
	{
		public object Create( ActorInitializer init ) { return new Husk( init ); }
	}

	class Husk : IOccupySpace, IFacing, ISync
	{
		[Sync]
		int2 location;
		
		[Sync]
		public int Facing { get; set; }
		public int ROT { get { return 0; } }
		public int InitialFacing { get { return 128; } }

		public Husk(ActorInitializer init)
		{
			this.location = init.Get<LocationInit,int2>();
			this.Facing = init.Contains<FacingInit>() ? init.Get<FacingInit,int>() : 128;
		}

		public int2 TopLeft { get { return location; } }

		public IEnumerable<Pair<int2, SubCell>> OccupiedCells() { yield return Pair.New(TopLeft, SubCell.FullCell); }
		public int2 PxPosition { get { return Util.CenterOfCell( location ); } }
	}
}
