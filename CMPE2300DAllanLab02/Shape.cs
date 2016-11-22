/****************************************
*Program:   Lab 02 - Tetr-i-matic       *
*Author:    Dillon Allan                *
****************************************/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDIDrawer;

namespace CMPE2300DAllanLab02
{
    //Contains the different shapes used in-game.
    //Each shape is named after the letter it resembles.
    public enum ShapeType
    {
        I,
        J,
        L,
        O,
        S,
        T,
        Z
    }
    
    //Contains all shape-related properties and methods.
    class Shape
    {
        //Properties
        public ShapeType _ShapeType { get; private set; }   //describes what shape it is.
        public bool _bFalling { get; set; }                 //is the shape falling?
        public Point _pCoreLocation { get; private set; }   //Defines x-y location of the core block in the object.
                                                            //i.e. the shape rotates around _pCoreLocation.
        public List<Block> _lBlocks = new List<Block>();    //contains the blocks that make the shape.

        //Function name:    Shape
        //Description:      Instance constructor for Shape objects.
        //Returns:          none (CTOR)
        //Arguments:        Point shapeBaseLocation - draw the core block here.
        public Shape(Point shapeBaseLocation)
        {
            _bFalling = true;                                       //Object is falling by default
            Color c = RandColor.GetColor();                         //Object gets a random color
            this._pCoreLocation = shapeBaseLocation;                //core block is drawn at the base location
            this._ShapeType = (ShapeType)Block._rGen.Next(0, 7);    //Object gets a random shape type

            switch (_ShapeType)                                     //Add blocks to each shape type differently.
            {
                case ShapeType.I:
                    for (int i = 0; i < 3; i++)
                    {
                        _lBlocks.Add(new Block(this, i, 0, c));
                    }
                    _lBlocks.Add(new Block(this, -1, 0, c));
                    break;

                case ShapeType.J:
                    _lBlocks.Add(new Block(this, 0, 0, c));
                    _lBlocks.Add(new Block(this, -1, 0, c));
                    for (int i = 0; i < 2; i++)
                    {
                        _lBlocks.Add(new Block(this, 1, i, c));
                    }
                    break;

                case ShapeType.L:
                    _lBlocks.Add(new Block(this, 0, 0, c));
                    _lBlocks.Add(new Block(this, 1, 0, c));
                    for (int i = 0; i < 2; i++)
                    {
                        _lBlocks.Add(new Block(this, -1, i, c));
                    }
                    break;

                case ShapeType.O:
                    for (int i = 0; i < 2; i++)
                    {
                        _lBlocks.Add(new Block(this, 0, -i, c));
                    }
                    for (int i = 0; i < 2; i++)
                    {
                        _lBlocks.Add(new Block(this, -1, -i, c));
                    }
                    break;

                case ShapeType.S:
                    for (int i = 0; i < 2; i++)
                    {
                        _lBlocks.Add(new Block(this, 0, i, c));
                    }
                    _lBlocks.Add(new Block(this, 1, 0, c));
                    _lBlocks.Add(new Block(this, -1, 1, c));
                    break;
                case ShapeType.T:
                    for (int i = 0; i < 2; i++)
                    {
                        _lBlocks.Add(new Block(this, 0, i, c));
                    }
                    _lBlocks.Add(new Block(this, -1, 0, c));
                    _lBlocks.Add(new Block(this, 1, 0, c));
                    break;
                case ShapeType.Z:
                    for (int i = 0; i < 2; i++)
                    {
                        _lBlocks.Add(new Block(this, 0, i, c));
                    }
                    _lBlocks.Add(new Block(this, -1, 0, c));
                    _lBlocks.Add(new Block(this, 1, 1, c));
                    break;
            }
        }

        //Function name:    ShowShape
        //Description:      Iterates through a shape instance,
        //                  invoking ShowBlock on each block.
        //Return type:      void
        //Arguments:        none 
        public void ShowShape()
        {
            this._lBlocks.ForEach(b => b.ShowBlock()); //draw each block of the shape
        }

        //Function name:    Falling
        //Description:      Predicate used to test if the 
        //                  given shape is falling.
        //Return type:      bool - true for falling, false for not falling.
        //Arguments:        Shape s - test shape
        public static bool Falling(Shape s)     
        {
            return s._bFalling; //return true if shape is falling.
        }

