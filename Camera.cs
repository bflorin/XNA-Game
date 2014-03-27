using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
class Camera : GameComponent
{
    //Attributes
    private Vector3 cameraPosition;
    private Vector3 cameraRotation;
    private float cameraSpeed;
    public Vector3 cameraLookAt;
    private Vector3 mouseRotationBuffer;
    private MouseState currentMouseState;
    private MouseState prevMouseState;
    private float gravity;
    private float down;
    private int maxX, maxY, maxZ;
    private int minX, minY, minZ;
    //Properties

    public Vector3 Position
    {
        get { return cameraPosition; }
        set
        {
            cameraPosition = value;
            UpdateLookAt();
        }
    }

    public Vector3 Rotation
    {
        get { return cameraRotation; }
        set
        {
            cameraRotation = value;
            UpdateLookAt();
        }
    }

    public Matrix Projection
    {
        get;
        protected set;
    }

    public Matrix View
    {
        get
        {
            return Matrix.CreateLookAt(cameraPosition, cameraLookAt, Vector3.Up);
        }
    }

    //Constructor
    public Camera(Game game, Vector3 position, Vector3 rotation, float speed, float Gravity)
        : base(game)
    {
        cameraSpeed = speed;
        //Setup projection matrix
        Projection = Matrix.CreatePerspectiveFieldOfView(
            MathHelper.PiOver4,
            Game.GraphicsDevice.Viewport.AspectRatio,
            0.1f,
            1000.0f);

        SetMaxValues(int.MaxValue, int.MaxValue, int.MaxValue);
        SetMinValues(int.MinValue, int.MinValue, int.MinValue);

        //Set camera position and rotation
        MoveTo(position, rotation);

        down = 0;
        gravity = Gravity;
        prevMouseState = Mouse.GetState();
    }

    //Set camera's position and rotation
    private void MoveTo(Vector3 pos, Vector3 rot)
    {
        Position = pos;
        Rotation = rot;
    }


    public void SetMinValues(int x, int y, int z)
    {
        minX = x;
        minY = y;
        minZ = z;
    }

    public void SetMaxValues(int x, int y, int z)
    {
        maxX = x;
        maxY = y;
        maxZ = z;
    }

    //update the look at vector
    private void UpdateLookAt()
    {
        //Build a rotation matrix
        Matrix rotationMatrix = Matrix.CreateRotationX(cameraRotation.X) * Matrix.CreateRotationY(cameraRotation.Y);
        //Build look at offset vector
        Vector3 lookAtOffset = Vector3.Transform(Vector3.UnitZ, rotationMatrix);
        //Update our camera's look at vector
        cameraLookAt = cameraPosition + lookAtOffset;
    }

    //Method that simulates movement
    private Vector3 PreviewMove(Vector3 amount)
    {
        //Create a rotate matrix
        Matrix rotate = Matrix.CreateRotationY(cameraRotation.Y);
        //Create a movement vector
        Vector3 movement = Vector3.Transform(amount, rotate);
        //Return the value of camera position + movement vector
        return cameraPosition + movement;
    }

    //Method that actually moves the camera
    private void Move(Vector3 scale)
    {
        MoveTo(PreviewMove(scale), Rotation);
    }

    //update method
    public override void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        currentMouseState = Mouse.GetState();
        KeyboardState ks = Keyboard.GetState();

        //Handle basic key movement

        Vector3 moveVector = Vector3.Zero;

        if (ks.IsKeyDown(Keys.W))
            if ((Math.Cos(Rotation.Y) > 0 && Position.Z > maxZ) || (Math.Cos(Rotation.Y) < 0 && Position.Z < minZ) || (Math.Sin(Rotation.Y) > 0 && Position.X > maxX) || (Math.Sin(Rotation.Y) < 0 && Position.X < minX))
                moveVector.Z = 0;
            else moveVector.Z = 1;

