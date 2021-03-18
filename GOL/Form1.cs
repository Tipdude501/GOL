using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GOL
{
    public partial class Form1 : Form
    {
         // The universe array
        bool[,] universe;
        
        // Drawing colors
        Color gridColor;
        Color cellColor;
        Color backColor;

        // The Timer class
        Timer timer = new Timer();

        // Status strip items
        int generations = 0;

        //Current seed
        int seed = 0;

        //View menu items
        bool showGrid = true;
        bool showNeighborCount = true;
        bool showHUD = true;
        bool isToroidal = true;


        public Form1()
        {
            InitializeComponent();

            //initialise settings
            gridColor = Properties.Settings.Default.GridColor;
            cellColor = Properties.Settings.Default.CellColor;
            backColor = Properties.Settings.Default.BackColor;

            // Setup the timer
            timer.Interval = Properties.Settings.Default.Interval;
            timer.Tick += Timer_Tick;
            timer.Enabled = false; // start timer not running

            //initialize universe
            universe = new bool[Properties.Settings.Default.Width, Properties.Settings.Default.Height];
        }

        // Calculate the next generation of cells
        private void NextGeneration()
        {
            //Scratchpad array to draw next generation on
            bool[,] scratchpad = new bool[universe.GetLength(0), universe.GetLength(1)];

            // Iterate through the universe in the y, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    //get neighbor count
                    int count = CountNeighbors(x, y);

                    // apply the rules of life
                    if (count < 2 || count > 3) scratchpad[x, y] = false;
                    if (universe[x, y] == true && (count == 3 || count == 2)) scratchpad[x, y] = true;
                    if (universe[x, y] == false && count == 3) scratchpad[x, y] = true;
                }
            }

            //copy scratchpad over to universe
            universe = scratchpad;

            //iterate generations
            generations++;
            toolStripStatusLabelGenerations.Text = "Generations = " + generations.ToString();
            
            graphicsPanel1.Invalidate();
        }

        // The event called by the timer every Interval milliseconds.
        private void Timer_Tick(object sender, EventArgs e)
        {
            NextGeneration();
        }

        //Counts the number of living neighbors of a given cell, treating the esges like they wrap around
        private int CountNeighbors(int x, int y)
        {
            //Neigbor count
            int count = 0;
            //Universe bounds
            int xLen = universe.GetLength(0);
            int yLen = universe.GetLength(1);

            //Iterate through the adjacent cells
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    //getting coordinates for given adjacent cell
                    int xCheck = x + xOffset;
                    int yCheck = y + yOffset;

                    //ignore if this is the origianl cell
                    if (xOffset == 0 && yOffset == 0) continue;

                    if (isToroidal)
                    {
                        //toroidal boundary behavior
                        if (xCheck < 0) xCheck = xLen - 1;
                        if (yCheck < 0) yCheck = yLen - 1;
                        if (xCheck >= xLen) xCheck = 0;
                        if (yCheck >= yLen) yCheck = 0;
                    }
                    else
                    {
                        //finite boundary behavior
                        if (xCheck < 0) continue;
                        if (yCheck < 0) continue;
                        if (xCheck >= xLen) continue;
                        if (yCheck >= yLen) continue;
                    }

                    //add to count if given adjacent cell is alive
                    if (universe[xCheck, yCheck] == true) count++;
                }
            }
            return count;
        }

        //paint event
        private void graphicsPanel1_Paint(object sender, PaintEventArgs e)
        {
            // Calculate the width and height of each cell in pixels
            float cellWidth = (float)(graphicsPanel1.ClientSize.Width-1) / universe.GetLength(0);
            float cellHeight = (float)(graphicsPanel1.ClientSize.Height-1) / universe.GetLength(1);

            // Graphics objects
            Pen gridPen = new Pen(gridColor, 1);
            Brush cellBrush = new SolidBrush(cellColor);
            Brush backBrush = new SolidBrush(backColor);

            // Iterate through the universe in the y, top to bottom
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                // Iterate through the universe in the x, left to right
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    // A rectangle to represent each cell in pixels
                    RectangleF cellRect = RectangleF.Empty;
                    cellRect.X = x * cellWidth;
                    cellRect.Y = y * cellHeight;
                    cellRect.Width = cellWidth;
                    cellRect.Height = cellHeight;

                    // Fill the cell with a brush if alive
                    if (universe[x, y] == true)
                    {
                        e.Graphics.FillRectangle(cellBrush, cellRect);
                    }
                    else
                    {
                        e.Graphics.FillRectangle(backBrush, cellRect);
                    }


                    //show neighbor count
                    if (showNeighborCount && CountNeighbors(x, y) > 0)
                    {
                        //calculate font size based on cell size
                        float fontSize = cellHeight - 10;
                        if (fontSize < 7)
                        {
                            fontSize = cellHeight - 5;
                        }
                        if (fontSize < 7)
                        {
                            fontSize = cellHeight - 2;
                        }
                        if (fontSize < 7)
                        {
                            fontSize = cellHeight;
                        }

                        //font for neighbor count
                        Font font = new Font("Arial", fontSize);

                        //format text in the center of the cell
                        StringFormat stringFormat = new StringFormat();
                        stringFormat.Alignment = StringAlignment.Center;
                        stringFormat.LineAlignment = StringAlignment.Center;

                        //neighbor count
                        e.Graphics.DrawString(CountNeighbors(x, y).ToString(), font, Brushes.Black, cellRect, stringFormat);
                    }


                    // Draw grid
                    if(showGrid) e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                }
            }

            //Draw HUD
            if (showHUD)
            {
                Font font = new Font("Arial", 15f);

                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Near;
                stringFormat.LineAlignment = StringAlignment.Far;

                e.Graphics.DrawString(GetHUDMessage(), font, Brushes.Black, graphicsPanel1.ClientRectangle, stringFormat);
            }


            //Update living cell count
            toolStripStatusLabelLivingCells.Text = "Living Cells = " + GetCellCount();

            // Cleaning up pens and brushes
            gridPen.Dispose();
            cellBrush.Dispose();
        }
        
        //Randomly fills the universe with living cells using the value of seed as its seed
        private void FillUniverseRandom()
        {
            Random r = new Random(seed);
            bool[,] scratchpad = new bool[universe.GetLength(0), universe.GetLength(1)];

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    //randomaly wake the cell
                    int state = r.Next(0, 2);
                    if (state == 0) scratchpad[x, y] = true;
                    else scratchpad[x, y] = false;
                }
            }
            universe = scratchpad;

            graphicsPanel1.Invalidate();
        }

        //returns the current number of living cells in the universe
        private int GetCellCount()
        {
            int count = 0;

            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    if (universe[x, y] == true)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        private string GetHUDMessage()
        {
            string HUD = string.Empty;

            HUD += "Generation: " + generations + "\n";
            HUD += "Cell count: " + GetCellCount() + "\n";

            HUD += "Boundary Behavior: ";
            if (isToroidal) HUD += "Toroidal\n";
            else HUD += "Finite\n";

            HUD += "Universe size: {Width=" + universe.GetLength(0) + ", Length=" + universe.GetLength(1) + "}\n";

            return HUD;
        }
        
        #region Click Events
        //graphics panel click event
        private void graphicsPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            // If the left mouse button was clicked
            if (e.Button == MouseButtons.Left)
            {
                //calculate cell sizes
                float cellWidth = (float)(graphicsPanel1.ClientSize.Width - 1) / universe.GetLength(0);
                float cellHeight = (float)(graphicsPanel1.ClientSize.Height - 1) / universe.GetLength(1);


                // Calculate the cell that was clicked in
                // CELL X = MOUSE X / CELL WIDTH
                int x = (int)(e.X / cellWidth);
                // CELL Y = MOUSE Y / CELL HEIGHT
                int y = (int)(e.Y / cellHeight);

                // Toggle the cell's state
                universe[x, y] = !universe[x, y];

                //invalidate
                graphicsPanel1.Invalidate();
            }
        }

        //Exit menu item click event
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //start tool strip button click event
        private void startToolStripButton_Click(object sender, EventArgs e)
        {
            timer.Enabled = true;
        }

        //pause tool strip button click event
        private void pauseToolStripButton_Click(object sender, EventArgs e)
        {
            timer.Enabled = false;
        }

        //next tool strip button click event
        private void nextToolStripButton_Click(object sender, EventArgs e)
        {
            NextGeneration();
        }

        //new toolstrip button click event
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //set all the cells in the universe to false
            for (int y = 0; y < universe.GetLength(1); y++)
            {
                for (int x = 0; x < universe.GetLength(0); x++)
                {
                    universe[x, y] = false;
                }
            }

            //reset generations count
            generations = 0;

            graphicsPanel1.Invalidate();
        }

        //seed from current time click event
        private void seedFromCurrentTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            seed = (int)DateTime.Now.Ticks;
            FillUniverseRandom();
        }

        //seed from custom seed click event
        private void fromSeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SeedDialog s = new SeedDialog();
            s.Seed = seed;

            if (DialogResult.OK == s.ShowDialog())
            {
                seed = s.Seed;
                graphicsPanel1.Invalidate();
                FillUniverseRandom();
            }
        }

        //option to change background color
        private void backColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog c = new ColorDialog();
            c.Color = backColor;

            if (DialogResult.OK == c.ShowDialog())
            {
                backColor = c.Color;
                graphicsPanel1.Invalidate();
            }
        }

        //option to change grid color
        private void lineColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog c = new ColorDialog();
            c.Color = gridColor;

            if (DialogResult.OK == c.ShowDialog())
            {
                gridColor = c.Color;
                graphicsPanel1.Invalidate();
            }
        }

        //option to change cell color
        private void cellColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog c = new ColorDialog();
            c.Color = cellColor;

            if (DialogResult.OK == c.ShowDialog())
            {
                cellColor = c.Color;
                graphicsPanel1.Invalidate();
            }
        }
        
        //open options modal
        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsModal o = new OptionsModal();
            o.Interval = timer.Interval;
            o.Width = universe.GetLength(0);
            o.Height = universe.GetLength(1);

            if (DialogResult.OK == o.ShowDialog())
            {
                //get time interval
                timer.Interval = o.Interval;

                //get universe size
                if (o.Width != universe.GetLength(0) || o.Height != universe.GetLength(1))
                {
                    universe = new bool[o.Width, o.Height];
                }

                graphicsPanel1.Invalidate();
            }
        }

        //reload previous settings
        private void reloadPreviousToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reload();

            //reassign color
            gridColor = Properties.Settings.Default.GridColor;
            cellColor = Properties.Settings.Default.CellColor;
            backColor = Properties.Settings.Default.BackColor;

            //reassign time interval
            timer.Interval = Properties.Settings.Default.Interval;

            //reassign universe size
            if (universe.GetLength(0) != Properties.Settings.Default.Width || universe.GetLength(1) != Properties.Settings.Default.Height)
            {
                universe = new bool[Properties.Settings.Default.Width, Properties.Settings.Default.Height];
            }

            graphicsPanel1.Invalidate();
        }

        //reset default settings
        private void resetToDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Reset();

            //reassign color
            gridColor = Properties.Settings.Default.GridColor;
            cellColor = Properties.Settings.Default.CellColor;
            backColor = Properties.Settings.Default.BackColor;

            //reassign time interval
            timer.Interval = Properties.Settings.Default.Interval;

            //reassign universe size
            if (universe.GetLength(0) != Properties.Settings.Default.Width || universe.GetLength(1) != Properties.Settings.Default.Height)
            {
                universe = new bool[Properties.Settings.Default.Width, Properties.Settings.Default.Height];
            }

            graphicsPanel1.Invalidate();
        }

        //toggle boundary behavior
        private void toggleBoundaryBehavior_Click(object sender, EventArgs e)
        {
            isToroidal = !isToroidal;
            toroidalToolStripMenuItem.Checked = !toroidalToolStripMenuItem.Checked;
            finiteToolStripMenuItem.Checked = !finiteToolStripMenuItem.Checked;
            graphicsPanel1.Invalidate();
        }

        //toggle view count nieghbor
        private void neighborCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showNeighborCount = !showNeighborCount;
            neighborCountToolStripMenuItem.Checked = !neighborCountToolStripMenuItem.Checked;
            graphicsPanel1.Invalidate();
        }
        
        //toggle view grid
        private void gridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showGrid = !showGrid;
            gridToolStripMenuItem.Checked = !gridToolStripMenuItem.Checked;
            graphicsPanel1.Invalidate();
        }
        
        //toggle view HUD
        private void hUDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showHUD = !showHUD;
            hUDToolStripMenuItem.Checked = !hUDToolStripMenuItem.Checked;
            graphicsPanel1.Invalidate();
        }
        #endregion

        //form closed event
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //update settings
            Properties.Settings.Default.GridColor = gridColor;
            Properties.Settings.Default.CellColor = cellColor;
            Properties.Settings.Default.BackColor = backColor;
            Properties.Settings.Default.Interval = timer.Interval;
            Properties.Settings.Default.Width = universe.GetLength(0);
            Properties.Settings.Default.Height = universe.GetLength(1);

            //save settings
            Properties.Settings.Default.Save();
        }  
    }
}
