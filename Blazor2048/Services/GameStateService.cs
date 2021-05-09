using Blazor2048.Extensions;
using Blazored.LocalStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazor2048.Enums;

namespace Blazor2048.Services
{
    public class GameStateService : IGameStateService
    {
        private readonly ILocalStorageService _localStorage;
        private readonly Random r = new Random();

        // Representation of the board.
        private Tile[] Tiles { get; set; } = new Tile[Constants.BOARD_HEIGHT * Constants.BOARD_WIDTH];
        private Tile[][] Rows { get; } = new Tile[Constants.BOARD_HEIGHT][];
        private Tile[][] Columns { get; } = new Tile[Constants.BOARD_WIDTH][];

        // Helper properties to store state at various points in a move. Used for animations.
        private Tile[] PreMoveTiles { get; set; } = new Tile[Constants.BOARD_HEIGHT * Constants.BOARD_WIDTH];
        private Tile[] PostMoveTiles { get; set; } = new Tile[Constants.BOARD_HEIGHT * Constants.BOARD_WIDTH];
        private Tile[] PostGenerateTiles { get; set; } = new Tile[Constants.BOARD_HEIGHT * Constants.BOARD_WIDTH];

        public int Score { get; private set; } = 0;
        public int HighScore { get; private set; } = 0;
        public bool GameOver { get; private set; } = false;
        public bool IsMoving { get; private set; } = false;
        public bool IsInitialized { get; private set; } = false;

        public GameStateService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public Tile[][] GetRows() => Rows;
        public Tile[][] GetPreMoveRows() => PreMoveTiles.Split(4).ToArray().Transpose();
        public Tile[][] GetPostMoveRows() => PostMoveTiles.Split(4).ToArray().Transpose();
        public Tile[][] GetPostGenerateRows() => PostGenerateTiles.Split(4).ToArray().Transpose();

        public async Task InitializeAsync()
        {
            int highScore = await _localStorage.GetItemAsync<int>(Constants.LS_HIGH_SCORE);
            if (highScore > 0)
                HighScore = highScore;

            bool loadedFromStorage = false;
            if (await _localStorage.ContainKeyAsync(Constants.LS_TILES))
            {
                Tiles = await _localStorage.GetItemAsync<Tile[]>(Constants.LS_TILES);
                foreach (var tile in Tiles)
                    tile.NewTile = true;

                PreMoveTiles = Tiles.Select(_ => new Tile(0)).ToArray();
                PostMoveTiles = Tiles.Select(_ => new Tile(0)).ToArray();
                PostGenerateTiles = Tiles.Select(t => new Tile(t.Value)).ToArray();

                loadedFromStorage = true;
            }

            for (var y = 0; y < Constants.BOARD_HEIGHT; y++)
                Rows[y] = new Tile[Constants.BOARD_WIDTH];

            for (var x = 0; x < Constants.BOARD_WIDTH; x++)
                Columns[x] = new Tile[Constants.BOARD_HEIGHT];

            var counter = 0;
            for (var x = 0; x < Constants.BOARD_WIDTH; x++)
            {
                for (var y = 0; y < Constants.BOARD_HEIGHT; y++)
                {
                    var tile = loadedFromStorage ? Tiles[counter] : new Tile(0);

                    if (!loadedFromStorage)
                        Tiles[counter] = tile;

                    Rows[y][x] = tile;
                    Columns[x][y] = tile;

                    counter++;
                }
            }

            if (loadedFromStorage)
            {
                await CalcScoreAsync();
            }
            else
            {
                await ResetBoardAsync();
            }
            IsInitialized = true;
        }

        public async Task ResetBoardAsync()
        {
            Tiles.ToList().ForEach(t => t.Value = 0);
            GameOver = false;

            PreMoveTiles = Tiles.Select(t => new Tile(t.Value)).ToArray();

            PostMoveTiles = Tiles.Select(t => new Tile(t.Value)).ToArray();

            for (var i = 0; i < 2; i++)
            {
                await GenerateNewTileAsync();
            }

            PostGenerateTiles = Tiles.Select(t => new Tile(t.Value)).ToArray();

            await _localStorage.SetItemAsync(Constants.LS_TILES, Tiles);
        }

        private async Task<bool> HasLost()
            => await Task.FromResult(Tiles.ToList().All(t => t.Value > 0) && !(Rows.Any(t => t.HasConsecutiveDuplicates()) || Columns.Any(t => t.HasConsecutiveDuplicates())));


        private async Task CalcScoreAsync()
        {
            Score = Tiles.Sum(t => t.Value);
            if (Score > HighScore)
            {
                HighScore = Score;
                await _localStorage.SetItemAsync(Constants.LS_HIGH_SCORE, HighScore);
            }
        }

        private async Task GenerateNewTileAsync()
        {
            List<Tile> emptyTiles = Tiles.ToList().Where(t => t.Value == 0).ToList();
            int index = r.Next(emptyTiles.Count);

            emptyTiles[index].Value = await GenerateNewTileValueAsync();
            emptyTiles[index].NewTile = true;
            await CalcScoreAsync();
        }

        private async Task<int> GenerateNewTileValueAsync()
        {
            return await Task.FromResult(r.Next(0, 100) <= 89 ? 2 : 4);
        }

        public async Task MoveAsync(GameMove move)
        {
            if (GameOver)
                return;

            PreMoveTiles = Tiles.Select(t => new Tile(t.Value)).ToArray();

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
                PostMoveTiles = Tiles.Select(t => new Tile(t.Value)).ToArray();

                // This delay is here to give the current tiles time to animate before the new tiles are rendered.
                await Task.Delay(140);

                foreach (var tile in Tiles)
                {
                    tile.AnimationFactor = 0;
                    tile.NewTile = false;
                }

                await GenerateNewTileAsync();

                PostGenerateTiles = Tiles.Select(t => new Tile(t.Value)).ToArray();

                GameOver = await HasLost();

                if (GameOver)
                {
                    await _localStorage.RemoveItemAsync(Constants.LS_TILES);
                }
                else
                {
                    await _localStorage.SetItemAsync(Constants.LS_TILES, Tiles);
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
