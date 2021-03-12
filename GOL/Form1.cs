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
        bool[,] universe = new bool[30, 30];

        // Drawing colors
        Color gridColor = Color.LightGray;
        Color cellColor = Color.DarkMagenta;

        // The Timer class
        Timer timer = new Timer();

        // Generation count
        int generations = 0;

        //Number of living cells
        int cells = 0;

        public Form1()
        {
            InitializeComponent();

            // Setup the timer
            timer.Interval = 50; // milliseconds
            timer.Tick += Timer_Tick;
            timer.Enabled = false; // start timer not running
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
                    int count = CountNeighborsToroidal(x, y);

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

        //Counts the number of living neighbors of a given cell
        private int CountNeighborsToroidal(int x, int y)
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

                    //if past the universe bounds, wrap around to the other side
                    if (xCheck < 0) xCheck = xLen - 1;
                    if (yCheck < 0) yCheck = yLen - 1;
                    if (xCheck >= xLen) xCheck = 0;
                    if (yCheck >= yLen) yCheck = 0;

                    //add to count if given adjacent cell is alive
                    if (universe[xCheck, yCheck] == true) count++;
                }
            }
            return count;
        }

        //Called every time WM_PAINT message if received
        private void graphicsPanel1_Paint(object sender, PaintEventArgs e)
        {
            //reset living cell count
            cells = 0;

            // Calculate the width and height of each cell in pixels
            // CELL WIDTH = WINDOW WIDTH / NUMBER OF CELLS IN X
            float cellWidth = (float)(graphicsPanel1.ClientSize.Width-1) / universe.GetLength(0);
            // CELL HEIGHT = WINDOW HEIGHT / NUMBER OF CELLS IN Y
            float cellHeight = (float)(graphicsPanel1.ClientSize.Height-1) / universe.GetLength(1);

            // A Pen for drawing the grid lines (color, width)
            Pen gridPen = new Pen(gridColor, 1);

            // A Brush for filling living cells interiors (color)
            Brush cellBrush = new SolidBrush(cellColor);

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

                    // Fill the cell with a brush if alive and iterate living cell count
                    if (universe[x, y] == true)
                    {
                        e.Graphics.FillRectangle(cellBrush, cellRect);
                        cells++;
                    }

                    // Outline the cell with a pen
                    e.Graphics.DrawRectangle(gridPen, cellRect.X, cellRect.Y, cellRect.Width, cellRect.Height);
                }
            }

            //Update living cell count
            toolStripStatusLabelLivingCells.Text = "Living Cells = " + cells.ToString();

            // Cleaning up pens and brushes
            gridPen.Dispose();
            cellBrush.Dispose();
        }

        //Click event on the graphics panel
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

                //Iterate the cells count
                if (universe[x, y] == true) cells++;
                else cells--;

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

            graphicsPanel1.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
