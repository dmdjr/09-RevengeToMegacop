# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**RevengeToMegacop** is a Unity 3D top-down action game where the player fights waves of enemies using guns, a throwable sword, and a teleporting shuriken. The player can parry/guard bullets and execute weakened enemies.

- Unity version: 6000.0.67f1 (Unity 6)
- Language: C# (global namespace, no custom namespaces)

## Build & Test Commands

Set the Unity editor path first:
```bash
export UNITY_PATH="/Applications/Unity/Hub/Editor/6000.0.67f1/Unity.app/Contents/MacOS/Unity"
# Windows: UNITY_PATH="C:\\Program Files\\Unity\\Hub\\Editor\\6000.0.67f1\\Editor\\Unity.exe"
```

**Build:**
```bash
"$UNITY_PATH" -batchmode -nographics -quit -projectPath "$PWD" -logFile build.log -executeMethod BuildScript.PerformBuild
```

**Run all tests (play mode):**
```bash
"$UNITY_PATH" -batchmode -nographics -quit -projectPath "$PWD" -runTests -testPlatform playmode -testResults testResults.xml -logFile test.log
```

**Run a single test:**
```bash
# Add: -testFilter "Namespace.ClassName.MethodName"
"$UNITY_PATH" -batchmode -nographics -quit -projectPath "$PWD" -runTests -testPlatform playmode -testFilter "MyGame.Tests.PlayerMovementTests.TestJump" -testResults testResults.xml -logFile test.log
```

Check `build.log` for compilation errors (exit code 0 = success).

