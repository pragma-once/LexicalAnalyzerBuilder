using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Controls;

namespace LexicalAnalyzer.UI
{
    public class SolidColorBrushController
    {
        public SolidColorBrushController(Color InitialColor)
        {
            brush = new SolidColorBrush(InitialColor);
        }

        public SolidColorBrushController(byte R, byte G, byte B)
        {
            brush = new SolidColorBrush(Color.FromRgb(R, G, B));
        }

        public SolidColorBrushController(byte A, byte R, byte G, byte B)
        {
            brush = new SolidColorBrush(Color.FromArgb(A, R, G, B));
        }

        public SolidColorBrushController(SolidColorBrush Brush)
        {
            brush = Brush;
        }

        SolidColorBrush brush;
        public SolidColorBrush Brush
        {
            get { return brush; }
        }

        int thread_id = 0;
        public async void FadeTo(Color NewColor, float Speed)
        {
            thread_id++;
            int this_thread = thread_id;

            Speed *= 0.025F;
            Color OldColor = Brush.Color;
            await Task.Delay(25);
            for (float i = 0; i < 1; i += Speed)
            {
                Brush.Color = Color.FromRgb(
                    (byte)(OldColor.R + (NewColor.R - OldColor.R) * i),
                    (byte)(OldColor.G + (NewColor.G - OldColor.G) * i),
                    (byte)(OldColor.B + (NewColor.B - OldColor.B) * i)
                    );

                await Task.Delay(25);
                if (thread_id != this_thread)
                    return;
            }
            Brush.Color = NewColor;
        }

        public void FadeTo(byte R, byte G, byte B, float Speed)
        {
            FadeTo(Color.FromRgb(R, G, B), Speed);
        }

        public void FadeTo(byte A, byte R, byte G, byte B, float Speed)
        {
            FadeTo(Color.FromArgb(A, R, G, B), Speed);
        }
    }

    class WPFInterface : Core.Interface
    {
        public WPFInterface(ListView OutputList)
        {
            this.OutputList = OutputList;
        }

        ListView OutputList;

        public void Clear()
        {
            OutputList.Items.Clear();
        }

        public override void Print(string Message)
        {
            OutputList.Items.Add(Message);
        }

        /*SolidColorBrush BGBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
        SolidColorBrush FGBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
        int dark_toggle_thread_count = 0;
        public async void ToggleDarkMode(bool Dark)
        {
            dark_toggle_thread_count++;
            await Task.Delay(25);
            if (Dark)
            {
                for (byte i = FGBrush.Color.R; i < 248; i += 8)
                {
                    byte reverse = (byte)(255 - i);
                    BGBrush.Color = Color.FromRgb(reverse, reverse, reverse);
                    FGBrush.Color = Color.FromRgb(i, i, i);
                    await Task.Delay(1);
                    if (dark_toggle_thread_count > 1)
                    {
                        dark_toggle_thread_count--;
                        return;
                    }
                }
            }
            else
            {
                for (byte i = BGBrush.Color.R; i < 248; i += 8)
                {
                    byte reverse = (byte)(255 - i);
                    BGBrush.Color = Color.FromRgb(i, i, i);
                    FGBrush.Color = Color.FromRgb(reverse, reverse, reverse);
                    await Task.Delay(1);
                    if (dark_toggle_thread_count > 1)
                    {
                        dark_toggle_thread_count--;
                        return;
                    }
                }
            }
            dark_toggle_thread_count--;
        }*/
    }
}
