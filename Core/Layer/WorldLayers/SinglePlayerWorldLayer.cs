using Helion.Audio;
using Helion.Input;
using Helion.Maps;
using Helion.Resources;
using Helion.Resources.Archives.Collection;
using Helion.Resources.Definitions.MapInfo;
using Helion.Models;
using Helion.Util;
using Helion.Util.Configs;
using Helion.Util.Configs.Values;
using Helion.Util.Consoles;
using Helion.World;
using Helion.World.Entities.Players;
using Helion.World.Geometry;
using Helion.World.Geometry.Builder;
using Helion.World.Impl.SinglePlayer;
using Helion.World.StatusBar;
using NLog;
using System;
using System.Drawing;
using Helion.Render.OpenGL.Legacy.Commands;
using Helion.Render.OpenGL.Legacy.Shared;
using Helion.Render.OpenGL.Legacy.Shared.Drawers;
using Helion.Util.Timing;
using static Helion.Util.Assertion.Assert;

namespace Helion.Layer.WorldLayers
{
    public class SinglePlayerWorldLayer : WorldLayer
    {
        private const int TickOverflowThreshold = (int)(10 * Constants.TicksPerSecond);
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly Ticker m_ticker = new(Constants.TicksPerSecond);
        private readonly (ConfigValueEnum<Key>, TickCommands)[] m_consumeDownKeys;
        private readonly (ConfigValueEnum<Key>, TickCommands)[] m_consumePressedKeys;
        private readonly WorldHudDrawer m_worldHudDrawer;
        private readonly SinglePlayerWorld m_world;
        private TickerInfo m_lastTickInfo = new(0, 0);
        private TickCommand m_tickCommand = new();
        private bool m_drawAutomap;
        private bool m_disposed;

        public override WorldBase World => m_world;
        public MapInfoDef CurrentMap { get; set; }

        private SinglePlayerWorldLayer(GameLayer parent, Config config, HelionConsole console, ArchiveCollection archiveCollection,
            IAudioSystem audioSystem, SinglePlayerWorld world, MapInfoDef mapInfoDef)
            : base(parent, config, console, archiveCollection, audioSystem)
        {
            CurrentMap = mapInfoDef;
            m_world = world;
            m_worldHudDrawer = new(archiveCollection);

            m_ticker.Start();

            m_consumeDownKeys = new[]
            {
                (config.Controls.Forward,   TickCommands.Forward),
                (config.Controls.Backward,  TickCommands.Backward),
                (config.Controls.Left,      TickCommands.Left),
                (config.Controls.Right,     TickCommands.Right),
                (config.Controls.TurnLeft,  TickCommands.TurnLeft),
                (config.Controls.TurnRight, TickCommands.TurnRight),
                (config.Controls.LookDown,  TickCommands.LookDown),
                (config.Controls.LookUp,    TickCommands.LookUp),
                (config.Controls.Jump,      TickCommands.Jump),
                (config.Controls.Crouch,    TickCommands.Crouch),
                (config.Controls.Attack,    TickCommands.Attack),
                (config.Controls.AttackAlt, TickCommands.Attack),
                (config.Controls.Run,       TickCommands.Speed),
                (config.Controls.RunAlt,    TickCommands.Speed),
                (config.Controls.Strafe,    TickCommands.Strafe),
            };

            m_consumePressedKeys = new[]
            {
                (config.Controls.Use,            TickCommands.Use),
                (config.Controls.NextWeapon,     TickCommands.NextWeapon),
                (config.Controls.PreviousWeapon, TickCommands.PreviousWeapon),
                (config.Controls.WeaponSlot1,    TickCommands.WeaponSlot1),
                (config.Controls.WeaponSlot2,    TickCommands.WeaponSlot2),
                (config.Controls.WeaponSlot3,    TickCommands.WeaponSlot3),
                (config.Controls.WeaponSlot4,    TickCommands.WeaponSlot4),
                (config.Controls.WeaponSlot5,    TickCommands.WeaponSlot5),
                (config.Controls.WeaponSlot6,    TickCommands.WeaponSlot6),
                (config.Controls.WeaponSlot7,    TickCommands.WeaponSlot7),
            };
        }

        ~SinglePlayerWorldLayer()
        {
            FailedToDispose(this);
            PerformDispose();
        }

        public static SinglePlayerWorldLayer? Create(GameLayer parent, GlobalData globalData, Config config, HelionConsole console, 
            IAudioSystem audioSystem, ArchiveCollection archiveCollection, MapInfoDef mapInfoDef, 
            SkillDef skillDef, IMap map, Player? existingPlayer, WorldModel? worldModel)
        {
            string displayName = mapInfoDef.GetMapNameWithPrefix(archiveCollection);
            Log.Info(displayName);
            TextureManager.Init(archiveCollection, mapInfoDef);
            SinglePlayerWorld? world = CreateWorldGeometry(globalData, config, audioSystem, archiveCollection, mapInfoDef, skillDef, 
                map, existingPlayer, worldModel);
            if (world == null)
                return null;
            return new SinglePlayerWorldLayer(parent, config, console, archiveCollection, audioSystem, world, mapInfoDef);
        }

        private static SinglePlayerWorld? CreateWorldGeometry(GlobalData globalData, Config config, IAudioSystem audioSystem,
            ArchiveCollection archiveCollection, MapInfoDef mapDef, SkillDef skillDef, IMap map,
                Player? existingPlayer, WorldModel? worldModel)
        {
            MapGeometry? geometry = GeometryBuilder.Create(map, config);
            if (geometry == null)
                return null;

            try
            {
                return new SinglePlayerWorld(globalData, config, archiveCollection, audioSystem, geometry, mapDef, skillDef, map,
                    existingPlayer, worldModel);
            }
            catch(HelionException e)
            {
                Log.Error(e.Message);
            }

            return null;
        }

