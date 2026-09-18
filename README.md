# Unity - 360 Video Tour

An immersive 360° Virtual Reality tour experience built with Unity. The project integrates multiple 360 video environments, interactive VR hotspots with gaze/click navigation, points of interest (POIs) with informational panels, comfortable VR fade transitions, background audio mixing, and a multi-scene tour architecture.

## Overview & Architecture

The application contains three core scenes organized for VR navigation:
1. **`MainMenuScene`**: The welcome and launch portal featuring a World Space VR interface with access to the tours.
2. **`IntranetTourScene`** (`360VideoTour`): The 4-room Holberton/ALU intranet experience featuring:
   - **Living Room**: Main entry lounge with hotspots to Cantina and Cube, plus info boxes.
   - **Cantina**: Social and event space with hotspots to Living Room and Cube.
   - **Cube**: Quiet study nook with hotspots to Living Room, Cantina, and Mezzanine.
   - **Mezzanine**: Collaborative study area with hotspots to Cube and dual POI stations.
3. **`CustomCampusTourScene`**: A personalized campus tour featuring 3 continuous original 360 captures (`Room1`, `Room2`, `Room3`).

## Key Features
- **Inside-Out 360 Spherical Video**: High-definition equirectangular 360° video mapped to inverted spheres using Unity Video Players and Render Textures.
- **Interactive Hotspots**: Gaze and pointer raycast interaction with hover scale micro-animations and audio confirmation.
- **Informational Points of Interest (POIs)**: Floating panels providing context on each room with auto-dismissal on room change.
- **VR Comfort Transitions**: Screen fade-to-black effect between all room transitions and scene changes to prevent disorientation.
- **Audio Mixing**: Background music routed through an Audio Mixer with dedicated group volume attenuation.

## Audio Attribution
This project uses the following royalty-free audio asset:
- **"Tech Live"** Kevin MacLeod ([incompetech.com](https://incompetech.com))
  Licensed under Creative Commons: By Attribution 4.0 License
  [http://creativecommons.org/licenses/by/4.0/](http://creativecommons.org/licenses/by/4.0/)

## Author
ALU / Holberton School Software Engineering Program
