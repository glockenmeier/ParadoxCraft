using ParadoxCraft.Terrain;
using ParadoxCraft.Blocks;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Engine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Paradox.DataModel;
using ParadoxCraft.Blocks.Chunks;

namespace ParadoxCraft
{
    /// <summary>
    /// Main game instance
    /// </summary>
    public class ParadoxCraftGame : Game
    {
        #region Variables
        /// <summary>
        /// Bool to switch wireframe mode
        /// </summary>
        private bool isWireframe = false;

        /// <summary>
        /// Player camera entity
        /// </summary>
        private Entity Camera { get; set; }

        /// <summary>
        /// Terrain entity
        /// </summary>
        private GraphicalTerrain Terrain { get; set; }

        /// <summary>
        /// Player movement helper
        /// </summary>
        private Movement PlayerMovement { get; set; }

        /// <summary>
        /// Chunk handling factory
        /// </summary>
        public ChunkFactory Factory { get; private set; }
        #endregion

        #region Initialization
        /// <summary>
        /// Creates a new instance of <see cref="ParadoxCraftGame"/>
        /// </summary>
        public ParadoxCraftGame()
        {
            // Target 11.0 profile by default
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_11_0 };

            Factory = new ChunkFactory();
        }

        /// <summary>
        /// Loads the game content
        /// </summary>
        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // Create pipeline
            RenderPipelineLightingFactory.CreateDefaultForward(this, "ParadoxCraftEffectMain", Color.DarkBlue, false, false);

            // Wireframe mode
            if (isWireframe)
                GraphicsDevice.Parameters.Set(Effect.RasterizerStateKey, RasterizerState.New(GraphicsDevice, new RasterizerStateDescription(CullMode.None) { FillMode = FillMode.Wireframe }));

            // Lights
            Entities.Add(CreateDirectLight(new Vector3(-1, 1, 1), new Color3(1, 1, 1), 0.25f));
            Entities.Add(CreateDirectLight(new Vector3(1, -1, -1), new Color3(1, 1, 1), 0.25f));

            // Entities
            LoadTerrain();
            LoadCamera();

            // Scripts
            Script.Add(MovementScript);
            Script.Add(RenderChunksScript);
        }

        /// <summary>
        /// Creates the entity for the player camera
        /// </summary>
        private void LoadCamera()
        {
            // Initialize player (camera)
            var campos = new Vector3(0, 0, 0);
            Camera = new Entity(campos)
            {
                new CameraComponent(null, .1f, 18 * 2 * 16) {
                    VerticalFieldOfView = 1.5f,
                    TargetUp = Vector3.UnitY
                },
            };
            PlayerMovement = new Movement(Camera);
            RenderSystem.Pipeline.SetCamera(Camera.Get<CameraComponent>());
            Entities.Add(Camera);
        }

        /// <summary>
        /// Creates the entity for the terrain
        /// </summary>
        private void LoadTerrain()
        {
            Terrain = new GraphicalTerrain(GraphicsDevice, Asset.Load<Material>("Materials/Terrain"));
            Terrain.Build();
            Factory.Terrain = Terrain;
            Entities.Add(Terrain);
        } 
        #endregion

        #region Scripts
        /// <summary>
        /// Script for tracking key inputs to move the player
        /// </summary>
        private async Task MovementScript()
        {
            float dragX = 0f,
                  dragY = 0f;
            double time = UpdateTime.Total.TotalSeconds;
            while (IsRunning)
            {
                await Script.NextFrame();

                //Else the player slows down on a lower framerate
                var diff = UpdateTime.Total.TotalSeconds - time;
                time = UpdateTime.Total.TotalSeconds;

                if (Input.IsKeyDown(Keys.W))
                    PlayerMovement.Add(Direction.Foward);

                if (Input.IsKeyDown(Keys.S))
                    PlayerMovement.Add(Direction.Back);

                if (Input.IsKeyDown(Keys.A))
                    PlayerMovement.Add(Direction.Left);

                if (Input.IsKeyDown(Keys.D))
                    PlayerMovement.Add(Direction.Right);

                if (Input.IsKeyDown(Keys.Q))
                    PlayerMovement.Add(Direction.Up);

                if (Input.IsKeyDown(Keys.E))
                    PlayerMovement.Add(Direction.Down);

                PlayerMovement.Move(diff);

                //Move the 'camera'
                dragX = 0.95f * dragX;
                dragY = 0.95f * dragY;
                if (Input.PointerEvents.Count > 0)
                {
                    dragX = Input.PointerEvents.Sum(x => x.DeltaPosition.X);
                    dragY = Input.PointerEvents.Sum(x => x.DeltaPosition.Y);
                    PlayerMovement.YawPitch(dragX, dragY);
                }
            }
        }

        /// <summary>
        /// Script for checking chunks that should be rendered
        /// </summary>
        private async Task RenderChunksScript()
        {
            while (IsRunning)
            {
                await Script.NextFrame();
                
                double playerX = PlayerMovement.X;
                double playerZ = PlayerMovement.Z;
                //playerX += StartPosition.X;
                //playerZ += StartPosition.Z;
                playerX /= Constants.ChunkSize;
                playerZ /= Constants.ChunkSize;

                byte radius = 2;
                for (int i = 0; i < radius * radius; i++)
                {
                    int x = i % radius;
                    int z = (i - x) / radius;

                    if ((x * x) + (z * z) > (radius * radius)) continue;

                    Factory.CheckLoad(playerX + x, 0, playerZ + z);
                    Factory.CheckLoad(playerX - x, 0, playerZ + z);
                    Factory.CheckLoad(playerX + x, 0, playerZ - z);
                    Factory.CheckLoad(playerX - x, 0, playerZ - z);

                    if (i % radius == 0)
                        await Task.Delay(100);
                }
                //Factory.PurgeDistancedChunks(playerX, playerZ, radius + 1);
            }
        }
        #endregion

        #region Lights
        /// <summary>
        /// Creates a light entity based on the forward lightning example
        /// </summary>
        /// <param name="direction">Light direction</param>
        /// <param name="color">Light color</param>
        /// <param name="intensity">Light intensity</param>
        /// <returns>The light entity</returns>
        private static Entity CreateDirectLight(Vector3 direction, Color3 color, float intensity)
        {
            return new Entity()
            {
                new LightComponent
                {
                    Type = LightType.Directional,
                    Color = color,
                    Deferred = false,
                    Enabled = true,
                    Intensity = intensity,
                    LightDirection = direction,
                    Layers = RenderLayers.RenderLayerAll,
                    ShadowMap = false,
                    ShadowFarDistance = 3000,
                    ShadowNearDistance = 10,
                    ShadowMapMaxSize = 1024,
                    ShadowMapMinSize = 512,
                    ShadowMapCascadeCount = 4,
                    ShadowMapFilterType = ShadowMapFilterType.Variance,
                    BleedingFactor = 0,
                    MinVariance = 0
                }
            };
        } 
        #endregion
    }
}