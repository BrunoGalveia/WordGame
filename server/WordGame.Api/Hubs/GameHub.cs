using Microsoft.AspNetCore.SignalR;
using WordGame.Application.Dtos;
using WordGame.Application.Rooms;
using WordGame.Domain.Exceptions;

namespace WordGame.Api.Hubs;

public class GameHub(IRoomService roomService) : Hub
{
    private const string RoomCodeItemKey = "roomCode";
    private const string PlayerIdItemKey = "playerId";

    public async Task<JoinRoomResultDto> JoinRoom(string roomCode, string nickname, Guid playerId)
    {
        roomCode = roomCode.Trim().ToUpperInvariant();

        JoinRoomResultDto result;
        try
        {
            result = roomService.JoinRoom(roomCode, nickname, playerId, Context.ConnectionId);
        }
        catch (RoomNotFoundException ex)
        {
            throw new HubException(ex.Message);
        }

        Context.Items[RoomCodeItemKey] = roomCode;
        Context.Items[PlayerIdItemKey] = playerId;

        await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
        // A private, per-player group — lets us push that player's own word/hint without
        // the hub needing to know connection ids (those stay a Domain/Infrastructure detail).
        await Groups.AddToGroupAsync(Context.ConnectionId, PlayerGroup(roomCode, playerId));
        await Clients.OthersInGroup(roomCode).SendAsync("PlayerListUpdated", result.Players);

        return result;
    }

    public async Task RequestNewWord(string roomCode)
    {
        roomCode = roomCode.Trim().ToUpperInvariant();
        if (Context.Items[PlayerIdItemKey] is not Guid requestingPlayerId)
        {
            throw new HubException("You must join the room before requesting a new word.");
        }

        IReadOnlyDictionary<Guid, WordAssignmentDto> assignments;
        try
        {
            assignments = await roomService.RequestNewWordAsync(roomCode, requestingPlayerId);
        }
        catch (RoomNotFoundException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (NotHostException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (NoActiveWordsException ex)
        {
            throw new HubException(ex.Message);
        }

        await Clients.Group(roomCode).SendAsync("RoundStarted");

        foreach (var (playerId, assignment) in assignments)
        {
            await Clients.Group(PlayerGroup(roomCode, playerId)).SendAsync("YourAssignment", assignment);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Context.Items[RoomCodeItemKey] is string roomCode && Context.Items[PlayerIdItemKey] is Guid playerId)
        {
            roomService.HandleDisconnect(roomCode, playerId, Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private static string PlayerGroup(string roomCode, Guid playerId) => $"{roomCode}:player:{playerId}";
}
