﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PacmanWars
{
    public struct ControlSchema
    {
        public Keys MoveUp;
        public Keys MoveDown;
        public Keys MoveRight;
        public Keys MoveLeft;
    }

    public class Player : DrawableGameComponent
    {
        private enum Direction
        {
            Up, Down, Right, Left
        }

        private Game1 _game;
        private SpriteBatch _batch;

        private Texture2D _spriteSheet;
        private Point _origin;
        private Point _position;
        private Point _targetPosition;
        private ControlSchema _controls;
        private Direction _direction = Direction.Up;
        private int _score = 0;

        public Player(Game1 game, Point position, ControlSchema controls) : base(game)
        {
            _game = game;
            _batch = game.SpriteBatch;

            _spriteSheet = game.SpriteSheet;
            _origin = _position = _targetPosition = position.Multiply(Game1.TileSize);
            _controls = controls;
        }

        /// <summary>
        /// Get player's position in Point.
        /// </summary>
        public Point Position => _position;

        /// <summary>
        /// Get player's position in Vector2.
        /// </summary>
        public Vector2 PositionVec => _position.ToVector2();

        /// <summary>
        /// Get player's score.
        /// </summary>
        public int Score => _score;

        public override void Update(GameTime gameTime)
        {
            if (_position == _targetPosition)
            {
                HandleInput();
            }
            else
            {
                // Move player
                Vector2 vec = _targetPosition.ToVector2() - _position.ToVector2();
                vec.Normalize();

                _position = (_position.ToVector2() + vec).ToPoint();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _batch.Begin();

            _batch.Draw(
                texture: _spriteSheet,
                destinationRectangle: new Rectangle(_position, new Point(Game1.TileSize)),
                sourceRectangle: new Rectangle(new Point(0), new Point(16)),
                color: Color.White
            );

            _batch.End();
        }

        /// <summary>
        /// Add points to player score.
        /// </summary>
        /// <param name="points">Amount of points to add to the player.</param>
        public void AddPoints(int points)
        {
            _score += points;
        }

        private void HandleInput()
        {
            KeyboardState state = Keyboard.GetState();
            bool wasKeyPressed = false;

            if (state.IsKeyDown(_controls.MoveUp))
            {
                wasKeyPressed = true;
                _direction = Direction.Up;
            }

            if (state.IsKeyDown(_controls.MoveDown))
            {
                wasKeyPressed = true;
                _direction = Direction.Down;
            }

            if (state.IsKeyDown(_controls.MoveRight))
            {
                wasKeyPressed = true;
                _direction = Direction.Right;
            }

            if (state.IsKeyDown(_controls.MoveLeft))
            {
                wasKeyPressed = true;
                _direction = Direction.Left;
            }

            if (wasKeyPressed)
            {
                switch (_direction)
                {
                    case Direction.Up:
                        _targetPosition.Y -= Game1.TileSize;
                        break;
                    case Direction.Down:
                        _targetPosition.Y += Game1.TileSize;
                        break;
                    case Direction.Right:
                        _targetPosition.X += Game1.TileSize;
                        break;
                    case Direction.Left:
                        _targetPosition.X -= Game1.TileSize;
                        break;
                }
                
                if (_game.Board[_targetPosition.X / Game1.TileSize, _targetPosition.Y / Game1.TileSize] != ' ')
                    _targetPosition = _position;
            }
        }
    }
}