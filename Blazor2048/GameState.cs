using Blazor2048.Extensions;
using Blazored.LocalStorage;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor2048
{
    public class GameState
    {
        private readonly GameModel _model;

        public GameState(ISyncLocalStorageService localStorage)
        {
            _model = new GameModel(localStorage);
        }

        public async Task Move(GameMove move)
        {
            await _model.Move(move);
        }

        public void ResetGame()
        {
            _model.ResetBoard();
        }

        public Tile[] GetTiles() => _model.Tiles;
        public Tile[] GetPreMoveTiles() => _model.PreMoveTiles;
        public Tile[][] GetPreMoveRows() => _model.PreMoveTiles.Split(4).ToArray().Transpose();
        public Tile[] GetPostMoveTiles() => _model.PostMoveTiles;
        public Tile[][] GetPostMoveRows() => _model.PostMoveTiles.Split(4).ToArray().Transpose();
        public Tile[] GetPostGenerateTiles() => _model.PostGenerateTiles;
        public Tile[][] GetPostGenerateRows() => _model.PostGenerateTiles.Split(4).ToArray().Transpose();
        public Tile[][] GetRows() => _model.Rows;
        public Tile[][] GetColumns() => _model.Columns;
        public int GetScore() => _model.Score;
        public int GetHighScore() => _model.HighScore;
        public bool GetGameOverStatus() => _model.GameOver;
        public bool IsMoving() => _model.IsMoving;
    }

    public enum GameMove
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
    }
}
