# v1.5 validation

Unity Play Mode checks cover 100 seeded task selections, distinct tasks, capped progress, exactly-once rewards, star threshold boundaries, pause, collection/pulse/capture event integration, persistence across injury, restart resets, defeat protection for saved stars, both levels, and UI text bounds.

Screenshots include task HUD, victory, defeat and saved stars on level selection. Runs preserve and restore existing score, time and star preferences. Test victories use deterministic event calls and controlled timing; they are not manual speed-run balance tests. Time targets may be tuned after player feedback.

Final results: 56 checks passed at 1280x720 and 56 at 1024x768, both TOTAL_ERRORS=0. Screenshots reviewed after final layout correction.
