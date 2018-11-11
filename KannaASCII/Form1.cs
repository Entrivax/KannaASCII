using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KannaASCII
{
    public partial class Form1 : Form
    {
        Animation _animation;
        private readonly float _timeBeforeClose;

        public Form1(string animationFile, float timeBeforeClose)
        {
            InitializeComponent();

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.TransparencyKey = Color.Transparent;
            Location = new Point(20 + Screen.AllScreens.Min(s => s.Bounds.Left), 200);

            _animation = JsonConvert.DeserializeObject<Animation>(File.ReadAllText(animationFile));
            _animation.ImageSources = new List<ImageSource>();
            foreach (var img in _animation.Images)
            {
                var source = File.ReadAllLines(img).Aggregate((a, b) => a + '\n' + b);
                _animation.ImageSources.Add(new ImageSource
                {
                    Source = source,
                    Size = TextRenderer.MeasureText(source, label1.Font)
                });
            }
            var width = 0;
            var height = 0;
            for (var i = 0; i < _animation.Frames.Count; i++)
            {
                var frame = _animation.Frames[i];
                foreach (var subframe in frame)
                {
                    width = Math.Max(_animation.ImageSources[subframe].Size.Width + i * _animation.Offset, width);
                    height = Math.Max(_animation.ImageSources[subframe].Size.Height, height);
                }
            }
            this.Size = new Size(width * TextRenderer.MeasureText(" ", label1.Font).Width, height);
            _timeBeforeClose = timeBeforeClose;
        }

        protected override void OnPaintBackground(PaintEventArgs e) { /* Ignore */ }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                var lastValue = "";
                for (var i = 0; i < _animation.Frames.Count; i++)
                {
                    var frame = _animation.Frames[i];
                    foreach (var subframe in frame)
                    {
                        lastValue = BleedTemplate(lastValue, i * _animation.Offset, _animation.ImageSources[subframe].Source);

                        this.BeginInvoke(new Action(() =>
                        {
                            label1.Text = lastValue;
                        }));

                        await Task.Delay(1000 / _animation.Framerate);
                    }
                }
                await Task.Delay((int)(1000 * _timeBeforeClose));
                this.BeginInvoke(new Action(() =>
                {
                    this.Close();
                }));
            });
        }

        private static string BleedTemplate(string original, int charsOffset, string template)
        {
            var originalLines = original.Split(new[] { '\n' });
            var templateLines = template.Split(new[] { '\n' });
            var lineCount = Math.Max(originalLines.Length, templateLines.Length);
            var newLines = new List<string>();
            for (var i = 0; i < lineCount; i++)
            {
                newLines.Add(BleedLine(i < originalLines.Length ? originalLines[i] : null, charsOffset, i < templateLines.Length ? templateLines[i] : null));
            }
            return newLines.Aggregate((a, b) => a + '\n' + b);
        }

        private static string BleedLine(string original, int charsOffset, string newLine)
        {
            var builder = new StringBuilder(Math.Max(newLine.Length + charsOffset, original?.Length ?? 0));
            builder.Append(original?.PadRight(charsOffset, ' ') ?? "".PadRight(charsOffset, ' '), 0, charsOffset);
            var lineLength = Math.Max(newLine.Length, (original?.Length ?? 0) - charsOffset);
            for (var i = 0; i < lineLength; i++)
            {
                builder.Append(BleedChar((original?.Length ?? 0) > i + charsOffset ? original[i + charsOffset] : ' ', (newLine?.Length ?? 0) > i ? newLine[i] : ' '));
            }
            return builder.ToString();
        }

        private static char BleedChar(char originalChar, char newChar)
        {
            return newChar != ' ' ? newChar : originalChar;
        }
    }
}
