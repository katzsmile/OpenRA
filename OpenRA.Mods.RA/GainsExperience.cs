#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made 
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Collections.Generic;
using System.Linq;
using OpenRA.GameRules;
using OpenRA.Graphics;
using OpenRA.Mods.RA.Effects;
using OpenRA.Traits;
using OpenRA.FileFormats;

namespace OpenRA.Mods.RA
{
	public class GainsExperienceInfo : ITraitInfo, Requires<ValuedInfo>
	{
		public readonly float[] CostThreshold = { 2, 4, 8, 16 };
		public readonly float[] FirepowerModifier = { 1.1f, 1.15f, 1.2f, 1.5f };
		public readonly float[] ArmorModifier = { 1.1f, 1.2f, 1.3f, 1.5f };
		public readonly decimal[] SpeedModifier = { 1.1m, 1.15m, 1.2m, 1.5m };
		public object Create(ActorInitializer init) { return new GainsExperience(init, this); }
	}

	public class GainsExperience : IFirepowerModifier, ISpeedModifier, IDamageModifier, IRenderModifier, ISync
	{
		readonly Actor self;
		readonly int[] Levels;
		readonly GainsExperienceInfo Info;
		readonly Animation RankAnim;

		public GainsExperience(ActorInitializer init, GainsExperienceInfo info)
		{
			self = init.self;
			this.Info = info;
			var cost = self.Info.Traits.Get<ValuedInfo>().Cost;
			Levels = Info.CostThreshold.Select(t => (int)(t * cost)).ToArray();
			RankAnim = new Animation("rank");
			RankAnim.PlayFetchIndex("rank", () => Level - 1);

			if (init.Contains<ExperienceInit>())
			{
				Experience = init.Get<ExperienceInit, int>();

				while (Level < Levels.Length && Experience >= Levels[Level])
					Level++;
			}
		}

		[Sync]
		int Experience = 0;
		[Sync]
		public int Level { get; private set; }

		public void GiveOneLevel()
		{
			if (Level < Levels.Length)
				GiveExperience(Levels[Level] - Experience);
		}

		public void GiveExperience(int amount)
		{
			Experience += amount;

			while (Level < Levels.Length && Experience >= Levels[Level])
			{
				Level++;

//				Game.Debug("{0} became Level {1}".F(self.Info.Name, Level));
				var eva = self.World.WorldActor.Info.Traits.Get<EvaAlertsInfo>();
				Sound.PlayToPlayer(self.Owner, eva.LevelUp, self.CenterLocation);
				self.World.AddFrameEndTask(w => w.Add(new CrateEffect(self, "levelup", new int2(0,-24))));
			}
		}

        public float GetDamageModifier(Actor attacker, WarheadInfo warhead)
		{
			return Level > 0 ? 1 / Info.ArmorModifier[Level - 1] : 1;
		}

		public float GetFirepowerModifier()
		{
			return Level > 0 ? Info.FirepowerModifier[Level - 1] : 1;
		}

		public decimal GetSpeedModifier()
		{
			return Level > 0 ? Info.SpeedModifier[Level - 1] : 1m;
		}

		public IEnumerable<Renderable> ModifyRender(Actor self, IEnumerable<Renderable> rs)
		{
			if (self.Owner == self.World.LocalPlayer && Level > 0)
				return InnerModifyRender(self, rs);
			else
				return rs;
		}

		IEnumerable<Renderable> InnerModifyRender(Actor self, IEnumerable<Renderable> rs)
		{
			foreach (var r in rs)
				yield return r;

			RankAnim.Tick();	// hack
			var bounds = self.GetBounds(false);
			yield return new Renderable(RankAnim.Image,
				new float2(bounds.Right - 6, bounds.Bottom - 8), "effect", self.CenterLocation.Y);
		}
	}

	class ExperienceInit : IActorInit<int>
	{
		[FieldFromYamlKey]
		public readonly int value = 0;

		public ExperienceInit() { }

		public ExperienceInit(int init)
		{
			value = init;
		}

		public int Value(World world)
		{
			return value;
		}
	}
}
