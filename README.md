# ![unity](https://img.shields.io/badge/Unity-100000?style=for-the-badge&logo=unity&logoColor=white) Audio Manager Lite

[![Unity Version](https://img.shields.io/badge/Unity-2020.3+-blue.svg)](https://unity3d.com/get-unity/download)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.md)
![Development Status](https://img.shields.io/badge/Status-In%20Active%20Development-orange)
![Build](https://img.shields.io/badge/Build-Unstable-red)

⚠️ **WARNING: This project is in an early, unstable state of development.** The API is volatile and subject to drastic changes without notice. Functionality is incomplete, and bugs are expected. **Not recommended for production use or the faint of heart.**

*A lightweight, modular audio management system for Unity, designed for simplicity and flexibility.*

## Overview

AudioManagerLite is a work-in-progress Unity package aimed at providing a straightforward solution for managing sound effects and music in your projects. It focuses on a component-based approach, avoiding monolithic managers and giving you control over how you structure your audio.

### Key Features (Planned & Partial)

- **Modular Design**: Built from independent components (Audio Player, Pool, Database) rather than a single God object.
- **Audio Channel Management**: Organize sounds into logical channels (SFX, Music, UI, Ambient) with independent control.
- **Priority System**: Prevent less important sounds from "stealing" audio sources and cutting off crucial audio.
- **Object Pooling**: Efficient management of AudioSource objects for performance.
- **Inspector-Driven Setup**: Configure and link components easily through the Unity Editor.
- **Lightweight**: Minimal runtime overhead, designed to be just what you need.

## Quick Start (Subject to Change!)

1.  

## Installation (Unstable Version)

## Core Components (WIP)

## Roadmap (Tentative & Evolving)
- [x] Basic project structure and core component skeletons
- [x] Initial implementation of AudioSource pooling
- [ ] Stable API for playing/stopping sounds
- [ ] Audio Channel system for group control
- [ ] Priority system to handle audio source contention
- [ ] Basic fade in/out functionality for music
- [ ] Runtime audio configuration (global volume, channel volume)
- [ ] Proper error handling and logging
- [ ] Demo scene with usage examples
- [ ] [FUTURE] Audio Mixer Group integration
- [ ] [FUTURE] Spatial audio support
- [ ] [FUTURE] Playlist system for music

## License

This project is licensed under the MIT License - see the [LICENSE](https://github.com/LLarean/audiomanager-lite?tab=MIT-1-ov-file) file for details.

## Links
- [Documentation] (Does not exist yet)
- [API Reference] (Does not exist yet)
- [Examples] (See the /Demos folder, if it exists yet)
