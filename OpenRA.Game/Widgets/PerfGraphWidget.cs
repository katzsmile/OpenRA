#region Copyright & License Information
/*
 * Copyright 2007-2011 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made 
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Drawing;
using System.Linq;
using OpenRA.Graphics;
using OpenRA.Support;

namespace OpenRA.Widgets
{
	public class PerfGraphWidget : Widget
	{
		public PerfGraphWidget() : base() { }

		public override void DrawInner()
		{
			var rect = RenderBounds;
			float2 origin = Game.viewport.Location + new float2(rect.Right, rect.Bottom);
			float2 basis = new float2(-rect.Width / 100, -rect.Height / 100);

			Game.Renderer.LineRenderer.DrawLine(origin, origin + new float2(100, 0) * basis, Color.White, Color.White);
			Game.Renderer.LineRenderer.DrawLine(origin + new float2(100, 0) * basis, origin + new float2(100, 100) * basis, Color.White, Color.White);

            int k = 0;
			foreach (var item in PerfHistory.items.Values.ToArray())
			{
				int n = 0;
				item.Samples().Aggregate((a, b) =>
				{
					Game.Renderer.LineRenderer.DrawLine(
						origin + new float2(n, (float)a) * basis,
						origin + new float2(n + 1, (float)b) * basis,
						item.c, item.c);
					++n;
					return b;
				});

                var u = Game.viewport.Location + new float2(rect.Left, rect.Top);

                Game.Renderer.LineRenderer.DrawLine(
                    u + new float2(10, 10 * k + 5),
                    u + new float2(12, 10 * k + 5),
                    item.c, item.c);

                Game.Renderer.LineRenderer.DrawLine(
                    u + new float2(10, 10 * k + 4),
                    u + new float2(12, 10 * k + 4),
                    item.c, item.c);

                Game.Renderer.TinyFont.DrawText(item.Name, new float2(rect.Left, rect.Top) + new float2(18, 10 * k - 3), Color.White);
                Game.Renderer.Flush();
                ++k;
			}
		}
	}
}