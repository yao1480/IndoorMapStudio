using MathExtension.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DXFReader.DataModel
{
    public class BlockClass
    {
        EntityClass entity;
        Point basePoint;

        public string Name { get; set; }
        public string LayerName { get; set; }
        public Point BasePoint
        {
            get
            {
                return basePoint;
            }
            set
            {
                basePoint = value;
            }
        }
        public  EntityClass Entity
        {
            get
            {
                return entity;
            }
            set
            {
                entity = value;
            }
        }

        public BlockClass()
        {
            entity = new EntityClass();
            basePoint = new Point();
        }
    }
}
