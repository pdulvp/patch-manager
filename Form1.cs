using System.Diagnostics;
using System.Globalization;
using static System.Windows.Forms.ListViewItem;
namespace PatchManager
{

    public partial class Form1 : Form
    {
        private String currentPath;
        private String CurrentPath { get { return currentPath; } set { currentPath = value; OnPathChanged(); } }

        public event System.EventHandler PathChanged;
        protected virtual void OnPathChanged()
        {
            if (PathChanged != null) PathChanged(this, EventArgs.Empty);
        }



        private String executable;
        private String Executable { get { return executable; } set { executable = value; OnExecutableChanged(); } }

        private String outputExecutable;
        private String OutputExecutable { get { return outputExecutable; } set { outputExecutable = value; OnOutputExecutableChanged(); } }

        public event System.EventHandler OutputExecutableChanged;
        protected virtual void OnOutputExecutableChanged()
        {
            if (OutputExecutableChanged != null) OutputExecutableChanged(this, EventArgs.Empty);
        }

        public event System.EventHandler ExecutableChanged;
        protected virtual void OnExecutableChanged()
        {
            if (ExecutableChanged != null) ExecutableChanged(this, EventArgs.Empty);
        }


        private List<PatchFile> patches;
        public List<PatchFile> Patches { get { return patches; } set { patches = value; OnPatchesChanged(); } }

        public event System.EventHandler PatchesChanged;
        protected virtual void OnPatchesChanged()
        {
            if (PatchesChanged != null) PatchesChanged(this, EventArgs.Empty);
        }



        private List<PatchFile> selectedPatches;
        public List<PatchFile> SelectedPatches { get { return selectedPatches; } set { selectedPatches = value; OnSelectedPatchesChanged(); } }

        public event System.EventHandler SelectedPatchesChanged;
        protected virtual void OnSelectedPatchesChanged()
        {
            if (SelectedPatchesChanged != null) SelectedPatchesChanged(this, EventArgs.Empty);
        }


        private void reloadFiles()
        {
            List<PatchFile> patches = new List<PatchFile>();
            if (CurrentPath.Length > 0)
            {
                DirectoryInfo d = new DirectoryInfo(CurrentPath);
                foreach (FileInfo zipInfo in d.GetFiles("*.1337"))
                {
                    FileInfo jsonInfo = new FileInfo(zipInfo.FullName);
                    PatchFile skeleton = new PatchFile();
                    skeleton.FullName = jsonInfo.FullName;

                    patches.Add(skeleton);
                }
            }
            Patches = patches;
        }

        public Form1()
        {
            InitializeComponent();

            toolStripStatusLabel1.Text = "";
            toolStripStatusLabel3.Text = "";

            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.Columns.Add("Name", 500);
            listView1.Columns.Add("Size", 100);
            listView1.Columns.Add("Summary", 100);

            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.pictureBox10, "Reset");

            PathChanged += (object? sender, EventArgs e) =>
            {
                if (CurrentPath != null)
                {
                    reloadFiles();
                }
                linkLabel6.Visible = CurrentPath == null;
                pictureBox8.Visible = CurrentPath == null;
            };

            ExecutableChanged += (object? sender, EventArgs e) =>
            {
                pictureBox10.Visible = Executable != null;

                linkLabel1.Visible = Executable != null;
                linkLabel4.Visible = Executable != null;
                pictureBox5.Visible = Executable != null;
                pictureBox6.Visible = Executable != null;

                linkLabel5.Visible = Executable == null;
                pictureBox7.Visible = Executable == null;

                if (Executable != null)
                {
                    if (pictureBox1.Tag == null)
                    {
                        pictureBox1.Tag = pictureBox1.Image;
                    }
                    pictureBox1.Image = WinIcons.getLargeIcon(Executable);
                    label1.Text = Path.GetFileName(Executable);

                }
                else
                {
                    label1.Text = "Select an executable";
                    if (pictureBox1.Tag != null)
                    {
                        pictureBox1.Image = (Image)pictureBox1.Tag;
                    }
                }

            };