        //Function name:    CanRotate
        //Description:      Tests if a shape will go out of bounds by rotating.
        //Return type:      bool - true for pass, false for can't rotate.
        //Arguments:        Shape s - test shape
        public bool CanRotate()
        {
            //local members for testing rotation
            int iXOffset;
            int iYOffset;
            int iXTest;     
            int iYTest;
            foreach (Block b in this._lBlocks)                          //iterate through all the shape's blocks
            {
                iXOffset = -b._iYOffset;                                //rotate offsets
                iYOffset = b._iXOffset;
                iXTest = this._pCoreLocation.X + iXOffset;              //calculate new location
                iYTest = this._pCoreLocation.Y + iYOffset;

                if (iXTest < 0 || iXTest >= Block.canvas.ScaledWidth || //test new location for out of bounds
                    iYTest < 0 || iYTest >= Block.canvas.ScaledHeight)
                    return false;
            }
            return true; 
        }

        //Function name:    RotateShape
        //Description:      Iterates through a shape object, invoking Rotate on each block.
        //                  Checks for shape overlaps before committing rotation.
        //Return type:      void
        //Arguments:        List<Block[]> lCurrentFloor - state of game floor
        public void RotateShape(List<Block[]> lCurrentFloor) //ccw
        {
            if (!this.CanRotate())                          //At least one of the shape's blocks can't rotate.
                return;

            
            List<Block> backupBlocks = new List<Block>();   //Before proceeding, make a backup of the shape's previous location.
            this._lBlocks.ForEach((Block b) =>              
            {
                backupBlocks.Add(new Block(b._parentShape,
                    b._iXOffset, b._iYOffset, b._cColor));
            });

            this._lBlocks.ForEach(b => b.Rotate());         //rotate each block of the shape.
            List<Block> overlaps = new List<Block>();       //make a new list for overlapped shapes
            lCurrentFloor.ForEach((Block[] row) =>          //go through each row in game floor
            {
                foreach (Block b in row)
                {
                    if (this._lBlocks.Contains(b))          //add any overlapping blocks to the list
                        overlaps.Add(b);
                }
            });

            if (overlaps.Count != 0)                        //rotation failed
            {
                this._lBlocks = backupBlocks;               //undo the rotation
                return;
            }
            return;                                         //no overlaps; okay to keep rotation.
        }

        //Function Name:    Drop
        //Description:      Decides next shape position 
        //                  based on it's current state and the shapes around it.
        //Return type:      bool - true for successful drop, false for can't drop i.e. game over.
        //Arguments:        List<Block[]> lCurrentFloor - state of game floor
        public bool Drop(List<Block[]> lCurrentFloor)
        {
            if (!this._bFalling) return true;           //is the shape not falling? Get outta there!
            if (!this._lBlocks.All(Block.CanFall))      //At least one of the shape's blocks can't fall.
            {
                this._bFalling = false;                 //change property to indicate shape is stationary
                this.AddBlocksToRows(lCurrentFloor);    //add stationary shape to the floor. 
                return true;                            
            }
            
            Point backupLocation = new Point(_pCoreLocation.X, _pCoreLocation.Y);   //Before proceeding, make a backup of the shape's previous location.

            this._pCoreLocation = new Point(this._pCoreLocation.X,                  //update the shape's location to drop it.
                this._pCoreLocation.Y + 1);
            List<Block> overlaps = new List<Block>();                               //make a new list for overlapped shapes
            lCurrentFloor.ForEach((Block[] row) =>                                  //iterate through all rows in the game floor.
            {
                foreach (Block b in row)                                            
                {
                    if (this._lBlocks.Contains(b))                                  //if an overlap is found,
                        overlaps.Add(b);                                            //add it to the overlaps list.
                }
            });

            if (overlaps.Count > 0)                     //drop failed
            {
                this._pCoreLocation = backupLocation;   //undo the drop
                overlaps = new List<Block>();           //reset overlaps list;
                                                        //have to check original spawn location for overlaps now.
                lCurrentFloor.ForEach((Block[] row) =>                  
                {
                    foreach (Block b in row)                            
                    {
                        if (this._lBlocks.Contains(b))                  
                            overlaps.Add(b);                            
                    }
                });
                if (overlaps.Count > 0)
                {
                    return false;                           //game over - game floor has collided with top frame of game window
                }

                this._bFalling = false;                     //change property to reflect a stationary shape.
                if (!this.AddBlocksToRows(lCurrentFloor))   //
                    return false;                           //add the stationary shape to the currentFloor list.
                return true;
            }
            return true;                                    
        }

