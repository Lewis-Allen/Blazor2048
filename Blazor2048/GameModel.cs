using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Blazor2048
{
    public class GameModel
    {
        public Tile[] Tiles { get; } = new Tile[16];
        public Tile[][] Rows { get; } = new Tile[4][];
        public Tile[][] Columns { get; } = new Tile[4][];
        public int Score { get; private set; } = 0;
        public bool GameOver { get; private set; } = false;

        private readonly Random r = new Random();

        public GameModel()
        {
            for (var i = 0; i < 4; i++)
            {
                Rows[i] = new Tile[4];
                Columns[i] = new Tile[4];
            }

            var counter = 0;
            for (var x = 0; x < 4; x++)
            {
                for (var y = 0; y < 4; y++)
                {
                    var tile = new Tile(0);

                    Tiles[counter] = tile;
                    Rows[y][x] = tile;
                    Columns[x][y] = tile;

                    counter++;
                }
            }

            ResetBoard();
        }

        

        public void ResetBoard()
        {
            Tiles.ToList().ForEach(t => t.Value = 0);
            Score = 0;

            for(var i = 0; i < 2; i++)
            {
                GenerateNewTile();
            }

        }

        public void SetValue(int value, int x, int y)
        {
            int oldValue = Rows[x][y].Value;
            Rows[x][y].Value = value;
            Score += value - oldValue;
        }

        public void Move(GameMove move)
        {
            bool HasMoved = false;

            switch(move)
            {
                case GameMove.UP:
                    break;

                case GameMove.RIGHT:
                    break;

                case GameMove.DOWN:
                    break;

                case GameMove.LEFT:
                    break;
            }

            if(HasMoved)
            {
                GenerateNewTile();
                GameOver = HasLost();
            }
        }

        private bool HasLost()
        {
            return !(Tiles.ToList().Any(t => t.Value == 0));
        }

        private void GenerateNewTile()
        {
            List<Tile> emptyTiles = Tiles.ToList().Where(t => t.Value == 0).ToList();
            int index = r.Next(emptyTiles.Count);

            emptyTiles[index].Value = GenerateNewTileValue();
        }

        private int GenerateNewTileValue()
        {
            return r.Next(0, 100) <= 89 ? 2 : 4;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
