using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        private Models.Account? _currentAccount; // current logged-in account
        // Tunable parameters
        private double PlayerSpeed = 1.0; // tiles per tick for player (increased)
        private double LerpAlpha = 0.01;   // render smoothing factor (0..1)
        private int _playerDeathFrame = -1; // track when player died for 3-second delay
        private const int DEATH_DELAY_FRAMES = 30; // 30 frames at ~10 FPS = ~3 seconds

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
            // prompt user to pick or create an account before doing anything else
            ShowLogin();
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
            // ensure we have an account before allowing mode selection
            if (_currentAccount == null)
            {
                ShowAccountSelection();
                return;
            }
            var gameModes = GameModeFactory.CreateAllGameModes();
            var content = new StackPanel { Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)) };

            // show current account and trophies if available
            if (_currentAccount != null)
            {
                var acctInfo = new TextBlock
                {
                    Text = $"User: {_currentAccount.Username}   Trophies: {_currentAccount.Trophies}   Gems: {_currentAccount.Gems}   XP: {_currentAccount.ShootPassXP}",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Colors.LimeGreen),
                    Margin = new Thickness(20, 5, 20, 10),
                    TextAlignment = TextAlignment.Center
                };
                content.Children.Add(acctInfo);

                var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(20) };
                
                var ownedBtn = new Button
                {
                    Content = "Collection",
                    Width = 120,
                    Height = 40,
                    Margin = new Thickness(5, 0, 5, 0),
                    Background = new SolidColorBrush(Color.FromRgb(80, 120, 80)),
                    Foreground = new SolidColorBrush(Colors.White),
                    Cursor = Cursors.Hand
                };
                ownedBtn.Click += (s, e) => ShowOwnedShooters();
                btnPanel.Children.Add(ownedBtn);

                var passBtn = new Button
                {
                    Content = "ShootPass",
                    Width = 120,
                    Height = 40,
                    Margin = new Thickness(5, 0, 5, 0),
                    Background = new SolidColorBrush(Color.FromRgb(80, 80, 120)),
                    Foreground = new SolidColorBrush(Colors.White),
                    Cursor = Cursors.Hand
                };
                passBtn.Click += (s, e) => ShowShootPass();
                btnPanel.Children.Add(passBtn);

                var questBtn = new Button
                {
                    Content = "Quests",
                    Width = 120,
                    Height = 40,
                    Margin = new Thickness(5, 0, 5, 0),
                    Background = new SolidColorBrush(Color.FromRgb(120, 80, 80)),
                    Foreground = new SolidColorBrush(Colors.White),
                    Cursor = Cursors.Hand
                };
                questBtn.Click += (s, e) => ShowQuest();
                btnPanel.Children.Add(questBtn);

                var leaderboardBtn = new Button
                {
                    Content = "Leaderboard",
                    Width = 150,
                    Height = 40,
                    Margin = new Thickness(10, 0, 10, 0),
                    Background = new SolidColorBrush(Color.FromRgb(100, 100, 150)),
                    Foreground = new SolidColorBrush(Colors.White),
                    Cursor = Cursors.Hand
                };
                leaderboardBtn.Click += (s, e) => ShowLeaderboard();
                btnPanel.Children.Add(leaderboardBtn);
                
                var logoutBtn = new Button
                {
                    Content = "Change Account",
                    Width = 150,
                    Height = 40,
                    Margin = new Thickness(10, 0, 10, 0),
                    Background = new SolidColorBrush(Color.FromRgb(150, 50, 50)),
                    Foreground = new SolidColorBrush(Colors.White),
                    Cursor = Cursors.Hand
                };
                logoutBtn.Click += (s, e) =>
                {
                    _currentAccount = null;
                    ShowAccountSelection();
                };
                btnPanel.Children.Add(logoutBtn);
                content.Children.Add(btnPanel);
            }

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
            var allShooters = ShooterFactory.CreateAllShooters();
            var shooters = allShooters.Where(s => _currentAccount.OwnedShooters.Contains(s.Name)).ToList();
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
            // show icon if available
            var iconBlock = new TextBlock { Text = shooter.Icon, FontSize = 24, TextAlignment = TextAlignment.Center };
            var nameBlock = new TextBlock { Text = shooter.Name, TextAlignment = TextAlignment.Center, FontWeight = FontWeights.Bold };
            var classBlock = new TextBlock { Text = shooter.Class.ToString(), TextAlignment = TextAlignment.Center, FontSize = 10, Foreground = new SolidColorBrush(Colors.LimeGreen), Margin = new Thickness(0, 3, 0, 0) };
            var hpBlock = new TextBlock { Text = $"HP: {shooter.MaxHealth}", TextAlignment = TextAlignment.Center, FontSize = 9, Margin = new Thickness(0, 3, 0, 0) };

            content.Children.Add(iconBlock);
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
            _engine = new GameEngine(_gameState) { AISpeed = 0.8 };
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

            // Don't process movement if frozen from attacking
            if (player1.AttackFreezeTimer > 0)
            {
                // Still allow attack/ultimate input even when frozen
                if (_pressedKeys.Contains(Key.Space) && player1.CanAttack())
                {
                    var nearestEnemy = _gameState.Players
                        .Where(p => p != player1 && p.IsAlive)
                        .OrderBy(p => player1.PositionF.DistanceTo(p.PositionF))
                        .FirstOrDefault();

                    if (nearestEnemy != null)
                    {
                        _gameState.ProcessAttack(player1, nearestEnemy);
                    }
                }

                if (_pressedKeys.Contains(Key.E) && player1.CanUltimate())
                {
                    var nearestEnemy = _gameState.Players
                        .Where(p => p != player1 && p.IsAlive)
                        .OrderBy(p => player1.PositionF.DistanceTo(p.PositionF))
                        .FirstOrDefault();

                    if (nearestEnemy != null)
                    {
                        _gameState.ProcessUltimate(player1, nearestEnemy.Position);
                    }
                }
                return; // freeze prevents movement
            }

            Position newPos = new Position(player1.Position.X, player1.Position.Y);

            // WASD or arrow keys for smooth movement
            var moveDir = new Vec(0,0);
            if (_pressedKeys.Contains(Key.W) || _pressedKeys.Contains(Key.Up))
                moveDir = new Vec(moveDir.X, moveDir.Y - 1);
            else if (_pressedKeys.Contains(Key.S) || _pressedKeys.Contains(Key.Down))
                moveDir = new Vec(moveDir.X, moveDir.Y + 1);

            if (_pressedKeys.Contains(Key.A) || _pressedKeys.Contains(Key.Left))
                moveDir = new Vec(moveDir.X - 1, moveDir.Y);
            else if (_pressedKeys.Contains(Key.D) || _pressedKeys.Contains(Key.Right))
                moveDir = new Vec(moveDir.X + 1, moveDir.Y);

            if (moveDir.X != 0 || moveDir.Y != 0)
            {
                double speed = PlayerSpeed;
                double nx = player1.PositionF.X + moveDir.X * speed;
                double ny = player1.PositionF.Y + moveDir.Y * speed;
                var candidate = new Vec(nx, ny);
                if (_gameState.CurrentMap.IsWalkable(candidate))
                {
                    player1.PositionF = candidate;
                    player1.Position = new Position((int)System.Math.Floor(player1.PositionF.X), (int)System.Math.Floor(player1.PositionF.Y));
                    _gameState.Events.Add(new GameEvent($"YOU moved to {player1.Position}", GameEventType.PlayerMove));
                }
                // if the target tile is blocked we simply do nothing – no sliding or jitter
            }

            // Attack closest enemy with spacebar
            if (_pressedKeys.Contains(Key.Space) && player1.CanAttack())
            {
                var nearestEnemy = _gameState.Players
                    .Where(p => p != player1 && p.IsAlive)
                    .OrderBy(p => player1.PositionF.DistanceTo(p.PositionF))
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
                    .OrderBy(p => player1.PositionF.DistanceTo(p.PositionF))
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

            // Check if player is dead and waiting for leaderboard
            if (_playerDeathFrame >= 0 && _gameState.CurrentFrame - _playerDeathFrame >= DEATH_DELAY_FRAMES)
            {
                _gameTimer?.Stop();
                ShowLeaderboard();
            }
            else if (!_gameState.IsGameRunning && _playerDeathFrame < 0)
            {
                // Mark player death if player is dead
                if (!_gameState.Players[0].IsAlive)
                {
                    _playerDeathFrame = _gameState.CurrentFrame;
                }
                else
                {
                    // Player is alive and game is not running anymore (won or time limit)
                    _gameTimer?.Stop();
                    ShowLeaderboard();
                }
            }
        }

        private void RedrawGame()
        {
            if (_gameState == null || _gameCanvas == null) return;

            _gameCanvas.Children.Clear();
            _playerShapes.Clear();

            // Draw map
            DrawMap();
            
            // Draw poison zone
            DrawPoisonZone();

            // draw range circles for all players (under players so they don't cover them)
            DrawRangeCircles();

            // Smooth render positions toward logical positions
            if (_gameState != null)
            {
                foreach (var p in _gameState.Players)
                {
                    p.RenderPosition = new Vec(
                        p.RenderPosition.X + (p.PositionF.X - p.RenderPosition.X) * LerpAlpha,
                        p.RenderPosition.Y + (p.PositionF.Y - p.RenderPosition.Y) * LerpAlpha
                    );
                }
            }

            // Draw players
            DrawPlayers();

            // draw shot/projectile effects
            DrawShotAnimations();

            // show ultimate charge bar for player 1
            DrawUltChargeBar();

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

                double x = player.RenderPosition.X * tileWidth - (circle.Width / 2);
                double y = player.RenderPosition.Y * tileHeight - (circle.Height / 2);

                Canvas.SetLeft(circle, x);
                Canvas.SetTop(circle, y);
                _gameCanvas.Children.Add(circle);

                _playerShapes[player.Id] = circle;

                // icon above character
                if (!string.IsNullOrEmpty(player.Shooter.Icon))
                {
                    var iconText = new TextBlock
                    {
                        Text = player.Shooter.Icon,
                        FontSize = 16,
                        TextAlignment = TextAlignment.Center
                    };
                    Canvas.SetLeft(iconText, x);
                    Canvas.SetTop(iconText, y - 16);
                    _gameCanvas.Children.Add(iconText);
                }

                // health bar
                double healthRatio = (double)player.CurrentHealth / player.Shooter.MaxHealth;
                var healthBar = new Rectangle
                {
                    Width = tileWidth * 0.6 * healthRatio,
                    Height = 6,
                    Fill = new SolidColorBrush(Color.FromRgb(0, 200, 0)),
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1
                };
                Canvas.SetLeft(healthBar, x + (tileWidth * 0.8 - healthBar.Width) / 2);
                // position bar above icon/text (higher than before)
                Canvas.SetTop(healthBar, y - 22);
                _gameCanvas.Children.Add(healthBar);

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

                // ammo indicator (only for players with limited ammo)
                if (player.MaxAmmo > 0)
                {
                    var ammoText = new TextBlock
                    {
                        Text = $"AMMO: {player.CurrentAmmo}",
                        FontSize = 8,
                        Foreground = new SolidColorBrush(Colors.LightYellow),
                        TextAlignment = TextAlignment.Center
                    };
                    Canvas.SetLeft(ammoText, x);
                    Canvas.SetTop(ammoText, y + tileHeight * 0.6);
                    _gameCanvas.Children.Add(ammoText);
                }
            }
        }

        private void UpdateStatus()
        {
            if (_gameState == null || _statusText == null) return;

            var player1 = _gameState.Players[0];
            string status = $"HP: {player1.CurrentHealth}/{player1.Shooter.MaxHealth} | AMMO: {player1.CurrentAmmo}/{player1.MaxAmmo} | ULT: {player1.UltimateCharge}/100 | " +
                          $"Time: {_gameState.TimeRemaining}s | Alive: {_gameState.Players.Count(p => p.IsAlive)}/{_gameState.Players.Count}";
            _statusText.Text = status;
        }

        private void DrawUltChargeBar()
        {
            if (_gameState == null || _gameCanvas == null) return;
            var player1 = _gameState.Players[0];
            double width = 200;
            double height = 10;
            double x = 20;
            double y = 60;
            double ratio = player1.UltimateCharge / 100.0;
            var bg = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = new SolidColorBrush(Color.FromRgb(50, 50, 50)),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 1
            };
            Canvas.SetLeft(bg, x);
            Canvas.SetTop(bg, y);
            _gameCanvas.Children.Add(bg);
            var fg = new Rectangle
            {
                Width = width * ratio,
                Height = height,
                Fill = new SolidColorBrush(Colors.OrangeRed)
            };
            Canvas.SetLeft(fg, x);
            Canvas.SetTop(fg, y);
            _gameCanvas.Children.Add(fg);
        }

        private void UpdateEventLog()
        {
            if (_gameState == null || _eventLog == null) return;

            // Only show death/elimination events
            var deathEvents = _gameState.Events.Where(e => e.Type == GameEventType.PlayerEliminated).TakeLast(15).ToList();
            _eventLog.Text = string.Join("\n", deathEvents.Select(e => $"• {e.Message}"));
        }

        // login screen is very simple: ask for a username and load/create the account
        private void ShowLogin()
        {
            ShowAccountSelection();
        }

        private void ShowAccountSelection()
        {
            var content = new StackPanel
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                Margin = new Thickness(20)
            };

            var title = new TextBlock
            {
                Text = "SELECT ACCOUNT",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Cyan),
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            content.Children.Add(title);

            var accounts = Account.GetAllAccounts();
            var scrollPanel = new WrapPanel { Margin = new Thickness(10) };

            foreach (var acc in accounts)
            {
                var btn = new Button
                {
                    Width = 180,
                    Height = 80,
                    Margin = new Thickness(10),
                    Background = new SolidColorBrush(Color.FromRgb(50, 100, 150)),
                    Foreground = new SolidColorBrush(Colors.White),
                    Cursor = Cursors.Hand
                };
                var panel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
                var nameBlock = new TextBlock { Text = acc.Username, FontSize = 14, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center };
                var trophyBlock = new TextBlock { Text = $"🏆 {acc.Trophies}", FontSize = 12, TextAlignment = TextAlignment.Center, Foreground = new SolidColorBrush(Colors.Gold), Margin = new Thickness(0, 5, 0, 0) };
                panel.Children.Add(nameBlock);
                panel.Children.Add(trophyBlock);
                btn.Content = panel;
                btn.Click += (s, e) => { _currentAccount = acc; ShowGameModeSelection(); };
                btn.MouseEnter += (s, e) => btn.Background = new SolidColorBrush(Color.FromRgb(80, 130, 180));
                btn.MouseLeave += (s, e) => btn.Background = new SolidColorBrush(Color.FromRgb(50, 100, 150));
                scrollPanel.Children.Add(btn);
            }

            var scrollViewer = new ScrollViewer { Content = scrollPanel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Height = 300 };
            content.Children.Add(scrollViewer);

            // "Create New Account" button
            var newAcctBtn = new Button
            {
                Content = "+ NEW ACCOUNT",
                Width = 200,
                Height = 50,
                Margin = new Thickness(20),
                Background = new SolidColorBrush(Color.FromRgb(100, 150, 100)),
                Foreground = new SolidColorBrush(Colors.White),
                Cursor = Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            newAcctBtn.Click += (s, e) =>
            {
                ShowNewAccountDialog();
            };
            content.Children.Add(newAcctBtn);

            this.Content = content;
        }

        private void ShowNewAccountDialog()
        {
            var content = new StackPanel
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(20)
            };

            var title = new TextBlock
            {
                Text = "NEW ACCOUNT",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Cyan),
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            content.Children.Add(title);

            var label = new TextBlock
            {
                Text = "Account Name:",
                FontSize = 14,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0, 0, 0, 10)
            };
            content.Children.Add(label);

            var inputBox = new TextBox
            {
                Width = 250,
                Height = 40,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 20)
            };
            content.Children.Add(inputBox);

            var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 20, 0, 0) };
            var createBtn = new Button
            {
                Content = "CREATE",
                Width = 100,
                Height = 40,
                Margin = new Thickness(10),
                Background = new SolidColorBrush(Color.FromRgb(100, 150, 100)),
                Foreground = new SolidColorBrush(Colors.White),
                Cursor = Cursors.Hand
            };
            createBtn.Click += (s, e) =>
            {
                var name = inputBox.Text.Trim();
                if (!string.IsNullOrEmpty(name) && name.Length > 0)
                {
                    _currentAccount = Account.Load(name);
                    ShowGameModeSelection();
                }
            };
            btnPanel.Children.Add(createBtn);

            var backBtn = new Button
            {
                Content = "BACK",
                Width = 100,
                Height = 40,
                Margin = new Thickness(10),
                Background = new SolidColorBrush(Color.FromRgb(150, 50, 50)),
                Foreground = new SolidColorBrush(Colors.White),
                Cursor = Cursors.Hand
            };
            backBtn.Click += (s, e) => ShowAccountSelection();
            btnPanel.Children.Add(backBtn);

            content.Children.Add(btnPanel);
            this.Content = content;
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

            // if an account is active, adjust trophies based on ranking
            if (_currentAccount != null)
            {
                var ranking = _gameState.Players
                    .OrderByDescending(p => p.IsAlive)
                    .ThenByDescending(p => p.LastDamagedFrameTime)
                    .ToList();
                int position = ranking.FindIndex(p => p.Id == 1) + 1;
                int trophyDelta;
                if (position == 1) trophyDelta = 50;
                else if (position == 2) trophyDelta = 20;
                else if (position <= 4) trophyDelta = 5;
                else trophyDelta = -10;

                _currentAccount.Trophies = Math.Max(0, _currentAccount.Trophies + trophyDelta);
                
                // record match result in history
                var matchResult = new MatchResult
                {
                    GameMode = _gameState.CurrentGameMode.Name,
                    Position = position,
                    TrophyChange = trophyDelta,
                    Date = DateTime.Now
                };
                _currentAccount.MatchHistory.Add(matchResult);

                // update quests
                _currentAccount.UpdateQuestProgress("Matches", 1);
                if (winner != null && winner.Id == 1) _currentAccount.UpdateQuestProgress("WinMatch", 1);
                _currentAccount.UpdateQuestProgress("Kills", _gameState.PlayerKills);
                _currentAccount.UpdateQuestProgress("Damage", _gameState.PlayerDamageDealt);
                _currentAccount.UpdateQuestProgress("Survive", _gameState.PlayerTimeSurvived);

                // add to pass progress
                _currentAccount.PassProgress += 25;
                Account.Save(_currentAccount);

                var trophyNotice = new TextBlock
                {
                    Text = $"({(trophyDelta >= 0 ? "+" : "")}{trophyDelta} trophies, total {_currentAccount.Trophies})",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Colors.White),
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                this.Tag = trophyNotice;
            }

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
            // if we stored a trophy notice, append it now
            if (this.Tag is TextBlock notice)
            {
                content.Children.Add(notice);
                this.Tag = null;
            }

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

        private void ShowLeaderboard()
        {
            var content = new StackPanel
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                Margin = new Thickness(20)
            };

            var title = new TextBlock
            {
                Text = "🏆 TOP 10 LEADERBOARD 🏆",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Colors.Gold),
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            content.Children.Add(title);

            var leaderboard = Account.GetLeaderboard(10);
            var listPanel = new StackPanel { Margin = new Thickness(20) };

            if (leaderboard.Count == 0)
            {
                var noData = new TextBlock
                {
                    Text = "No accounts yet!",
                    FontSize = 16,
                    Foreground = new SolidColorBrush(Colors.White),
                    TextAlignment = TextAlignment.Center
                };
                listPanel.Children.Add(noData);
            }
            else
            {
                for (int i = 0; i < leaderboard.Count; i++)
                {
                    var acc = leaderboard[i];
                    var medal = i == 0 ? "🥇" : i == 1 ? "🥈" : i == 2 ? "🥉" : $"#{i + 1}";
                    var statText = new TextBlock
                    {
                        Text = $"{medal}  {acc.Username.PadRight(20)} | 🏆 {acc.Trophies.ToString().PadLeft(5)} | W:{acc.GetWinCount()} T4:{acc.GetTopFourCount()} | WR:{acc.GetWinRate():F1}%",
                        FontSize = 13,
                        Foreground = i < 3 ? new SolidColorBrush(Colors.Gold) : new SolidColorBrush(Colors.White),
                        Margin = new Thickness(0, 8, 0, 0),
                        FontFamily = new System.Windows.Media.FontFamily("Courier New")
                    };
                    listPanel.Children.Add(statText);
                }
            }

            var scrollViewer = new ScrollViewer { Content = listPanel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Height = 400 };
            content.Children.Add(scrollViewer);

            // Back button
            var backBtn = new Button
            {
                Content = "BACK",
                Width = 150,
                Height = 50,
                Margin = new Thickness(20),
                Background = new SolidColorBrush(Color.FromRgb(100, 100, 150)),
                Foreground = new SolidColorBrush(Colors.White),
                Cursor = Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            backBtn.Click += (s, e) => ShowGameModeSelection();
            content.Children.Add(backBtn);

            this.Content = content;
        }

        private StackPanel CreateGemDisplay(int gemCount)
        {
            var gemPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            try
            {
                // Try multiple possible paths for the gem image
                string[] possiblePaths = new string[]
                {
                    @"C:\Users\Leonhardt Heyne\My Programms\gamepng's\shootgem.png",
                    @"C:\Users\Leonhardt Heyne\Downloads\My Programms\gamepng's\shootgem.png",
                    System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "shootgem.png")
                };
                
                string? foundPath = possiblePaths.FirstOrDefault(p => System.IO.File.Exists(p));
                if (foundPath != null)
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(foundPath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    var gemImage = new Image { Source = bitmap, Width = 20, Height = 20, Margin = new Thickness(0,0,5,0) };
                    gemPanel.Children.Add(gemImage);
                }
            }
            catch { /* Silently fail if image can't be loaded */ }
            
            var gemText = new TextBlock { Text = $": {gemCount}", FontSize = 16, Foreground = new SolidColorBrush(Colors.Gold) };
            gemPanel.Children.Add(gemText);
            return gemPanel;
        }

        private int GetShooterCost(ShooterClass cls)
        {
            switch (cls)
            {
                case ShooterClass.Sniper: return 60;
                case ShooterClass.Tank: return 100;
                default: return 90;
            }
        }

        private void ShowOwnedShooters()
        {
            if (_currentAccount == null) return;
            var content = new StackPanel { Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)), Margin = new Thickness(20) };
            var title = new TextBlock { Text = "OWNED SHOOTERS & SHOP", FontSize = 24, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Colors.Cyan), TextAlignment = TextAlignment.Center, Margin = new Thickness(0,0,0,20) };
            content.Children.Add(title);

            var gemDisplay = CreateGemDisplay(_currentAccount.Gems);
            gemDisplay.Margin = new Thickness(0,0,0,10);
            content.Children.Add(gemDisplay);

            var all = ShooterFactory.CreateAllShooters();
            var panel = new WrapPanel { Margin = new Thickness(10) };
            foreach (var shooter in all)
            {
                var box = new StackPanel { Width = 140, Margin = new Thickness(10), Background = new SolidColorBrush(Color.FromRgb(40,40,40)) };
                var name = new TextBlock { Text = shooter.Name, FontSize = 14, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center, Foreground = new SolidColorBrush(Colors.White) };
                var icon = new TextBlock { Text = shooter.Icon, FontSize = 24, TextAlignment = TextAlignment.Center };
                box.Children.Add(icon);
                box.Children.Add(name);
                if (_currentAccount.OwnedShooters.Contains(shooter.Name))
                {
                    var ownedLbl = new TextBlock { Text = "(owned)", FontSize = 12, Foreground = new SolidColorBrush(Colors.LimeGreen), TextAlignment = TextAlignment.Center };
                    box.Children.Add(ownedLbl);
                }
                else
                {
                    int cost = GetShooterCost(shooter.Class);
                    var buyBtn = new Button { Content = $"Buy {cost}g", Margin = new Thickness(5), FontSize = 12, Cursor = Cursors.Hand };
                    buyBtn.Click += (s, e) =>
                    {
                        if (_currentAccount.PurchaseShooter(shooter.Name, cost))
                        {
                            Account.Save(_currentAccount);
                            ShowOwnedShooters();
                        }
                        else {
                            MessageBox.Show("Not enough gems or already owned.");
                        }
                    };
                    box.Children.Add(buyBtn);
                }
                panel.Children.Add(box);
            }
            var scroll = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Height = 400 };
            content.Children.Add(scroll);

            var backBtn = new Button { Content = "BACK", Width = 150, Height = 50, Margin = new Thickness(20), Background = new SolidColorBrush(Color.FromRgb(100,100,150)), Foreground = new SolidColorBrush(Colors.White), Cursor = Cursors.Hand, HorizontalAlignment = HorizontalAlignment.Center };
            backBtn.Click += (s,e) => ShowGameModeSelection();
            content.Children.Add(backBtn);
            this.Content = content;
        }

        private void ShowShootPass()
        {
            if (_currentAccount == null) return;
            var content = new StackPanel { Background = new SolidColorBrush(Color.FromRgb(30,30,30)), Margin = new Thickness(20) };
            var title = new TextBlock { Text = "SHOOTPASS", FontSize = 24, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Colors.Gold), TextAlignment = TextAlignment.Center, Margin = new Thickness(0,0,0,20) };
            content.Children.Add(title);

            var progressText = new TextBlock { Text = $"Total XP: {_currentAccount.PassProgress}/50000", FontSize = 16, Foreground = new SolidColorBrush(Colors.White), TextAlignment = TextAlignment.Center, Margin = new Thickness(0,0,0,10) };
            content.Children.Add(progressText);

            // Big progress bar
            var progressBar = new ProgressBar
            {
                Minimum = 0,
                Maximum = 50000,
                Value = Math.Min(_currentAccount.PassProgress, 50000),
                Height = 40,
                Margin = new Thickness(0,0,0,20)
            };
            content.Children.Add(progressBar);

            // Gems panel
            var gemsPanel = new WrapPanel { HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0,0,0,20) };
            for (int i = 1; i <= 25; i++) // 25 milestones at 2000 each
            {
                int milestone = i * 2000;
                var gemBtn = new Button
                {
                    Content = "💎",
                    Width = 40,
                    Height = 40,
                    Margin = new Thickness(5),
                    FontSize = 20,
                    Cursor = Cursors.Hand,
                    IsEnabled = _currentAccount.PassProgress >= milestone && !_currentAccount.ClaimedMilestones.Contains(milestone)
                };
                gemBtn.Click += (s,e) =>
                {
                    if (_currentAccount.PassProgress >= milestone && !_currentAccount.ClaimedMilestones.Contains(milestone))
                    {
                        _currentAccount.ClaimedMilestones.Add(milestone);
                        _currentAccount.Gems += 10;
                        Account.Save(_currentAccount);
                        ShowShootPass(); // refresh
                    }
                };
                gemsPanel.Children.Add(gemBtn);
            }
            content.Children.Add(gemsPanel);

            var gemDisplay = CreateGemDisplay(_currentAccount.Gems);
            gemDisplay.Margin = new Thickness(0,0,0,20);
            content.Children.Add(gemDisplay);

            var backBtn = new Button { Content = "BACK", Width = 150, Height = 50, Margin = new Thickness(20), Background = new SolidColorBrush(Color.FromRgb(100,100,150)), Foreground = new SolidColorBrush(Colors.White), Cursor = Cursors.Hand, HorizontalAlignment = HorizontalAlignment.Center };
            backBtn.Click += (s,e) => ShowGameModeSelection();
            content.Children.Add(backBtn);
            this.Content = content;
        }

        private void ShowQuest()
        {
            if (_currentAccount == null) return;
            var content = new StackPanel { Background = new SolidColorBrush(Color.FromRgb(30,30,30)), Margin = new Thickness(20) };
            var title = new TextBlock { Text = "ACTIVE QUESTS", FontSize = 24, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Colors.Cyan), TextAlignment = TextAlignment.Center, Margin = new Thickness(0,0,0,20) };
            content.Children.Add(title);

            var refreshBtn = new Button { Content = "REFRESH QUESTS", Width = 150, Height = 40, Margin = new Thickness(0,0,0,20), Background = new SolidColorBrush(Color.FromRgb(100,100,150)), Foreground = new SolidColorBrush(Colors.White), Cursor = Cursors.Hand, HorizontalAlignment = HorizontalAlignment.Center };
            refreshBtn.Click += (s,e) =>
            {
                _currentAccount.GenerateNewQuests();
                Account.Save(_currentAccount);
                ShowQuest();
            };
            content.Children.Add(refreshBtn);

            foreach (var quest in _currentAccount.ActiveQuests)
            {
                var questPanel = new StackPanel { Margin = new Thickness(0,0,0,20), Background = new SolidColorBrush(Color.FromRgb(40,40,40)) };
                var desc = new TextBlock { Text = quest.Description, FontSize = 14, Foreground = new SolidColorBrush(Colors.White), Margin = new Thickness(0,0,0,5) };
                questPanel.Children.Add(desc);

                // Progress bar
                var progressBar = new ProgressBar
                {
                    Minimum = 0,
                    Maximum = quest.Target,
                    Value = Math.Min(quest.Current, quest.Target),
                    Height = 20,
                    Margin = new Thickness(0,0,0,5)
                };
                questPanel.Children.Add(progressBar);

                var progressText = new TextBlock { Text = $"{quest.Current}/{quest.Target}", FontSize = 12, Foreground = new SolidColorBrush(Colors.LimeGreen), TextAlignment = TextAlignment.Center };
                questPanel.Children.Add(progressText);

                var reward = new TextBlock { Text = $"Reward: {quest.RewardXP} XP", FontSize = 12, Foreground = new SolidColorBrush(Colors.Gold), Margin = new Thickness(0,5,0,0) };
                questPanel.Children.Add(reward);

                if (quest.IsCompleted)
                {
                    var claimBtn = new Button { Content = "CLAIM", Width = 100, Height = 30, Background = new SolidColorBrush(Color.FromRgb(80,150,80)), Foreground = new SolidColorBrush(Colors.White), Cursor = Cursors.Hand, Margin = new Thickness(0,5,0,0) };
                    claimBtn.Click += (s,e) =>
                    {
                        _currentAccount.ClaimQuest(quest);
                        Account.Save(_currentAccount);
                        ShowQuest(); // refresh
                    };
                    questPanel.Children.Add(claimBtn);
                }
                content.Children.Add(questPanel);
            }

            var backBtn = new Button { Content = "BACK", Width = 150, Height = 50, Margin = new Thickness(20), Background = new SolidColorBrush(Color.FromRgb(100,100,150)), Foreground = new SolidColorBrush(Colors.White), Cursor = Cursors.Hand, HorizontalAlignment = HorizontalAlignment.Center };
            backBtn.Click += (s,e) => ShowGameModeSelection();
            content.Children.Add(backBtn);
            this.Content = content;
        }

        // Draw range circles indicating attack radius for each player
        private void DrawRangeCircles()
        {
            if (_gameState == null || _gameCanvas == null) return;

            double tileWidth = _gameCanvas.ActualWidth / _gameState.CurrentMap.Width;
            double tileHeight = _gameCanvas.ActualHeight / _gameState.CurrentMap.Height;

            foreach (var player in _gameState.Players)
            {
                if (!player.IsAlive) continue;

                // Determine range circle radius based on shooter health (class)
                double rangeGrids;
                bool isSniperClass = player.Shooter.MaxHealth <= 60;
                
                if (player.Shooter.MaxHealth >= 90)
                    rangeGrids = 3.0; // close combat
                else if (isSniperClass)
                    rangeGrids = 6.0; // sniper/ranged
                else
                    rangeGrids = 4.5; // normal

                // Convert to pixel radius
                double radiusPixels = rangeGrids * Math.Max(tileWidth, tileHeight) / 2.0;

                // Center of the player
                double x = player.RenderPosition.X * tileWidth - radiusPixels;
                double y = player.RenderPosition.Y * tileHeight - radiusPixels;

                // Draw outer circle outline (semi-transparent for player 1, more transparent for others)
                var circle = new Ellipse
                {
                    Width = radiusPixels * 2,
                    Height = radiusPixels * 2,
                    Stroke = player.Id == 1 
                        ? new SolidColorBrush(Color.FromArgb(100, 100, 200, 255))  // blue for player
                        : new SolidColorBrush(Color.FromArgb(60, 255, 100, 100)),  // red for enemies
                    StrokeThickness = 1
                };

                Canvas.SetLeft(circle, x);
                Canvas.SetTop(circle, y);
                _gameCanvas.Children.Add(circle);

                // Draw inner circle for snipers only (1.5 grid radius)
                if (isSniperClass)
                {
                    double innerGrids = 1.5;
                    double innerRadiusPixels = innerGrids * Math.Max(tileWidth, tileHeight) / 2.0;
                    double innerX = player.RenderPosition.X * tileWidth - innerRadiusPixels;
                    double innerY = player.RenderPosition.Y * tileHeight - innerRadiusPixels;

                    var innerCircle = new Ellipse
                    {
                        Width = innerRadiusPixels * 2,
                        Height = innerRadiusPixels * 2,
                        Stroke = player.Id == 1
                            ? new SolidColorBrush(Color.FromArgb(80, 150, 100, 255))  // darker blue for inner zone
                            : new SolidColorBrush(Color.FromArgb(50, 255, 150, 150)), // darker red for inner zone
                        StrokeThickness = 1,
                        StrokeDashArray = new DoubleCollection { 2, 2 } // dashed line
                    };

                    Canvas.SetLeft(innerCircle, innerX);
                    Canvas.SetTop(innerCircle, innerY);
                    _gameCanvas.Children.Add(innerCircle);
                }
            }
        }

        private void DrawPoisonZone()
        {
            if (_gameState == null || _gameCanvas == null) return;

            double tileWidth = _gameCanvas.ActualWidth / _gameState.CurrentMap.Width;
            double tileHeight = _gameCanvas.ActualHeight / _gameState.CurrentMap.Height;

            // Draw poison zone circle
            double radiusPixels = _gameState.ZoneRadius * Math.Max(tileWidth, tileHeight) / 2.0;
            double x = _gameState.ZoneCenterX * tileWidth - radiusPixels;
            double y = _gameState.ZoneCenterY * tileHeight - radiusPixels;

            var zoneCircle = new Ellipse
            {
                Width = radiusPixels * 2,
                Height = radiusPixels * 2,
                Stroke = new SolidColorBrush(Color.FromArgb(120, 100, 200, 100)), // Green for safe zone
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 4, 4 }
            };

            Canvas.SetLeft(zoneCircle, x);
            Canvas.SetTop(zoneCircle, y);
            _gameCanvas.Children.Add(zoneCircle);
        }

        private void DrawShotAnimations()
        {
            if (_gameState == null || _gameCanvas == null) return;
            double tileWidth = _gameCanvas.ActualWidth / _gameState.CurrentMap.Width;
            double tileHeight = _gameCanvas.ActualHeight / _gameState.CurrentMap.Height;
            foreach (var shot in _gameState.Shots)
            {
                var pos = shot.GetPosition(_gameState.CurrentFrame);
                double x = pos.X * tileWidth;
                double y = pos.Y * tileHeight;
                var proj = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = new SolidColorBrush(Colors.Yellow),
                    Stroke = new SolidColorBrush(Colors.Orange),
                    StrokeThickness = 1
                };
                Canvas.SetLeft(proj, x - 4);
                Canvas.SetTop(proj, y - 4);
                _gameCanvas.Children.Add(proj);
            }
        }
    }
}
