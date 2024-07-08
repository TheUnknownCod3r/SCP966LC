## 1.0.0

- Initial release

## 1.0.1
- Fixed a bug where your weight would just infinitly go up
- Modified ReadMe.md
- Prepared Scan if needed
## 1.1.1
- Changed Attack speed from 0.3 seconds cooldown to 2.7 seconds
- Changed the attack distance to be bigger
- Added a collision to the monster
- Remove LateGameUpgrade as a dependency (WARNING : THE GOGGLES WILL STILL WORK IF YOU INSTALL THE MOD)
- Adjusted the NavMesh for it to work better ( Made it smaller )
- Scanning will make you able to see the monster for 0.2 seconds
- Adjusted description of Thunderstore Page
## 1.1.2
- Made the scanning more memory and thread efficient for less lag!
- Created a SCP966 manager which will handle all SCP966 instance in the scene
## 1.1.3
- Fixed a minor problem, where scanning would display an error message if SCP966 was not present in the scene
## 1.1.4
- Fixed a problem when destroying  ( NOT KILLING ) an SCP966 instance where he would stay in the SCP manager
- Made the scan more consistent
- Changed the curve spawn rate!
- Put up the base rarity
- Added configurations!
- CONFIG : Time you see the monster --> The amount of time the monster is visible on scan
- CONFIG : Rarity --> The rarity of SCp 966 on all the moons
## 1.1.5
- Fixed null reference exception regarding scp 966 target player
- REMOVED weight mechanic. it was buggy as hell
- ADDED no stamina regen mechanic!
- Fixed some small bugs here and there