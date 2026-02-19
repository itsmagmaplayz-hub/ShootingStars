using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ShootingStars.Models;
using ShootingStars.Game;

namespace ShootingStars.UI
{
    public partial class GameWindow : Window
    {
        private GameState? _gameState;
        private GameEngine? _engine;
        private MapGenerator _mapGenerator;
        private Canvas? _gameCanvas;
        private TextBlock? _statusText;
        private TextBlock? _eventLog;
        private Dictionary<int, Ellipse> _playerShapes;
        private System.Windows.Threading.DispatcherTimer? _gameTimer;
        private HashSet<Key> _pressedKeys;

        public GameWindow()
        {
            InitializeComponent();
            _mapGenerator = new MapGenerator();
            _playerShapes = new Dictionary<int, Ellipse>();
            _pressedKeys = new HashSet<Key>();

            this.Title = "ShootingStars Game";
            this.Width = 1200;
            this.Height = 800;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));

            SetupUI();
            ShowGameModeSelection();
        }

        private void SetupUI()
        {
            var mainPanel = new DockPanel();
            mainPanel.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));

            // Top panel with title and status
            var topPanel = new StackPanel { Orientation = Orientation.Horizontal, Height = 50 };
            topPanel.Background = new SolidColorBrush(Color.FromRgb(50, 50, 50));

            var title = new TextBlock
            {
                Text = "SHOOTING STARS",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Cyan),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0)
            };

            _statusText = new TextBlock
            {
                Text = "Ready to play",
                FontSize = 14,
                Foreground = new SolidColorBrush(Colors.LimeGreen),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0)
            };

            topPanel.Children.Add(title);
            topPanel.Children.Add(_statusText);
            DockPanel.SetDock(topPanel, Dock.Top);
            mainPanel.Children.Add(topPanel);

            // Main game area
            var mainArea = new DockPanel();
            mainArea.Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));

            // Right panel for event log
            var rightPanel = new StackPanel { Width = 250 };
            rightPanel.Background = new SolidColorBrush(Color.FromRgb(40, 40, 40));

            var eventTitle = new TextBlock
            {
                Text = "EVENTS",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Yellow),
                Margin = new Thickness(10)
            };

            _eventLog = new TextBlock
            {
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(10),
                MaxHeight = 700
            };

            var scrollViewer = new ScrollViewer
            {
                Content = _eventLog,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            rightPanel.Children.Add(eventTitle);
            rightPanel.Children.Add(scrollViewer);
            DockPanel.SetDock(rightPanel, Dock.Right);
            mainArea.Children.Add(rightPanel);

            // Center - game canvas
            _gameCanvas = new Canvas();
            _gameCanvas.Background = new SolidColorBrush(Color.FromRgb(60, 60, 60));
            mainArea.Children.Add(_gameCanvas);

            mainPanel.Children.Add(mainArea);
            this.Content = mainPanel;
        }

        private void ShowGameModeSelection()
        {
            var gameModes = GameModeFactory.CreateAllGameModes();
            var content = new StackPanel { Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)) };

            var title = new TextBlock
            {
                Text = "SELECT GAME MODE",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Cyan),
                Margin = new Thickness(20),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            content.Children.Add(title);

            var gridPanel = new WrapPanel { Margin = new Thickness(20) };

            for (int i = 0; i < gameModes.Count; i++)
            {
                var button = CreateModeButton(gameModes[i], gameModes.Count);
                gridPanel.Children.Add(button);
            }

            content.Children.Add(gridPanel);
            this.Content = content;
        }

        private Button CreateModeButton(GameMode mode, int totalModes)
        {
            var button = new Button
            {
                Width = 200,
                Height = 100,
                Margin = new Thickness(10),
                Background = new SolidColorBrush(Color.FromRgb(50, 50, 100)),
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand
            };

            var content = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var nameBlock = new TextBlock { Text = mode.Name, FontSize = 14, TextAlignment = TextAlignment.Center };
            var descBlock = new TextBlock
            {
                Text = $"{mode.MaxPlayers} Players",
                FontSize = 11,
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(Colors.LimeGreen),
                Margin = new Thickness(0, 5, 0, 0)
            };

            content.Children.Add(nameBlock);
            content.Children.Add(descBlock);
            button.Content = content;

            button.Click += (s, e) => ShowShooterSelection(mode);
            button.MouseEnter += (s, e) => button.Background = new SolidColorBrush(Color.FromRgb(80, 80, 150));
            button.MouseLeave += (s, e) => button.Background = new SolidColorBrush(Color.FromRgb(50, 50, 100));

            return button;
        }

        private void ShowShooterSelection(GameMode gameMode)
        {
            var shooters = ShooterFactory.CreateAllShooters();
            var selectedShooters = new List<Shooter>();

            var content = new StackPanel
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                Margin = new Thickness(20)
            };

            var title = new TextBlock
            {
                Text = $"SELECT YOUR SHOOTER - {gameMode.Name}",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Cyan),
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            content.Children.Add(title);

            var gridPanel = new WrapPanel { Margin = new Thickness(10) };

            foreach (var shooter in shooters)
            {
                var button = CreateShooterButton(shooter, gameMode, shooters);
                gridPanel.Children.Add(button);
            }

            var scrollViewer = new ScrollViewer { Content = gridPanel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            content.Children.Add(scrollViewer);

            this.Content = content;
        }

        private Button CreateShooterButton(Shooter shooter, GameMode gameMode, List<Shooter> allShooters)
        {
            var button = new Button
            {
                Width = 120,
                Height = 140,
                Margin = new Thickness(10),
                Background = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 11,
                Cursor = Cursors.Hand
            };

            var content = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var nameBlock = new TextBlock { Text = shooter.Name, TextAlignment = TextAlignment.Center, FontWeight = FontWeights.Bold };
            var classBlock = new TextBlock { Text = shooter.Class.ToString(), TextAlignment = TextAlignment.Center, FontSize = 10, Foreground = new SolidColorBrush(Colors.LimeGreen), Margin = new Thickness(0, 3, 0, 0) };
            var hpBlock = new TextBlock { Text = $"HP: {shooter.MaxHealth}", TextAlignment = TextAlignment.Center, FontSize = 9, Margin = new Thickness(0, 3, 0, 0) };

            content.Children.Add(nameBlock);
            content.Children.Add(classBlock);
            content.Children.Add(hpBlock);
            button.Content = content;

            button.Click += (s, e) => StartGame(gameMode, shooter);
            button.MouseEnter += (s, e) => button.Background = new SolidColorBrush(Color.FromRgb(100, 100, 100));
            button.MouseLeave += (s, e) => button.Background = new SolidColorBrush(Color.FromRgb(60, 60, 60));

            return button;
        }

        private void StartGame(GameMode gameMode, Shooter playerShooter)
        {
            // Generate map
            Map gameMap = _mapGenerator.GenerateRandomMap(40, 30);

            // Ensure enough spawn points
            while (gameMap.SpawnPoints.Count < gameMode.MaxPlayers)
            {
                Position randomPos = new Position(_mapGenerator.RandomInt(0, gameMap.Width), _mapGenerator.RandomInt(0, gameMap.Height));
                if (gameMap.IsWalkable(randomPos))
                    gameMap.AddSpawnPoint(randomPos);
            }

            // Create players
            var shooters = ShooterFactory.CreateAllShooters();
            List<Player> players = new List<Player>();

            // Player 1 is controlled by user
            players.Add(new Player(1, "YOU", playerShooter, gameMap.SpawnPoints[0]));

            // AI players
            Random random = new Random();
            for (int i = 1; i < gameMode.MaxPlayers; i++)
            {
                var randomShooter = shooters[random.Next(shooters.Count)];
                players.Add(new Player(i + 1, $"AI-{i}", randomShooter, gameMap.SpawnPoints[i % gameMap.SpawnPoints.Count]));
            }

            // Initialize game
            _gameState = new GameState(gameMode, gameMap, players);
            _engine = new GameEngine(_gameState);
            _engine.Start();

            // Setup input handling
            this.KeyDown += GameWindow_KeyDown;
            this.KeyUp += GameWindow_KeyUp;

            // Start game loop
            _gameTimer = new System.Windows.Threading.DispatcherTimer();
            _gameTimer.Interval = TimeSpan.FromMilliseconds(100);
            _gameTimer.Tick += (s, e) => GameLoop();
            _gameTimer.Start();

            // Redraw UI
            SetupUI();
            RedrawGame();
        }

        private void GameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (_gameState == null) return;
            _pressedKeys.Add(e.Key);
            e.Handled = true;
        }

        private void GameWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (_gameState == null) return;
            _pressedKeys.Remove(e.Key);
            e.Handled = true;
        }

        private void ProcessPlayerInput()
        {
            if (_gameState == null || _engine == null) return;

            var player1 = _gameState.Players[0];
            if (!player1.IsAlive) return;

            Position newPos = new Position(player1.Position.X, player1.Position.Y);

            // WASD or arrow keys for movement
            if (_pressedKeys.Contains(Key.W) || _pressedKeys.Contains(Key.Up))
                newPos = new Position(newPos.X, newPos.Y - 1);
            else if (_pressedKeys.Contains(Key.S) || _pressedKeys.Contains(Key.Down))
                newPos = new Position(newPos.X, newPos.Y + 1);

            if (_pressedKeys.Contains(Key.A) || _pressedKeys.Contains(Key.Left))
                newPos = new Position(newPos.X - 1, newPos.Y);
            else if (_pressedKeys.Contains(Key.D) || _pressedKeys.Contains(Key.Right))
                newPos = new Position(newPos.X + 1, newPos.Y);

            if (_gameState.CurrentMap.IsWalkable(newPos) && !newPos.Equals(player1.Position))
            {
                player1.Position = newPos;
                _gameState.Events.Add(new GameEvent($"YOU moved to {newPos}", GameEventType.PlayerMove));
            }

            // Attack closest enemy with spacebar
            if (_pressedKeys.Contains(Key.Space) && player1.CanAttack())
            {
                var nearestEnemy = _gameState.Players
                    .Where(p => p != player1 && p.IsAlive)
                    .OrderBy(p => player1.Position.DistanceTo(p.Position))
                    .FirstOrDefault();

                if (nearestEnemy != null)
                {
                    _gameState.ProcessAttack(player1, nearestEnemy);
                }
            }

            // Ultimate with E key
            if (_pressedKeys.Contains(Key.E) && player1.CanUltimate())
            {
                var nearestEnemy = _gameState.Players
                    .Where(p => p != player1 && p.IsAlive)
                    .OrderBy(p => player1.Position.DistanceTo(p.Position))
                    .FirstOrDefault();

                if (nearestEnemy != null)
                {
                    _gameState.ProcessUltimate(player1, nearestEnemy.Position);
                }
            }
        }

        private void GameLoop()
        {
            if (_gameState == null || _engine == null) return;

            ProcessPlayerInput();
            _engine.Update();
            RedrawGame();

            if (!_gameState.IsGameRunning)
            {
                _gameTimer?.Stop();
                ShowGameOver();
            }
        }

        private void RedrawGame()
        {
            if (_gameState == null || _gameCanvas == null) return;

            _gameCanvas.Children.Clear();
            _playerShapes.Clear();

            // Draw map
            DrawMap();

            // Draw players
            DrawPlayers();

            // Update status
            UpdateStatus();

            // Update event log
            UpdateEventLog();
        }

        private void DrawMap()
        {
            if (_gameState == null || _gameCanvas == null) return;

            Map map = _gameState.CurrentMap;
            double tileWidth = _gameCanvas.ActualWidth / map.Width;
            double tileHeight = _gameCanvas.ActualHeight / map.Height;

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    var tile = map.Tiles[x, y];
                    var rect = new Rectangle
                    {
                        Width = tileWidth,
                        Height = tileHeight,
                        Fill = GetTileColor(tile.Type)
                    };

                    Canvas.SetLeft(rect, x * tileWidth);
                    Canvas.SetTop(rect, y * tileHeight);
                    _gameCanvas.Children.Add(rect);
                }
            }
        }

        private Brush GetTileColor(TileType type)
        {
            return type switch
            {
                TileType.Grass => new SolidColorBrush(Color.FromRgb(34, 139, 34)),
                TileType.Wall => new SolidColorBrush(Color.FromRgb(128, 128, 128)),
                TileType.Water => new SolidColorBrush(Color.FromRgb(30, 144, 255)),
                TileType.Rock => new SolidColorBrush(Color.FromRgb(105, 105, 105)),
                TileType.Bush => new SolidColorBrush(Color.FromRgb(0, 100, 0)),
                TileType.Lava => new SolidColorBrush(Color.FromRgb(255, 69, 0)),
                TileType.Sand => new SolidColorBrush(Color.FromRgb(210, 180, 140)),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }

        private void DrawPlayers()
        {
            if (_gameState == null || _gameCanvas == null) return;

            double tileWidth = _gameCanvas.ActualWidth / _gameState.CurrentMap.Width;
            double tileHeight = _gameCanvas.ActualHeight / _gameState.CurrentMap.Height;

            foreach (var player in _gameState.Players)
            {
                if (!player.IsAlive) continue;

                var circle = new Ellipse
                {
                    Width = tileWidth * 0.8,
                    Height = tileHeight * 0.8,
                    Fill = player.Id == 1 ? new SolidColorBrush(Colors.Yellow) : new SolidColorBrush(Colors.Red),
                    Stroke = new SolidColorBrush(Colors.White),
                    StrokeThickness = 2
                };

                double x = player.Position.X * tileWidth + (tileWidth - circle.Width) / 2;
                double y = player.Position.Y * tileHeight + (tileHeight - circle.Height) / 2;

                Canvas.SetLeft(circle, x);
                Canvas.SetTop(circle, y);
                _gameCanvas.Children.Add(circle);

                _playerShapes[player.Id] = circle;

                // Draw player name
                var nameText = new TextBlock
                {
                    Text = player.Name,
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.White),
                    TextAlignment = TextAlignment.Center
                };

                Canvas.SetLeft(nameText, x);
                Canvas.SetTop(nameText, y + tileHeight * 0.5);
                _gameCanvas.Children.Add(nameText);
            }
        }

        private void UpdateStatus()
        {
            if (_gameState == null || _statusText == null) return;

            var player1 = _gameState.Players[0];
            string status = $"HP: {player1.CurrentHealth}/{player1.Shooter.MaxHealth} | ULT: {player1.UltimateCharge}/100 | " +
                          $"Time: {_gameState.TimeRemaining}s | Alive: {_gameState.Players.Count(p => p.IsAlive)}/{_gameState.Players.Count}";
            _statusText.Text = status;
        }

        private void UpdateEventLog()
        {
            if (_gameState == null || _eventLog == null) return;

            var recentEvents = _gameState.Events.TakeLast(15).ToList();
            _eventLog.Text = string.Join("\n", recentEvents.Select(e => $"• {e.Message}"));
        }

        private void ShowGameOver()
        {
            if (_gameState == null) return;

            var content = new StackPanel
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var gameOverText = new TextBlock
            {
                Text = "GAME OVER",
                FontSize = 48,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Red),
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(20)
            };
            content.Children.Add(gameOverText);

            var winner = _gameState.GetWinner();
            TextBlock resultText;
            if (winner != null && winner.Id == 1)
            {
                resultText = new TextBlock
                {
                    Text = "YOU WON!",
                    FontSize = 32,
                    FontWeight = FontWeights.Bold,
                    Foreground = new SolidColorBrush(Colors.LimeGreen),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(20)
                };
            }
            else if (winner != null)
            {
                resultText = new TextBlock
                {
                    Text = $"Winner: {winner.Name}",
                    FontSize = 24,
                    Foreground = new SolidColorBrush(Colors.Yellow),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(20)
                };
            }
            else
            {
                resultText = new TextBlock
                {
                    Text = "Time's Up - Draw!",
                    FontSize = 24,
                    Foreground = new SolidColorBrush(Colors.Orange),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(20)
                };
            }
            content.Children.Add(resultText);

            var playAgainButton = new Button
            {
                Content = "PLAY AGAIN",
                Width = 150,
                Height = 50,
                FontSize = 16,
                Margin = new Thickness(20),
                Background = new SolidColorBrush(Color.FromRgb(50, 150, 50)),
                Foreground = new SolidColorBrush(Colors.White),
                Cursor = Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            playAgainButton.Click += (s, e) => ShowGameModeSelection();
            content.Children.Add(playAgainButton);

            this.Content = content;
            this.KeyDown -= GameWindow_KeyDown;
            this.KeyUp -= GameWindow_KeyUp;
        }
    }
}
