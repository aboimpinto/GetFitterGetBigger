#!/bin/bash
# Status Dashboard for GetFitterGetBigger Development

echo "================================================"
echo "GetFitterGetBigger Development Status Dashboard"
echo "================================================"
echo ""

# Current PI
echo "📅 Current PI: $(grep -A1 "Current PI" releases/CURRENT_PI.md | tail -1 | sed 's/.*: //')"
echo ""

# Features Status
echo "✨ FEATURES STATUS"
echo "=================="
echo "  📋 Ready to Develop: $(ls -1 features/1-READY_TO_DEVELOP 2>/dev/null | wc -l)"
if [ -d "features/1-READY_TO_DEVELOP" ] && [ "$(ls -A features/1-READY_TO_DEVELOP)" ]; then
    for feature in features/1-READY_TO_DEVELOP/*; do
        [ -d "$feature" ] && echo "     - $(basename "$feature")"
    done
fi

echo "  🚧 In Progress: $(ls -1 features/2-IN_PROGRESS 2>/dev/null | wc -l)"
if [ -d "features/2-IN_PROGRESS" ] && [ "$(ls -A features/2-IN_PROGRESS)" ]; then
    for feature in features/2-IN_PROGRESS/*; do
        [ -d "$feature" ] && echo "     - $(basename "$feature")"
    done
fi

echo "  ✅ Completed: $(ls -1 features/3-COMPLETED 2>/dev/null | wc -l)"
if [ -d "features/3-COMPLETED" ] && [ "$(ls -A features/3-COMPLETED)" ]; then
    for feature in features/3-COMPLETED/*; do
        [ -d "$feature" ] && echo "     - $(basename "$feature")"
    done
fi

echo "  🚫 Blocked: $(ls -1 features/4-BLOCKED 2>/dev/null | wc -l)"
if [ -d "features/4-BLOCKED" ] && [ "$(ls -A features/4-BLOCKED)" ]; then
    for feature in features/4-BLOCKED/*; do
        [ -d "$feature" ] && echo "     - $(basename "$feature")"
    done
fi

echo "  ⏭️  Skipped: $(ls -1 features/5-SKIPPED 2>/dev/null | wc -l)"
echo ""

# Bugs Status
echo "🐛 BUGS STATUS"
echo "=============="
echo "  🆕 Open: $(ls -1 bugs/1-OPEN 2>/dev/null | wc -l)"
if [ -d "bugs/1-OPEN" ] && [ "$(ls -A bugs/1-OPEN)" ]; then
    for bug in bugs/1-OPEN/*; do
        [ -d "$bug" ] && echo "     - $(basename "$bug")"
    done
fi

echo "  🔧 In Progress: $(ls -1 bugs/2-IN_PROGRESS 2>/dev/null | wc -l)"
if [ -d "bugs/2-IN_PROGRESS" ] && [ "$(ls -A bugs/2-IN_PROGRESS)" ]; then
    for bug in bugs/2-IN_PROGRESS/*; do
        [ -d "$bug" ] && echo "     - $(basename "$bug")"
    done
fi

echo "  ✔️  Fixed: $(ls -1 bugs/3-FIXED 2>/dev/null | wc -l)"
if [ -d "bugs/3-FIXED" ] && [ "$(ls -A bugs/3-FIXED)" ]; then
    for bug in bugs/3-FIXED/*; do
        [ -d "$bug" ] && echo "     - $(basename "$bug")"
    done
fi

echo "  🚫 Blocked: $(ls -1 bugs/4-BLOCKED 2>/dev/null | wc -l)"
echo "  ❌ Won't Fix: $(ls -1 bugs/5-WONT_FIX 2>/dev/null | wc -l)"
echo ""

# Next Bug ID
echo "📝 Next Bug ID: $(cat bugs/NEXT_BUG_ID.txt 2>/dev/null || echo 'BUG-001')"
echo ""

# Summary
echo "📊 SUMMARY"
echo "=========="
total_features=$(($(ls -1 features/1-READY_TO_DEVELOP 2>/dev/null | wc -l) + \
                  $(ls -1 features/2-IN_PROGRESS 2>/dev/null | wc -l) + \
                  $(ls -1 features/3-COMPLETED 2>/dev/null | wc -l) + \
                  $(ls -1 features/4-BLOCKED 2>/dev/null | wc -l) + \
                  $(ls -1 features/5-SKIPPED 2>/dev/null | wc -l)))

total_bugs=$(($(ls -1 bugs/1-OPEN 2>/dev/null | wc -l) + \
              $(ls -1 bugs/2-IN_PROGRESS 2>/dev/null | wc -l) + \
              $(ls -1 bugs/3-FIXED 2>/dev/null | wc -l) + \
              $(ls -1 bugs/4-BLOCKED 2>/dev/null | wc -l) + \
              $(ls -1 bugs/5-WONT_FIX 2>/dev/null | wc -l)))

echo "  Total Features: $total_features"
echo "  Total Bugs: $total_bugs"
echo "  Ready for Release: $(($(ls -1 features/3-COMPLETED 2>/dev/null | wc -l) + $(ls -1 bugs/3-FIXED 2>/dev/null | wc -l))) items"
echo ""

echo "================================================"
echo "Run from: memory-bank/"
echo "Generated: $(date)"
echo "================================================"