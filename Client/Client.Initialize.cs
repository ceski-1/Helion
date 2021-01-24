﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Helion.Layer;
using Helion.Layer.WorldLayers;
using Helion.Resources.Definitions.MapInfo;
using Helion.Resources.IWad;
using Helion.Util.Time;
using Helion.World.Util;

namespace Helion.Client
{
    public partial class Client
    {
        private const int StopwatchFrequencyValue = 1000000;

        private readonly FpsTracker m_fpsTracker = new();
        private readonly Stopwatch m_fpsLimit = new();
        private int m_fpsLimitValue;

        private void SetFPSLimit()
        {
            if (m_config.Render.MaxFPS > 0)
                m_fpsLimitValue = StopwatchFrequencyValue / m_config.Render.MaxFPS;
            m_fpsLimit.Start();
        }

        private void Initialize()
        {
            SetFPSLimit();
            LoadFiles();

            if (m_commandLineArgs.Skill.HasValue)
                SetSkill(m_commandLineArgs.Skill.Value);

            m_config.Game.NoMonsters = m_commandLineArgs.NoMonsters;

            CheckLoadMap();
        }

        private void LoadFiles()
        {
            if (!m_archiveCollection.Load(m_commandLineArgs.Files, GetIwad()))
                Log.Error("Unable to load files at startup");
        }

        private void CheckLoadMap()
        {
            if (m_commandLineArgs.Map != null)
            {
                Loadmap(m_commandLineArgs.Map);
            }
            else if (m_commandLineArgs.Warp != null)
            {
                if (MapWarp.GetMap(m_commandLineArgs.Warp, m_archiveCollection.Definitions.MapInfoDefinition.MapInfo,
                    out MapInfoDef? mapInfoDef) && mapInfoDef != null)
                    Loadmap(mapInfoDef.MapName);
            }
            else
            {
                MapInfoDef? mapInfoDef = GetDefaultMap();
                if (mapInfoDef == null)
                {
                    Log.Error("Unable to find start map.");
                    return;
                }
                Loadmap(mapInfoDef.MapName);
            }
        }

        private string? GetIwad()
        {
            if (m_commandLineArgs != null && m_commandLineArgs.Iwad != null)
                return m_commandLineArgs.Iwad;

            string? iwad = LocateIwad();
            if (iwad == null)
            {
                Log.Error("No IWAD found!");
                return null;
            }
            else
            {
                return iwad;
            }
        }

        private static string? LocateIwad()
        {
            IWadLocator iwadLocator = new(new[] { Directory.GetCurrentDirectory() });
            List<(string, IWadInfo)> iwadData = iwadLocator.Locate();
            if (iwadData.Count > 0)
                return iwadData[0].Item1;

            return null;
        }

        private MapInfoDef? GetDefaultMap()
        {
            if (m_archiveCollection.Definitions.MapInfoDefinition.MapInfo.Episodes.Count == 0)
            {
                Log.Error("No episodes defined.");
                return null;
            }

            var mapInfo = m_archiveCollection.Definitions.MapInfoDefinition.MapInfo;
            string startMapName = mapInfo.Episodes[0].StartMap;
            return mapInfo.GetMap(startMapName);
        }

        private void SetSkill(int value)
        {
            if (value > 0 && value < 6)
                m_config.Game.Skill = (Maps.Shared.SkillLevel)value;
            else
                Log.Info($"Invalid skill level: {value}");
        }

        private void Loadmap(string mapName)
        {
            m_console.AddInput($"map {mapName}\n");

            // If the map is corrupt, go to the console.
            if (!m_layerManager.Contains(typeof(WorldLayer)))
            {
                ConsoleLayer consoleLayer = new(m_archiveCollection, m_console);
                m_layerManager.Add(consoleLayer);
            }
        }
    }
}
