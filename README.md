# 🎱 Billiards Game

![Platform](https://img.shields.io/badge/platform-Windows-blue?style=flat-square&logo=windows)
![Framework](https://img.shields.io/badge/.NET-4.7.2-purple?style=flat-square&logo=dotnet)
![Language](https://img.shields.io/badge/language-C%23-239120?style=flat-square&logo=csharp)
![Build](https://img.shields.io/badge/build-passing-brightgreen?style=flat-square&logo=visualstudio)
![License](https://img.shields.io/badge/license-MIT-orange?style=flat-square)

A 2D billiards game built with **C# Windows Forms** featuring realistic ball physics, collision detection, turn-based multiplayer, and a live score/timer HUD.

---

## 🕹️ Gameplay

- **2 Players** take turns shooting the cue ball
- Pocket colored balls to score points
- The player with the most balls pocketed wins
- Game ends when all balls are cleared from the table

---

## ✨ Features

| Feature | Details |
|---|---|
| 🎯 Aim System | Click and drag to aim — release to shoot |
| ⚡ Physics | Velocity, friction, and elastic ball-to-ball collisions |
| 🕳️ Pockets | 6 pockets with configurable positions (saved to `pockets.txt`) |
| 👥 Turn System | Alternates turns; pocketing a ball keeps your turn |
| 🏆 Scoreboard | Live score display for both players |
| ⏱️ Timer | Elapsed game time shown in the HUD |
| 🛠️ Pocket Editor | Hold `Alt` + drag to reposition pockets |

---

## 🚀 Getting Started

### Prerequisites

- Windows OS
- [Visual Studio 2019+](https://visualstudio.microsoft.com/) or [.NET Framework 4.7.2](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472)

### Run

```bash
# Clone the repo
git clone https://github.com/your-username/BilliardsGame.git

# Open in Visual Studio
# Build → Start (F5)
```

> Make sure `1.png` (the table image) is in the same directory as the executable.

---

## 🎮 Controls

| Input | Action |
|---|---|
| `Left Click + Drag` | Aim the cue ball |
| `Release` | Shoot |
| `Alt + Drag` | Move a pocket |

---

## 📁 Project Structure

```
BilliardsGame/
├── Form1.cs              # Game logic, physics, rendering
├── Form1.Designer.cs     # WinForms designer output
├── Program.cs            # Entry point
├── 1.png                 # Table background image
├── pockets.txt           # Saved pocket positions (auto-generated)
└── BilliardsGame.csproj  # Project file
```

---

## 🧠 How It Works

- **Ball physics** — each ball has a position and velocity; friction (`0.98` multiplier) slows them down each frame
- **Collision resolution** — elastic collision using normal/tangential decomposition
- **Pocket detection** — balls within pocket radius are removed from play
- **Turn logic** — turn switches only if no ball was pocketed; prevents double-switch bug with a `turnProcessed` flag

---

## 📸 Screenshot

> _Add a screenshot of your game here_

---

## 📄 License

MIT — do whatever you want with it.
