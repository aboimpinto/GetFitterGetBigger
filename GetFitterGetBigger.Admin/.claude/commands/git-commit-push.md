# Git Commit and Push

Please help me commit and push my changes to the remote repository.

## Process:
1. **Check Current Status**
   - Run `git status` to see all modified, staged, and untracked files
   - Run `git diff --stat` to see a summary of changes
   - Check current branch and ensure it's the correct one

2. **Review Changes**
   - Show me a summary of what files have been modified
   - If there are many changes, categorize them by type (e.g., bug fixes, features, refactoring, tests, docs)
   - Identify if any sensitive information might be accidentally committed

3. **Stage Files**
   - Stage all relevant files (excluding any that shouldn't be committed)
   - If there are untracked files, ask me if they should be included
   - Respect .gitignore patterns

4. **Commit Message**
   - Follow the repository's commit message convention (check recent commits with `git log`)
   - Use conventional commit format: type(scope): description
   - Common types: feat, fix, docs, style, refactor, test, chore
   - Include a detailed body if changes are complex
   - Add the required signature per CLAUDE.md

5. **Pre-Push Checks**
   - Ensure the commit was successful
   - Check if we need to pull latest changes first
   - Verify we're pushing to the correct remote and branch

6. **Push Changes**
   - Push to the appropriate remote repository
   - Handle any conflicts or errors that may occur
   - Confirm successful push

## Important Notes:
- If there are uncommitted changes in multiple areas, consider if they should be separate commits
- Always ensure no secrets, API keys, or sensitive data are being committed
- If the push fails due to conflicts, help me resolve them
- If working on a feature branch, ask if I want to push to that branch or create a PR

## Optional Parameters:
- If I specify "all" - stage all modified files
- If I specify "amend" - amend the previous commit
- If I specify a commit message - use it directly
- If I specify "force" - use with caution and confirm first