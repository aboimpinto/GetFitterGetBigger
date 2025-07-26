using System.Collections.Generic;
using System.Threading.Tasks;
using GetFitterGetBigger.Admin.Models.Dtos;
using GetFitterGetBigger.Admin.Models.ReferenceData;
using GetFitterGetBigger.Admin.Services;
using Moq;

namespace GetFitterGetBigger.Admin.Tests.TestHelpers
{
    /// <summary>
    /// Extension methods to simplify mocking of IGenericReferenceDataService in tests.
    /// </summary>
    public static class MockGenericReferenceDataServiceExtensions
    {
        public static void SetupDifficultyLevels(this Mock<IGenericReferenceDataService> mock, IEnumerable<ReferenceDataDto> data)
        {
            mock.Setup(x => x.GetReferenceDataAsync<DifficultyLevels>()).ReturnsAsync(data);
        }

        public static void SetupMuscleGroups(this Mock<IGenericReferenceDataService> mock, IEnumerable<ReferenceDataDto> data)
        {
            mock.Setup(x => x.GetReferenceDataAsync<MuscleGroups>()).ReturnsAsync(data);
        }

        public static void SetupMuscleRoles(this Mock<IGenericReferenceDataService> mock, IEnumerable<ReferenceDataDto> data)
        {
            mock.Setup(x => x.GetReferenceDataAsync<MuscleRoles>()).ReturnsAsync(data);
        }

        public static void SetupEquipment(this Mock<IGenericReferenceDataService> mock, IEnumerable<ReferenceDataDto> data)
        {
            mock.Setup(x => x.GetReferenceDataAsync<Equipment>()).ReturnsAsync(data);
        }

        public static void SetupBodyParts(this Mock<IGenericReferenceDataService> mock, IEnumerable<ReferenceDataDto> data)
        {
            mock.Setup(x => x.GetReferenceDataAsync<BodyParts>()).ReturnsAsync(data);
        }

        public static void SetupMovementPatterns(this Mock<IGenericReferenceDataService> mock, IEnumerable<ReferenceDataDto> data)
        {
            mock.Setup(x => x.GetReferenceDataAsync<MovementPatterns>()).ReturnsAsync(data);
        }

        public static void SetupExerciseTypes(this Mock<IGenericReferenceDataService> mock, IEnumerable<ReferenceDataDto> data)
        {
            mock.Setup(x => x.GetReferenceDataAsync<ExerciseTypes>()).ReturnsAsync(data);
        }

        public static void SetupKineticChainTypes(this Mock<IGenericReferenceDataService> mock, IEnumerable<ReferenceDataDto> data)
        {
            mock.Setup(x => x.GetReferenceDataAsync<KineticChainTypes>()).ReturnsAsync(data);
        }

        public static void SetupMetricTypes(this Mock<IGenericReferenceDataService> mock, IEnumerable<ReferenceDataDto> data)
        {
            mock.Setup(x => x.GetReferenceDataAsync<MetricTypes>()).ReturnsAsync(data);
        }

        public static void SetupClearEquipmentCache(this Mock<IGenericReferenceDataService> mock)
        {
            mock.Setup(x => x.ClearCache<Equipment>());
        }

        public static void SetupClearMuscleGroupsCache(this Mock<IGenericReferenceDataService> mock)
        {
            mock.Setup(x => x.ClearCache<MuscleGroups>());
        }
    }
}