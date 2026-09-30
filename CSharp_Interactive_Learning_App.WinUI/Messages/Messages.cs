using CSharp_Interactive_Learning_App.Shared.Models;

namespace CSharp_Interactive_Learning_App.WinUI.Messages;

public sealed record BattleMessage(BattleResult Result);

public sealed record InfoBarMessage(string Title, string Message, bool IsWarning = false, bool IsError = false);
public sealed record TeachingTipMessage(string Title, string Message);

public sealed record NavbarMessage(bool Hide);

public sealed record NavigationMessage(bool GoBack, Type TargetPage = null!, object Parameter = null!);

public sealed record ShowDialogMessage(string? Title, string? Content, string? PrimaryText, string? SecondaryText);

public sealed class AbandonBattleMessage
{
    public required Action<bool> Respond { get; init; }
}