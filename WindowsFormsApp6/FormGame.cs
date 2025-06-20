using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SpaceShooter
{
    public partial class FormGame : Form
    {
        private bool gameWon = false;
        private List<PictureBox> bullets = new List<PictureBox>();
        private List<PictureBox> enemies = new List<PictureBox>();
        private Random rnd = new Random();
        private PictureBox player;
        private Timer gameTimer = new Timer();
        private int score = 0;
        //инициализирует форму FormGame
        public FormGame()
        {
            InitializeComponent();
            InitGame();
        }
        //отрисовка игрока и спавн врагов
        private void InitGame()
        {
            player = new PictureBox
            {
                BackColor = Color.Cyan,
                Size = new Size(40, 40),
                Top = this.ClientSize.Height - 60,
                Left = (this.ClientSize.Width - 40) / 2
            };
            this.Controls.Add(player);

            this.KeyPreview = true;
            this.KeyDown += FormGame_KeyDown;

            gameTimer.Interval = 10;
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            SpawnEnemy();
        }

        //удаление патрона если он вылетел за карту
        private void GameLoop(object sender, EventArgs e)
        {
            foreach (var bullet in bullets.ToArray())
            {
                bullet.Top -= 10;
                if (bullet.Top < 0)
                {
                    this.Controls.Remove(bullet);
                    bullets.Remove(bullet);
                }
            }

            foreach (var enemy in enemies.ToArray())
            {
                enemy.Top += 5;
                if (enemy.Bounds.IntersectsWith(player.Bounds))
                {
                    gameTimer.Stop();
                    MessageBox.Show("Вы проиграли! Очки: " + score); //условие проигрыша
                    this.Close();
                }
                else if (enemy.Top > this.ClientSize.Height)
                {
                    this.Controls.Remove(enemy);
                    enemies.Remove(enemy); 
                    SpawnEnemy();
                }
            }

            foreach (var bullet in bullets.ToArray())
            {
                foreach (var enemy in enemies.ToArray())
                {
                    if (bullet.Bounds.IntersectsWith(enemy.Bounds))//условие попадания по врагу
                    {
                        this.Controls.Remove(bullet);
                        this.Controls.Remove(enemy);
                        bullets.Remove(bullet);
                        enemies.Remove(enemy);
                        AddScore(1);
                        SpawnEnemy();
                        return;
                    }
                }
            }
        }
        //Метод для отрисовски счета и проверка условия победы
        private void AddScore(int points)
        {
            score += points;
            labelScore.Text = $"Счёт: {score}";

            // Условие победы 
            if (score >= 100)
            {
                MessageBox.Show("Победа!", "Поздравляем");
            }
        }


        //метод спавна врагов
        private void SpawnEnemy()
        {
            PictureBox enemy = new PictureBox
            {
                BackColor = Color.Red,
                Size = new Size(40, 40),
                Top = 0,
                Left = rnd.Next(0, this.ClientSize.Width - 40)
            };
            enemies.Add(enemy);
            this.Controls.Add(enemy);
            enemy.BringToFront();
        }
        //метод для стрельбы
        private void FormGame_KeyDown(object sender, KeyEventArgs e)
        {
            int speed = 10;
            if (e.KeyCode == Keys.Left && player.Left > 0)
                player.Left -= speed;
            else if (e.KeyCode == Keys.Right && player.Left < this.ClientSize.Width - player.Width)
                player.Left += speed;
            else if (e.KeyCode == Keys.Space)
                Shoot();
        }
        //отрисовка патрона
        private void Shoot()
        {
            PictureBox bullet = new PictureBox
            {
                BackColor = Color.Yellow,
                Size = new Size(5, 20),
                Left = player.Left + player.Width / 2 - 2,
                Top = player.Top - 20
            };
            bullets.Add(bullet);
            this.Controls.Add(bullet);
            bullet.BringToFront();
        }

        private void FormGame_Load(object sender, EventArgs e)
        {

        }
    }
}
