## 2026-01-15 — Phase 1A: Camera System

### Added
- Implemented a basic 2D camera system (position + zoom) using a MonoGame transform matrix
- Added keyboard movement (WASD / Arrow Keys)
- Added mouse wheel zoom with clamped min/max zoom

	### Notes
	- Camera movement speed is adjusted by zoom so navigation feels consistent when zoomed in/out
	- Camera uses a view matrix so all draw calls can stay in world coordinates

	### Next
	- Add debug overlay: FPS, camera position, mouse screen/world coordinates
	- Draw a simple grid/tile map to visualize movement

## 2026-01-25 - Phase 1B : Seed and Map generation
- implemented noise map generation using character-sensitive SEEDS
- same words will produce different seeds if the case of any one letter is different
- SEED is divided into 2 : sky and earth

- added map preview
- restructured world design to be one massive island instead of one endless map.
- map preview shows any one edge or corner of the world