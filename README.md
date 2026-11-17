# Mark II Drift — Unity Android starter

A mobile-first arcade drift prototype inspired by a white Japanese sedan, blue wheels, wide body kit and night touge atmosphere.

## Target
- Unity **2022.3 LTS**
- Android, landscape gameplay
- Built-in Render Pipeline

## Included
- Runtime-generated night test road and placeholder sedan
- Rear-wheel-drive arcade drift controller with handbrake
- Chase camera
- Touch steering, throttle, brake and handbrake UI
- Basic speed/drift score HUD
- GitHub Actions Android build workflow

## Open in Unity
1. Install Unity 2022.3 LTS with Android Build Support, Android SDK & NDK Tools, and OpenJDK on a computer.
2. Open this folder as a Unity project.
3. Open Assets/Scenes/Boot.unity and press Play.
4. Set Build Settings to Android and Landscape.
5. Build the APK.

## GitHub Actions APK build
Add a repository secret named UNITY_LICENSE containing your Unity Personal license .ulf contents. Then run Actions → Android APK Build. The workflow uploads MarkII-Drift.apk as an artifact.

## Important
This is a starter prototype, not a finished game. The car is a procedural placeholder; a detailed Toyota Mark II model, production-quality physics, sound, polished track art and Android device testing remain to be added. Do not distribute third-party assets without appropriate rights.
