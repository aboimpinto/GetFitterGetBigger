using Bunit;
using FluentAssertions;
using GetFitterGetBigger.Admin.Components.Shared;
using GetFitterGetBigger.Admin.Models.Dtos;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using AngleSharp.Dom;

namespace GetFitterGetBigger.Admin.Tests.Components
{
    public class CoachNotesEditorTests : TestContext
    {
        [Fact]
        public void CoachNotesEditor_WithNoNotes_ShowsEmptyState()
        {
            // Arrange & Act
            var component = RenderComponent<CoachNotesEditor>(parameters => parameters
                .Add(p => p.Notes, new List<CoachNoteCreateDto>())
            );

            // Assert
            component.Find(".text-center").TextContent.Should().Contain("No coach notes added yet");
            component.Find("button").TextContent.Should().Contain("Add Note");
        }

        [Fact]
        public void AddNote_WhenClicked_AddsNewNoteToList()
        {
            // Arrange
            var notes = new List<CoachNoteCreateDto>();
            var notesChanged = false;
            
            var component = RenderComponent<CoachNotesEditor>(parameters => parameters
                .Add(p => p.Notes, notes)
                .Add(p => p.NotesChanged, EventCallback.Factory.Create<List<CoachNoteCreateDto>>(this, (list) => notesChanged = true))
            );

            // Act
            component.Find("button").Click();

            // Assert
            notes.Should().HaveCount(1);
            notes[0].Text.Should().BeEmpty();
            notes[0].Order.Should().Be(0);
            notesChanged.Should().BeTrue();
        }

        [Fact]
        public void CoachNotesEditor_WithExistingNotes_DisplaysAllNotes()
        {
            // Arrange
            var notes = new List<CoachNoteCreateDto>
            {
                new() { Text = "First note", Order = 0 },
                new() { Text = "Second note", Order = 1 }
            };

            // Act
            var component = RenderComponent<CoachNotesEditor>(parameters => parameters
                .Add(p => p.Notes, notes)
            );

            // Assert
            var textareas = component.FindAll("textarea");
            textareas.Should().HaveCount(2);
            textareas[0].GetAttribute("value").Should().Be("First note");
            textareas[1].GetAttribute("value").Should().Be("Second note");
            
            component.FindAll("label").Where(l => l.TextContent.Contains("Step")).Should().HaveCount(2);
            component.Find("label").TextContent.Should().Contain("Step 1");
        }

        [Fact]
        public void UpdateNoteText_WhenTextChanged_UpdatesNoteContent()
        {
            // Arrange
            var notes = new List<CoachNoteCreateDto>
            {
                new() { Text = "Original text", Order = 0 }
            };
            var updatedNotes = new List<CoachNoteCreateDto>();
            
            var component = RenderComponent<CoachNotesEditor>(parameters => parameters
                .Add(p => p.Notes, notes)
                .Add(p => p.NotesChanged, EventCallback.Factory.Create<List<CoachNoteCreateDto>>(this, (list) => updatedNotes = list))
            );

            // Act
            var textarea = component.Find("textarea");
            textarea.Change("Updated text");

            // Assert
            notes[0].Text.Should().Be("Updated text");
            updatedNotes.Should().BeEquivalentTo(notes);
        }

        [Fact]
        public void UpdateNoteText_WithTextOver1000Chars_TruncatesText()
        {
            // Arrange
            var notes = new List<CoachNoteCreateDto>
            {
                new() { Text = "", Order = 0 }
            };
            var longText = new string('a', 1005);
            
            var component = RenderComponent<CoachNotesEditor>(parameters => parameters
                .Add(p => p.Notes, notes)
            );

            // Act
            component.Find("textarea").Change(longText);

            // Assert
            notes[0].Text.Length.Should().Be(1000);
            notes[0].Text.Should().Be(new string('a', 1000));
        }

        [Fact]
        public void CharacterCounter_WhenNearLimit_ShowsWarning()
        {
            // Arrange
            var notes = new List<CoachNoteCreateDto>
            {
                new() { Text = new string('a', 950), Order = 0 }
            };

            // Act
            var component = RenderComponent<CoachNotesEditor>(parameters => parameters
                .Add(p => p.Notes, notes)
            );

            // Assert
            component.Find(".text-yellow-600").TextContent.Should().Contain("50 characters remaining");
        }

