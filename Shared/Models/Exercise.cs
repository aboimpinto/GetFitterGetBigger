using System;

namespace GetFitterGetBigger.Shared.Models
{
    /// <summary>
    /// Represents an exercise in the GetFitterGetBigger ecosystem.
    /// </summary>
    public class Exercise
    {
        /// <summary>
        /// Gets or sets the unique identifier for the exercise.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the exercise.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of the exercise (e.g., bodyweight, weighted, cardio).
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the number of sets for the exercise.
        /// </summary>
        public int Sets { get; set; }

        /// <summary>
        /// Gets or sets the number of repetitions for each set.
        /// </summary>
        public int Reps { get; set; }

        /// <summary>
        /// Gets or sets the duration of the exercise in seconds (for timed exercises).
        /// </summary>
        public int Duration { get; set; }

        /// <summary>
        /// Gets or sets the rest time between sets in seconds.
        /// </summary>
        public int RestTime { get; set; }

        /// <summary>
        /// Gets or sets the URL of the image demonstrating the exercise.
        /// </summary>
        public string ImageUrl { get; set; }
    }
}
