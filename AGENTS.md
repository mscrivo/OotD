# Agent Notes

## Cutting a release

A release spans this repo and the website repo (`../outlookonthedesktop.com`), which hosts the appcast the installed app polls for updates.

1. **Pick the version** (`X.Y.Z`): patch for fixes/dependency/runtime bumps, minor for new features.
2. **Bump in this repo**, on `main`:
   - `version.txt` → `X.Y.Z` (no trailing newline; CI reads it as the app version).
   - `CHANGELOG.md`: rename `## [Unreleased]` to `## [X.Y.Z] - YYYY-MM-DD` and add a fresh empty `## [Unreleased]` above it. Entries are user-facing (Keep a Changelog: Added / Changed / Fixed). Mention a .NET runtime bump (with security fixes) under Changed; skip dev-only dependency bumps.
   - Commit `chore: bump version to X.Y.Z` and push to `main`.
3. **Wait for the `build` workflow** on that commit (`gh run watch`). It signs the app and installer via SignPath and uploads the signed `ootd-X.Y.Z.exe` as a run artifact.
4. **Create the GitHub release** with tag `X.Y.Z` on `main`, title `X.Y.Z`, attaching the signed installer:

   ```sh
   # The installer artifact is uploaded with archive: false, so `gh run download` fails ("not a valid zip"); fetch it raw:
   aid=$(gh api repos/mscrivo/OotD/actions/runs/<run-id>/artifacts --jq '.artifacts[]|select(.name=="ootd-X.Y.Z.exe")|.id')
   gh api repos/mscrivo/OotD/actions/artifacts/$aid/zip > <tmp-dir>/ootd-X.Y.Z.exe
   # Sanity check: Get-AuthenticodeSignature should be Valid, signed by SignPath Foundation
   gh release create X.Y.Z <tmp-dir>/ootd-X.Y.Z.exe --target main --title X.Y.Z --notes-file <notes.md>
   ```

   Release notes body format (copy the previous release, `gh release view`):

   ```md
   > [!IMPORTANT]
   > OotD only works with Outlook Classic, not the "new" Outlook that Microsoft is pushing on everyone. If you have the "new" Outlook, you will need to switch back to the classic version to use OotD.

   ### New ###
   ### Changes ###
   ### Fixes ###

   **Full Changelog**: https://github.com/mscrivo/OotD/compare/<prev>...X.Y.Z
   ```

   Omit empty sections.
5. **Update the website** (`../outlookonthedesktop.com`), one commit `chore: release X.Y.Z`, push to `main` (CI deploys):
   - `public/ootdAppcast.xml`: `<title>Version X.Y.Z</title>`, `<pubDate>` = the release's `publishedAt` in RFC 822 (`Sat, 03 Oct 2026 15:31:32 +0000`), enclosure `url`, `sparkle:version`, and `length` = installer size in bytes (`gh release view X.Y.Z --json publishedAt,assets`).
   - `public/ootdReleaseNotes.md`: prepend a `## X.Y.Z` section (same Important note, then `New:` / `Changes:` / `Fixes:` lists).
   - `src/data/site.ts`: `latestVersion`, `releaseDate` (`YYYY-MM-DD`), `downloadUrl`.
