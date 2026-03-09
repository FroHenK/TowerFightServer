namespace TowerFight.BusinessLogic.Services;

public record InsertHighscoreSuccess(Guid Guid);
public record NameOwnedByAnotherAccountError(string Reason);
public record OperationOkNoChanges(string Reason);
