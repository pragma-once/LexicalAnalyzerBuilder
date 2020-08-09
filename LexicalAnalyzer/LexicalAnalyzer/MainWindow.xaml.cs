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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ToolBarGrid.Background = ToolBarBG.Brush;
            LogGrid.Background = ToolBarBG.Brush;
            OutputList.Background = ColorPicker.BG.Brush;
            OutputList.Foreground = ColorPicker.FG.Brush;
            ColorPicker.SetBrushes(Editor.StringBrush, Editor.CommentBrush);

            PreviewToolBar.Background = ToolBarBG.Brush;
            PreviewLogGrid.Background = ToolBarBG.Brush;
            PreviewOutputList.Background = ColorPicker.BG.Brush;
            PreviewOutputList.Foreground = ColorPicker.FG.Brush;
            PreviewEditor.StringBrush = PreviewEditor.FG.Brush;
            PreviewEditor.CommentBrush = PreviewEditor.FG.Brush;
            
            DarkModeToggle.OnClick += DarkModeToggle_OnClick;
            BuildButton.OnClick += BuildButton_OnClick;
            CustomizeButton.OnClick += CustomizeButton_OnClick;
            ColorPickerCloseButton.OnClick += ColorPickerCloseButton_OnClick;
            SaveButton.OnClick += SaveButton_OnClick;

            PreviewBackButton.OnClick += PreviewBackButton_OnClick;
            PreviewProcessButton.OnClick += PreviewProcessButton_OnClick;
            
            Editor.Focus();
            Editor.Text = Properties.Settings.Default.EditorText;
            //Clipboard.SetText(Editor.Text); // (was to test)

            DarkMode = Properties.Settings.Default.Dark;
            UpdateColors(6, 100);
            await Task.Delay(1000);
            Editor.UpdateColors();

            Interface = new UI.WPFInterface(OutputList);
            PreviewInterface = new UI.WPFInterface(PreviewOutputList);
            StateMachine = new Core.StateMachine(Interface);
        }

        bool DarkMode = true;
        UI.SolidColorBrushController ToolBarBG = new UI.SolidColorBrushController(0, 0, 0);
        UI.SolidColorBrushController ToolBarFG = new UI.SolidColorBrushController(0, 0, 0);

        UI.WPFInterface Interface;
        UI.WPFInterface PreviewInterface;
        Core.StateMachine StateMachine;

        async void UpdateColors(float Speed = 4, int Delays = 200)
        {
            Color b1;
            Color f1;
            Color b2;
            Color f2;
            Color b3;
            Color f3;
            Color nb;
            Color nf;
            Color hb;
            Color hf;
            Color pb;
            Color pf;

            if (DarkMode)
            {
                b1 = Color.FromRgb(0, 0, 0);
                f1 = Color.FromRgb(255, 255, 255);
                b2 = Color.FromRgb(50, 50, 50);
                f2 = Color.FromRgb(200, 200, 200);
                b3 = Color.FromRgb(100, 100, 100);
                f3 = Color.FromRgb(200, 200, 200);

                nb = Color.FromRgb(100, 100, 100);
                nf = Color.FromRgb(200, 200, 200);
                hb = Color.FromRgb(220, 220, 220);
                hf = Color.FromRgb(50, 50, 50);
                pb = Color.FromRgb(200, 200, 200);
                pf = Color.FromRgb(255, 255, 255);
            }
            else
            {
                b1 = Color.FromRgb(255, 255, 255);
                f1 = Color.FromRgb(0, 0, 0);
                b2 = Color.FromRgb(240, 240, 240);
                f2 = Color.FromRgb(50, 50, 50);
                b3 = Color.FromRgb(220, 220, 220);
                f3 = Color.FromRgb(100, 100, 100);

                nb = Color.FromRgb(220, 220, 220);
                nf = Color.FromRgb(0, 0, 0);
                hb = Color.FromRgb(100, 100, 100);
                hf = Color.FromRgb(220, 220, 220);
                pb = Color.FromRgb(80, 80, 80);
                pf = Color.FromRgb(255, 255, 255);
            }

            await Task.Delay(Delays);
            ColorPicker.BG.FadeTo(b3, Speed);
            ColorPicker.FG.FadeTo(f3, Speed);
            ColorPicker.resetButton.FadeSpeed = Speed;
            ColorPicker.resetButton.NormalBackground = nb;
            ColorPicker.resetButton.NormalForeground = nf;
            ColorPicker.resetButton.HoveredBackground = hb;
            ColorPicker.resetButton.HoveredForeground = hf;
            ColorPicker.resetButton.PressedBackground = pb;
            ColorPicker.resetButton.PressedForeground = pf;
            ColorPicker.resetButton.FadeSpeed = 8;
            //await Task.Delay(Delays);
            Editor.BG.FadeTo(b1, Speed);
            Editor.FG.FadeTo(f1, Speed);
            PreviewEditor.BG.FadeTo(b1, Speed);
            PreviewEditor.FG.FadeTo(f1, Speed);
            await Task.Delay(Delays);
            ToolBarBG.FadeTo(b2, Speed);
            ToolBarFG.FadeTo(f2, Speed);
            await Task.Delay(Delays);
            DarkModeToggle.FadeSpeed = Speed;
            DarkModeToggle.NormalBackground = nb;
            DarkModeToggle.NormalForeground = nf;
            DarkModeToggle.HoveredBackground = hb;
            DarkModeToggle.HoveredForeground = hf;
            DarkModeToggle.PressedBackground = pb;
            DarkModeToggle.PressedForeground = pf;
            DarkModeToggle.FadeSpeed = 8;
            //await Task.Delay(Delays);
            SaveButton.FadeSpeed = Speed;
            SaveButton.NormalBackground = nb;
            SaveButton.NormalForeground = nf;
            SaveButton.HoveredBackground = hb;
            SaveButton.HoveredForeground = hf;
            SaveButton.PressedBackground = pb;
            SaveButton.PressedForeground = pf;
            SaveButton.FadeSpeed = 8;
            //await Task.Delay(Delays);
            BuildButton.FadeSpeed = Speed;
            BuildButton.NormalBackground = nb;
            BuildButton.NormalForeground = nf;
            BuildButton.HoveredBackground = hb;
            BuildButton.HoveredForeground = hf;
            BuildButton.PressedBackground = pb;
            BuildButton.PressedForeground = pf;
            BuildButton.FadeSpeed = 8;
            //await Task.Delay(Delays);
            CustomizeButton.FadeSpeed = Speed;
            CustomizeButton.NormalBackground = nb;
            CustomizeButton.NormalForeground = nf;
            CustomizeButton.HoveredBackground = hb;
            CustomizeButton.HoveredForeground = hf;
            CustomizeButton.PressedBackground = pb;
            CustomizeButton.PressedForeground = pf;
            CustomizeButton.FadeSpeed = 8;
            //await Task.Delay(Delays);
            ColorPickerCloseButton.FadeSpeed = Speed;
            ColorPickerCloseButton.NormalBackground = nb;
            ColorPickerCloseButton.NormalForeground = nf;
            ColorPickerCloseButton.HoveredBackground = hb;
            ColorPickerCloseButton.HoveredForeground = hf;
            ColorPickerCloseButton.PressedBackground = pb;
            ColorPickerCloseButton.PressedForeground = pf;
            ColorPickerCloseButton.FadeSpeed = 8;
            //await Task.Delay(Delays);
            PreviewBackButton.FadeSpeed = Speed;
            PreviewBackButton.NormalBackground = nb;
            PreviewBackButton.NormalForeground = nf;
            PreviewBackButton.HoveredBackground = hb;
            PreviewBackButton.HoveredForeground = hf;
            PreviewBackButton.PressedBackground = pb;
            PreviewBackButton.PressedForeground = pf;
            PreviewBackButton.FadeSpeed = 8;
            //await Task.Delay(Delays);
            PreviewProcessButton.FadeSpeed = Speed;
            PreviewProcessButton.NormalBackground = nb;
            PreviewProcessButton.NormalForeground = nf;
            PreviewProcessButton.HoveredBackground = hb;
            PreviewProcessButton.HoveredForeground = hf;
            PreviewProcessButton.PressedBackground = pb;
            PreviewProcessButton.PressedForeground = pf;
            PreviewProcessButton.FadeSpeed = 8;
        }

        private void DarkModeToggle_OnClick(object sender, EventArgs e)
        {
            DarkMode = !DarkMode;
            UpdateColors();
            Properties.Settings.Default.Dark = DarkMode;
            Properties.Settings.Default.Save();
        }

        private void BuildButton_OnClick(object sender, EventArgs e)
        {
            Interface.Clear();
            StateMachine.Interface = Interface;
            if (StateMachine.Build(Editor.Text))
            {
                TogglePreview(true);
            }
        }

        int color_picker_toggle_thread_id = 0;
        async void ToggleColorPicker(bool OpenPanel, double Speed = 8)
        {
            color_picker_toggle_thread_id++;
            int this_thread = color_picker_toggle_thread_id;
            await Task.Delay(25);

            Speed *= 0.025;
            double TargetWidth;
            if (OpenPanel)
                TargetWidth = 192;
            else
                TargetWidth = 0;

            while (Math.Abs(TargetWidth - ColorPickerGrid.Width) > 0.01)
            {
                ColorPickerGrid.Width += (TargetWidth - ColorPickerGrid.Width) * Speed;
                await Task.Delay(25);
                if (color_picker_toggle_thread_id != this_thread)
                    return;
            }
            ColorPickerGrid.Width = TargetWidth;
        }

        private void CustomizeButton_OnClick(object sender, EventArgs e)
        {
            ToggleColorPicker(true);
        }

        private void ColorPickerCloseButton_OnClick(object sender, EventArgs e)
        {
            ToggleColorPicker(false);
        }

        bool Preview = false;
        int preview_toggle_thread_id = 0;
        async void TogglePreview(bool Open, double Speed = 8)
        {
            preview_toggle_thread_id++;
            int this_thread = preview_toggle_thread_id;
            await Task.Delay(25);

            if (Preview != Open)
            {
                if (Open)
                {
                    PreviewEditor.Focus();
                    PreviewInterface.Clear();
                }
                else
                {
                    Editor.Focus();
                }
            }

            Speed *= 0.025;
            Preview = Open;
            double TargetHeight;
            if (Open)
                TargetHeight = CodeMainGrid.ActualHeight;
            else
                TargetHeight = 0;

            while (Math.Abs(TargetHeight - PreviewGrid.Height) > 0.01)
            {
                PreviewGrid.Height += (TargetHeight - PreviewGrid.Height) * Speed;
                await Task.Delay(25);
                if (preview_toggle_thread_id != this_thread)
                    return;
            }
            PreviewGrid.Height = TargetHeight;
        }

        void ProcessPreview()
        {
            PreviewInterface.Clear();
            StateMachine.Interface = PreviewInterface;
            if (StateMachine.Process(PreviewEditor.Text, out List<Core.Token> Tokens))
            {
                PreviewInterface.Print("The string was accepted!");
            }
            else
            {
                if (Tokens != null)
                    PreviewInterface.Print("The string was not accepted.");
            }
            
            if (Tokens == null)
            {
                PreviewInterface.Print("No tokens to show.");
            }
            else
            {
                if (Tokens.Count > 0)
                {
                    int errors = 0;
                    int warnings = 0;
                    int hints = 0;
                    foreach (Core.Token token in Tokens)
                    {
                        if (token.IsMessage)
                        {
                            if (token.MessageType == Core.TokenMessageType.Error)
                                errors++;
                            if (token.MessageType == Core.TokenMessageType.Warning)
                                warnings++;
                            if (token.MessageType == Core.TokenMessageType.Hint)
                                hints++;
                        }
                    }
                    PreviewInterface.Print("It contains "
                        + errors + " error" + (errors == 1 ? "" : "s") + ", "
                        + warnings + " warning" + (warnings == 1 ? "" : "s")  + " and "
                        + hints + " hint" + (hints == 1 ? "" : "s"));

                    PreviewInterface.Print("Here are the tokens:");
                    foreach (Core.Token token in Tokens)
                    {
                        if (token.IsMessage)
                            PreviewInterface.Print(
                            "Message Token: { Character Index: " + token.CharacterIndex +
                            ", Line: " + token.LocationY +
                            ", Column: " + token.LocationX +
                            ", Message Type: " + token.MessageType +
                            ", Message: \"" + token.Value +
                            "\" }"
                            );
                        else
                            PreviewInterface.Print(
                                "Processed Substring Token: { Character Index: " + token.CharacterIndex +
                                ", Line: " + token.LocationY +
                                ", Column: " + token.LocationX +
                                ", Type: '" + token.Type +
                                "', Value: \"" + token.Value +
                                "\" }"
                                );
                    }
                }
                else
                {
                    PreviewInterface.Print("No tokens to show.");
                }
            }
        }

        private void PreviewBackButton_OnClick(object sender, EventArgs e)
        {
            TogglePreview(false);
        }

        private void PreviewProcessButton_OnClick(object sender, EventArgs e)
        {
            ProcessPreview();
        }

        private void SaveButton_OnClick(object sender, EventArgs e)
        {
            Properties.Settings.Default.EditorText = Editor.Text;
            Properties.Settings.Default.Save();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Preview)
                TogglePreview(true);
        }

        bool LCtrl = false;
        bool RCtrl = false;

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                LCtrl = true;
            }
            else if (e.Key == Key.RightCtrl)
            {
                RCtrl = true;
            }

            if (e.Key == Key.F5)
            {
                if (Preview)
                    PreviewProcessButton_OnClick(this, null);
                else
                    BuildButton_OnClick(this, null);
            }
            else if (e.Key == Key.Escape)
            {
                TogglePreview(false);
            }
            else if (e.Key == Key.S && (LCtrl || RCtrl))
            {
                SaveButton_OnClick(this, null);
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
            {
                LCtrl = false;
            }
            else if (e.Key == Key.RightCtrl)
            {
                RCtrl = false;
            }
        }
    }
}
