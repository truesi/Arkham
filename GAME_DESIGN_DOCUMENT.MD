This is my indie game - Arkham. An isometric turn-based game based on the Arkham Horror board game. 

1. High-level concept.
1.1. Working title - Arkham.
1.2. Concept - Turn based roleplaying co-op quest story.
1.3. Genre - Turn Based Isometric Co-op PVE Quest.

2. Product design.
2.1. Player Experience
Players are detective on the mission to solve quest to save the world from upcoming dooms day. Setting - Mid 19th century - England. Lovecraftian Horrors are emerging from the bottom and threatining the world. Players must solve the different quests / fight minions in order to progress the quests and find the solution to stop the Evil. With each turn players are getting closer to the dooms day, but they can find the solutions in order to stop the evil from emerging. Each turn the forces of evil becomes stronger and plot thickens.
2.2 Visual and Audio style.
Victorian era english gentlemen and ladies.

3. Detailed and Game system design.
3.1 Core loops - Players are given a turn to move/fight/investigate mysteries/solve puzzles/purchase items in shop in a small town of england. After player turn the game makes it move moving forward the enemies, creating obstacles and progress the quest story futher. The quest story has its pivot points and if the players are too slow to reach those pivot points the game either becomes progressivily harder or ends with bad ending. However if players are making progress in the quest investigations they are able to progress further with a chance for a good ending of the quest.
3.2 Objectives and Progression - Main objective of the players is to investigate clues found in different locations of the town. The enemies are actively trying to prevent players to reach those locations. If enough clues were investigated players are progressing further in quest. The game also progresses with each turn and if players were not able to progress in set time the story becomes harder or ends.
3.3 Game map - The town in England is separated into different district tiles. Districts are separated into smaller street tiles. Districts are connected only with bridges which is separate tile zones. Players are able to move from street to street or from street to bridges in order to cross into other district tile.
3.4 Game systems - Each street will spawn different events. Events could be - clues to investigate, void zone where enemies are spawning.
3.5 Player movement - Player can control his character with mouse. Move camera with mouse.
3.6 Player charater - Player can choose at the start of the game 3 different charaters with different stat and baseline HP.
3.6.1 Stat system - Strenght - for combat checks, Intelligence - for clue solve checks and mysteries help, Agility - for defense check
3.7 Combat - When a player engages an enemy (by moving onto its tile) or an enemy reaches a player on the world turn, the game enters a dedicated combat scene shown as a full-screen panel inspired by Slay-the-Spire-style framing (player on one side, enemy on the other, with HP bars and a combat log). Combat is turn-based: the player and the enemy take alternating attacks, each resolved by a skill check against the player's strenght stat (roll ≤ skill = hit). Hits deal the attacker's strenght damage; misses do nothing. Combat ends when one side reaches 0 HP. If the player wins, the enemy is removed from the map and the game returns to whichever phase preceded combat (player turn or world turn). If the player loses, the game ends.
  Current MVP: the player has a single "Attack" action (one button = one round of player attack + enemy counter). The combat system is intended to deepen over time — additional actions (defend, item use, special abilities), per-enemy stats and attack patterns, and richer presentation are open design space for later iterations.
3.7 Camera movement - Camera is following active player. Can be moved with the mouse.