        if (ks.IsKeyDown(Keys.S))
            if ((Math.Cos(Rotation.Y) < 0 && Position.Z > maxZ) || (Math.Cos(Rotation.Y) > 0 && Position.Z < minZ) || (Math.Sin(Rotation.Y) < 0 && Position.X > maxX) || (Math.Sin(Rotation.Y) > 0 && Position.X < minX))
                moveVector.Z = 0;
            else moveVector.Z = -1;

        if (ks.IsKeyDown(Keys.A))
            if ((Math.Cos(Rotation.Y) < 0 && Position.X < minX) || (Math.Cos(Rotation.Y) > 0 && Position.X > maxX) || (Math.Sin(Rotation.Y) < 0 && Position.Z > maxZ) || (Math.Sin(Rotation.Y) > 0 && Position.Z < minZ))
                moveVector.X = 0;
            else moveVector.X = 1;

        if (ks.IsKeyDown(Keys.D))
            if ((Math.Cos(Rotation.Y) > 0 && Position.X < minX) || (Math.Cos(Rotation.Y) < 0 && Position.X > maxX) || (Math.Sin(Rotation.Y) > 0 && Position.Z > maxZ) || (Math.Sin(Rotation.Y) < 0 && Position.Z < minZ))
                moveVector.X = 0;
            else moveVector.X = -1;
        
        if (ks.IsKeyDown(Keys.X))
            moveVector.Y = 1;
        if (ks.IsKeyDown(Keys.C))
            moveVector.Y = -1;

        if (ks.IsKeyDown(Keys.Space) && down == 0)
            down = gravity + 1;
        if (down > gravity / 2)
        {
            down -= 1f;
            moveVector.Y = 1;
        }
        if (down > 0 && down < gravity / 2 + 1)
        {
            down -= 1f;
            moveVector.Y = -1;
        }
        
        if (moveVector != Vector3.Zero)
        {
            //normalize that vector
            //so that we don't move faster diagonally
            moveVector.Normalize();
            //Now we add in smooth and speed


            if (ks.IsKeyDown(Keys.LeftShift))
                moveVector *= dt * cameraSpeed * 15;
            else moveVector *= dt * cameraSpeed;

            //Move camera
            Move(moveVector);
        }



        //Handle mouse movement

        float deltaX;
        float deltaY;

        if (currentMouseState != prevMouseState)
        {
            //Cache mouse location
            deltaX = currentMouseState.X - (Game.GraphicsDevice.Viewport.Width / 2);
            deltaY = currentMouseState.Y - (Game.GraphicsDevice.Viewport.Height / 2);

            //Calculate rotation from mouse movement
            mouseRotationBuffer.X -= 0.01f * deltaX * dt * 2;
            mouseRotationBuffer.Y -= 0.01f * deltaY * dt * 2;

            //Clamp the rotational movement
            if (mouseRotationBuffer.Y < MathHelper.ToRadians(-75.0f))
                mouseRotationBuffer.Y = mouseRotationBuffer.Y - (mouseRotationBuffer.Y - MathHelper.ToRadians(-75.0f));
            if (mouseRotationBuffer.Y > MathHelper.ToRadians(75.0f))
                mouseRotationBuffer.Y = mouseRotationBuffer.Y - (mouseRotationBuffer.Y - MathHelper.ToRadians(75.0f));

            //Finally add that rotation to our rotation vector clamping as needed
            Rotation = new Vector3(-MathHelper.Clamp(mouseRotationBuffer.Y,
                                MathHelper.ToRadians(-75.0f), MathHelper.ToRadians(75.0f)),
                                MathHelper.WrapAngle(mouseRotationBuffer.X), 0);

            //Reset our change in mouse position
            deltaX = 0;
            deltaY = 0;

        }

        //Set mouse cursor to center of screen
        Mouse.SetPosition(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2);

        //Set prev state to current state
        prevMouseState = currentMouseState;

        base.Update(gameTime);
    }

}
