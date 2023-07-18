# Simple Save System

**Simple Save System** is a tool for saving game data to files.

## Advanced Features

### Can I use the plugin for consoles?

Yes, we used the **Simple Save System** to port our game [Garden Simulator](https://store.steampowered.com/app/1403310/Garden_Simulator/) 
to all major consoles including PS4, PS5, Nintendo Switch, Xbox One and Xbox Series.
To do so, you need to implement the `IFileReadWriter` interface as each platform comes with its own
SDK and an API for reading and writing data. Use `SaveFileUtility.SetFileReadWriter(IFileReadWriter)`
to use your custom implementation instead of the `DefaultFileReadWriter`.

### Shipped Save Slot

For testing purposes it can be helpful to ship a save game on a certain save slot. 
If you want to do so, create a `ShippedSaveSlot` by right-clicking in the Project 
view and select `Create > PRODUKTIVKELLER > Simple Save System > Shipped Save Slot`. 
Put the file in a *Resource* folder, so it will be found when the game starts. 
Put the JSON data in the scriptable object and specify the save slot you want to use.