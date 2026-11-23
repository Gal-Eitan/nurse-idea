# Dramatic Elements – Last Shift

This document details the dramatic elements of the game **Last Shift** at the level of programming specifications (Game Design Specifications), building upon the game's Formal Elements document.

## 1. Challenge

### A. Required Player Skills

To succeed in the game, the player must demonstrate four core skills:

1. **Triage & Prioritization Under Pressure:** Evaluating the urgency of each patient's treatment relative to their remaining time, walking distance, and medical supply availability.
2. **Spatial Planning & Working Memory:** Constructing efficient movement routes across the ward (e.g., picking up a diagnostic kit from the storage room on the way to the examination room without making redundant trips).
3. **Hand-Eye Coordination & Precision (Fine Motor Skills & Timing):** Completing short medical mini-games (stopping a cursor in a valid zone, dragging and calibrating medication dosages) quickly and accurately.
4. **Resource Management:** Balancing the nurse's energy meter against the pace of patient treatment, deciding when to pause for a brief rest and when to push forward.

### B. Manual Selection Between Three Difficulty Levels

At the start of the game, the player can choose one of three difficulty levels. This selection loads a configuration object (`DifficultyConfig`) that directly modifies numerical parameters in the `ShiftManager`, `PatientSpawner`, and `PlayerController` classes:

| Code Parameter (`DifficultyConfig`) | Level 1: Intern (Easy) | Level 2: Registered Nurse (Medium) | Level 3: Shift Supervisor (Hard) | Gameplay Impact                                           |
| :---------------------------------- | :--------------------- | :--------------------------------- | :------------------------------- | :-------------------------------------------------------- |
| `shiftDurationSeconds`              | $420$ (7 min)          | $480$ (8 min)                      | $600$ (10 min)                   | Total shift duration                                      |
| `taskSpawnIntervalRange`            | $[18, 22]$ sec         | $[12, 16]$ sec                     | $[8, 12]$ sec                    | Arrival rate of new patients and tasks                    |
| `maxConcurrentTasks`                | $3$                    | $5$                                | $7$                              | Maximum active tasks simultaneously                       |
| `patientHealthDecayPerSec`          | $0.8$                  | $1.2$                              | $1.8$                            | Condition decay rate of waiting patients                  |
| `playerEnergyDrainPerStep`    | $0.5$            | $1.0$                        | $1.5$                      | Nurse fatigue rate during movement and treatment    |
| `restEnergyRecoveryPerSec`    | $8.0$            | $5.0$                        | $3.5$                      | Energy recovery rate in the break area              |
| `initialMedicalSupplies`            | $15$                   | $10$                               | $6$                              | Initial supply units before needing restock               |
| `minigameSuccessZoneWidth`    | $30\%$           | $20\%$                       | $12\%$                     | Width of the green target zone in timing mini-games |

### C. Dynamic Difficulty Adjustment (DDA)

The `DynamicDifficultyController` system samples player performance every $30$ seconds and calculates a **Rolling Skill Index ($S$)** in the range $[0, 100]$ using the formula:

$$
S = 0.5 \cdot Q_{\text{care}} + 0.3 \cdot \left(R_{\text{time}} \cdot 100\right) - 10 \cdot E_{\text{recent}}
$$

Where:

* $Q_{\text{care}}$ – Current value of the Care Quality meter ($0\text{–}100$).
* $R_{\text{time}}$ – Average ratio of remaining time left for patients at the moment their treatment was completed during the last time window ($0.0\text{–}1.0$).
* $E_{\text{recent}}$ – Number of mini-game errors or expired tasks in the last $30$ seconds.

**Adjustment Logic in Code:**

* **High Mastery State ($S > 85$):** The system reduces `taskSpawnIntervalRange` by $15\%$ and increases the probability of urgent emergency cases (`emergencyWeight += 0.15`) to prevent boredom.
* **Overload State ($S < 45$):** The system increases `taskSpawnIntervalRange` by $20\%$, slows down `patientHealthDecayPerSec` by $15\%$, and increases the likelihood that the next patient requires a simple task (not requiring supplies from storage) to allow the player to recover.

