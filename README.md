# P4G Dungeon Editor
A tool meant to help modders edit dungeon-related data in Persona 4 Golden.


## How to use
Since this tool is still early in development (and I'm lacking in experience with teaching users how to use something like this), this section is dedicated to explaining each page of the tool and what data is editable through it.

Page List: 
+ [Startup](#startup)
+ [Encounters](#encounters)
+ [Encounter Tables](#encounter-tables)
+ [Loot Tables](#loot-tables)
+ [Room Data](#room-data)
+ [Floors](#floors)
+ [Templates](#templates)
+ [Using the modifications](#using-the-modifications)
### Note <br>
All of the tabs have something like this in the top-right corner:<br>
![image](https://github.com/Some-Body-Else/P4G-Dungeon-Editor/assets/86819277/af2d6779-2d2c-428f-b234-3497a1fca6f3) <br>
The number indicates the current entry being viewed, while the [+] and [-] buttons add and remove entries from the current table respectively.<br>
Added entries are appended to the end of the table, while clicking [-] will remove the current entry.
### Startup
![image](https://github.com/Some-Body-Else/P4G-Dungeon-Editor/assets/86819277/b2eff435-901c-40ba-a321-e90dc1fe7b60)
<br>
This is how the program boots up. Upon loading an existing project or creating a new one, the background color changes, indicating that data is ready to be edited.<br>
Unlike the previous iterations, no external file is required to create new projects; every bit of editable data is provided in the JSON folder

### Encounters
![image](https://github.com/Some-Body-Else/P4G-Dungeon-Editor/assets/86819277/a24feb33-bdc6-4980-b0fd-a1c2d522a7f4)
<br>
This is the page for editing the game's encounters, which are listed here: https://amicitia.miraheze.org/wiki/Persona_4_Golden/Encounters 
- The unit IDs correspond to the enemy IDs on the Amicitia wiki: https://amicitia.miraheze.org/wiki/Persona_4_Golden/Enemies
- The flags are currently unknown, to the best of my knowledge. They are typically set to 1 for regular encounters, but the bosses have a number of them set.
- Music ID and the music it correlates to is on Amicitia's encounter page.
- Field ID and room ID indicate the file that the battlefield is pulled from. Regular encounters set it to 0, while boss encounters call specific fields. The filed for these fields are found in /field/pack/, like any other field in the game.
- Field04 and Field06 are unknown to me, may be unused and subject to renaming/removal as the tool develops.

### Encounter Tables
![image](https://github.com/Some-Body-Else/P4G-Dungeon-Editor/assets/86819277/ae1efbad-a027-42bb-891a-700f81905594)
<br>
This table dictates what encounters can be found on a given floor. It has changed significantly since the previous iterations due to my understanding of the vanilla table becoming greater. 
-- The weights, as best I can tell, indicate how the enemy spawns would be divied up between the three types of shadows *assuming equal selection odds*
-- AlwaysFF is what it says on the tin, not even sure if this one is accessed. May be removed in the future, but keeping around for now just in case.
-- % Usage is a value that indicates the likelihood of either a gold or rare shadow taking the place of a normal one. To my best uneducated (read: I didn't check the code directly, just messed with values), the game rolls a number to figure out which of the weights it's dealing with, and if it's handling the rare or gold shadows, then it rolls another number relative to the percentage to see if a rare/gold shadow is spawned or if a normal encounter is spawned anyways.
--- The end result is that, to increase likelihood and amount of rare/gold shadows, you need to increase their weights *and* these percentages for really noticable results, otherwise you get bottlenecked.
-- All encounters in a group (normal/rare/gold) have their encounter IDs as well as a weight that, when summed, should equal 100 (haven't checked too thoroughly)
-- Encounters 21-25 are reserved for rare shadows and encounters 26-30 are reserved for gold shadows, so their color backgrounds have been changed to reflect this.
-- Button on top switches between the encounter table used for regular days and the ones used on days where it is raining. Weights and encounters can be changed between the two, but not the percentages for rare/gold shadows

### Loot Tables
![image](https://github.com/Some-Body-Else/P4G-Dungeon-Editor/assets/86819277/095ce6a4-d017-4f37-8296-11c19c58ca52)
<br>
The table here contains information on everything relating to the chests that can spawn.
- The items weights of a table are summed up in-game, so each weight is relative to the collective weight sum of the table. You can mark your percentages out from there.
- Item ID corresponds to the IDs found here: https://amicitia.miraheze.org/wiki/Persona_4_Golden/Items
- Chest flags are mostly unknown for now. Marking a flag as 1 means the chest will be locked upon spawning, but there are other values that show up that require more testing.
- Big Chest is just a flag for whether the chest uses the small red design (off) or the big gold one (on)

### Room Data
![image](https://github.com/Some-Body-Else/P4G-Dungeon-Editor/assets/86819277/287d2794-6cd4-48fc-a702-84f9d831c899)<br>
Okay, this is the big one.<br>
Persona 4 Golden, natively, has 14 rooms that it builds its dungeons with. This editor lets you view and modify the data of all of them, plus an invalid one. The dungeon itself is a 16x24 grid where rooms are placed in following a simple set of cunnection rules.
- The square on the left is used by the game to determine how the room is placed in-game. The buttons indicate the direction tiles are connected in, either to tiles from other rooms or with tiles in the same room.
- When the *HasDoor* button is enabled, buttons in-between tiles will show up, representing the door connections between rooms. It is technically possible to have doors that connect to entirely seperate rooms, but the game does not natively do that so (at least for now) doors can only be connected to tiles within the same room.
- Clicking on a tile will change its color (slight changes in this version, planned to be changed once I figure out what palette to use). Tiles that are connected to each other and of the same color are essentially part of the same 'piece' and will reveal other parts of the chain on the map when discovered*. Pieces with doors between them should be of different colors to mask what is behind the door until it is opened.
  --*This is not entirely accurate to the way the game actually goes about it. Each of the pieces in a chain are revealed all at once, according to the internal minimap, but visually only one of the tiles has control over the minimap texture the player sees. As the game reveals each tile in the piece, it inevitably will reveal the one in control of the texture, subsequently showing the texture for every tile under that banner. Changing the control tile is something that will likely be implemented in a future update.

- While it is possible to add new room data with this tool, *I do not recommend doing using this feature at this time.* Each room needs it's own visual model, player collision model, camera collision model, minimap texture, and minimap data to properly generate in-game. While all of this is certainly possible, I intend to implement features into this tool to aid in this (hence the greyed-out *Room Models* tab), so doing so now could be painful. If you wish to add rooms anyways, please feel free to ask me about any difficulties encountered on the [Persona Modding Discord](https://discord.gg/naoto).
- Size X and Size Y dictate the size of the room, in tiles. Currently, the game expects a maximum room size of 3x3, but this is hypothetically possible to change. If and when the tool and the dungeon framework are further in development, the 3x3 limit could be abolished. It's also worth noting that the game natively does not have any rooms where the height and width are not equivalent. From my looking, I don't think this would cause an issue, but having not test it personally, I can't really say.
- HasDoor was mentioned earlier, and IsExit is a shorthand way to denote that a room is designated as having the stairs for a dungeon and must therefore be spawned. Haven't looked into the logistics of exits in the game too thoroughly, so this button may be subject to change as time goes on.
- The grid at the bottom-right is essentially a generalization of the room for the game to look at when generating the map. I've labelled it as the *outline* of the room, since it's essentially telling the game what tiles are going to be used if the game spawns this in. Unchecked boxes signify no space is used, once-checked tiles are for room tiles, and double-checked tiles are for marker tiles (tiles that exist for spacing out rooms to prevent potentially illegal generation).

### Floors
![image](https://github.com/Some-Body-Else/P4G-Dungeon-Editor/assets/86819277/afad5d4c-ac70-4822-af00-8de69dcd1a8e)
<br>
Every dungeon's definition starts here, in essence. The table here is actually two tables, one of which is in ENCOUNT.TBL, so it will be expandable at a later time.
- Field ID and Room ID dictate the field being used for a given 'floor'. Not every floor is part of a dungeon (entry 2 is the TV Hub) and not all fields follow the same rules here. Notably, randomly-generated floors will *always* have a Room ID of 0. The ID of a field also appears to influence how the game reacts to it being loaded in: fields with IDs between 40 and 59 are randomly-generated floors, 60-80 are pregenated floors that use the tiles of the fields 20 spaces below them, and fields in the 200s range are used for battles. Ideally these rules will be accounted for by the dungeon framework at some point, but it is not so at the moment.
- Min and max tile counts are used by randomly-generated floors to put distinct bounds on the dungeon's size. Setting the min above the max causes a crash, and having the two values set to too wide a range has also resulted in crashes, so I hope to take a look at the generation logic and potentially stabilize it in the future.
- Dungeon script indicates which of the dungeon scripts will be loaded in. Again, need to look into more to figure out what this ultimately entails.
- .ENV chooses which .ENV in the dungeon's data arc will be used. Data arcs are located in /field/pack/fdXXX_001.arc, where XXX is the 3-digit representation of the field ID. .ENV files control some of the dungeon visuals and are responsible for the color changes as you head higher up the dungeon.
- Byte04 and Byte0A both appear vestigial, but Byte04 is specifically set to 1 for the entrance of Heaven, so that one gives me pause.
- Encounter Table picks which of the encounter tables the floor pulls from.
- Loot Table picks the loot table the game pulls from for a given floor.
- Max Chest Count is, unsuprisingly, the maximum number of chest that a given floor spawns with (may possibly just be the number of chests the floor spawns with). **Using a loot table with fewer entries than the max chest count will crash the game!** 
- Max Enemy Count dictates how many enemies will appear when the floor first generates, Min. Enemy Count is the minimum number of enemies that exist on the floor at a given time (dropping below this number will spawn a new one to take the old one's place) **Using an encounter table with less entries than the max will crash the game!**

### Templates
![image](https://github.com/Some-Body-Else/P4G-Dungeon-Editor/assets/86819277/e6139da5-9733-4244-a504-66b044e59b0f)<br>
Templates are how Persona 4 Golden determines which dungeon uses which rooms. Each template consists of a minimum list of rooms, a list of rooms added to the former  after certain condition (need to figure out that condition, starts happening higher up in the dungeons), and the room designated for the exit. There's also a table indicating which randomly-generated fields use which templates. Might add something to allow for multiple types of entrances to be selected from, but that is something for down the line.<br>
*Note: Unlike the previous pages, the upper portion of this page interacts through drag and drop. Apologies to those who hate mice*

-The upper-left box contains the regular rooms used in generation, split between the general generation rooms (left) and the extended list that is used on higher floors (right). Beneath them is the list of unused rooms.
- Upper-right box selects the room used as an exit. Will look more into this, but I can't quite recall if the exit to the dungeon is linked with another tile that is the same in proportions, but lacks the data to be an exit. Again, upper box is what is used, lower box is unused.
- Lower box is what fields use this template. The up-down to the right allows you to enter a field ID, the plus adds it to the list, the minus removes it. Recommend caution with this one due to the framework not yet accounting for the field ID regulations put in place by the game.

### Using the modifications
So you've decided to make some changes to game's data with this tool. Great! Now how do you get your changes showing up in-game?<br>
All of the modifications done through this tool results in a series of .json files when a project is saved. These files are loaded into the game through my [Dungeon Framework mod](https://github.com/Some-Body-Else/p4gpc.dungeonframework), so you'll want to install that (it's a Reloaded-II mod)
- Find your Persona 4 Golden's executable directory (right-click on the game in Steam, Properties->Installed Files->Browse)
- Create a new folder named "dungeonframework" (no quotes, all lowercase, no space)
- Place your .json files in the dungeonframework folder
- Enable "Persona 4 Golden Dungeon Framework" and its prerequisites in Reloaded-II
- Launching the game through Reloaded-II should have the changes applied once the game loads up
If you encounter any bugs or have any questions, feel free to ask me on the Persona Modding Discord

## Thank You
Just want to give a thank you to the members of the [Persona Modding Discord](https://discord.gg/naoto) for lending a hand whenever I had a question, even though most of this stuff was in the dark. Also wanted to give it shoutouts to my brother, who drew the icon for the program.

