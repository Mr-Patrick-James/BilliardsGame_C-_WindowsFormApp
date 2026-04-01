using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BilliardsGame
{
    public partial class Form1 : Form
    {
        Image tableImage;
        List<Ball> balls;
        List<Pocket> pockets;
        Ball selectedBall;
        PointF aimDirection;
        Timer timer;
        bool isDragging = false;

        int left = 60;
        int right = 955;
        int top = 130;
        int bottom = 595;

        Pocket draggingPocket = null;
        string pocketFilePath = "pockets.txt";

        int playerTurn = 1;
        int player1Score = 0;
        int player2Score = 0;
        Stopwatch stopwatch = new Stopwatch();
        Label lblScore;
        Label lblTimer;
        Label lblPlayer2;
        bool gameEnded = false;

        public Form1()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Width = 1152;
            this.Height = 768;

            tableImage = Image.FromFile("1.png");

            balls = new List<Ball>
            {
                new Ball(200, 350, 10, Brushes.White),
                new Ball(500, 300, 10, Brushes.Red),
                new Ball(600, 280, 10, Brushes.Yellow),
                new Ball(530, 260, 10, Brushes.Blue),
                new Ball(580, 250, 10, Brushes.Green),
                new Ball(620, 270, 10, Brushes.Purple),
                new Ball(480, 320, 10, Brushes.Orange),
                new Ball(450, 290, 10, Brushes.Brown)
            };
            selectedBall = balls[0];

            pockets = LoadPocketPositions(pocketFilePath) ?? new List<Pocket>
            {
                new Pocket(110, 145), new Pocket(510, 140), new Pocket(905, 145),
                new Pocket(110, 565), new Pocket(510, 565), new Pocket(905, 565)
            };

            lblScore = new Label
            {
                Location = new Point(20, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Text = "Player 1: 0",
                 BackColor = Color.Transparent,
            };

            lblTimer = new Label
            {
                Location = new Point(this.Width / 2 - 70, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Text = "Time: 00:00",
                 BackColor = Color.Transparent,
            };

            lblPlayer2 = new Label
            {
                Location = new Point(this.Width - 180, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Text = "Player 2: 0",
                 BackColor = Color.Transparent,
            };

            this.Controls.Add(lblScore);
            this.Controls.Add(lblTimer);
            this.Controls.Add(lblPlayer2);

            timer = new Timer { Interval = 1000 / 60 };
            timer.Tick += UpdateGame;
            timer.Start();
            stopwatch.Start();
        }

        bool hasPocketedBallThisTurn = false;
        bool turnProcessed = false; // 🆕 prevents blinking bug
        private void UpdateGame(object sender, EventArgs e)
        {
            if (gameEnded) return;

            int scoredThisFrame = 0;
            bool anyBallMoving = false;

            for (int i = 0; i < balls.Count; i++)
            {
                Ball ballA = balls[i];
                ballA.Update();

                if (ballA.IsMoving())
                    anyBallMoving = true;

                if (ballA.Position.X - ballA.Radius < left || ballA.Position.X + ballA.Radius > right)
                    ballA.Velocity = new PointF(-ballA.Velocity.X, ballA.Velocity.Y);
                if (ballA.Position.Y - ballA.Radius < top || ballA.Position.Y + ballA.Radius > bottom)
                    ballA.Velocity = new PointF(ballA.Velocity.X, -ballA.Velocity.Y);

                for (int j = i + 1; j < balls.Count; j++)
                {
                    Ball ballB = balls[j];
                    ResolveCollision(ballA, ballB);
                }

                foreach (var pocket in pockets)
                {
                    if (pocket.IsBallInPocket(ballA) && ballA != selectedBall && ballA.Position.X >= 0)
                    {
                        scoredThisFrame++;
                        ballA.Position = new PointF(-100, -100); // Off-screen
                        ballA.Velocity = new PointF(0, 0);
                    }
                }
            }

            if (scoredThisFrame > 0)
            {
                if (playerTurn == 1) player1Score += scoredThisFrame;
                else player2Score += scoredThisFrame;
                hasPocketedBallThisTurn = true;
            }

            // 👇 This logic now runs ONLY ONCE after balls stop moving
            if (!anyBallMoving && !isDragging && !turnProcessed)
            {
                if (!hasPocketedBallThisTurn)
                {
                    playerTurn = 3 - playerTurn;
                }

                hasPocketedBallThisTurn = false;
                turnProcessed = true;
            }

            if (balls.Count(b => b.Position.X > 0 && b != selectedBall) == 0)
            {
                gameEnded = true;
                string winner = player1Score > player2Score ? "Player 1 Wins!" :
                                player2Score > player1Score ? "Player 2 Wins!" : "It's a tie!";
                MessageBox.Show($"Game Over!\n{winner}", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            TimeSpan elapsed = stopwatch.Elapsed;
            lblScore.Text = $"Player 1: {player1Score}";
            lblPlayer2.Text = $"Player 2: {player2Score}";
            lblTimer.Text = $"Time: {elapsed.Minutes:D2}:{elapsed.Seconds:D2}   Turn: Player {playerTurn}";

            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawImage(tableImage, 0, 0, this.ClientSize.Width, this.ClientSize.Height);

            if (isDragging)
            {
                Pen aimLine = new Pen(Color.Red, 2);
                g.DrawLine(aimLine, selectedBall.Position.X, selectedBall.Position.Y,
                    selectedBall.Position.X + (selectedBall.Position.X - aimDirection.X),
                    selectedBall.Position.Y + (selectedBall.Position.Y - aimDirection.Y));
            }

            foreach (var ball in balls)
                ball.Draw(g);

            //foreach (var pocket in pockets)
            //{
            //    Pen debugPen = new Pen(Color.LimeGreen, 2);
            //    g.DrawEllipse(debugPen,
            //        pocket.Position.X - pocket.Radius,
            //        pocket.Position.Y - pocket.Radius,
            //        pocket.Radius * 2,
            //        pocket.Radius * 2);
            //}
        }

        private void ResolveCollision(Ball a, Ball b)
        {
            float dx = b.Position.X - a.Position.X;
            float dy = b.Position.Y - a.Position.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);
            float minDist = a.Radius + b.Radius;

            if (distance < minDist && distance > 0)
            {
                float nx = dx / distance;
                float ny = dy / distance;
                float tx = -ny;
                float ty = nx;

                float dpTanA = a.Velocity.X * tx + a.Velocity.Y * ty;
                float dpTanB = b.Velocity.X * tx + b.Velocity.Y * ty;

                float dpNormA = a.Velocity.X * nx + a.Velocity.Y * ny;
                float dpNormB = b.Velocity.X * nx + b.Velocity.Y * ny;

                float mA = dpNormB;
                float mB = dpNormA;

                a.Velocity = new PointF(tx * dpTanA + nx * mA, ty * dpTanA + ny * mA);
                b.Velocity = new PointF(tx * dpTanB + nx * mB, ty * dpTanB + ny * mB);

                float overlap = minDist - distance;
                a.Position = new PointF(a.Position.X - nx * overlap / 2, a.Position.Y - ny * overlap / 2);
                b.Position = new PointF(b.Position.X + nx * overlap / 2, b.Position.Y + ny * overlap / 2);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if ((ModifierKeys & Keys.Alt) == Keys.Alt)
            {
                foreach (var pocket in pockets)
                {
                    if (pocket.IsMouseOver(e.Location))
                    {
                        draggingPocket = pocket;
                        draggingPocket.IsDragging = true;
                        draggingPocket.Offset = new Point((int)(e.X - pocket.Position.X), (int)(e.Y - pocket.Position.Y));
                        return;
                    }
                }
            }
            else if (!selectedBall.IsMoving())
            {
                isDragging = true;
                aimDirection = e.Location;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (draggingPocket != null && draggingPocket.IsDragging)
            {
                draggingPocket.Position = new PointF(e.X - draggingPocket.Offset.X, e.Y - draggingPocket.Offset.Y);
                this.Invalidate();
            }
            else if (isDragging)
            {
                aimDirection = e.Location;
                this.Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (draggingPocket != null)
            {
                draggingPocket.IsDragging = false;
                draggingPocket = null;
                SavePocketPositions(pocketFilePath);
            }
            else if (isDragging)
            {
                float dx = selectedBall.Position.X - e.X;
                float dy = selectedBall.Position.Y - e.Y;

                selectedBall.Velocity = new PointF(dx / 10f, dy / 10f);
                isDragging = false;
                turnProcessed = false; // 🆕 allow new turn processing after shot
            }

        }

        private void SavePocketPositions(string path)
        {
            using (StreamWriter sw = new StreamWriter(path))
            {
                foreach (var pocket in pockets)
                    sw.WriteLine($"{pocket.Position.X},{pocket.Position.Y}");
            }
        }

        private List<Pocket> LoadPocketPositions(string path)
        {
            if (!File.Exists(path))
                return null;

            var list = new List<Pocket>();
            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 2 &&
                    float.TryParse(parts[0], out float x) &&
                    float.TryParse(parts[1], out float y))
                {
                    list.Add(new Pocket(x, y));
                }
            }
            return list;
        }
    }

    public class Ball
    {
        public PointF Position;
        public PointF Velocity;
        public int Radius;
        public Brush BallBrush;

        public Ball(float x, float y, int radius, Brush color)
        {
            Position = new PointF(x, y);
            Velocity = new PointF(0, 0);
            Radius = radius;
            BallBrush = color;
        }

        public void Update()
        {
            Position = new PointF(Position.X + Velocity.X, Position.Y + Velocity.Y);
            Velocity = new PointF(Velocity.X * 0.98f, Velocity.Y * 0.98f);

            if (Math.Abs(Velocity.X) < 0.1f) Velocity.X = 0;
            if (Math.Abs(Velocity.Y) < 0.1f) Velocity.Y = 0;
        }

        public void Draw(Graphics g)
        {
            g.FillEllipse(BallBrush, Position.X - Radius, Position.Y - Radius, Radius * 2, Radius * 2);
        }

        public bool IsMoving()
        {
            return Math.Abs(Velocity.X) > 0.01f || Math.Abs(Velocity.Y) > 0.01f;
        }
    }

    public class Pocket
    {
        public PointF Position;
        public int Radius = 20;
        public bool IsDragging = false;
        public Point Offset;

        public Pocket(float x, float y)
        {
            Position = new PointF(x, y);
        }

        public bool IsBallInPocket(Ball b)
        {
            float dx = b.Position.X - Position.X;
            float dy = b.Position.Y - Position.Y;
            return Math.Sqrt(dx * dx + dy * dy) < Radius;
        }

        public bool IsMouseOver(Point mouse)
        {
            float dx = mouse.X - Position.X;
            float dy = mouse.Y - Position.Y;
            return Math.Sqrt(dx * dx + dy * dy) <= Radius;
        }
    }
}
