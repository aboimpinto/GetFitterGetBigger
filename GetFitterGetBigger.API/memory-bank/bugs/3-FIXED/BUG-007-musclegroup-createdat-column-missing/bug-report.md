# BUG-007: MuscleGroup CreatedAt Column Missing in Database

## Bug ID: BUG-007
## Reported: 2025-01-30
## Status: FIXED
## Severity: Critical
## Affected Version: Current Development
## Fixed Version: 2025-01-30

## Description
When calling the GET /api/exercises endpoint through Swagger, the application throws a PostgreSQL exception indicating that the column `m.CreatedAt` does not exist. This prevents the exercise list from being retrieved, making the exercise management feature completely unusable.

## Error Message
```
Npgsql.PostgresException (0x80004005): 42703: column m.CreatedAt does not exist

POSITION: 563
   at Npgsql.Internal.NpgsqlConnector.ReadMessageLong(Boolean async, DataRowLoadingMode dataRowLoadingMode, Boolean readingNotifications, Boolean isReadingPrependedMessage)
   at System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder`1.StateMachineBox`1.System.Threading.Tasks.Sources.IValueTaskSource<TResult>.GetResult(Int16 token)
   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)
   at Npgsql.NpgsqlDataReader.NextResult(Boolean async, Boolean isConsuming, CancellationToken cancellationToken)
   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)
   at Npgsql.NpgsqlCommand.ExecuteReader(Boolean async, CommandBehavior behavior, CancellationToken cancellationToken)
   at Npgsql.NpgsqlCommand.ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReaderAsync(RelationalCommandParameterObject parameterObject, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Query.RelationalShapedQueryCompilingExpressionVisitor.ShaperProcessingExpressionVisitor.<PopulateSplitIncludeCollectionAsync>g__InitializeReaderAsync|26_0[TIncludingEntity,TIncludedEntity](RelationalQueryContext queryContext, RelationalCommandResolver relationalCommandResolver, IReadOnlyList`1 readerColumns, Boolean detailedErrorsEnabled, CancellationToken cancellationToken)
   at Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.NpgsqlExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Query.RelationalShapedQueryCompilingExpressionVisitor.ShaperProcessingExpressionVisitor.PopulateSplitIncludeCollectionAsync[TIncludingEntity,TIncludedEntity](Int32 collectionId, RelationalQueryContext queryContext, IExecutionStrategy executionStrategy, RelationalCommandResolver relationalCommandResolver, IReadOnlyList`1 readerColumns, Boolean detailedErrorsEnabled, SplitQueryResultCoordinator resultCoordinator, Func`3 childIdentifier, IReadOnlyList`1 identifierValueComparers, Func`5 innerShaper, Func`4 relatedDataLoaders, INavigationBase inverseNavigation, Action`2 fixup, Boolean trackingQuery)
   at Microsoft.EntityFrameworkCore.Query.RelationalShapedQueryCompilingExpressionVisitor.ShaperProcessingExpressionVisitor.TaskAwaiter(Func`1[] taskFactories)
   at Microsoft.EntityFrameworkCore.Query.Internal.SplitQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()
   at Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync[TSource](IQueryable`1 source, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync[TSource](IQueryable`1 source, CancellationToken cancellationToken)
   at GetFitterGetBigger.API.Repositories.Implementations.ExerciseRepository.GetPagedAsync(Int32 pageNumber, Int32 pageSize, String name, Nullable`1 difficultyId, IEnumerable`1 muscleGroupIds, IEnumerable`1 equipmentIds, IEnumerable`1 movementPatternIds, IEnumerable`1 bodyPartIds, Boolean includeInactive) in /home/esqueleto/myWork/GetFitterGetBigger/GetFitterGetBigger.API/GetFitterGetBigger.API/Repositories/Implementations/ExerciseRepository.cs:line 95
   at GetFitterGetBigger.API.Services.Implementations.ExerciseService.GetPagedAsync(ExerciseFilterParams filterParams) in /home/esqueleto/myWork/GetFitterGetBigger/GetFitterGetBigger.API/GetFitterGetBigger.API/Services/Implementations/ExerciseService.cs:line 46
   at GetFitterGetBigger.API.Controllers.ExercisesController.GetExercises(ExerciseFilterParams filterParams) in /home/esqueleto/myWork/GetFitterGetBigger/GetFitterGetBigger.API/GetFitterGetBigger.API/Controllers/ExercisesController.cs:line 52
```

## Reproduction Steps
1. Start the API application
2. Navigate to Swagger UI (http://localhost:5214/swagger)
3. Authenticate with valid credentials
4. Try to execute GET /api/exercises
5. Expected: List of exercises returned
6. Actual: 500 Internal Server Error with PostgreSQL column not found exception

## Root Cause
The Entity Framework migration `20250629125752_AddCrudFieldsToMuscleGroup` exists but has not been applied to the database. This migration adds the following columns to the MuscleGroups table:
- CreatedAt (timestamp)
- UpdatedAt (timestamp) 
- IsActive (boolean)

The application code (specifically `ExerciseRepository.GetPagedAsync()`) includes MuscleGroup entities in its query, and Entity Framework generates SQL that references `m.CreatedAt`. Since the migration hasn't been applied, the database column doesn't exist, causing the PostgreSQL error.

## Impact
- Users affected: All users attempting to access exercise data
- Features affected: 
  - Exercise listing
  - Exercise filtering
  - Any feature dependent on exercise data retrieval
- Business impact: Complete inability to manage exercises, which is a core feature of the application

## Workaround
None available. The migration must be applied to fix this issue.

## Test Data
No specific test data required - the error occurs with any attempt to retrieve exercises.

## Fix Summary
The fix was straightforward - applied the pending migration `AddCrudFieldsToMuscleGroup` using `dotnet ef database update`. This added the missing CreatedAt, UpdatedAt, and IsActive columns to the MuscleGroups table. After applying the migration:
- All unit tests pass (100% success rate)
- The GET /api/exercises endpoint works correctly
- Exercise filtering and retrieval functions as expected
- No additional code changes were required

## Related Feature
This bug was discovered during the implementation of the exercise management feature.