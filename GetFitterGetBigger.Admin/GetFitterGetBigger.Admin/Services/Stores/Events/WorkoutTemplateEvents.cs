namespace GetFitterGetBigger.Admin.Services.Stores.Events
{
    public record WorkoutTemplateCreatedEvent(string TemplateId, string TemplateName);
    public record WorkoutTemplateUpdatedEvent(string TemplateId, string TemplateName);
    public record WorkoutTemplateDeletedEvent(string TemplateId);
    public record WorkoutTemplateStateChangedEvent(string TemplateId, string NewStateId);
    public record WorkoutTemplateDuplicatedEvent(string OriginalId, string NewId, string NewName);
}