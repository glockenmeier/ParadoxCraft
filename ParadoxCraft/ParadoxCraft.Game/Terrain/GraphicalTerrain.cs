﻿using ParadoxCraft.Blocks;
using ParadoxCraft.Helpers;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buffer = SiliconStudio.Paradox.Graphics.Buffer;

namespace ParadoxCraft.Terrain
{
    /// <summary>
    /// Terrain entity helper
    /// </summary>
    public class GraphicalTerrain
    {
        /// <summary>
        /// GraphicsDevice of the current game
        /// </summary>
        private GraphicsDevice GraphicsDevice { get; set; }

        /// <summary>
        /// Resulting Entity for this instance
        /// </summary>
        private Entity TerrainEntity { get; set; }

        /// <summary>
        /// Mesh buffer
        /// </summary>
        private List<TerrainMesh> Meshes { get; set; }

        private byte LastMeshIndex = 0;

        /// <summary>
        /// Creates a new instance of <see cref="GraphicalTerrain"/>
        /// </summary>
        /// <param name="device">GraphicsDevice of the current game to draw on</param>
        /// <param name="material">Terrain material</param>
        public GraphicalTerrain(GraphicsDevice device, Material material)
        {
            // Initilize variables
            GraphicsDevice = device;
            Meshes = TerrainMesh.New(device, 16).ToList();

            // Create the terrain entity
            var model = new Model();            
            foreach (Mesh mesh in Meshes)
            {
                mesh.Material = material;
                model.Add(mesh);
            }
            TerrainEntity = new Entity();
            TerrainEntity.Add(ModelComponent.Key, new ModelComponent { Model = model });
        }

        /// <summary>
        /// Adds a block to be drawn
        /// </summary>
        /// <remarks>
        /// Threadsafe
        /// </remarks>
        public void AddBlocks(Point3 chunkPos, List<GraphicalBlock> toAdd)
        {
            Meshes
                .Aggregate((left, right) => left.BlockCount < right.BlockCount ? left : right)
                .AddBlocks(chunkPos, toAdd);
        }

        /// <summary>
        /// Removes a chunk from the draw queue
        /// </summary>
        public IEnumerable<Point3> PurgeChunks(Point3<double> position, int radius)
        {
            foreach (TerrainMesh mesh in Meshes)
                foreach (Point3 pos in mesh.TryPurgeChunks(position, radius))
                    yield return pos;
        }

        /// <summary>
        /// Builds the meshes
        /// </summary>
        public void Build()
        {
            Meshes[LastMeshIndex++].Build();
            if (LastMeshIndex >= Meshes.Count)
                LastMeshIndex = 0;
        }

        /// <summary>
        /// Cast operator so we can add this instance 'as entity' to the pipeline
        /// </summary>
        public static implicit operator Entity(GraphicalTerrain terrain)
        {
            return terrain.TerrainEntity;
        }
    }
}
