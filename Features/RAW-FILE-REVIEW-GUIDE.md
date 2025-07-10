# RAW File Review Guide

## Purpose
This guide defines the review process for RAW feature files to ensure they maintain technology independence and focus on business requirements rather than implementation details.

## Core Principles

### 1. True Technology Independence
RAW files must describe features without assuming any specific technology stack, database, or framework.

### 2. Focus on "What" not "How"
Document WHAT the business needs, not HOW to implement it.

### 3. Reduce Confusion
Avoid implementation details that might conflict with actual project choices.

### 4. Better for Evolution
Features should remain valid regardless of technology changes.

### 5. Clearer Intent
Use business language, not technical jargon.

## Review Checklist

### ❌ AVOID - Technology-Specific Patterns

#### Database Specifics
```
❌ BAD:  Id INT PRIMARY KEY
❌ BAD:  Code VARCHAR(50) NOT NULL
❌ BAD:  CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP

✅ GOOD: Identifier (unique)
✅ GOOD: Code (unique, immutable)
✅ GOOD: Creation timestamp
```

#### Data Types
```
❌ BAD:  "id": "string (guid)"
❌ BAD:  "weight": "decimal(10,2)"
❌ BAD:  "isActive": "boolean"

✅ GOOD: Identifier
✅ GOOD: Weight value
✅ GOOD: Active indicator
```

#### API Specifics
```
❌ BAD:  GET /api/exercises/{id}
❌ BAD:  POST /api/workouts
❌ BAD:  Returns 200 OK with JSON

✅ GOOD: Retrieve exercise by identifier
✅ GOOD: Create new workout
✅ GOOD: Returns success with exercise data
```

#### Code Syntax
```
❌ BAD:  if (exercise.weightType == "BODYWEIGHT_ONLY") { ... }
❌ BAD:  public class ExerciseWeightType { ... }
❌ BAD:  SELECT * FROM Exercises WHERE ...

✅ GOOD: If exercise type is BODYWEIGHT_ONLY then...
✅ GOOD: ExerciseWeightType entity contains...
✅ GOOD: Retrieve exercises where...
```

### ✅ USE - Business-Focused Patterns

#### Entity Descriptions
```
ExerciseWeightType:
  - Identifier (unique)
  - Code (unchangeable reference)
  - Display name
  - Description
  - Active status
  - Display order
```

#### Relationships
```
Exercise:
  - Has one ExerciseWeightType (required)
  
ExerciseWeightType:
  - Can be referenced by many Exercises
```

#### Business Rules
```
When validating weight for an exercise:
  If weight type is BODYWEIGHT_ONLY:
    → Weight must be empty or zero
  If weight type is WEIGHT_REQUIRED:
    → Weight must be greater than zero
```

#### Operations
```
Retrieve All Weight Types
  - Purpose: List available weight types
  - Access: Public
  - Returns: Collection of weight types

Create Workout Entry
  - Purpose: Log exercise performance
  - Access: Authenticated users
  - Validates: Weight rules based on exercise type
```

## Review Process

### Step 1: Initial Scan
Quick review for obvious technology dependencies:
- [ ] No SQL statements
- [ ] No specific data types (int, string, guid)
- [ ] No API paths or HTTP methods
- [ ] No programming language syntax
- [ ] No framework-specific terms

### Step 2: Language Review
Ensure business-friendly language:
- [ ] Would a non-technical stakeholder understand this?
- [ ] Are technical terms explained in business context?
- [ ] Is the value/purpose clear without implementation details?

### Step 3: Completeness Check
Verify all business aspects are covered:
- [ ] Business problem clearly stated
- [ ] Solution approach described
- [ ] All entities and relationships defined
- [ ] Business rules documented
- [ ] Operations/capabilities listed
- [ ] Success metrics included

### Step 4: Independence Test
Ask these questions:
- [ ] Could this be implemented in any programming language?
- [ ] Could this work with any database type?
- [ ] Would this survive a complete technology rewrite?
- [ ] Are the business rules clear without code?

## Examples

### ❌ Poor Example (Too Technical)
```
The ExerciseWeightType table uses an integer primary key with 
foreign key constraints to the Exercises table. The API returns 
JSON responses with HTTP 200 status codes.
```

### ✅ Good Example (Business-Focused)
```
Each exercise must be assigned a weight type that determines 
how weight can be recorded. The system enforces these rules 
when users log their workout performance.
```

### ❌ Poor Example (Implementation Leak)
```json
{
  "id": "guid",
  "code": "string(50)",
  "name": "nvarchar(100)",
  "isActive": "bit"
}
```

### ✅ Good Example (Conceptual Model)
```
ExerciseWeightType consists of:
- Unique identifier
- Immutable code for system reference
- Display name for users
- Active/inactive status
```

## Common Pitfalls

### 1. HTTP/REST Assumptions
Instead of "GET /api/..." use "Retrieve..." or "List..."

### 2. Database Assumptions  
Instead of "JOIN tables" use "Related to..." or "Associated with..."

### 3. Type Assumptions
Instead of "string ID" use "identifier" or "reference"

### 4. Format Assumptions
Instead of "returns JSON" use "returns data" or "provides information"

### 5. Authentication Assumptions
Instead of "JWT token required" use "requires authentication" or "authenticated users only"

## Benefits of This Approach

1. **Longevity**: Documentation remains valid through technology changes
2. **Clarity**: Business stakeholders can understand features
3. **Flexibility**: Implementation teams have freedom to use best practices
4. **Consistency**: All teams understand the same business requirements
5. **Maintenance**: Fewer updates needed when technology changes

## When to Update RAW Files

RAW files should only be updated when:
- Business requirements change
- New capabilities are added
- Business rules are modified
- Relationships between entities change

RAW files should NOT be updated when:
- Technology stack changes
- API design patterns change
- Database schema is optimized
- Implementation details are refactored

---

Remember: RAW files document the BUSINESS FEATURE, not the technical implementation!