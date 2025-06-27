using System;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Xunit;

namespace GetFitterGetBigger.API.Tests.Models.Entities;

public class CoachNoteTests
{
    [Fact]
    public void CreateNew_ValidInput_CreatesCoachNote()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var text = "Keep your back straight during the movement";
        var order = 1;
        
        // Act
        var coachNote = CoachNote.Handler.CreateNew(exerciseId, text, order);
        
        // Assert
        Assert.NotEqual(default(CoachNoteId), coachNote.Id);
        Assert.Equal(exerciseId, coachNote.ExerciseId);
        Assert.Equal(text, coachNote.Text);
        Assert.Equal(order, coachNote.Order);
    }
    
    [Fact]
    public void CreateNew_TextWithWhitespace_TrimsText()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var text = "  Keep your back straight  ";
        var order = 1;
        
        // Act
        var coachNote = CoachNote.Handler.CreateNew(exerciseId, text, order);
        
        // Assert
        Assert.Equal("Keep your back straight", coachNote.Text);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateNew_EmptyText_ThrowsArgumentException(string text)
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var order = 1;
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            CoachNote.Handler.CreateNew(exerciseId, text, order));
        Assert.Equal("Text cannot be empty (Parameter 'text')", exception.Message);
    }
    
    [Fact]
    public void CreateNew_TextExceeds1000Characters_ThrowsArgumentException()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var text = new string('a', 1001);
        var order = 1;
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            CoachNote.Handler.CreateNew(exerciseId, text, order));
        Assert.Equal("Text cannot exceed 1000 characters (Parameter 'text')", exception.Message);
    }
    
    [Fact]
    public void CreateNew_NegativeOrder_ThrowsArgumentException()
    {
        // Arrange
        var exerciseId = ExerciseId.New();
        var text = "Valid text";
        var order = -1;
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            CoachNote.Handler.CreateNew(exerciseId, text, order));
        Assert.Equal("Order must be non-negative (Parameter 'order')", exception.Message);
    }
    
    [Fact]
    public void Create_ValidInput_CreatesCoachNoteWithSpecificId()
    {
        // Arrange
        var id = CoachNoteId.New();
        var exerciseId = ExerciseId.New();
        var text = "Keep your core engaged";
        var order = 2;
        
        // Act
        var coachNote = CoachNote.Handler.Create(id, exerciseId, text, order);
        
        // Assert
        Assert.Equal(id, coachNote.Id);
        Assert.Equal(exerciseId, coachNote.ExerciseId);
        Assert.Equal(text, coachNote.Text);
        Assert.Equal(order, coachNote.Order);
    }
    
    [Fact]
    public void UpdateText_ValidText_UpdatesText()
    {
        // Arrange
        var original = CoachNote.Handler.CreateNew(ExerciseId.New(), "Original text", 1);
        var newText = "Updated text";
        
        // Act
        var updated = CoachNote.Handler.UpdateText(original, newText);
        
        // Assert
        Assert.Equal(newText, updated.Text);
        Assert.Equal(original.Id, updated.Id);
        Assert.Equal(original.ExerciseId, updated.ExerciseId);
        Assert.Equal(original.Order, updated.Order);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateText_EmptyText_ThrowsArgumentException(string newText)
    {
        // Arrange
        var original = CoachNote.Handler.CreateNew(ExerciseId.New(), "Original text", 1);
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            CoachNote.Handler.UpdateText(original, newText));
        Assert.Equal("Text cannot be empty (Parameter 'newText')", exception.Message);
    }
    
    [Fact]
    public void UpdateText_TextExceeds1000Characters_ThrowsArgumentException()
    {
        // Arrange
        var original = CoachNote.Handler.CreateNew(ExerciseId.New(), "Original text", 1);
        var newText = new string('a', 1001);
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            CoachNote.Handler.UpdateText(original, newText));
        Assert.Equal("Text cannot exceed 1000 characters (Parameter 'newText')", exception.Message);
    }
    
    [Fact]
    public void UpdateOrder_ValidOrder_UpdatesOrder()
    {
        // Arrange
        var original = CoachNote.Handler.CreateNew(ExerciseId.New(), "Text", 1);
        var newOrder = 5;
        
        // Act
        var updated = CoachNote.Handler.UpdateOrder(original, newOrder);
        
        // Assert
        Assert.Equal(newOrder, updated.Order);
        Assert.Equal(original.Id, updated.Id);
        Assert.Equal(original.ExerciseId, updated.ExerciseId);
        Assert.Equal(original.Text, updated.Text);
    }
    
    [Fact]
    public void UpdateOrder_NegativeOrder_ThrowsArgumentException()
    {
        // Arrange
        var original = CoachNote.Handler.CreateNew(ExerciseId.New(), "Text", 1);
        var newOrder = -1;
        
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
            CoachNote.Handler.UpdateOrder(original, newOrder));
        Assert.Equal("Order must be non-negative (Parameter 'newOrder')", exception.Message);
    }
    
    [Fact]
    public void CoachNote_IsRecord_SupportsValueEquality()
    {
        // Arrange
        var id = CoachNoteId.New();
        var exerciseId = ExerciseId.New();
        var text = "Test text";
        var order = 1;
        
        // Act
        var note1 = CoachNote.Handler.Create(id, exerciseId, text, order);
        var note2 = CoachNote.Handler.Create(id, exerciseId, text, order);
        
        // Assert
        Assert.Equal(note1, note2);
        Assert.True(note1 == note2);
    }
}