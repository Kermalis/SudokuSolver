namespace Kermalis.SudokuSolver.UI
{
    internal sealed partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer _components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (_components != null))
            {
                _components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this._splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._sudokuBoard = new Kermalis.SudokuSolver.UI.SudokuBoard();
            this._logList = new System.Windows.Forms.ListBox();
            this._solveButton = new System.Windows.Forms.Button();
            this._menuStrip1 = new System.Windows.Forms.MenuStrip();
            this._fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this._statusStrip1 = new System.Windows.Forms.StatusStrip();
            this._puzzleLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this._statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer1)).BeginInit();
            this._splitContainer1.Panel1.SuspendLayout();
            this._splitContainer1.Panel2.SuspendLayout();
            this._splitContainer1.SuspendLayout();
            this._menuStrip1.SuspendLayout();
            this._statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this._splitContainer1.IsSplitterFixed = true;
            this._splitContainer1.Location = new System.Drawing.Point(35, 53);
            this._splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this._splitContainer1.Panel1.Controls.Add(this._sudokuBoard);
            // 
            // splitContainer1.Panel2
            // 
            this._splitContainer1.Panel2.Controls.Add(this._logList);
            this._splitContainer1.Size = new System.Drawing.Size(941, 470);
            this._splitContainer1.SplitterDistance = 470;
            this._splitContainer1.SplitterWidth = 1;
            this._splitContainer1.TabIndex = 2;
            // 
            // sudokuBoard
            // 
            this._sudokuBoard.Cursor = System.Windows.Forms.Cursors.Hand;
            this._sudokuBoard.Dock = System.Windows.Forms.DockStyle.Fill;
            this._sudokuBoard.Font = new System.Drawing.Font("Leelawadee", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._sudokuBoard.Location = new System.Drawing.Point(0, 0);
            this._sudokuBoard.Margin = new System.Windows.Forms.Padding(0);
            this._sudokuBoard.Name = "sudokuBoard";
            this._sudokuBoard.Size = new System.Drawing.Size(470, 470);
            this._sudokuBoard.TabIndex = 0;
            // 
            // logList
            // 
            this._logList.Dock = System.Windows.Forms.DockStyle.Bottom;
            this._logList.Font = new System.Drawing.Font("Meiryo", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._logList.FormattingEnabled = true;
            this._logList.HorizontalScrollbar = true;
            this._logList.ItemHeight = 17;
            this._logList.Location = new System.Drawing.Point(0, 7);
            this._logList.Name = "logList";
            this._logList.Size = new System.Drawing.Size(470, 463);
            this._logList.TabIndex = 0;
            // 
            // solveButton
            // 
            this._solveButton.Enabled = false;
            this._solveButton.Location = new System.Drawing.Point(448, 27);
            this._solveButton.Name = "solveButton";
            this._solveButton.Size = new System.Drawing.Size(75, 23);
            this._solveButton.TabIndex = 1;
            this._solveButton.Text = "Solve";
            this._solveButton.UseVisualStyleBackColor = true;
            this._solveButton.Click += new System.EventHandler(this.SolvePuzzle);
            // 
            // menuStrip1
            // 
            this._menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._fileToolStripMenuItem});
            this._menuStrip1.Location = new System.Drawing.Point(0, 0);
            this._menuStrip1.Name = "menuStrip1";
            this._menuStrip1.Size = new System.Drawing.Size(1011, 24);
            this._menuStrip1.TabIndex = 3;
            this._menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this._fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._newToolStripMenuItem,
            this._openToolStripMenuItem,
            this._saveAsToolStripMenuItem,
            this._exitToolStripMenuItem});
            this._fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this._fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this._fileToolStripMenuItem.Text = "File";
            // 
            // newToolStripMenuItem
            // 
            this._newToolStripMenuItem.Name = "newToolStripMenuItem";
            this._newToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this._newToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this._newToolStripMenuItem.Text = "New";
            this._newToolStripMenuItem.Click += new System.EventHandler(this.NewPuzzle);
            // 
            // openToolStripMenuItem
            // 
            this._openToolStripMenuItem.Name = "openToolStripMenuItem";
            this._openToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this._openToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this._openToolStripMenuItem.Text = "Open";
            this._openToolStripMenuItem.Click += new System.EventHandler(this.OpenPuzzle);
            // 
            // saveAsToolStripMenuItem
            // 
            this._saveAsToolStripMenuItem.Enabled = false;
            this._saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this._saveAsToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this._saveAsToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this._saveAsToolStripMenuItem.Text = "Save As";
            this._saveAsToolStripMenuItem.Click += new System.EventHandler(this.SavePuzzle);
            // 
            // exitToolStripMenuItem
            // 
            this._exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this._exitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
            this._exitToolStripMenuItem.Size = new System.Drawing.Size(154, 22);
            this._exitToolStripMenuItem.Text = "Exit";
            this._exitToolStripMenuItem.Click += new System.EventHandler(this.Exit);
            // 
            // statusStrip1
            // 
            this._statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._puzzleLabel,
            this._statusLabel});
            this._statusStrip1.Location = new System.Drawing.Point(0, 552);
            this._statusStrip1.Name = "statusStrip1";
            this._statusStrip1.Size = new System.Drawing.Size(1011, 22);
            this._statusStrip1.TabIndex = 0;
            this._statusStrip1.Text = "statusStrip1";
            // 
            // puzzleLabel
            // 
            this._puzzleLabel.Name = "puzzleLabel";
            this._puzzleLabel.Size = new System.Drawing.Size(68, 17);
            this._puzzleLabel.Text = "puzzleLabel";
            // 
            // statusLabel
            // 
            this._statusLabel.Name = "statusLabel";
            this._statusLabel.Size = new System.Drawing.Size(66, 17);
            this._statusLabel.Text = "statusLabel";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1011, 574);
            this.Controls.Add(this._statusStrip1);
            this.Controls.Add(this._solveButton);
            this.Controls.Add(this._splitContainer1);
            this.Controls.Add(this._menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this._menuStrip1;
            this.Name = "MainWindow";
            this.Text = "Sudoku Solver";
            this._splitContainer1.Panel1.ResumeLayout(false);
            this._splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer1)).EndInit();
            this._splitContainer1.ResumeLayout(false);
            this._menuStrip1.ResumeLayout(false);
            this._menuStrip1.PerformLayout();
            this._statusStrip1.ResumeLayout(false);
            this._statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer _splitContainer1;
        private SudokuSolver.UI.SudokuBoard _sudokuBoard;
        private System.Windows.Forms.Button _solveButton;
        private System.Windows.Forms.MenuStrip _menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem _fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _openToolStripMenuItem;
        private System.Windows.Forms.StatusStrip _statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel _statusLabel;
        private System.Windows.Forms.ListBox _logList;
        private System.Windows.Forms.ToolStripStatusLabel _puzzleLabel;
        private System.Windows.Forms.ToolStripMenuItem _newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem _exitToolStripMenuItem;
    }
}

