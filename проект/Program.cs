using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SpaceShooter
{
    public class SpaceShooterGame : Form
    {
        private GameEngine gameEngine;

        public SpaceShooterGame()
        {
            this.Text = "game";
            this.ClientSize = new Size(800, 600);
            this.DoubleBuffered = true;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            gameEngine = new GameEngine(this);
            gameEngine.StartGame();

            this.KeyDown += (sender, e) => gameEngine.HandleKeyDown(e.KeyCode);
            this.KeyUp += (sender, e) => gameEngine.HandleKeyUp(e.KeyCode);
            this.Paint += (sender, e) => gameEngine.Render(e.Graphics);
        }

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SpaceShooterGame());
        }
    }

    public class GameEngine
    {
        private const int FPS = 60;
        private Timer gameTimer;
        private Form gameForm;

        private PlayerShip player;
        private List<EnemyShip> enemies;
        private List<Projectile> projectiles;
        private Random random;
        private int score;
        private int enemySpawnCooldown;
        private bool isGameOver;

        private Keys[] pressedKeys;

        public GameEngine(Form form)
        {
            gameForm = form;
            random = new Random();
            pressedKeys = new Keys[0];

            InitializeGame();
        }

        private void InitializeGame()
        {
            player = new PlayerShip(new Point(gameForm.ClientSize.Width / 2, gameForm.ClientSize.Height - 100));
            enemies = new List<EnemyShip>();
            projectiles = new List<Projectile>();
            score = 0;
            isGameOver = false;

            gameTimer = new Timer();
            gameTimer.Interval = 1000 / FPS;
            gameTimer.Tick += GameLoop;
        }

        public void StartGame()
        {
            gameTimer.Start();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            if (!isGameOver)
            {
                Update();
                gameForm.Invalidate(); // Вызывает перерисовку формы
            }
        }

        private void Update()
        {
            // Управление игроком
            foreach (var key in pressedKeys)
            {
                switch (key)
                {
                    case Keys.Left:
                        player.Move(-5, 0);
                        break;
                    case Keys.Right:
                        player.Move(5, 0);
                        break;
                    case Keys.Space:
                        if (player.CanShoot())
                        {
                            projectiles.Add(player.Shoot());
                        }
                        break;
                }
            }

            // Спавн врагов
            if (enemySpawnCooldown <= 0)
            {
                SpawnEnemy();
                enemySpawnCooldown = random.Next(30, 90);
            }
            else
            {
                enemySpawnCooldown--;
            }

            // Обновление врагов
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                enemies[i].Move();

                // Проверка выхода за границы
                if (enemies[i].Position.Y > gameForm.ClientSize.Height)
                {
                    enemies.RemoveAt(i);
                    continue;
                }

                // Проверка столкновения с игроком
                if (enemies[i].Bounds.IntersectsWith(player.Bounds))
                {
                    GameOver();
                    return;
                }
            }

            // Обновление снарядов
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                projectiles[i].Move();

                // Проверка выхода за границы
                if (projectiles[i].Position.Y < 0)
                {
                    projectiles.RemoveAt(i);
                    continue;
                }

                // Проверка попадания во врага
                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    if (projectiles[i].Bounds.IntersectsWith(enemies[j].Bounds))
                    {
                        enemies.RemoveAt(j);
                        projectiles.RemoveAt(i);
                        score += 10;
                        break;
                    }
                }
            }
        }

        private void SpawnEnemy()
        {
            int x = random.Next(50, gameForm.ClientSize.Width - 50);
            int speed = random.Next(1, 4);
            enemies.Add(new EnemyShip(new Point(x, -50), speed));
        }

        public void Render(Graphics g)
        {
            g.Clear(Color.Black);

            // Отрисовка игрока
            player.Draw(g);

            // Отрисовка врагов
            foreach (var enemy in enemies)
            {
                enemy.Draw(g);
            }

            // Отрисовка снарядов
            foreach (var projectile in projectiles)
            {
                projectile.Draw(g);
            }

            // Отрисовка счета
            var scoreFont = new Font("Arial", 16);
            g.DrawString($"Счет: {score}", scoreFont, Brushes.White, 10, 10);

            if (isGameOver)
            {
                var gameOverFont = new Font("Arial", 32, FontStyle.Bold);
                string gameOverText = "ИГРА ОКОНЧЕНА";
                var textSize = g.MeasureString(gameOverText, gameOverFont);
                g.DrawString(gameOverText, gameOverFont, Brushes.Red,
                    (gameForm.ClientSize.Width - textSize.Width) / 2,
                    (gameForm.ClientSize.Height - textSize.Height) / 2);

                var restartFont = new Font("Arial", 16);
                string restartText = "Нажмите R для рестарта";
                textSize = g.MeasureString(restartText, restartFont);
                g.DrawString(restartText, restartFont, Brushes.White,
                    (gameForm.ClientSize.Width - textSize.Width) / 2,
                    (gameForm.ClientSize.Height - textSize.Height) / 2 + 50);
            }
        }

        public void HandleKeyDown(Keys key)
        {
            if (isGameOver && key == Keys.R)
            {
                InitializeGame();
                return;
            }

            if (!Array.Exists(pressedKeys, k => k == key))
            {
                Array.Resize(ref pressedKeys, pressedKeys.Length + 1);
                pressedKeys[pressedKeys.Length - 1] = key;
            }
        }

        public void HandleKeyUp(Keys key)
        {
            pressedKeys = Array.FindAll(pressedKeys, k => k != key);
        }

        private void GameOver()
        {
            isGameOver = true;
            gameTimer.Stop();
        }
    }

    public class GameObject
    {
        public Point Position { get; protected set; }
        public Size Size { get; protected set; }
        public Rectangle Bounds => new Rectangle(Position, Size);

        public GameObject(Point position, Size size)
        {
            Position = position;
            Size = size;
        }

        public virtual void Draw(Graphics g)
        {
            // Базовый метод отрисовки
            g.FillRectangle(Brushes.Gray, Bounds);
        }
    }

    public class PlayerShip : GameObject
    {
        private const int ShootCooldown = 20;
        private int currentCooldown;

        public PlayerShip(Point position) : base(position, new Size(40, 60))
        {
            currentCooldown = 0;
        }

        public void Move(int dx, int dy)
        {
            Position = new Point(
                Math.Max(0, Math.Min(Position.X + dx, 760)), // 800 - 40 (ширина корабля)
                Math.Max(0, Math.Min(Position.Y + dy, 540))  // 600 - 60 (высота корабля)
            );

            if (currentCooldown > 0)
            {
                currentCooldown--;
            }
        }

        public bool CanShoot()
        {
            return currentCooldown <= 0;
        }

        public Projectile Shoot()
        {
            currentCooldown = ShootCooldown;
            return new Projectile(
                new Point(Position.X + Size.Width / 2 - 3, Position.Y - 20),
                -10, true);
        }

        public override void Draw(Graphics g)
        {
            // Корпус корабля
            g.FillRectangle(Brushes.LightBlue, Bounds);

            // Кабина
            g.FillRectangle(Brushes.Blue,
                Position.X + 10, Position.Y + 10, 20, 15);

            // Двигатели
            g.FillRectangle(Brushes.Orange,
                Position.X + 5, Position.Y + Size.Height - 15, 10, 10);
            g.FillRectangle(Brushes.Orange,
                Position.X + Size.Width - 15, Position.Y + Size.Height - 15, 10, 10);
        }
    }

    public class EnemyShip : GameObject
    {
        private int speed;

        public EnemyShip(Point position, int speed) : base(position, new Size(40, 40))
        {
            this.speed = speed;
        }

        public void Move()
        {
            Position = new Point(Position.X, Position.Y + speed);
        }

        public override void Draw(Graphics g)
        {
            // Корпус врага
            g.FillRectangle(Brushes.Red, Bounds);

            // Окно
            g.FillRectangle(Brushes.Yellow,
                Position.X + 10, Position.Y + 10, 20, 10);
        }
    }

    public class Projectile : GameObject
    {
        private int speed;
        private bool isPlayerProjectile;

        public Projectile(Point position, int speed, bool isPlayerProjectile)
            : base(position, new Size(6, 15))
        {
            this.speed = speed;
            this.isPlayerProjectile = isPlayerProjectile;
        }

        public void Move()
        {
            Position = new Point(Position.X, Position.Y + speed);
        }

        public override void Draw(Graphics g)
        {
            Brush projectileBrush = isPlayerProjectile ? Brushes.Green : Brushes.Orange;
            g.FillRectangle(projectileBrush, Bounds);
        }
    }
}