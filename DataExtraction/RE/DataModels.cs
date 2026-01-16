using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathExtension.Geometry;

namespace RE
{
    /// <summary>
    /// 功能区关键节点，包括功能区抽象点和门点
    /// </summary>
    public class SPKeyNodes
    {
        public Point FRPoint; 

        public List<Point> DoorPoints ;

        public SPKeyNodes()
        {
            FRPoint = new Point();
            DoorPoints = new List<Point>();
        }
    }

    public class Route
    {
        List<Point> nodes;

        public Point FRPoint { get; set; }

        public Point DoorPoint { get; set; }

        /// <summary>
        /// 路径节点集合，方向为从门点至功能区抽象点
        /// </summary>
        public List< Point> Nodes
        {
            get { return nodes; }
            set { nodes = value; }
        }


        public Route(Point functionRegionPoint, Point doorPoint)
        {
            FRPoint = functionRegionPoint;
            DoorPoint = doorPoint;

            nodes = new List<Point>();
            nodes.Add(DoorPoint);
            nodes.Add(FRPoint);
        }

        public void AddNode(Point point)
        {
            Nodes.Insert(Nodes.Count - 1, point);
        }

        
    }
}