## 2. Flow

### A. Focusing Attention and Preventing Distractions

* **Visual Contrast & Hierarchy:** The hospital environment is designed in calm, cool tones (light gray-blue floors and walls), while interactive elements and tasks use vibrant traffic-light colors (green $\rightarrow$ yellow $\rightarrow$ flashing red) that immediately draw the eye.
* **Dynamic Off-Screen Pointers:** When a patient in a side room (such as the CT room or X-ray room) drops below $30\%$ health, a flashing arrow appears at the edge of the screen pointing toward the relevant room.
* **Isolation During Mini-Games:** When opening a treatment window (mini-game), the background dims slightly (`Overlay Alpha = 0.4`) and the decay rate of other patients slows down by $50\%$ (`externalTimersScale = 0.5`), allowing the player to focus on the motor task without feeling cheated by the game while concentrating.

### B. Sense of Control and Agency

* **Immediate Input Response:** Player movement (`WASD`) features smooth acceleration and deceleration with zero delay (`Input Lag = 0`), plus a sprint option (`Left Shift`) that boosts movement speed by $35\%$ at the cost of double energy consumption—a tactical choice in the player's hands.
* **Complete Prioritization Freedom:** The game does not enforce a strict task order; the player decides which tasks to tackle first and can cancel an interaction (`Esc`) at any moment if a more urgent emergency arises.
* **Visible Impact:** A successfully treated patient gets out of bed, smiles, and exits the ward (freeing up space for a new patient), whereas neglect causes visible deterioration in the patient's animation and the Care Quality meter.

### C. Feedback on Success or Failure

* **Positive Feedback (Treatment Success):**
  * Visual: A bouncing green heart/star icon above the patient + floating text (`+100 pts`).
  * Audio: A harmonious chime sound (`SFX_HealSuccess`).
  * Systemic: Immediate $+3\%$ increase in the Care Quality meter.
* **Negative Feedback (Mini-Game Mistake or Neglect):**
  * Visual: Subtle camera shake (`CameraShake(intensity: 0.15f, duration: 0.2f)`), red flash on the Care Quality meter, and a disappointment bubble above the patient.
  * Audio: A dull buzzer sound (`SFX_ActionFail`) and an accelerated heart-monitor beep (`SFX_HeartMonitorBeep`) when a patient's timer drops below $15\%$.

## 3. Play

### A. Fun Interactions Unrelated to Direct Game Goals (Micro-Interactions)

To give the ward a sense of life and humor, optional interactions via the `E` key are included:

1. **Coffee Machine in the Upper Hallway:** Pressing `E` next to the machine triggers an espresso-drinking animation and displays a humorous thought bubble (*"Pure caffeine!"* / *"4 more hours to go"*).
2. **Anatomical Skeletons in Examination Rooms:** Interacting with a skeleton causes the nurse to shake its hand or straighten its skull, accompanied by witty remarks (*"At least he doesn't complain about the wait"*).
3. **Watering the Ward Plants:** Interacting with the green potted plants in the hallway and ICU makes them bloom momentarily and unlocks a secret achievement (*"Green Thumb"*).
4. **Hallway Chat with the Receptionist:** Approaching the reception desk displays random medical gossip lines (*"Dr. Ron lost his stethoscope again"*).

### B. Target Player Types (Bartle / Fullerton Taxonomy)

* **Achievers (Primary Audience):** Strive to achieve a "Full Victory" ($\ge 95\%$ task completion with no major errors) and maximize their score.
* **Problem Solvers & Strategists:** Enjoy planning optimal movement routes and managing medical supplies and energy under tight constraints.

### C. Expanding Appeal to Additional Player Types

