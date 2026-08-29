# Shaping Extended

![Extended radial menu](/assets/radial_menu.png)

A gameplay mod for Allumeria that adds **new block shapes** and a **new hammer behaviour**.

Thanks to _Mirage_ and other community members in [this suggestions thread](https://discord.com/channels/1311156281658183760/1537267016430391366/1537267016430391366) that provided the initial idea.

## How to Install

1. Install Ignitron mod loader (skip if you already did)
   - Create `/mods` folder inside the game directory
     > Default location: `C:\Program Files (x86)\Steam\steamapps\common\Allumeria`
   - Download the latest [Ignitron.Loader.zip](https://allumeria-db.haaxor1689.dev/api/ignitron-loader)
   - Extract the zip into the `/mods` folder directly (not into another folder within)
1. Download the latest [shaping-extended-mod.zip](https://github.com/haaxor1689/shaping-extended-mod/releases/latest/download/shaping-extended-mod.zip)
1. Put the zip (not extracted) into the `/mods` folder in the game files

## Changelog

### 1.1.1

- Slightly increased radial menu radius to better fit all the items

### 1.1.0

- Updated to **Allumeria 0.15** early access release
- Added new Column and Vertical Slab shapes
- Renamed all "Side" variants to "Vertical" for consistency

## Extended shapes

![Preview of the shaped blocks](/assets/preview.png)

This mod extends the shaping radial menu with new options and also extends some existing ones:

- **Outer/Inner Corner Stair** (can be rotated and flipped)
- **Side Stair** (can be rotated)
- **Step** (can be rotated and flipped) - basically a half slab
- **Side Step** (can be rotated)
- **Flooring** (can be flipped) - because ceilings also need carpets sometimes
- **Siding** (can be rotated) - a flooring for your walls
- **Mini Block** (can be rotated and flipped) - a centered mini block that support 8 rotations instead of just 4

> **Side Slabs** are coming in the Early Access release and that's why I decided not to add them.

## Extended hammers capabilities

Hammers are getting a set of new functions on their left click depending on what shaped block you are looking at.

### Easy rotation and flip

|                                                                           |                               |
| ------------------------------------------------------------------------- | ----------------------------- |
| `Left-Click`: **rotates** a block that supports rotations                 | ![Rotate](/assets/rotate.gif) |
| `Shift + Left-Click`: **flips** a block that support upside down rotation | ![Flip](/assets/flip.gif)     |

### Toggle fence/panel connections

|                                                               |                                                     |
| ------------------------------------------------------------- | --------------------------------------------------- |
| `Left-Click`: toggles a connection facing away from you       | ![Toggle connection](/assets/toggle_a.gif)          |
| `Shift + Left-Click`: toggles a connection facing towards you | ![Toggle connection inverted](/assets/toggle_b.gif) |
