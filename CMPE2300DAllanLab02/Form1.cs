/****************************************
*Program:   Lab 02 - Tetr-i-matic       *
*Author:    Dillon Allan                *
****************************************/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using GDIDrawer;

namespace CMPE2300DAllanLab02
{
    //Main form
    public partial class Form1 : Form
    {
        //Members
        bool _bW = false;                           //flag for W key state
        bool _bS = false;                           //S key state
        bool _bA = false;                           //A key state
        bool _bD = false;                           //D key state
        CDrawer _infoBox;                           //displays game score and upcoming shape.
        const int _iEasy = 750;                     //easy = 1000ms drop speed
        const int _iMedium = 500;                   //500ms 
        const int _iHard = 300;                     //300ms
        const int margin = 20;                      //x offset in pixels; used for positioning windows
                                                    //around the main game screen without overlaps.
        int _iDifficulty = 0;                       //game difficulty.
        int _iScore = 0;                            //Score based on number of cleared rows.
        List<Block[]> _lGameFloor;                  //Contains all fallen blocks in game.
        Point _pMouseCoords = new Point();          //Mouse click location for l- and r-clicks
        Queue<Shape> _qCurrentLevelShapes;          //Contains all shapes in current game level
        Shape _CurrentShape = null;                 //Current shape in play i.e. the falling shape
        Stopwatch _swGameWatch = new Stopwatch();   //timer measures stopwatch elapsed time for game progress.

        //Sets the initial position of the form and sets timer properties
        public Form1()
        {
            InitializeComponent();
            _tGameTimer.Interval = 50;  //50ms tick
            this.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width / 2,   //game menu starts center screen.
                Screen.PrimaryScreen.WorkingArea.Height / 2); 
        }

        //Starts a new game. Creates game window and info box.
        //side effects: repositions windows around game window, 
        //              and resets difficulty, score, and timers.
        private void btn_NewGame_Click(object sender, EventArgs e)
        {
            Block.canvas = new CDrawer(500, 1000);      //canvas is 10 blocks wide by 20 blocks high.
            _infoBox = new CDrawer(200, 300);           //infobox is 4 blocks wide by 6 blocks high.
            _infoBox.BBColour = BackColor;              //infobox background color same as form
            Block.canvas.Scale = _infoBox.Scale = 50;   //both CDrawers get same scale to depict shapes identically.
            
            //Center game window, with the form on the right, and the info box on the left.
            Block.canvas.Position = new Point(Screen.PrimaryScreen.WorkingArea.Width / 2 - Block.canvas.m_ciWidth / 2,
                Screen.PrimaryScreen.WorkingArea.Height / 2 - Block.canvas.m_ciHeight / 2);
            this.Location = new Point(Block.canvas.Position.X + Block.canvas.m_ciWidth + margin, 
                Block.canvas.Position.Y);
            _infoBox.Position = new Point(Block.canvas.Position.X - _infoBox.m_ciWidth - margin, 
                Block.canvas.Position.Y);

            _iDifficulty = 0;               //reset game difficulty.
            _iScore = 0;                    //reset game score.
            _lGameFloor = Block.NewFloor(); //populate floor with empty rows waiting to be filled.
            NextLevel();                    //prepare level 1 shapes and difficulty settings
            Activate();                     //return focus to main form for key controls.
            _swGameWatch.Start();           //start tracking time elapsed.
            _tGameTimer.Enabled = true;     //start ticking.
        }

        //Triggers game key controls
        //side effects: sets direction key flags to true on key down.
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (Block.canvas == null) return;   //key controls disabled until game begins.
            switch (e.KeyCode)
            {
                case Keys.S:                    //drop fast
                    _bS = true;
                    break;
                case Keys.A:                    //shift left
                    _bA = true;
                    break;
                case Keys.D:                    //shift right
                    _bD = true;
                    break;
                case Keys.W:                    //rotate
                    _bW = true;
                    break;
                case Keys.P:                    //toggle pause and resume functions.
                    if (_swGameWatch.IsRunning) 
                        Pause();
                    else
                        Resume();
                    break;
                case Keys.Q:                    //quit and exit game
                    Application.Exit();
                    break;
            }
        }

        //Runs every 50ms when timer is started.
        //Handles "game over" event, shape drops, 
        //level transitions, updating game display, and key controls.
        //side effects: Modifies current shape and level queue, 
        //              resets key flags to false for new key presses,
        //              and pauses game upon losing.
        private void _tGameTimer_Tick(object sender, EventArgs e)
        {
            if (Block.canvas == null)                                               //Game hasn't started yet.
                return;

            Block.canvas.Clear();                                                   //get rid of all drawn shapes  

            if (_qCurrentLevelShapes.Count == 0)                                    //no shapes left in the level queue
                if (!NextLevel())                                                   //if game isn't over, load next level.
                    return;

            InfoDraw();                                                             //Update the infobox 

            if (_bS || _swGameWatch.ElapsedMilliseconds >= _iDifficulty)            //check if it's time to drop a shape
            {
                if (_CurrentShape == null && _qCurrentLevelShapes.Count > 0)        //Shape has stopped falling and the level isn't over.
                {                                                                   
                    _CurrentShape = _qCurrentLevelShapes.Dequeue();                 //next shape to drop
                }
                if(!_CurrentShape.Drop(_lGameFloor))                                //drop the shape and check for game over
                {
                    Block.canvas.AddText("Game Over", 24, Color.White);             //obligatory game over message
                    Block.canvas.AddText("You have failed the", 24, 
                        Block.canvas.ScaledWidth / 2, 
                        Block.canvas.ScaledHeight / 2 + 2, 0, 0, Color.White);
                    Block.canvas.AddText("people of Tetronimo IV.", 24, 
                        Block.canvas.ScaledWidth / 2, 
                        Block.canvas.ScaledHeight / 2 + 3, 0, 0, Color.White);
                    Block.canvas.AddText("All hope is lost.", 24, 
                        Block.canvas.ScaledWidth / 2, 
                        Block.canvas.ScaledHeight / 2 + 6, 0, 0, Color.White);                   
                    Pause();                                                        //pause game indefinitely
                    return;                                                             
                }
                if (!_CurrentShape._bFalling)                                       //check for stationary shape
                    _CurrentShape = null;                                           //clear current shape
                _swGameWatch.Restart();                                             //clear game stopwatch for new drop
            }

            if (_CurrentShape != null)                                              //Check if the shape is still falling
            {
                if (_bA || Block.canvas.GetLastMouseLeftClick(out _pMouseCoords))   //shift left (l-click or Left key)
                    _CurrentShape.ShiftLeft(_lGameFloor);
                if (_bD || Block.canvas.GetLastMouseRightClick(out _pMouseCoords))  //shift right (r-click or Right key)
                    _CurrentShape.ShiftRight(_lGameFloor);             
                if (_bW)                                                            //rotate shape
                    _CurrentShape.RotateShape(_lGameFloor);
                _CurrentShape.ShowShape();                                          //draw shape in its new location
            }

            RowComplete(_lGameFloor);                                               //Check if any rows are full.
            Block.ShowFloor(_lGameFloor);                                           //redraw fallen blocks.
            _bW = false;                                                            //reset key press flags
            _bS = false;
            _bA = false;
            _bD = false;
        }
    }
}
