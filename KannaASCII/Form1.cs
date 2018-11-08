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
        int targetBleed = 20;
        int offset = 35;
        string template;
        public Form1()
        {
            InitializeComponent();

            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.TransparencyKey = Color.Transparent;
            Location = new Point(60, 200);

            template = File.ReadAllLines("./kanna.txt").Aggregate((a, b) => a + '\n' + b);
            var templateSize = TextRenderer.MeasureText(template, new Font("Consolas", 5.25f, GraphicsUnit.Point));
            this.Size = new Size(templateSize.Width + TextRenderer.MeasureText(" ", new Font("Consolas", 5.25f, GraphicsUnit.Point)).Width * offset * targetBleed, templateSize.Height);
        }

        protected override void OnPaintBackground(PaintEventArgs e) { /* Ignore */ }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Task.Run(async () =>
            {
                var lastValue = "";
                for (var i = 0; i < targetBleed; i++)
                {
                    lastValue = BleedTemplate(lastValue, i * offset, template);

                    this.BeginInvoke(new Action(() =>
                    {
                        label1.Text = lastValue;
                    }));

                    await Task.Delay(50);
                }
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
