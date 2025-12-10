# Audio Assets for VR Duck Hunt

This folder should contain the following audio files:

## Required Audio Files:

1. **HitSound.wav** - Sound effect played when a duck is successfully hit
   - Should be a satisfying "pop" or "thud" sound
   - Duration: 0.5-1.0 seconds
   - Format: WAV, 44.1kHz, 16-bit

2. **MissSound.wav** - Sound effect played when a shot misses
   - Should be a subtle "whoosh" or "click" sound
   - Duration: 0.3-0.5 seconds  
   - Format: WAV, 44.1kHz, 16-bit

## Temporary Setup:
For now, we can use existing audio files from MRTemplateAssets as placeholders:
- Hit Sound: Use Goal.wav (Assets/MRTemplateAssets/Audio/Goal.wav)
- Miss Sound: Use ButtonClick.wav (Assets/MRTemplateAssets/Audio/ButtonClick.wav)

## Integration:
These audio clips should be assigned to the ShootingController component in the scene.