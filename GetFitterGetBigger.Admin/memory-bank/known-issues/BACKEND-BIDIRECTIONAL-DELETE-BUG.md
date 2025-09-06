# Backend Bidirectional Delete Bug

## Issue Description
The backend API's `deleteReverse=True` parameter does not correctly delete bidirectional links for Warmup/Cooldown/Workout relationships.

## Status
**RESOLVED** - Discovered 2025-09-06, Fixed in Backend 2025-09-06

### Resolution
Backend fix implemented to properly handle `deleteReverse=True` parameter. The endpoint now correctly deletes both forward and reverse links in a single atomic transaction.

## Symptoms
1. When deleting a workout link from a warmup exercise using `DELETE /api/exercises/{exerciseId}/links/{linkId}?deleteReverse=True`
2. The API returns 204 (success)
3. The link is removed from the warmup exercise
4. BUT the reverse link (warmup from workout's perspective) is NOT deleted
5. This leaves orphaned links in the database

## Steps to Reproduce
1. Create a warmup link: Workout → Warmup (creates bidirectional links)
2. Navigate to the warmup exercise and verify workout appears
3. Delete the workout link from the warmup using bidirectional deletion
4. Navigate to the workout - the warmup link is still present (should be deleted)

## Evidence from Logs
```
[ExerciseLinkService] DeleteBidirectionalLinkAsync called
[ExerciseLinkService] Request URL: api/exercises/exercise-950e8400-e29b-41d4-a716-446655440101/links/exerciselink-301a6ecf-e5ea-4710-85d6-e1872e124920?deleteReverse=True
Response: 204 NoContent (Success)

// But when navigating to the workout:
[ExerciseLinkService] GetLinksAsync - Parsed response:
  - Links Count: 1
    - Link: WARMUP to exercise-950e8400-e29b-41d4-a716-446655440101
    // This link should have been deleted!
```

## Root Cause
The backend endpoint `/api/exercises/{exerciseId}/links/{linkId}?deleteReverse=True` is not properly handling the `deleteReverse` parameter for non-Alternative link types.

## Impact
- Users must manually delete links from both exercises
- Data integrity issues with orphaned links
- Confusing UX where deleted relationships still appear

## Workaround (Frontend)
Instead of relying on `deleteReverse=True`, manually delete both link IDs:

```csharp
// Workaround: Delete both directions manually
public async Task DeleteBidirectionalLinkWorkaround(string exerciseId, string linkId)
{
    // 1. Get the link details to find the reverse link
    var links = await GetLinksAsync(exerciseId, includeReverse: true);
    var linkToDelete = links.Links.FirstOrDefault(l => l.Id == linkId);
    var targetExerciseId = linkToDelete?.TargetExerciseId;
    
    // 2. Delete the forward link
    await DeleteLinkAsync(exerciseId, linkId);
    
    if (!string.IsNullOrEmpty(targetExerciseId))
    {
        // 3. Get the reverse link ID
        var reverseLinks = await GetLinksAsync(targetExerciseId);
        var reverseLink = reverseLinks.Links.FirstOrDefault(l => 
            l.TargetExerciseId == exerciseId && 
            IsReverseType(linkToDelete.LinkType, l.LinkType));
        
        // 4. Delete the reverse link
        if (reverseLink != null)
        {
            await DeleteLinkAsync(targetExerciseId, reverseLink.Id);
        }
    }
}

private bool IsReverseType(string originalType, string reverseType)
{
    return (originalType, reverseType) switch
    {
        ("WARMUP", "WORKOUT") => true,
        ("WORKOUT", "WARMUP") => true,
        ("COOLDOWN", "WORKOUT") => true,
        ("WORKOUT", "COOLDOWN") => true,
        ("ALTERNATIVE", "ALTERNATIVE") => true,
        _ => false
    };
}
```

## Backend Fix Required
The backend needs to:
1. When `deleteReverse=True` is passed
2. Find the corresponding reverse link in the database
3. Delete both the forward and reverse links in a transaction
4. Return success only if both are deleted

## Related Issues
- Similar to BACKEND-ALTERNATIVE-LINKS-BUG.md but affects all link types
- May be related to how the backend creates automatic reverse links