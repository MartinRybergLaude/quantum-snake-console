using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Media;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;

namespace QuantumSnakeConsole {
    internal class Program {

        private class Position {
            public int Left;
            public int Top;
            private int G;
            private int H;
            private int F;
            private Position Previous;

            public void SetPrevious(Position previousPos) {
                Previous = previousPos;
            }

            public Position GetPrevious() {
                return Previous;
            }

            public int GetX() {
                return Left;
            }

            public int GetY() {
                return Top;
            }

            public int GetG() {
                return G;
            }

            public int GetH() {
                return H;
            }

            public int GetF() {
                return G + H;
            }

            public void SetX(int x) {
                this.Left = x;
            }

            public void SetY(int y) {
                this.Top = y;
            }

            public void SetG(Position current) {
                this.G = CalculateG(current);
            }

            public void SetH(Position goal) {
                this.H = Math.Abs(GetX() - goal.GetX()) + Math.Abs(GetY() - goal.GetY());
            }

            public int CalculateG(Position current) {
                return current.GetPrevious().GetG() + 1;
            }

            public override int GetHashCode() {
                int result = GetX();
                result = 31 * result + GetY();
                return result;
            }

            public override bool Equals(object o) {
                if (this == o) return true;
                if (o == null || GetType() != o.GetType()) return false;

                Position that = (Position)o;

                if (GetX() != that.GetX()) return false;
                return GetY() == that.GetY();
            }
        }
        private class FComparator : IComparer<Position> {

            int IComparer<Position>.Compare(Position x, Position y) {
                if (x.GetF() < y.GetF()) {
                    return -1;
                } else if (x.GetF() == y.GetF()) {
                    return 0;
                } else {
                    return 1;
                }
            
            }
        }

    private const string SnakeCharacter = "██";
        private const string AppleCharacter = "██";
        private const string WallCharacter = "██";
        private const int SnakeIncrease = 3;
        private const int ApplesCount = 4;

        private const ConsoleColor LogoTopColor = ConsoleColor.Cyan;
        private const ConsoleColor LogoBottomColor = ConsoleColor.Magenta;
        private const ConsoleColor MenuItemColor = ConsoleColor.Magenta;
        private const ConsoleColor ScoreColor = ConsoleColor.Cyan;
        private const ConsoleColor ErrorColor = ConsoleColor.Red;

        private const ConsoleColor WallColor = ConsoleColor.Red;
        private const ConsoleColor SnakeColor = ConsoleColor.Cyan;
        private const ConsoleColor EnemyColor = ConsoleColor.White;
        private const ConsoleColor FoodColor = ConsoleColor.Magenta;

        private static int snakeLength = 5;
        private static int enemyLength = 5;
        private static int currentScore = 0;
        private static int gameSpeed = 50;
        private static int menuState = 0;

        private static List<Position> snakePoints = new List<Position>();
        private static List<Position> enemyPoints = new List<Position>();
        private static List<Position> apples = new List<Position>();
        private static Position foodPosition;
        private static Random random = new Random();
        private static DateTime nextUpdate = DateTime.MinValue;
        private static bool alive;
        private static ConsoleKeyInfo lastKey;
        private static bool hasStarted = false;

        private enum Speed {
            VerySlow,
            Slow,
            Normal,
            Fast,
            VeryFast
        }

        private static Speed currentSpeed;

        private enum Direction {
            North,
            South,
            East,
            West
        };

        private static Direction currentDirection;
        private static Direction enemyDirection;

        private enum GameMode {
            Classic,
            Vscpu
        };

        private static GameMode currentGameMode;

        private enum NodeState {
            Untested,
            Open,
            Closed
        }

        private static int windowWidth = Console.WindowWidth;
        private static int windowHeight = Console.WindowHeight;
        private static int arenaWidth;
        private static int arenaHeight;

