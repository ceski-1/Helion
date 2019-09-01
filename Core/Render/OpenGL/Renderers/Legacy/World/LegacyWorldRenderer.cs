using System;
using Helion.Render.OpenGL.Context;
using Helion.Render.OpenGL.Context.Types;
using Helion.Render.OpenGL.Renderers.Legacy.World.Data;
using Helion.Render.OpenGL.Renderers.Legacy.World.Entities;
using Helion.Render.OpenGL.Renderers.Legacy.World.Geometry;
using Helion.Render.OpenGL.Shader;
using Helion.Render.OpenGL.Texture.Legacy;
using Helion.Render.OpenGL.Vertex;
using Helion.Render.OpenGL.Vertex.Attribute;
using Helion.Render.Shared;
using Helion.Render.Shared.World.ViewClipping;
using Helion.Resources.Archives.Collection;
using Helion.Util;
using Helion.Util.Configuration;
using Helion.Util.Geometry;
using Helion.World;
using Helion.World.Bsp;
using static Helion.Util.Assertion.Assert;

namespace Helion.Render.OpenGL.Renderers.Legacy.World
{
    public class LegacyWorldRenderer : WorldRenderer
    {
        public static readonly VertexArrayAttributes Attributes = new VertexArrayAttributes(
            new VertexPointerFloatAttribute("pos", 0, 3),
            new VertexPointerFloatAttribute("uv", 1, 2),
            new VertexPointerFloatAttribute("lightLevel", 2, 1));
        
        private readonly Config m_config;
        private readonly IGLFunctions gl;
        private readonly GeometryRenderer m_geometryRenderer;
        private readonly EntityRenderer m_entityRenderer;
        private readonly LegacyShader m_shaderProgram;
        private readonly RenderWorldDataManager m_worldDataManager;
        private readonly ViewClipper m_viewClipper = new ViewClipper();

        public LegacyWorldRenderer(Config config, ArchiveCollection archiveCollection, GLCapabilities capabilities, 
            IGLFunctions functions, LegacyGLTextureManager textureManager)
        {
            m_config = config;
            gl = functions;
            m_worldDataManager = new RenderWorldDataManager(capabilities, gl);
            m_entityRenderer = new EntityRenderer(textureManager);
            m_geometryRenderer = new GeometryRenderer(config, archiveCollection, capabilities, functions, 
                textureManager, m_viewClipper, m_worldDataManager);

            using (ShaderBuilder shaderBuilder = LegacyShader.MakeBuilder(functions))
                m_shaderProgram = new LegacyShader(functions, shaderBuilder, Attributes);
        }

        ~LegacyWorldRenderer()
        {
            Fail($"Did not dispose of {GetType().FullName}, finalizer run when it should not be");
            ReleaseUnmanagedResources();
        }

        public override void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        protected override void UpdateToNewWorld(WorldBase world)
        {
            m_geometryRenderer.UpdateTo(world);
            m_entityRenderer.UpdateTo(world);
        }

        protected override void PerformRender(WorldBase world, RenderInfo renderInfo)
        {
            Clear(world, renderInfo);
            
            TraverseBsp(world, renderInfo);
            
            RenderWorldData(renderInfo);
            m_geometryRenderer.Render(renderInfo);
        }

        private void Clear(WorldBase world, RenderInfo renderInfo)
        {
            m_viewClipper.Clear();
            m_worldDataManager.Clear();
            
            m_geometryRenderer.Clear(renderInfo.TickFraction);
            m_entityRenderer.Clear(world, renderInfo.TickFraction);
        }
        
        private void TraverseBsp(WorldBase world, RenderInfo renderInfo)
        {
            Vec2D position = renderInfo.Camera.Position.To2D().ToDouble();
            
            m_viewClipper.Center = position;
            RecursivelyRenderBsp(world.BspTree.Root, position, world);
        }

        private bool Occluded(in BspNodeCompact node, in Vec2D position)
        {
            if (node.BoundingBox.Contains(position))
                return false;
            
            (Vec2D first, Vec2D second) = node.BoundingBox.GetSpanningEdge(position);
            return m_viewClipper.InsideAnyRange(first, second);
        }

        private void RecursivelyRenderBsp(BspNodeCompact node, Vec2D position, WorldBase world)
        {
            if (Occluded(node, position))
                return;
            
            // TODO: Consider changing to xor trick to avoid branching?
            if (node.Splitter.OnRight(position))
            {
                if (node.IsRightSubsector)
                    RenderSubsector(world.BspTree.Subsectors[node.RightChildAsSubsector], position);
                else
                    RecursivelyRenderBsp(world.BspTree.Nodes[node.RightChild], position, world);
                
                if (node.IsLeftSubsector)
                    RenderSubsector(world.BspTree.Subsectors[node.LeftChildAsSubsector], position);
                else
                    RecursivelyRenderBsp(world.BspTree.Nodes[node.LeftChild], position, world);
            }
            else
            {
                if (node.IsLeftSubsector)
                    RenderSubsector(world.BspTree.Subsectors[node.LeftChildAsSubsector], position);
                else
                    RecursivelyRenderBsp(world.BspTree.Nodes[node.LeftChild], position, world);
                
                if (node.IsRightSubsector)
                    RenderSubsector(world.BspTree.Subsectors[node.RightChildAsSubsector], position);
                else
                    RecursivelyRenderBsp(world.BspTree.Nodes[node.RightChild], position, world);
            }
        }

        private void RenderSubsector(Subsector subsector, Vec2D position)
        {
            m_geometryRenderer.RenderSubsector(subsector, position);
            m_entityRenderer.RenderSubsector(subsector, position);
        }
        
        private void SetUniforms(RenderInfo renderInfo)
        {
            float fovX = (float)MathHelper.ToRadians(m_config.Engine.Render.FieldOfView);
            
            m_shaderProgram.BoundTexture.Set(gl, 0);
            m_shaderProgram.Mvp.Set(gl, GLRenderer.CalculateMvpMatrix(renderInfo, fovX));
        }
        
        private void RenderWorldData(RenderInfo renderInfo)
        {
            m_shaderProgram.Bind();
            
            SetUniforms(renderInfo);
            gl.ActiveTexture(TextureUnitType.Zero);
            m_worldDataManager.Draw();

            m_shaderProgram.Unbind();
        }
        
        private void ReleaseUnmanagedResources()
        {
            m_shaderProgram.Dispose();
            m_geometryRenderer.Dispose();
            m_worldDataManager.Dispose();
        }
    }
}