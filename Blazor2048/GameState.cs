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

        public void Move(GameMove move)
        {
            Model.Move(move);
        }

        public void ResetGame()
        {
            Model.ResetBoard();
        }

        public Tile[][] GetRows() => Model.Rows;
        public Tile[][] GetColumns() => Model.Columns;
        public int GetScore() => Model.Score;
        public int GetHighScore() => Model.HighScore;
        public bool GetGameOverStatus() => Model.GameOver;
    }

    public enum GameMove
    {
        UP,
        RIGHT,
        DOWN,
        LEFT
    }
}
