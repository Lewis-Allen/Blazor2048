using Blazor2048.Enums;
using System.Threading.Tasks;

namespace Blazor2048.Services
{
    public interface IGameStateService
    {
        bool GameOver { get; }
        int HighScore { get; }
        bool IsInitialized { get; }
        bool IsMoving { get; }
        int Score { get; }

        Tile[][] GetPostGenerateRows();
        Tile[][] GetPostMoveRows();
        Tile[][] GetPreMoveRows();
        Tile[][] GetRows();
        Task InitializeAsync();
        Task MoveAsync(GameMove move);
        Task ResetBoardAsync();
    }
}