# CS-ChirpBanner
Cities Skylines Mod - ChirpBanner
Replaces Chirpy with a scrolling marquee style banner along the top 

V1.1.1 
- typo: scrolling not srolling 
- added catch to serialize code 

V1.1 
- added configuration file "ChirpBannerConfig.xml" which gets created automatically with defaults in C:\Program Files (x86)\Steam\steamapps\common\Cities_Skylines 
- edit it to change values for: 

DestroyBuiltinChirper (bool, true/false, default = false) 
MaxChirps (int, 1-10, default = 3) 
ScrollSpeed (int, 1-100, default = 30) 

Hopefully I got the new version update working properly... 

V1.0 
- displays a max of 3 chirps 
- new chirps push oldest ones out 
- chirps never deleted until new ones come in (so you'll always see the same chirps scrolling until new ones come in 
- no configurability yet (# chirps, speed, color, position, etc) 


DLL only as it references UnityEngine.UI etc.

In Steam Workshop: http://steamcommunity.com/sharedfiles/filedetails/?id=406623071
