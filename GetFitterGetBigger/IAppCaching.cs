using System.Collections.Generic;
using GetFitterGetBigger.Model;

namespace GetFitterGetBigger;

public interface IAppCaching
{
    IList<Workout> Workouts { get; }

    Workout WorkoutOfTheDay { get; set; }
}
