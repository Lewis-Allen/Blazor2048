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

        public int Score { get; private set; } = 0;
        public int HighScore { get; private set; } = 0;
        public bool GameOver { get; private set; } = false;
        public bool IsMoving { get; private set; } = false;
        public bool IsInitialized { get; private set; } = false;

        public GameStateService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public Tile[][] GetRows() => Tiles.GetRows();

        public async Task InitializeAsync()
        {
            int highScore = await _localStorage.GetItemAsync<int>(Constants.LS_HIGH_SCORE);
            if (highScore > 0)
                HighScore = highScore;

            if (await _localStorage.ContainKeyAsync(Constants.LS_TILES))
            {
                Tiles = await _localStorage.GetItemAsync<Tile[]>(Constants.LS_TILES);
                foreach (var tile in Tiles)
                    tile.NewTile = true;

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
            for (var i = 0; i < Tiles.Length; i++)
                Tiles[i] = new Tile(0);

            GameOver = false;

            for (var i = 0; i < Constants.BOARD_STARTING_TILES; i++)
                await GenerateNewTileAsync();

            await _localStorage.SetItemAsync(Constants.LS_TILES, Tiles);
        }

        private async Task<bool> HasLost()
            => await Task.FromResult(Tiles.ToList().All(t => t.Value > 0) 
                && !(Tiles.GetRows().Any(t => t.HasConsecutiveDuplicates()) || Tiles.GetColumns().Any(t => t.HasConsecutiveDuplicates())));


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
            Tile[] emptyTiles = Tiles.Where(t => t.Value == 0).ToArray();
            int index = r.Next(emptyTiles.Length);
            
            Tile tile = emptyTiles[index];
            tile.Value = await GenerateNewTileValueAsync();
            tile.NewTile = true;

            await CalcScoreAsync();
        }

        private async Task<int> GenerateNewTileValueAsync()
        {
            return await Task.FromResult(r.Next(0, 100) <= 89 ? 2 : 4);
        }

        public async Task MoveAsync(GameMove move)
        {
            if (GameOver || IsMoving)
                return;

            IsMoving = true;

            // Store a deep copy of the current tiles
            Tile[] newTiles = Tiles.Select(t => new Tile(t.Value)).ToArray();

            // Was anything moved from this input?
            bool HasMoved = move switch
            {
                GameMove.UP => newTiles.GetColumns().Aggregate(false, (acc, column) => acc | MoveLine(column)),
                GameMove.RIGHT => newTiles.GetRows().Aggregate(false, (acc, row) => acc | MoveLine(row.Reversed())),
                GameMove.DOWN => newTiles.GetColumns().Aggregate(false, (acc, column) => acc | MoveLine(column.Reversed())),
                GameMove.LEFT => newTiles.GetRows().Aggregate(false, (acc, row) => acc | MoveLine(row)),
                _ => throw new NotSupportedException("Move not supported.")
            };

            if (HasMoved)
            {

                // Attach the animation factor onto the current tile configuration but don't take the new value yet. This will render the animation classes.
                Tiles = Tiles.Zip(newTiles, (a, b) => new Tile(a.Value, b.AnimationFactor)).ToArray();

                // Delay so the animations can play.
                await Task.Delay(120);

                // Render the new tiles in their new positions.
                Tiles = newTiles.Select(t => new Tile(t.Value, t.Merged)).ToArray();

                await GenerateNewTileAsync();

                GameOver = await HasLost();

                if (GameOver)
                {
                    await _localStorage.RemoveItemAsync(Constants.LS_TILES);
                }
                else
                {
                    await _localStorage.SetItemAsync(Constants.LS_TILES, Tiles.Select(t => new Tile(t.Value)).ToArray());
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
                        tiles[x1].Merged = true;
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
