## PICOEMU

This is the repository for my implementation of a PICO-8 Emulator!
 
It keeps the memory layout but without the CPU and memory limits so that we can expand on PICO-8 games as much as we want to \o/

### Usage

In the folder PicoInterpreter/Pico8 you will find a Pico8.cs file. There, the PICO-8 main class is defined.

Firstly, you will need to create a new Pico8 object

```c#
Pico8 pico8 = new Pico8()
```

Since this class is platform independent, it is important to set a few things so that it can process input and screen color values. Firstly, let's add functionality to allow for input processing:

```c#
pico8.SetBtnPressedCallback(((x) => Keyboard.GetState().IsKeyDown((Keys)x)));
pico8.SetControllerKeys(0, (int)Keys.Left, (int)Keys.Right, (int)Keys.Up, (int)Keys.Down, (int)Keys.Z, (int)Keys.X);
```

In this case, I am using the MonoGame API to read button input. In the first line I add a simple lambda function that takes an integer value representing the key that was pressed and call a IsKeyDown function that takes that value and returns whether that key is down or not.
After that, I set all the integer values that represent all the PICO-8 keys: Left, Rignt, Up, Down, Z and X.

Now we need to tell the class how to interpret the PICO-8 screen color values and draw it to the screen. This process is done by passing an array of color values to the Flip function and a lambda function that taks rgb integer values and returns the corresponding color value.

```c#
Color[] screenColorData = new Color[128 * 128];

function Draw()
{
  // (...)
  
  pico8.graphics.Flip(ref screenColorData, ((r, g, b) => new Color(r, g, b))) ;

  screenTexture.SetData(screenColorData);
  spriteBatch.Draw(screenTexture, new Rectangle(0, 0, 128, 128), Color.White);
  
  // (...)
}
```

In the first line, we create an array of size 128 * 128 to represent the color values of the screen. It is then passed to the Flip function so that the class can write screen color data onto it. The lambda takes the rgb value and returns a new Color object (that is specific to Monogame, change it to the equivalent in whatever you are using).

Finally, we set the texture with the color data and write it to the screen. Again, this is Monogame specific and needs to be changed to your draw-to-the-screen equivalent.

Now, to actually run a game you will need to load a .p8 lua file (not the cartridge, but the actual file with the code, gfx, map, sfx and music values).

```c#
pico8.LoadGame("game.lua", new MoonSharpInterpreter());
```

The LoadGame function takes the path to your game and a lua interpreter. You can use another interpreter if you want to, just implement the ILuaInterpreter interface and pass it instead.

This function will load the game into the PICO-8 ram, initialize the API and run the _init function on your .p8 file if there is one. To call the update and draw function, simply call:

```c#
pico8.Update();

pico8.Draw();
```

REMEMBER: you should call Flip() AFTER calling Draw()!
