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
    //Utilities for main form
    public partial class Form1 : Form
    {
        //Function Name:    Pause
        //Description:      pauses the timer and stopwatch, 
        //                  effectively ceasing game progress.
        //Returns:          void
        //Arguments:        none
        private void Pause()
        {
            _swGameWatch.Stop();
            _tGameTimer.Stop();
        }

        //Function Name:    Resume
        //Description:      starts the timer and stopwatch, 
        //                  allowing game progress to continue.
        //Returns:          void
        //Arguments:        none
        private void Resume()
        {
            _tGameTimer.Start();
            _swGameWatch.Start();
        }

        //Function name:    NextLevel
        //Description:      Handles level transitions and game victory event.
        //                  Increases difficulty, sets new queue for shapes.
        //Return type:      bool - true for game not over, false for game over.
        //Arguments:        none
        private bool NextLevel()
        {
            switch (_iDifficulty)                           //set game difficulty
            {
                case 0:
                    _iDifficulty = _iEasy;
                    break;
                case _iEasy:
                    _iDifficulty = _iMedium;
                    break;
                case _iMedium:
                    _iDifficulty = _iHard;
                    break;
                case _iHard:                                //end game stage
                    if (_CurrentShape == null && 
                        _qCurrentLevelShapes.Count == 0)                        //check if queue is empty 
                    {                                                           //game is won when final shape is laid successfully.
                        Pause();                                                //game won - pause game indefinitely.
                        Block.canvas.AddText("You won!", 23, Color.White);      //obligatory game victory message.
                        Block.canvas.AddText("The people of Tetronimo IV", 24,
                        Block.canvas.ScaledWidth / 2,
                        Block.canvas.ScaledHeight / 2 + 2, 0, 0, Color.White);
                        Block.canvas.AddText("are forever in your debt.", 24,
                        Block.canvas.ScaledWidth / 2,
                        Block.canvas.ScaledHeight / 2 + 3, 0, 0, Color.White);
                        Block.canvas.AddText("Hope is found once again.", 24,
                        Block.canvas.ScaledWidth / 2,
                        Block.canvas.ScaledHeight / 2 + 6, 0, 0, Color.White);
                        return false;                                           
                    }
                    return true;
            }
            _qCurrentLevelShapes = new Queue<Shape>();                          //new Queue for new level
            for (int i = 0; i < 20; i++)                                        //fill level queue with 20 random shapes
                _qCurrentLevelShapes.Enqueue(new Shape(new Point(Block.canvas.ScaledWidth / 2, 0)));
            return true;
        }

        //Function name:    RowComplete
        //Description:      Checks for row completion. 
        //                  Repopulates list with empty rows when necessary.
        //Return type:      void
        //Arguments:        List<Block[]> lCurrentFloor - state of the game floor
        private void RowComplete(List<Block[]> lCurrentFloor)
        {
            for (int row = 0; row < Block.canvas.ScaledHeight; row++)           //iterate through all the rows in the floor.
            {
                if (!lCurrentFloor[row].Any(b => b == null) &&                  //check for a full (non-null) row 
                    (lCurrentFloor[row].Count() == lCurrentFloor[row].Length))    
                {
                    lCurrentFloor.Remove(lCurrentFloor[row]);                   //get rid of it.
                    lCurrentFloor.Insert(0, new Block[10]);                     //add a new row at the beginning 
                                                                                //of the list to "push" floor downward.
                                                            
                    Block.DropRow(lCurrentFloor, row);                          //drop all blocks above the cleared row
                    _iScore++;                                                  //keep track of the score
                }
            }
        }

        //Function name:    InfoDraw
        //Description:      Displays current difficulty, current score,
        //                  and next shape to drop (if there is one.)
        //Return type:      void
        //Arguments:        none
        private void InfoDraw()
        {
            string difficulty = null;       //local member for indicating game difficulty
            switch (_iDifficulty)           //set the local accordingly.
            {
                case _iEasy:
                    difficulty = "Easy";
                    break;
                case _iMedium:
                    difficulty = "Medium";
                    break;
                case _iHard:
                    difficulty = "Hard";
                    break;
            }
            _infoBox.Clear();                                       //clear for new information.
            _infoBox.AddText("Difficulty: " + difficulty, 14,       //display difficulty,
                _infoBox.ScaledWidth / 2, 1, 0, 0, Color.Black);    
            _infoBox.AddText("Score: " + _iScore, 14,               //score,
                _infoBox.ScaledWidth / 2, 2, 0, 0, Color.Black);
            _infoBox.AddText("Next Shape:", 14,                     //and the next shape text.
                _infoBox.ScaledWidth / 2, 3, 0, 0, Color.Black);

            if (_qCurrentLevelShapes.Count() > 0)                           //check if there are any remaining shapes this level.
                _qCurrentLevelShapes.Peek()._lBlocks.ForEach((Block b) =>   
                {
                    switch (b._parentShape._ShapeType)                      //Draw upcoming shape in the info box (centered).
                    {
                        case ShapeType.I:
                            _infoBox.AddRectangle(_infoBox.ScaledWidth / 2 + b._iXOffset - 1,
                                _infoBox.ScaledHeight - 2 + b._iYOffset, 1, 1, b._cColor, 1, Color.Gray);
                            break;
                        case ShapeType.J:
                            _infoBox.AddRectangle(_infoBox.ScaledWidth / 2 + b._iXOffset,
                                _infoBox.ScaledHeight - 2 + b._iYOffset, 1, 1, b._cColor, 1, Color.Gray);
                            break;
                        case ShapeType.L:
                            _infoBox.AddRectangle(_infoBox.ScaledWidth / 2 + b._iXOffset,
                                _infoBox.ScaledHeight - 2 + b._iYOffset, 1, 1, b._cColor, 1, Color.Gray);
                            break;
                        case ShapeType.O:
                            _infoBox.AddRectangle(_infoBox.ScaledWidth / 2 + b._iXOffset,
                                _infoBox.ScaledHeight - 2 + b._iYOffset + 1, 1, 1, b._cColor, 1, Color.Gray);
                            break;
                        case ShapeType.S:
                            _infoBox.AddRectangle(_infoBox.ScaledWidth / 2 + b._iXOffset,
                                _infoBox.ScaledHeight - 2 + b._iYOffset, 1, 1, b._cColor, 1, Color.Gray);
                            break;
                        case ShapeType.T:
                            _infoBox.AddRectangle(_infoBox.ScaledWidth / 2 + b._iXOffset,
                                _infoBox.ScaledHeight - 2 + b._iYOffset, 1, 1, b._cColor, 1, Color.Gray);
                            break;
                        case ShapeType.Z:
                            _infoBox.AddRectangle(_infoBox.ScaledWidth / 2 + b._iXOffset,
                                _infoBox.ScaledHeight - 2 + b._iYOffset, 1, 1, b._cColor, 1, Color.Gray);
                            break;
                    }
                });
        }
    }
}