            System.EventHandler OutputExecutableChange = (object? sender, EventArgs e) =>
            {

                if (Executable != null && SelectedPatches != null && SelectedPatches.Count > 0)
                {
                    string value = "output";
                    foreach (PatchFile file in SelectedPatches)
                    {
                        value += file.Name;
                    }
                    OutputExecutable = Path.Combine(Path.GetDirectoryName(Executable), Path.GetFileNameWithoutExtension(Executable) + "_" + toSHA(value) + Path.GetExtension(Executable));
                }
                else
                {
                    OutputExecutable = null;
                };

                linkLabel7.Visible = SelectedPatches != null && SelectedPatches.Count > 0 && OutputExecutable != null;
                pictureBox9.Visible = SelectedPatches != null && SelectedPatches.Count > 0 && OutputExecutable != null;
            };

            ExecutableChanged += OutputExecutableChange;
            SelectedPatchesChanged += OutputExecutableChange;


            PatchesChanged += (object? sender, EventArgs e) =>
            {
                foreach (PatchFile file in Patches)
                {
                    ListViewItem item = new ListViewItem(file.Name, 0);
                    item.Tag = file;
                    item.SubItems.Add(new ListViewSubItem(item, "" + file.Length));
                    item.SubItems.Add(new ListViewSubItem(item, "" + file.Summary));
                    listView1.Items.Add(item);
                    file.SummaryChanged += (object? sender, EventArgs e) =>
                    {
                        item.SubItems[2].Text = file.Summary;
                    };
                    file.FullNameChanged += (object? sender, EventArgs e) =>
                    {
                        item.SubItems[0].Text = file.Name;
                    };
                }
            };

            SelectedPatchesChanged += (object? sender, EventArgs e) =>
            {
                label4.Visible = SelectedPatches.Count > 0;
                label4.Text = "" + SelectedPatches.Count;

                textBox1.Tag = null;
                textBox2.Tag = null;
                textBox1.Text = "";
                textBox2.Text = "";
                richTextBox1.Clear();
                if (SelectedPatches.Count == 1)
                {
                    textBox1.Text = Path.GetFileNameWithoutExtension(SelectedPatches[0].Name);
                    textBox1.Tag = SelectedPatches[0];
                    textBox2.Text = SelectedPatches[0].Summary;
                    textBox2.Tag = SelectedPatches[0];

                    richTextBox1.Tag = SelectedPatches[0];

                    richTextBox1.BackColor = Color.White;
                    richTextBox1.Clear();
                    RichTextBoxRedrawHandler rh = new RichTextBoxRedrawHandler(richTextBox1);
                    rh.SuspendPainting();
                    // do things with richTextBox

                    foreach (PatchItem item in SelectedPatches[0].Content)
                    {
                        richTextBox1.SelectedText = "   ";
                        richTextBox1.SelectionColor = Color.DarkBlue;
                        richTextBox1.SelectedText = item.Address.ToString("X8");
                        richTextBox1.SelectionColor = Color.Black;
                        richTextBox1.SelectedText = " = ";
                        richTextBox1.SelectionColor = Color.Orange;
                        richTextBox1.SelectedText = item.CurrentValue.ToString("X2");
                        richTextBox1.SelectionColor = Color.Black;
                        richTextBox1.SelectedText = " > ";
                        richTextBox1.SelectionColor = Color.Green;
                        richTextBox1.SelectedText = item.ChangedValue.ToString("X2");
                        richTextBox1.SelectedText = "\n";
                    }
                    rh.ResumePainting();

                }
            };

