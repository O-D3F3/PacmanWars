﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PacmanWars
{
    /// <summary>
    /// This represents a controls schema to be used by Player class when getting inputs from the user.
    /// </summary>
    public struct ControlSchema
    {
        public Keys MoveUp;
        public Keys MoveDown;
        public Keys MoveRight;
        public Keys MoveLeft;
    }

    /// <summary>
    /// This represents a player and it's responsible handle movement, lives and score.
    /// </summary>
    public class Player : DrawableGameComponent
    {
        /// <summary>
        /// This event is triggered when a player runs out of lives.
        /// </summary>
        public static event Action OnPlayerLose;

        private const float _speed = 2.0f;
        private const float _invincibleTimeAfterDie = 2.0f;
        private const int _extraLifeScore = 10000;

        private Game1 _game;
        private SpriteBatch _batch;

        private Texture2D _spriteSheet;
        private Point _origin;
        private Point _position;
        private Point _targetPosition;
        private Rectangle _area;
        private ControlSchema _controls;
        private Dictionary<Direction, Vector2> _spritePositions;
        private Direction _direction;
        private int _number;
        private int _frame = 0;
        private int _score = 0;
        private int _lives = 3;
        private bool _wasExtraLifeAwarded = false;
        private float _invincibleTimer = 0.0f;

        /// <summary>
        /// Creates an instance of Player.
        /// </summary>
        /// <param name="game">Reference to the game</param>
        /// <param name="position">Starting position of the player</param>
        /// <param name="controls">Control schema for this player</param>
        public Player(Game1 game, Point position, ControlSchema controls, int number, Direction startingDir = Direction.Up) : base(game)
        {
            DrawOrder = 2;

            _game = game;
            _batch = game.SpriteBatch;

            _spriteSheet = game.SpriteSheet;
            _origin = _position = _targetPosition = position.Multiply(Game1.TileSize);
            _controls = controls;
            _number = number;
            _direction = startingDir;

            _spritePositions = new Dictionary<Direction, Vector2>
            {
                [Direction.Up] = new Vector2(1, 2),
                [Direction.Down] = new Vector2(1, 3),
                [Direction.Right] = new Vector2(1, 0),
                [Direction.Left] = new Vector2(1, 1)
            };
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
        /// Get player's area rectangle.
        /// </summary>
        public Rectangle Area => _area;

        /// <summary>
        /// Get player's score.
        /// </summary>
        public int Score => _score;

        /// <summary>
        /// Get player's live amount including current.
        /// </summary>
        public int Lives => _lives;

        /// <summary>
        /// Get player's number.
        /// </summary>
        public int Number => _number;

        public override void Update(GameTime gameTime)
        {
            // Reduce invincible timer if needed
            if (_invincibleTimer > 0.0f)
                _invincibleTimer -= gameTime.DeltaTime();

            // Calculate player rectangle to be used bay enemies, pacdots and power pellets
            _area = new Rectangle(_position, new Point(Game1.TileSize));

            if (_position == _targetPosition)
            {
                HandleInput();
            }
            else
            {
                // Move player
                Vector2 vec = _targetPosition.ToVector2() - _position.ToVector2();
                vec.Normalize();

                _position = (_position.ToVector2() + (vec * _speed)).ToPoint();

                if ((_position.X + _position.Y) % 8 == 0)
                {
                    _frame++;
                    if (_frame > 1)
                        _frame = 0;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            _batch.Begin(samplerState: SamplerState.PointClamp);

            _batch.Draw(
                texture: _spriteSheet,
                destinationRectangle: new Rectangle(_position, new Point(Game1.TileSize)),
                sourceRectangle: new Rectangle(((_spritePositions[_direction] + Vector2.UnitX * -_frame) * 16).ToPoint(), (Vector2.One * 16).ToPoint()),
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

            // Player is awarded a single bonus life at 10000 points
            if (!_wasExtraLifeAwarded && _score >= _extraLifeScore)
            {
                _lives++;
                _wasExtraLifeAwarded = true;
            }
        }

        /// <summary>
        /// Kill player and decrement 1 life.
        /// Player will be automatically moved to his origin. 
        /// </summary>
        public void Die()
        {
            // Player can't die if is invincible
            if (_invincibleTimer > 0.0f) return;

            _lives--;

            _position = _targetPosition = _origin;
            _invincibleTimer = _invincibleTimeAfterDie;

            if (_lives == 0)
            {
                OnPlayerLose?.Invoke();
            }
        }

        /// <summary>
        /// Reset player position.
        /// </summary>
        public void ResetPosition()
        {
            _position = _targetPosition = _origin;
        }

        /// <summary>
        /// Handle player input.
        /// </summary>
        private void HandleInput()
        {
            KeyboardState state = Keyboard.GetState();
            bool wasKeyPressed = false;
            _frame = 0;

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

                if (_game.Board[_targetPosition.Divide(Game1.TileSize)] != ' ')
                    _targetPosition = _position;
            }
        }
    }
}