        public override void HandleInput(InputEvent input)
        {
            if (m_drawAutomap)
                HandleAutoMapInput(input);

            if (!m_world.Paused)
            {
                HandleMovementInput(input);
                m_world.HandleFrameInput(input);
            }

            if (input.ConsumeKeyPressed(Config.Controls.HudDecrease))
                ChangeHudSize(false);
            else if (input.ConsumeKeyPressed(Config.Controls.HudIncrease))
                ChangeHudSize(true);
            else if (input.ConsumeKeyPressed(Config.Controls.Automap))
            {
                m_drawAutomap = !m_drawAutomap;
                Config.Hud.AutoMapOffsetX.Set(0);
                Config.Hud.AutoMapOffsetY.Set(0);
            }

            base.HandleInput(input);
        }

        private void HandleAutoMapInput(InputEvent input)
        {
            if (input.ConsumeKeyPressed(Config.Controls.AutoMapDecrease))
                ChangeAutoMapSize(false);
            else if (input.ConsumeKeyPressed(Config.Controls.AutoMapIncrease))
                ChangeAutoMapSize(true);
            else if (input.ConsumeKeyPressed(Config.Controls.AutoMapUp))
                ChangeAutoMapOffsetY(true);
            else if (input.ConsumeKeyPressed(Config.Controls.AutoMapDown))
                ChangeAutoMapOffsetY(false);
            else if (input.ConsumeKeyPressed(Config.Controls.AutoMapRight))
                ChangeAutoMapOffsetX(true);
            else if (input.ConsumeKeyPressed(Config.Controls.AutoMapLeft))
                ChangeAutoMapOffsetX(false);
        }

        private void ChangeAutoMapOffsetY(bool increase)
        {
            if (increase)
                Config.Hud.AutoMapOffsetY.Set(Config.Hud.AutoMapOffsetY + 1);
            else
                Config.Hud.AutoMapOffsetY.Set(Config.Hud.AutoMapOffsetY - 1);
        }

        private void ChangeAutoMapOffsetX(bool increase)
        {
            if (increase)
                Config.Hud.AutoMapOffsetX.Set(Config.Hud.AutoMapOffsetX + 1);
            else
                Config.Hud.AutoMapOffsetX.Set(Config.Hud.AutoMapOffsetX - 1);
        }

        public override void RunLogic()
        {
            m_lastTickInfo = m_ticker.GetTickerInfo();
            int ticksToRun = m_lastTickInfo.Ticks;

            if (ticksToRun <= 0)
                return;

            m_world.SetTickCommand(m_tickCommand);

            if (ticksToRun > TickOverflowThreshold)
            {
                Log.Warn("Large tick overflow detected (likely due to delays/lag), reducing ticking amount");
                ticksToRun = 1;
            }

            while (ticksToRun > 0)
            {
                m_world.Tick();
                ticksToRun--;
            }
            
            base.RunLogic();
        }

        public override void Render(RenderCommands renderCommands)
        {
            if (m_drawAutomap)
            {
                // (int screenW, int screenH) = renderCommands.WindowDimension.Vector;
                // ImageBox2I screen = new ImageBox2I(0, 0, screenW, screenH);
                // renderCommands.FillRect(screen, Color.Black, 1.0f);
                renderCommands.Clear(Color.Black);
            }
            
            Camera camera = m_world.Player.GetCamera(m_lastTickInfo.Fraction);
            Player player = m_world.Player;
            renderCommands.DrawWorld(m_world, camera, m_lastTickInfo.Ticks, m_lastTickInfo.Fraction, player, m_drawAutomap);

            // TODO: Should not be passing the window dimension as the viewport.
            m_worldHudDrawer.Draw(player, m_world, m_lastTickInfo.Fraction, Console, renderCommands.WindowDimension,
                Config, m_drawAutomap, renderCommands);
            
            base.Render(renderCommands);
        }

        protected override void PerformDispose()
        {
            if (m_disposed)
                return;

            m_world.Dispose();

            m_disposed = true;

            base.PerformDispose();
        }

        private void HandleMovementInput(InputEvent input)
        {
            foreach (var (inputKey, command) in m_consumeDownKeys)
                if (input.ConsumeKeyPressedOrDown(inputKey))
                    m_tickCommand.Add(command);

            foreach (var (inputKey, command) in m_consumePressedKeys)
                if (input.ConsumeKeyPressed(inputKey))
                    m_tickCommand.Add(command);
        }

        private void ChangeAutoMapSize(bool increase)
        {
            if (increase)
                Config.Hud.AutoMapScale.Set(Config.Hud.AutoMapScale.Value + 0.1);
            else
                Config.Hud.AutoMapScale.Set(Config.Hud.AutoMapScale.Value - 0.1);
        }

        private void ChangeHudSize(bool increase)
        {
            int size = (int)Config.Hud.StatusBarSize.Value;
            if (increase)
                size++;
            else
                size--;

            size = Math.Clamp(size, 0, Enum.GetValues(typeof(StatusBarSizeType)).Length - 1);

            if ((int)Config.Hud.StatusBarSize.Value != size)
            {
                Config.Hud.StatusBarSize.Set((StatusBarSizeType)size);
                m_world.SoundManager.PlayStaticSound(Constants.MenuSounds.Change);
            }
        }
    }
}