using Blazor2048.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor2048
{
    public class GameState
    {
        public GameModel Model { get; set; } = new GameModel();

        public GameState()
        {
        }

        public async Task Move(GameMove move)
        {
            await Model.Move(move);
        }

        public void ResetGame()
        {
            Model.ResetBoard();
        }

        public Tile[] GetTiles() => Model.Tiles;
        public Tile[] GetPreMoveTiles() => Model.PreMoveTiles;
        public Tile[][] GetPreMoveRows() => Model.PreMoveTiles.Split(4).ToArray();
        public Tile[] GetPostMoveTiles() => Model.PostMoveTiles;
        public Tile[][] GetPostMoveRows() => Model.PostMoveTiles.Split(4).ToArray();
        public Tile[] GetPostGenerateTiles() => Model.PostGenerateTiles;
        public Tile[][] GetPostGenerateRows() => Model.PostGenerateTiles.Split(4).ToArray();
        public Tile[][] GetRows() => Model.Rows;
        public Tile[][] GetColumns() => Model.Columns;
        public int GetScore() => Model.Score;
        public int GetHighScore() => Model.HighScore;
        public bool GetGameOverStatus() => Model.GameOver;
        public bool NewTileFlip() => Model.NewTileFlip;
        public bool IsMoving() => Model.IsMoving;
    }

    public enum GameMove
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
    }
}