* **Explorers:** Adding hidden "medical files" and notes around the ward that reveal funny or heartwarming stories about the hospital's history and patients.
* **Socializers:** Adding a stylized end-of-shift "Shift Evaluation Sheet" summary screen that can be shared, along with building emotional rapport with staff characters and recurring patients.
* **Competitors:** Adding a "Daily Challenge Shift" mode (with a fixed random Seed) and a leaderboard to compare performance against other players.

### C. Customization
- with the points achieved through the levels, you can buy accessories for the break room-some will make you wakefullness refill faster and some will be for only be decorative purpose.
	 

## 4. Emotions

### A. Expected Emotions During Gameplay

1. **Urgency & Positive Tension:** The adrenaline rush when multiple patients need help simultaneously and the clock is ticking down.
2. **Empathy & Responsibility:** Genuine concern for waiting patients and the drive to prevent their condition from worsening.
3. **Relief & Triumph:** The sense of accomplishment upon successfully surviving a hectic shift.
4. **Lightheartedness & Humor:** A smile evoked by the charming Pixel Art aesthetic and playful micro-interactions.

### B. How Game Design Fosters These Emotions

* **Adaptive Soundtrack:** The music tempo (BPM) gradually increases from $100$ to $140$ when the Care Quality meter drops below $40\%$ or during the final $60$ seconds of the shift, physiologically heightening the sense of urgency.
* **Expressive Pixel Art Animations:** Patients display clear emotional icons above their heads (sweat drops, pained expressions, smiling hearts) that evoke immediate empathy.
* **Spatial Contrast:** The break area features warm lighting and muted ambient sound, creating an immediate sense of momentary relief amidst the chaos when stepping inside.

## 5. Backstory

### A. Narrative Background

The game takes place in "Ward C" of the old Municipal Hospital. Tonight is the final night of operation for the veteran ward before it closes for major renovations, as well as the protagonist's **last shift** in the ward before transferring to a new role.
During the evening, a severe winter storm paralyzes city transit; the morning relief crew is stranded on the roads, while a multi-car pileup on the highway sends an unexpected wave of patients rushing through the ward doors. The player must hold down Ward C almost single-handedly until the storm subsides and reinforcements arrive.

### B. Conveying the Backstory to the Player