        private static readonly string[] Logo = new string[] {
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
                    Console.ForegroundColor = MenuItemColor;
                    Console.WriteLine("Press 1 to start Classic");
                    Console.SetCursorPosition(40, 16);
                    Console.WriteLine("Press 2 to start VSCPU");
                    break;
                case 1:
                    Console.SetCursorPosition(40, 18);
                    Console.ForegroundColor = ErrorColor;
                    Console.WriteLine("Not implemented yet!");
                    break;
                case 2:
                    Console.Clear();
                    PrintLogo();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.SetCursorPosition(40, 12);
                    Console.WriteLine("Score: " + currentScore);
                    Console.SetCursorPosition(40, 14);
                    Console.ForegroundColor = MenuItemColor;
                    Console.WriteLine("Press 1 to start Classic");
                    Console.SetCursorPosition(40, 16);
                    Console.WriteLine("Press 2 to start VSCPU");
                    Console.SetCursorPosition(40, 18);
                    Console.ForegroundColor = ErrorColor;
                    Console.WriteLine("You lost!");
                    break;
                default:
                    PrintLogo();
                    Console.SetCursorPosition(40, 14);
                    Console.ForegroundColor = MenuItemColor;
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
                    currentGameMode = GameMode.Classic;
                    GameStart();
                    break;
                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    menuState = 0;
                    Console.Clear();
                    currentGameMode = GameMode.Vscpu;
                    GameStart();
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
            windowWidth = windowWidth / 2;
            arenaWidth = windowWidth - 3;
            arenaHeight = windowHeight - 4;
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
            Console.ForegroundColor = LogoTopColor;
            Console.SetCursorPosition(0, 3);
            for (int i = 0; i < Logo.Length; i++) {
                for (int j = 0; j < Logo[i].Length; j++) {
                    if (i >= 3 && i < 7 && j > 2 && j < 5) {
                        Console.ForegroundColor = LogoBottomColor;
                    }

                    Console.Write(Logo[i][j]);
                }

                Console.WriteLine();
            }

            Console.ResetColor();
        }

        private static void DrawWalls() {
            Console.ForegroundColor = WallColor;
            for (var i = 0; i < windowWidth; i++) {
                DrawPixel(i, 1, WallCharacter, WallColor);
                DrawPixel(i, windowHeight - 2, WallCharacter, WallColor);
            }

            for (var i = 0; i < windowHeight - 2; i++) {
                DrawPixel(windowWidth - 1, i, WallCharacter, WallColor);
                DrawPixel(0, i, WallCharacter, WallColor);
            }
        }

