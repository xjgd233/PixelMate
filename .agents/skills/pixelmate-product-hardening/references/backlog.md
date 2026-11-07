# PixelMate Product Hardening Backlog

Prepared from the macOS `1.0.0` audit on 2026-11-05. Work in this order unless
the user explicitly selects another item. Each item is intentionally narrow.

## P0-1 Unsaved-change protection

- [ ] Mark the current image as dirty when measurements or annotations change.
- [ ] Clear the dirty state after a successful export, and keep it correct after
  undo/redo.
- [ ] Before replacing the current image or closing the window, ask whether to
  save, discard, or cancel when edits are unsaved.
- [ ] Show a persistent unsaved indicator without changing the image title.

Focus: `src/PixelMate/MainWindow.axaml.cs`,
`src/PixelMate/MainWindow.axaml`, `src/PixelMate/Controls/ImageCanvas.cs`.
Likely tests: MainWindow edit-state logic extracted into a small testable helper,
or focused coverage around the state transition used by the window.

Done when: clean images open and close without prompts; dirty images cannot be
discarded accidentally; save, undo, and redo keep the indicator accurate.

## P0-2 macOS release package

- [ ] Add a repeatable macOS ARM64 release script that produces a self-contained
  `.app` and compressed user deliverable.
- [ ] Include the icon, product name, version, and required bundle metadata.
- [ ] Prefer a `.dmg` or ZIP with a simple launch path; do not require .NET SDK
  on the end-user machine.
- [ ] Verify the packaged app launches and opens an image.
- [ ] Leave Developer ID signing and notarization as optional credential-driven
  steps if no certificate is available; report exactly what remains.

Focus: `scripts/macos/`, `scripts/run-macos.sh`, `src/PixelMate/PixelMate.csproj`,
`Directory.Build.targets`.

Done when: one command generates a fresh macOS ARM64 artifact from a clean
Release build, and the artifact launches outside the development checkout.

## P1-1 Editable, deletable annotations

- [ ] Let the user select an existing annotation without losing zoom/pan state.
- [ ] Edit its text, move its anchor, and delete only that annotation.
- [ ] Preserve undo/redo and update counts/actions correctly.
- [ ] Keep coordinates anchored to the original image.

Focus: `src/PixelMate/Annotations/PixelAnnotation.cs`,
`src/PixelMate/Controls/ImageCanvas.cs`, `src/PixelMate/MainWindow.axaml*`,
`tests/PixelMate.Tests/PixelAnnotationTests.cs`.

Done when: two annotations can be independently edited or deleted; undo/redo
restores each operation; export remains accurate.

## P1-2 Rectangle and area measurement

- [ ] Add a rectangle mode alongside line measurement.
- [ ] Show width, height, area, and original-image coordinates.
- [ ] Keep Shift axis locking and existing line behavior unchanged.
- [ ] Include rectangle values in copy/export and undo/redo.

Focus: `src/PixelMate/Measurement/`,
`src/PixelMate/Controls/ImageCanvas.cs`, relevant MainWindow toolbar/status code.

Done when: the same rectangle remains correct after zoom, pan, resize, reversal,
and export.

## P1-3 Color history and practical formats

- [ ] Keep a small recent-color history for the current image/session.
- [ ] Allow one-click copying of HEX and RGB from the history.
- [ ] Add HSL/HSV display only if it can reuse a small tested conversion helper.
- [ ] Preserve alpha handling for transparent pixels.

Focus: `src/PixelMate/Colors/`, `src/PixelMate/MainWindow.axaml*`, and focused
color tests. Do not build a full palette generator or scene-preview suite.

Done when: repeated inspections are faster, history is capped, and transparent
colors do not lose their RGB channels.

## P1-4 Tool-specific cursors

- [ ] Use visibly distinct cursors for measurement, eyedropper, magnifier, and
  annotation while retaining the temporary-pan cursor.
- [ ] Keep platform fallbacks if a custom cursor cannot load.

Focus: `src/PixelMate/Controls/ImageCanvas.cs` and existing resources.

Done when: pointer mode is distinguishable without reading the toolbar, and
no cursor exception affects image opening.

## P2-1 CI and repository hygiene

- [ ] Add a GitHub Actions workflow that restores, builds, and tests the solution.
- [ ] Add a license only after the user explicitly chooses one.
- [ ] Add issue/PR support only if the repository is opened to contributions.

Done when: a pushed change gets a reproducible build/test result without needing
local Windows hardware.

## P2-2 Desktop integration

- [ ] Add a recent-files entry point or native “Open With” association if it fits
  the current UI.
- [ ] Avoid adding image tabs or batch-management features until requested.

Done when: opening a known file takes fewer steps without changing the compact
single-image workflow.

## P2-3 Release presentation

- [ ] Replace the static hero screenshot with a short real-action demo GIF only
  after core workflow changes settle.
- [ ] Add release notes and a downloadable GitHub Release once macOS artifacts
  pass smoke testing.

Keep the README product-focused. Do not reintroduce internal phase history.

## Research decisions already made

- Model the concise README flow on StrangeLens and ShareX.
- Use use-case tables and feature evidence, but avoid Color Card's long
  development-history block.
- Bilibili users value “lightweight, fast, no Photoshop required”; preserve that
  promise. PixPin-style OCR, long screenshots, and recording remain out of scope.

Sources:

- <https://github.com/somethingSTRANGE/StrangeLens>
- <https://github.com/ShareX/ShareX>
- <https://github.com/qingshangongzai/Color_Card>
- <https://www.bilibili.com/video/BV1Cd4y167ym/>
- <https://www.bilibili.com/video/BV1Vu4y1G7Uw/>
