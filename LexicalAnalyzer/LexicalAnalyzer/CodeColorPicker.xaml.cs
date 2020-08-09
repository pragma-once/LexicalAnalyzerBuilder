using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LexicalAnalyzer
{
    /// <summary>
    /// Interaction logic for CodeColorPicker.xaml
    /// </summary>
    public partial class CodeColorPicker : UserControl
    {
        Color[] Colors =
        {
            Color.FromRgb(0xFF, 0X10, 0X80),
            Color.FromRgb(0x80, 0X10, 0XFF),
            Color.FromRgb(0x10, 0X80, 0XFF),
            Color.FromRgb(0x10, 0XFF, 0X80),
            Color.FromRgb(200, 160, 200),
            Color.FromRgb(160, 200, 200),
            Color.FromRgb(200, 200, 160),
            Color.FromRgb(200, 200, 200)
        };

        public CodeColorPicker()
        {
            InitializeComponent();
        }

        System.Windows.Threading.DispatcherTimer Timer = new System.Windows.Threading.DispatcherTimer();
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            GradientStopCollection coll = new GradientStopCollection();
            for (int i = 0; i < Colors.Length; i++)
            {
                coll.Add(new GradientStop(Colors[i], (double)i / (double)Colors.Length + 0.001));
                coll.Add(new GradientStop(Colors[i], (double)(i + 1) / (double)Colors.Length));
            }
            LinearGradientBrush brush = new LinearGradientBrush(coll, 0);
            //brush.Transform = new RotateTransform(-90);
            StringColorBar.Background = brush;
            CommentColorBar.Background = brush;

            Timer.Interval = TimeSpan.FromMilliseconds(25);
            Timer.Tick += Timer_Tick;

            strIndex = Properties.Settings.Default.StringColorIndex;
            commIndex = Properties.Settings.Default.CommentColorIndex;

            UpdateColor();

            StringColorIndicator.Fill = strBrush;
            CommentColorIndicator.Fill = commBrush;

            MainGrid.Background = BG.Brush;
            StringLabel.Foreground = FG.Brush;
            CommentLabel.Foreground = FG.Brush;
            ResetButton.Foreground = FG.Brush;

            ResetButton.OnClick += ResetButton_OnClick;
        }

        double strTarget0 = 0;
        double commTarget0 = 0;
        double strTarget1 = 0;
        double commTarget1 = 0;
        double strTarget2 = 0;
        double commTarget2 = 0;
        SolidColorBrush strBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        SolidColorBrush commBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (Math.Abs(strTarget2 - strTarget0) < 0.001 && Math.Abs(commTarget2 - commTarget0) < 0.001)
            {
                Timer.IsEnabled = false;

                strTarget1 = strTarget0;
                commTarget1 = commTarget0;

                strTarget2 = strTarget1;
                commTarget2 = commTarget1;
            }
            else
            {
                strTarget1 = Step(strTarget1, strTarget0, 0.1);
                commTarget1 = Step(commTarget1, commTarget0, 0.1);

                strTarget2 = strTarget2 + (strTarget1 - strTarget2) * 0.25;
                commTarget2 = commTarget2 + (commTarget1 - commTarget2) * 0.25;
            }

            StringColorIndicator.Margin = new Thickness(
                (strTarget2 + (0.5 / Colors.Length)) * StringColorBar.ActualWidth - 4,
                -8, -8, -8);
            CommentColorIndicator.Margin = new Thickness(
               (commTarget2 + (0.5 / Colors.Length)) * CommentColorBar.ActualWidth - 4,
               -8, -8, -8);

            double t;

            t = strTarget2 * Colors.Length;
            for (int i = 1; i < Colors.Length; i++)
            {
                if (t < i)
                {
                    Color c = strBrush.Color;
                    int i_1 = i - 1;
                    double i_delta = t - i_1;
                    c.R = (byte)((double)Colors[i_1].R + ((double)Colors[i].R - (double)Colors[i_1].R) * (i_delta));
                    c.G = (byte)((double)Colors[i_1].G + ((double)Colors[i].G - (double)Colors[i_1].G) * (i_delta));
                    c.B = (byte)((double)Colors[i_1].B + ((double)Colors[i].B - (double)Colors[i_1].B) * (i_delta));
                    strBrush.Color = c;
                    break;
                }
            }

            t = commTarget2 * Colors.Length;
            for (int i = 1; i < Colors.Length; i++)
            {
                if (t < i)
                {
                    Color c = commBrush.Color;
                    int i_1 = i - 1;
                    double i_delta = t - i_1;
                    c.R = (byte)(Colors[i_1].R + (Colors[i].R - Colors[i_1].R) * (i_delta));
                    c.G = (byte)(Colors[i_1].G + (Colors[i].G - Colors[i_1].G) * (i_delta));
                    c.B = (byte)(Colors[i_1].B + (Colors[i].B - Colors[i_1].B) * (i_delta));
                    commBrush.Color = c;
                    break;
                }
            }
        }

        int strIndex = 0;
        public int StringColorIndex
        {
            get { return strIndex; }
            set
            {
                if (value >= 0 && value < Colors.Length)
                {
                    strIndex = value;
                    UpdateColor();
                    Properties.Settings.Default.StringColorIndex = value;
                }
            }
        }

        int commIndex = 6;
        public int CommentColorIndex
        {
            get { return commIndex; }
            set
            {
                if (value >= 0 && value < Colors.Length)
                {
                    commIndex = value;
                    UpdateColor();
                    Properties.Settings.Default.CommentColorIndex = value;
                }
            }
        }

        public void ResetColors()
        {
            StringColorIndex = 0;
            CommentColorIndex = 6;
            Properties.Settings.Default.StringColorIndex = 0;
            Properties.Settings.Default.CommentColorIndex = 6;
            Properties.Settings.Default.Save();
        }

        public Color StringColor
        {
            get
            {
                try
                {
                    return Colors[StringColorIndex];
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public Color CommentColor
        {
            get
            {
                try
                {
                    return Colors[CommentColorIndex];
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public void SetBrushes(SolidColorBrush StringBrush, SolidColorBrush CommentBrush)
        {
            strBrush = StringBrush;
            commBrush = CommentBrush;
            StringColorIndicator.Fill = strBrush;
            CommentColorIndicator.Fill = commBrush;
            UpdateColor();
        }

        public UI.SolidColorBrushController FG = new UI.SolidColorBrushController(0, 0, 0);
        public UI.SolidColorBrushController BG = new UI.SolidColorBrushController(0, 0, 0);
        public CustomButton ResetButton
        {
            get { return resetButton; }
        }

        void UpdateColor()
        {
            strTarget0 = ((double)strIndex / (double)Colors.Length); // + (0.5 / (double)Colors.Length);
            commTarget0 = ((double)commIndex / (double)Colors.Length); // + (0.5 / (double)Colors.Length);
            Timer.IsEnabled = true;
        }

        double Step(double CurrentPoint, double Target, double Step)
        {
            if (CurrentPoint < Target)
            {
                double Next = CurrentPoint + Step;
                if (Next < Target)
                    return Next;
                else
                    return Target;
            }
            else
            {
                double Next = CurrentPoint - Step;
                if (Next > Target)
                    return Next;
                else
                    return Target;
            }
        }

        bool strMouseDown = false;
        private void StringColorBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double i = (e.GetPosition(StringColorBar).X / StringColorBar.ActualWidth) * Colors.Length;
            if (i < 0)
                StringColorIndex = 0;
            else if (i > Colors.Length)
                StringColorIndex = Colors.Length - 1;
            else
                StringColorIndex = (int)i;

            strMouseDown = true;
        }

        private void StringColorBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (strMouseDown)
            {
                double i = (e.GetPosition(StringColorBar).X / StringColorBar.ActualWidth) * Colors.Length;
                if (i < 0)
                    StringColorIndex = 0;
                else if (i > Colors.Length)
                    StringColorIndex = Colors.Length - 1;
                else
                    StringColorIndex = (int)i;
            }
        }

        private void StringColorBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //double i = (e.GetPosition(StringColorBar).X / StringColorBar.ActualWidth) * Colors.Length;
            //if (i < 0)
            //    StringColorIndex = 0;
            //else if (i > Colors.Length)
            //    StringColorIndex = Colors.Length - 1;
            //else
            //    StringColorIndex = (int)i;

            strMouseDown = false;
            Properties.Settings.Default.Save();
        }

        bool commMouseDown = false;
        private void CommentColorBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            double i = (e.GetPosition(CommentColorBar).X / CommentColorBar.ActualWidth) * Colors.Length;
            if (i < 0)
                CommentColorIndex = 0;
            else if (i > Colors.Length)
                CommentColorIndex = Colors.Length - 1;
            else
                CommentColorIndex = (int)i;

            commMouseDown = true;
        }

        private void CommentColorBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (commMouseDown)
            {
                double i = (e.GetPosition(CommentColorBar).X / CommentColorBar.ActualWidth) * Colors.Length;
                if (i < 0)
                    CommentColorIndex = 0;
                else if (i > Colors.Length)
                    CommentColorIndex = Colors.Length - 1;
                else
                    CommentColorIndex = (int)i;
            }
        }

        private void CommentColorBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //double i = (e.GetPosition(CommentColorBar).X / CommentColorBar.ActualWidth) * Colors.Length;
            //if (i < 0)
            //    CommentColorIndex = 0;
            //else if (i > Colors.Length)
            //    CommentColorIndex = Colors.Length - 1;
            //else
            //    CommentColorIndex = (int)i;

            commMouseDown = false;
            Properties.Settings.Default.Save();
        }

        private void StringColorBar_MouseLeave(object sender, MouseEventArgs e)
        {
            strMouseDown = false;
            Properties.Settings.Default.Save();
        }

        private void CommentColorBar_MouseLeave(object sender, MouseEventArgs e)
        {
            commMouseDown = false;
            Properties.Settings.Default.Save();
        }

        private void ResetButton_OnClick(object sender, EventArgs e)
        {
            ResetColors();
        }
    }
}
