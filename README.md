## PICOEMU

This is the repository for my implementation of a PICO-8 Emulator!
 
It keeps the memory layout but without the CPU and memory limits so that we can expand on PICO-8 games as much as we want to \o/

### Usage

In the folder PicoInterpreter/Pico8 you will find a Pico8.cs file. There, the PICO-8 main class is defined.

Firstly, you will need to create a new Pico8 object

```c#
Pico8 pico8 = new Pico8<Color>()
```

Since this class does not depend on any external libraries other than LUA interpreters, it needs to know how to process input, convert screen color data and convert audio buffer data to the desired format. That's why we have a 'Color' data structure given to the classes constructor, so that way it can know about which data structure you choose the define a color value as.

Let's start breaking these down.

### Input Processing

To tell the PICO-8 Emulator how to process input, we need to do something similar to the code below:

```c#
pico8.AddLeftButtonDownFunction(() => { return Keyboard.GetState().IsKeyDown(Keys.Left); }, 0);
pico8.AddDownButtonDownFunction(() => { return Keyboard.GetState().IsKeyDown(Keys.Down); }, 0);
pico8.AddUpButtonDownFunction(() => { return Keyboard.GetState().IsKeyDown(Keys.Up); }, 0);
pico8.AddRightButtonDownFunction(() => { return Keyboard.GetState().IsKeyDown(Keys.Right); }, 0);
pico8.AddOButtonDownFunction(() => { return Keyboard.GetState().IsKeyDown(Keys.Z); }, 0);
pico8.AddXButtonDownFunction(() => { return Keyboard.GetState().IsKeyDown(Keys.X); }, 0);
```

In this case, I am using the MonoGame API to read button input. What you need to do is to provide functions that return whether or not a button is down or not for each of PICO-8s buttons (left, right, up, down, O, Z). You also need to provide the player index that you want to assign that function to. Keep in mind that you can add multiple functions for the same button, so you can add a keyboard button and a gamepad button at the same time.

### Screen Data

Now we need to tell the class how to interpret the PICO-8 screen color values and draw it to the screen. This process is done by setting an array of color values named 'screenColorData' to the PICO-8 class and a function that takes rgb integer values and returns the corresponding color value called 'rgbToColor'. You can see below how that can be done:

```c#
// Screen Color Data of size 128 * 128, which is PICO-8s screen size.
Color[] screenColorData = new Color[128 * 128];

// Give the class instance the array and a function that can take rgb values
// and interpret it as your choice of data structure.
pico8.screenColorData = screenColorData;
pico8.rgbToColor = (r, g, b) => new Color(r, g, b);
```

In the first line, we create an array of size 128 * 128 to represent the color values of the screen. It is then passed to the Flip function so that the class can write screen color data onto it. The lambda takes the rgb value and returns a new Color object (that is specific to Monogame, change it to the equivalent in whatever you are using).

The 'screenColorData' array that you gave to the class will be used to set each pixel of the 128*128 screen. That happens in the `pico8.Draw()` function, so after calling it you can read the array and use it to draw to your screen.

### Loading a Cartridge

To actually run a game you will need to load a .p8 lua file (not the cartridge, but the actual file with the code, gfx, map, sfx and music values).

```c#
pico8.LoadGame("game.lua", new MoonSharpInterpreter());
```

The LoadGame function takes the path to your game and a lua interpreter. You can use another interpreter if you want to, just implement the ILuaInterpreter interface and pass it instead.

This function will load the game into the PICO-8 ram, initialize the API and run the _init function on your .p8 file if there is one. To call the update and draw function, simply call:

```c#
pico8.Update();

pico8.Draw();
```
