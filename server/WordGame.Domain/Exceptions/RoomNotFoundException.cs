namespace WordGame.Domain.Exceptions;

public class RoomNotFoundException(string code) : Exception($"Room '{code}' not found.");
