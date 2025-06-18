using System;
using System.Collections.Generic;

namespace GetFitterGetBigger.Shared.Models
{
    /// <summary>
    /// Represents a workout in the GetFitterGetBigger ecosystem.
    /// </summary>
    public class Workout
    {
        /// <summary>
        /// Gets or sets the unique identifier for the workout.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the workout.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the workout.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the category of the workout (e.g., strength, cardio, flexibility).
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the difficulty level of the workout (e.g., beginner, intermediate, advanced).
        /// </summary>
        public string Difficulty { get; set; }

        /// <summary>
        /// Gets or sets the duration of the workout in minutes.
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Gets or sets the list of exercises in the workout.
        /// </summary>
        public List<Exercise> Exercises { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the workout was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the workout was last updated.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