        //Function Name:    ShiftLeft
        //Description:      Attempts to move shape to the left, testing
        //                  for out of bounds and shape overlaps.
        //Return type:      void
        //Arguments:        List<Block[]> lCurrentFloor - state of game floor
        public void ShiftLeft(List<Block[]> lCurrentFloor)
        {
            if (!this._bFalling) return;                                            //is the shape not falling? Get outta there!
            if (!this._lBlocks.All(Block.CanShiftLeft))                             //At least one of the shape's blocks can't shift left.
                return;

            Point backupLocation = new Point(_pCoreLocation.X, _pCoreLocation.Y);   //Before proceeding, make a backup of the shape's previous location.

            this._pCoreLocation = new Point(this._pCoreLocation.X - 1,              //update the shape's location to shift it.
                                        this._pCoreLocation.Y);
            List<Block> overlaps = new List<Block>();                               //make a new list for overlapped shapes
            lCurrentFloor.ForEach((Block[] row) =>                                  //check all rows in game floor
            {
                foreach (Block b in row)                                
                {
                    if (this._lBlocks.Contains(b))                                  //add overlaps to list
                        overlaps.Add(b);                                
                }                                                       
            });

            if (overlaps.Count != 0)                                                //l-shift failed
            {
                this._pCoreLocation = backupLocation;                               //undo the l-shift
                return;
            }
            return;                                                                 //successful l-shift
        }

        //Function Name:    ShiftRight
        //Description:      Attempts to move shape to the right, testing
        //                  for out of bounds and shape overlaps.
        //Return type:      void
        //Arguments:        List<Block[]> lCurrentFloor - state of game floor
        public void ShiftRight(List<Block[]> lCurrentFloor)
        {
            if (!this._bFalling) return;                                            //is the shape not falling? Get outta there!
            if (!this._lBlocks.All(Block.CanShiftRight))                            //At least one of the shape's blocks can't shift left.
                return;

            Point backupLocation = new Point(_pCoreLocation.X, _pCoreLocation.Y);   //Before proceeding, make a backup of the shape's previous location.

            this._pCoreLocation = new Point(this._pCoreLocation.X + 1,              //update the shape's location to shift it.
                                        this._pCoreLocation.Y);
            List<Block> overlaps = new List<Block>();                               //make a new list for overlapped shapes
                                                                         
            lCurrentFloor.ForEach((Block[] row) =>                                  //check rows in game floor
            {
                foreach (Block b in row)
                {
                    if (this._lBlocks.Contains(b))                                  //add overlaps to list
                        overlaps.Add(b);
                }
            });

            if (overlaps.Count != 0)                                                //r-shift failed
            {
                this._pCoreLocation = backupLocation;                               //undo the r-shift
                return;
            }
            return;                                                                 //successful r-shift
        }

        //Function Name:    ShiftRight
        //Description:      Add each block of the invoking shape to row(s) in the game floor.
        //                  Checks for second game over cnd: overflow over the top of the game frame.
        //Return type:      bool - true for successful add, false for overflow/game over.
        //Arguments:        List<Block[]> lCurrentFloor - state of game floor
        private bool AddBlocksToRows(List<Block[]> lCurrentFloor)
        {
            foreach (Block b in this._lBlocks)                      //add each block to an 2D indexed location in the game floor.
            {
                if (b._iYLocation < 0)                              //overflow - game over.
                    return false;
                lCurrentFloor[b._iYLocation][b._iXLocation] = b;    
                b._iXFloor = b._iXLocation;                         //set block's floor x- and y-positions.
                b._iYFloor = b._iYLocation;
            }
            return true;
        }
    }

    //Contains all Block-related properties and methods.
    class Block
    {
        static CDrawer _canvas = null;                      //game window
        public static CDrawer canvas    
        {
            get
            {
                return _canvas;
            }
            set
            {
                if (_canvas != null) _canvas.Close();       //new game - create a new window.
                _canvas = value;
            }
        }
        static public Random _rGen { get; private set; }    //used for random shapes in Shape ctor.
        public Shape _parentShape = null;                   //describes the shape the block belongs to.
        public int _iXOffset;                               //horizontal distance from core block.
        public int _iYOffset;                               //vertical distance from core block.
        public int _iXFloor;                                //horizontal position in game floor
        public int _iYFloor;                                //vertical position in game floor
        public int _iXLocation                              //absolute x-position on the canvas.
        {
            get
            {
                return _parentShape._pCoreLocation.X + _iXOffset;
            }
        }
        public int _iYLocation                              //absolute y-position on the canvas
        {
            get
            {
                return _parentShape._pCoreLocation.Y + _iYOffset;
            }
        }
        public Color _cColor;                               //block draw color

        //Function name:    Block
        //Description:      Static constructor for _rGen property.
        //Returns:          none (CTOR)
        //Arguments:        none
        static Block()
        {
            _rGen = new Random();
        }

