using ConvertService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace JsonConverter
{
    public partial class Form1 : Form
    {
        private string _targetDir;
        private List<ReplaceString> _replaceStrings;

        private static string _resultPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\result\";

        public Form1()
        {
            InitializeComponent();
            Directory.CreateDirectory(_resultPath);
            panel1.DragEnter += Panel1_DragEnter;
            panel1.DragDrop += Panel1_DragDrop;
        }

        private void Panel1_DragDrop(object sender, DragEventArgs e)
        {
            var input = ((string[])e.Data.GetData(DataFormats.FileDrop, false))[0];

            if (Directory.Exists(input))
            {
                _targetDir = input;
                DecompressFiles();
                var files = GetJsonFile(_targetDir);
                var dispStr = string.Join(Environment.NewLine, files);
                richTextBox1.Text = dispStr;
            }

            if (File.Exists(input))
            {
                _replaceStrings = ReadReplaceStr(input).ToList();

                var dispStrList = new List<string>();
                _replaceStrings.ForEach(str => 
                {
                    dispStrList.Add(str.DispString());
                });
                var dispStr = string.Join(Environment.NewLine, dispStrList);
                richTextBox2.Text = dispStr;
            }
        }

        private void Panel1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private string[] GetGzFileList(string targetDir)
        {
            var gzFileList = Directory.GetFiles(targetDir, "*.gz", SearchOption.AllDirectories);
            return gzFileList;
        }

        private string[] GetJsonFile(string targetDir)
        {
            var jsonFileList = Directory.GetFiles(targetDir, "*.json", SearchOption.AllDirectories);
            return jsonFileList;
        }

        private void buttonExecute_Click(object sender, EventArgs e)
        {
            DecompressFiles();

            foreach(var jsonPath in GetJsonFile(_targetDir))
            {
                string jsonText;
                string result;
                using (var sr = new StreamReader(jsonPath))
                {
                    jsonText = sr.ReadToEnd();
                    Converter.Convert(jsonText, _replaceStrings, out result);
                }

                var fileName = Path.GetFileName(jsonPath);
                var number = Regex.Match(fileName, @"\d+").Value;

                using (var sw = new StreamWriter(_resultPath + number + ".txt"))
                {
                    sw.Write(result);
                }
            }
        }

        private void DecompressFiles()
        {
            var gzFiles = GetGzFileList(_targetDir).ToList();

            gzFiles.ForEach(file =>
            {
                Utilities.Utilities.DecompressGz(new FileInfo(file));
            });
        }

        private IEnumerable<ReplaceString> ReadReplaceStr(string filePath)
        {
            using (var sr = new StreamReader(filePath))
            {
                string line;
                var replaceStrList = new List<ReplaceString>();
                while((line = sr.ReadLine()) != null)
                {
                    line = RemoveBlack(line);
                    var split = line.Split(',');
                    replaceStrList.Add(new ReplaceString(split[0], split[1]));
                }
                replaceStrList.Sort((a, b) => b.FromLen - a.FromLen);

                return replaceStrList;
            }
        }

        private string RemoveBlack(string target)
        {
            string removedStr;
            removedStr = Regex.Replace(target, @"\s+", "");
            return removedStr;
        }
    }
}
