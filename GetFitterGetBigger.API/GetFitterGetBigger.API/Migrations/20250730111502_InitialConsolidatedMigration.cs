using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GetFitterGetBigger.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialConsolidatedMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BodyParts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BodyParts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DifficultyLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DifficultyLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipment", x => x.EquipmentId);
                });

            migrationBuilder.CreateTable(
                name: "ExecutionProtocols",
                columns: table => new
                {
                    ExecutionProtocolId = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TimeBase = table.Column<bool>(type: "boolean", nullable: false),
                    RepBase = table.Column<bool>(type: "boolean", nullable: false),
                    RestPattern = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IntensityLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutionProtocols", x => x.ExecutionProtocolId);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseTypes",
                columns: table => new
                {
                    ExerciseTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseTypes", x => x.ExerciseTypeId);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseWeightTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseWeightTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KineticChainTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KineticChainTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MetricTypes",
                columns: table => new
                {
                    MetricTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetricTypes", x => x.MetricTypeId);
                });

            migrationBuilder.CreateTable(
                name: "MovementPatterns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovementPatterns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MuscleRoles",
                columns: table => new
                {
                    MuscleRoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuscleRoles", x => x.MuscleRoleId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutCategories",
                columns: table => new
                {
                    WorkoutCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Icon = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Color = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    PrimaryMuscleGroups = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutCategories", x => x.WorkoutCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutObjectives",
                columns: table => new
                {
                    WorkoutObjectiveId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutObjectives", x => x.WorkoutObjectiveId);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutStates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MuscleGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BodyPartId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuscleGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MuscleGroups_BodyParts_BodyPartId",
                        column: x => x.BodyPartId,
                        principalTable: "BodyParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    VideoUrl = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    IsUnilateral = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DifficultyId = table.Column<Guid>(type: "uuid", nullable: false),
                    KineticChainId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExerciseWeightTypeId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exercises_DifficultyLevels_DifficultyId",
                        column: x => x.DifficultyId,
                        principalTable: "DifficultyLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exercises_ExerciseWeightTypes_ExerciseWeightTypeId",
                        column: x => x.ExerciseWeightTypeId,
                        principalTable: "ExerciseWeightTypes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Exercises_KineticChainTypes_KineticChainId",
                        column: x => x.KineticChainId,
                        principalTable: "KineticChainTypes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Claims",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Resource = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Claims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Claims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    DifficultyId = table.Column<Guid>(type: "uuid", nullable: false),
                    EstimatedDurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    Tags = table.Column<List<string>>(type: "text[]", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    WorkoutStateId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutTemplates_DifficultyLevels_DifficultyId",
                        column: x => x.DifficultyId,
                        principalTable: "DifficultyLevels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkoutTemplates_WorkoutCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "WorkoutCategories",
                        principalColumn: "WorkoutCategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkoutTemplates_WorkoutStates_WorkoutStateId",
                        column: x => x.WorkoutStateId,
                        principalTable: "WorkoutStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutMuscles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    MuscleGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    EngagementLevel = table.Column<int>(type: "integer", nullable: false),
                    LoadEstimation = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutMuscles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutMuscles_MuscleGroups_MuscleGroupId",
                        column: x => x.MuscleGroupId,
                        principalTable: "MuscleGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoachNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoachNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoachNotes_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseBodyParts",
                columns: table => new
                {
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    BodyPartId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseBodyParts", x => new { x.ExerciseId, x.BodyPartId });
                    table.ForeignKey(
                        name: "FK_ExerciseBodyParts_BodyParts_BodyPartId",
                        column: x => x.BodyPartId,
                        principalTable: "BodyParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseBodyParts_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseEquipment",
                columns: table => new
                {
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    EquipmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseEquipment", x => new { x.ExerciseId, x.EquipmentId });
                    table.ForeignKey(
                        name: "FK_ExerciseEquipment_Equipment_EquipmentId",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseEquipment_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseExerciseTypes",
                columns: table => new
                {
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseTypeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseExerciseTypes", x => new { x.ExerciseId, x.ExerciseTypeId });
                    table.ForeignKey(
                        name: "FK_ExerciseExerciseTypes_ExerciseTypes_ExerciseTypeId",
                        column: x => x.ExerciseTypeId,
                        principalTable: "ExerciseTypes",
                        principalColumn: "ExerciseTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseExerciseTypes_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    LinkType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseLinks_Exercises_SourceExerciseId",
                        column: x => x.SourceExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExerciseLinks_Exercises_TargetExerciseId",
                        column: x => x.TargetExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseMetricSupport",
                columns: table => new
                {
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    MetricTypeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseMetricSupport", x => new { x.ExerciseId, x.MetricTypeId });
                    table.ForeignKey(
                        name: "FK_ExerciseMetricSupport_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseMetricSupport_MetricTypes_MetricTypeId",
                        column: x => x.MetricTypeId,
                        principalTable: "MetricTypes",
                        principalColumn: "MetricTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseMovementPatterns",
                columns: table => new
                {
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementPatternId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseMovementPatterns", x => new { x.ExerciseId, x.MovementPatternId });
                    table.ForeignKey(
                        name: "FK_ExerciseMovementPatterns_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseMovementPatterns_MovementPatterns_MovementPatternId",
                        column: x => x.MovementPatternId,
                        principalTable: "MovementPatterns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseMuscleGroups",
                columns: table => new
                {
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    MuscleGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    MuscleRoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseMuscleGroups", x => new { x.ExerciseId, x.MuscleGroupId });
                    table.ForeignKey(
                        name: "FK_ExerciseMuscleGroups_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseMuscleGroups_MuscleGroups_MuscleGroupId",
                        column: x => x.MuscleGroupId,
                        principalTable: "MuscleGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseMuscleGroups_MuscleRoles_MuscleRoleId",
                        column: x => x.MuscleRoleId,
                        principalTable: "MuscleRoles",
                        principalColumn: "MuscleRoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExerciseTargetedMuscles",
                columns: table => new
                {
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    MuscleGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    MuscleRoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseTargetedMuscles", x => new { x.ExerciseId, x.MuscleGroupId });
                    table.ForeignKey(
                        name: "FK_ExerciseTargetedMuscles_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseTargetedMuscles_MuscleGroups_MuscleGroupId",
                        column: x => x.MuscleGroupId,
                        principalTable: "MuscleGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExerciseTargetedMuscles_MuscleRoles_MuscleRoleId",
                        column: x => x.MuscleRoleId,
                        principalTable: "MuscleRoles",
                        principalColumn: "MuscleRoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutLogSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LogId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    SetOrder = table.Column<int>(type: "integer", nullable: false),
                    RepsCompleted = table.Column<int>(type: "integer", nullable: true),
                    WeightUsedKg = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    DurationCompletedSec = table.Column<int>(type: "integer", nullable: true),
                    DistanceCompletedM = table.Column<int>(type: "integer", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutLogSets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutLogSets_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkoutLogSets_WorkoutLogs_LogId",
                        column: x => x.LogId,
                        principalTable: "WorkoutLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutTemplateExercises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Zone = table.Column<int>(type: "integer", nullable: false),
                    SequenceOrder = table.Column<int>(type: "integer", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutTemplateExercises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkoutTemplateExercises_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkoutTemplateExercises_WorkoutTemplates_WorkoutTemplateId",
                        column: x => x.WorkoutTemplateId,
                        principalTable: "WorkoutTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkoutTemplateObjectives",
                columns: table => new
                {
                    WorkoutTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutObjectiveId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkoutTemplateObjectives", x => new { x.WorkoutTemplateId, x.WorkoutObjectiveId });
                    table.ForeignKey(
                        name: "FK_WorkoutTemplateObjectives_WorkoutObjectives_WorkoutObjectiv~",
                        column: x => x.WorkoutObjectiveId,
                        principalTable: "WorkoutObjectives",
                        principalColumn: "WorkoutObjectiveId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WorkoutTemplateObjectives_WorkoutTemplates_WorkoutTemplateId",
                        column: x => x.WorkoutTemplateId,
                        principalTable: "WorkoutTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SetConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkoutTemplateExerciseId = table.Column<Guid>(type: "uuid", nullable: false),
                    SetNumber = table.Column<int>(type: "integer", nullable: false),
                    TargetReps = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    TargetWeight = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    TargetTimeSeconds = table.Column<int>(type: "integer", nullable: true),
                    RestSeconds = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetConfigurations_WorkoutTemplateExercises_WorkoutTemplateE~",
                        column: x => x.WorkoutTemplateExerciseId,
                        principalTable: "WorkoutTemplateExercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "BodyParts",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "Value" },
                values: new object[,]
                {
                    { new Guid("3e9f8a7d-6c5b-4a3e-8d2f-1b7c9a6d5e4c"), null, 6, true, "Core" },
                    { new Guid("4a6f1b42-5c9b-4c4e-878a-b3d9f2c1f1f5"), null, 3, true, "Legs" },
                    { new Guid("7c5a2d6e-e87e-4c8a-9f1d-9eb734f3df3c"), null, 1, true, "Chest" },
                    { new Guid("9c5f1b4e-2b8a-4c9d-8e7f-c5a9e2d7b8c1"), null, 5, true, "Arms" },
                    { new Guid("b2d89d5c-cb8a-4f5d-8a9e-2c3b76612c5a"), null, 2, true, "Back" },
                    { new Guid("d7e0e24c-f8d4-4b8a-b1e0-cf9c2e6b5d0a"), null, 4, true, "Shoulders" }
                });

            migrationBuilder.InsertData(
                table: "DifficultyLevels",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "Value" },
                values: new object[,]
                {
                    { new Guid("3e27f9a7-d5a5-4f8e-8a76-6de2d23c9a3c"), "Suitable for those with significant fitness experience", 3, true, "Advanced" },
                    { new Guid("8a8adb1d-24d2-4979-a5a6-0d760e6da24b"), "Suitable for those new to fitness", 1, true, "Beginner" },
                    { new Guid("9c7b59a4-bcd8-48a6-971a-cd67b0a7ab5a"), "Suitable for those with some fitness experience", 2, true, "Intermediate" }
                });

            migrationBuilder.InsertData(
                table: "ExecutionProtocols",
                columns: new[] { "ExecutionProtocolId", "Code", "Description", "DisplayOrder", "IntensityLevel", "IsActive", "RepBase", "RestPattern", "TimeBase", "Value" },
                values: new object[,]
                {
                    { new Guid("30000003-3000-4000-8000-300000000001"), "STANDARD", "Standard protocol with balanced rep and time components", 1, "Moderate to High", true, true, "60-90 seconds between sets", true, "Standard" },
                    { new Guid("30000003-3000-4000-8000-300000000002"), "SUPERSET", "Perform exercises back-to-back without rest", 2, "High", true, true, "Rest after completing both exercises", false, "Superset" },
                    { new Guid("30000003-3000-4000-8000-300000000003"), "DROP_SET", "Reduce weight after reaching failure", 3, "Very High", true, true, "Minimal rest between drops", false, "Drop Set" },
                    { new Guid("30000003-3000-4000-8000-300000000004"), "AMRAP", "As Many Reps As Possible in given time", 4, "High", true, false, "Fixed rest periods", true, "AMRAP" }
                });

            migrationBuilder.InsertData(
                table: "ExerciseTypes",
                columns: new[] { "ExerciseTypeId", "Description", "DisplayOrder", "IsActive", "Value" },
                values: new object[,]
                {
                    { new Guid("a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d"), "Exercises performed to prepare the body for more intense activity", 1, true, "Warmup" },
                    { new Guid("b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e"), "Main exercises that form the core of the training session", 2, true, "Workout" },
                    { new Guid("c3d4e5f6-7a8b-9c0d-1e2f-3a4b5c6d7e8f"), "Exercises performed to help the body recover after intense activity", 3, true, "Cooldown" },
                    { new Guid("d4e5f6a7-8b9c-0d1e-2f3a-4b5c6d7e8f9a"), "Periods of rest between exercises or sets", 4, true, "Rest" }
                });

            migrationBuilder.InsertData(
                table: "ExerciseWeightTypes",
                columns: new[] { "Id", "Code", "Description", "DisplayOrder", "IsActive", "Value" },
                values: new object[,]
                {
                    { new Guid("a1f3e2d4-5b6c-4d7e-8f9a-0b1c2d3e4f5a"), "BODYWEIGHT_ONLY", "Exercises that cannot have external weight added", 1, true, "Bodyweight Only" },
                    { new Guid("b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f"), "BODYWEIGHT_OPTIONAL", "Exercises that can be performed with or without additional weight", 2, true, "Bodyweight Optional" },
                    { new Guid("c3d5c4b6-7b8c-6d9e-0f1a-2b3c4d5e6f7a"), "WEIGHT_REQUIRED", "Exercises that must have external weight specified", 3, true, "Weight Required" },
                    { new Guid("d4c6b5a7-8c9d-7e0f-1a2b-3c4d5e6f7a8b"), "MACHINE_WEIGHT", "Exercises performed on machines with weight stacks", 4, true, "Machine Weight" },
                    { new Guid("e5b7a698-9d0e-8f1a-2b3c-4d5e6f7a8b9c"), "NO_WEIGHT", "Exercises that do not use weight as a metric", 5, true, "No Weight" }
                });

            migrationBuilder.InsertData(
                table: "KineticChainTypes",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "Value" },
                values: new object[,]
                {
                    { new Guid("2b3e7cb2-9a3e-4c9a-88d8-b7c019c90d1b"), "Exercises that work a single muscle group", 2, true, "Isolation" },
                    { new Guid("f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4"), "Exercises that work multiple muscle groups", 1, true, "Compound" }
                });

            migrationBuilder.InsertData(
                table: "MuscleRoles",
                columns: new[] { "MuscleRoleId", "Description", "DisplayOrder", "IsActive", "Value" },
                values: new object[,]
                {
                    { new Guid("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d"), "A muscle that helps stabilize the body during the exercise", 3, true, "Stabilizer" },
                    { new Guid("5d8e9a7b-3c2d-4f6a-9b8c-1e5d7f3a2c9b"), "The main muscle targeted by the exercise", 1, true, "Primary" },
                    { new Guid("8c7d6b5a-4e3f-2a1b-9c8d-7e6f5d4c3b2a"), "A muscle that assists in the exercise", 2, true, "Secondary" }
                });

            migrationBuilder.InsertData(
                table: "WorkoutCategories",
                columns: new[] { "WorkoutCategoryId", "Color", "Description", "DisplayOrder", "Icon", "IsActive", "PrimaryMuscleGroups", "Value" },
                values: new object[,]
                {
                    { new Guid("20000002-2000-4000-8000-200000000001"), "#FF5722", "Push exercises targeting chest, shoulders, and triceps", 1, "💪", true, "Chest,Shoulders,Triceps", "Upper Body - Push" },
                    { new Guid("20000002-2000-4000-8000-200000000002"), "#4CAF50", "Pull exercises targeting back and biceps", 2, "🏋️", true, "Back,Biceps", "Upper Body - Pull" },
                    { new Guid("20000002-2000-4000-8000-200000000003"), "#2196F3", "Lower body exercises for legs and glutes", 3, "🦵", true, "Quadriceps,Hamstrings,Glutes,Calves", "Lower Body" },
                    { new Guid("20000002-2000-4000-8000-200000000004"), "#9C27B0", "Core stability and strength exercises", 4, "🎯", true, "Abs,Obliques,Lower Back", "Core" },
                    { new Guid("20000002-2000-4000-8000-200000000005"), "#FF9800", "Compound exercises engaging multiple muscle groups", 5, "🏃", true, "Multiple", "Full Body" }
                });

            migrationBuilder.InsertData(
                table: "WorkoutObjectives",
                columns: new[] { "WorkoutObjectiveId", "Description", "DisplayOrder", "IsActive", "Value" },
                values: new object[,]
                {
                    { new Guid("10000001-1000-4000-8000-100000000001"), "Build maximum strength through heavy loads and low repetitions", 1, true, "Muscular Strength" },
                    { new Guid("10000001-1000-4000-8000-100000000002"), "Increase muscle size through moderate loads and volume", 2, true, "Muscular Hypertrophy" },
                    { new Guid("10000001-1000-4000-8000-100000000003"), "Improve ability to sustain effort over time", 3, true, "Muscular Endurance" },
                    { new Guid("10000001-1000-4000-8000-100000000004"), "Develop explosive strength and speed", 4, true, "Power Development" }
                });

            migrationBuilder.InsertData(
                table: "WorkoutStates",
                columns: new[] { "Id", "Description", "DisplayOrder", "IsActive", "Value" },
                values: new object[,]
                {
                    { new Guid("02000001-0000-0000-0000-000000000001"), "Template under construction", 1, true, "DRAFT" },
                    { new Guid("02000001-0000-0000-0000-000000000002"), "Active template for use", 2, true, "PRODUCTION" },
                    { new Guid("02000001-0000-0000-0000-000000000003"), "Retired template", 3, true, "ARCHIVED" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_UserId",
                table: "Claims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CoachNote_ExerciseId_Order",
                table: "CoachNotes",
                columns: new[] { "ExerciseId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionProtocol_Code",
                table: "ExecutionProtocols",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionProtocol_Value",
                table: "ExecutionProtocols",
                column: "Value");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseBodyParts_BodyPartId",
                table: "ExerciseBodyParts",
                column: "BodyPartId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseEquipment_EquipmentId",
                table: "ExerciseEquipment",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseExerciseTypes_ExerciseTypeId",
                table: "ExerciseExerciseTypes",
                column: "ExerciseTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseLink_Source_Target_Type_Unique",
                table: "ExerciseLinks",
                columns: new[] { "SourceExerciseId", "TargetExerciseId", "LinkType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseLink_SourceExerciseId_LinkType",
                table: "ExerciseLinks",
                columns: new[] { "SourceExerciseId", "LinkType" });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseLink_TargetExerciseId",
                table: "ExerciseLinks",
                column: "TargetExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseMetricSupport_MetricTypeId",
                table: "ExerciseMetricSupport",
                column: "MetricTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseMovementPatterns_MovementPatternId",
                table: "ExerciseMovementPatterns",
                column: "MovementPatternId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseMuscleGroups_MuscleGroupId",
                table: "ExerciseMuscleGroups",
                column: "MuscleGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseMuscleGroups_MuscleRoleId",
                table: "ExerciseMuscleGroups",
                column: "MuscleRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_DifficultyId",
                table: "Exercises",
                column: "DifficultyId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_ExerciseWeightTypeId",
                table: "Exercises",
                column: "ExerciseWeightTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_KineticChainId",
                table: "Exercises",
                column: "KineticChainId");

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_Name",
                table: "Exercises",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseTargetedMuscles_MuscleGroupId",
                table: "ExerciseTargetedMuscles",
                column: "MuscleGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseTargetedMuscles_MuscleRoleId",
                table: "ExerciseTargetedMuscles",
                column: "MuscleRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseWeightType_Code",
                table: "ExerciseWeightTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseWeightType_Value",
                table: "ExerciseWeightTypes",
                column: "Value");

            migrationBuilder.CreateIndex(
                name: "IX_MuscleGroup_Name_IsActive",
                table: "MuscleGroups",
                columns: new[] { "Name", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_MuscleGroups_BodyPartId",
                table: "MuscleGroups",
                column: "BodyPartId");

            migrationBuilder.CreateIndex(
                name: "IX_SetConfiguration_Exercise_SetNumber",
                table: "SetConfigurations",
                columns: new[] { "WorkoutTemplateExerciseId", "SetNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutCategory_Value",
                table: "WorkoutCategories",
                column: "Value");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogs_UserId",
                table: "WorkoutLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogSets_ExerciseId",
                table: "WorkoutLogSets",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutLogSets_LogId",
                table: "WorkoutLogSets",
                column: "LogId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutMuscles_MuscleGroupId",
                table: "WorkoutMuscles",
                column: "MuscleGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutObjective_Value",
                table: "WorkoutObjectives",
                column: "Value");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutTemplateExercise_Template_Zone_Order",
                table: "WorkoutTemplateExercises",
                columns: new[] { "WorkoutTemplateId", "Zone", "SequenceOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutTemplateExercises_ExerciseId",
                table: "WorkoutTemplateExercises",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutTemplateObjectives_WorkoutObjectiveId",
                table: "WorkoutTemplateObjectives",
                column: "WorkoutObjectiveId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutTemplate_CreatedAt",
                table: "WorkoutTemplates",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutTemplate_Name",
                table: "WorkoutTemplates",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutTemplates_CategoryId",
                table: "WorkoutTemplates",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutTemplates_DifficultyId",
                table: "WorkoutTemplates",
                column: "DifficultyId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutTemplates_WorkoutStateId",
                table: "WorkoutTemplates",
                column: "WorkoutStateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Claims");

            migrationBuilder.DropTable(
                name: "CoachNotes");

            migrationBuilder.DropTable(
                name: "ExecutionProtocols");

            migrationBuilder.DropTable(
                name: "ExerciseBodyParts");

            migrationBuilder.DropTable(
                name: "ExerciseEquipment");

            migrationBuilder.DropTable(
                name: "ExerciseExerciseTypes");

            migrationBuilder.DropTable(
                name: "ExerciseLinks");

            migrationBuilder.DropTable(
                name: "ExerciseMetricSupport");

            migrationBuilder.DropTable(
                name: "ExerciseMovementPatterns");

            migrationBuilder.DropTable(
                name: "ExerciseMuscleGroups");

            migrationBuilder.DropTable(
                name: "ExerciseTargetedMuscles");

            migrationBuilder.DropTable(
                name: "SetConfigurations");

            migrationBuilder.DropTable(
                name: "WorkoutLogSets");

            migrationBuilder.DropTable(
                name: "WorkoutMuscles");

            migrationBuilder.DropTable(
                name: "WorkoutTemplateObjectives");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "ExerciseTypes");

            migrationBuilder.DropTable(
                name: "MetricTypes");

            migrationBuilder.DropTable(
                name: "MovementPatterns");

            migrationBuilder.DropTable(
                name: "MuscleRoles");

            migrationBuilder.DropTable(
                name: "WorkoutTemplateExercises");

            migrationBuilder.DropTable(
                name: "WorkoutLogs");

            migrationBuilder.DropTable(
                name: "MuscleGroups");

            migrationBuilder.DropTable(
                name: "WorkoutObjectives");

            migrationBuilder.DropTable(
                name: "Exercises");

            migrationBuilder.DropTable(
                name: "WorkoutTemplates");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "BodyParts");

            migrationBuilder.DropTable(
                name: "ExerciseWeightTypes");

            migrationBuilder.DropTable(
                name: "KineticChainTypes");

            migrationBuilder.DropTable(
                name: "DifficultyLevels");

            migrationBuilder.DropTable(
                name: "WorkoutCategories");

            migrationBuilder.DropTable(
                name: "WorkoutStates");
        }
    }
}
