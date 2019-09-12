using System;
using System.Collections.Generic;
using System.Drawing;
using Helion.Graphics.String;
using Helion.Render.Commands;
using Helion.Render.Commands.Align;
using Helion.Render.Shared.Drawers.Helper;
using Helion.Util;
using Helion.Util.Geometry;
using Helion.Util.Time;
using Helion.World;
using Helion.World.Entities.Players;
using MoreLinq;

namespace Helion.Render.Shared.Drawers
{
    public static class WorldHudDrawer
    {
        private const int MaxHudMessages = 4;
        private const int LeftOffset = 1;
        private const int TopOffset = 1;
        private const int MessageSpacing = 1;
        private const long MaxVisibleTimeNanos = 4 * 1000L * 1000L * 1000L;
        private const long FadingNanoSpan = 350L * 1000L * 1000L;
        private const long OpaqueNanoRange = MaxVisibleTimeNanos - FadingNanoSpan;

        public static void Draw(Player player, WorldBase world, HelionConsole console, Dimension viewport, RenderCommands cmd)
        {
            DrawHelper helper = new DrawHelper(cmd);
            
            cmd.ClearDepth();

            DrawHud(player, world, viewport, helper);
            DrawPickupFlash(world, helper);
            DrawDamage(world, helper);
            DrawRecentConsoleMessages(world, console, helper);
        }
        
        private static void DrawHud(Player player, WorldBase world, Dimension viewport, DrawHelper helper)
        {
            int x = 4;
            int y = viewport.Height - 4;
            helper.Image("MEDIA0", x, y, Alignment.BottomLeft, out Dimension medkitArea);

            x += medkitArea.Width;
            int health = Math.Max(0, player.Health);
            helper.Text(Color.Red, health.ToString(), "LargeHudFont", 24, x, y, Alignment.BottomLeft, out _);
        }
        
        private static void DrawPickupFlash(WorldBase world, DrawHelper helper)
        {
            // TODO
        }

        private static void DrawDamage(WorldBase world, DrawHelper helper)
        {
            // TODO
        }

        private static void DrawRecentConsoleMessages(WorldBase world, HelionConsole console, DrawHelper helper)
        {
            long currentNanos = Ticker.NanoTime();

            int messagesDrawn = 0;
            int offsetY = TopOffset;

            // We want to draw the ones that are less recent at the top first,
            // so when we iterate and see most recent to least recent, pushing
            // most recent onto the stack means when we iterate over this we
            // will draw the later ones at the top. Otherwise if we were to do
            // forward iteration without the stack, then they get drawn in the
            // reverse order and fading begins at the wrong end.
            Stack<(ColoredString msg, float alpha)> msgs = new Stack<(ColoredString, float)>();
            foreach (ConsoleMessage msg in console.Messages)
            {
                if (messagesDrawn >= MaxHudMessages)
                    break;

                if (msg.TimeNanos < world.CreationTimeNanos)
                    break;

                long timeSinceMessage = currentNanos - msg.TimeNanos;
                if (timeSinceMessage > MaxVisibleTimeNanos)
                    break;

                msgs.Push((msg.Message, CalculateFade(timeSinceMessage)));
                messagesDrawn++;
            }
            
            msgs.ForEach(pair =>
            {
                helper.Text(pair.msg, "SmallFont", 16, LeftOffset, offsetY, Alignment.TopLeft, 
                            pair.alpha, out Dimension drawArea);
                offsetY += drawArea.Height + MessageSpacing;
            });
        }

        private static float CalculateFade(long timeSinceMessage)
        {
            if (timeSinceMessage < OpaqueNanoRange)
                return 1.0f;

            double fractionIntoFadeRange = (double)(timeSinceMessage - OpaqueNanoRange) / FadingNanoSpan;
            return 1.0f - (float)fractionIntoFadeRange;
        }
    }
}