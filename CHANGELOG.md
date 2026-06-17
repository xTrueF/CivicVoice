## [0.3.0] - 2026-06-16
### Added
- Three-way elections with incumbent re-election, two-term limits and approval-weighted voting
- Mayor aging and birthday system — mayors age in real time over their term
- Mayor priority badges — gold star on proposals matching the current mayor's specialty
- Toast notifications — slide-in popups when the panel is closed, colour coded by event type
- Elections mod compatibility — CivicVoice elections disabled when Elections mod is detected, mayor data read from Elections mod
- I18n Everywhere support
- New and tweaked proposals across all tiers
- Abandoned projects tracked separately from failed in city stats
- City stats redesigned with collapsible City Overview and Project Overview sections

### Changed
- Health and crime now read real city data via game system reflection
- Progress bars fixed for all goal types — above and below goals now calculate correctly
- Cooldown date comparison fixed — proposals no longer get stuck when expiry date equals current date
- Various UI improvements across the panel

### Fixed
- Metric proposal cooldown label showing raw binding key in settings
- Healthcare proposal being immediately removed after triggering due to goal target being too low
- Progress bar showing incorrectly for health and other above-threshold goals


## [0.2.1] - 2026-06-13
### Added
- Reset to defaults button in settings
- Total projects completed and failed shown in City Stats tab
- Current mayor specialty and slogan shown in Election tab when no election is running
- Notification history — last 3 notifications with fading opacity, collapsible with Recent Activity heading
- Collapsible sections for proposal tiers and active project tiers, state persists when switching tabs
- Project count shown in each collapsible section header and in Proposals and Active tabs
- Active projects now show tier and category pills matching proposal card style
- Eligible voter count now uses real age data (teens + adults + seniors) instead of flat 88% estimate

### Changed
- Active project cards now match proposal card visual style
- Endorse button changed from solid blue to outline style for consistency
- Hover states added to all buttons, tabs, collapsible section headers and Recent Activity header
- Button spacing improved
- Active Projects tab shortened to Active
- Notification history replaces single notification bar

### Fixed
- Notifications disappearing to quickly

## [0.2.0] - 2026-06-12
### Added
- Full settings menu with configurable options:
  . Election frequency and minimum population threshold
  . Proposal trigger thresholds for unemployment, crime, homelessness, housing demand, health and wellbeing
  . Maximum active proposals per tier
  . Major project minimum population
  . Citizen proposal and rejected proposal cooldowns
  . Endorsement influence percentage
  . Show/hide notifications toggle
- Universal Mod Menu support — toggle between top-right toolbar and UMM in settings
- Force Election button in settings
- Save migration — existing AdHoc projects now correctly show Mark Complete button

### Changed
- Citizen proposals now use manual completion — mark done when you've built the thing
- Proposal descriptions for citizen and major projects now show correct text
- Fixed options menu showing raw localisation keys

## [0.1.1] - 2026-06-11
### Fixed
- Fixed mod icon not showing when subscribed via Paradox Mods