using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BasicMinesweeper
{
    public partial class Form1 : Form
    {
        //Panel for game to be played on
        Panel gameArea = new Panel();

        //Arrays for cell data
        bool[] isBomb = new bool[81];
        bool[] uncovered = new bool[81];
        bool[] flagged = new bool[81];
        int[] guideValue = new int[81];

        int mines = 0;

        //Random number generator
        Random random = new Random();

        public Form1()
        {
            //Set attributes of gameArea
            gameArea.Parent = this;
            gameArea.Top = 60;
            gameArea.Left = 20;
            gameArea.Height = 270;
            gameArea.Width = 270;
            gameArea.MouseClick += gameArea_Click;

            InitializeComponent();
            gameArea.Show();
            Reset();

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            lblMines.Text = ("Mines: " + mines + "/10");

            //Get graphics for gameArea
            Graphics gr = gameArea.CreateGraphics();

            //Clear the panel to white
            gr.Clear(Color.White);


            for(int i = 0; i < isBomb.Length; i++)
            {
                //Convert from cell value to pixel values
                int x = (i % 9) * 30;
                int y = (i / 9) * 30;

                if (flagged[i])
                {
                    Brush br = new SolidBrush(Color.Yellow);
                    Rectangle rect = new Rectangle(x, y, 30, 30);
                    gr.FillRectangle(br, rect);
                    br.Dispose();
                }
                else if (!uncovered[i])
                {
                    //Draw a grey box if cell is covered
                    Brush br = new SolidBrush(Color.DarkSlateGray);
                    Rectangle rect = new Rectangle(x, y, 30, 30);            
                    gr.FillRectangle(br, rect);
                    br.Dispose();
                }
                else
                {
                    //Draw a B for a bomb and a guide number if not
                    string text;
                    if (isBomb[i])
                    {
                        text = "B";
                    }
                    else
                    {
                        text = guideValue[i].ToString();
                    }
                    Brush br = new SolidBrush(Color.Black);
                    Font font = new Font("Arial", 23);

                    gr.DrawString(text, font, br, x, y);
                    br.Dispose();
                    font.Dispose();
                }
                
            }
            gr.Dispose();
        }

        private void Reset()
        {
            //Reset all the arrays
            for (int i = 0; i < uncovered.Length; i++)
            {
                isBomb[i] = false;
                uncovered[i] = false;
                flagged[i] = false;
                guideValue[i] = 0;
                mines = 0;
            }

            //Assign 10 bombs
            int bombsAssigned = 0;
            while (bombsAssigned < 10)
            {
                int cell = random.Next(0, 81);
                if (!isBomb[cell])
                {
                    isBomb[cell] = true;
                    bombsAssigned++;
                }
            }

            //Assign guide numbers
            for (int i = 0; i < uncovered.Length; i++)
            {
                bool top = (i < 9);
                bool bottom = (i > 71);
                bool left = (i % 9 == 0);
                bool right = (i % 9 == 8);

                if (!top && !left && isBomb[i - 10])
                {
                    guideValue[i]++;
                }
                if (!top && isBomb[i - 9])
                {
                    guideValue[i]++;
                }
                if (!top && !right && isBomb[i - 8])
                {
                    guideValue[i]++;
                }
                if (!left && isBomb[i - 1])
                {
                    guideValue[i]++;
                }
                if (!right && isBomb[i + 1])
                {
                    guideValue[i]++;
                }
                if (!bottom && !left && isBomb[i + 8])
                {
                    guideValue[i]++;
                }
                if (!bottom && isBomb[i + 9])
                {
                    guideValue[i]++;
                }
                if (!bottom && !right && isBomb[i + 10])
                {
                    guideValue[i]++;
                }

                //Debug code to check minefields
                if (i % 9 == 8)
                {                    
                    if (isBomb[i])
                    {
                        Console.WriteLine("B");
                    }
                    else
                    {
                        Console.WriteLine(guideValue[i]);
                    }
                    
                }
                else
                {
                    if (isBomb[i])
                    {
                        Console.Write("B");
                    }
                    else
                    {
                        Console.Write(guideValue[i]);
                    }
                }
            }
            Form1_Paint(this, null);
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            Reset();
        }

        public void gameArea_Click(object sender, MouseEventArgs e)
        {
            //Get a cursor position and convert to cell data
            Point pos = gameArea.PointToClient(Cursor.Position);
            int posX = pos.X / 30;
            int posY = pos.Y / 30;
            int cell = (posY * 9) + posX;

            if (e.Button == MouseButtons.Right)
            {
                if (!uncovered[cell])
                {
                    flagged[cell] = true;
                    mines++;
                }
            }
            else if(e.Button == MouseButtons.Left)
            {
                //Make sure the mouse is clicked inside the game area
                if (cell < 0 || cell > 81)
                {
                    return;
                }
                //Tell the player and reset the game if they lose
                else if (isBomb[cell] && !flagged[cell])
                {
                    uncovered[cell] = true;
                    Form1_Paint(this, null);
                    MessageBox.Show("You lose");
                    Reset();
                }
                //otherwise uncover the clicked cell
                else if (!flagged[cell])
                {
                    Uncover(cell);
                }
                //If all non-bomb cells are uncovered the player wins
                if (CheckWin())
                {
                    MessageBox.Show("You win");
                    Reset();
                }
            }

            

            Form1_Paint(this, null);

            Console.WriteLine("{0}, {1}, {2}", posX, posY, cell);
        }

        public bool CheckWin()
        {
            //make sure each cell is either uncovered or a bomb
            for (int i = 0; i < uncovered.Length; i++)
            {
                if (!uncovered[i] && !isBomb[i])
                {
                    return false;
                }
            }
            return true;
        }

        public void Uncover(int cell)
        {
            uncovered[cell] = true;
            //If the cell is a 0, uncover all adjacent cells automatically
            if (guideValue[cell] == 0)
            {
                for (int i = 0; i < 9; i++)
                {
                    int modifier = (i / 3 - 1) * 9 + (i % 3 - 1);
                    int value = cell + modifier;
                    int rowDiff = Math.Abs(cell % 9 - value % 9);
                    if (value >= 0 && value < 81 && rowDiff < 2 && !uncovered[value])
                    {
                        Uncover(cell + modifier);
                    }
                }
            }
            
        }
    }
}
