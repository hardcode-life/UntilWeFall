## [2026-01-15] Phase 1A: Camera System
- Implemented a basic 2D camera system (position + zoom) using a MonoGame transform matrix
- Added keyboard movement (WASD / Arrow Keys)
- Added mouse wheel zoom with clamped min/max zoom

	# Notes
	- Camera movement speed is adjusted by zoom so navigation feels consistent when zoomed in/out
	- Camera uses a view matrix so all draw calls can stay in world coordinates

### [2026-01-25] Phase 1B : Seed and Map generation
- implemented noise map generation using character-sensitive SEEDS
- same words will produce different seeds if the case of any one letter is different
- SEED is divided into 2 : sky and earth

- added map preview
- restructured world design to be one massive island instead of one endless map.
- map preview shows any one edge or corner of the world

- updated map preview filter:
	+ Each time the player commits a seed:
	+ Choose a random "window" (64×64) somewhere that includes an edge or corner
	+ Generate noise for that window
	+ Apply mask(s)
	+ Classify each cell as land vs water
	+ Compute land percent
		+ If (land% >= 45%) → accept (set spawn + preview)
		+ Else reroll a different edge window (limited attempts)

	# future plans:
	- implement single-seed variants
	- Spawn Archetypes (continent, archipelago, peninsula)

### [2026-01-30]
- made a cosmetic change: "randomized" the character used for the sea on the preview map.
- adjusted the opacity, too.