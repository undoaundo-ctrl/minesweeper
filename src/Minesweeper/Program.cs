using System;
using System.Text;

internal static class Program
{
    private static int width = 9;
    private static int height = 9;
    private static int mineCount = 10;
    private static bool[,] mines = null!;
    private static int[,] counts = null!;
    private static bool[,] revealed = null!;
    private static bool[,] flagged = null!;
    private static int cursorX;
    private static int cursorY;
    private static int flaggedCount;
    private static int revealedCount;
    private static int elapsed;
    private static bool dead;
    private static bool won;
    private static readonly Random Rng = new Random();

    private static void Main()
    {
        Term.Setup("Minesweeper");
        bool again = true;
        while (again)
        {
            int choice = Term.AskChoice("Choose a difficulty", new string[]
            {
                "Beginner   9x9, 10 mines",
                "Medium    16x16, 40 mines",
                "Expert     30x16, 99 mines"
            });

            if (choice == 0) Setup(9, 9, 10);
            else if (choice == 1) Setup(16, 16, 40);
            else Setup(30, 16, 99);

            again = Play();
        }

        Term.AnyKey("Press any key to exit...");
    }

    private static void Setup(int w, int h, int m)
    {
        width = w;
        height = h;
        mineCount = m;
        mines = new bool[h, w];
        counts = new int[h, w];
        revealed = new bool[h, w];
        flagged = new bool[h, w];
        cursorX = w / 2;
        cursorY = h / 2;
        flaggedCount = 0;
        revealedCount = 0;
        elapsed = 0;
        dead = false;
        won = false;
    }

    private static bool Play()
    {
        long last = DateTime.UtcNow.Ticks;
        Term.ClearScreen();

        while (!dead && !won)
        {
            while (Term.KeyAvailable)
            {
                ConsoleKeyInfo? key = Term.Key();
                if (!key.HasValue) break;
                switch (key.Value.Key)
                {
                    case ConsoleKey.LeftArrow: if (cursorX > 0) cursorX--; break;
                    case ConsoleKey.RightArrow: if (cursorX < width - 1) cursorX++; break;
                    case ConsoleKey.UpArrow: if (cursorY > 0) cursorY--; break;
                    case ConsoleKey.DownArrow: if (cursorY < height - 1) cursorY++; break;
                    case ConsoleKey.Space: Reveal(cursorX, cursorY); break;
                    case ConsoleKey.F: ToggleFlag(cursorX, cursorY); break;
                }
            }

            long now = DateTime.UtcNow.Ticks;
            if (now - last >= TimeSpan.TicksPerMillisecond * 1000)
            {
                last = now;
                elapsed++;
            }

            Render();
            Term.Sleep(30);
        }

        Render();
        Term.Line();
        if (won) Term.Line(Term.BrightGreen + "  Cleared in " + elapsed + " seconds!" + Term.Reset);
        else Term.Line(Term.BrightRed + "  Boom! You hit a mine." + Term.Reset);
        ConsoleKeyInfo? again = Term.WaitKey("  Press Enter for the menu, Q to quit...");
        return again.HasValue && again.Value.Key != ConsoleKey.Q && again.Value.Key != ConsoleKey.Escape;
    }

    private static void ToggleFlag(int x, int y)
    {
        if (revealed[y, x]) return;
        if (flagged[y, x])
        {
            flagged[y, x] = false;
            flaggedCount--;
        }
        else
        {
            flagged[y, x] = true;
            flaggedCount++;
        }
    }

    private static void Reveal(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        if (revealed[y, x] || flagged[y, x]) return;

        if (revealedCount == 0)
        {
            PlaceMines(x, y);
            ComputeCounts();
        }

        if (mines[y, x])
        {
            dead = true;
            return;
        }

        RevealArea(x, y);
        CheckWin();
    }

    private static void PlaceMines(int safeX, int safeY)
    {
        int placed = 0;
        int guard = 0;
        while (placed < mineCount && guard < 10000)
        {
            guard++;
            int x = Rng.Next(width);
            int y = Rng.Next(height);
            if (mines[y, x]) continue;
            if (Math.Abs(x - safeX) <= 1 && Math.Abs(y - safeY) <= 1) continue;
            mines[y, x] = true;
            placed++;
        }
    }

    private static void ComputeCounts()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int count = 0;
                for (int dy = -1; dy <= 1; dy++)
                {
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                        if (mines[ny, nx]) count++;
                    }
                }
                counts[y, x] = count;
            }
        }
    }

    private static void RevealArea(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        if (revealed[y, x] || flagged[y, x]) return;
        revealed[y, x] = true;
        revealedCount++;
        if (counts[y, x] == 0)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue;
                    RevealArea(x + dx, y + dy);
                }
            }
        }
    }

    private static void CheckWin()
    {
        if (revealedCount + mineCount != width * height) return;
        won = true;
    }

    private static string NumberColor(int value)
    {
        switch (value)
        {
            case 1: return Term.BrightBlue;
            case 2: return Term.BrightGreen;
            case 3: return Term.BrightRed;
            case 4: return Term.BrightMagenta;
            case 5: return Term.BrightYellow;
            case 6: return Term.BrightCyan;
            case 7: return Term.White;
            case 8: return Term.Gray;
            default: return Term.Reset;
        }
    }

    private static void Render()
    {
        StringBuilder sb = new StringBuilder();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x == cursorX && y == cursorY) sb.Append(Term.Reverse);
                if (dead && mines[y, x])
                {
                    sb.Append(Term.BrightRed).Append('*').Append(Term.Reset);
                }
                else if (revealed[y, x])
                {
                    if (counts[y, x] == 0) sb.Append(Term.Dim).Append('.').Append(Term.Reset);
                    else sb.Append(NumberColor(counts[y, x])).Append(counts[y, x]).Append(Term.Reset);
                }
                else if (flagged[y, x])
                {
                    sb.Append(Term.BrightRed).Append('F').Append(Term.Reset);
                }
                else
                {
                    sb.Append(Term.Gray).Append('#').Append(Term.Reset);
                }
            }
            sb.Append('\n');
        }

        sb.Append(Term.Yellow).Append("  Mines left ").Append(mineCount - flaggedCount)
          .Append("   Time ").Append(elapsed)
          .Append("s").Append(Term.Reset).Append('\n');
        sb.Append(Term.Dim).Append("  Arrows move, Space reveals, F flags.").Append(Term.Reset).Append('\n');

        Term.ClearScreen();
        Term.Write(sb.ToString());
    }
}
