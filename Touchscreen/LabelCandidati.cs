using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VotoTouch
{
    public partial class LabelCandidati : UserControl
    {
        private string _TText = "";
        private bool label1_Visible = false, label2_Visible = false, label3_Visible = false;

        private ContentAlignment _TextAlign;
        public ContentAlignment TextAlign
        {
            get { return _TextAlign; }
            set
            {
                _TextAlign = value;
                SetCandidatiAligment(value);
            }
        }

        public LabelCandidati()
        {
            InitializeComponent();
        }

        [Browsable(true), EditorBrowsable(EditorBrowsableState.Always)]
        [Description("Test text displayed in the textbox"), Category("Data")]        
        public override string Text
        {
            get { return _TText; }
            set { SetCandidatiText(value); }
        }

        private string _TextNote;
        public string TextNote
        {
            get { return _TextNote; }
            set
            {
                _TextNote = value;
                SetFontNote();
                label_note.Text = value;
            }
        }

        private void SetCandidatiAligment(ContentAlignment ATextAlign)
        {
            label1.TextAlign = ATextAlign;
            label2.TextAlign = ATextAlign;
            label3.TextAlign = ATextAlign;
        }

        private void SetCandidatiText(string AText)
        {
            _TText = AText;
            char separator = ';';

            // ok ora devo verificare se ci sono dei separatori nel testo, possono essere ; o |
            // questi indicano se ci sono candidati e quindi colonne
            if (_TText.IndexOf(";", System.StringComparison.Ordinal) > 0)
                separator = ';';
            if (_TText.IndexOf("|", System.StringComparison.Ordinal) > 0)
                separator = '|';

            // ok ora trasformo la stringa in lista di strings
            List<string> ris = _TText.Split(separator).ToList();

            string txlab = "";
            label1.Text = "";
            label2.Text = "";
            label3.Text = "";
            //int ccc = 0;
            // ok ora so quanti ce ne sono
            if (ris.Count <= 6)
            {
                // 1 colonna sola
                label1_Visible = true;
                label2_Visible = false;
                label3_Visible = false;
                SpostaLabel();
                // ok ora il testo
                txlab = ris.Aggregate("", (current, ss) => current + (ss + "\n"));
                label1.Text = txlab;
                return;
            }
            if (ris.Count <= 12)
            {
                // 2 colonne
                label1_Visible = true;
                label2_Visible = true;
                label3_Visible = false;
                SpostaLabel();
                // devo dividere in due il count
                float cc = ris.Count/2;
                if (cc == Math.Truncate(cc))
                {
                    // non ha decimali
                }
                else
                {
                    cc++;
                }
                int po = 1;
                txlab = "";
                foreach (string ss in ris)
                {
                    txlab += ss + "\n";
                    if (po == (int)cc)
                    {
                        label1.Text = txlab;
                        txlab = "";
                    }
                    po++;
                }
                label2.Text = txlab;
                return;
            }
            if (ris.Count <= 18)
            {
                label1_Visible = true;
                label2_Visible = true;
                label3_Visible = true;
                SpostaLabel();
                // devo dividere in tre il count
                // devo dividere in due il count
                float cc = ris.Count / 3;
                if (cc == Math.Truncate(cc))
                {
                    // non ha decimali
                }
                else
                {
                    cc++;
                }
                int po = 1;
                txlab = "";
                foreach (string ss in ris)
                {
                    txlab += ss + "\n";
                    if (po == (int)cc)
                    {
                        label1.Text = txlab;
                        txlab = "";
                    }
                    if (po == ((int)cc * 2))
                    {
                        label2.Text = txlab;
                        txlab = "";
                    }

                    po++;
                }
                label3.Text = txlab;

            }


        }

        private void SetFontNote()
        {
            FontStyle fs = FontStyle.Italic;
            //if (Convert.ToBoolean(r["Bold"])) fs = FontStyle.Bold;
            label_note.Font = new Font(this.Font.Name, this.Font.Size - 2, fs);            
        }

        //private void LabelCandidati_Resize(object sender, EventArgs e)
        //{
        //    SpostaLabel();
        //}

        //Size last = new Size(0, 0);

        private void LabelCandidati_Resize(object sender, System.EventArgs e)
        {
            //if (last != new Size(0, 0))
            //{
            //    this.Parent.Size = Size.Add(this.Parent.Size, Size.Subtract(this.Size, last));
            //}
            //last = this.Size;
            SpostaLabel();
        }

        private void SpostaLabel()
        {
            //Debug.WriteLine(this.Size.Width);
            //Debug.WriteLine(this.Size.Height);
            int ww = this.Width;
            int hh = this.Height-40;
            // evento resize 1 sola label
            if (label1_Visible && !label2_Visible && !label3_Visible)
            {
                label1.SetBounds(0, 0, ww, hh);
            }
            // evento resize 2  label
            if (label1_Visible && label2_Visible && !label3_Visible)
            {
                label1.SetBounds(0, 0, (ww/2), hh);
                label2.SetBounds((ww / 2), 0, (ww/2), hh);
            }
            // evento resize 3  label
            if (label1_Visible && label2_Visible && label3_Visible)
            {
                label1.SetBounds(0, 0, (ww / 3), hh);
                label2.SetBounds((ww / 3), 0, (ww / 3), hh);
                label3.SetBounds((ww / 3)*2, 0, (ww / 3), hh);                
                //label1.Left = 0;
                //label1.Top = 0;
                //label1.Width = this.Size.Width / 3;
                //label1.Height = this.Height;
                //label2.Left = this.Size.Width / 3; ;
                //label2.Top = 0;
                //label2.Width = this.Size.Width / 3;
                //label2.Height = this.Size.Height;
                //label3.Left = (this.Size.Width / 3) * 2; ;
                //label3.Top = 0;
                //label3.Width = this.Size.Width / 3;
                //label3.Height = this.Size.Height;
            }
            label1.Visible = label1_Visible;
            label2.Visible = label2_Visible;
            label3.Visible = label3_Visible;
            // le note
            label_note.SetBounds(0, hh, ww, 40);
            label_note.Visible = true;

        }

        //[Description("Test text displayed in the textbox"), Category("Data")]
        //public string Text
        //{
        //    get { return label1.Text; }
        //    set { label1.Text = value; }
        //}
    }
}
