using System.Collections.Generic;

namespace Olimpo.NavigationManager;

public record NavigationHistoryEntry(
    string ViewName,
    IDictionary<string, object> Parameters);

public static class NavigationHistoryEntryHandler
{
    public static NavigationHistoryEntry CreateNew(
        string viewModelName,
        IDictionary<string, object> parameters) =>
        new(viewModelName, parameters);
}
