# CleanMaster
CleanMaster - a plugin for the final cleanup of SCP:SL
## 🔥 Features
CleanMaster is the ultimate cleanup solution for your SCP:SL server, offering granular control over what gets cleaned and when. Say goodbye to lag and clutter!

## 🧹 Advanced Corpse Cleanup
- Configurable lifetime for corpses (default: 180 seconds)

- Protected zones where corpses never get cleaned (armories, checkpoints, etc.)

- Round-end cleanup option

- Detailed logging for debugging
  
## 💎 Intelligent Item Cleanup
- Cleanup of loose items with zone protection

- Player death cleanup - remove items after player dies (with configurable delay)

- Smart filtering - only cleans items without owners

## 📟 Commands
`clean (true/false)` - disables/enables round-based cleaning (a function has been created for events)
# Permission required for clean command
clean_command_permission: `cleanmaster.control`

## ⚙️ Fully Configurable
    # Enable plugin
    is_enabled: true
    # Debug mode
    debug: false
    # Enable corpse cleanup
    enable_corpse_cleanup: true
    # Corpse lifetime in seconds
    corpse_lifetime: 180
    # Cleanup interval in seconds
    cleanup_interval: 60
    # Initial delay after spawn in seconds
    initial_delay: 120
    # Clean on round end
    clean_on_round_end: true
    # Clean items after death
    clean_items_on_death: true
    # Item cleanup delay after death (seconds)
    items_cleanup_delay: 15
    # Enable loose items cleanup
    enable_item_cleanup: true
    # List of protected item types
    protected_items:
      - MicroHID
      - KeycardO5
      - SCP1576
      - SCP127
      - Jailbird
      - ParticleDisruptor
    # Permission required for clean command
    clean_command_permission: cleanmaster.control

## 📥 Installation
`Download the latest release`

`Place CleanMaster.dll in your plugins folder`

`Restart or reload your server`

## 🛠️ Configuration Guide

![2025-06-28_22-55-09](https://github.com/user-attachments/assets/218e3609-30c1-4071-b044-3e06fe6b1c27)

## 🛡️ Protected Zones
The plugin automatically protects important areas from cleanup:

- Checkpoints

- Armories

- Lockers

- Camera rooms

- SCP-096 chamber

- MicroHID rooms

- Pocket dimension

- Shelters

# Optimize your server's performance today with CleanMaster! 🚀

## 🛑 If you find an error, etc. Submit a request for the owner to fix it
https://docs.google.com/forms/d/e/1FAIpQLSfBlZ6sDQE8Mvr8YbS6-0PO7gQq4CcSchJl-T8U-Kjv6hK7Cg/viewform?usp=dialog