1. **Brief Intro Cutscene (3 Pixel Art Slides):** A news flash about the winter storm followed by a radio call from the Head Nurse: *"The relief crew is stuck in the storm. This is Ward C's last shift—hold the line until morning!"*
2. **Environmental Storytelling:** Half-packed cardboard boxes in the hallways (signaling the ward's closure), a *"Thank You Ward C Staff"* banner near the reception desk, and muffled audio effects of rain and thunder outside.
3. **Patient & Staff Speech Bubbles:** Brief remarks referencing the storm outside and the ward's closing during gameplay.

## 6. Characters

### A. Main Characters

* **The Protagonist (The Player – Ward Nurse):** A dedicated, agile, and empathetic nurse in white scrubs. Designed as a Semi-Silent Protagonist so the player can fully project themselves onto the character.
* **The Main Antagonist:** **Chaos and the Clock ("The Storm Shift")** – A systemic force represented by patient overload, dwindling supplies, and running out of time. On a personal level, this pressure is voiced by **Dr. Levy (Shift Director on the PA system)**, who broadcasts urgent alerts and reminds the player of the ward's limitations.

### B. Secondary Characters (Aligned with Ward Layout)

1. **Sarah (Receptionist at the Upper Desk):** Checks in arriving patients, provides early warnings about incoming waves, and adds dry humor to the atmosphere.
2. **Dr. Ron (Physician in the Lower-Right Exam Room):** Diagnoses complex cases; certain tasks require delivering vitals to him to obtain a prescription.
3. **Noa (CT Technician in the Lower-Left Room):** Operates the imaging scanner, creating brief cooperative interaction loops.
4. **Patient Archetypes:**
   * *Mr. Goldstein (Elderly Patient):* Moves slowly, requires patient vitals checks, and awards a high score bonus upon discharge.
   * *Danny (Anxious / Impatient Patient):* Health meter decays faster if ignored; requires quick reassurance and prompt attention.
   * *Trauma Patient (Emergency Case):* Enters through the blue double doors and requires immediate medical supplies from the storage room.

### C. Character Progression During the Game

* **Ward Staff:** Begin the shift in a calm, routine mood, but as pressure mounts, characters switch to faster work animations and urgent speech bubbles. Upon a successful shift, they express deep gratitude toward the protagonist.
* **The Protagonist:** Progresses in skill and earns an end-of-shift title reflecting their performance (ranging from *"Surviving Intern"* to *"Legend of Ward C"*).

### D. Behavior Resembling "Free Will" (Illusion of Free Will)

Non-player characters (`NPCs`) are driven by an independent `State Machine`:

* Waiting patients in the hallway do not stand frozen; they locate an empty chair (the green and yellow chairs in the corridor), sit down, glance at the wall clock, occasionally walk over to the vending machine, or chat with a neighboring patient.
* Dr. Ron and Noa type on their computers, inspect X-ray scans on the lightboard, and occasionally take brief coffee breaks, creating the illusion of a living, breathing hospital.

## 7. Plot

### A. Dramatic Arc (Graph)

<img width="1024" height="572" alt="image" src="https://github.com/user-attachments/assets/90f01951-9dc4-4b3b-ba42-48a101b1ca68" />


### B. Plot Progression Along the Dramatic Arc

1. **Exposition (0:00–1:00):** Start of the shift, meeting the staff, and treating the first simple patient to learn core mechanics.
2. **Inciting Incident (1:00):** PA announcement about a highway pileup caused by the storm and the delay of the morning shift. The blue doors burst open and the rush begins.
3. **Rising Action (1:00–5:30):** Surge in patient arrivals, depleting medical supplies, and mounting triage dilemmas.
4. **Climax / Crisis (5:30–7:00):** "The Great Emergency Wave" – A temporary malfunction in the CT scanner coincides with the arrival of two critical trauma cases, demanding peak focus from the player.
5. **Falling Action (7:00–7:45):** The storm weakens, patient inflow slows down, final stabilization of the ward, and the distant sound of relief crew sirens arriving.
6. **Resolution (7:45–8:00):** The shift ends and a narrative ending plays based on the player's performance.

### C. How Player Behavior Influences Plot Development

Player performance determines which of three endings is triggered:

* **Heroic Ending (Full Victory $\ge 95\%$):** Hospital administration is impressed by the exemplary performance during the storm, allocates a special budget to keep Ward C open, and awards the protagonist a Medal of Excellence.
* **Balanced Ending (Standard Victory $\ge 80\%$):** The ward survives the night, patients are safely transferred by morning, and the protagonist bids farewell to the team with a sense of pride.
* **Failure Ending (Missed Goals / Care Quality Collapse):** The hospital director is forced to order an early emergency evacuation of the ward, and the protagonist ends their shift with a sense of regret.

### D. Story Structure in Twine (Accompanying HTML File)

As an accompanying narrative meta-game, the shift's storyline was built in **Twine** (exported to `last-shift-story.html`). It simulates the dramatic decision-making throughout the night while tracking the variables `$careQuality`, `$energy`, and `$supplies`, allowing the player to experience branching narrative paths and multiple endings.

## 8. Worldbuilding

### A. Laws of Nature, Geography, History, Economy, Society & Politics

* **Laws of Nature & Time:** A flat 2D top-down world governed by Time Dilation: every $1$ real-world minute represents $1$ shift hour ($8$ minutes = a full night shift from midnight to 08:00). Physical energy depletes through movement and treatment and replenishes rapidly during rest.
* **Geography:** Ward C of the Municipal Medical Center—an enclosed facility featuring a central corridor, reception desk, waiting area, main double entrance doors, medical supply storage, staff break area, CT/imaging room, general examination room, and X-ray/ICU room.
* **History:** Built in the 1980s, Ward C served for decades as the medical backbone of the city's older neighborhoods, though it now suffers from aging infrastructure.
* **Economy:** A public healthcare system operating under strict budget constraints ("scarcity economy")—medical supplies in the ward are finite and require careful management to avoid waste.
* **Society & Politics:** Ongoing tension between hospital administration (seeking to cut costs by closing the ward) and the dedicated medical staff and local residents who rely on accessible care. The narrative touches upon the ethics of triage (prioritizing patients strictly by medical urgency).

### B. How the Player Learns About the World

* **Level Design & Geography:** Clear signage above rooms, numbered beds, and recognizable medical equipment in each room.
* **Visual & Environmental Clues:** Moving boxes, a hallway bulletin board with newspaper clippings, and old commendation plaques.
* **PA Announcements & Dialogue:** Brief lines from Sarah the receptionist and patients revealing conditions in the city and hospital.

## 9. Meta-game

Among the meta-game types studied, the most fitting combination for *Last Shift* is **Progression & Unlocks** paired with an **Achievements System**:

1. **Cross-Shift Progression:** At the end of each shift, the player's score is converted into "Nursing Experience Points" (`NursingXP`). Accumulating XP unlocks permanent upgrades for subsequent shifts:
   * *Orthopedic Running Shoes (Tier 1):* Increases base movement speed by $10\%$.
   * *Double Supply Pouch (Tier 2):* Allows the nurse to carry $2$ medical supply items from storage simultaneously instead of $1$.
   * *Night Shift Stamina (Tier 3):* Reduces energy drain rate by $15\%$.
2. **Achievements & Badges System:** Encourages replayability and experimentation with different playstyles, such as: *"Flawless Shift"* ($100\%$ tasks completed), *"Guardian Angel"* (saving $5$ patients from critical condition), and *"Caffeine Addict"* (drinking $5$ cups of coffee in a single shift).

## 10. LiveOps Management

### A. Dynamic Configuration Loading Architecture

Upon game initialization (`GameManager.Awake`), the system performs an asynchronous network request (`UnityWebRequest.Get`) to fetch a JSON configuration file hosted on a remote server (such as the project's GitHub Pages), updating gameplay parameters at runtime without requiring a new code build. If the client is offline, the game falls back to a local `default-config.json` file.

### B. Example Live Event: "Winter Flu Surge"

Below is the dynamic configuration file (`live-config.json`) activating a weekend winter live event:

```json
{
  "configVersion": "1.2.0",
  "activeEvent": {
    "eventId": "WINTER_FLU_SURGE_2026",
    "isActive": true,
    "eventTitle": "Live Event: Winter Flu Surge!",
    "eventDescription": "Unusual influx of flu patients! Check vitals quickly to earn a special winter badge.",
    "themeTintHex": "#DCEEFF",
    "modifiers": {
      "vitalsTaskProbabilityMultiplier": 1.8,
      "taskSpawnIntervalMultiplier": 0.85,
      "feverPatientSpeedBonus": 1.25,
      "scoreMultiplier": 1.5
    },
    "rewardBadgeId": "BADGE_WINTER_HERO_2026"
  }
}
```

**How the Code Utilizes This Configuration:**

1. **`UIEventBanner`:** Checks if `activeEvent.isActive == true`. If active, displays a winter-blue banner (`themeTintHex`) on the start screen with the event's title and description.
2. **`PatientSpawner`:** Multiplies the spawn probability of temperature and vitals-check tasks by $1.8$ (`vitalsTaskProbabilityMultiplier`) and shortens the interval between incoming patients by $15\%$ (`taskSpawnIntervalMultiplier`).
3. **`ScoreManager`:** Multiplies the player's final score by $1.5$ (`scoreMultiplier`), and upon successfully completing the shift ($\ge 80\%$), unlocks the exclusive `BADGE_WINTER_HERO_2026` in the achievements gallery.