        [Fact]
        public void RemoveNote_WhenClicked_RemovesNoteAndReorders()
        {
            // Arrange
            var notes = new List<CoachNoteCreateDto>
            {
                new() { Text = "Note 1", Order = 0 },
                new() { Text = "Note 2", Order = 1 },
                new() { Text = "Note 3", Order = 2 }
            };
            
            var component = RenderComponent<CoachNotesEditor>(parameters => parameters
                .Add(p => p.Notes, notes)
            );

            // Act - Remove the middle note
            var removeButtons = component.FindAll("button[title='Remove note']");
            removeButtons[1].Click();

            // Assert
            notes.Should().HaveCount(2);
            notes[0].Text.Should().Be("Note 1");
            notes[0].Order.Should().Be(0);
            notes[1].Text.Should().Be("Note 3");
            notes[1].Order.Should().Be(1); // Reordered
        }

        [Fact]
        public void MoveNoteUp_WhenNotFirstItem_SwapsWithPrevious()
        {
            // Arrange
            var notes = new List<CoachNoteCreateDto>
            {
                new() { Text = "Note 1", Order = 0 },
                new() { Text = "Note 2", Order = 1 }
            };
            
            var component = RenderComponent<CoachNotesEditor>(parameters => parameters
                .Add(p => p.Notes, notes)
            );

            // Act
            var moveUpButtons = component.FindAll("button[title='Move up']");
            moveUpButtons[1].Click(); // Click the second note's up button

            // Assert
            notes[0].Text.Should().Be("Note 2");
            notes[0].Order.Should().Be(0);
            notes[1].Text.Should().Be("Note 1");
            notes[1].Order.Should().Be(1);
        }

        [Fact]
        public void MoveNoteDown_WhenNotLastItem_SwapsWithNext()
        {
            // Arrange
            var notes = new List<CoachNoteCreateDto>
            {
                new() { Text = "Note 1", Order = 0 },
                new() { Text = "Note 2", Order = 1 }
            };
            
            var component = RenderComponent<CoachNotesEditor>(parameters => parameters
                .Add(p => p.Notes, notes)
            );

            // Act
            var moveDownButtons = component.FindAll("button[title='Move down']");
            moveDownButtons[0].Click(); // Click the first note's down button

            // Assert
            notes[0].Text.Should().Be("Note 2");
            notes[0].Order.Should().Be(0);
            notes[1].Text.Should().Be("Note 1");
            notes[1].Order.Should().Be(1);
        }

        [Fact]
        public void MoveButtons_AtBoundaries_AreDisabled()
        {
            // Arrange
            var notes = new List<CoachNoteCreateDto>
            {
                new() { Text = "Note 1", Order = 0 },
                new() { Text = "Note 2", Order = 1 },
                new() { Text = "Note 3", Order = 2 }
            };
            
            var component = RenderComponent<CoachNotesEditor>(parameters => parameters
                .Add(p => p.Notes, notes)
            );

            // Assert
            var moveUpButtons = component.FindAll("button[title='Move up']");
            var moveDownButtons = component.FindAll("button[title='Move down']");
            
            // First item's up button should be disabled
            moveUpButtons[0].HasAttribute("disabled").Should().BeTrue();
            moveUpButtons[0].GetAttribute("class").Should().Contain("opacity-50");
            
            // Last item's down button should be disabled
            moveDownButtons[2].HasAttribute("disabled").Should().BeTrue();
            moveDownButtons[2].GetAttribute("class").Should().Contain("opacity-50");
            
            // Middle items should not be disabled
            moveUpButtons[1].HasAttribute("disabled").Should().BeFalse();
            moveDownButtons[1].HasAttribute("disabled").Should().BeFalse();
        }

        [Fact]
        public void MultipleMoveOperations_MaintainCorrectOrder()
        {
            // Arrange
            var notes = new List<CoachNoteCreateDto>
            {
                new() { Text = "A", Order = 0 },
                new() { Text = "B", Order = 1 },
                new() { Text = "C", Order = 2 },
                new() { Text = "D", Order = 3 }
            };
            
            var component = RenderComponent<CoachNotesEditor>(parameters => parameters
                .Add(p => p.Notes, notes)
            );

            // Act - Move C up twice
            var moveUpButtons = component.FindAll("button[title='Move up']");
            moveUpButtons[2].Click(); // C moves to position 1
            
            moveUpButtons = component.FindAll("button[title='Move up']"); // Re-query after DOM update
            moveUpButtons[1].Click(); // C moves to position 0

            // Assert
            notes.Select(n => n.Text).Should().BeEquivalentTo(new[] { "C", "A", "B", "D" }, options => options.WithStrictOrdering());
            notes.Select(n => n.Order).Should().BeEquivalentTo(new[] { 0, 1, 2, 3 });
        }
    }
}