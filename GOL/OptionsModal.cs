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
    public partial class OptionsModal : Form
    {
        public OptionsModal()
        {
            InitializeComponent();
        }

        //property for time interval value in numericUpDown
        public int Interval 
        {
            get
            {
                return (int)intervalNumericUpDown.Value;
            }
            set
            {
                intervalNumericUpDown.Value = value;
            }
        }

        //poperty for width value in numericUpDown
        public int Width
        {
            get
            {
                return (int)widthNumericUpDown.Value;
            }
            set
            {
                widthNumericUpDown.Value = value;
            }
        }

        //poperty for height value in numericUpDown
        public int Height
        {
            get
            {
                return (int)heightNumericUpDown.Value;
            }
            set
            {
                heightNumericUpDown.Value = value;
            }
        }
    }
}
