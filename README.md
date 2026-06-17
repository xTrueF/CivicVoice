# CivicVoice

A democracy and governance mod for Cities: Skylines II that adds a living civic layer to your city. Citizens propose projects, vote on initiatives, and elect a mayor whose agenda shapes the city's direction.

## Overview

Civic Voice is a roleplay and immersion mod. It reads your city's real data to generate meaningful proposals and elections but DOES NOT directly modify gameplay mechanics. Think of it as adding a democratic narrative layer to your city-building experience.

## Features

### Urgent Issues
Triggered automatically when city metrics fall below certain levels. High unemployment, housing shortage, budget deficit — citizens will demand action. Proposals are driven by live city data so they always feel relevant to your current situation.

### Citizen Proposals
General improvements your citizens want to see, influenced by whoever is currently in office. The mayor's specialty nudges which proposals come forward — e.g. an Environment mayor brings more green space proposals, a Healthcare mayor pushes for clinics and hospitals.

### Major Projects
Ambitious year-long undertakings that define your city's future. A new port, a university campus, a signature landmark. Major projects require your approval and are marked complete manually when you've built them in game.

### Mayoral Elections
Every year your city holds a democratic election. Three candidates campaign with distinct specialties, party affiliations and slogans. The incumbent can stand for re-election for up to two terms. Endorse your preferred candidate to add your influence but the citizens decide. The winning mayor's specialty influences which proposals citizens come forward with.

### Live City Data
All proposals and descriptions update in real time based on your city's actual metrics: population, unemployment, happiness, health, housing demand, crime probability, budget and more.

### Toast Notifications
Slide-in notifications appear when the panel is closed, keeping you informed of elections, completed projects and urgent citizen demands without interrupting your city building.

### Elections Mod Compatibility
If the Elections mod is detected, CivicVoice's own election system is disabled and mayor data is read from the Elections mod instead. Proposals and projects continue to work normally.

## Settings

CivicVoice is fully configurable via the in-game options menu:

- Election frequency and minimum population threshold
- Proposal trigger thresholds for unemployment, crime, homelessness, housing demand, health and wellbeing
- Maximum active proposals per tier
- Cooldown periods for citizen and rejected proposals
- Endorsement influence percentage
- Show/hide notifications toggle
- Universal Mod Menu support

## Installation

### Via Paradox Mods
Search for **Civic Voice** in the Paradox Mods browser inside Cities: Skylines II and subscribe.

### Manual Installation
1. Download the latest release from the [Releases](https://github.com/xTrueF/CivicVoice/releases) page
2. Extract to `%AppData%\..\LocalLow\Colossal Order\Cities Skylines II\Mods\CivicVoice\`
3. Enable the mod in the Paradox Mods menu in game

## Compatibility

- Safe to add or remove from any city at any time
- Does not conflict with other mods
- Compatible with the Elections mod
- Save data is stored per city in the CS2 save file

## How It Works

The mod reads live game data every tick to trigger proposals when city metrics fall outside acceptable ranges, update proposal descriptions with current values, remove proposals when their conditions are already met, and track active projects and mark them complete when goals are achieved.

Proposals are organised into three tiers:

**Urgent Issues**: triggered automatically by live city data such as high unemployment, housing shortages, poor health or a budget deficit. Up to 3 can be active at once.

**Citizen Proposals**: general improvements influenced by the current mayor's specialty. Up to 2 can be active at once.

**Major Projects**: ambitious year-long undertakings like a new port or university campus. Marked complete manually when built in game. Up to 2 can be active at once.

## Contributing

Contributions are welcome. Feel free to open issues or pull requests on GitHub.

Ideas for future versions:
- More proposal variety per category
- City council system with multiple councillors
- Notification integration with CS2's chirper
- Map marker at town hall during elections

## License

MIT License: see [LICENSE](LICENSE) for details.

## Credits

Built by [xTrueF](https://github.com/xTrueF)