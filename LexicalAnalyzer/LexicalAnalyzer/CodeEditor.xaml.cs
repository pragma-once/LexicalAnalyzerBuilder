using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace LexicalAnalyzer
{
    /// <summary>
    /// Interaction logic for CodeEditor.xaml
    /// </summary>
    public partial class CodeEditor : UserControl
    {
        public CodeEditor()
        {
            InitializeComponent();
        }

        //public Brush BackgroundColor = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        //public Brush NormalColor = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        public SolidColorBrush StringBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        public SolidColorBrush CommentBrush = new SolidColorBrush(Color.FromRgb(0, 0, 0));

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            EditorT.Foreground = fg.Brush;
            //EditorT.TextChanged += EditorT_TextChanged;
            EditorT.KeyUp += EditorT_KeyUp;
            EditorT.SetValue(Paragraph.LineHeightProperty, 1.0);
            EditorT.Background = BG.Brush;
            UpdateColors();
            //BG.FadeTo(0, 0, 0, 2);
            //FG.FadeTo(255, 255, 255, 2);
        }

        public UI.SolidColorBrushController BG = new UI.SolidColorBrushController(Color.FromRgb(0, 0, 0));
        public UI.SolidColorBrushController fg = new UI.SolidColorBrushController(Color.FromRgb(0, 0, 0));
        public UI.SolidColorBrushController FG
        {
            get { return fg; }
            set { fg = value; EditorT.Foreground = fg.Brush; }
        }

        private void EditorT_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            UpdateLineNumbers();
            
            UpdateColors();
        }
        
        //private async void EditorT_TextChanged(object sender, TextChangedEventArgs e)

        public static string GetText(RichTextBox rtb)
        {
            TextRange textRange = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
            string text = textRange.Text;
            return text;
        }

        void UpdateLineNumbers()
        {

        }

        void HighlightRun(string Text, TextPointer Start, TextPointer End,
            ref bool comment, ref bool str1, ref bool str2, ref bool ignore,
            ref int last_start, ref int j,
            // ref TextPointer StartPosition, ref TextPointer EndPosition,
            ref bool schedule_end)
        {
            if (Text.Length == 0)
                return;

            TextPointer StartPosition = Start;
            TextPointer EndPosition = null;

            try
            {
                if (schedule_end)
                {
                    SolidColorBrush brush;
                    if (comment)
                    {
                        brush = CommentBrush;
                        comment = false;
                    }
                    else if (str1)
                    {
                        brush = StringBrush;
                        str1 = false;
                    }
                    else if (str2)
                    {
                        brush = StringBrush;
                        str2 = false;
                    }
                    else
                    {
                        throw new Exception("Impossible !");
                    }
                    EndPosition = Start.GetPositionAtOffset(0, LogicalDirection.Backward);
                    TextRange range = new TextRange(StartPosition, EndPosition);
                    range.ApplyPropertyValue(TextElement.ForegroundProperty, CommentBrush);
                    last_start = j + 1;
                    //if (last_start < Text.Length)
                    StartPosition = Start.GetPositionAtOffset(0, LogicalDirection.Forward);
                    schedule_end = false;
                }
                for (int i = 0; i < Text.Length; i++)
                {
                    char ch = Text[i];

                    if (comment)
                    {
                        if (ch == '#')
                        {
                            EndPosition = Start.GetPositionAtOffset(i + 1, LogicalDirection.Backward);
                            if (EndPosition == null)
                            {
                                EndPosition = End;
                            }
                            else
                            {
                                TextRange range = new TextRange(StartPosition, EndPosition);
                                range.ApplyPropertyValue(TextElement.ForegroundProperty, CommentBrush);
                                last_start = j + 1;
                                //if (last_start < Text.Length)
                                StartPosition = Start.GetPositionAtOffset(i + 1, LogicalDirection.Forward);
                                comment = false;
                            }
                        }
                    }
                    else if (str1)
                    {
                        if (ignore)
                        {
                            ignore = false;
                            continue;
                        }
                        if (ch == '\\')
                        {
                            ignore = true;
                            continue;
                        }
                        if (ch == '\'')
                        {
                            EndPosition = Start.GetPositionAtOffset(i + 1, LogicalDirection.Backward);
                            if (EndPosition == null)
                            {
                                EndPosition = End;
                            }
                            else
                            {
                                TextRange range = new TextRange(StartPosition, EndPosition);
                                range.ApplyPropertyValue(TextElement.ForegroundProperty, StringBrush);
                                last_start = j + 1;
                                //if (last_start < Text.Length)
                                StartPosition = Start.GetPositionAtOffset(i + 1, LogicalDirection.Forward);
                                str1 = false;
                            }
                        }
                    }
                    else if (str2)
                    {
                        if (ignore)
                        {
                            ignore = false;
                            continue;
                        }
                        if (ch == '\\')
                        {
                            ignore = true;
                            continue;
                        }
                        if (ch == '"')
                        {
                            EndPosition = Start.GetPositionAtOffset(i + 1, LogicalDirection.Backward);
                            if (EndPosition == null)
                            {
                                EndPosition = End;
                            }
                            else
                            {
                                TextRange range = new TextRange(StartPosition, EndPosition);
                                range.ApplyPropertyValue(TextElement.ForegroundProperty, StringBrush);
                                last_start = j + 1;
                                //if (last_start < Text.Length)
                                StartPosition = Start.GetPositionAtOffset(i + 1, LogicalDirection.Forward);
                                str2 = false;
                            }
                        }
                    }
                    else
                    {
                        if (ch == '#')
                        {
                            if (last_start < j)
                            {
                                EndPosition = Start.GetPositionAtOffset(i, LogicalDirection.Backward);
                                if (EndPosition == null)
                                {
                                    EndPosition = End;
                                }
                                else
                                {
                                    TextRange range = new TextRange(StartPosition, EndPosition);
                                    range.ApplyPropertyValue(TextElement.ForegroundProperty, FG.Brush);
                                }
                            }
                            StartPosition = Start.GetPositionAtOffset(i, LogicalDirection.Forward);
                            last_start = j;
                            comment = true;
                        }
                        else if (ch == '\'')
                        {
                            if (last_start < j)
                            {
                                EndPosition = Start.GetPositionAtOffset(i, LogicalDirection.Backward);
                                if (EndPosition == null)
                                {
                                    EndPosition = End;
                                }
                                else
                                {
                                    TextRange range = new TextRange(StartPosition, EndPosition);
                                    range.ApplyPropertyValue(TextElement.ForegroundProperty, FG.Brush);
                                }
                            }
                            StartPosition = Start.GetPositionAtOffset(i, LogicalDirection.Forward);
                            last_start = j;
                            str1 = true;
                        }
                        else if (ch == '"')
                        {
                            if (last_start < j)
                            {
                                EndPosition = Start.GetPositionAtOffset(i, LogicalDirection.Backward);
                                if (EndPosition == null)
                                {
                                    EndPosition = End;
                                }
                                else
                                {
                                    TextRange range = new TextRange(StartPosition, EndPosition);
                                    range.ApplyPropertyValue(TextElement.ForegroundProperty, FG.Brush);
                                }
                            }
                            StartPosition = Start.GetPositionAtOffset(i, LogicalDirection.Forward);
                            last_start = j;
                            str2 = true;
                        }
                    }

                    j++;
                }

                if (last_start < j) // (Text.Length > 0 && last_start < j)
                {
                    EndPosition = Start.GetPositionAtOffset(Text.Length, LogicalDirection.Backward);
                    TextRange range = new TextRange(StartPosition, End);
                    if (str1 || str2)
                    {
                        range.ApplyPropertyValue(TextElement.ForegroundProperty, StringBrush);
                    }
                    else if (comment)
                    {
                        range.ApplyPropertyValue(TextElement.ForegroundProperty, CommentBrush);
                    }
                    else
                    {
                        range.ApplyPropertyValue(TextElement.ForegroundProperty, FG.Brush);
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing :D
            }
        }

        List<TextPointer> Normals = new List<TextPointer>();
        List<TextPointer> NormalsSwap = new List<TextPointer>();
        int update_colors_thread_id = 0;
        public async void UpdateColors()
        {
            update_colors_thread_id++;
            int this_thread = update_colors_thread_id;
            await Task.Delay(1);

            bool comment = false;
            bool str1 = false;
            bool str2 = false;
            bool ignore = false;
            int last_start = 0;
            int j = 0;
            bool schedule_end = false;

            TextPointer start;
            int counter;
            if (Normals.Count > 0)
            {
                TextPointer caret = EditorT.CaretPosition;
                int i = Normals.Count - 1;
                while (Normals[i].CompareTo(caret) == 1)
                {
                    i--;
                    if (i < 0)
                        break;
                }

                if (i > 64)
                {
                    i -= 16;
                    start = Normals[i];
                    counter = 0;
                    while (true)
                    {
                        TextPointer end = start.GetNextContextPosition(LogicalDirection.Forward);
                        if (end == null)
                        {
                            break;
                        }
                        TextRange range = new TextRange(start, end);

                        HighlightRun(range.Text, start, end,
                                        ref comment, ref str1, ref str2, ref ignore,
                                        ref last_start, ref j,
                                        ref schedule_end);

                        start = end;
                        counter++;
                        if (counter > 4)
                        {
                            await Task.Delay(1);
                            if (update_colors_thread_id != this_thread)
                                return;
                            counter = 0;
                        }
                    }
                }
            }
            NormalsSwap.Clear();
            start = EditorT.Document.ContentStart;
            counter = 0;
            comment = false;
            str1 = false;
            str2 = false;
            ignore = false;
            last_start = 0;
            j = 0;
            schedule_end = false;
            while (true)
            {
                TextPointer end = start.GetNextContextPosition(LogicalDirection.Forward);
                if (end == null)
                {
                    break;
                }
                TextRange range = new TextRange(start, end);

                if (!comment && !str1 && !str2)
                    NormalsSwap.Add(start);

                HighlightRun(range.Text, start, end,
                                ref comment, ref str1, ref str2, ref ignore,
                                ref last_start, ref j,
                                ref schedule_end);
                
                start = end;
                counter++;
                if (counter > 4)
                {
                    await Task.Delay(1);
                    if (update_colors_thread_id != this_thread)
                        return;
                    counter = 0;
                }
            }
            List<TextPointer> tmp = Normals;
            Normals = NormalsSwap;
            NormalsSwap = tmp;

            /*int i = 0;
            while (true)
            {
                int k = 0;
                Run run = null;
                foreach (Paragraph p in EditorT.Document.Blocks.OfType<Paragraph>())
                {
                    foreach (Run r in p.Inlines.OfType<Run>())
                    {
                        if (i == k)
                        {
                            run = r;
                            k++;
                            break;
                        }
                        k++;
                    }
                }

                if (k <= i)
                    break;

                HighlightRun(run, // Text, RunStart, RunEnd,
                                ref comment, ref str1, ref str2, ref ignore,
                                ref last_start, ref j,
                                ref StartPosition, ref EndPosition,
                                ref schedule_end);

                i++;
            }*/

            /*if (EditorT.Document.Blocks.Count > 0)
            {
                Block block = EditorT.Document.Blocks.FirstBlock;
                while (true)
                {
                    if (block == null)
                        break;
                    if (block.GetType() == typeof(Paragraph))
                        break;
                    block = block.NextBlock;
                }
                if (block == null)
                    return;
                Paragraph p = block as Paragraph;

                while (true)
                {
                    if (p.Inlines.Count > 0)
                    {
                        Inline inline = p.Inlines.FirstInline;
                        while (true)
                        {
                            if (inline == null)
                                break;
                            if (inline.GetType() == typeof(Run))
                                break;
                            inline = inline.NextInline;
                        }
                        if (inline == null)
                            return;
                        Run r = inline as Run;

                        while (true)
                        {
                            HighlightRun(r, // Text, RunStart, RunEnd,
                                ref comment, ref str1, ref str2, ref ignore,
                                ref last_start, ref j,
                                ref StartPosition, ref EndPosition,
                                ref schedule_end);
                            
                            inline = inline.NextInline;
                            while (true)
                            {
                                if (inline == null)
                                    break;
                                if (inline.GetType() == typeof(Run))
                                    break;
                                inline = inline.NextInline;
                            }
                            if (inline == null)
                                return;
                            r = inline as Run;
                        }
                    }

                    block = block.NextBlock;
                    while (true)
                    {
                        if (block == null)
                            break;
                        if (block.GetType() == typeof(Paragraph))
                            break;
                        block = block.NextBlock;
                    }
                    if (block == null)
                        break;
                    p = block as Paragraph;
                }
            }*/

            /*Queue<Run> Runs = new Queue<Run>();
            foreach (Paragraph p in EditorT.Document.Blocks.OfType<Paragraph>())
            {
                foreach (Run r in p.Inlines.OfType<Run>())
                {
                    Runs.Enqueue(r);
                }
            }

            while (Runs.Count > 0)
            {
                Run r = Runs.Dequeue();
                
                //Text = r.Text;
                //StartPosition = r.ContentStart;
                //TextPointer RunStart = StartPosition;
                //TextPointer RunEnd = r.ContentEnd;

                HighlightRun(r, // Text, RunStart, RunEnd,
                    ref comment, ref str1, ref str2, ref ignore,
                    ref last_start, ref j,
                    ref StartPosition, ref EndPosition,
                    ref schedule_end);

                //await Task.Delay(1);
            }*/

            /*while (pointer != null)
            {
                Text = pointer.GetTextInRun(LogicalDirection.Forward);
                //tst += Text;
                
                StartPosition = pointer;
                TextPointer RunStart = pointer;

                HighlightRun(Text, RunStart,
                    ref comment, ref str1, ref str2, ref ignore,
                    ref last_start, ref j,
                    ref StartPosition, ref EndPosition);

                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }*/

            //tst += "";
        }

        public string Text
        {
            get
            {
                string Text = "";

                bool IsFirst = true;
                foreach (Paragraph p in EditorT.Document.Blocks.OfType<Paragraph>())
                {
                    if (IsFirst)
                        IsFirst = false;
                    else
                        Text += '\n';
                    foreach (Run r in p.Inlines.OfType<Run>())
                    {
                        Text += r.Text;
                    }
                }

                return Text;
            }

            set
            {
                EditorT.Document.Blocks.Clear();
                EditorT.AppendText(value);
            }
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            EditorT.Focus();
        }
    }
}
