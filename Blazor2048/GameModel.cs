using Blazor2048.Extensions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Blazor2048
{
    public class GameModel
    {
        public Tile[] Tiles { get; } = new Tile[BOARD_HEIGHT * BOARD_WIDTH];
        public Tile[][] Rows { get; } = new Tile[BOARD_HEIGHT][];
        public Tile[][] Columns { get; } = new Tile[BOARD_WIDTH][];
        public int Score { get; private set; } = 0;
        public int HighScore { get; private set; } = 0;
        public bool GameOver { get; private set; } = false;
        private readonly Random r = new Random();

        private const int BOARD_WIDTH = 4;
        private const int BOARD_HEIGHT = 4;

        public GameModel()
        {
            for (var y = 0; y < BOARD_HEIGHT; y++)
            {
                Rows[y] = new Tile[BOARD_WIDTH];
            }

            for (var x = 0; x < BOARD_WIDTH; x++)
            {
                Columns[x] = new Tile[BOARD_HEIGHT];
            }

            var counter = 0;
            for (var x = 0; x < BOARD_WIDTH; x++)
            {
                for (var y = 0; y < BOARD_HEIGHT; y++)
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
            GameOver = false;

            for (var i = 0; i < 2; i++)
            {
                GenerateNewTile();
            }
        }

        private bool HasLost()
            => Tiles.ToList().All(t => t.Value > 0) && !(Rows.Any(t => t.HasConsecutiveDuplicates()) || Columns.Any(t => t.HasConsecutiveDuplicates()));


        private void CalcScore()
        {
            Score = Tiles.Sum(t => t.Value);
            if (Score > HighScore)
                HighScore = Score;
        }

        private void GenerateNewTile()
        {
            List<Tile> emptyTiles = Tiles.ToList().Where(t => t.Value == 0).ToList();
            int index = r.Next(emptyTiles.Count);

            emptyTiles[index].Value = GenerateNewTileValue();
            CalcScore();
        }

        private int GenerateNewTileValue()
        {
            return r.Next(0, 100) <= 89 ? 2 : 4;
        }

        public void Move(GameMove move)
        {
            if (GameOver)
                return;

            // Was anything moved from this input?
            bool HasMoved = move switch
            {
                GameMove.UP => Columns.Aggregate(false, (acc, column) => acc | MoveLine(column)),
                GameMove.RIGHT => Rows.Aggregate(false, (acc, row) => acc | MoveLine(row.Reversed())),
                GameMove.DOWN => Columns.Aggregate(false, (acc, column) => acc | MoveLine(column.Reversed())),
                GameMove.LEFT => Rows.Aggregate(false, (acc, row) => acc | MoveLine(row)),
                _ => throw new NotSupportedException("Move not supported.")
            };

            if (HasMoved)
            {
                GenerateNewTile();
                GameOver = HasLost();
            }
        }

        private static bool MoveLine(Tile[] tiles)
        {
            bool hasMoved = false;
            for (int x1 = 0; x1 < tiles.Length - 1; x1++)
            {
                for (int x2 = x1 + 1; x2 < tiles.Length; x2++)
                {
                    // Can't move.
                    if (tiles[x1].Value != 0 && tiles[x2].Value != 0 && tiles[x1].Value != tiles[x2].Value)
                    {
                        break;
                    }

                    // Can move but not merge.
                    if (tiles[x1].Value == 0 && tiles[x2].Value != 0)
                    {
                        var value = tiles[x2].Value;
                        tiles[x2].Value = 0;
                        tiles[x1].Value = value;
                        hasMoved = true;
                        continue;
                    }

                    // Can move and merge.
                    if (tiles[x1].Value == tiles[x2].Value && tiles[x1].Value != 0)
                    {
                        var value = tiles[x1].Value + tiles[x2].Value;
                        tiles[x2].Value = 0;
                        tiles[x1].Value = value;
                        hasMoved = true;
                        break;
                    }
                }
            }

            return hasMoved;
        }
    }
}
