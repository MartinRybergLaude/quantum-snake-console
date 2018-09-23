using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace QuantumSnakeConsole {
    internal class Program {

        private static string snakeCharacter = "■";
        private static string appleCharacter = "■";
        private static string wallCharacter = "█";

        private static readonly ConsoleColor logoTopColor = ConsoleColor.Cyan;
        private static readonly ConsoleColor logoBottomColor = ConsoleColor.Magenta;
        private static readonly ConsoleColor menuItemColor = ConsoleColor.Magenta;
        private static readonly ConsoleColor errorColor = ConsoleColor.Red;

        private const ConsoleColor WallColor = ConsoleColor.Red;
        private const ConsoleColor SnakeColor = ConsoleColor.Cyan;
        private const ConsoleColor FoodColor = ConsoleColor.Magenta;

        private static int gameSpeed = 50;
        private static int snakeLength = 5;
        private static readonly int snakeIncrease = 3;
        private static readonly int applesCount = 4;
        private static readonly int speedChange = 10;
        private static readonly int upperSpeedLimit = 100;
        private static readonly int lowerSpeedLimit = 10;
        private static int currentScore = 0;
        private static int menuState = 0;

        private static List<Position> points = new List<Position>();
        private static List<Position> apples = new List<Position>();
        private static Position foodPosition;
        private static Random random = new Random();
        private static DateTime nextUpdate = DateTime.MinValue;
        private static bool alive;
        private static ConsoleKeyInfo lastKey;
        private static bool hasStarted = false;

        private enum Speed { VerySlow, Slow, Normal, Fast, VeryFast }
        private static Speed currentSpeed;
        private enum Direction { North, South, East, West };
        private static Direction currentDirection;

        private static int windowWidth = Console.WindowWidth;
        private static int windowHeight = Console.WindowHeight;

        private static readonly string[] Logo = new string[]
    {
        "██████╗ ██╗   ██╗ █████╗ ███╗   ██╗████████╗██╗   ██╗███╗   ███╗    ███████╗███╗   ██╗ █████╗ ██╗  ██╗███████╗",
        "██╔═══██╗██║   ██║██╔══██╗████╗  ██║╚══██╔══╝██║   ██║████╗ ████║    ██╔════╝████╗  ██║██╔══██╗██║ ██╔╝██╔════╝",
        "██║   ██║██║   ██║███████║██╔██╗ ██║   ██║   ██║   ██║██╔████╔██║    ███████╗██╔██╗ ██║███████║█████╔╝ █████╗  ",
        "██║▄▄ ██║██║   ██║██╔══██║██║╚██╗██║   ██║   ██║   ██║██║╚██╔╝██║    ╚════██║██║╚██╗██║██╔══██║██╔═██╗ ██╔══╝",
        "╚██████╔╝╚██████╔╝██║  ██║██║ ╚████║   ██║   ╚██████╔╝██║ ╚═╝ ██║    ███████║██║ ╚████║██║  ██║██║  ██╗███████╗",
        " ╚══▀▀═╝  ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═══╝   ╚═╝    ╚═════╝ ╚═╝     ╚═╝    ╚══════╝╚═╝  ╚═══╝╚═╝  ╚═╝╚═╝  ╚═╝╚══════╝"
    };

        private static void Main(string[] args) {
            Console.Title = "BEEP BOOP";
            Console.CursorVisible = false;
            Menu();

        }
        private static void Menu() {
            switch (menuState) {
                case 0:
                    PrintLogo();
                    Console.SetCursorPosition(40, 14);
                    Console.ForegroundColor = menuItemColor;
                    Console.WriteLine("Press 1 to start Classic");
                    Console.SetCursorPosition(40, 16);
                    Console.WriteLine("Press 2 to start VSCPU");
                    break;
                case 1:
                    Console.SetCursorPosition(40, 18);
                    Console.ForegroundColor = errorColor;
                    Console.WriteLine("Not implemented yet!");
                    break;
                case 2:
                    Console.Clear();
                    PrintLogo();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.SetCursorPosition(40, 12);
                    Console.WriteLine("Score: " + currentScore);
                    Console.SetCursorPosition(40, 14);
                    Console.ForegroundColor = menuItemColor;
                    Console.WriteLine("Press 1 to start Classic");
                    Console.SetCursorPosition(40, 16);
                    Console.WriteLine("Press 2 to start VSCPU");
                    Console.SetCursorPosition(40, 18);
                    Console.ForegroundColor = errorColor;
                    Console.WriteLine("You lost!");
                    break;
                default:
                    PrintLogo();
                    Console.SetCursorPosition(40, 14);
                    Console.ForegroundColor = menuItemColor;
                    Console.WriteLine("Press 1 to start Classic");
                    Console.SetCursorPosition(40, 16);
                    Console.WriteLine("Press 2 to start VSCPU");
                    break;
                    ;
            }
            switch (Console.ReadKey(true).Key) {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    menuState = 0;
                    Console.Clear();
                    GameStart();
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    menuState = 1;
                    Menu();
                    break;
                default:
                    menuState = 0;
                    Console.Clear();
                    Menu();
                    break;
            }
        }
        private static void GameStart() {
            currentScore = 0;
            currentSpeed = Speed.Normal;
            currentDirection = Direction.East;
            gameSpeed = 50;
            windowWidth = Console.WindowWidth;
            windowHeight = Console.WindowHeight;
            Console.BufferWidth = Console.WindowWidth;
            Console.BufferHeight = Console.WindowHeight;
            foodPosition = null;
            alive = true;
            DrawWalls();
            DrawCurrentScore();
            DrawCurrentSpeed();
            DrawCurrentDirection();
            OnDraw();
            GameLoop();
            GameOver();
        }
        private static void GameLoop() {
            while (alive) {
                if ((AcceptInput() && UpdateGame()) || (UpdateGame())) {
                    OnDraw();
                }
                Thread.Sleep(1);
            }
        }
        private static void PrintLogo() {
            Console.ForegroundColor = logoTopColor;
            Console.SetCursorPosition(0, 3);
            for (int i = 0; i < Logo.Length; i++) {
                for (int j = 0; j < Logo[i].Length; j++) {
                    if (i >= 3 && i < 7 && j > 2 && j < 5) {
                        Console.ForegroundColor = logoBottomColor;
                    }
                    Console.Write(Logo[i][j]);
                }
                Console.WriteLine();
            }
            Console.ResetColor();
        }
        private static void DrawWalls() {
            Console.ForegroundColor = WallColor;
            for (var i = 0; i < Console.WindowWidth - 1; i++) {
                Console.Write(wallCharacter);
                Console.SetCursorPosition(i, 0);
            }
            for (var i = 0; i < Console.WindowWidth - 0; i++) {
                Console.Write(wallCharacter);
                Console.SetCursorPosition(i, Console.WindowHeight - 2);
            }
            for (var i = 0; i < Console.WindowHeight - 1; i++) {
                Console.Write(wallCharacter);
                Console.SetCursorPosition(0, i);
            }
            for (var i = 0; i < Console.WindowHeight - 1; i++) {
                Console.Write(wallCharacter);
                Console.SetCursorPosition(Console.WindowWidth - 1, i);
            }
        }
        private static void OnDraw() {
            foreach (var apple in apples) {
                Console.ForegroundColor = FoodColor;
                Console.SetCursorPosition(apple.Left, apple.Top);
                Console.Write(appleCharacter);
            }

            if (points.Count == 0) return;
            Console.ForegroundColor = SnakeColor;
            Console.SetCursorPosition(points.Last().Left, points.Last().Top);
            Console.Write(snakeCharacter);
        }
        private class Position {
            public int Left;
            public int Top;
        }
        private static bool AcceptInput() {
            if (!Console.KeyAvailable) {
                return false;
            }
            lastKey = Console.ReadKey(true);
            hasStarted = true;
            return true;
        }
        private static void OnChooseDirection(ConsoleKeyInfo key) {
            Position currentPos;
            if (points.Count != 0) {
                currentPos = currentPos = new Position() {
                    Left = points.Last().Left,
                    Top = points.Last().Top
                };
            } else {
                currentPos = GetStartPosition();
            }
            if (!hasStarted) {
                currentPos.Left++;
            } else {
                // Choose Direction depending on key
                switch (key.Key) {
                    case ConsoleKey.S:
                    case ConsoleKey.DownArrow:
                        if ((currentPos.Top < windowHeight - 1) && (currentDirection != Direction.North)) {
                            currentPos.Top++;
                            currentDirection = Direction.South;
                            DrawCurrentDirection();
                        } else {
                            SetDirection(currentPos);
                        }
                        break;
                    case ConsoleKey.W:
                    case ConsoleKey.UpArrow:
                        if ((currentPos.Top > 0) && (currentDirection != Direction.South)) {
                            currentPos.Top--;
                            currentDirection = Direction.North;
                            DrawCurrentDirection();
                        } else {
                            SetDirection(currentPos);
                        }
                        break;
                    case ConsoleKey.A:
                    case ConsoleKey.LeftArrow:
                        if ((currentPos.Left > 0) && (currentDirection != Direction.East)) {
                            currentPos.Left--;
                            currentDirection = Direction.West;
                            DrawCurrentDirection();
                        } else {
                            SetDirection(currentPos);
                        }

                        break;
                    case ConsoleKey.D:
                    case ConsoleKey.RightArrow:
                        if ((currentPos.Left < windowWidth - 2) && (currentDirection != Direction.West)) {
                            currentPos.Left++;
                            currentDirection = Direction.East;
                            DrawCurrentDirection();
                        } else {
                            SetDirection(currentPos);
                        }
                        break;
                    case ConsoleKey.OemPlus:
                    case ConsoleKey.Add:
                        switch (currentSpeed) {
                            case Speed.VerySlow:
                                currentSpeed = Speed.Slow;
                                gameSpeed = 100;
                                break;
                            case Speed.Slow:
                                currentSpeed = Speed.Normal;
                                gameSpeed = 50;
                                break;
                            case Speed.Normal:
                                currentSpeed = Speed.Fast;
                                gameSpeed = 25;
                                break;
                            case Speed.Fast:
                                currentSpeed = Speed.VeryFast;
                                gameSpeed = 10;
                                break;
                            default:
                                currentSpeed = currentSpeed;
                                break;
                        }
                        DrawCurrentSpeed();
                        SetDirection(currentPos);
                        SetLastKey(currentDirection);
                        break;
                    case ConsoleKey.OemMinus:
                    case ConsoleKey.Subtract:
                        switch (currentSpeed) {
                            case Speed.Slow:
                                currentSpeed = Speed.VerySlow;
                                gameSpeed = 200;
                                break;
                            case Speed.Normal:
                                currentSpeed = Speed.Slow;
                                gameSpeed = 100;
                                break;
                            case Speed.Fast:
                                currentSpeed = Speed.Normal;
                                gameSpeed = 50;
                                break;
                            case Speed.VeryFast:
                                currentSpeed = Speed.Fast;
                                gameSpeed = 25;
                                break;
                            default:
                                currentSpeed = currentSpeed;
                                gameSpeed = gameSpeed;
                                break;
                        }
                        DrawCurrentSpeed();
                        SetDirection(currentPos);
                        SetLastKey(currentDirection);
                        break;
                    // If an invalid key is pressed, choose direction based on last known direction.
                    default:
                        SetDirection(currentPos);
                        break;
                }
            }
            DetectCollision(currentPos);
            points.Add(currentPos);
            CleanUp();
        }

        private static void DrawCurrentDirection() {
            Console.SetCursorPosition(80, 0);
            Console.WriteLine("                    ");
            Console.SetCursorPosition(80, 0);
            Console.WriteLine(" Direction: " + currentDirection + " ");
        }

        private static void DrawCurrentSpeed() {
            Console.SetCursorPosition(50, 0);
            Console.WriteLine("                    ");
            Console.SetCursorPosition(50, 0);
            Console.WriteLine(" Speed: " + currentSpeed + " ");
        }

        private static void DrawCurrentScore() {
            Console.SetCursorPosition(20, 0);
            Console.WriteLine(" Score: " + currentScore + " ");
        }

        private static void SetLastKey(Direction direction) {
            switch (direction) {
                case Direction.South:
                    lastKey = new ConsoleKeyInfo((char)ConsoleKey.S, ConsoleKey.S, false, false, false);
                    break;
                case Direction.North:
                    lastKey = new ConsoleKeyInfo((char)ConsoleKey.W, ConsoleKey.W, false, false, false);
                    break;
                case Direction.West:
                    lastKey = new ConsoleKeyInfo((char)ConsoleKey.A, ConsoleKey.A, false, false, false);
                    break;
                case Direction.East:
                    lastKey = new ConsoleKeyInfo((char)ConsoleKey.D, ConsoleKey.D, false, false, false);
                    break;
            }
        }

        private static void SetDirection(Position currentPos) {
            switch (currentDirection) {
                case Direction.South:
                    if (currentPos.Top < windowHeight - 1) {
                        currentPos.Top++;
                        currentDirection = Direction.South;
                        DrawCurrentDirection();
                    }
                    break;
                case Direction.North:
                    if (currentPos.Top > 0) {
                        currentPos.Top--;
                        currentDirection = Direction.North;
                        DrawCurrentDirection();
                    }
                    break;
                case Direction.West:
                    if (currentPos.Left > 0) {
                        currentPos.Left--;
                        currentDirection = Direction.West;
                        DrawCurrentDirection();
                    }
                    break;
                case Direction.East:
                    if (currentPos.Left < windowWidth - 2) {
                        currentPos.Left++;
                        currentDirection = Direction.East;
                        DrawCurrentDirection();
                    }

                    break;
                default:
                    currentPos.Left++;
                    break;
            }
        }
        private static bool UpdateGame() {
            if (DateTime.Now < nextUpdate) return false;
            Console.CursorVisible = false;
            if (apples.Count() < applesCount) {
                while (apples.Count() < applesCount) {
                    foodPosition = new Position() {
                        Left = random.Next(4, windowWidth - 4),
                        Top = random.Next(4, windowHeight - 4)
                    };
                    apples.Add(foodPosition);
                }
            }
            OnChooseDirection(lastKey);
            nextUpdate = DateTime.Now.AddMilliseconds(gameSpeed);
            return true;
        }

        private static void DetectCollision(Position currentPos) {
            // Off screen check     
            if (currentPos.Top < 1) {
                currentPos.Top = windowHeight - 3;
            }
            if (currentPos.Top > windowHeight - 3) {
                currentPos.Top = 1;
            }
            if (currentPos.Left < 1) {
                currentPos.Left = windowWidth - 3;
            }
            if (currentPos.Left > windowWidth - 3) {
                currentPos.Left = 1;
            }

            // Tail collision check
            if (points.Any(p => p.Left == currentPos.Left && p.Top == currentPos.Top)) {
                GameOver();
                alive = false;
            }

            // Apple collision check
            if (apples.Any(a => a.Left == currentPos.Left && a.Top == currentPos.Top)) {
                for (int i = 0; i < apples.Count(); i++) {
                    if (apples[i].Left == currentPos.Left && apples[i].Top == currentPos.Top) {
                        apples.Remove(apples[i]);
                    }
                }
                snakeLength += snakeIncrease;
                currentScore++;
                DrawCurrentScore();
            }
        }
        private static void GameOver() {
            foodPosition = null;
            hasStarted = false;
            points.Clear();
            snakeLength = 5;
            menuState = 2;
            Menu();
        }
        private static void CleanUp() {
            Console.SetCursorPosition(points.First().Left, points.First().Top);
            Console.WriteLine(new string(' ', 1));
            // Console.WriteLine("\b \b");

            while (points.Count() > snakeLength) {
                points.Remove(points.First());
            }
        }
        private static Position GetStartPosition() {
            return new Position() {
                Left = 10,
                Top = 10
            };

        }
    }
}