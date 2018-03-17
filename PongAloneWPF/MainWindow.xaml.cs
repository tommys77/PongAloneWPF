using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using WMPLib;

namespace PongAloneWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isPlaying = false;
        private double batterTop = 0;
        private bool soundFx = true;

        private int score = 0;
        private int lives = 3;

        private double movementX;
        private double movementY;

        private string BALL_HIT_PATH = Environment.CurrentDirectory + new Uri(@"pack://siteoforigin:,,,/Resources/ball_hit.wav").AbsolutePath;
        private string WALL_HIT_PATH = Environment.CurrentDirectory + new Uri(@"pack://siteoforigin:,,,/Resources/wall_hit.wav").AbsolutePath;
        private string LOST_LIFE_PATH = Environment.CurrentDirectory + new Uri(@"pack://siteoforigin:,,,/Resources/lost_life.wav").AbsolutePath;
        private string GAME_OVER_PATH = Environment.CurrentDirectory + new Uri(@"pack://siteoforigin:,,,/Resources/game_over.wav").AbsolutePath;

        WindowsMediaPlayer fx_ball_hit;
        WindowsMediaPlayer fx_lost_life;
        WindowsMediaPlayer fx_wall_hit;
        WindowsMediaPlayer fx_game_over;

        private bool movingLeft = true;
        private bool movingUp = false;

        DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            InitializeGame();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            MoveBall();
        }

        private void InitializeGame()
        {
            Canvas.SetTop(batter, playfield.Height - batter.Height);
            batterTop = Canvas.GetTop(batter);

            // Set up a timer for moving objects
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 10);

            btn_Restart.IsEnabled = false;

            // Assign sound effects
            fx_ball_hit = new WindowsMediaPlayer();
            fx_lost_life = new WindowsMediaPlayer();
            fx_game_over = new WindowsMediaPlayer();
            fx_wall_hit = new WindowsMediaPlayer();
        }

        private void MoveBall()
        {
            double ballX = Canvas.GetLeft(ball);
            double ballY = Canvas.GetTop(ball);

            SetBallMovement(ballX, ballY, 5);
        }

        private void SetBallMovement(double x, double y, double speed)
        {
            var collision = CollidedWithBat(x);
            movementX = x + speed;
            movementY = y + speed;

            // Adjusting course for x
            if (x <= 0)
            {
                if (soundFx)
                {
                    fx_wall_hit.URL = WALL_HIT_PATH;
                }
                movingLeft = false;
            }
            if (x >= playfield.ActualWidth - ball.ActualWidth)
            {
                if (soundFx)
                {
                    fx_wall_hit.URL = WALL_HIT_PATH;
                }

                movingLeft = true;
            }

            if (movingLeft)
            {
                movementX = x - speed;
            }

            // Adjusting course for y, depending on wether the ball is saved by the batter

            if (y <= 0)
            {
                if (soundFx)
                {
                    fx_wall_hit.URL = WALL_HIT_PATH;
                }
                movingUp = false;
            }

            if (y >= batterTop - ball.Height)
            {
                if (collision)
                {
                    if (soundFx)
                    {
                        fx_ball_hit.URL = BALL_HIT_PATH;
                    }

                    score++;
                    UpdateScreen();
                    movingUp = true;
                }
                else
                {
                    if (lives == 0)
                    {
                        GameOver();
                    }
                    else
                    {
                        lives--;
                        if (soundFx)
                        {
                            fx_lost_life.URL = LOST_LIFE_PATH;
                        }
                        UpdateScreen();
                    }

                    movingUp = false;
                    ResetBall();
                }
            }

            if (movingUp)
            {
                movementY = y - speed;
            }

            Canvas.SetLeft(ball, movementX);
            Canvas.SetTop(ball, movementY);
        }

        private void ResetBall()
        {
            movementX = new Random().Next((Int32)(playfield.Width - ball.Width));
            movementY = ball.Height;
        }

        private void GameOver()
        {
            timer.Stop();
            if (soundFx)
            {
                fx_game_over.URL = GAME_OVER_PATH;
            }
            btn_play.Content = "Play";
            btn_play.IsEnabled = false;
            ShowCursor(true);
        }

        private void RestartGame()
        {
            lives = 3;
            score = 0;
            UpdateScreen();
            ResetBall();
            ShowCursor(false);
            btn_play.Content = "Pause";
            btn_play.IsEnabled = true;
            isPlaying = true;
            timer.Start();
        }

        private void UpdateScreen()
        {
            tb_lives.Text = lives.ToString();
            tb_score.Text = score.ToString();
        }

        private bool CollidedWithBat(double x)
        {
            var minX = Canvas.GetLeft(batter) - ball.Width;
            var maxX = minX + batter.ActualWidth + ball.Width;
            if (x > minX && x < maxX)
            {
                return true;
            }
            else return false;
        }

        private void KeyboardShortcuts(object sender, KeyEventArgs e)
        {
            var key = e.Key;

            switch (key)
            {
                case Key.Space:
                    ButtonAutomationPeer peer = new ButtonAutomationPeer(btn_play);
                    IInvokeProvider invprov = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                    invprov.Invoke();
                    break;
            }
        }


        private void ShowCursor(bool _bool)
        {
            if (_bool)
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            else Mouse.OverrideCursor = Cursors.None;
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPlaying)
            {

                Point pos = e.GetPosition(playfield);
                var maxX = playfield.Width - batter.Width;

                if (pos.X < 0)
                    Canvas.SetLeft(batter, 0);
                else if (pos.X > maxX)
                    Canvas.SetLeft(batter, maxX);
                else
                    Canvas.SetLeft(batter, pos.X);
            }
        }

        private void btn_play_Click(object sender, RoutedEventArgs e)
        {
            isPlaying = !isPlaying;
            if (isPlaying)
            {
                timer.Start();
                ShowCursor(false);
                btn_play.Content = "Pause";
            }
            else
            {
                timer.Stop();
                ShowCursor(true);
                btn_play.Content = "Play";
            }
            btn_Restart.IsEnabled = true;
        }

        private void playfield_GotMouseCapture(object sender, MouseEventArgs e)
        {

        }

        private void btn_Restart_Click(object sender, RoutedEventArgs e)
        {
            RestartGame();
        }

        private void btn_sound_Click(object sender, RoutedEventArgs e)
        {
            soundFx = !soundFx;
            if (soundFx)
            {
                btn_sound.Content = "Turn sound OFF";
            }
            else btn_sound.Content = "Turn sound ON";

        }
    }
}
