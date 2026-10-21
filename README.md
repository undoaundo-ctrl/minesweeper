# Minesweeper

Clear the board by flagging every mine.

> Part of **100 Terminal Games** - a collection of 100 playable games written in C# for the terminal.
> Repository: <https://github.com/undoaundo-ctrl/minesweeper>

## Features

First click is safe, difficulty levels, timers and a mine counter

## Controls

Arrow keys to move, Space to reveal, F to flag, Q to quit

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download) (or newer)
- A terminal that supports ANSI escape sequences

## How to run

```bash
dotnet run --project src/Minesweeper
```

Build once and run the produced binary:

```bash
dotnet build src/Minesweeper -c Release
dotnet src/Minesweeper/bin/Release/net8.0/Minesweeper.dll
```

## Project layout

```text
src/Minesweeper/
  Program.cs      # the game itself
  TerminalKit.cs  # small ANSI colour / input helper library
```

There are no NuGet dependencies - only the .NET base class library is used.

## Category

Puzzle

## License

MIT - see [LICENSE](LICENSE).
