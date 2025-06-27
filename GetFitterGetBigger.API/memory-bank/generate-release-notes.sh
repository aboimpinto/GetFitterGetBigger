#!/bin/bash
# Generate Release Notes for a Product Increment

if [ -z "$1" ]; then
    echo "Usage: ./generate-release-notes.sh [PI-YYYY-QX]"
    echo "Example: ./generate-release-notes.sh PI-2025-Q1"
    exit 1
fi

PI=$1
PI_DIR="releases/$PI"
OUTPUT="$PI_DIR/RELEASE_NOTES.md"

# Check if PI directory exists
if [ ! -d "$PI_DIR" ]; then
    echo "Error: PI directory $PI_DIR does not exist"
    echo "Creating PI directory..."
    mkdir -p "$PI_DIR"
fi

# Prepare release by copying completed items
echo "Preparing release for $PI..."

# Copy completed features
if [ -d "features/3-COMPLETED" ] && [ "$(ls -A features/3-COMPLETED)" ]; then
    echo "Copying completed features..."
    mkdir -p "$PI_DIR/features"
    cp -r features/3-COMPLETED/* "$PI_DIR/features/" 2>/dev/null || true
fi

# Copy fixed bugs
if [ -d "bugs/3-FIXED" ] && [ "$(ls -A bugs/3-FIXED)" ]; then
    echo "Copying fixed bugs..."
    mkdir -p "$PI_DIR/bugs"
    cp -r bugs/3-FIXED/* "$PI_DIR/bugs/" 2>/dev/null || true
fi

# Generate release notes
echo "Generating release notes..."

cat > "$OUTPUT" << EOF
# Release Notes - $PI

## Version: [X.Y.Z]
## Release Date: $(date +%Y-%m-%d)
## Code Name: [TBD]

## 🎯 Release Highlights
This release includes $(ls -1 "$PI_DIR/features" 2>/dev/null | wc -l) new features and $(ls -1 "$PI_DIR/bugs" 2>/dev/null | wc -l) bug fixes.

## ✨ New Features

EOF

# Add features
if [ -d "$PI_DIR/features" ] && [ "$(ls -A $PI_DIR/features)" ]; then
    for feature_dir in "$PI_DIR/features"/*; do
        if [ -d "$feature_dir" ]; then
            feature_name=$(basename "$feature_dir")
            echo "### $(echo $feature_name | sed 's/-/ /g' | sed 's/\b\(.\)/\u\1/g')" >> "$OUTPUT"
            
            # Try to extract description from feature-description.md
            if [ -f "$feature_dir/feature-description.md" ]; then
                # Extract description section
                desc=$(grep -A3 "## Description" "$feature_dir/feature-description.md" 2>/dev/null | tail -n +2 | head -3 | sed 's/^/- /')
                if [ ! -z "$desc" ]; then
                    echo "$desc" >> "$OUTPUT"
                else
                    echo "- Feature implementation completed" >> "$OUTPUT"
                fi
            fi
            echo "" >> "$OUTPUT"
        fi
    done
else
    echo "No new features in this release." >> "$OUTPUT"
    echo "" >> "$OUTPUT"
fi

echo "## 🐛 Bug Fixes" >> "$OUTPUT"
echo "" >> "$OUTPUT"

# Add bugs
if [ -d "$PI_DIR/bugs" ] && [ "$(ls -A $PI_DIR/bugs)" ]; then
    # Group by severity if possible
    echo "### Fixed Issues" >> "$OUTPUT"
    for bug_dir in "$PI_DIR/bugs"/*; do
        if [ -d "$bug_dir" ]; then
            bug_id=$(basename "$bug_dir" | cut -d'-' -f1-2)
            
            # Try to extract description from bug-report.md
            if [ -f "$bug_dir/bug-report.md" ]; then
                # Extract description
                title=$(grep "^# BUG-" "$bug_dir/bug-report.md" 2>/dev/null | sed 's/# BUG-[0-9]*: //')
                if [ ! -z "$title" ]; then
                    echo "- **$bug_id**: $title" >> "$OUTPUT"
                else
                    echo "- **$bug_id**: Bug fix implemented" >> "$OUTPUT"
                fi
            else
                echo "- **$bug_id**: Bug fix implemented" >> "$OUTPUT"
            fi
        fi
    done
else
    echo "No bug fixes in this release." >> "$OUTPUT"
fi

cat >> "$OUTPUT" << EOF

## 📊 Technical Improvements
- Code quality improvements
- Performance optimizations
- Test coverage enhancements

## 🔄 Breaking Changes
None in this release.

## 📋 Known Issues
Please refer to the open bugs in the bug tracking system.

## 🚀 Upgrade Instructions
1. Backup your database
2. Pull the latest code
3. Run database migrations: \`dotnet ef database update\`
4. Update configuration files if needed
5. Deploy the application
6. Run verification tests

## 📈 Release Metrics
- Features Delivered: $(ls -1 "$PI_DIR/features" 2>/dev/null | wc -l)
- Bugs Fixed: $(ls -1 "$PI_DIR/bugs" 2>/dev/null | wc -l)
- Total Items: $(($(ls -1 "$PI_DIR/features" 2>/dev/null | wc -l) + $(ls -1 "$PI_DIR/bugs" 2>/dev/null | wc -l)))

## 🙏 Acknowledgments
Thanks to all contributors who made this release possible.

## 📅 Next Release Preview
Stay tuned for exciting new features in the next PI!

---
*Generated on $(date)*
EOF

echo ""
echo "✅ Release notes generated successfully!"
echo "📄 Output: $OUTPUT"
echo ""
echo "Next steps:"
echo "1. Review and edit the release notes in $OUTPUT"
echo "2. Update version number and code name"
echo "3. Add specific details for features and bugs"
echo "4. Create release branch and tag"
echo "5. Move completed items to archive after release"