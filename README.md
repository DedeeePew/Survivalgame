# 🎮 SurvivalGame – PvE Survival (Unity)

Ein modulares PvE-Survivalgame, iterativ aufgebaut in klaren Milestones.

---

## 📦 Voraussetzungen

1. **Unity Hub** installieren: https://unity.com/download
2. **Unity 2022.3 LTS** (oder neuer) über Unity Hub installieren
   - Beim Installieren sicherstellen, dass **Windows Build Support** aktiviert ist

---

## 🚀 Projekt öffnen

1. Unity Hub öffnen
2. **"Add" / "Open"** → Navigiere zu `Desktop/SurvivalGame`
3. Unity wird das Projekt erkennen und die Pakete importieren (kann beim ersten Mal 2-5 Min dauern)
4. Falls Unity nach der Unity-Version fragt: Wähle deine installierte 2022.3+ Version

---

## 🏗️ M1: Test-Scene erstellen (Automatisch!)

Nach dem Import:

1. **Menü:** `SurvivalGame → Setup Test Scene (M1)`
2. Die gesamte Test-Scene wird automatisch erstellt:
   - Ground (grüne Ebene)
   - Player (FPS Controller mit Kamera)
   - 3 farbige Test-Cubes (interaktiv)
   - 2 Wände (Orientierung)
   - Directional Light
   - GameManager (Bootstrapper + Debug UI)
3. **Scene speichern:** `Ctrl+S` → Speichere als `Assets/Scenes/TestScene.unity`
4. **Play drücken!** ▶️

### ⚠️ Falls Input nicht funktioniert:
- `Edit → Project Settings → Player → Active Input Handling` → auf **"Both"** oder **"Input Manager (Old)"** setzen
- Die Scripts nutzen aktuell den alten Input Manager (`Input.GetAxis`)

---

## 🎮 Controls (M1)

| Taste | Aktion |
|-------|--------|
| **WASD** | Bewegen |
| **Maus** | Umsehen |
| **Shift** | Sprinten |
| **Space** | Springen |
| **E** | Interagieren |
| **F1** | Debug-Panel toggle |
| **ESC** | Cursor lock/unlock |

---

## ✅ M1 Self-Test Checkliste

Prüfe nach dem Starten:

- [ ] **Bewegung:** WASD bewegt den Spieler über die Ebene
- [ ] **Maus:** Kamera dreht sich mit der Maus (horizontal + vertikal)
- [ ] **Sprint:** Shift + W = schneller (DebugUI zeigt höhere Speed)
- [ ] **Jump:** Space = Sprung, Spieler kommt zurück auf den Boden
- [ ] **Debug UI:** F1 togglet das Panel oben links (FPS, Position, Speed, Grounded)
- [ ] **Crosshair:** Weißes Fadenkreuz in der Mitte sichtbar
- [ ] **Interagieren:** Laufe zu einem farbigen Cube, Crosshair wird GRÜN
- [ ] **Prompt:** Text erscheint unter dem Crosshair (z.B. "Red Box [E]")
- [ ] **E-Taste:** Cube blinkt kurz gelb, Console zeigt Interaction-Log
- [ ] **Weg schauen:** Prompt verschwindet, Crosshair wird weiß
- [ ] **ESC:** Cursor wird sichtbar, nochmal ESC = wieder locked

---

## 📁 Projektstruktur

```
Assets/
├── Scripts/
│   ├── Core/           ← ServiceLocator, Events, Interfaces, Bootstrapper
│   ├── Player/         ← PlayerController (FPS Movement)
│   ├── Interaction/    ← InteractSystem, TestInteractable
│   ├── UI/             ← DebugUI (IMGUI)
│   ├── Items/          ← (M2: ItemDef, ItemStack)
│   ├── Inventory/      ← (M2: Model, Controller, UI)
│   ├── Loot/           ← (M3: LootTable, Roller, Container)
│   ├── Gathering/      ← (M4: HarvestNode, ToolDef)
│   ├── Crafting/       ← (M5: Recipes, CraftingSystem)
│   ├── Building/       ← (M6: Placement, Snap)
│   ├── Skills/         ← (M7: SkillTree, Modifiers)
│   ├── AI/             ← (M9: EnemyAI, StateMachine)
│   └── Save/           ← (M8: SaveData, JSON)
├── ScriptableObjects/
│   ├── Items/          ← ItemDef Assets
│   ├── Recipes/        ← RecipeDef Assets
│   ├── LootTables/     ← LootTableDef Assets
│   ├── Skills/         ← SkillTreeDef Assets
│   ├── Enemies/        ← EnemyDef Assets
│   └── BuildPieces/    ← BuildPieceDef Assets
├── Prefabs/
│   ├── Items/
│   ├── Environment/
│   ├── Building/
│   └── UI/
├── Materials/
└── Scenes/
```

