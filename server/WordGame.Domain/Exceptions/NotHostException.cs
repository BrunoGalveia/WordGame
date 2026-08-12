namespace WordGame.Domain.Exceptions;

public class NotHostException() : Exception("Only the room host can perform this action.");
