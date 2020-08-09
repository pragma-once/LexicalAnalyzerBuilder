using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for CustomButton.xaml
    /// </summary>
    public partial class CustomButton : UserControl
    {
        public CustomButton()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            MainGrid.Background = BG.Brush;
            ButtonLabel.Foreground = FG.Brush;
        }

        UI.SolidColorBrushController FG = new UI.SolidColorBrushController(0, 0, 0);
        UI.SolidColorBrushController BG = new UI.SolidColorBrushController(0, 0, 0);

        Color Nbg;
        Color Hbg;
        Color Pbg;

        Color Nfg;
        Color Hfg;
        Color Pfg;

        public float FadeSpeed = 2;

        public Color NormalBackground
        {
            get { return Nbg; }
            set
            {
                Nbg = value;
                if (!Clicking)
                {
                    BG.FadeTo(value, FadeSpeed);
                }
            }
        }
        public Color HoveredBackground
        {
            get { return Hbg; }
            set
            {
                Hbg = value;
            }
        }
        public Color PressedBackground
        {
            get { return Pbg; }
            set
            {
                Pbg = value;
                if (Clicking)
                {
                    BG.FadeTo(value, FadeSpeed);
                }
            }
        }

        public Color NormalForeground
        {
            get { return Nfg; }
            set
            {
                Nfg = value;
                if (!Clicking)
                {
                    FG.FadeTo(value, FadeSpeed);
                }
            }
        }
        public Color HoveredForeground
        {
            get { return Hfg; }
            set
            {
                Hfg = value;
            }
        }
        public Color PressedForeground
        {
            get { return Pfg; }
            set
            {
                Pfg = value;
                if (Clicking)
                {
                    FG.FadeTo(value, FadeSpeed);
                }
            }
        }

        [Description("Test text displayed in the textbox"), Category("Common")]
        public string Text
        {
            set
            {
                ButtonLabel.Content = value;
            }
            get
            {
                return ButtonLabel.Content.ToString();
            }
        }

        public event EventHandler<EventArgs> OnClick;

        bool Clicking = false;

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            BG.FadeTo(Hbg, FadeSpeed);
            FG.FadeTo(Hfg, FadeSpeed);
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            BG.FadeTo(Nbg, FadeSpeed);
            FG.FadeTo(Nfg, FadeSpeed);
            Clicking = false;
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            BG.FadeTo(Pbg, FadeSpeed);
            FG.FadeTo(Pfg, FadeSpeed);
            Clicking = true;
        }

        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            BG.FadeTo(Hbg, FadeSpeed);
            FG.FadeTo(Hfg, FadeSpeed);
            if (Clicking)
            {
                OnClick?.Invoke(this, null);
            }
            Clicking = false;
        }
    }
}
