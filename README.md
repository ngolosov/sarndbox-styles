# SARndbox Style Files

This collection provides style files to improve the visual appearance of Oliver Kreylos' SARndbox 2.8.

## Height Map Styles

Nine new height map styles have been implemented based on ColorBrewer color schemes. Since ColorBrewer does not provide more than 11 color classes and SARndbox expects 15 classes, additional colors were interpolated at elevation levels -0.25, 0.25, 9.0, and 20.0.

### Available Color Schemes
The following color schemes are included:
- **BrBg.cpt** - Brown to Blue-Green diverging
- **PiYG.cpt** - Pink to Yellow-Green diverging
- **PRGn.cpt** - Purple-Red to Green diverging
- **PuOr.cpt** - Purple to Orange diverging
- **RdBu.cpt** - Red to Blue diverging
- **RdGu.cpt** - Red to Green diverging
- **RdYlBu.cpt** - Red-Yellow to Blue diverging
- **RdYlGn.cpt** - Red-Yellow to Green diverging
- **Spectral.cpt** - Spectral rainbow scheme

### Installation
To use these height map styles:

1. Copy all `.cpt` files from `ColorBrewer styles` to the following directory:
   ```
   ~/src/SARndbox-2.8/etc/SARndbox-2.8/
   ```

## Launcher Shortcuts

The shortcuts directory contains convenient launcher scripts for running SARndbox with different color schemes.

### Setup
1. Place the `Shortcuts` directory in a convenient location (e.g., your desktop)
2. Make the `.sh` files executable:
   ```bash
   chmod +x Shortcuts/*.sh
   ```
3. Run SARndbox with your desired color scheme by executing the corresponding `.sh` file

Each script filename corresponds to its respective color scheme name and will display elevations using the associated colors.

## Surface Contour Lines Shader

The `SurfaceAddContourLines` shader adds Tanaka contouring and hillshade effects to the displayed terrain. Two versions are available:

- **SurfaceAddContourLines.fs** - Standard single-width contour lines
- **SurfaceAddContourLines.fs_wide_lines** - Enhanced 4x width contour lines

### Installation
Replace the original `SurfaceAddContourLines.fs` shader file with your preferred version from the `Shaders` directory in the following directory on your machine:
`~/src/SARndbox-2.8/share/SARndbox-2.8/Shaders/`

