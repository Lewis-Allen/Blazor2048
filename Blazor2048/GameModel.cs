﻿using System;
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
        public Tile[] Tiles { get; } = new Tile[16];
        public Tile[][] Rows { get; } = new Tile[4][];
        public Tile[][] Columns { get; } = new Tile[4][];
        public int Score { get; private set; } = 0;
        public int HighScore { get; private set; } = 0;
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
            GameOver = false;

            for (var i = 0; i < 2; i++)
            {
                GenerateNewTile();
            }
        }

        public void Move(GameMove move)
        {
            if (GameOver)
                return;

            // Was anything moved from this input?
            bool HasMoved = false;
            
            switch (move)
            {
                case GameMove.UP:
                    foreach (Tile[] column in Columns)
                    {
                        for (int x1 = 0; x1 < column.Length - 1; x1++)
                        {
                            for (int x2 = x1 + 1; x2 < column.Length; x2++)
                            {
                                bool stop = false;
                                MoveHelper(column, x1, x2, ref HasMoved, ref stop);

                                if (stop)
                                    break;
                            }
                        }
                    }
                    break;

                case GameMove.RIGHT:
                    foreach (Tile[] row in Rows)
                    {
                        for (int x1 = Rows.Length - 1; x1 > 0; x1--)
                        {
                            for (int x2 = x1 - 1; x2 >= 0; x2--)
                            {
                                bool stop = false;
                                MoveHelper(row, x1, x2, ref HasMoved, ref stop);

                                if (stop)
                                    break;
                            }
                        }
                    }

                    break;

                case GameMove.DOWN:
                    foreach (Tile[] column in Columns)
                    {
                        for (int x1 = Columns.Length - 1; x1 > 0; x1--)
                        {
                            for (int x2 = x1 - 1; x2 >= 0; x2--)
                            {
                                bool stop = false;
                                MoveHelper(column, x1, x2, ref HasMoved, ref stop);

                                if (stop)
                                    break;
                            }
                        }
                    }

                    break;

                case GameMove.LEFT:
                    foreach (Tile[] row in Rows)
                    {
                        for (var x1 = 0; x1 < row.Length; x1++)
                        {
                            for (var x2 = x1 + 1; x2 < row.Length; x2++)
                            {
                                bool stop = false;
                                MoveHelper(row, x1, x2, ref HasMoved, ref stop);

                                if (stop)
                                    break;
                            }
                        }
                    }

                    break;
            }

            if (HasMoved)
            {
                GenerateNewTile();
                GameOver = HasLost();
            }
        }

        private static void MoveHelper(Tile[] row, int x1, int x2, ref bool moved, ref bool stop)
        {
            // Can't move.
            if(row[x1].Value != 0 && row[x2].Value != 0 && row[x1].Value != row[x2].Value)
            {
                stop = true;
                return;
            }

            // Can move but not merge.
            if(row[x1].Value == 0 && row[x2].Value != 0)
            {
                var value = row[x2].Value;
                row[x2].Value = 0;
                row[x1].Value = value;
                moved = true;
            }

            // Can move and merge.
            if(row[x1].Value == row[x2].Value && row[x1].Value != 0)
            {
                var value = row[x1].Value + row[x2].Value;
                row[x2].Value = 0;
                row[x1].Value = value;
                moved = true;
                stop = true;
                return;
            }

            stop = false;
        }

        private bool HasLost() 
            => Tiles.ToList().All(t => t.Value > 0) && !(Rows.Any(HasConsecutiveDuplicates) || Columns.Any(HasConsecutiveDuplicates));

        private static bool HasConsecutiveDuplicates(Tile[] row)
        {
            for (var i = 0; i < row.Length - 1; i++)
            {
                if (row[i].Value == row[i + 1].Value)
                {
                    return true;
                }
            }

            return false;
        }

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
    }
}