---

## 🏗️ M2: Inventory Setup

Nach M1:

1. **Menü:** `SurvivalGame → Create Example Items (M2)` → erstellt 16 Items + ItemDatabase
2. **Menü:** `SurvivalGame → Setup Test Scene (M2 - Inventory)` → erstellt Scene mit Inventar + WorldItems
3. **Scene speichern:** `Ctrl+S` → als `Assets/Scenes/TestScene_M2.unity`
4. **Play!** ▶️

### 🎮 Neue Controls (M2)

| Taste | Aktion |
|-------|--------|
| **TAB** | Inventar öffnen/schließen |
| **E** | Item aufheben (WorldItem) |
| **G** | Ausgewähltes Item droppen (ganzer Stack) |
| **Shift+G** | 1 Stück droppen |
| **Mausrad-Klick** | Stack halbieren (Split) |
| **Links-Klick Slot** | Auswählen / Verschieben / Tauschen |

### ✅ M2 Self-Test Checkliste

- [ ] **Pickup:** Laufe zu einem kleinen Cube, drücke E → Item wird aufgesammelt
- [ ] **TAB:** Inventar öffnet sich, Cursor wird sichtbar
- [ ] **Items sichtbar:** Aufgesammelte Items erscheinen im Grid mit Name + Menge
- [ ] **Gewicht:** Weight-Anzeige oben zeigt aktuelle Last
- [ ] **Move:** Klick auf Item → Klick auf anderen Slot = Item verschoben
- [ ] **Stack:** Klick auf gleiches Item → Stacks werden zusammengeführt
- [ ] **Split:** Mausrad auf Stack mit >1 → Stack wird halbiert
- [ ] **Drop:** Item auswählen → G drücken → Cube fliegt vor dir raus
- [ ] **Drop 1:** Shift+G → nur 1 Stück wird gedroppt
- [ ] **Pickup gedroppt:** Weggeworfenes Item kann wieder aufgehoben werden
- [ ] **Rarity Colors:** Items zeigen farbigen Text (weiß=Common, grün=Uncommon)
- [ ] **Tooltip:** Ausgewähltes Item zeigt Details unten (Name, Gewicht, Beschreibung)
- [ ] **TAB schließen:** Inventar schließt, Cursor wird wieder locked

---

## 📋 Milestone-Übersicht

| # | Milestone | Status |
|---|-----------|--------|
| M1 | Player Move + Interact + Debug UI | ✅ Done |
| M2 | Items & Inventory | ✅ Done |
| M3 | Looting (Container + LootTables) | ⬜ |
| M4 | Gathering (Harvest Nodes + Tools) | ⬜ |
| M5 | Crafting (Recipes + Stations) | ⬜ |
| M6 | Base Building (Snap + Placement) | ⬜ |
| M7 | Skill Tree (3 Branches) | ⬜ |
| M8 | Save/Load (JSON) | ⬜ |
| M9 | PvE Enemy (AI + Loot) | ⬜ |

---

## 🏛️ Architektur-Entscheidungen

- **Service Locator** statt DI-Framework (kein Zenject/VContainer nötig)
- **GameEvents** als statischer Event-Bus für entkoppelte Kommunikation
- **IInteractable** Interface für alle interagierbaren Objekte
- **IMGUI** für Debug UI (kein Canvas nötig, schnell, funktional)
- **Old Input Manager** aktuell (Migration zu New Input System optional)
- **ScriptableObjects** für alle Konfigurationsdaten (Items, Rezepte, Loot, Skills)