        //Function name:    Block
        //Description:      Instance constructor for Blocks.
        //Returns:          none (CTOR)
        //Arguments:        Shape parentShape
        //                  int xOffset
        //                  int yOffset
        //                  Color color
        public Block(Shape parentShape, int xOffset, int yOffset, Color color)
        {
            _parentShape = parentShape;
            _iXOffset = xOffset;
            _iYOffset = yOffset;
            _cColor = color;
        }

        //Function name:    ShowBlock
        //Description:      Draw block onto the canvas.
        //Returns:          void
        //Arguments:        none
        public void ShowBlock()
        {
            _canvas.AddRectangle(_iXLocation, _iYLocation, 1, 1, _cColor, 1, Color.Gray);
        }

        //Function name:    Equals
        //Description:      Custom Equals for Block class.
        //                  IMPORTANT: This Equals is only valid for testing Block SHAPE location
        //                  to block FLOOR location in that order i.e. one way equivalence.
        //Returns:          bool - true for equivalence, false for inequivalence
        //Arguments:        object obj - test subject
        public override bool Equals(object obj)
        {
            if (!(obj is Block)) return false;
            Block arg = obj as Block;
            return this._iXLocation.Equals(arg._iXFloor) && this._iYLocation.Equals(arg._iYFloor);
        }

        public override int GetHashCode()
        {
            return 1;
        }

        //Function name:    Rotate
        //Description:      Transform block position to rotate.
        //Returns:          bool - true for equivalence, false for inequivalence.
        //Arguments:        object obj - test subject
        public void Rotate()
        {
            if (this._parentShape._ShapeType != ShapeType.O) //blocks have x- and y-symmetry so rotation is pointless.
            {
                int tmp = _iXOffset;
                _iXOffset = -_iYOffset;
                _iYOffset = tmp;
            }
        }

        //Function name:    CanShiftLeft
        //Description:      predicate for testing if a l-shift would be out of bounds.
        //Returns:          bool - true for within bounds, false for out of bounds.
        //Arguments:        Block b - test block
        public static bool CanShiftLeft(Block b)
        {
            return b._iXLocation - 1 > -1;
        }

        //Function name:    CanShiftRight
        //Description:      predicate for testing if a r-shift would be out of bounds.
        //Returns:          bool - true for within bounds, false for out of bounds.
        //Arguments:        Block b - test block
        public static bool CanShiftRight(Block b)
        {
            return b._iXLocation + 1 < _canvas.ScaledWidth;
        }

        //Function name:    CanFall
        //Description:      predicate for testing if a drop would be out of bounds.
        //Returns:          bool - true for within bounds, false for out of bounds.
        //Arguments:        Block b - test block
        public static bool CanFall(Block b)
        {
            return b._iYLocation + 1 < _canvas.ScaledHeight;
        }

        //Function name:    NewFloor
        //Description:      makes a new floor, populated with empty rows of fixed size.
        //Returns:          List<Block[]> - new game floor
        //Arguments:        none
        public static List<Block[]> NewFloor()
        {
            List<Block[]> newFloor = new List<Block[]>();
            for (int rows = 0; rows < 20; rows++)           //add 20 empty rows to the list. 
                newFloor.Add(new Block[10]);                //rows will be filled by blocks in-game.
            return newFloor;
        }

        //Function name:    ShowFloor
        //Description:      draws each block in the game floor.
        //Returns:          void
        //Arguments:        List<Block[]> lCurrentFloor - state of game floor
        public static void ShowFloor(List<Block[]> lCurrentFloor)
        {
            lCurrentFloor.ForEach((Block[] bRow) =>
            {
                foreach (Block b in bRow)
                {
                    if (b != null)
                        canvas.AddRectangle(b._iXFloor, b._iYFloor, 
                            1, 1, b._cColor, 1, Color.Gray);
                }
            });
        }

        //Function name:    DropRow
        //Description:      moves all blocks in rows at and above the completed row
        //                  down one position in the game floor.
        //Returns:          void
        //Arguments:        List<Block[]>   lCurrentFloor - state of game floor
        //                  int             rowNumber - row that was just cleared
        public static void DropRow(List<Block[]> lCurrentFloor, int rowNumber)
        {
            lCurrentFloor.ForEach((Block[] row) =>
            {
                if (lCurrentFloor.IndexOf(row) <= rowNumber) //only drop rows at or above the cleared row.
                    foreach (Block b in row)
                        if (b != null)
                            b._iYFloor++;
            });
        }
    }
}