            OutputExecutableChanged += (object? sender, EventArgs e) =>
            {
                label3.Enabled = OutputExecutable != null;
                label3.Text = OutputExecutable == null ? "Destination executable" : Path.GetFileName(OutputExecutable);
                linkLabel2.Visible = File.Exists(OutputExecutable);
                linkLabel3.Visible = File.Exists(OutputExecutable);
                pictureBox3.Visible = File.Exists(OutputExecutable);
                pictureBox4.Visible = File.Exists(OutputExecutable);

                if (OutputExecutable != null)
                {
                    if (pictureBox2.Tag == null)
                    {
                        pictureBox2.Tag = pictureBox2.Image;
                    }
                    pictureBox2.Image = WinIcons.getLargeIcon(Executable);
                }
                else if (pictureBox2.Tag != null)
                {
                    pictureBox2.Image = (Image)pictureBox2.Tag;
                }
            };

            CurrentPath = null;
            Executable = null;
            OutputExecutable = null;
            SelectedPatches = new List<PatchFile>();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void openFolderPatchesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Desktop;
            folderBrowserDialog1.ShowDialog();
            CurrentPath = folderBrowserDialog1.SelectedPath;
        }

        private void chooseATargetExecutableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = Environment.SpecialFolder.Desktop.ToString();
            openFileDialog1.Filter = "Executable (*.exe)|*.exe";
            openFileDialog1.Multiselect = false;
            openFileDialog1.ShowDialog();
            Executable = openFileDialog1.FileName;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label4_Paint(object sender, PaintEventArgs e)
        {
            //Color color = Color.FromArgb(0xFF, 255, 0, 0);
            //SolidBrush brush = new SolidBrush(color);
            //Rectangle rect = new Rectangle(0, 0, 20, 20);
            //e.Graphics.FillEllipse(brush, rect);
            //e.Graphics.DrawEllipse(new Pen(new SolidBrush(Color.FromArgb(0xFF, 200, 0, 0))), rect);
            // e.Graphics.DrawString("10", label4.Font, new SolidBrush(Color.White), new PointF(0, 0));
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {

            List<PatchFile> result = new List<PatchFile>();
            foreach (ListViewItem i in listView1.SelectedItems)
            {
                result.Add((PatchFile)i.Tag);
            }
            SelectedPatches = result;
        }

        private String toSHA(string value)
        {
            System.Security.Cryptography.SHA1 sha = System.Security.Cryptography.SHA1.Create();
            String hash = String.Empty;

            foreach (byte b in sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value)))
            {
                hash += b.ToString("x2").ToLower();
            }
            return hash.Substring(0, 8).ToUpper();
        }

        private void listView1_BeforeLabelEdit(object sender, LabelEditEventArgs e)
        {

        }

        private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Tag != null)
            {
                PatchFile file = (PatchFile)textBox1.Tag;
                file.FullName = Path.Combine(Path.GetDirectoryName(file.FullName), textBox1.Text + ".1337");
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (textBox2.Tag != null)
            {
                ((PatchFile)textBox2.Tag).Summary = textBox2.Text;
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = Path.GetDirectoryName(Executable),
                FileName = "explorer.exe"
            };
            Process.Start(startInfo);
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                Arguments = Path.GetDirectoryName(OutputExecutable),
                FileName = "explorer.exe"
            };
            Process.Start(startInfo);
        }

        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ProcessStartInfo StartInfo = new ProcessStartInfo(Executable)
            {
                WindowStyle = ProcessWindowStyle.Normal,
                WorkingDirectory = Path.GetDirectoryName(Executable)
            };
            try
            {
                Process.Start(StartInfo);
            }
            catch (Exception ee)
            {
                toolStripStatusLabel1.Text = ee.Message;
            }
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            ProcessStartInfo StartInfo = new ProcessStartInfo(OutputExecutable)
            {
                WindowStyle = ProcessWindowStyle.Normal,
                WorkingDirectory = Path.GetDirectoryName(OutputExecutable)
            };
            try
            {
                Process.Start(StartInfo);
            }
            catch (Exception ee)
            {
                toolStripStatusLabel1.Text = ee.Message;
            }
        }

        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            chooseATargetExecutableToolStripMenuItem_Click(sender, e);
        }

        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            openFolderPatchesToolStripMenuItem_Click(sender, e);
        }

        private void pictureBox3_Click_1(object sender, EventArgs e)
        {

        }

        private void linkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            bool ok = true;
            byte[] binary = File.ReadAllBytes(Executable);
            try
            {
                foreach (PatchFile patch in SelectedPatches)
                {
                    foreach (PatchItem item in patch.Content)
                    {
                        binary[item.Address] = item.ChangedValue;
                    }
                }
                if (File.Exists(OutputExecutable))
                {
                    File.Delete(OutputExecutable);
                }
                File.WriteAllBytes(OutputExecutable, binary);
                OnOutputExecutableChanged();
                toolStripStatusLabel3.Text = "OK";
                toolStripStatusLabel1.Text = "";
                var t = Task.Run(async delegate
                {
                    await Task.Delay(3000);
                    toolStripStatusLabel3.Text = "";
                });
            }
            catch (Exception ex)
            {
                toolStripStatusLabel1.Text = "ERROR: " + ex.Message;
                toolStripStatusLabel3.Text = "";
                var t = Task.Run(async delegate
                {
                    await Task.Delay(3000);
                    toolStripStatusLabel1.Text = "";
                });
            }

        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            Executable = null;
        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void toolStripStatusLabel1_Click(object sender, EventArgs e)
        {

        }

    }

    public class PatchItem
    {
        int address;

        public int Address
        {
            get
            {
                return address;
            }
        }

        byte currentValue;

        public byte CurrentValue
        {
            get
            {
                return currentValue;
            }
        }

        byte changedValue;

        public byte ChangedValue
        {
            get
            {
                return changedValue;
            }
        }

        public String Value
        {
            set
            {
                string[] val = value.Split(":");
                address = int.Parse(val[0], NumberStyles.HexNumber) - 0xC00;
                val = val[1].Replace("->", ":").Split(":");
                currentValue = byte.Parse(val[0], NumberStyles.HexNumber);
                changedValue = byte.Parse(val[1], NumberStyles.HexNumber);
            }
        }
    }

    public class PatchFile
    {
        public string Name
        {
            get
            {
                return Path.GetFileName(FullName);
            }
        }

        private string fullName;

        public string FullName
        {
            get
            {
                return fullName;
            }
            set
            {
                if (value.Length > 0)
                {
                    if (fullName != null)
                    {
                        File.Move(fullName, value);

                        string mdFile = Path.ChangeExtension(fullName, ".md");
                        if (File.Exists(mdFile))
                        {
                            string newMdFile = Path.ChangeExtension(value, ".md");
                            File.Move(mdFile, newMdFile);
                        }
                    }

                    fullName = value;
                    OnFullNameChanged();
                }
            }
        }

        public event System.EventHandler FullNameChanged;
        protected virtual void OnFullNameChanged()
        {
            if (FullNameChanged != null) FullNameChanged(this, EventArgs.Empty);
        }

        public long Length
        {
            get
            {
                return Content.Count;
            }
        }

        public List<PatchItem> Content
        {
            get
            {
                List<PatchItem> result = new List<PatchItem>();
                string[] lines = File.ReadAllLines(FullName);
                for (int i = 1; i < lines.Length; i++)
                {
                    PatchItem item = new PatchItem();
                    item.Value = lines[i];
                    result.Add(item);
                }
                return result;
            }
        }

        private string summary;
        public string Summary
        {
            get
            {
                if (summary == null)
                {
                    string mdFile = Path.ChangeExtension(FullName, ".md");
                    if (File.Exists(mdFile))
                    {
                        string[] lines = File.ReadAllLines(mdFile);
                        summary = lines[0];
                    }
                    else
                    {
                        summary = "";
                    }
                }
                return summary;
            }

            set
            {
                string mdFile = Path.ChangeExtension(FullName, ".md");
                File.WriteAllLines(mdFile, value.Split("\n"));
                summary = null;
                OnSummaryChanged();
            }
        }

        public event System.EventHandler SummaryChanged;
        protected virtual void OnSummaryChanged()
        {
            if (SummaryChanged != null) SummaryChanged(this, EventArgs.Empty);
        }

    }

}