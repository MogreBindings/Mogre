using System;
using System.Xml;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AutoWrap.Meta;

namespace AutoWrap
{
    public partial class AutoWrap : Form
    {
        private readonly Wrapper _wrapper;
        private bool _showHeaderFiles = true;

        public AutoWrap(Wrapper wrapper)
        {
            InitializeComponent();

            _wrapper = wrapper;
            wrapper.IncludeFileWrapped += new EventHandler<IncludeFileWrapEventArgs>(IncludeFileWrapped);

            for (int i = 0; i < _wrapper.IncludeFiles.Count; i++)
            {
                _inputFilesList.Items.Add(_wrapper.IncludeFiles.Keys[i]);
            }
        }

        private void GenerateButtonClicked(object sender, EventArgs e)
        {
            bar.Visible = true;
            bar.Minimum = 0;
            bar.Maximum = _wrapper.IncludeFiles.Count;
            bar.Step = 1;
            bar.Value = 0;

            // Generate the C++/CLI source files
            _wrapper.GenerateCodeFiles();
            MessageBox.Show("Generation complete.");

            bar.Visible = false;
        }

        void IncludeFileWrapped(object sender, IncludeFileWrapEventArgs e)
        {
            bar.Value++;
            bar.Refresh();
        }

        private void ToggleButtonClicked(object sender, EventArgs e)
        {
            _showHeaderFiles = !this._showHeaderFiles;
            if (_showHeaderFiles)
                _showToggleButton.Text = "Show CPP File";
            else
                _showToggleButton.Text = "Show Header File";
            ShowCurrentFile();
        }

        private void lstTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowCurrentFile();
        }

        private void ShowCurrentFile() 
        {
          if (_inputFilesList.SelectedItem == null)
              return;

          if (_showHeaderFiles)
              _sourceCodeField.Text = _wrapper.CreateIncludeCodeForIncludeFile(_inputFilesList.SelectedItem.ToString()).Replace("\n", "\r\n");
          else 
          {
              bool hasContent;
              _sourceCodeField.Text = _wrapper.CreateCppCodeForIncludeFile(_inputFilesList.SelectedItem.ToString(), out hasContent).Replace("\n", "\r\n");
          }
        }
    }
}