# /commit-feature

Commits changes, pushes to remote, and updates the current feature's checkpoint with the commit hash.

## Usage

```
/commit-feature [feature] [checkpoint] [highlights]
```

## Parameters

- `feature` (optional): Feature identifier (e.g., "FEAT-030" or "030")
- `checkpoint` (optional): Checkpoint name (e.g., "Phase 3" or leave empty for auto-detect)
- `highlights` (optional): Key changes to emphasize in commit message

## Examples

```bash
# Auto-detect everything
/commit-feature

# Specify feature only
/commit-feature FEAT-030

# Specify feature and checkpoint
/commit-feature FEAT-030 "Phase 3"

# With commit highlights
/commit-feature FEAT-030 "Phase 3" "fixed validation, added error handling, refactored delete methods"

# Auto-detect with highlights only
/commit-feature "" "" "major refactoring of service layer"
```

## Implementation

```javascript
async function handleCommitFeature(args) {
    // Parse arguments
    const [feature, checkpoint, ...highlightParts] = args.split(' ');
    const highlights = highlightParts.join(' ');
    
    // Build prompt for agent
    let prompt = "Commit current changes, push to remote, and update the feature checkpoint.\n\n";
    
    if (feature && feature !== '""' && feature !== "''") {
        prompt += `Feature: ${feature}\n`;
    }
    
    if (checkpoint && checkpoint !== '""' && checkpoint !== "''") {
        prompt += `Checkpoint: ${checkpoint}\n`;
    }
    
    if (highlights) {
        prompt += `Highlights for commit: ${highlights}\n`;
    }
    
    // Launch the feature-commit-push agent
    await launchAgent('feature-commit-push', prompt);
}
```

## Agent Integration

This command launches the `feature-commit-push` agent which will:

1. **Analyze current git status** to understand what changes need to be committed
2. **Identify the feature and checkpoint** (auto-detect if not provided)
3. **Create a meaningful commit message** incorporating any highlights
4. **Commit and push** the changes
5. **Update the feature checkpoint** with the commit hash and summary

## Workflow

1. User runs `/commit-feature` with optional parameters
2. Command launches the feature-commit-push agent
3. Agent analyzes the repository state
4. Agent creates and pushes the commit
5. Agent updates the feature documentation
6. User receives confirmation with commit details

## Benefits

- **Streamlined workflow**: Single command for commit, push, and documentation
- **Automatic tracking**: Commit hashes automatically added to feature checkpoints
- **Flexible**: Works with or without parameters
- **Consistent**: Ensures proper commit message format and documentation updates
- **Time-saving**: Eliminates manual checkpoint updates

## Related Commands

- `/review-feature`: Review code for a feature
- `/feature-status`: Check current feature progress
- `/git-status`: Check git repository status