        private static void OnDraw() {
            foreach (var apple in apples) {
                Console.ForegroundColor = FoodColor;
                DrawPixel(apple.Left, apple.Top, AppleCharacter, FoodColor);
            }

            if (snakePoints.Count == 0) return;
            DrawPixel(snakePoints.Last().Left, snakePoints.Last().Top, SnakeCharacter, SnakeColor);
            if (currentGameMode == GameMode.Vscpu) {
                DrawPixel(enemyPoints.Last().Left, enemyPoints.Last().Top, SnakeCharacter, EnemyColor);
            }
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
            if (snakePoints.Count != 0) {
                currentPos = new Position() {
                    Left = snakePoints.Last().Left,
                    Top = snakePoints.Last().Top
                };
            }
            else {
                currentPos = GetStartPosition();
            }

            if (!hasStarted) {
                currentPos.Left++;
            }
            else {
                // Choose Direction depending on key
                switch (key.Key) {
                    case ConsoleKey.S:
                    case ConsoleKey.DownArrow:
                        if ((currentPos.Top < windowHeight - 1) && (currentDirection != Direction.North)) {
                            currentPos.Top++;
                            currentDirection = Direction.South;
                            DrawCurrentDirection();
                        }
                        else {
                            SetDirection(currentPos);
                        }

                        break;
                    case ConsoleKey.W:
                    case ConsoleKey.UpArrow:
                        if ((currentPos.Top > 0) && (currentDirection != Direction.South)) {
                            currentPos.Top--;
                            currentDirection = Direction.North;
                            DrawCurrentDirection();
                        }
                        else {
                            SetDirection(currentPos);
                        }

                        break;
                    case ConsoleKey.A:
                    case ConsoleKey.LeftArrow:
                        if ((currentPos.Left > 0) && (currentDirection != Direction.East)) {
                            currentPos.Left--;
                            currentDirection = Direction.West;
                            DrawCurrentDirection();
                        }
                        else {
                            SetDirection(currentPos);
                        }

                        break;
                    case ConsoleKey.D:
                    case ConsoleKey.RightArrow:
                        if ((currentPos.Left < windowWidth - 2) && (currentDirection != Direction.West)) {
                            currentPos.Left++;
                            currentDirection = Direction.East;
                            DrawCurrentDirection();
                        }
                        else {
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
            snakePoints.Add(currentPos);
        }

        private static void DrawCurrentDirection() {
            DrawPixel(40, 0, "                    ");
            DrawPixel(40, 0, " Direction: " + currentDirection + " ", ScoreColor);
        }

        private static void DrawCurrentSpeed() {
            DrawPixel(25, 0, "                    ");
            DrawPixel(25, 0, " Speed: " + currentSpeed + " ", ScoreColor);
        }

        private static void DrawCurrentScore() {
            DrawPixel(10, 0, "Score: " + currentScore + " ", ScoreColor);
        }

        private static void DrawPixel(double x, double y, string text, ConsoleColor color = ConsoleColor.White) {
            Console.ForegroundColor = color;
            Console.SetCursorPosition(Convert.ToInt32(x * 2), Convert.ToInt32(y));
            Console.WriteLine(text);
        }

        private static void SetLastKey(Direction direction) {
            switch (direction) {
                case Direction.South:
                    lastKey = new ConsoleKeyInfo((char) ConsoleKey.S, ConsoleKey.S, false, false, false);
                    break;
                case Direction.North:
                    lastKey = new ConsoleKeyInfo((char) ConsoleKey.W, ConsoleKey.W, false, false, false);
                    break;
                case Direction.West:
                    lastKey = new ConsoleKeyInfo((char) ConsoleKey.A, ConsoleKey.A, false, false, false);
                    break;
                case Direction.East:
                    lastKey = new ConsoleKeyInfo((char) ConsoleKey.D, ConsoleKey.D, false, false, false);
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
            if (apples.Count() < ApplesCount) {
                while (apples.Count() < ApplesCount) {
                    foodPosition = new Position() {
                        Left = random.Next(4, arenaWidth),
                        Top = random.Next(4, arenaHeight)
                    };
                    while (snakePoints.Any(p => p.Left == foodPosition.Left && p.Top == foodPosition.Top)) {
                        foodPosition = new Position() {
                            Left = random.Next(4, arenaWidth),
                            Top = random.Next(4, arenaHeight)
                        };
                    }

                    apples.Add(foodPosition);
                }
            }
            
            if (currentGameMode == GameMode.Vscpu) {
                EnemyAlgorithm();
            }
            OnChooseDirection(lastKey);
            CleanUp();
            nextUpdate = DateTime.Now.AddMilliseconds(gameSpeed);
            return true;
        }

        private static void DetectCollision(Position currentPos) {
            // Off screen check     
            if (currentPos.Top < 1) {
                currentPos.Top = arenaHeight;
            }

            if (currentPos.Top > arenaHeight) {
                currentPos.Top = 1;
            }

            if (currentPos.Left < 1) {
                currentPos.Left = arenaWidth;
            }

            if (currentPos.Left > arenaWidth) {
                currentPos.Left = 1;
            }

            // Tail collision check
            if (snakePoints.Any(p => p.Left == currentPos.Left && p.Top == currentPos.Top)) {
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
                snakeLength += SnakeIncrease;
                currentScore++;
                DrawCurrentScore();
            }
            if (currentGameMode == GameMode.Vscpu && apples.Any(a => a.Left == enemyPoints.Last().Left && a.Top == enemyPoints.Last().Top)) {
                for (int i = 0; i < apples.Count(); i++) {
                    if (apples[i].Left == enemyPoints.Last().Left && apples[i].Top == enemyPoints.Last().Top) {
                        apples.Remove(apples[i]);
                        randomApple = random.Next(0, 3);
                    }
                }

                enemyLength += SnakeIncrease;
            }
            
            // Enemy collision check
            if (enemyPoints.Any(p => p.Left == currentPos.Left && p.Top == currentPos.Top)) {
                GameOver();
                alive = false;
            }
        }

        private static void GameOver() {
            foodPosition = null;
            hasStarted = false;
            snakePoints.Clear();
            enemyPoints.Clear();
            snakeLength = 5;
            enemyLength = 5;
            menuState = 2;
            Menu();
        }

        private static void CleanUp() {
            DrawPixel(snakePoints.First().Left, snakePoints.First().Top, "  ");
            while (snakePoints.Count() > snakeLength) {
                snakePoints.Remove(snakePoints.First());
            }

            if (currentGameMode == GameMode.Vscpu) {
                DrawPixel(enemyPoints.First().Left, enemyPoints.First().Top, "  ");
                while (enemyPoints.Count() > enemyLength) {
                    enemyPoints.Remove(enemyPoints.First());
                }
            }
        }

        private static void CleanUpEnemy() {
            enemyPoints.Clear();
            enemyLength = 5;
        }

        private static Position GetStartPosition() {
            return new Position() {
                Left = 10,
                Top = 10
            };
        }
        private static Position GetEnemyStartPosition() {
            return new Position() {
                Left = 10,
                Top = 20
            };
        }

        private static int randomApple = 1;
        private static SortedSet<Position> openList;
        private static HashSet<Position> closedList;
        private static List<Position> Path;
        private static void EnemyAlgorithm() {
            //System.Diagnostics.Debug.WriteLine("Init reached");
            Position currentPos;
            Position goalPos;
            goalPos = apples[randomApple];
            if (enemyPoints.Count != 0) {
                currentPos = new Position() {
                    Left = enemyPoints.Last().Left,
                    Top = enemyPoints.Last().Top
                };
            }
            else {
                currentPos = GetEnemyStartPosition();
            }

            Path = FinishedPath(currentPos, goalPos);
            ChooseEnemyDirection(Path, currentPos, goalPos);
        }

        private static void ChooseEnemyDirection(List<Position> path, Position currentPos, Position goalPos) {
            //System.Diagnostics.Debug.WriteLine("Choose direction reached");
            if (path == null) {
                path = FinishedPath(currentPos, goalPos);
                Position goTo = path[0];
                int X = goTo.GetX();
                int Y = goTo.GetY();
                ListDirectionSelector(X, Y, goTo);
                path.RemoveAt(0);
            }

            if (!path.Any()) {
                List<Position> randomPositions = GetAdjacent(currentPos, true);
                if (randomPositions.Any()) {
                    int index = random.Next(randomPositions.Count);
                    Position tempPosition = randomPositions[index];
                    int X = tempPosition.GetX();
                    int Y = tempPosition.GetY();
                    ListDirectionSelector(X, Y, tempPosition);
                    path.RemoveAt(0);
                }
                else {
                    CleanUpEnemy();
                }
            }
            else {
                Position goTo = Path[0];
                int X = goTo.GetX();
                int Y = goTo.GetY();
                ListDirectionSelector(X, Y, currentPos);
            }
        }

        private static void ListDirectionSelector(int X, int Y, Position position) {
            if (X > position.GetX()) {
                enemyDirection = Direction.East;
                position.Left++;
            } else if (X < position.GetX()) {
                enemyDirection = Direction.West;
                position.Left--;
            } else if (Y > position.GetY()) {
                enemyDirection = Direction.South;
                position.Top++;
            } else if (Y < position.GetY()) {
                enemyDirection = Direction.North;
                position.Top--;
            }
            enemyPoints.Add(position);
        }

        private static List<Position> FinishedPath(Position start, Position goal) {
            openList = new SortedSet<Position>(new FComparator());
            closedList = new HashSet<Position>();
            Position currentNode;
            openList.Add(start);
            bool done = false;
            while (openList.Any()) {
                currentNode = openList.Count != 0 ? LowestFInOpen() : start;
                closedList.Add(currentNode);
                openList.Remove(currentNode);
               
                if (closedList.Any(e => e.Left == goal.Left && e.Top == goal.Top)) {
                    return GetPath(start, currentNode);
                }

                List<Position> adjacentNodes = GetAdjacent(currentNode, false);
                foreach (var currentAdj in adjacentNodes) {
                    if (!openList.Contains(currentAdj)) {
                        currentAdj.SetPrevious(currentNode);
                        currentAdj.SetH(goal);
                        currentAdj.SetG(currentAdj);
                        openList.Add(currentAdj);
                    } else {
                        if (currentAdj.GetG() > currentAdj.CalculateG(currentNode)) {
                            currentAdj.SetPrevious(currentNode);
                            currentAdj.SetG(currentNode);
                        }
                    }
                }
                if (!openList.Any()) {
                    CleanUpEnemy();
                }
            }
            return null;
        }
        private static Position LowestFInOpen() {
            Position cheapest = openList.First();
            return cheapest;
        }
        private static List<Position> GetPath(Position start, Position goal) {
            List<Position> path = new List<Position>();

            Position curr = goal;
            bool done = false;
            while (!done) {
                path.Insert(0, curr);
                curr = curr.GetPrevious();
                if (curr.Equals(start)) {
                    done = true;
                }
            }
            return path;
        }
        private static List<Position> GetAdjacent(Position node, bool isRandom) {
            int X = node.GetX();
            int Y = node.GetY();

            List<Position> adj = new List<Position>();
            Position temp1 = new Position() {
                Left = X - 1,
                Top = Y
            };
            Position temp2 = new Position() {
                Left = X + 1,
                Top = Y
            };
            Position temp3 = new Position() {
                Left = X,
                Top = Y - 1
            };
            Position temp4 = new Position() {
                Left = X,
                Top = Y + 1
            };
            if (!isRandom) {
                if (IsWalkable(temp1) && !closedList.Contains(temp1)) {
                    adj.Add(temp1);
                }
                if (IsWalkable(temp2) && !closedList.Contains(temp2)) {
                    adj.Add(temp2);
                }
                if (IsWalkable(temp3) && !closedList.Contains(temp3)) {
                    adj.Add(temp3);
                }
                if (IsWalkable(temp4) && !closedList.Contains(temp4)) {
                    adj.Add(temp4);
                }
            }
            else {
                if (IsWalkable(temp1)) {
                    adj.Add(temp1);
                }
                if (IsWalkable(temp2)) {
                    adj.Add(temp2);
                }
                if (IsWalkable(temp3)) {
                    adj.Add(temp3);
                }
                if (IsWalkable(temp4)) {
                    adj.Add(temp4);
                }
            }

            return adj;
        }
        private static bool IsWalkable(Position x) {
            if (x.Left > arenaWidth
                || x.Left < 1
                || x.Top < 1
                || x.Top > arenaHeight
                || (enemyPoints.Any(e => e.Left == x.Left && e.Top == x.Top))
                || (snakePoints.Any(e => e.Left == x.Left && e.Top == x.Top))) {
               // System.Diagnostics.Debug.WriteLine("isnt walkable");
                return false;
            }
           // System.Diagnostics.Debug.WriteLine("is walkable");
            return true;

        }
    }
}