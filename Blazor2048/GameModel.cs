using Blazor2048.Extensions;
using Blazored.LocalStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Blazor2048
{
    public class GameModel
    {
        // Board sizes
        private const int BOARD_HEIGHT = 4;
        private const int BOARD_WIDTH = 4;

        // Local Storage Keys
        private const string LS_TILES = "Tiles";
        private const string LS_HIGH_SCORE = "HighScore";

        // Representation of the board.
        public Tile[] Tiles { get; } = new Tile[BOARD_HEIGHT * BOARD_WIDTH];
        public Tile[][] Rows { get; } = new Tile[BOARD_HEIGHT][];
        public Tile[][] Columns { get; } = new Tile[BOARD_WIDTH][];

        // Helper properties to store state at various points in the move. Used for animations.
        public Tile[] PreMoveTiles { get; set; } = new Tile[BOARD_HEIGHT * BOARD_WIDTH];
        public Tile[] PostMoveTiles { get; set; } = new Tile[BOARD_HEIGHT * BOARD_WIDTH];
        public Tile[] PostGenerateTiles { get; set; } = new Tile[BOARD_HEIGHT * BOARD_WIDTH];

        public int Score { get; private set; } = 0;
        public int HighScore { get; private set; } = 0;
        public bool GameOver { get; private set; } = false;
        public bool IsMoving { get; set; } = false;

        private readonly Random r = new Random();
        private readonly ISyncLocalStorageService _localStorage;

        public GameModel(ISyncLocalStorageService localStorage)
        {
            _localStorage = localStorage;

            int highScore = _localStorage.GetItem<int>(LS_HIGH_SCORE);
            if (highScore > 0)
                HighScore = highScore;

            bool loadedFromStorage = false;
            if (_localStorage.ContainKey(LS_TILES))
            {
                Tiles = _localStorage.GetItem<Tile[]>(LS_TILES);
                foreach (var tile in Tiles)
                    tile.NewTile = true;

                PreMoveTiles = Tiles.Select(_ => new Tile(0)).ToArray();
                PostMoveTiles = Tiles.Select(_ => new Tile(0)).ToArray();
                PostGenerateTiles = Tiles.Select(a => new Tile(a.Value)).ToArray();

                loadedFromStorage = true;
            }

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
                    var tile = loadedFromStorage ? Tiles[counter] : new Tile(0);

                    if(!loadedFromStorage)
                        Tiles[counter] = tile;

                    Rows[y][x] = tile;
                    Columns[x][y] = tile;

                    counter++;
                }
            }

            if (!loadedFromStorage)
            {
                ResetBoard();
            }
            else
            {
                CalcScore();
            }
        }

        public void ResetBoard()
        {
            Tiles.ToList().ForEach(t => t.Value = 0);
            GameOver = false;

            PreMoveTiles = Tiles.Select(a => (Tile)a.Clone()).ToArray();

            PostMoveTiles = Tiles.Select(a => (Tile)a.Clone()).ToArray();

            for (var i = 0; i < 2; i++)
            {
                GenerateNewTile();
            }

            PostGenerateTiles = Tiles.Select(a => (Tile)a.Clone()).ToArray();

            _localStorage.SetItem(LS_TILES, Tiles);
        }

        private bool HasLost()
            => Tiles.ToList().All(t => t.Value > 0) && !(Rows.Any(t => t.HasConsecutiveDuplicates()) || Columns.Any(t => t.HasConsecutiveDuplicates()));


        private void CalcScore()
        {
            Score = Tiles.Sum(t => t.Value);
            if (Score > HighScore)
            {
                HighScore = Score;
                _localStorage.SetItem(LS_HIGH_SCORE, HighScore);
            }
        }

        private void GenerateNewTile()
        {
            List<Tile> emptyTiles = Tiles.ToList().Where(t => t.Value == 0).ToList();
            int index = r.Next(emptyTiles.Count);

            emptyTiles[index].Value = GenerateNewTileValue();
            emptyTiles[index].NewTile = true;
            CalcScore();
        }

        private int GenerateNewTileValue()
        {
            return r.Next(0, 100) <= 89 ? 2 : 4;
        }

        public async Task Move(GameMove move)
        {
            if (GameOver)
                return;

            PreMoveTiles = Tiles.Select(a => (Tile)a.Clone()).ToArray();

            IsMoving = true;

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
                PostMoveTiles = Tiles.Select(a => (Tile)a.Clone()).ToArray();

                // This delay is here to give the current tiles time to animate before the new tiles are rendered.
                await Task.Delay(140);

                foreach(var tile in Tiles)
                {
                    tile.AnimationFactor = 0;
                    tile.NewTile = false;
                }

                GenerateNewTile();

                PostGenerateTiles = Tiles.Select(a => (Tile)a.Clone()).ToArray();

                GameOver = HasLost();
                
                if(GameOver)
                {
                    _localStorage.RemoveItem(LS_TILES);
                }
                else
                {
                    _localStorage.SetItem(LS_TILES, Tiles);
                }
            }
            IsMoving = false;
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
                        break;

                    // Can move but not merge.
                    if (tiles[x1].Value == 0 && tiles[x2].Value != 0)
                    {
                        var value = tiles[x2].Value;
                        tiles[x2].Value = 0;
                        tiles[x1].Value = value;
                        tiles[x2].AnimationFactor = x2 - x1;
                        hasMoved = true;
                        continue;
                    }

                    // Can move and merge.
                    if (tiles[x1].Value == tiles[x2].Value && tiles[x1].Value != 0)
                    {
                        var value = tiles[x1].Value + tiles[x2].Value;
                        tiles[x2].Value = 0;
                        tiles[x1].Value = value;
                        tiles[x2].AnimationFactor = x2 - x1;
                        hasMoved = true;
                        break;
                    }
                }
            }

            return hasMoved;
        }


    }
}
