using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TextureBg
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        BasicEffect effect;
        Camera camera;
        Cub sky;
        Cub cubs;
        Tiles road;
        Model terrain;
        Model temple;
        Model ship;
        Vector3 velocity;
        Vector3 actualPosition;
        Vector3 lastPosition;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            //this.graphics.IsFullScreen = true;
            //graphics.ApplyChanges();

            spriteBatch = new SpriteBatch(GraphicsDevice);
            camera = new Camera(this, new Vector3(100, 40, 100), Vector3.Zero, 10f, 30f);
            Components.Add(camera);

            //camera.SetMaxValues(400, 400, 400);
            //camera.SetMinValues(2, 2, 2);
            actualPosition = new Vector3(0, 20, 0);

            terrain = Content.Load<Model>("Models/Terrain/OUT");
            temple = Content.Load<Model>("Models/Terrain/temple1");
            ship = Content.Load<Model>("Models/Terrain/temple1");

            road = new Tiles(this, Content.Load<Texture2D>("Textures/gras"), new Vector3(0, -2.01f, 0));
            road.SetOffsets(400, 1, 400, 2, 400);
            cubs = new Cub(this, new Vector3(30, 0, 30), 5, Content.Load<Texture2D>("Textures/crate"));
            sky = new Cub(this, Vector3.Down * 2, 400, Content.Load<Texture2D>("Textures/sky2"));

            effect = new BasicEffect(GraphicsDevice);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            velocity = Vector3.Up;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            //
            //cubs.Draw(camera, effect);
            
            
            //DrawModel(temple, new Vector3(50, 26, 50), (float)Math.PI / 2 * 3, (float)Math.PI / 2 * 3);
            //DrawModel(terrain, new Vector3(0, 0, 0), (float)Math.PI / 2 * 3, 0);

            MoveForward();

            foreach(ModelMesh mesh1 in temple.Meshes)
            {
                BoundingSphere sphere1 = mesh1.BoundingSphere;
                sphere1.Center += Vector3.Zero;
                foreach(ModelMesh mesh2 in ship.Meshes)
                {
                    BoundingSphere sphere2 = mesh2.BoundingSphere;
                    sphere2.Center += actualPosition + velocity;
                    if((mesh1.BoundingSphere).Intersects(mesh2.BoundingSphere))
                    {
                        ReverseVelocity();
                        Backup();
                        ReverseVelocity();
                    }
                }
            }
            
           road.Draw(camera, effect);
            DrawModel(ship, actualPosition, camera.Rotation.Y, 0);


            spriteBatch.Begin();
            spriteBatch.DrawString(Content.Load<SpriteFont>("Font"),
                camera.View.M11.ToString() + "  " +
                camera.View.M12.ToString() + "  " +
                camera.View.M13.ToString() + "  " +
                camera.View.M14.ToString() + Environment.NewLine +
                camera.View.M21.ToString() + "  " +
                camera.View.M22.ToString() + "  " +
                camera.View.M23.ToString() + "  " +
                camera.View.M24.ToString() + Environment.NewLine +
                camera.View.M31.ToString() + "  " +
                camera.View.M32.ToString() + "  " +
                camera.View.M33.ToString() + "  " +
                camera.View.M34.ToString() + Environment.NewLine +
                camera.View.M41.ToString() + "  " +
                camera.View.M42.ToString() + "  " +
                camera.View.M43.ToString() + "  " +
                camera.View.M44.ToString() 
                , new Vector2(20, 20), Color.White);
            spriteBatch.End();


            base.Draw(gameTime);
        }

        public void MoveForward()
        {
            lastPosition = actualPosition;
            actualPosition += velocity;
        }
        public void Backup()
        {
            actualPosition -= velocity;
        }
        public void ReverseVelocity()
        {
            velocity.X = -velocity.X;
        }

        private void DrawModel(Model model, Vector3 position, float rotationY, float rotationX)
        {
            Matrix[] transformation = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transformation);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect efect in mesh.Effects)
                {
                    efect.EnableDefaultLighting();
                    efect.View = camera.View;
                    efect.Projection = camera.Projection;
                    efect.World = transformation[mesh.ParentBone.Index] * 
                        Matrix.CreateRotationY(rotationY) * 
                        Matrix.CreateRotationX(rotationX) * 
                        Matrix.CreateTranslation(position);
                }
                mesh.Draw();
            }
        }

        
    }
}
